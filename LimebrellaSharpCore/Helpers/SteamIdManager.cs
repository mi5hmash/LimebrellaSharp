// v2024-12-08 21:16:48

using System.Text.RegularExpressions;

namespace LimebrellaSharpCore.Helpers;

public class SteamIdManager(string pathPattern = "")
{
    private uint _steamIdInput;
    private uint _steamIdOutput;

    /// <summary>
    /// Sets <paramref name="steamIdInput"/> and <paramref name="steamIdOutput"/>.
    /// </summary>
    /// <param name="steamIdInput"></param>
    /// <param name="steamIdOutput"></param>
    public void Set(uint steamIdInput, uint steamIdOutput)
    {
        SetInput(steamIdInput);
        SetOutput(steamIdOutput);
    }

    /// <summary>
    /// Sets <paramref name="steamIdInput"/> and <paramref name="steamIdOutput"/>.
    /// </summary>
    /// <param name="steamIdInput"></param>
    /// <param name="steamIdOutput"></param>
    public void Set(string steamIdInput, string steamIdOutput)
    {
        SetInput(steamIdInput);
        SetOutput(steamIdOutput);
    }

    /// <summary>
    /// Gets Input SteamID.
    /// </summary>
    /// <returns></returns>
    public string GetInput()
        => _steamIdInput.ToString();
    public uint GetInputNumeric()
        => _steamIdInput;

    /// <summary>
    /// Gets Output SteamID.
    /// </summary>
    /// <returns></returns>
    public string GetOutput()
        => _steamIdOutput.ToString();
    public uint GetOutputNumeric()
        => _steamIdOutput;

    /// <summary>
    /// Sets Input SteamID.
    /// </summary>
    /// <param name="steamId"></param>
    /// <returns></returns>
    public bool SetInput(string steamId)
    {
        var result = uint.TryParse(steamId, out var parsedValue);
        if (result) _steamIdInput = parsedValue;
        return result;
    }

    /// <summary>
    /// Sets Input SteamID.
    /// </summary>
    /// <param name="steamId"></param>
    public void SetInput(uint steamId)
        => _steamIdInput = steamId;

    /// <summary>
    /// Sets Output SteamID.
    /// </summary>
    /// <param name="steamId"></param>
    public void SetOutput(uint steamId)
        => _steamIdOutput = steamId;

    /// <summary>
    /// Sets Output SteamID. 
    /// </summary>
    /// <param name="steamId"></param>
    /// <returns></returns>
    public bool SetOutput(string steamId)
    {
        var result = uint.TryParse(steamId, out var parsedValue);
        if (result) _steamIdOutput = parsedValue;
        return result;
    }

    /// <summary>
    /// Swaps <see cref="_steamIdInput"/> and <see cref="_steamIdOutput"/>.
    /// </summary>
    public void SteamIdInterchange()
        => (_steamIdInput, _steamIdOutput) = (_steamIdOutput, _steamIdInput);

    /// <summary>
    /// Extracts SteamID from a directory path.
    /// </summary>
    /// <param name="directoryPath"></param>
    public void ExtractSteamIdFromPathIfValid(string directoryPath)
    {
        var match = Regex.Match(directoryPath, pathPattern);
        if (match.Success) _steamIdInput = uint.TryParse(match.Groups[1].Value, out var parsedValue) ? parsedValue : default;
    }
}