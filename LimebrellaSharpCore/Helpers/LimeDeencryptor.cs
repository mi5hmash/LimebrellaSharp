using System.Runtime.InteropServices;
using LimebrellaSharpCore.Models.DSSS.Lime;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using Aes = System.Runtime.Intrinsics.X86.Aes;
using AesNative = System.Security.Cryptography.Aes;

namespace LimebrellaSharpCore.Helpers;

public class LimeDeencryptor
{
    #region CONSTANTS

    private const byte ContainerCapacityInUlongs = 0x20;
    private const byte ChecksumContainerCapacityInBytes = 0x19;
    private const byte KeyType = 0x14;
    
    private readonly ulong[] _privateKey1;
    private readonly ulong[] _privateKey2;
    private readonly ulong[] _privateKey3;
    private readonly byte[] _limerouselRotationsTable;
    private readonly byte[] _limerouselPositionsTable;
    private readonly ulong[] _limerouselXorsTable;
    private readonly byte[] _headerKey;

    #endregion

    /// <summary>
    /// Default Constructor that loads configuration.
    /// </summary>
    public LimeDeencryptor()
    {
        _privateKey1 = "RjMzQjZGQjk3MkEwQjcyNTE1RTQ1QzM5MTgyOUUxODJBRDhBOUJEQzBBNjREMzQ0NEQ3OUM4MTBBQjg2MzcxNw=="
            .Base64DecodeUtf8().ToUlongArray();
        _privateKey2 = "Rjk5REI3NUMzOUQwREI5MjBBNzJBRTFDOEM5NDcwQzE1NkM1NEQ2RTA1QjI2OUEyQTYzQzY0ODg1NUMzOUIwQg=="
            .Base64DecodeUtf8().ToUlongArray();
        _privateKey3 = "RTY2RjU0NEFGQ0NFNjhDNUVGMDdCOUEwN0IyNzc1ODUzNDRBMURCNjEzNzZFODMxRjczQjlGQkQ1RjQ0RjcxNQ=="
            .Base64DecodeUtf8().ToUlongArray();
        _limerouselRotationsTable = "MDEwMzA2MEEwRjE1MUMyNDJEMzcwMjBFMUIyOTM4MDgxOTJCM0UxMjI3M0QxNDJD"
            .Base64DecodeUtf8().ToByteArray();
        _limerouselPositionsTable = "MEEwNzBCMTExMjAzMDUxMDA4MTUxODA0MEYxNzEzMEQwQzAyMTQwRTE2MDkwNjAx"
            .Base64DecodeUtf8().ToByteArray();
        _limerouselXorsTable = "MDEwMDAwMDAwMDAwMDAwMDgyODAwMDAwMDAwMDAwMDA4QTgwMDAwMDAwMDAwMDgwMDA4MDAwODAwMDAwMDA4MDhCODAwMDAwMDAwMDAwMDAwMTAwMDA4MDAwMDAwMDAwODE4MDAwODAwMDAwMDA4MDA5ODAwMDAwMDAwMDAwODA4QTAwMDAwMDAwMDAwMDAwODgwMDAwMDAwMDAwMDAwMDA5ODAwMDgwMDAwMDAwMDAwQTAwMDA4MDAwMDAwMDAwOEI4MDAwODAwMDAwMDAwMDhCMDAwMDAwMDAwMDAwODA4OTgwMDAwMDAwMDAwMDgwMDM4MDAwMDAwMDAwMDA4MDAyODAwMDAwMDAwMDAwODA4MDAwMDAwMDAwMDAwMDgwMEE4MDAwMDAwMDAwMDAwMDBBMDAwMDgwMDAwMDAwODA4MTgwMDA4MDAwMDAwMDgwODA4MDAwMDAwMDAwMDA4MDAxMDAwMDgwMDAwMDAwMDAwODgwMDA4MDAwMDAwMDgwMDEwMzA2MEEwRjE1MUMyNA=="
            .Base64DecodeUtf8().ToUlongArray();
        _headerKey = CreateKeyFragmentHeader();
    }

    #region HELPER FUNCS

    /// <summary>
    /// Multiplies two ulong values.
    /// Function based on: https://gist.github.com/cocowalla/6070a53445e872f2bb24304712a3e1d2.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns>High-order ulong value</returns>
    private static ulong MulHigh(ulong left, ulong right)
    {
        const byte shift = 0x20;

        ulong l0 = (uint)left;
        var l1 = left >> shift;
        ulong r0 = (uint)right;
        var r1 = right >> shift;

        var p11 = l1 * r1;
        var p01 = l0 * r1;
        var p10 = l1 * r0;
        var p00 = l0 * r0;

        // 64-bit product + two 32-bit values
        var middle = p10 + (p00 >> shift) + (uint)p01;

        // 64-bit product + two 32-bit values
        return p11 + (middle >> shift) + (p01 >> shift);
    }

    /// <summary>
    /// Multiplies two ulong values.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns>Low-order ulong value.</returns>
    private static ulong MulLow(ulong left, ulong right)
        => left * right;

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
    /// Checks if the Most Significant Bit is set.
    /// </summary>
    /// <param name="number"></param>
    /// <returns>Returns <c>true</c> if the Most Significant Bit is set</returns>
    private static bool IsMostSignificantBitSet(ulong number)
        => (number & 0x8000000000000000) != 0;
    private static bool IsMostSignificantBitSet(uint number)
        => (number & 0x80000000) != 0;

    /// <summary>
    /// Randomizes the range of bytes in a given span.
    /// </summary>
    /// <param name="span"></param>
    /// <param name="start"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static void RandomizeSpan(Span<byte> span, int start = 0)
    {
        if (start < 0 || start > span.Length)
            throw new ArgumentOutOfRangeException(nameof(start), "Start index is out of range for the span.");
        Random random = new();
        for (var i = start; i < span.Length; i++) span[i] = (byte)random.Next(byte.MaxValue + 1);
    }

    /// <summary>
    /// Performs NOT operation on provided UserID.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>Negated <paramref name="userId"/>.</returns>
    private static ulong NotUserId(ulong userId) => ~userId;

    /// <summary>
    /// Compares two containers in reverse lexicographical order to determine if <paramref name="containerA"/> is less than <paramref name="containerB"/>.
    /// Starting from the last element, it checks each element pair by pair until a difference is found or all elements have been compared.
    /// </summary>
    /// <param name="containerA">The first container to compare.</param>
    /// <param name="containerB">The second container to compare.</param>
    /// <returns>
    /// Returns <c>true</c> if <paramref name="containerA"/> is reverse-ordered less than <paramref name="containerB"/>; otherwise, returns <c>false</c>.
    /// </returns>
    private static bool IsReverseOrderedLess(ReadOnlySpan<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        for (var i = ContainerCapacityInUlongs; i > 0; i--)
        {
            // check if nth element of containerA is less than containerB
            if (containerA[i - 1] < containerB[i - 1]) return true;
            // check if nth element of containerA is greater than containerB
            if (containerA[i - 1] > containerB[i - 1]) return false;
        }
        return false;
    }

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
    /// Loads <paramref name="cargo"/> into a <paramref name="container"/> at given <paramref name="position"/> and clears the free space.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="container"></param>
    /// <param name="cargo"></param>
    /// <param name="position"></param>
    private static void SetupContainer<T>(Span<T> container, T cargo, int position = 0) where T : struct
    {
        container[position] = cargo;
        container[..position].Clear();
        container[(position + 1)..].Clear();
    }

    /// <summary>
    /// Loads <paramref name="cargo"/> into a <paramref name="container"/> at given <paramref name="position"/> and clears the free space.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="container"></param>
    /// <param name="cargo"></param>
    /// <param name="position"></param>
    private static void SetupContainer<T>(Span<T> container, ReadOnlySpan<T> cargo, int position = 0) where T : struct
    {
        cargo.CopyTo(container[position..]);
        container[..position].Clear();
        container[(position + cargo.Length)..].Clear();
    }

    /// <summary>
    /// Creates a Key Fragment Header.
    /// </summary>
    /// <returns></returns>
    private byte[] CreateKeyFragmentHeader()
        => MemoryMarshal.Cast<ulong, byte>(CreateKey(KeyType)).ToArray();

    #endregion
    
    #region ENCRYPTION METHODS

    /// <summary>
    /// Creates a Key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    private Span<ulong> CreateKey(ulong key, int length = 0)
    {
        // create localContainerA
        Span<ulong> localContainerA = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer(localContainerA, key);
        // create localContainerB
        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer(localContainerB, _privateKey2);
        // create localContainerC
        Span<ulong> localContainerC = stackalloc ulong[ContainerCapacityInUlongs];
        // execute a set of encryption methods
        SetOfEncryptionMethods(localContainerA, localContainerB, localContainerC);
        // prepare localContainerB
        SetupContainer(localContainerB, _privateKey3);
        // prepare localContainerC
        SetupContainer(localContainerC, _privateKey1);
        // calculate a key and return it
        Limegator(localContainerB, localContainerA, localContainerC);
        length = length == 0 ? LastNonZeroIndexZeroBased<ulong>(localContainerB) : length;
        return localContainerB[..length].ToArray();
    }

    /// <summary>
    /// Subtracts two containers from one another.
    /// </summary>
    /// <param name="containerA">The container from which a <paramref name="containerB"/> will be deducted.</param>
    /// <param name="containerB">The container which will be deducted from the <paramref name="containerA"/></param>
    /// <returns>Modifies <paramref name="containerA"/>.</returns>
    private static void SubtractContainers(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        byte testA = 0;
        byte testB = 0;
        for (var i = 0; i < ContainerCapacityInUlongs; i++)
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
    /// <param name="containerA">The first container to add.</param>
    /// <param name="containerB">The second container to add.</param>
    /// <returns>Modifies <paramref name="containerA"/>.</returns>
    private static void AddContainers(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        byte testA = 0;
        byte testB = 0;
        for (var i = 0; i < ContainerCapacityInUlongs; i++)
        {
            var test0 = Convert.ToByte(testA | testB);
            var newValue = containerA[i] + containerB[i] + test0;
            testA = containerB[i] == newValue ? test0 : (byte)0;
            testB = Convert.ToByte(newValue < containerB[i]);
            containerA[i] = newValue;
        }
    }

    /// <summary>
    /// Handles overflow in the provided <paramref name="container"/>.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="localContainer"></param>
    /// <returns>Modifies <paramref name="container"/>.</returns>
    private static void HandleOverflow(Span<ulong> container, Span<ulong> localContainer)
    {
        // create a localContainer
        SetupContainer(localContainer, (ulong)1);

        SubtractContainers(container, localContainer);
        const byte x = 0x2;
        for (var i = 0; i < ContainerCapacityInUlongs - x; i++)
            container[i] = ~container[i] & 0xFFFFFFFFFFFFFFFF;
        for (var i = ContainerCapacityInUlongs - x; i < ContainerCapacityInUlongs; i++)
            container[i] = ~container[i];
    }

    /// <summary>
    /// Decrypts or encrypts <paramref name="data"/> (uses intrinsic functions). 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns>Modified <paramref name="data"/></returns>
    private static void DeencryptIntrinsics(Span<Vector128<byte>> data, ReadOnlySpan<Vector128<byte>> key)
    {
        const byte rounds = 0xA;
        const int shift = 4;
        Span<Vector128<byte>> aesRoundKeys = stackalloc Vector128<byte>[rounds + 1];

        //// AES KEYGEN
        // build the first block (Expand AES-128 key)
        aesRoundKeys[0] = key[0];
        for (var i = 0; i < rounds; i++)
        {
            var innerRoundKey = i switch
            {
                // AES-128(128 - bit key): 10 rounds
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
                _ => Aes.KeygenAssist(aesRoundKeys[i], 0x8D)
            };
            // shift xmm2 left by 4 bytes
            var shift1 = Sse2.ShiftLeftLogical128BitLane(aesRoundKeys[i].AsUInt32(), shift).AsInt32();
            // shift shift1 left by 4 bytes
            var shift2 = Sse2.ShiftLeftLogical128BitLane(shift1.AsUInt32(), shift).AsInt32();
            // shift shift2 left by 4 bytes
            var shift3 = Sse2.ShiftLeftLogical128BitLane(shift2, shift);
            // compute the final result using shuffle and XOR instructions
            var shuffle1 = Sse2.Shuffle(innerRoundKey.AsInt32(), 255);
            var xor1 = Sse2.Xor(shift1, aesRoundKeys[i].AsInt32());
            var xor2 = Sse2.Xor(shift2, xor1);
            var xor3 = Sse2.Xor(xor2, shift3);
            var xor4 = Sse2.Xor(xor3, shuffle1);
            // add key to the aesRoundKeys
            aesRoundKeys[i + 1] = xor4.AsByte();
        }

        //// AES ENCRYPT
        var state = key[^1];
        for (var i = 0; i < data.Length; i++)
        {
            state = Sse2.Xor(state, aesRoundKeys[0]);
            for (var y = 1; y < rounds; y++)
                state = Aes.Encrypt(state, aesRoundKeys[y]);
            state = Aes.EncryptLast(state, aesRoundKeys[rounds]);
            // Decrypt row of input data
            data[i] = Sse2.Xor(data[i], state);
        }
    }

    /// <summary>
    /// Decrypts or encrypts <paramref name="data"/> (software-based).
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <param name="state"></param>
    private static void AesDeencryptSoftwareBased(Span<byte> data, byte[] key, byte[] state)
    {
        using var aes = AesNative.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.PKCS7;
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var buffer = new byte[16];
        for (var i = 0; i < data.Length; i += 16)
        {
            using (var ms = new MemoryStream())
            {
                using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                cs.Write(state);
                cs.FlushFinalBlock();
                Buffer.BlockCopy(ms.ToArray(), 0, buffer, 0, buffer.Length);
                Buffer.BlockCopy(buffer, 0, state, 0, state.Length);
            }
            // Decrypt row of input data
            for (var j = 0; j < buffer.Length; j++)
                data[i + j] ^= buffer[j];
        }
    }

    /// <summary>
    /// Decrypts or encrypts <paramref name="inputData"/>.
    /// </summary>
    /// <param name="inputData"></param>
    /// <param name="encryptionKey"></param>
    /// /// <returns>Modified <paramref name="inputData"/></returns>
    private static void DeencryptData(Span<byte> inputData, ReadOnlySpan<byte> encryptionKey)
    {
        // if intrinsics are not supported then use the software-based functions
        if (Aes.IsSupported && Sse2.IsSupported)
        {
            var dataAsVectors = MemoryMarshal.Cast<byte, Vector128<byte>>(inputData);
            var encryptionKeyAsVectors = MemoryMarshal.Cast<byte, Vector128<byte>>(encryptionKey);
            DeencryptIntrinsics(dataAsVectors, encryptionKeyAsVectors);
            return;
        }
        var key = encryptionKey[..16].ToArray();
        var state = encryptionKey[16..].ToArray();
        AesDeencryptSoftwareBased(inputData, key, state);
    }

    /// <summary>
    /// First type of encryption.
    /// </summary>
    /// <param name="containerA">The first container to use.</param>
    /// <param name="containerB">The second container to use.</param>
    /// <returns>Modifies <paramref name="containerA"/>.</returns>
    private static void EncryptionFirst(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        // check for empty containers
        var containerALength = LastNonZeroIndexZeroBased<ulong>(containerA);
        if (containerALength == 0) return;
        var containerBLength = LastNonZeroIndexZeroBased(containerB);
        if (containerBLength == 0) goto ORDER_66;

        // create a localContainerB
        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacityInUlongs];
        containerB.CopyTo(localContainerB);

        // create a resultContainer
        Span<ulong> resultContainer = stackalloc ulong[ContainerCapacityInUlongs];

        // detect overflow in...
        var overflowSwitch = false;
        // ... localContainerA
        if (IsMostSignificantBitSet(containerA[^1]))
        {
            overflowSwitch ^= true;
            HandleOverflow(containerA, resultContainer);
            // re-check container length
            containerALength = LastNonZeroIndexZeroBased<ulong>(containerA);
        }

        // ... localContainerB
        if (IsMostSignificantBitSet(localContainerB[^1]))
        {
            overflowSwitch ^= true;
            HandleOverflow(localContainerB, resultContainer);
            // re-check container length
            containerBLength = LastNonZeroIndexZeroBased<ulong>(localContainerB);
            if (containerBLength == 0) goto OVERFLOWN_ENDING;
        }

        // prepare resultContainer
        resultContainer.Clear();

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
    /// <returns>Modifies <paramref name="dataContainer"/>.</returns>
    private static void EncryptionSecond(Span<ulong> dataContainer, int bits)
    {
        var division = bits >> 6; // division by 64
        var reminder = bits & 0x3F; // division reminder

        if (reminder != 0)
        {
            if (division != ContainerCapacityInUlongs && division != ContainerCapacityInUlongs - 1)
            {
                var curElement = ContainerCapacityInUlongs;
                for (var i = ContainerCapacityInUlongs - division; i > 1; i--, curElement--)
                {
                    dataContainer[curElement - 1] = dataContainer[i - 1] << reminder;
                    dataContainer[curElement - 1] |= dataContainer[i - 2] >> (0x40 - reminder);
                }
            }
            dataContainer[division] = dataContainer[0] << reminder;
        }
        else if (division != 0)
        {
            var curElement = ContainerCapacityInUlongs - division;
            if (division != ContainerCapacityInUlongs)
            {
                for (var i = ContainerCapacityInUlongs; curElement > 0; i--, curElement--)
                    dataContainer[i - 1] = dataContainer[curElement - 1];
            }
        }

        if (division == 0) return;
        for (var i = division; i > 0; i--) dataContainer[division - i] = 0;
    }

    /// <summary>
    /// Prepare a delicious knot of Limeghetti.
    /// </summary>
    /// <param name="containerA">The first container to use.</param>
    /// <param name="containerB">The second container to use.</param>
    /// <returns>Modifies <paramref name="containerA"/>.</returns>
    private static void Limeghetti(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        // check for empty containers
        var containerALength = LastNonZeroIndexZeroBased<ulong>(containerA);
        if (containerALength == 0) return;
        var containerBLength = LastNonZeroIndexZeroBased(containerB);
        if (containerBLength == 0) goto ORDER_66;

        // create a localContainerB
        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacityInUlongs];
        containerB.CopyTo(localContainerB);

        // create other localContainers
        Span<ulong> localContainerC = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> localContainerD = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> localContainerE = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> resultContainer = stackalloc ulong[ContainerCapacityInUlongs];
        // clear resultContainer
        resultContainer.Clear();

        // detect overflow in...
        // ... localContainerA
        if (IsMostSignificantBitSet(containerA[^1]))
        {
            HandleOverflow(containerA, localContainerC);
        }
        // ... localContainerB
        if (IsMostSignificantBitSet(localContainerB[^1]))
        {
            HandleOverflow(localContainerB, localContainerC);
        }

        if (IsMostSignificantBitSet(containerA[^1]))
        {
            if (!IsMostSignificantBitSet(localContainerB[^1])) goto ORDER_66;
            if (IsReverseOrderedLess(containerA, localContainerB)) goto ORDER_66;
        }
        else if (!IsMostSignificantBitSet(localContainerB[^1]))
        {
            if (IsReverseOrderedLess(containerA, localContainerB)) goto ORDER_66;
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
                if (IsReverseOrderedLess(containerA, localContainerB)) goto ESCAPE;
            }
            else if (!IsMostSignificantBitSet(lastElementB))
            {
                if (IsReverseOrderedLess(containerA, localContainerB)) goto ESCAPE;
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
                            localContainerD[0] = lastQueueElemDiv;
                            localContainerD[1..].Clear();
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

                            if (!IsReverseOrderedLess(containerA, localContainerC)) break;

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
                        localContainerE[0] = lastQueueElemDiv;
                        localContainerE[1..].Clear();

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
        if (!IsReverseOrderedLess(localContainerB, containerA)) goto RETURNAL;
        SECOND_GATE:
        SetupContainer(localContainerE, containerA[0] / localContainerB[0]);
        AddContainers(resultContainer, localContainerE);
    RETURNAL:
        // Padawans survived
        resultContainer.CopyTo(containerA);
        return;

    // Execute...
    ORDER_66:
        // set all the containerA elements to 0
        containerA.Clear();
    }

    /// <summary>
    /// Executes set of encryption methods on provided containers.
    /// </summary>
    /// <param name="containerA">The first container to use.</param>
    /// <param name="containerB">The second container to use.</param>
    /// <param name="localContainer">The third container to use.</param>
    /// <returns>Modifies <paramref name="containerA"/>.</returns>
    private static void SetOfEncryptionMethods(Span<ulong> containerA, ReadOnlySpan<ulong> containerB, Span<ulong> localContainer)
    {
        // prepare localContainer
        containerA.CopyTo(localContainer);
        // execute set of encryption methods
        Limeghetti(localContainer, containerB);
        EncryptionFirst(localContainer, containerB);
        SubtractContainers(containerA, localContainer);
    }

    /// <summary>
    /// Watch out for its sharp teeth!
    /// </summary>
    /// <param name="containerA">The first container to use.</param>
    /// <param name="containerB">The second container to use.</param>
    /// <param name="containerC">The third container to use.</param>
    /// <returns>Modifies <paramref name="containerA"/></returns>
    private void Limegator(Span<ulong> containerA, ReadOnlySpan<ulong> containerB, ReadOnlySpan<ulong> containerC)
    {
        // create resultContainer
        Span<ulong> resultContainer = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer<ulong>(resultContainer, 1);

        // create localContainerB
        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacityInUlongs];
        containerB.CopyTo(localContainerB);

        // create other localContainers
        Span<ulong> localContainerA = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> localContainerC = stackalloc ulong[ContainerCapacityInUlongs];

        while (true)
        {
            localContainerA.Clear();
            if (LoopBreaker(localContainerB, localContainerA)) break;

            if (((byte)localContainerB[0] & 1) != 0)
            {
                resultContainer.CopyTo(localContainerA);
                EncryptionFirst(localContainerA, containerA);
                SetOfEncryptionMethods(localContainerA, containerC, localContainerC);
                localContainerA.CopyTo(resultContainer);
            }
            containerA.CopyTo(localContainerA);
            containerA.CopyTo(localContainerC);
            EncryptionFirst(localContainerA, localContainerC);
            SetOfEncryptionMethods(localContainerA, containerC, localContainerC);
            localContainerA.CopyTo(containerA);

            for (var i = 0; i < ContainerCapacityInUlongs - 1; i++)
            {
                localContainerB[i] >>= 1;
                localContainerB[i] |= localContainerB[i + 1] << 63;
            }
            localContainerB[^1] >>= 1;
        }

        resultContainer.CopyTo(containerA);
        return;

        static bool LoopBreaker(ReadOnlySpan<ulong> containerLeft, ReadOnlySpan<ulong> containerRight)
            => LastNonZeroIndexZeroBased(containerLeft) == 0 || IsReverseOrderedLess(containerLeft, containerRight);
    }

    /// <summary>
    /// Hashes public keys and combines them together.
    /// </summary>
    /// <param name="segmentHashedKey"></param>
    /// <param name="cKey1"></param>
    /// <param name="cUserId"></param>
    /// <param name="limeBank"></param>
    private void HashPublicKeys(Span<ulong> segmentHashedKey, ReadOnlySpan<ulong> cKey1, ReadOnlySpan<ulong> cUserId, ReadOnlySpan<LimeHashedKeyBank> limeBank)
    {
        Span<ulong> localContainerA = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacityInUlongs];
        for (var i = 0; i < limeBank.Length; i++)
        {
            limeBank[i].Header.CopyTo(localContainerA);
            Limegator(localContainerA, cUserId, cKey1);
            limeBank[i].KeyFragment.CopyTo(localContainerB);
            Limeghetti(localContainerB, localContainerA);
            // put the calculated part of the key in the segmentHashedKey
            segmentHashedKey[i] = localContainerB[0];
        }
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
            for (var i = 0; i < _limerouselRotationsTable.Length; i++)
            {
                var rotatedItem = ulong.RotateLeft(item, _limerouselRotationsTable[i]);
                item = containerA[_limerouselPositionsTable[i]];
                containerA[_limerouselPositionsTable[i]] = rotatedItem;
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
            containerA[0] ^= _limerouselXorsTable[x];
        }
    }

    /// <summary>
    /// Calculates checksum of a Lime Data Segment.
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    private void CalculateSegmentDataChecksum(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
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
    /// Decrypts <paramref name="limeSegments"/>
    /// </summary>
    /// <param name="limeSegments"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public bool DecryptData(Span<LimeDataSegment> limeSegments, ulong userId)
    {
        // load key into container
        Span<ulong> cKey1 = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer(cKey1, _privateKey1);

        // load userID into container
        Span<ulong> cUserId = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer(cUserId, NotUserId(userId));

        Span<ulong> cHashPublicKeysResult = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> checksumContainer = stackalloc ulong[ChecksumContainerCapacityInBytes];
        for (var i = 0; i < limeSegments.Length; i++)
        {
            Span<byte> dataAsBytes = limeSegments[i].SegmentData;
            var dataAsUlongs = MemoryMarshal.Cast<byte, ulong>(dataAsBytes);
            // hash public keys
            HashPublicKeys(cHashPublicKeysResult, cKey1, cUserId, limeSegments[i].HashedKeyBanks);
            cHashPublicKeysResult = cHashPublicKeysResult[..4];

            // deencrypt SegmentData
            var cHashPublicKeysResultAsBytes = MemoryMarshal.Cast<ulong, byte>(cHashPublicKeysResult);
            DeencryptData(dataAsBytes, cHashPublicKeysResultAsBytes);

            // compare a newly calculated checksum with the old one on the first segment and break loop if not equal
            if (i != 0) continue;
            CalculateSegmentDataChecksum(checksumContainer, dataAsUlongs);
            if (!limeSegments[i].ValidateSegmentChecksum(checksumContainer)) return false;
        }
        return true;
    }

    /// <summary>
    /// Encrypts <paramref name="limeSegments"/>
    /// </summary>
    /// <param name="limeSegments"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public bool EncryptData(Span<LimeDataSegment> limeSegments, ulong userId)
    {
        // load key into container
        Span<ulong> cKey1 = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer(cKey1, _privateKey1);

        // load userID into container
        Span<ulong> cUserId = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer(cUserId, NotUserId(userId));
        
        // load key type into container
        Span<ulong> cKeyType = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer(cKeyType, KeyType);
        
        // calculate seed
        Span<ulong> cLimeSeed = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer(cLimeSeed, _privateKey3);
        Limegator(cLimeSeed, cUserId, cKey1);
        Limegator(cLimeSeed, cKeyType, cKey1);

        // calculate header
        Span<ulong> cHeader = stackalloc ulong[ContainerCapacityInUlongs];
        ReadOnlySpan<ulong> header = MemoryMarshal.Cast<byte, ulong>(_headerKey);
        SetupContainer(cHeader, header);

        Span<ulong> cHashPublicKeysResult = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> checksumContainer = stackalloc ulong[ChecksumContainerCapacityInBytes];
        Span<ulong> cHashedKeyPart = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> cRandomizer = stackalloc ulong[ContainerCapacityInUlongs];
        var cRandomizerAsBytesSpan = MemoryMarshal.Cast<ulong, byte>(cRandomizer)[..8];
        foreach (var segment in limeSegments)
        {
            Span<byte> dataAsBytes = segment.SegmentData;
            var dataAsUlongs = MemoryMarshal.Cast<byte, ulong>(dataAsBytes);

            for (var j = 0; j < segment.HashedKeyBanks.Length; j++)
            {
                // calculate hashed key
                cLimeSeed.CopyTo(cHashedKeyPart);
                RandomizeSpan(cRandomizerAsBytesSpan);
                EncryptionFirst(cHashedKeyPart, cRandomizer);
                // update header and hashed key
                segment.HashedKeyBanks[j].Header = cHeader[..segment.HashedKeyBanks[j].Header.Length].ToArray();
                segment.HashedKeyBanks[j].KeyFragment = cHashedKeyPart[..segment.HashedKeyBanks[j].KeyFragment.Length].ToArray();
            }

            // calculate and set a checksum of current segment
            checksumContainer.Clear();
            CalculateSegmentDataChecksum(checksumContainer, dataAsUlongs);
            segment.SetSegmentChecksum(checksumContainer);

            // hash public keys
            HashPublicKeys(cHashPublicKeysResult, cKey1, cUserId, segment.HashedKeyBanks);
            cHashPublicKeysResult = cHashPublicKeysResult[..4];

            // deencrypt SegmentData
            var cHashPublicKeysResultAsBytes = MemoryMarshal.Cast<ulong, byte>(cHashPublicKeysResult);
            DeencryptData(dataAsBytes, cHashPublicKeysResultAsBytes);
        }
        return true;
    }

    /// <summary>
    /// Very slow, but effective. This is an ultimate act of desperation.
    /// </summary>
    /// <param name="cts"></param>
    /// <param name="limeSegment"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="userId"></param>
    public bool LimepickSegmentBatch(CancellationTokenSource cts, LimeDataSegment limeSegment, uint start, uint end, out uint userId)
    {
        // load key into container
        Span<ulong> cKey1 = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer(cKey1, _privateKey1);

        userId = 0;
        byte[] pattern = [ 0x01, 0x0, 0x0, 0x0 ];
        var segmentDataFirstRow = limeSegment.SegmentData.AsSpan(0, 16);
        Span<byte> dataAsBytes = stackalloc byte[segmentDataFirstRow.Length];
        Span<ulong> cUserId = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> cHashPublicKeysResult = stackalloc ulong[ContainerCapacityInUlongs];
        for (var i = start; i <= end; i++)
        {
            if (cts.IsCancellationRequested) return false;

            // load userID into container
            SetupContainer(cUserId, NotUserId(i));

            // load segment data
            segmentDataFirstRow.CopyTo(dataAsBytes);

            // hash public keys
            HashPublicKeys(cHashPublicKeysResult, cKey1, cUserId, limeSegment.HashedKeyBanks);
            cHashPublicKeysResult = cHashPublicKeysResult[..4];

            // deencrypt SegmentData
            var cHashPublicKeysResultAsBytes = MemoryMarshal.Cast<ulong, byte>(cHashPublicKeysResult);
            DeencryptData(dataAsBytes, cHashPublicKeysResultAsBytes);

            var result = dataAsBytes.Slice(4, 4).SequenceEqual(pattern);
            if (!result) continue;
            userId = i;
            return result;
        }
        return false;
    }
    
    #endregion

}