namespace LimebrellaSharpBlazorWASM.Services;

/// <summary>
/// A service that manages the SuperUser status.
/// </summary>
public class SuperUserService(WebStorageManagerService webStorage)
{
    /// <summary>
    /// Gets SuperUser status from Secured Local Storage.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> GetSuperUserStatus()
        => await webStorage.GetLocalStorageSecuredItemAsync("isSuperUser", false);

    /// <summary>
    /// Activates SuperUser.
    /// </summary>
    public async Task ActivateSuperUser()
        => await webStorage.SetLocalStorageSecuredItemAsync("isSuperUser", true);
}