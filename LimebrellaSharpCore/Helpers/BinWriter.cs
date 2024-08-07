// v2024-08-06 23:19:03

using System.Text;

namespace LimebrellaSharpCore.Helpers;

/// Based on: https://github.com/tremwil/DS3SaveUnpacker/blob/master/DS3SaveUnpacker/BinRW.cs
public class BinWriter(Stream stream) : BinaryWriter(stream)
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
    /// Write a 2-byte wide null-terminated string.
    /// </summary>
    public void WriteWideString(string str)
    {
        foreach (var c in str)
        {
            Write((ushort)c);
        }
        Write((ushort)0);
    }

    /// <summary>
    /// Write a 1-byte wide ShiftJIS null-terminated string.
    /// </summary>
    public void WriteShiftJis(string str)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // crucial for "Shift_JIS" encoding
        var bytes = Encoding.GetEncoding("Shift_JIS").GetBytes(str);
        Write(bytes);
        Write((byte)0);
    }
}