using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace LimebrellaSharpBlazorWASM.Services;

/// <summary>
/// A service that manages the application's languages.
/// </summary>
/// <param name="jsRuntime"></param>
public class LanguageManagerService(IJSRuntime jsRuntime, NavigationManager navigation)
{
    /// <summary>
    /// Gets Language Selector options.
    /// </summary>
    /// <returns></returns>
    public async Task<List<KeyValuePair<string, string>>?> GetLanguageSelectorOptionsAsync()
    {
        // Available languages are loaded from the JavaScript file
        var keys = await jsRuntime.InvokeAsync<string[]>("getAppLanguageKeys");
        var values = await jsRuntime.InvokeAsync<string[]>("getAppLanguageValues");
        return keys.Select((t, i) => new KeyValuePair<string, string>(t, values[i])).ToList();
    }

    /// <summary>
    /// Language type.
    /// </summary>
    private enum Type
    {
        BrowsersDefault,
        Current,
        Stored
    }
    
    /// <summary>
    /// Gets the application's language.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private async Task<string> GetAppLanguageAsync(Type type)
    {
        var func = type switch
        {
            Type.BrowsersDefault => "getBrowsersLanguage",
            Type.Current => "getCurrentLanguage",
            Type.Stored => "getStoredLanguage",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        return await jsRuntime.InvokeAsync<string>(func);
    }

    /// <summary>
    /// Gets a language that the browser recommends.
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetBrowsersLanguageAsync()
        => await GetAppLanguageAsync(Type.BrowsersDefault);

    /// <summary>
    /// Gets a language that the application is currently using.
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetCurrentAppLanguageAsync()
        => await GetAppLanguageAsync(Type.Current);

    /// <summary>
    /// Gets a language from the web storage.
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetStoredAppLanguageAsync()
        => await GetAppLanguageAsync(Type.Stored);

    /// <summary>
    /// Sets the application's language.
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public async Task SetAppLanguageAsync(string lang)
    {
        await jsRuntime.InvokeVoidAsync("setAppLanguage", lang);
        navigation.NavigateTo(navigation.Uri, forceLoad: true);
    }
}