﻿using System.Runtime.InteropServices;

namespace LimebrellaSharpCore.Models.DSSS.Lime;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
public class DsssHeader
{
    /// <summary>
    /// This should be the ANSI for "DSSS", or 0x5353_5344.
    /// </summary>
    public uint FileFormat { get; set; } = 0x5353_5344;

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown1 { get; set; } = 0x2;

    /// <summary>
    /// Save File encryption type.
    /// Possible values: ?:None; ?:AutoStrong; ?:XOR; ?:BlowFish; ?:Citrus; 16:Lime; ?:RdsModule
    /// </summary>
    public uint EncryptionType { get; set; } = 0x10;

    /// <summary>
    /// Don't know what's there.
    /// </summary>
    public uint Unknown2 { get; set; } = 0U;

    /// <summary>
    /// Create a parameter-less <see cref="DsssHeader"/>.
    /// </summary>
    public DsssHeader() { }

    /// <summary>
    /// Create a <see cref="DsssHeader"/> with given parameters.
    /// </summary>
    /// <param name="unknown1"></param>
    /// <param name="encryptionType"></param>
    /// <param name="unknown2"></param>
    public DsssHeader(uint unknown1, uint encryptionType, uint unknown2)
    {
        Unknown1 = unknown1;
        EncryptionType = encryptionType;
        Unknown2 = unknown2;
    }

    /// <summary>
    /// Returns false if this <see cref="FileFormat"/> does not make sense.
    /// </summary>
    /// <returns></returns>
    public BoolResult CheckIntegrity() => 
        FileFormat == 0x5353_5344 ? new BoolResult(true) : new BoolResult(false, "Invalid file header!");
}