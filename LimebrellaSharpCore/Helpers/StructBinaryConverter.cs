using System.Runtime.InteropServices;

namespace LimebrellaSharpCore.Helpers;

/// <summary>
/// Provides static methods for serializing and deserializing reference type objects to and from their binary representations using unmanaged memory operations.
/// </summary>
/// <remarks>Based on: https://github.com/tremwil/DS3SaveUnpacker/blob/master/DS3SaveUnpacker/GenBitConverter.cs</remarks>
public static class StructBinaryConverter
{
    /// <summary>
    /// Converts an object of reference type to a byte array using its memory representation.
    /// </summary>
    /// <typeparam name="T">The reference type of the object to convert to a byte array.</typeparam>
    /// <param name="obj">The object to convert. Must be a reference type and not null.</param>
    /// <returns>A byte array containing the memory representation of the specified object.</returns>
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
    /// Converts a byte array to an instance of the specified reference type using unmanaged memory operations.
    /// </summary>
    /// <typeparam name="T">The reference type to convert the byte array to.</typeparam>
    /// <param name="buffer">The byte array containing the data to be converted into an instance of type <typeparamref name="T"/>.</param>
    /// <returns>An instance of type <typeparamref name="T"/> containing the data from the byte array, or <see langword="null"/> if the conversion fails.</returns>
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
    /// Reads a structure of type T from the current position in the binary stream.
    /// </summary>
    /// <typeparam name="T">The reference type of the structure to read from the stream. Must be a class.</typeparam>
    /// <param name="br">The BinaryReader instance to read the structure from. The stream must be positioned at the start of the structure data.</param>
    /// <returns>An instance of type T containing the data read from the stream, or <see langword="null"/> if the conversion fails.</returns>
    public static T? ReadStruct<T>(this BinaryReader br) where T : class
    {
        var size = Marshal.SizeOf<T>();

        var buff = new byte[size];
        _ = br.Read(buff, 0, size);

        return ToStruct<T>(buff);
    }

    /// <summary>
    /// Reads a structure of type T from the specified span of bytes.
    /// </summary>
    /// <typeparam name="T">The reference type to read from the byte span. Must be a class.</typeparam>
    /// <param name="span">The span of bytes containing the serialized representation of the structure to read.</param>
    /// <returns>An instance of type T if the span contains a valid representation; otherwise, <see langword="null"/>.</returns>
    public static T? ReadStruct<T>(this Span<byte> span) where T : class
        => ToStruct<T>(span.ToArray());

    /// <summary>
    /// Writes the binary representation of the specified object to the underlying stream using the provided BinaryWriter.
    /// </summary>
    /// <typeparam name="T">The type of the object to write. Must be a reference type.</typeparam>
    /// <param name="bw">The BinaryWriter instance used to write the object's data to the stream.</param>
    /// <param name="obj">The object to serialize and write to the stream.</param>
    public static void WriteStruct<T>(this BinaryWriter bw, T obj) where T : class
    {
        var buff = ToBytes(obj);
        bw.Write(buff, 0, buff.Length);
    }

    /// <summary>
    /// Serializes the specified reference type object to a byte array representation.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize. Must be a reference type.</typeparam>
    /// <param name="obj">The object instance to serialize to a byte array.</param>
    /// <returns>A byte array containing the serialized representation of the specified object.</returns>
    public static byte[] WriteStruct<T>(this T obj) where T : class
        => ToBytes(obj);
}