using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace LimebrellaSharpBlazorWASM.Helpers;

public class CustomErrorBoundary : ErrorBoundary
{
    [Inject]
    private IWebAssemblyHostEnvironment? Environment { get; set; }

    protected override Task OnErrorAsync(Exception exception)
        => Environment!.IsDevelopment() ? base.OnErrorAsync(exception) : Task.CompletedTask;
}