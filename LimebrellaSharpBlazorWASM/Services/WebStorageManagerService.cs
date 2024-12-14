// v2024-07-22 19:34:11

using LimebrellaSharpBlazorWASM.Helpers;
using Microsoft.JSInterop;
using System.Text.Json;

namespace LimebrellaSharpBlazorWASM.Services;

/// <summary>
/// A service that manages Local and Session Storages.
/// </summary>
public class WebStorageManagerService(uint murMurSeed, IJSRuntime jsRuntime)
{
    /// <summary>
    /// Enumeration of WebStorage type.
    /// </summary>
    private enum StorageType
    {
        Local,
        Session
    }

    private string? _encKeyL;
    private string? _encKeyS;

    /// <summary>
    /// Semaphore used during Initialization. 
    /// </summary>
    private static readonly SemaphoreSlim InitializationSemaphore = new(1, 1);
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Encrypts <paramref name="value"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="storageType"></param>
    private void Encrypt(ref string value, StorageType storageType)
    {
        var encKey = storageType == StorageType.Local ? _encKeyL : _encKeyS;
        if (!string.IsNullOrEmpty(encKey)) value = value.Encrypto(encKey, murMurSeed);
    }

    /// <summary>
    /// Decrypts <paramref name="value"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="storageType"></param>
    private void Decrypt(ref string value, StorageType storageType)
    {
        var encKey = storageType == StorageType.Local ? _encKeyL : _encKeyS;
        if (!string.IsNullOrEmpty(encKey)) value = value.Decrypto(encKey);
    }

    /// <summary>
    /// Gets the WebStorage <paramref name="type"/> as string.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static string GetStorageTypeAsString(StorageType type)
        => type.ToString().ToLower();

    /// <summary>
    /// Initializes service.
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        await InitializationSemaphore.WaitAsync();

        try
        {
            if (IsInitialized) return;
            await SetupSecureStoragesAsync();
            IsInitialized = true;
        }
        finally { InitializationSemaphore.Release(); }
    }
    
    /// <summary>
    /// Setups secure storages.
    /// </summary>
    /// <returns></returns>
    private async Task SetupSecureStoragesAsync()
    {
        const string storageGuid = "storageGuid";
        const string storageKey = "storageKey";

        await SetupStorages(StorageType.Local);
        await SetupStorages(StorageType.Session);

        return;

        async Task SetupStorages(StorageType storageType)
        {
            await SetStorageKeys(storageType, storageGuid);
            await SetStorageKeys(storageType, storageKey, true);
        }

        async Task SetStorageKeys(StorageType storageType, string key, bool secured = false)
        {
            var currentStorageGuid = await GetStorageItemAsync(storageType, key, "", secured);
            if (string.IsNullOrEmpty(currentStorageGuid))
            {
                currentStorageGuid = Guid.NewGuid().ToString();
                await SetStorageItemAsync(storageType, key, currentStorageGuid, secured);
            }

            if (storageType == StorageType.Local)
                _encKeyL = currentStorageGuid;
            else
                _encKeyS = currentStorageGuid;
        }
    }

    /// <summary>
    /// Sets an item in the localStorage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetLocalStorageItemAsync<T>(string key, T value)
        => await SetStorageItemAsync(StorageType.Local, key, value);

    /// <summary>
    /// Sets an item in the sessionStorage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetSessionStorageItemAsync<T>(string key, T value)
        => await SetStorageItemAsync(StorageType.Session, key, value);

    /// <summary>
    /// Sets a secured item in the localStorage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetLocalStorageSecuredItemAsync<T>(string key, T value)
        => await SetStorageItemAsync(StorageType.Local, key, value, true);

    /// <summary>
    /// Sets a secured item in the sessionStorage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetSessionStorageSecuredItemAsync<T>(string key, T value)
        => await SetStorageItemAsync(StorageType.Session, key, value, true);

    /// <summary>
    /// Sets an item in the storage of <paramref name="storageType"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="storageType"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="secure"></param>
    /// <returns></returns>
    private async Task SetStorageItemAsync<T>(StorageType storageType, string key, T value, bool secure = false)
    {
        var stringValue = value as string ?? JsonSerializer.Serialize(value);
        if (secure)
        {
            Encrypt(ref key, storageType);
            Encrypt(ref stringValue, storageType);
        }

        await jsRuntime.InvokeVoidAsync($"{GetStorageTypeAsString(storageType)}Storage.setItem", key, stringValue);
    }

    /// <summary>
    /// Gets an item from the localStorage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public async Task<T?> GetLocalStorageItemAsync<T>(string key, T defaultValue)
        => await GetStorageItemAsync(StorageType.Local, key, defaultValue);

    /// <summary>
    /// Gets an item from the sessionStorage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public async Task<T?> GetSessionStorageItemAsync<T>(string key, T defaultValue)
        => await GetStorageItemAsync(StorageType.Session, key, defaultValue);

    /// <summary>
    /// Gets a secured item from the localStorage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public async Task<T?> GetLocalStorageSecuredItemAsync<T>(string key, T defaultValue)
        => await GetStorageItemAsync(StorageType.Local, key, defaultValue, true);

    /// <summary>
    /// Gets a secured item from the sessionStorage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public async Task<T?> GetSessionStorageSecuredItemAsync<T>(string key, T defaultValue)
        => await GetStorageItemAsync(StorageType.Session, key, defaultValue, true);

    /// <summary>
    /// Gets an item from the storage of <paramref name="storageType"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="storageType"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <param name="secured"></param>
    /// <returns></returns>
    private async Task<T?> GetStorageItemAsync<T>(StorageType storageType, string key, T defaultValue,
        bool secured = false)
    {
        if (secured) Encrypt(ref key, storageType);
        var value = await jsRuntime.InvokeAsync<string>($"{GetStorageTypeAsString(storageType)}Storage.getItem", key);
        // return a default value if null
        if (string.IsNullOrEmpty(value)) return defaultValue;
        if (secured) Decrypt(ref value, storageType);
        return typeof(T) == typeof(string) ? (T)(object)value : JsonSerializer.Deserialize<T>(value);
    }

    /// <summary>
    /// Removes an item from the localStorage.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task RemoveLocalStorageItemAsync(string key)
        => await RemoveStorageItemAsync(StorageType.Local, key);

    /// <summary>
    /// Removes an item from the sessionStorage.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task RemoveSessionStorageItemAsync(string key)
        => await RemoveStorageItemAsync(StorageType.Session, key);

    /// <summary>
    /// Removes a secured item from the localStorage.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task RemoveLocalStorageSecuredItemAsync(string key)
        => await RemoveStorageItemAsync(StorageType.Local, key, true);

    /// <summary>
    /// Removes a secured item from the sessionStorage.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task RemoveSessionStorageSecuredItemAsync(string key)
        => await RemoveStorageItemAsync(StorageType.Session, key, true);

    /// <summary>
    /// Removes an item from the storage of <paramref name="storageType"/>.
    /// </summary>
    /// <param name="storageType"></param>
    /// <param name="key"></param>
    /// <param name="secured"></param>
    /// <returns></returns>
    private async Task RemoveStorageItemAsync(StorageType storageType, string key, bool secured = false)
    {
        if (secured) Encrypt(ref key, storageType);
        await jsRuntime.InvokeVoidAsync($"{GetStorageTypeAsString(storageType)}Storage.removeItem", key);
    }

    /// <summary>
    /// Clears localStorage and sessionStorage.
    /// </summary>
    /// <param name="reloadPageOnceDone"></param>
    /// <returns></returns>
    public async Task ClearWebStorageAsync(bool reloadPageOnceDone = false)
    {
        await ClearStorageAsync(StorageType.Local);
        await ClearStorageAsync(StorageType.Session, reloadPageOnceDone);
    }

    /// <summary>
    /// Clears the localStorage.
    /// </summary>
    /// <param name="reloadPageOnceDone"></param>
    /// <returns></returns>
    public async Task ClearLocalStorageAsync(bool reloadPageOnceDone = false)
        => await ClearStorageAsync(StorageType.Local, reloadPageOnceDone);

    /// <summary>
    /// Clears the sessionStorage.
    /// </summary>
    /// <param name="reloadPageOnceDone"></param>
    /// <returns></returns>
    public async Task ClearSessionStorageAsync(bool reloadPageOnceDone = false)
        => await ClearStorageAsync(StorageType.Session, reloadPageOnceDone);

    /// <summary>
    /// Clears the storage of <paramref name="storageType"/>.
    /// </summary>
    /// <param name="storageType"></param>
    /// <param name="reloadPageOnceDone"></param>
    /// <returns></returns>
    private async Task ClearStorageAsync(StorageType storageType, bool reloadPageOnceDone = false)
    {
        await jsRuntime.InvokeVoidAsync($"{GetStorageTypeAsString(storageType)}Storage.clear");
        if (reloadPageOnceDone) await ReloadPageAsync();
    }

    /// <summary>
    /// Reloads the WebApp.
    /// </summary>
    /// <returns></returns>
    public async Task ReloadPageAsync()
        => await jsRuntime.InvokeVoidAsync("location.reload");
}