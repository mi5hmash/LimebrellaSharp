using Microsoft.JSInterop;

namespace LimebrellaSharpBlazorWASM.Services;

/// <summary>
/// A service that manages the application's theme.
/// </summary>
/// <param name="jsRuntime"></param>
public class ThemeManagerService(IJSRuntime jsRuntime)
{
    /// <summary>
    /// Gets an array of available themes, with the first element representing the DARK theme and the second element representing the LIGHT theme
    /// </summary>
    /// <returns></returns>
    private async Task<string[]> GetAvailableAppThemes()
        => await jsRuntime.InvokeAsync<string[]>("getAvailableAppThemes");

    /// <summary>
    /// Gets Theme Selector options.
    /// </summary>
    /// <param name="dark">A local theme's name for "Dark"</param>
    /// <param name="light">A local theme's name for "Light"</param>
    /// <returns></returns>
    public async Task<List<KeyValuePair<string, string>>?> GetThemeSelectorOptions(string dark = "Dark", string light = "Light")
    {
        var themes = await GetAvailableAppThemes();
        return
        [
            new KeyValuePair<string, string> (themes[0], dark),
            new KeyValuePair<string, string> (themes[1], light)
        ];
    }
    
    /// <summary>
    /// Theme type.
    /// </summary>
    private enum Type
    {
        BrowsersDefault,
        Current,
        Stored
    }

    /// <summary>
    /// Gets the application's theme.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private async Task<string> GetAppThemeAsync(Type type)
    {
        var func = type switch
        {
            Type.BrowsersDefault => "getBrowsersAppTheme",
            Type.Current => "getCurrentAppTheme",
            Type.Stored => "getStoredAppTheme",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        return await jsRuntime.InvokeAsync<string>(func);
    }

    /// <summary>
    /// Gets a theme that the browser recommends.
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetBrowsersAppThemeAsync()
        => await GetAppThemeAsync(Type.BrowsersDefault);
    
    /// <summary>
    /// Gets a theme that the application is currently using.
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetCurrentAppThemeAsync()
        => await GetAppThemeAsync(Type.Current);
    
    /// <summary>
    /// Gets a theme from the web storage.
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetStoredAppThemeAsync()
        => await GetAppThemeAsync(Type.Stored);

    /// <summary>
    /// Sets the application's theme.
    /// </summary>
    /// <param name="theme"></param>
    /// <returns></returns>
    public async Task SetAppThemeAsync(string theme)
        => await jsRuntime.InvokeVoidAsync("setAppTheme", theme);

    /// <summary>
    /// Toggles the application's theme.
    /// </summary>
    /// <returns></returns>
    public async Task ToggleAppThemeAsync()
        => await jsRuntime.InvokeVoidAsync("toggleAppTheme");

    /// <summary>
    /// Changes App Theme to Dark.
    /// </summary>
    /// <returns></returns>
    public async Task SetAppThemeDarkAsync()
        => await jsRuntime.InvokeVoidAsync("setAppThemeDark");

    /// <summary>
    /// Changes App Theme to Light.
    /// </summary>
    /// <returns></returns>
    public async Task SetAppThemeLightAsync()
        => await jsRuntime.InvokeVoidAsync("setAppThemeLight");
}