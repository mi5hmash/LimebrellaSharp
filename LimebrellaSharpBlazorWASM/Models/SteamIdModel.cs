using System.ComponentModel.DataAnnotations;

namespace LimebrellaSharpBlazorWASM.Models;

public class SteamIdModel
{
    [Range(0, uint.MaxValue, ErrorMessage = "Please enter a number between 0 and uint.MaxValue.")]
    public uint SteamIdInput { get; set; }

    [Range(0, uint.MaxValue, ErrorMessage = "Please enter a number between 0 and uint.MaxValue.")]
    public uint SteamIdOutput { get; set; }

    /// <summary>
    /// Swaps <see cref="SteamIdInput"/> and <see cref="SteamIdOutput"/>.
    /// </summary>
    public void SteamIdInterchange()
        => (SteamIdInput, SteamIdOutput) = (SteamIdOutput, SteamIdInput);
}

