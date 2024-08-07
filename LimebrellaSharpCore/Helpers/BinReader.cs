// v2024-08-06 23:19:03

using System.Text;

namespace LimebrellaSharpCore.Helpers;

/// Based on: https://github.com/tremwil/DS3SaveUnpacker/blob/master/DS3SaveUnpacker/BinRW.cs
public class BinReader(Stream stream) : BinaryReader(stream)
{
    /// <summary>
    /// LIFO stack to keep track of the positions across StepInto and StepOut calls
    /// </summary>
    protected Stack<long> Positions = new();

    /// <summary>
    /// Step into an offset. Call <see cref="StepOut"/> to come back to the last position.
    /// </summary>
    /// <param name="offset"></param>
    public void StepInto(long offset)
    {
        Positions.Push(BaseStream.Position);
        BaseStream.Position = offset;
    }

    /// <summary>
    /// Step out and back to the position before the last <see cref="StepInto"/> call.
    /// </summary>
    public void StepOut() 
        => BaseStream.Position = Positions.Pop();

    /// <summary>
    /// Read a 2-byte wide null-terminated string.
    /// </summary>
    public string ReadWideString()
    {
        StringBuilder sb = new();
        var chr = ReadUInt16();
        while (chr != 0)
        {
            sb.Append(char.ConvertFromUtf32(chr));
            chr = ReadUInt16();
        }
        return sb.ToString();
    }

    /// <summary>
    /// Read a 1-byte wide ShiftJIS null-terminated string.
    /// </summary>
    public string ReadShiftJis()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // crucial for "Shift_JIS" encoding
        List<byte> buffer = [];
        var chr = ReadByte();
        while (chr != 0)
        {
            buffer.Add(chr);
            chr = ReadByte();
        }
        return Encoding.GetEncoding("Shift_JIS").GetString(buffer.ToArray());
    }

    /// <summary>
    /// Reads number of bytes determined by <paramref name="count"/> at <paramref name="offset"/> without advancing the stream's position. 
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public byte[] ReadBytes(long offset, int count)
    {
        StepInto(offset);
        var result = base.ReadBytes(count);
        StepOut();
        return result;
    }

    /// <summary>
    /// Reads uint at <paramref name="offset"/> without advancing the stream's position. 
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public uint ReadUint(long offset)
    {
        StepInto(offset);
        var result = base.ReadUInt32();
        StepOut();
        return result;
    }
}