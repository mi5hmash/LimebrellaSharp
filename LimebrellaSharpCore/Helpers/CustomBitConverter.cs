using System.Runtime.InteropServices;
using static System.Globalization.NumberStyles;

namespace LimebrellaSharpCore.Helpers;

/// Based on: https://github.com/tremwil/DS3SaveUnpacker/blob/master/DS3SaveUnpacker/GenBitConverter.cs
public static class CustomBitConverter
{
    /// <summary>
    /// Splits a <paramref name="inputString"/> into chunks of fixed <paramref name="chunkSize"/>.
    /// </summary>
    /// <param name="inputString"></param>
    /// <param name="chunkSize"></param>
    /// <returns></returns>
    public static string[] SplitStringIntoChunks(string inputString, int chunkSize)
    {
        return Enumerable.Range(0, (int)Math.Ceiling((double)inputString.Length / chunkSize))
            .Select(i => inputString.Substring(i * chunkSize, Math.Min(chunkSize, inputString.Length - i * chunkSize)))
            .ToArray();
    }

    /// <summary>
    /// Convert structure to bytes.
    /// </summary>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static byte[] ToBytes<T>(T obj) where T : class
    {
        var size = Marshal.SizeOf(obj);
        var arr = new byte[size];

        var ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(obj, ptr, false);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);

        return arr;
    }

    /// <summary>
    /// Convert bytes to structure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static T? ToStruct<T>(byte[] buffer) where T : class
    {
        var size = Marshal.SizeOf<T>();
        var ptr = Marshal.AllocHGlobal(size);

        Marshal.Copy(buffer, 0, ptr, size);
        var obj = Marshal.PtrToStructure<T>(ptr);
        Marshal.FreeHGlobal(ptr);

        return obj;
    }

    /// <summary>
    /// Read a structure from a BinaryReader
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bin"></param>
    /// <returns></returns>
    public static T? ReadStruct<T>(this BinaryReader bin) where T : class
    {
        var size = Marshal.SizeOf<T>();

        var buff = new byte[size];
        _ = bin.Read(buff, 0, size);

        return ToStruct<T>(buff);
    }

    /// <summary>
    /// Write a structure to a BinaryWriter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bin"></param>
    /// <param name="obj"></param>
    public static void WriteStruct<T>(this BinaryWriter bin, T obj) where T : class
    {
        var buff = ToBytes(obj);
        bin.Write(buff, 0, buff.Length);
    }

    /// <summary>
    /// Turns a hex string into a byte array.
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static byte[] ToBytes(this string hex)
    {
        hex = hex.Replace(" ", "").Replace("-", "");
        var data = new byte[hex.Length / 2];
        for (var i = 0; i < hex.Length; i += 2)
        {
            data[i / 2] = byte.Parse(hex.Substring(i, 2), HexNumber);
        }
        return data;
    }

    /// <summary>
    /// Turns a byte array into a hex string.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ToHexString(this byte[] bytes) =>
        BitConverter.ToString(bytes).Replace("-", string.Empty);

    /// <summary>
    /// Turns a hex string into an ulong array.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="isBigEndian"></param>
    /// <returns></returns>
    public static ulong[] ToUlongArray(this string str, bool isBigEndian = true)
    {
        var strArr = SplitStringIntoChunks(str.Replace(" ", "").Replace("-", ""), 16);
        List<ulong> uLongs = [];
        foreach (var chunk in strArr)
        {
            var formattedChunk = string.Empty;
            if (isBigEndian) formattedChunk = string.Join("", Enumerable.Range(0, chunk.Length / 2).Select(i => chunk.Substring(i * 2, 2)).Reverse());
            uLongs.Add(Convert.ToUInt64(formattedChunk, 16));
        }
        return [.. uLongs];
    }

    /// <summary>
    /// Turns a hex string into an uint array.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="isBigEndian"></param>
    /// <returns></returns>
    public static uint[] ToUintArray(this string str, bool isBigEndian = true)
    {
        var strArr = SplitStringIntoChunks(str.Replace(" ", "").Replace("-", ""), 8);
        List<uint> uInts = [];
        foreach (var chunk in strArr)
        {
            var formattedChunk = string.Empty;
            if (isBigEndian) formattedChunk = string.Join("", Enumerable.Range(0, chunk.Length / 2).Select(i => chunk.Substring(i * 2, 2)).Reverse());
            uInts.Add(Convert.ToUInt32(formattedChunk, 16));
        }
        return [.. uInts];
    }

    /// <summary>
    /// Turns a ulong into a hex string.
    /// </summary>
    /// <param name="int64"></param>
    /// <param name="isBigEndian"></param>
    /// <returns></returns>
    public static string ToHexString(this ulong int64, bool isBigEndian = true)
    {
        var str = int64.ToString("X").PadLeft(16, '0');
        var iEnum = isBigEndian ? Enumerable.Range(0, str.Length / 2).Select(i => str.Substring(i * 2, 2)).Reverse()
            : Enumerable.Range(0, str.Length / 2).Select(i => str.Substring(i * 2, 2));
        return string.Join(" ", iEnum.ToArray());
    }
    /// <summary>
    /// Turns a ulong into a hex string.
    /// </summary>
    /// <param name="int64"></param>
    /// <param name="isBigEndian"></param>
    /// <returns></returns>
    public static string ToHexString(this ulong[] int64, bool isBigEndian = true) 
        => string.Join(" ", int64.Select(chunk => chunk.ToHexString(isBigEndian)).ToList());

}