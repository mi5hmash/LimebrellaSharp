using System.Text;

namespace LimebrellaSharpCore.Helpers;

public static class Base64Dencryptor
{
    /// <summary>
    /// Converts a byte array into a Base64 string. 
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string Base64Encode(this byte[] bytes) => Convert.ToBase64String(bytes);

    /// <summary>
    /// Convert a Base64 string into an array of bytes. 
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] Base64Decode(this string str) => Convert.FromBase64String(str);

    /// <summary>
    /// Encodes an ASCII string into a Base64 string. 
    /// </summary>
    /// <param name="asciiStr"></param>
    /// <returns></returns>
    public static string Base64EncodeAscii(this string asciiStr) => Base64Encode(asciiStr.FromAsciiString());
    /// <summary>
    /// Decodes a Base64 string into an ASCII string. 
    /// </summary>
    /// <param name="asciiStr"></param>
    /// <returns></returns>
    public static string Base64DecodeAscii(this string asciiStr) => Base64Decode(asciiStr).ToAsciiString();

    /// <summary>
    /// Encodes a Unicode string into a Base64 string.
    /// </summary>
    /// <param name="unicodeStr"></param>
    /// <returns></returns>
    public static string Base64EncodeUnicode(this string unicodeStr) => Base64Encode(unicodeStr.FromUnicodeString());
    /// <summary>
    /// Decodes a Base64 string into a Unicode string. 
    /// </summary>
    /// <param name="unicodeStr"></param>
    /// <returns></returns>
    public static string Base64DecodeUnicode(this string unicodeStr) => Base64Decode(unicodeStr).ToUnicodeString();

    /// <summary>
    /// Encodes a UTF8 string into a Base64 string.
    /// </summary>
    /// <param name="utf8Str"></param>
    /// <returns></returns>
    public static string Base64EncodeUtf8(this string utf8Str) => Base64Encode(utf8Str.FromUtf8String());
    /// <summary>
    /// Decodes a Base64 string into a UTF8 string.  
    /// </summary>
    /// <param name="utf8Str"></param>
    /// <returns></returns>
    public static string Base64DecodeUtf8(this string utf8Str) => Base64Decode(utf8Str).ToUtf8String();

    /// <summary>
    /// Turns an ASCII string into a byte array.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] FromAsciiString(this string str) => Encoding.ASCII.GetBytes(str);
    /// <summary>
    /// Turns a byte array into an ASCII string.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ToAsciiString(this byte[] bytes) => Encoding.ASCII.GetString(bytes);

    /// <summary>
    /// Turns a Unicode string into a byte array.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] FromUnicodeString(this string str) => Encoding.Unicode.GetBytes(str);
    /// <summary>
    /// Turns a byte array into a Unicode string.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ToUnicodeString(this byte[] bytes) => Encoding.Unicode.GetString(bytes);

    /// <summary>
    /// Turns a Utf8 string into a byte array.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] FromUtf8String(this string str) => Encoding.UTF8.GetBytes(str);
    /// <summary>
    /// Turns a byte array into a Utf8 string.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ToUtf8String(this byte[] bytes) => Encoding.UTF8.GetString(bytes);
}