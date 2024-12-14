using LimebrellaSharpBlazorWASM.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

namespace LimebrellaSharpBlazorWASM.Extensions
{
    public static class WebAssemblyHostExtension
	{
		public static async Task SetDefaultCulture(this WebAssemblyHost host)
		{
			var languageManager = host.Services.GetRequiredService<LanguageManagerService>();
            var result = await languageManager.GetStoredAppLanguageAsync();

			var culture = string.IsNullOrEmpty(result) ? new CultureInfo("en") : new CultureInfo(result);

			CultureInfo.DefaultThreadCurrentCulture = culture;
			CultureInfo.DefaultThreadCurrentUICulture = culture;
		}
	}
}
