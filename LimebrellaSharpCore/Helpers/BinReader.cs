using System.Text;

namespace LimebrellaSharpCore.Helpers;

/// Based on: https://github.com/tremwil/DS3SaveUnpacker/blob/master/DS3SaveUnpacker/BinRW.cs
public class BinReader : BinaryReader
{
    /// <summary>
    /// LIFO stack to keep track of the positions across StepInto and StepOut calls
    /// </summary>
    protected Stack<long> Positions;

    public BinReader(Stream stream) : base(stream)
    {
        Positions = new Stack<long>();
    }

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
    public void StepOut() => BaseStream.Position = Positions.Pop();

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
        List<byte> buffer = new();
        var chr = ReadByte();
        while (chr != 0)
        {
            buffer.Add(chr);
            chr = ReadByte();
        }
        return Encoding.GetEncoding("Shift_JIS").GetString(buffer.ToArray());
    }
}