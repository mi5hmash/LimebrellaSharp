using LimebrellaSharpCore.Models.DSSS.Lime;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace LimebrellaSharpCore.Helpers;

public class LimeDeencryptor
{
    #region CONSTANTS

    private const ulong BitMaskPattern = 0x8000000000000000;
    private const ulong KeyType = 20;
    private const int EncSteps = 10;
    private const int AesBlockLength = EncSteps + 1;
    private const int CLengthMax = 34;
    private const int ChecksumContainerLength = 25;

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
            .Base64DecodeUtf8().ToBytes();
        _checksumTable2 = "MEEwNzBCMTExMjAzMDUxMDA4MTUxODA0MEYxNzEzMEQwQzAyMTQwRTE2MDkwNjAx"
            .Base64DecodeUtf8().ToBytes();
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
    public static void RandomizeSpan(ref Span<byte> span, int length = 0, int startPos = 0)
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
    /// Checks how many elements are in the queue.
    /// </summary>
    /// <param name="queue"></param>
    /// <returns></returns>
    private static int QueueLength(ReadOnlySpan<ulong> queue)
    {
        for (var i = queue.Length; i > 0; i--) if (queue[i - 1] != 0) return i;
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

    #endregion

    #region ENCRYPTION METHODS

    /// <summary>
    /// First type of encryption.
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    private static void EncryptionFirst(ref Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        if (QueueLength(containerA) == 0 || QueueLength(containerB) == 0) return;

        var cLengthA = QueueLength(containerA);
        var cLengthB = QueueLength(containerB);

        Span<ulong> localContainer = stackalloc ulong[CLengthMax];
        
        if (cLengthA > 0)
        {
            for (var y = 0; y < cLengthA; y++)
            {
                ulong salt = 0;
                if (cLengthB > 0)
                {
                    var firstPart = containerA[y];
                    for (var i = 0; i < cLengthB; i++)
                    {
                        var lowBytes = MulLow(firstPart, containerB[i]);
                        var highBytes = MulHigh(firstPart, containerB[i]);
                        var basis = lowBytes + salt;
                        if (basis < salt) highBytes++;
                        var result = basis + localContainer[y + i];
                        if (result < basis) highBytes++;
                        localContainer[y + i] = result;
                        salt = highBytes;
                    }
                }
                localContainer[cLengthB + y] = salt;
            }
        }
        // Update referenced containerA
        localContainer.CopyTo(containerA);
    }

    /// <summary>
    /// Second type of encryption.
    /// </summary>
    /// <param name="dataContainer"></param>
    /// <param name="bits"></param>
    private static void EncryptionSecond(ref Span<ulong> dataContainer, int bits)
    {
        var division = bits >> 6; // division by 64
        var reminder = bits & 0x3F; // division reminder

        if (reminder != 0)
        {
            if (division != CLengthMax && division != CLengthMax - 1)
            {
                var curElement = CLengthMax;
                for (var i = CLengthMax - division; i > 1; i--, curElement--)
                {
                    dataContainer[curElement - 1] = dataContainer[i - 1] << reminder;
                    dataContainer[curElement - 1] |= dataContainer[i - 2] >> (0x40 - reminder);
                }
            }
            dataContainer[division] = dataContainer[0] << reminder;
        }
        else if (division != 0)
        {
            var curElement = CLengthMax - division;
            if (division != CLengthMax)
            {
                for (var i = CLengthMax; curElement > 0; i--, curElement--)
                    dataContainer[i - 1] = dataContainer[curElement - 1];
            }
        }

        if (division == 0) return;
        for (var i = division; i > 0; i--) dataContainer[division - i] = 0;
    }

    /// <summary>
    /// Third type of encryption (subtract variant).
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    /// <returns>Modifies <paramref name="containerA"/></returns>
    private static void EncryptionThirdSub(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        byte testA = 0;
        byte testB = 0;
        for (var i = 0; i < CLengthMax; i++)
        {
            var test0 = Convert.ToByte(testA | testB);
            var newValue = containerA[i] - containerB[i] - test0;
            testA = containerA[i] == newValue ? test0 : (byte)0;
            testB = Convert.ToByte(containerA[i] < newValue);
            containerA[i] = newValue;
        }
    }

    /// <summary>
    /// Third type of encryption (addition variant).
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    /// <returns>Modifies <paramref name="containerA"/></returns>
    private static void EncryptionThirdAdd(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        byte testA = 0;
        byte testB = 0;
        for (var i = 0; i < CLengthMax; i++)
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
    private void HashPublicKeys(ref Span<ulong> segmentHashedKey, ReadOnlySpan<ulong> cKey1, ReadOnlySpan<ulong> cSteamId, ReadOnlySpan<DsssLimeHashedKeyBank> limeBank)
    {
        Span<ulong> localContainerA = stackalloc ulong[CLengthMax];
        Span<ulong> localContainerB = stackalloc ulong[CLengthMax];
        for (var i = 0; i < limeBank.Length; i++)
        {
            limeBank[i].Header.CopyTo(localContainerA);
            Limegator(localContainerA, cSteamId, cKey1);
            limeBank[i].HashedKey.CopyTo(localContainerB);
            Limeghetti(ref localContainerB, localContainerA);
            // put the calculated part of the key in the segmentHashedKey
            segmentHashedKey[i] = localContainerB[0];
        }
    }

    /// <summary>
    /// Generates the Encryption Key.
    /// </summary>
    /// <param name="aesRoundKeys"></param>
    /// <param name="inputKey"></param>
    private static void AesKeygen(ref Span<Vector128<byte>> aesRoundKeys, ReadOnlySpan<Vector128<byte>> inputKey)
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
    private void Limeghetti(ref Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        // check for empty containers
        var cLengthA = QueueLength(containerA);
        if (cLengthA == 0) return;
        var cLengthB = QueueLength(containerB);
        if (cLengthB == 0) goto ORDER_66;
        
        // check if the last bit of localContainerA is not 0
        if ((containerA[^1] & BitMaskPattern) != 0)
        {
            // check if the last bit of localContainerB is 0
            if ((containerB[^1] & BitMaskPattern) == 0) goto ORDER_66;
        }

        // check if the last bit of localContainerB is 0
        if ((containerB[^1] & BitMaskPattern) == 0)
        {
            for (var i = CLengthMax; i > 0; i--)
            {
                // check if nth element of localContainerA is less than localContainerB
                if (containerA[i - 1] < containerB[i - 1]) goto ORDER_66;
                // check if nth element of localContainerA is greater than localContainerB
                if (containerA[i - 1] > containerB[i - 1]) break;
            }
        }

        // calculate how many ints are in a localContainerB
        var tinyHashesToCalculate = (cLengthA - cLengthB) * 2;

        var bits = 32 - ((RootDegree(containerB[cLengthB - 1], 1) + ((cLengthB - 1) << 6)) & 31);

        // create a localContainerB
        Span<ulong> localContainerB = stackalloc ulong[CLengthMax];
        containerB.CopyTo(localContainerB);

        // perform EncryptionSecond on both containers
        EncryptionSecond(ref containerA, bits);
        EncryptionSecond(ref localContainerB, bits);

        // recalculate containers' lengths
        cLengthA = QueueLength(containerA);
        cLengthB = QueueLength(localContainerB);

        // create containers
        Span<ulong> localContainerC = stackalloc ulong[CLengthMax];
        Span<ulong> localContainerD = stackalloc ulong[CLengthMax];
        Span<ulong> localContainerE = stackalloc ulong[CLengthMax];
        Span<ulong> resultContainer = stackalloc ulong[CLengthMax];

        var currentSegment = 0;
        var lastQueueElemB = localContainerB[cLengthB - 1] >> 32;
        do
        {
            var isEven = tinyHashesToCalculate % 2 == 0;
            // increase currentSegment by 1 when the tinyHashesToCalculate is divisible by 2
            if (isEven) currentSegment++;

            // set lastQueueElemA and calculate lastQueueElemDiv
            var lastQueueElemA = isEven
                ? containerA[cLengthA - currentSegment]
                : (containerA[cLengthA - currentSegment] << 32) | (containerA[cLengthA - currentSegment - 1] >> 32);
            var lastQueueElemDiv = lastQueueElemA / lastQueueElemB;

            // create a snapshot of current localContainerB
            localContainerB.CopyTo(localContainerC);

            // create a new container with lastQueueElemDiv at index 0
            localContainerD.Clear();
            localContainerD[0] = lastQueueElemDiv;

            // perform EncryptionSecond and EncryptionFirst on localContainerC
            EncryptionSecond(ref localContainerC, (tinyHashesToCalculate - 1) * 32);
            EncryptionFirst(ref localContainerC, localContainerD);

            for (var i = CLengthMax; i > 0; i--)
            {
                // check if nth element of localContainerA is less than localContainerC
                if (containerA[i - 1] < localContainerC[i - 1])
                {
                    // reuse localContainerD as a snapshot of current localContainerB 
                    localContainerB.CopyTo(localContainerD);
                    EncryptionSecond(ref localContainerD, (tinyHashesToCalculate - 1) * 32);
                    EncryptionThirdSub(localContainerC, localContainerD);
                    // decrease lastQueueElemDiv by 1
                    lastQueueElemDiv -= 1;
                    // reset loop
                    i = CLengthMax;
                }
                // check if nth element of localContainerA is greater than localContainerC
                if (containerA[i - 1] > localContainerC[i - 1]) break;
            }

            // clear localContainerD
            localContainerD.Clear();
            // clear localContainerE and put lastQueueElemDiv at index 0
            localContainerE.Clear();
            localContainerE[0] = lastQueueElemDiv;
            EncryptionThirdAdd(localContainerE, localContainerD);
            EncryptionSecond(ref localContainerE, (tinyHashesToCalculate - 1) * 32);
            EncryptionThirdAdd(resultContainer, localContainerE);
            EncryptionThirdSub(containerA, localContainerC);
            tinyHashesToCalculate--;
        } while (tinyHashesToCalculate > 0);

        // Padawans survived
        resultContainer.CopyTo(containerA);
        return;

    // Execute...
    ORDER_66:
        // set all the containerA ulongs to 0
        containerA.Clear();
    }

    /// <summary>
    /// Watch out for its sharp teeth!
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    /// <param name="containerC"></param>
    /// <returns>Modifies <paramref name="containerA"/></returns>
    private void Limegator(Span<ulong> containerA, ReadOnlySpan<ulong> containerB, ReadOnlySpan<ulong> containerC)
    {
        Span<ulong> localContainerA = stackalloc ulong[CLengthMax];
        localContainerA[0] = 1;

        Span<ulong> localContainerB = stackalloc ulong[CLengthMax];
        containerB.CopyTo(localContainerB);

        Span<ulong> localContainerC = stackalloc ulong[CLengthMax];
        Span<ulong> localContainerD = stackalloc ulong[CLengthMax];

        while (true)
        {
            localContainerC.Clear();
            if (LoopBreaker(localContainerB, localContainerC)) break;

            if ((localContainerB[0] & 1) != 0)
            {
                localContainerA.CopyTo(localContainerC);
                EncryptionFirst(ref localContainerC, containerA);
                localContainerC.CopyTo(localContainerD);
                Limeghetti(ref localContainerD, containerC);
                EncryptionFirst(ref localContainerD, containerC);
                EncryptionThirdSub(localContainerC, localContainerD);
                localContainerC.CopyTo(localContainerA);
            }
            containerA.CopyTo(localContainerC);
            containerA.CopyTo(localContainerD);
            EncryptionFirst(ref localContainerC, localContainerD);
            localContainerC.CopyTo(localContainerD);
            Limeghetti(ref localContainerD, containerC);
            EncryptionFirst(ref localContainerD, containerC);
            EncryptionThirdSub(localContainerC, localContainerD);
            localContainerC.CopyTo(containerA);
            
            for (var i = 0; i < CLengthMax - 1; i++)
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
            var cLength = QueueLength(containerLeft);
            if (cLength == 0) return true;
            for (var i = CLengthMax; i > 0; i--)
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
    /// <returns>Modifies <paramref name="inputDataAsVectors"/></returns>
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
    private void LimeChecksum(ref Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        
        var laps = (containerB.Length * sizeof(ulong) - 137) / 136 + 1;
        for (var i = 0; i < laps; i++)
        {
            for (var y = 0; y < 17; y++)
                containerA[y] ^= containerB[i * 17 + y];

            Limerousel(ref containerA);
        }

        containerA[0] ^= containerB[^2];
        containerA[1] ^= containerB[^1];
        containerA[2] ^= 6;
        containerA[16] ^= BitMaskPattern;

        Limerousel(ref containerA);
    }

    /// <summary>
    /// You spin me right 'round, baby, right 'round.
    /// </summary>
    /// <param name="containerA"></param>
    private void Limerousel(ref Span<ulong> containerA)
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
    public bool Limetree(Span<DsssLimeDataSegment> segments, ulong steamId, Mode mode)
    {
        // load key into container
        Span<ulong> cKey1 = stackalloc ulong[CLengthMax];
        _privateKey1.CopyTo(cKey1);

        // load steamID into container
        Span<ulong> cSteamId = stackalloc ulong[CLengthMax];
        cSteamId[0] = NotSteamId(steamId);
        
        Span<ulong> cHashPublicKeysResult = stackalloc ulong[CLengthMax];
        Span<Vector128<byte>> aesRoundKeys = stackalloc Vector128<byte>[2 * AesBlockLength + 1];
        Span<ulong> checksumContainer = stackalloc ulong[ChecksumContainerLength];

        // calculate header and seed
        Span<ulong> cKeyType = stackalloc ulong[CLengthMax];
        Span<ulong> cHeader = stackalloc ulong[CLengthMax];
        Span<ulong> cLimeSeed = stackalloc ulong[CLengthMax];
        Span<ulong> cHashedKeyPart = stackalloc ulong[CLengthMax];
        Span<ulong> cRandomizer = stackalloc ulong[CLengthMax];
        var cRandomizerAsBytes = MemoryMarshal.Cast<ulong, byte>(cRandomizer[..(cRandomizer.Length / sizeof(byte) * sizeof(byte))]);
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
            var dataAsUlongs = MemoryMarshal.Cast<byte, ulong>(dataAsBytes[..(dataAsBytes.Length / sizeof(ulong) * sizeof(ulong))]);
            
            if (mode == Mode.Encrypt)
            {
                for (var j = 0; j < segments[i].HashedKeyBanks.Length; j++)
                {
                    // calculate hashed key
                    cLimeSeed.CopyTo(cHashedKeyPart);
                    RandomizeSpan(ref cRandomizerAsBytes,sizeof(ulong));
                    EncryptionFirst(ref cHashedKeyPart, cRandomizer);
                    // update header and hashed key
                    segments[i].HashedKeyBanks[j].Header = cHeader[..segments[i].HashedKeyBanks[j].Header.Length].ToArray();
                    segments[i].HashedKeyBanks[j].HashedKey = cHashedKeyPart[..segments[i].HashedKeyBanks[j].HashedKey.Length].ToArray();
                }

                // calculate and set a checksum of current segment
                checksumContainer.Clear();
                LimeChecksum(ref checksumContainer, dataAsUlongs);
                segments[i].SetSegmentChecksum(checksumContainer);
            }

            // hash public keys
            HashPublicKeys(ref cHashPublicKeysResult, cKey1, cSteamId, segments[i].HashedKeyBanks);

            // create round keys from public keys
            aesRoundKeys.Clear();
            var cHashPublicKeysResultAsVector128Span = MemoryMarshal.Cast<ulong, Vector128<byte>>(cHashPublicKeysResult[..(cHashPublicKeysResult.Length / Vector128<byte>.Count * Vector128<byte>.Count)]);
            AesKeygen(ref aesRoundKeys, cHashPublicKeysResultAsVector128Span);

            // deencrypt SegmentData
            var dataAsVectors128 = MemoryMarshal.Cast<byte, Vector128<byte>>(dataAsBytes[..(dataAsBytes.Length / Vector128<byte>.Count * Vector128<byte>.Count)]);
            Deencrypt(dataAsVectors128, aesRoundKeys);

            // compare a newly calculated checksum with the old one on the first segment and break loop if not equal
            if (mode == Mode.Decrypt && i == 0)
            {
                LimeChecksum(ref checksumContainer, dataAsUlongs);
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
    public bool LimepickSegment(DsssLimeDataSegment limeSegment, ulong steamId)
    {
        // load key into container
        Span<ulong> cKey1 = stackalloc ulong[CLengthMax];
        _privateKey1.CopyTo(cKey1);

        // load steamID into container
        Span<ulong> cSteamId = stackalloc ulong[CLengthMax];
        cSteamId[0] = NotSteamId(steamId);

        // load segment data
        Span<byte> dataAsBytes = stackalloc byte[limeSegment.SegmentData.Length];
        limeSegment.SegmentData.CopyTo(dataAsBytes);

        Span<ulong> cHashPublicKeysResult = stackalloc ulong[CLengthMax];
        Span<Vector128<byte>> aesRoundKeys = stackalloc Vector128<byte>[2 * AesBlockLength + 1];

        // hash public keys
        HashPublicKeys(ref cHashPublicKeysResult, cKey1, cSteamId, limeSegment.HashedKeyBanks);
        // create round keys from public keys
        var cHashPublicKeysResultAsVector128Span = MemoryMarshal.Cast<ulong, Vector128<byte>>(cHashPublicKeysResult[..(cHashPublicKeysResult.Length / Vector128<byte>.Count * Vector128<byte>.Count)]);
        AesKeygen(ref aesRoundKeys, cHashPublicKeysResultAsVector128Span);
        // deencrypt SegmentData
        var dataAsVectors128 = MemoryMarshal.Cast<byte, Vector128<byte>>(dataAsBytes[..(dataAsBytes.Length / Vector128<byte>.Count * Vector128<byte>.Count)]);
        Deencrypt(dataAsVectors128, aesRoundKeys);

        Span<ulong> checksumContainer = stackalloc ulong[ChecksumContainerLength];
        var dataAsUlongs = MemoryMarshal.Cast<byte, ulong>(dataAsBytes[..(dataAsBytes.Length / sizeof(ulong) * sizeof(ulong))]);
        LimeChecksum(ref checksumContainer, dataAsUlongs);
        
        return limeSegment.ValidateSegmentChecksum(checksumContainer);
    }
    
    #endregion

}