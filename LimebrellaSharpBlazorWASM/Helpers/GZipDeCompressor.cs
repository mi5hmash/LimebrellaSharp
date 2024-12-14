// v2024-12-08 21:16:48

using System.IO.Compression;

namespace LimebrellaSharpBlazorWASM.Helpers;

public static class GZipDeCompressor
{
    /// <summary>
    /// Compresses a byte array using GZip compression.
    /// </summary>
    /// <param name="bytes">The data to compress as a byte array.</param>
    /// <returns>A byte array containing the compressed data.</returns>
    public static byte[] GzipCompress(this byte[] bytes)
    {
        using var mso = new MemoryStream();
        using (var gz = new GZipStream(mso, CompressionLevel.Optimal))
        {
            gz.Write(bytes, 0, bytes.Length);
        }
        return mso.ToArray();
    }

    /// <summary>
    /// Asynchronously compresses a byte array using GZip compression.
    /// </summary>
    /// <param name="bytes">The data to compress as a byte array.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is a byte array containing the compressed data.</returns>
    public static async Task<byte[]> GZipCompressAsync(this byte[] bytes)
    {
        using var mso = new MemoryStream();
        await using (GZipStream gz = new(mso, CompressionLevel.Optimal))
        {
            gz.Write(bytes, 0, bytes.Length);
        }
        return mso.ToArray();
    }

    /// <summary>
    /// Decompresses a byte array that was compressed using GZip compression.
    /// </summary>
    /// <param name="bytes">The GZip-compressed data as a byte array.</param>
    /// <returns>A byte array containing the decompressed data.</returns>
    public static byte[] GzipDecompress(this byte[] bytes)
    {
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using (var gz = new GZipStream(msi, CompressionMode.Decompress))
        {
            gz.CopyTo(mso);
        }
        return mso.ToArray();
    }

    /// <summary>
    /// Asynchronously decompresses a byte array that was compressed using GZip compression.
    /// </summary>
    /// <param name="bytes">The GZip-compressed data as a byte array.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is a byte array containing the decompressed data.</returns>
    public static async Task<byte[]> GZipDecompressAsync(this byte[] bytes)
    {
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        await using (var gz = new GZipStream(msi, CompressionMode.Decompress))
        {
            await gz.CopyToAsync(mso);
        }
        return mso.ToArray();
    }
}