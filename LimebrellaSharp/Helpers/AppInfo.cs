using System.Reflection;

namespace LimebrellaSharp.Helpers;

/// <summary>
/// Container for Global Read-Only Variables.
/// </summary>
public static class AppInfo
{
    public static string RootPath => AppDomain.CurrentDomain.BaseDirectory;

    #region APP INFO

    public static string Title => "Limebrella Sharp";

    public static string Version => GetAssemblyVersion();

    public static string Author => GetCompany();

    public static string ProductTitle => GetProductTitle();

    public static string Description => GetDescription();

    public static string Copyright => GetCopyright();

    #endregion

    #region OTHER INFO

    public static string OutputFolder => "_OUTPUT";

    public static string OutputPath => Path.Combine(RootPath, OutputFolder);

    #endregion

    #region METHODS

    /// <summary>
    /// Gets the attribute value of the assembly.
    /// </summary>
    /// <typeparam name="TAttr"></typeparam>
    /// <param name="resolveFunc"></param>
    /// <param name="defaultResult"></param>
    /// <returns></returns>
    private static string GetAttributeValue<TAttr>(Func<TAttr, string> resolveFunc, string defaultResult = "") where TAttr : Attribute
    {
        // Source: https://www.codeproject.com/Tips/353819/Get-all-Assembly-Information
        var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(TAttr), false);
        return attributes.Length > 0 ? resolveFunc((TAttr)attributes[0]) : defaultResult;
    }

    /// <summary>
    /// Gets the application version.
    /// </summary>
    /// <returns></returns>
    private static string GetAssemblyVersion() => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";

    /// <summary>
    /// Gets the product title.
    /// </summary>
    /// <returns></returns>
    private static string GetProductTitle() => GetAttributeValue<AssemblyTitleAttribute>(a => a.Title, Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName));

    /// <summary>
    /// Gets the description about the application.
    /// </summary>
    /// <returns></returns>
    private static string GetDescription() => GetAttributeValue<AssemblyDescriptionAttribute>(a => a.Description);

    /// <summary>
    /// Gets the copyright information for the product.
    /// </summary>
    /// <returns></returns>
    private static string GetCopyright() => GetAttributeValue<AssemblyCopyrightAttribute>(a => a.Copyright);

    /// <summary>
    /// Gets the company information for the product.
    /// </summary>
    /// <returns></returns>
    private static string GetCompany() => GetAttributeValue<AssemblyCompanyAttribute>(a => a.Company);

    #endregion
}