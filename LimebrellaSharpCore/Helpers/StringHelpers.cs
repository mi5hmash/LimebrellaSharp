// v2024-08-03 21:16:48

namespace LimebrellaSharpCore.Helpers;

public static class StringHelpers
{
    /// <summary>
    /// Removes all digits from the end of a filename.
    /// </summary>
    /// <param name="inputString"></param>
    /// <returns></returns>
    public static string RemoveSuffixNumbers(this string inputString)
    {
        while (int.TryParse(inputString[^1..], out _)) inputString = inputString[..^1];
        return inputString;
    }

    /// <summary>
    /// Gets specific number of characters <paramref name="characterCount"/> from the left side of <paramref name="inputString"/>
    /// </summary>
    /// <param name="inputString"></param>
    /// <param name="characterCount"></param>
    /// <returns></returns>
    public static string Left(this string inputString, int characterCount)
        => inputString[..characterCount];

    /// <summary>
    /// Gets specific number of characters <paramref name="characterCount"/> from the right side of <paramref name="inputString"/>
    /// </summary>
    /// <param name="inputString"></param>
    /// <param name="characterCount"></param>
    /// <returns></returns>
    public static string Right(this string inputString, int characterCount)
        => inputString.Substring(inputString.Length - characterCount, characterCount);
}