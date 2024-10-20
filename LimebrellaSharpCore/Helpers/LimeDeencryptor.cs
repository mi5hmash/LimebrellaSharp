using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using LimebrellaSharpCore.Models.DSSS.Lime;

namespace LimebrellaSharpCore.Helpers;

public class LimeDeencryptor
{
    #region CONSTANTS

    private const int ContainerCapacity = 34;
    private const int ChecksumContainerCapacity = 25;

    private const ulong KeyType = 20;
    private const int EncSteps = 10;
    private const int AesBlockLength = EncSteps + 1;

    private readonly ulong[] _privateKey1;
    private readonly ulong[] _privateKey2;

    private readonly byte[] _checksumTable1;
    private readonly byte[] _checksumTable2;
    private readonly ulong[] _checksumTable3;

    #endregion

    /// <summary>
    /// Default Constructor that loads configuration.
    /// </summary>
    public LimeDeencryptor()
    {
        _privateKey1 = "RjMzQjZGQjk3MkEwQjcyNTE1RTQ1QzM5MTgyOUUxODJBRDhBOUJEQzBBNjREMzQ0NEQ3OUM4MTBBQjg2MzcxNw=="
            .Base64DecodeUtf8().ToUlongArray();
        _privateKey2 = "RTY2RjU0NEFGQ0NFNjhDNUVGMDdCOUEwN0IyNzc1ODUzNDRBMURCNjEzNzZFODMxRjczQjlGQkQ1RjQ0RjcxNQ=="
            .Base64DecodeUtf8().ToUlongArray();

        _checksumTable1 = "MDEwMzA2MEEwRjE1MUMyNDJEMzcwMjBFMUIyOTM4MDgxOTJCM0UxMjI3M0QxNDJD"
            .Base64DecodeUtf8().ToByteArray();
        _checksumTable2 = "MEEwNzBCMTExMjAzMDUxMDA4MTUxODA0MEYxNzEzMEQwQzAyMTQwRTE2MDkwNjAx"
            .Base64DecodeUtf8().ToByteArray();
        _checksumTable3 = "MDEwMDAwMDAwMDAwMDAwMDgyODAwMDAwMDAwMDAwMDA4QTgwMDAwMDAwMDAwMDgwMDA4MDAwODAwMDAwMDA4MDhCODAwMDAwMDAwMDAwMDAwMTAwMDA4MDAwMDAwMDAwODE4MDAwODAwMDAwMDA4MDA5ODAwMDAwMDAwMDAwODA4QTAwMDAwMDAwMDAwMDAwODgwMDAwMDAwMDAwMDAwMDA5ODAwMDgwMDAwMDAwMDAwQTAwMDA4MDAwMDAwMDAwOEI4MDAwODAwMDAwMDAwMDhCMDAwMDAwMDAwMDAwODA4OTgwMDAwMDAwMDAwMDgwMDM4MDAwMDAwMDAwMDA4MDAyODAwMDAwMDAwMDAwODA4MDAwMDAwMDAwMDAwMDgwMEE4MDAwMDAwMDAwMDAwMDBBMDAwMDgwMDAwMDAwODA4MTgwMDA4MDAwMDAwMDgwODA4MDAwMDAwMDAwMDA4MDAxMDAwMDgwMDAwMDAwMDAwODgwMDA4MDAwMDAwMDgwMDEwMzA2MEEwRjE1MUMyNA=="
            .Base64DecodeUtf8().ToUlongArray();
    }

    #region HELPER FUNCS

    /// <summary>
    /// Randomizes the range of bytes in a given span.
    /// </summary>
    /// <param name="span"></param>
    /// <param name="startPos"></param>
    /// <param name="length"></param>
    public static void RandomizeSpan(Span<byte> span, int length = 0, int startPos = 0)
    {
        startPos = startPos < 0 ? 0 : startPos;
        startPos = startPos > span.Length ? span.Length : startPos;
        length = length < 0 ? 0 : length;
        length = startPos + length > span.Length ? span.Length - startPos : length;

        Random random = new();
        for (var i = 0; i < length; i++) span[startPos + i] = (byte)random.Next(byte.MaxValue + 1);
    }

    /// <summary>
    /// Multiplies two ulong values.
    /// Function based on: https://gist.github.com/cocowalla/6070a53445e872f2bb24304712a3e1d2.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns>High-order ulong value</returns>
    private static ulong MulHigh(ulong left, ulong right)
    {
        ulong l0 = (uint)left;
        var l1 = left >> 32;
        ulong r0 = (uint)right;
        var r1 = right >> 32;

        var p11 = l1 * r1;
        var p01 = l0 * r1;
        var p10 = l1 * r0;
        var p00 = l0 * r0;

        // 64-bit product + two 32-bit values
        var middle = p10 + (p00 >> 32) + (uint)p01;

        // 64-bit product + two 32-bit values
        return p11 + (middle >> 32) + (p01 >> 32);
    }
    /// <summary>
    /// Multiplies two ulong values.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns>Low-order ulong value</returns>
    private static ulong MulLow(ulong left, ulong right) =>
        left * right;

    /// <summary>
    /// Returns the zero-based position of the last non-zero element in the span.
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    private static int LastNonZeroIndexZeroBased<T>(ReadOnlySpan<T> span) where T : struct, IEquatable<T>
    {
        for (var i = span.Length; i > 0; i--)
            if (!span[i - 1].Equals(default))
                return i;
        return 0;
    }

    /// <summary>
    /// Performs NOT operation on provided SteamID.
    /// </summary>
    /// <param name="steamId"></param>
    /// <returns></returns>
    private static ulong NotSteamId(ulong steamId) => ~steamId;

    /// <summary>
    /// Calculates an index of a radical expression.
    /// </summary>
    /// <param name="radicand"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    private static int RootDegree(ulong radicand, int step)
    {
        var index = 0;
        do
        {
            index++;
            radicand >>= step;
        }
        while (radicand != 0);
        return index;
    }

    /// <summary>
    /// Returns true if the Most Significant Bit is set.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    private static bool IsMostSignificantBitSet(ulong number)
        => (number & 0x8000000000000000) != 0;
    private static bool IsMostSignificantBitSet(uint number)
        => (number & 0x80000000) != 0;

    #endregion

    #region ENCRYPTION METHODS

    /// <summary>
    /// Handles overflow in the provided <paramref name="container"/>.
    /// </summary>
    /// <param name="container"></param>
    private static void HandleOverflow(Span<ulong> container)
    {
        // create a localContainer
        Span<ulong> localContainer = stackalloc ulong[ContainerCapacity];
        localContainer[0] = 1;

        SubtractContainers(container, localContainer);
        const int x = 2;
        for (var i = 0; i < ContainerCapacity - x; i++)
            container[i] = ~container[i] & 0xFFFFFFFFFFFFFFFF;
        for (var i = ContainerCapacity - x; i < ContainerCapacity; i++)
            container[i] = ~container[i];
    }

    /// <summary>
    /// First type of encryption.
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    /// <exception cref="OverflowException"></exception>
    private static void EncryptionFirst(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        // check for empty containers
        var containerALength = LastNonZeroIndexZeroBased<ulong>(containerA);
        if (containerALength == 0) return;
        var containerBLength = LastNonZeroIndexZeroBased(containerB);
        if (containerBLength == 0) goto ORDER_66;

        // create a localContainerB
        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacity];
        containerB.CopyTo(localContainerB);

        // detect overflow in...
        var overflowSwitch = false;
        // ... localContainerA
        if (IsMostSignificantBitSet(containerA[^1]))
        {
            throw new OverflowException("ContainerA Overflow");
            overflowSwitch ^= true;
            HandleOverflow(containerA);
            // re-check container length
            containerALength = LastNonZeroIndexZeroBased<ulong>(containerA);
        }

        // ... localContainerB
        if (IsMostSignificantBitSet(localContainerB[^1]))
        {
            throw new OverflowException("ContainerB Overflow");
            overflowSwitch ^= true;
            HandleOverflow(localContainerB);
            // re-check container length
            containerBLength = LastNonZeroIndexZeroBased<ulong>(localContainerB);
            if (containerBLength == 0) goto OVERFLOWN_ENDING;
        }

        // create a resultContainer
        Span<ulong> resultContainer = stackalloc ulong[ContainerCapacity];

        // manipulate bytes in both containers
        if (containerALength > 0)
        {
            for (var y = 0; y < containerALength; y++)
            {
                ulong salt = 0;
                if (containerBLength > 0)
                {
                    var firstPart = containerA[y];
                    for (var i = 0; i < containerBLength; i++)
                    {
                        var lowBytes = MulLow(firstPart, localContainerB[i]);
                        var highBytes = MulHigh(firstPart, localContainerB[i]);
                        var basis = lowBytes + salt;
                        if (basis < salt) highBytes++;
                        var result = basis + resultContainer[y + i];
                        if (result < basis) highBytes++;
                        resultContainer[y + i] = result;
                        salt = highBytes;
                    }
                }

                resultContainer[containerBLength + y] = salt;
            }
        }

        // if there was only one overflow then pick an alternative route
        if (overflowSwitch) goto OVERFLOWN_ENDING;

        // Update referenced containerA
        resultContainer.CopyTo(containerA);
        return;

    OVERFLOWN_ENDING:
        containerA.Clear();
        SubtractContainers(containerA, resultContainer);
        return;

    // Execute...
    ORDER_66:
        // set all the containerA elements to 0
        containerA.Clear();
    }

    /// <summary>
    /// Second type of encryption.
    /// </summary>
    /// <param name="dataContainer"></param>
    /// <param name="bits"></param>
    private static void EncryptionSecond(Span<ulong> dataContainer, int bits)
    {
        var division = bits >> 6; // division by 64
        var reminder = bits & 0x3F; // division reminder

        if (reminder != 0)
        {
            if (division != ContainerCapacity && division != ContainerCapacity - 1)
            {
                var curElement = ContainerCapacity;
                for (var i = ContainerCapacity - division; i > 1; i--, curElement--)
                {
                    dataContainer[curElement - 1] = dataContainer[i - 1] << reminder;
                    dataContainer[curElement - 1] |= dataContainer[i - 2] >> (0x40 - reminder);
                }
            }
            dataContainer[division] = dataContainer[0] << reminder;
        }
        else if (division != 0)
        {
            var curElement = ContainerCapacity - division;
            if (division != ContainerCapacity)
            {
                for (var i = ContainerCapacity; curElement > 0; i--, curElement--)
                    dataContainer[i - 1] = dataContainer[curElement - 1];
            }
        }

        if (division == 0) return;
        for (var i = division; i > 0; i--) dataContainer[division - i] = 0;
    }

    /// <summary>
    /// Subtracts two containers from one another.
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    /// <returns>Modifies <paramref name="containerA"/></returns>
    private static void SubtractContainers(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        byte testA = 0;
        byte testB = 0;
        for (var i = 0; i < ContainerCapacity; i++)
        {
            var test0 = Convert.ToByte(testA | testB);
            var newValue = containerA[i] - containerB[i] - test0;
            testA = containerA[i] == newValue ? test0 : (byte)0;
            testB = Convert.ToByte(containerA[i] < newValue);
            containerA[i] = newValue;
        }
    }

    /// <summary>
    /// Adds two containers to one another.
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    /// <returns>Modifies <paramref name="containerA"/></returns>
    private static void AddContainers(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        byte testA = 0;
        byte testB = 0;
        for (var i = 0; i < ContainerCapacity; i++)
        {
            var test0 = Convert.ToByte(testA | testB);
            var newValue = containerA[i] + containerB[i] + test0;
            testA = containerB[i] == newValue ? test0 : (byte)0;
            testB = Convert.ToByte(newValue < containerB[i]);
            containerA[i] = newValue;
        }
    }

    #endregion

    #region METHODS

    /// <summary>
    /// Hashes public keys and combines them together.
    /// </summary>
    /// <param name="segmentHashedKey"></param>
    /// <param name="cKey1"></param>
    /// <param name="cSteamId"></param>
    /// <param name="limeBank"></param>
    private void HashPublicKeys(Span<ulong> segmentHashedKey, ReadOnlySpan<ulong> cKey1, ReadOnlySpan<ulong> cSteamId, ReadOnlySpan<LimeHashedKeyBank> limeBank)
    {
        Span<ulong> localContainerA = stackalloc ulong[ContainerCapacity];
        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacity];
        for (var i = 0; i < limeBank.Length; i++)
        {
            limeBank[i].Header.CopyTo(localContainerA);
            Limegator(localContainerA, cSteamId, cKey1);
            limeBank[i].KeyFragment.CopyTo(localContainerB);
            Limeghetti(localContainerB, localContainerA);
            // put the calculated part of the key in the segmentHashedKey
            segmentHashedKey[i] = localContainerB[0];
        }
    }

    /// <summary>
    /// Generates the Encryption Key.
    /// </summary>
    /// <param name="aesRoundKeys"></param>
    /// <param name="inputKey"></param>
    private static void AesKeygen(Span<Vector128<byte>> aesRoundKeys, ReadOnlySpan<Vector128<byte>> inputKey)
    {
        // build the first block
        aesRoundKeys[0] = inputKey[0];
        for (var i = 0; i < EncSteps; i++)
        {
            var innerRoundKey = i switch
            {
                0 => Aes.KeygenAssist(aesRoundKeys[i], 0x01),
                1 => Aes.KeygenAssist(aesRoundKeys[i], 0x02),
                2 => Aes.KeygenAssist(aesRoundKeys[i], 0x04),
                3 => Aes.KeygenAssist(aesRoundKeys[i], 0x08),
                4 => Aes.KeygenAssist(aesRoundKeys[i], 0x10),
                5 => Aes.KeygenAssist(aesRoundKeys[i], 0x20),
                6 => Aes.KeygenAssist(aesRoundKeys[i], 0x40),
                7 => Aes.KeygenAssist(aesRoundKeys[i], 0x80),
                8 => Aes.KeygenAssist(aesRoundKeys[i], 0x1B),
                9 => Aes.KeygenAssist(aesRoundKeys[i], 0x36),
                // unused encryption steps
                //10 => 0x6C,
                //11 => 0xD8,
                //12 => 0xAB,
                //13 => 0x4D,
                //14 => 0x9A,
                _ => Aes.KeygenAssist(aesRoundKeys[i], 0x0)
            };
            // Shift xmm2 left by 4 bytes
            var shift1 = Sse2.ShiftLeftLogical128BitLane(aesRoundKeys[i].AsUInt32(), 4).AsInt32();
            // Shift shift1 left by 4 bytes
            var shift2 = Sse2.ShiftLeftLogical128BitLane(shift1.AsUInt32(), 4).AsInt32();
            // Shift shift2 left by 4 bytes
            var shift3 = Sse2.ShiftLeftLogical128BitLane(shift2, 4);
            // Compute the final result using shuffle and XOR instructions
            var shuffle1 = Sse2.Shuffle(innerRoundKey.AsInt32(), 255);
            var xor1 = Sse2.Xor(shift1, aesRoundKeys[i].AsInt32());
            var xor2 = Sse2.Xor(shift2, xor1);
            var xor3 = Sse2.Xor(xor2, shift3);
            var xor4 = Sse2.Xor(xor3, shuffle1);
            // add key to the roundKeys
            aesRoundKeys[i + 1] = xor4.AsByte();
        }
        // build the second block
        for (var i = 0; i < EncSteps; i++)
            aesRoundKeys[AesBlockLength + i] = Aes.InverseMixColumns(aesRoundKeys[i]);
        aesRoundKeys[^2] = aesRoundKeys[AesBlockLength - 1];
        // close roundKeys with the second part of an inputKey
        aesRoundKeys[^1] = inputKey[1];
    }
    
    /// <summary>
    /// Prepare a delicious knot of Limeghetti.
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    /// <exception cref="OverflowException"></exception>
    private static void Limeghetti(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        // check for empty containers
        var containerALength = LastNonZeroIndexZeroBased<ulong>(containerA);
        if (containerALength == 0) return;
        var containerBLength = LastNonZeroIndexZeroBased(containerB);
        if (containerBLength == 0) goto ORDER_66;
        
        // create a localContainerB
        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacity];
        containerB.CopyTo(localContainerB);

        // create other localContainers
        Span<ulong> localContainerC = stackalloc ulong[ContainerCapacity];
        Span<ulong> localContainerD = stackalloc ulong[ContainerCapacity];
        Span<ulong> localContainerE = stackalloc ulong[ContainerCapacity];
        Span<ulong> resultContainer = stackalloc ulong[ContainerCapacity];

        // detect overflow in...
        // ... localContainerA
        if (IsMostSignificantBitSet(containerA[^1]))
        {
            throw new OverflowException("ContainerA Overflow");
            HandleOverflow(containerA);
        }
        // ... localContainerB
        if (IsMostSignificantBitSet(localContainerB[^1]))
        {
            throw new OverflowException("ContainerB Overflow");
            HandleOverflow(localContainerB);
        }

        if (IsMostSignificantBitSet(containerA[^1]))
        {
            if (!IsMostSignificantBitSet(localContainerB[^1])) goto ORDER_66;
            if (IsLastElementOfASmallerThanB(containerA, localContainerB)) goto ORDER_66;
        }
        else if (!IsMostSignificantBitSet(localContainerB[^1]))
        {
            if (IsLastElementOfASmallerThanB(containerA, localContainerB)) goto ORDER_66;
        }

        // check container length
        var localContainerBLength = LastNonZeroIndexZeroBased<ulong>(localContainerB);
        int localContainerALength;

        // calculate bits
        var rootDegree = localContainerBLength == 0 ? 0 : RootDegree(localContainerB[localContainerBLength - 1], 1);
        var bits = 32 - (rootDegree & 0x1F);

        // perform EncryptionSecond on both localContainers
        EncryptionSecond(containerA, bits);
        EncryptionSecond(localContainerB, bits);

        // clear localContainerH
        resultContainer.Clear();

        // re-check container length
        localContainerBLength = LastNonZeroIndexZeroBased<ulong>(localContainerB);

        // remember the last element of localContainerA
        var lastElementA = containerA[^1];
        // remember the last element of localContainerB
        var lastElementB = containerA[^1];

        if (localContainerBLength > 0)
        {
            var tinyHashesB = 2 * localContainerBLength - 1;
            var lastQueueElemB = localContainerB[localContainerBLength - 1] >> 32;
            if (lastQueueElemB == 0)
            {
                tinyHashesB--;
                lastQueueElemB = localContainerB[localContainerBLength - 1];
            }

            LOOP_BREAKER:
            lastElementA = containerA[^1];
            if (IsMostSignificantBitSet(containerA[^1]))
            {
                if (!IsMostSignificantBitSet(lastElementB)) goto ESCAPE;
                if (IsLastElementOfASmallerThanB(containerA, localContainerB)) goto ESCAPE;
            }
            else if (!IsMostSignificantBitSet(lastElementB))
            {
                if (IsLastElementOfASmallerThanB(containerA, localContainerB)) goto ESCAPE;
            }
            // re-check container length
            localContainerALength = LastNonZeroIndexZeroBased<ulong>(containerA);
            switch (localContainerALength)
            {
                case 0:
                    goto ESCAPE;
                case >= 2:
                    {
                        var tinyHashesA = 2 * localContainerALength - 2;
                        var lastQueueElemA = containerA[localContainerALength - 1];
                        if (lastQueueElemA >> 32 == 0)
                        {
                            tinyHashesA--;
                            lastQueueElemA = (containerA[localContainerALength - 2] >> 32) + (lastQueueElemA << 32);
                        }
                        var hashesGap = tinyHashesA - tinyHashesB;
                        var lastQueueElemDiv = lastQueueElemA / lastQueueElemB;

                        // copy localContainerB into localContainerC
                        localContainerB.CopyTo(localContainerC);

                        if (tinyHashesA >= tinyHashesB)
                        {
                            if (lastQueueElemDiv >> 32 != 0) lastQueueElemDiv = 0xFFFFFFFF;
                            EncryptionSecond(localContainerC, 32 * hashesGap);
                            // prepare localContainerD
                            localContainerD.Clear();
                            localContainerD[0] = lastQueueElemDiv;
                            EncryptionFirst(localContainerC, localContainerD);
                        }
                        else
                        {
                            hashesGap = 0;
                            lastQueueElemDiv = 1;
                        }

                        while (true)
                        {
                            if (!IsMostSignificantBitSet(lastElementA))
                            {
                                if (IsMostSignificantBitSet(localContainerC[^1])) break;
                            }
                            else if (!IsMostSignificantBitSet(localContainerC[^1])) goto CONTINUE;

                            if (!IsLastElementOfASmallerThanB(containerA, localContainerC)) break;

                            CONTINUE:
                            // prepare localContainerD
                            localContainerB.CopyTo(localContainerD);

                            EncryptionSecond(localContainerD, 32 * hashesGap);
                            
                            SubtractContainers(localContainerC, localContainerD);
                            lastQueueElemDiv--;
                        }

                        // prepare localContainerD
                        localContainerD.Clear();
                        // prepare localContainerE
                        localContainerE.Clear();
                        localContainerE[0] = lastQueueElemDiv;

                        AddContainers(localContainerD, localContainerE);
                        EncryptionSecond(localContainerD, 32 * hashesGap);
                        lastElementB = localContainerB[^1];

                        // calculate resultContainer
                        AddContainers(resultContainer, localContainerD);

                        // prepare localContainerD
                        localContainerC.CopyTo(localContainerD);
                        // update containerA
                        SubtractContainers(containerA, localContainerD);
                        goto LOOP_BREAKER;
                    }
            }
        }
        ESCAPE:
        localContainerALength = LastNonZeroIndexZeroBased<ulong>(containerA);
        localContainerBLength = LastNonZeroIndexZeroBased<ulong>(localContainerB);

        if (localContainerBLength > 0 && localContainerALength == 0) goto SECOND_GATE;
        if (localContainerB.SequenceEqual(containerA)) goto SECOND_GATE;

        if (IsMostSignificantBitSet(lastElementB) && IsMostSignificantBitSet(lastElementA)) goto FIRST_GATE;
        if (!IsMostSignificantBitSet(lastElementA)) goto FIRST_GATE;
        goto RETURNAL;

    FIRST_GATE:
        if (!IsLastElementOfASmallerThanB(localContainerB, containerA)) goto RETURNAL; 
    SECOND_GATE:
        localContainerE.Clear();
        localContainerE[0] = containerA[0] / localContainerB[0];
        AddContainers(resultContainer, localContainerE);
    RETURNAL:
        // Padawans survived
        resultContainer.CopyTo(containerA);
        return;

    // Execute...
    ORDER_66:
        // set all the containerA elements to 0
        containerA.Clear();
        return;

        bool IsLastElementOfASmallerThanB(ReadOnlySpan<ulong> contA, ReadOnlySpan<ulong> contB)
        {
            for (var i = ContainerCapacity; i > 0; i--)
            {
                // check if nth element of contA is less than contB
                if (contA[i - 1] < contB[i - 1]) return true;
                // check if nth element of contA is greater than contB
                if (contA[i - 1] > contB[i - 1]) return false;
            }
            return false;
        }
    }

    /// <summary>
    /// Watch out for its sharp teeth!
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    /// <param name="containerC"></param>
    /// <returns>Modified <paramref name="containerA"/></returns>
    private void Limegator(Span<ulong> containerA, ReadOnlySpan<ulong> containerB, ReadOnlySpan<ulong> containerC)
    {
        Span<ulong> localContainerA = stackalloc ulong[ContainerCapacity];
        localContainerA[0] = 1;

        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacity];
        containerB.CopyTo(localContainerB);

        Span<ulong> localContainerC = stackalloc ulong[ContainerCapacity];
        Span<ulong> localContainerD = stackalloc ulong[ContainerCapacity];

        while (true)
        {
            localContainerC.Clear();
            if (LoopBreaker(localContainerB, localContainerC)) break;

            if ((localContainerB[0] & 1) != 0)
            {
                localContainerA.CopyTo(localContainerC);
                EncryptionFirst(localContainerC, containerA);
                localContainerC.CopyTo(localContainerD);
                Limeghetti(localContainerD, containerC);
                EncryptionFirst(localContainerD, containerC);
                SubtractContainers(localContainerC, localContainerD);
                localContainerC.CopyTo(localContainerA);
            }
            containerA.CopyTo(localContainerC);
            containerA.CopyTo(localContainerD);
            EncryptionFirst(localContainerC, localContainerD);
            localContainerC.CopyTo(localContainerD);
            Limeghetti(localContainerD, containerC);
            EncryptionFirst(localContainerD, containerC);
            SubtractContainers(localContainerC, localContainerD);
            localContainerC.CopyTo(containerA);
            
            for (var i = 0; i < ContainerCapacity - 1; i++)
            {
                localContainerB[i] >>= 1;
                localContainerB[i] |= localContainerB[i + 1] << 63;
            }
            localContainerB[^1] >>= 1;
        }

        localContainerA.CopyTo(containerA);
        return;

        static bool LoopBreaker(ReadOnlySpan<ulong> containerLeft, ReadOnlySpan<ulong> containerRight)
        {
            var cLength = LastNonZeroIndexZeroBased(containerLeft);
            if (cLength == 0) return true;
            for (var i = ContainerCapacity; i > 0; i--)
                if (containerLeft[i - 1] != containerRight[i - 1])
                    return false;
            return true;
        }
    }
    
    /// <summary>
    /// Decrypts or encrypts <paramref name="inputDataAsVectors"/>. 
    /// </summary>
    /// <param name="inputDataAsVectors"></param>
    /// <param name="roundKeys"></param>
    /// <returns>Modified <paramref name="inputDataAsVectors"/></returns>
    private static void Deencrypt(Span<Vector128<byte>> inputDataAsVectors, ReadOnlySpan<Vector128<byte>> roundKeys)
    {
        var key = roundKeys[^1];
        for (var i = 0; i < inputDataAsVectors.Length; i++)
        {
            key = Sse2.Xor(key, roundKeys[0]);
            for (var y = 1; y < EncSteps; y++)
                key = Aes.Encrypt(key, roundKeys[y]);
            key = Aes.EncryptLast(key, roundKeys[EncSteps]);

            inputDataAsVectors[i] = Sse2.Xor(inputDataAsVectors[i], key);
        }
    }

    /// <summary>
    /// Calculates checksum of a Lime DataSegment
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    private void LimeChecksum(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        
        var laps = (containerB.Length * sizeof(ulong) - 137) / 136 + 1;
        for (var i = 0; i < laps; i++)
        {
            for (var y = 0; y < 17; y++)
                containerA[y] ^= containerB[i * 17 + y];

            Limerousel(containerA);
        }

        containerA[0] ^= containerB[^2];
        containerA[1] ^= containerB[^1];
        containerA[2] ^= 6;
        containerA[16] ^= 0x8000000000000000;

        Limerousel(containerA);
    }

    /// <summary>
    /// You spin me right 'round, baby, right 'round.
    /// </summary>
    /// <param name="containerA"></param>
    private void Limerousel(Span<ulong> containerA)
    {
        const int laps = 5;
        Span<ulong> localContainer = stackalloc ulong[laps];
        for (var x = 0; x < containerA.Length - 1; x++)
        {
            // reset localContainer
            localContainer.Clear();

            // first ride
            for (var i = 0; i < laps; i++)
                for (var y = 0; y < laps; y++)
                    localContainer[i] ^= containerA[y * laps + i];

            // second ride
            for (var i = 0; i < laps; i++)
            {
                var left = ulong.RotateLeft(localContainer[(i + 1) % laps], 1);
                var right = localContainer[(i + laps - 1) % laps];
                for (var y = 0; y < laps; y++)
                    containerA[y * laps + i] ^= left ^ right;
            }

            // third ride
            var item = containerA[1];
            for (var i = 0; i < _checksumTable1.Length; i++)
            {
                var rotatedItem = ulong.RotateLeft(item, _checksumTable1[i]);
                item = containerA[_checksumTable2[i]];
                containerA[_checksumTable2[i]] = rotatedItem;
            }

            // fourth ride
            for (var i = 0; i < laps; i++)
            {
                var elementA = containerA[i * laps];
                var elementB = containerA[i * laps + 1];
                var elementC = containerA[i * laps + 2];
                var elementD = containerA[i * laps + 3];
                var elementE = containerA[i * laps + 4];
                containerA[i * laps] = elementA ^ elementC & ~elementB;
                containerA[i * laps + 1] = elementB ^ elementD & ~elementC;
                containerA[i * laps + 2] = elementC ^ elementE & ~elementD;
                containerA[i * laps + 3] = elementD ^ elementA & ~elementE;
                containerA[i * laps + 4] = elementE ^ elementB & ~elementA;
            }
            containerA[0] ^= _checksumTable3[x];
        }
    }

    public enum Mode
    {
        Encrypt,
        Decrypt
    }
    
    /// <summary>
    /// Lime domain.
    /// </summary>
    /// <param name="segments"></param>
    /// <param name="steamId"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public bool Limetree(Span<LimeDataSegment> segments, ulong steamId, Mode mode)
    {
        // load key into container
        Span<ulong> cKey1 = stackalloc ulong[ContainerCapacity];
        _privateKey1.CopyTo(cKey1);

        // load steamID into container
        Span<ulong> cSteamId = stackalloc ulong[ContainerCapacity];
        cSteamId[0] = NotSteamId(steamId);
        
        Span<ulong> cHashPublicKeysResult = stackalloc ulong[ContainerCapacity];
        Span<Vector128<byte>> aesRoundKeys = stackalloc Vector128<byte>[2 * AesBlockLength + 1];
        Span<ulong> checksumContainer = stackalloc ulong[ChecksumContainerCapacity];

        // calculate header and seed
        Span<ulong> cKeyType = stackalloc ulong[ContainerCapacity];
        Span<ulong> cHeader = stackalloc ulong[ContainerCapacity];
        Span<ulong> cLimeSeed = stackalloc ulong[ContainerCapacity];
        Span<ulong> cHashedKeyPart = stackalloc ulong[ContainerCapacity];
        Span<ulong> cRandomizer = stackalloc ulong[ContainerCapacity];
        var cRandomizerAsBytes = MemoryMarshal.Cast<ulong, byte>(cRandomizer);
        if (mode == Mode.Encrypt)
        {
            // load key type into container
            cKeyType[0] = KeyType;

            // calculate header
            _privateKey2.CopyTo(cHeader);
            Limegator(cHeader, cKeyType, cKey1);

            // calculate seed
            _privateKey2.CopyTo(cLimeSeed);
            Limegator(cLimeSeed, cSteamId, cKey1);
            Limegator(cLimeSeed, cKeyType, cKey1);
        }

        for (var i = 0; i < segments.Length; i++)
        {
            Span<byte> dataAsBytes = segments[i].SegmentData;
            var dataAsUlongs = MemoryMarshal.Cast<byte, ulong>(dataAsBytes);
            
            if (mode == Mode.Encrypt)
            {
                for (var j = 0; j < segments[i].HashedKeyBanks.Length; j++)
                {
                    // calculate hashed key
                    cLimeSeed.CopyTo(cHashedKeyPart);
                    RandomizeSpan(cRandomizerAsBytes,sizeof(ulong));
                    EncryptionFirst(cHashedKeyPart, cRandomizer);
                    // update header and hashed key
                    segments[i].HashedKeyBanks[j].Header = cHeader[..segments[i].HashedKeyBanks[j].Header.Length].ToArray();
                    segments[i].HashedKeyBanks[j].KeyFragment = cHashedKeyPart[..segments[i].HashedKeyBanks[j].KeyFragment.Length].ToArray();
                }

                // calculate and set a checksum of current segment
                checksumContainer.Clear();
                LimeChecksum(checksumContainer, dataAsUlongs);
                segments[i].SetSegmentChecksum(checksumContainer);
            }

            // hash public keys
            HashPublicKeys(cHashPublicKeysResult, cKey1, cSteamId, segments[i].HashedKeyBanks);

            // create round keys from public keys
            aesRoundKeys.Clear();
            var cHashPublicKeysResultAsVector128Span = MemoryMarshal.Cast<ulong, Vector128<byte>>(cHashPublicKeysResult);
            AesKeygen(aesRoundKeys, cHashPublicKeysResultAsVector128Span);

            // deencrypt SegmentData
            var dataAsVectors128 = MemoryMarshal.Cast<byte, Vector128<byte>>(dataAsBytes);
            Deencrypt(dataAsVectors128, aesRoundKeys);

            // compare a newly calculated checksum with the old one on the first segment and break loop if not equal
            if (mode == Mode.Decrypt && i == 0)
            {
                LimeChecksum(checksumContainer, dataAsUlongs);
                if (!segments[i].ValidateSegmentChecksum(checksumContainer)) return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Very slow, but effective. This is an ultimate act of desperation.
    /// </summary>
    /// <param name="limeSegment"></param>
    /// <param name="steamId"></param>
    public bool LimepickSegment(LimeDataSegment limeSegment, ulong steamId)
    {
        // load key into container
        Span<ulong> cKey1 = stackalloc ulong[ContainerCapacity];
        _privateKey1.CopyTo(cKey1);

        // load steamID into container
        Span<ulong> cSteamId = stackalloc ulong[ContainerCapacity];
        cSteamId[0] = NotSteamId(steamId);

        // load segment data
        Span<byte> dataAsBytes = stackalloc byte[limeSegment.SegmentData.Length];
        limeSegment.SegmentData.CopyTo(dataAsBytes);

        Span<ulong> cHashPublicKeysResult = stackalloc ulong[ContainerCapacity];
        Span<Vector128<byte>> aesRoundKeys = stackalloc Vector128<byte>[2 * AesBlockLength + 1];

        // hash public keys
        HashPublicKeys(cHashPublicKeysResult, cKey1, cSteamId, limeSegment.HashedKeyBanks);
        // create round keys from public keys
        var cHashPublicKeysResultAsVector128Span = MemoryMarshal.Cast<ulong, Vector128<byte>>(cHashPublicKeysResult);
        AesKeygen(aesRoundKeys, cHashPublicKeysResultAsVector128Span);
        // deencrypt SegmentData
        var dataAsVectors128 = MemoryMarshal.Cast<byte, Vector128<byte>>(dataAsBytes);
        Deencrypt(dataAsVectors128, aesRoundKeys);

        Span<ulong> checksumContainer = stackalloc ulong[ChecksumContainerCapacity];
        var dataAsUlongs = MemoryMarshal.Cast<byte, ulong>(dataAsBytes);
        LimeChecksum(checksumContainer, dataAsUlongs);
        
        return limeSegment.ValidateSegmentChecksum(checksumContainer);
    }
    
    #endregion

}