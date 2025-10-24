using LimebrellaSharpCore.Models.DSSS.Lime;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using static System.Threading.Tasks.Task;
using Aes = System.Runtime.Intrinsics.X86.Aes;
using AesNative = System.Security.Cryptography.Aes;

namespace LimebrellaSharpCore.Helpers;

public static class LimeDeencryptor
{
    #region CONSTANTS

    private const byte KeyType = 0x14;
    private const byte ChecksumContainerCapacityInBytes = 0x19;
    private const byte ContainerCapacityInUlongs = 0x20;
    
    private static readonly ulong[] PrivateKey1 = "8ztvuXKgtyUV5Fw5GCnhgq2Km9wKZNNETXnIEKuGNxc=".FromBase64<ulong>();
    private static readonly ulong[] PrivateKey2 = "+Z23XDnQ25IKcq4cjJRwwVbFTW4FsmmipjxkiFXDmws=".FromBase64<ulong>();
    private static readonly ulong[] PrivateKey3 = "5m9USvzOaMXvB7mgeyd1hTRKHbYTdugx9zufvV9E9xU=".FromBase64<ulong>();

    private static readonly byte[] LimerouselRotationsTable = "AQMGCg8VHCQtNwIOGyk4CBkrPhInPRQs".FromBase64<byte>();
    private static readonly byte[] LimerouselPositionsTable = "CgcLERIDBRAIFRgEDxcTDQwCFA4WCQYB".FromBase64<byte>();
    private static readonly ulong[] LimerouselXorsTable = "AQAAAAAAAACCgAAAAAAAAIqAAAAAAACAAIAAgAAAAICLgAAAAAAAAAEAAIAAAAAAgYAAgAAAAIAJgAAAAAAAgIoAAAAAAAAAiAAAAAAAAAAJgACAAAAAAAoAAIAAAAAAi4AAgAAAAACLAAAAAAAAgImAAAAAAACAA4AAAAAAAIACgAAAAAAAgIAAAAAAAACACoAAAAAAAAAKAACAAAAAgIGAAIAAAACAgIAAAAAAAIABAACAAAAAAAiAAIAAAACAAQMGCg8VHCQ=".FromBase64<ulong>();

    private static readonly ulong[] HeaderKey = CreateKey(KeyType);

    /// <summary>
    /// Represents the AES encryption platform currently supported by the environment.
    /// </summary>
    private static readonly AesEncryptionPlatform CurrentAesEncryptionPlatform = GetSupportedAesEncryption();
    
    #endregion

    #region AES_ENCRYPTION_PLATFORM

    /// <summary>
    /// Determines whether both AES and SSE2 hardware intrinsics are supported on the current platform.
    /// </summary>
    /// <returns><see langword="true"/> if both AES and SSE2 intrinsics are available; otherwise, <see langword="false"/>.</returns>
    public static bool IsIntrinsicsSupported()
        => Aes.IsSupported && Sse2.IsSupported;

    /// <summary>
    /// Determines whether software-based AES encryption is supported on the current platform.
    /// </summary>
    /// <returns><see langword="true"/> if software AES encryption is available; otherwise, <see langword="false"/>.</returns>
    public static bool IsSoftwareAesSupported()
    {
        try
        {
            using (AesNative.Create())
                return true;
        }
        catch (PlatformNotSupportedException)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Specifies the platform used to perform AES encryption operations.
    /// </summary>
    public enum AesEncryptionPlatform
    {
        Hardware,
        Software
    }

    /// <summary>
    /// Determines the supported AES encryption platform. Performs hardware and software checks to identify if AES encryption is supported on the current platform and returns the corresponding platform type.
    /// </summary>
    /// <returns>AesEncryptionPlatform value indicating the supported platform.</returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public static AesEncryptionPlatform GetSupportedAesEncryption()
    {
        // check for hardware support first
        if (IsIntrinsicsSupported()) 
            return AesEncryptionPlatform.Hardware;
        // check for software support next
        return IsSoftwareAesSupported() ? AesEncryptionPlatform.Software : throw new PlatformNotSupportedException();
    }

    /// <summary>
    /// Deencrypts the specified input data using the provided encryption key and the selected AES encryption platform.
    /// </summary>
    /// <param name="inputData">The span of bytes containing the data to be deencrypted.</param>
    /// <param name="encryptionKey">A span of bytes representing the encryption key and, for software mode, additional state information.</param>
    private static void DeencryptData(Span<byte> inputData, Span<byte> encryptionKey)
    {
        switch (CurrentAesEncryptionPlatform)
        {
            case AesEncryptionPlatform.Hardware:
                var dataAsVectors = MemoryMarshal.Cast<byte, Vector128<byte>>(inputData);
                var encryptionKeyAsVectors = MemoryMarshal.Cast<byte, Vector128<byte>>(encryptionKey);
                DeencryptIntrinsics(dataAsVectors, encryptionKeyAsVectors);
                return;
            default:
            case AesEncryptionPlatform.Software:
                var key = encryptionKey[..16].ToArray();
                var state = encryptionKey[16..];
                AesDeencryptSoftwareBased(inputData, key, state);
                return;
        }
    }

    /// <summary>
    /// Performs asynchronous deencryption of the specified data using the provided encryption key and the selected AES encryption platform.
    /// </summary>
    /// <param name="inputData">The encrypted data to be deencrypted.</param>
    /// <param name="encryptionKey">The key used for AES deencryption.</param>
    /// <returns>A task that represents the asynchronous deencryption operation.</returns>
    private static async Task DeencryptDataAsync(Memory<byte> inputData, Memory<byte> encryptionKey)
    {
        switch (CurrentAesEncryptionPlatform)
        {
            case AesEncryptionPlatform.Hardware:
                await DeencryptIntrinsicsAsync(inputData, encryptionKey);
                return;
            default:
            case AesEncryptionPlatform.Software:
                await AesDeencryptSoftwareBasedAsync(inputData, encryptionKey);
                return;
        }
    }

    #endregion

    #region HELPERS

    /// <summary>
    /// Creates a new instance of <see cref="ParallelOptions"/> configured for optimal parallel execution on the current machine.
    /// </summary>
    /// <param name="ct">An optional cancellation token to observe during parallel operations. If <paramref name="ct"/> is <see langword="null"/>, a default non-cancelable token is used.</param>
    /// <returns>A <see cref="ParallelOptions"/> instance with the specified cancellation token and a maximum degree of parallelism set to one less than the number of available processors.</returns>
    public static ParallelOptions GetParallelOptions(CancellationToken? ct = null)
        => new()
        {
            CancellationToken = ct ?? CancellationToken.None,
            MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1)
        };

    /// <summary>
    /// Calculates the upper 64 bits of the 128-bit product of two 64-bit unsigned integers.
    /// </summary>
    /// <remarks>Based on: https://gist.github.com/cocowalla/6070a53445e872f2bb24304712a3e1d2.</remarks>
    /// <param name="left">The first 64-bit unsigned integer to multiply.</param>
    /// <param name="right">The second 64-bit unsigned integer to multiply.</param>
    /// <returns>The upper 64 bits of the 128-bit product of the specified operands.</returns>
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
    /// Calculates the lower 64 bits of the product of two unsigned 64-bit integers.
    /// </summary>
    /// <param name="left">The first unsigned 64-bit integer to multiply.</param>
    /// <param name="right">The second unsigned 64-bit integer to multiply.</param>
    /// <returns>The lower 64 bits of the product of the specified values.</returns>
    private static ulong MulLow(ulong left, ulong right)
        => left * right;

    /// <summary>
    /// Calculates the number of times the specified unsigned integer can be right-shifted by the given step before reaching zero.
    /// </summary>
    /// <param name="radicand">The unsigned integer value to be repeatedly right-shifted.</param>
    /// <param name="step">The number of bits to shift right in each iteration.</param>
    /// <returns>The number of iterations required to reduce the radicand to zero by right-shifting it by the specified step each time.</returns>
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
    /// Determines whether the most significant bit of the specified 32-bit unsigned integer is set.
    /// </summary>
    /// <param name="number">The 32-bit unsigned integer to evaluate.</param>
    /// <returns><see langword="true"/> if the most significant bit of number is set; otherwise, <see langword="false"/>.</returns>
    private static bool IsMostSignificantBitSet(uint number) 
        => (number & 0x80000000) != 0;

    /// <summary>
    /// Determines whether the most significant bit of the specified 64-bit unsigned integer is set.
    /// </summary>
    /// <param name="number">The 64-bit unsigned integer to evaluate.</param>
    /// <returns><see langword="true"/> if the most significant bit of number is set; otherwise, <see langword="false"/>.</returns>
    private static bool IsMostSignificantBitSet(ulong number)
        => (number & 0x8000000000000000) != 0;

    /// <summary>
    /// Fills the specified span of bytes with random values, starting at the given index.
    /// </summary>
    /// <param name="span">The span of bytes to populate with random values.</param>
    /// <param name="start">The zero-based index at which to begin randomizing the span.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> is less than 0 or greater than the length of <paramref name="span"/>.</exception>
    private static void RandomizeSpan(Span<byte> span, int start = 0)
    {
        if (start < 0 || start > span.Length)
            throw new ArgumentOutOfRangeException(nameof(start), "Start index is out of range for the span.");
        Random random = new();
        for (var i = start; i < span.Length; i++)
            span[i] = (byte)random.Next(byte.MaxValue + 1);
    }

    /// <summary>
    /// Computes the bitwise complement of the specified user identifier.
    /// </summary>
    /// <param name="userId">The user identifier to invert.</param>
    /// <returns>An unsigned 64-bit integer representing the bitwise complement of the input user ID.</returns>
    private static ulong NotUserId(ulong userId) => ~userId;

    /// <summary>
    /// Compares two containers in reverse lexicographical order to determine if <paramref name="containerA"/> is less than <paramref name="containerB"/>.
    /// Starting from the last element, it checks each element pair by pair until a difference is found or all elements have been compared.
    /// </summary>
    /// <param name="containerA">The first container to compare.</param>
    /// <param name="containerB">The second container to compare.</param>
    /// <returns>Returns <see langword="true"/> if <paramref name="containerA"/> is reverse-ordered less than <paramref name="containerB"/>; otherwise, returns <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"></exception>
    private static bool IsReverseOrderedLess(ReadOnlySpan<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        if(containerA.Length != containerB.Length) throw new ArgumentException("The two containers must have the same length.");
        for (var i = containerA.Length; i > 0; i--)
        {
            // Check if nth element of containerA is less than containerB
            if (containerA[i - 1] < containerB[i - 1]) return true;
            // Check if nth element of containerA is greater than containerB
            if (containerA[i - 1] > containerB[i - 1]) return false;
        }
        return false;
    }

    /// <summary>
    /// Finds the zero-based index of the last element in the span that is not equal to the default value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The value type of the elements in the span. Must implement <see cref="IEquatable{T}"/> to support equality comparison with the default value.</typeparam>
    /// <param name="span">The read-only span of value type elements to search.</param>
    /// <returns>The zero-based index of the last non-default element in the span; returns 0 if all elements are equal to the default value.</returns>
    private static int LastNonZeroIndexZeroBased<T>(ReadOnlySpan<T> span) where T : struct, IEquatable<T>
    {
        for (var i = span.Length; i > 0; i--)
            if (!span[i - 1].Equals(default))
                return i;
        return 0;
    }

    /// <summary>
    /// Initializes the specified container by setting the element at the given position to the provided cargo value and clearing all other elements.
    /// </summary>
    /// <typeparam name="T">The value type of the elements contained in the span.</typeparam>
    /// <param name="container">The span to be initialized. All elements except the one at the specified position will be cleared.</param>
    /// <param name="cargo">The value to assign to the element at the specified position within the container.</param>
    /// <param name="position">The zero-based index at which to place the cargo value. Defaults to 0.</param>
    private static void SetupContainer<T>(Span<T> container, T cargo, int position = 0) where T : struct
    {
        container[position] = cargo;
        container[..position].Clear();
        container[(position + 1)..].Clear();
    }

    /// <summary>
    /// Initializes a segment of the specified container span with the contents of the cargo span, starting at the given position, and clears all other elements in the container.
    /// </summary>
    /// <typeparam name="T">The value type of the elements contained in the spans.</typeparam>
    /// <param name="container">The span to be initialized and cleared. Elements outside the cargo segment will be set to their default value.</param>
    /// <param name="cargo">The read-only span whose contents are copied into the container starting at the specified position.</param>
    /// <param name="position">The zero-based index in the container at which to begin copying the cargo. Defaults to 0.</param>
    private static void SetupContainer<T>(Span<T> container, ReadOnlySpan<T> cargo, int position = 0) where T : struct
    {
        cargo.CopyTo(container[position..]);
        container[..position].Clear();
        container[(position + cargo.Length)..].Clear();
    }
    
    /// <summary>
    /// Initializes the specified container by placing the provided cargo at the given position asynchronously.
    /// </summary>
    /// <typeparam name="T">The value type of the elements contained in the memory buffer.</typeparam>
    /// <param name="container">The memory buffer to be initialized. Represents a contiguous region of memory for value type elements.</param>
    /// <param name="cargo">The value to be placed into the container at the specified position.</param>
    /// <param name="position">The zero-based index within the container at which to place the cargo. Defaults to 0.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    private static async Task SetupContainerAsync<T>(Memory<T> container, T cargo, int position = 0) where T : struct
        => await Run(() => SetupContainer(container.Span, cargo, position));

    /// <summary>
    /// Initializes the specified container with the provided cargo, starting at the given position asynchronously.
    /// </summary>
    /// <typeparam name="T">The value type of the elements contained in both the container and cargo memories.</typeparam>
    /// <param name="container">The memory region to be set up with the cargo data. Must be large enough to accommodate the cargo at the specified position.</param>
    /// <param name="cargo">The read-only memory containing the data to be placed into the container.</param>
    /// <param name="position">The zero-based index in the container at which to begin placing the cargo. Must be within the bounds of the container.</param>
    /// <returns>A task that represents the asynchronous setup operation.</returns>
    private static async Task SetupContainerAsync<T>(Memory<T> container, ReadOnlyMemory<T> cargo, int position = 0) where T : struct
        => await Run(() => SetupContainer(container.Span, cargo.Span, position));

    #endregion

    #region ENCRYPTION_METHODS

    /// <summary>
    /// Generates an encryption key based on the specified seed and returns a sequence of unsigned 64-bit integers representing the key.
    /// </summary>
    /// <param name="seed">The input value used as the basis for key generation.</param>
    /// <param name="length">The number of elements to include in the returned key. If set to 0, the method automatically determines the length based on the generated data.</param>
    /// <returns>An array of unsigned 64-bit integers containing the generated encryption key.</returns>
    private static ulong[] CreateKey(ulong seed, int length = 0)
    {
        // Create localContainerA
        Span<ulong> localContainerA = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer(localContainerA, seed);
        // Create resultContainer
        Span<ulong> resultContainer = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer(resultContainer, PrivateKey2);
        // Create localContainerB
        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacityInUlongs];
        // Execute a set of encryption methods
        SetOfEncryptionMethods(localContainerA, resultContainer, localContainerB);
        // Prepare resultContainer
        SetupContainer(resultContainer, PrivateKey3);
        // Prepare localContainerB
        SetupContainer(localContainerB, PrivateKey1);
        // Calculate a key and return it
        Limegator(resultContainer, localContainerA, localContainerB);
        length = length == 0 ? LastNonZeroIndexZeroBased(resultContainer) : length;
        return resultContainer[..length].ToArray();
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
        for (var i = 0; i < containerA.Length; i++)
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
        for (var i = 0; i < containerA.Length; i++)
        {
            var test0 = Convert.ToByte(testA | testB);
            var newValue = containerA[i] + containerB[i] + test0;
            testA = containerB[i] == newValue ? test0 : (byte)0;
            testB = Convert.ToByte(newValue < containerB[i]);
            containerA[i] = newValue;
        }
    }
    
    /// <summary>
    /// Decrypts or encrypts <paramref name="data"/> (uses intrinsic functions). 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns>Modified <paramref name="data"/></returns>
    public static void DeencryptIntrinsics(Span<Vector128<byte>> data, ReadOnlySpan<Vector128<byte>> key)
    {
        const byte rounds = 0xA;
        const int shift = 4;
        Span<Vector128<byte>> aesRoundKeys = stackalloc Vector128<byte>[rounds + 1];

        //// AES KEYGEN
        // Build the first block (Expand AES-128 key)
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
            // Shift xmm2 left by 4 bytes
            var shift1 = Sse2.ShiftLeftLogical128BitLane(aesRoundKeys[i].AsUInt32(), shift).AsInt32();
            // Shift shift1 left by 4 bytes
            var shift2 = Sse2.ShiftLeftLogical128BitLane(shift1.AsUInt32(), shift).AsInt32();
            // Shift shift2 left by 4 bytes
            var shift3 = Sse2.ShiftLeftLogical128BitLane(shift2, shift);
            // Compute the final result using shuffle and XOR instructions
            var shuffle1 = Sse2.Shuffle(innerRoundKey.AsInt32(), 255);
            var xor1 = Sse2.Xor(shift1, aesRoundKeys[i].AsInt32());
            var xor2 = Sse2.Xor(shift2, xor1);
            var xor3 = Sse2.Xor(xor2, shift3);
            var xor4 = Sse2.Xor(xor3, shuffle1);
            // Add key to the aesRoundKeys
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
    /// Decrypts or encrypts <paramref name="data"/> asynchronously (uses intrinsic functions). 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns>Modified <paramref name="data"/></returns>
    public static async Task DeencryptIntrinsicsAsync(Memory<byte> data, ReadOnlyMemory<byte> key)
    {
        await Run(() =>
        {
            var dataAsVectors = MemoryMarshal.Cast<byte, Vector128<byte>>(data.Span);
            var encryptionKeyAsVectors = MemoryMarshal.Cast<byte, Vector128<byte>>(key.Span);
            DeencryptIntrinsics(dataAsVectors, encryptionKeyAsVectors);
        });
    }

    /// <summary>
    /// Decrypts or encrypts <paramref name="data"/> (software-based).
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <param name="state"></param>
    public static void AesDeencryptSoftwareBased(Span<byte> data, byte[] key, Span<byte> state)
    {
        using var aes = AesNative.Create();
        aes.Key = key;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.Zeros;
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        for (var i = 0; i < data.Length; i += 16)
        {
            using (var ms = new MemoryStream())
            {
                using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                cs.Write(state);
                cs.FlushFinalBlock();
                ms.ToArray().CopyTo(state);
            }
            // Decrypt row of input data
            for (var j = 0; j < state.Length; j++)
                data[i + j] ^= state[j];
        }
    }

    /// <summary>
    /// Decrypts or encrypts <paramref name="data"/> asynchronously (software-based).
    /// </summary>
    /// <param name="data"></param>
    /// <param name="encryptionKey"></param>
    public static async Task AesDeencryptSoftwareBasedAsync(Memory<byte> data, Memory<byte> encryptionKey)
    {
        await Run(() =>
        {
            var key = encryptionKey[..16].ToArray();
            var state = encryptionKey[16..].Span;
            AesDeencryptSoftwareBased(data.Span, key, state);
        });
    }

    /// <summary>
    /// Handles overflow in the provided <paramref name="containerA"/>.
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    /// <returns>Modifies <paramref name="containerA"/>.</returns>
    private static void HandleOverflow(Span<ulong> containerA, Span<ulong> containerB)
    {
        // Setup containerB
        SetupContainer(containerB, (ulong)1);

        SubtractContainers(containerA, containerB);
        const byte x = 0x2;
        for (var i = 0; i < ContainerCapacityInUlongs - x; i++)
            containerA[i] = ~containerA[i] & 0xFFFFFFFFFFFFFFFF;
        for (var i = ContainerCapacityInUlongs - x; i < ContainerCapacityInUlongs; i++)
            containerA[i] = ~containerA[i];
    }

    /// <summary>
    /// First type of encryption.
    /// </summary>
    /// <param name="containerA">The first container to use.</param>
    /// <param name="containerB">The second container to use.</param>
    /// <returns>Modifies <paramref name="containerA"/>.</returns>
    private static void EncryptionFirst(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        // Check for empty containers
        var containerALength = LastNonZeroIndexZeroBased(containerA);
        if (containerALength == 0) return;
        var containerBLength = LastNonZeroIndexZeroBased(containerB);
        if (containerBLength == 0)
        {
            // Set all the containerA elements to 0
            containerA.Clear();
            return;
        }

        // Create a resultContainer
        Span<ulong> resultContainer = stackalloc ulong[ContainerCapacityInUlongs];
        // Create a localContainerB
        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacityInUlongs];
        containerB.CopyTo(localContainerB);

        while (true)
        {
            // Detect overflow in...
            var overflowSwitch = false;
            // ... LocalContainerA
            if (IsMostSignificantBitSet(containerA[^1]))
            {
                overflowSwitch ^= true;
                HandleOverflow(containerA, resultContainer);
                // Re-check container length
                containerALength = LastNonZeroIndexZeroBased(containerA);
            }

            // ... LocalContainerB
            if (IsMostSignificantBitSet(localContainerB[^1]))
            {
                overflowSwitch ^= true;
                HandleOverflow(localContainerB, resultContainer);
                // Re-check container length
                containerBLength = LastNonZeroIndexZeroBased(localContainerB);
                if (containerBLength == 0) break;
            }

            // Prepare resultContainer
            resultContainer.Clear();

            // Manipulate bytes in both containers
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
            if (overflowSwitch) break;

            // Update referenced containerA
            resultContainer.CopyTo(containerA);
            return;
        }

        containerA.Clear();
        SubtractContainers(containerA, resultContainer);
    }

    /// <summary>
    /// Second type of encryption.
    /// </summary>
    /// <param name="dataContainer"></param>
    /// <param name="bits"></param>
    /// <returns>Modifies <paramref name="dataContainer"/>.</returns>
    private static void EncryptionSecond(Span<ulong> dataContainer, int bits)
    {
        var division = bits >> 6; // Division by 64
        var reminder = bits & 0x3F; // Division reminder

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
        for (var i = division; i > 0; i--) 
            dataContainer[division - i] = 0;
    }

    /// <summary>
    /// Prepare a delicious knot of Limeghetti.
    /// </summary>
    /// <param name="containerA">The first container to use.</param>
    /// <param name="containerB">The second container to use.</param>
    /// <returns>Modifies <paramref name="containerA"/>.</returns>
    private static void Limeghetti(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
    {
        // Check for empty containers
        var containerALength = LastNonZeroIndexZeroBased(containerA);
        if (containerALength == 0) return;
        var containerBLength = LastNonZeroIndexZeroBased(containerB);
        if (containerBLength == 0)
        {
            // ORDER_66
            // Set all the containerA elements to 0
            containerA.Clear();
            return;
        }

        // Create a localContainerB
        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacityInUlongs];
        containerB.CopyTo(localContainerB);

        // Create other localContainers
        Span<ulong> localContainerC = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> localContainerD = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> localContainerE = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> resultContainer = stackalloc ulong[ContainerCapacityInUlongs];
        // Clear resultContainer
        resultContainer.Clear();

        while (true)
        {
            // Detect overflow in...
            // ... localContainerA
            if (IsMostSignificantBitSet(containerA[^1]))
                HandleOverflow(containerA, localContainerC);

            // ... localContainerB
            if (IsMostSignificantBitSet(localContainerB[^1]))
                HandleOverflow(localContainerB, localContainerC);

            if (IsMostSignificantBitSet(containerA[^1]))
            {
                if (!IsMostSignificantBitSet(localContainerB[^1])) break; // ORDER_66
                if (IsReverseOrderedLess(containerA, localContainerB)) break; // ORDER_66
            }
            else if (!IsMostSignificantBitSet(localContainerB[^1]))
            {
                if (IsReverseOrderedLess(containerA, localContainerB)) break; // ORDER_66
            }

            // Check container length
            var localContainerBLength = LastNonZeroIndexZeroBased(localContainerB);
            int localContainerALength;

            // Calculate bits
            var rootDegree = localContainerBLength == 0 ? 0 : RootDegree(localContainerB[localContainerBLength - 1], 1);
            var bits = 32 - (rootDegree & 0x1F);

            // Perform EncryptionSecond on both localContainers
            EncryptionSecond(containerA, bits);
            EncryptionSecond(localContainerB, bits);

            // Re-check container length
            localContainerBLength = LastNonZeroIndexZeroBased(localContainerB);

            // Remember the last element of containerA
            var lastElementA = containerA[^1];
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

                while (true)
                {
                    //LOOP_BREAKER
                    lastElementA = containerA[^1];
                    if (IsMostSignificantBitSet(containerA[^1]))
                    {
                        if (!IsMostSignificantBitSet(lastElementB)) break; // ESCAPE
                        if (IsReverseOrderedLess(containerA, localContainerB)) break; // ESCAPE
                    }
                    else if (!IsMostSignificantBitSet(lastElementB) && IsReverseOrderedLess(containerA, localContainerB)) break; // ESCAPE

                    // Re-check container length
                    localContainerALength = LastNonZeroIndexZeroBased(containerA);
                    
                    if (localContainerALength == 0) break; // ESCAPE
                    if (localContainerALength < 2) continue;
                    var tinyHashesA = 2 * localContainerALength - 2;
                    var lastQueueElemA = containerA[localContainerALength - 1];
                    if (lastQueueElemA >> 32 == 0)
                    {
                        tinyHashesA--;
                        lastQueueElemA = (containerA[localContainerALength - 2] >> 32) + (lastQueueElemA << 32);
                    }

                    var hashesGap = tinyHashesA - tinyHashesB;
                    var lastQueueElemDiv = lastQueueElemA / lastQueueElemB;

                    // Copy localContainerB into localContainerC
                    localContainerB.CopyTo(localContainerC);

                    if (tinyHashesA >= tinyHashesB)
                    {
                        if (lastQueueElemDiv >> 32 != 0) lastQueueElemDiv = 0xFFFFFFFF;
                        EncryptionSecond(localContainerC, 32 * hashesGap);
                        // Prepare localContainerD
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
                        else if (!IsMostSignificantBitSet(localContainerC[^1]))
                        {
                            LocalSubtractionProcess(ref lastQueueElemDiv, localContainerB, localContainerC, localContainerD, hashesGap);
                            continue;
                        }

                        if (!IsReverseOrderedLess(containerA, localContainerC)) break;
                        LocalSubtractionProcess(ref lastQueueElemDiv, localContainerB, localContainerC, localContainerD, hashesGap);
                    }

                    // Prepare localContainerD
                    localContainerD.Clear();
                    // Prepare localContainerE
                    localContainerE[0] = lastQueueElemDiv;
                    localContainerE[1..].Clear();

                    AddContainers(localContainerD, localContainerE);
                    EncryptionSecond(localContainerD, 32 * hashesGap);
                    lastElementB = localContainerB[^1];

                    // Calculate resultContainer
                    AddContainers(resultContainer, localContainerD);

                    // Prepare localContainerD
                    localContainerC.CopyTo(localContainerD);
                    // Update containerA
                    SubtractContainers(containerA, localContainerD);
                }
            }
            // ESCAPE
            localContainerALength = LastNonZeroIndexZeroBased(containerA);
            localContainerBLength = LastNonZeroIndexZeroBased(localContainerB);
            
            while (true)
            {
                if ((localContainerBLength > 0 && localContainerALength == 0) || localContainerB.SequenceEqual(containerA))
                {
                    LocalAdditionProcess(containerA, localContainerB, localContainerE, resultContainer);
                    break;
                }

                if ((IsMostSignificantBitSet(lastElementB) && IsMostSignificantBitSet(lastElementA)) || !IsMostSignificantBitSet(lastElementA))
                {
                    if (!IsReverseOrderedLess(localContainerB, containerA)) break;
                    LocalAdditionProcess(containerA, localContainerB, localContainerE, resultContainer);
                }
                break;
            }
            // RETURNAL
            resultContainer.CopyTo(containerA);
            return;
        }
        // ORDER_66
        // Set all the containerA elements to 0
        containerA.Clear();
        return;

        static void LocalSubtractionProcess(ref ulong lastQueueElemDiv, ReadOnlySpan<ulong> localContainerB, Span<ulong> localContainerC, Span<ulong> localContainerD, int hashesGap)
        {
            localContainerB.CopyTo(localContainerD);
            EncryptionSecond(localContainerD, 32 * hashesGap);
            SubtractContainers(localContainerC, localContainerD);
            lastQueueElemDiv--;
        }

        static void LocalAdditionProcess(ReadOnlySpan<ulong> localContainerA, ReadOnlySpan<ulong> localContainerB, Span<ulong> localContainerE, Span<ulong> resultContainer)
        {
            SetupContainer(localContainerE, localContainerA[0] / localContainerB[0]);
            AddContainers(resultContainer, localContainerE);
        }
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
        // Prepare localContainer
        containerA.CopyTo(localContainer);
        // Execute set of encryption methods
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
    private static void Limegator(Span<ulong> containerA, ReadOnlySpan<ulong> containerB, ReadOnlySpan<ulong> containerC)
    {
        // Create resultContainer
        Span<ulong> resultContainer = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer<ulong>(resultContainer, 1);

        // Create localContainerB
        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacityInUlongs];
        containerB.CopyTo(localContainerB);

        // Create other localContainers
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
    private static void HashPublicKeys(Span<ulong> segmentHashedKey, ReadOnlySpan<ulong> cKey1, ReadOnlySpan<ulong> cUserId, ReadOnlySpan<LimeHashedKeyBank> limeBank)
    {
        Span<ulong> localContainerA = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> localContainerB = stackalloc ulong[ContainerCapacityInUlongs];
        for (var i = 0; i < limeBank.Length; i++)
        {
            limeBank[i].Header.CopyTo(localContainerA);
            Limegator(localContainerA, cUserId, cKey1);
            limeBank[i].KeyFragment.CopyTo(localContainerB);
            Limeghetti(localContainerB, localContainerA);
            // Put the calculated part of the key in the segmentHashedKey
            segmentHashedKey[i] = localContainerB[0];
        }
    }

    /// <summary>
    /// Hashes public keys and combines them together asynchronously.
    /// </summary>
    /// <param name="segmentHashedKey"></param>
    /// <param name="cKey1"></param>
    /// <param name="cUserId"></param>
    /// <param name="limeBank"></param>
    private static async Task HashPublicKeysAsync(Memory<byte> segmentHashedKey, ReadOnlyMemory<ulong> cKey1, ReadOnlyMemory<ulong> cUserId, ReadOnlyMemory<LimeHashedKeyBank> limeBank)
    {
        await Run(() =>
        {
            Span<ulong> localContainerA = stackalloc ulong[ContainerCapacityInUlongs];
            Span<ulong> localContainerB = stackalloc ulong[ContainerCapacityInUlongs];
            Span<ulong> resultContainer = stackalloc ulong[limeBank.Length];
            var localLimeBank = limeBank.Span;
            var localCUserId = cUserId.Span;
            var localCKey1 = cKey1.Span;
            for (var i = 0; i < limeBank.Length; i++)
            {
                localLimeBank[i].Header.CopyTo(localContainerA);
                Limegator(localContainerA, localCUserId, localCKey1);
                localLimeBank[i].KeyFragment.CopyTo(localContainerB);
                Limeghetti(localContainerB, localContainerA);
                // Put the calculated part of the key in the segmentHashedKey
                resultContainer[i] = localContainerB[0];
            }
            var resultContainerAsBytes = MemoryMarshal.Cast<ulong, byte>(resultContainer);
            resultContainerAsBytes.CopyTo(segmentHashedKey.Span);
        });
    }

    /// <summary>
    /// You spin me right 'round, baby, right 'round.
    /// </summary>
    /// <param name="containerA"></param>
    private static void Limerousel(Span<ulong> containerA)
    {
        const int laps = 5;
        Span<ulong> localContainer = stackalloc ulong[laps];
        for (var x = 0; x < containerA.Length - 1; x++)
        {
            // Reset localContainer
            localContainer.Clear();

            // First ride
            for (var i = 0; i < laps; i++)
                for (var y = 0; y < laps; y++)
                    localContainer[i] ^= containerA[y * laps + i];

            // Second ride
            for (var i = 0; i < laps; i++)
            {
                var left = ulong.RotateLeft(localContainer[(i + 1) % laps], 1);
                var right = localContainer[(i + laps - 1) % laps];
                for (var y = 0; y < laps; y++)
                    containerA[y * laps + i] ^= left ^ right;
            }

            // Third ride
            var item = containerA[1];
            for (var i = 0; i < LimerouselRotationsTable.Length; i++)
            {
                var rotatedItem = ulong.RotateLeft(item, LimerouselRotationsTable[i]);
                item = containerA[LimerouselPositionsTable[i]];
                containerA[LimerouselPositionsTable[i]] = rotatedItem;
            }

            // Fourth ride
            for (var i = 0; i < laps; i++)
            {
                var elementA = containerA[i * laps];
                var elementB = containerA[i * laps + 1];
                var elementC = containerA[i * laps + 2];
                var elementD = containerA[i * laps + 3];
                var elementE = containerA[i * laps + 4];
                containerA[i * laps] = elementA ^ (elementC & ~elementB);
                containerA[i * laps + 1] = elementB ^ (elementD & ~elementC);
                containerA[i * laps + 2] = elementC ^ (elementE & ~elementD);
                containerA[i * laps + 3] = elementD ^ (elementA & ~elementE);
                containerA[i * laps + 4] = elementE ^ (elementB & ~elementA);
            }
            containerA[0] ^= LimerouselXorsTable[x];
        }
    }
    
    /// <summary>
    /// Calculates checksum of a Lime Data Segment.
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    private static void CalculateSegmentDataChecksum(Span<ulong> containerA, ReadOnlySpan<ulong> containerB)
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
    /// Calculates checksum of a Lime Data Segment asynchronously.
    /// </summary>
    /// <param name="containerA"></param>
    /// <param name="containerB"></param>
    private static async Task CalculateSegmentDataChecksumAsync(Memory<byte> containerA, ReadOnlyMemory<byte> containerB)
    {
        await Run(() =>
        {
            var localContainerA = MemoryMarshal.Cast<byte, ulong>(containerA.Span);
            var localContainerB = MemoryMarshal.Cast<byte, ulong>(containerB.Span);
            CalculateSegmentDataChecksum(localContainerA, localContainerB);
        });
    }
    
    /// <summary>
    /// Decrypts the specified collection of LimeDataSegment objects asynchronously using the provided AES encryption platform and user identifier.
    /// </summary>
    /// <param name="limeSegments">A memory region containing the LimeDataSegment instances to be decrypted.</param>
    /// <param name="userId">The unique identifier of the user whose key is used for decryption.</param>
    /// <param name="po">Optional parallelization settings that control how the decryption tasks are scheduled and executed. If null, default parallel options are used.</param>
    /// <returns>A task that represents the asynchronous decryption operation.</returns>
    /// <exception cref="InvalidDataException">Thrown if the checksum validation fails for the first LimeDataSegment, indicating possible data corruption or tampering.</exception>
    public static async Task DecryptDataAsync(Memory<LimeDataSegment> limeSegments, ulong userId, ParallelOptions? po = null)
    {
        // Load key into container
        Memory<ulong> cKey1 = new ulong[ContainerCapacityInUlongs];
        await SetupContainerAsync(cKey1, PrivateKey1);

        // Load userID into container
        Memory<ulong> cUserId = new ulong[ContainerCapacityInUlongs];
        await SetupContainerAsync(cUserId, NotUserId(userId));

        // Assure ParallelOptions is not null
        po ??= GetParallelOptions();

        await Parallel.ForAsync(0, limeSegments.Length, po, async (i, _) =>
        {
            Memory<byte> cHashPublicKeysResult = new byte[ContainerCapacityInUlongs * sizeof(ulong)];
            Memory<byte> checksumContainer = new byte[ChecksumContainerCapacityInBytes * sizeof(ulong)];
            var segment = limeSegments.Span[i];
            // Hash public keys
            await HashPublicKeysAsync(cHashPublicKeysResult, cKey1, cUserId, segment.HashedKeyBanks);
            cHashPublicKeysResult = cHashPublicKeysResult[..(4 * sizeof(ulong))];

            // Deencrypt SegmentData
            Memory<byte> dataAsBytes = segment.SegmentData;
            await DeencryptDataAsync(dataAsBytes, cHashPublicKeysResult);
            // Compare a newly calculated checksum with the old one on the first segment and break loop if not equal
            if (i == 0)
            {
                await CalculateSegmentDataChecksumAsync(checksumContainer, dataAsBytes);
                var test = await segment.ValidateSegmentChecksumAsync(checksumContainer);
                if (!test) throw new InvalidDataException("Lime Data Segment checksum validation failed.");
            }
        });
    }
    
    /// <summary>
    /// Encrypts the specified collection of LimeDataSegment objects asynchronously using the provided AES encryption platform and user identifier.
    /// </summary>
    /// <param name="limeSegments">A memory buffer containing the LimeDataSegment objects to be encrypted. Each segment will be processed and encrypted individually.</param>
    /// <param name="userId">The unique identifier of the user for whom the encryption is performed.</param>
    /// <param name="po">Optional parallelization settings that control how the encryption tasks are scheduled and executed. If null, default parallel options are used.</param>
    /// <returns>A task that represents the asynchronous encryption operation. The task completes when all segments have been encrypted.</returns>
    public static async Task EncryptDataAsync(Memory<LimeDataSegment> limeSegments, ulong userId, ParallelOptions? po = null)
    {
        // Load key into container
        Memory<ulong> cKey1 = new ulong[ContainerCapacityInUlongs];
        await SetupContainerAsync(cKey1, PrivateKey1);

        // Load userID into container
        Memory<ulong> cUserId = new ulong[ContainerCapacityInUlongs];
        await SetupContainerAsync(cUserId, NotUserId(userId));

        // Calculate seed
        Memory<ulong> cLimeSeed = new ulong[ContainerCapacityInUlongs];
        await SetupContainerAsync(cLimeSeed, PrivateKey3);
        await Run(() =>
        {
            var cLimeSeedSpan = cLimeSeed.Span;
            var cKey1Span = cKey1.Span;

            // Load key type into container
            Span<ulong> cKeyType = new ulong[ContainerCapacityInUlongs];
            SetupContainer(cKeyType, KeyType);

            Limegator(cLimeSeedSpan, cUserId.Span, cKey1Span);
            Limegator(cLimeSeedSpan, cKeyType, cKey1Span);
        });

        // Prepare randomizer
        Memory<ulong> cRandomizer = new ulong[ContainerCapacityInUlongs];
        var cRandomizerSeedSpan = MemoryMarshal.Cast<ulong, byte>(cRandomizer.Span)[..sizeof(ulong)];
        RandomizeSpan(cRandomizerSeedSpan);
        var cRandomizerSeed = cRandomizer.Span[0];

        // Assure ParallelOptions is not null
        po ??= GetParallelOptions();

        await Parallel.ForAsync(0, limeSegments.Length, po, async (i, ct) =>
        {
            // Create other Memories
            Memory<ulong> cHashedKeyPart = new ulong[ContainerCapacityInUlongs];
            Memory<byte> checksumContainer = new byte[ChecksumContainerCapacityInBytes * sizeof(ulong)];
            Memory<byte> cHashPublicKeysResult = new byte[ContainerCapacityInUlongs * sizeof(ulong)];

            var segment = limeSegments.Span[i];
            await Run(() =>
            {
                var cLimeSeedSpan = cLimeSeed.Span;
                var cHashedKeyPartSpan = cHashedKeyPart.Span;
                var cRandomizerSpan = cRandomizer.Span;
                for (var j = 0; j < segment.HashedKeyBanks.Length; j++)
                {
                    // Calculate hashed key
                    cLimeSeedSpan.CopyTo(cHashedKeyPartSpan);
                    cRandomizerSpan[0] = cRandomizerSeed++;
                    EncryptionFirst(cHashedKeyPartSpan, cRandomizerSpan);
                    // Update header and hashed key
                    segment.HashedKeyBanks[j].SetHeader(HeaderKey);
                    segment.HashedKeyBanks[j].SetKey(cHashedKeyPartSpan[..segment.HashedKeyBanks[j].KeyFragment.Length]);
                }
            }, ct);

            var dataAsBytes = segment.SegmentData.AsMemory();

            // Calculate and set a checksum of current segment
            checksumContainer.Span.Clear();
            await CalculateSegmentDataChecksumAsync(checksumContainer, dataAsBytes);
            var checksum = MemoryMarshal.Cast<byte, ulong>(checksumContainer.Span).ToArray();
            segment.SetSegmentChecksum(checksum);

            // Hash public keys
            await HashPublicKeysAsync(cHashPublicKeysResult, cKey1, cUserId, segment.HashedKeyBanks.AsMemory());
            cHashPublicKeysResult = cHashPublicKeysResult[..(4 * sizeof(ulong))];

            // Deencrypt SegmentData
            await DeencryptDataAsync(dataAsBytes, cHashPublicKeysResult);
        });
    }

#if DEBUG
    /// <summary>
    /// Attempts to identify the user ID associated with a segment of Lime data by processing and decrypting the segment within the specified range.
    /// </summary>
    /// <param name="limeSegment">The LimeDataSegment containing the segment data and associated key banks to be analyzed.</param>
    /// <param name="po">Optional parallel execution settings, including cancellation support. If cancellation is requested, the operation terminates early.</param>
    /// <param name="start">The inclusive starting value of the user ID range to search. Defaults to uint.MinValue if not specified.</param>
    /// <param name="end">The inclusive ending value of the user ID range to search. Defaults to uint.MaxValue if not specified.</param>
    /// <returns>The user ID found within the specified range that matches the expected pattern after decryption. Returns null if no matching user ID is found.</returns>
    public static uint? LimepickSegmentBatch(LimeDataSegment limeSegment, ParallelOptions? po = null, uint start = uint.MinValue, uint end = uint.MaxValue)
    {
        // TODO: Make it doesn't get stuck on false candidates.
        // Load key into container
        Span<ulong> cKey1 = stackalloc ulong[ContainerCapacityInUlongs];
        SetupContainer(cKey1, PrivateKey1);

        // Assure ParallelOptions is not null
        po ??= GetParallelOptions();

        uint? userId = null;
        byte[] pattern = [0x01, 0x0, 0x0, 0x0];
        var segmentDataFirstRow = limeSegment.SegmentData.AsSpan(0, 16);
        Span<byte> dataAsBytes = stackalloc byte[segmentDataFirstRow.Length];
        Span<ulong> cUserId = stackalloc ulong[ContainerCapacityInUlongs];
        Span<ulong> cHashPublicKeysResult = stackalloc ulong[ContainerCapacityInUlongs];
        for (var i = start; i <= end; i++)
        {
            if (po.CancellationToken.IsCancellationRequested) return userId;

            // Load userID into container
            SetupContainer(cUserId, NotUserId(i));

            // Load segment data
            segmentDataFirstRow.CopyTo(dataAsBytes);

            // Hash public keys
            HashPublicKeys(cHashPublicKeysResult, cKey1, cUserId, limeSegment.HashedKeyBanks);
            cHashPublicKeysResult = cHashPublicKeysResult[..4];

            // Deencrypt SegmentData
            var cHashPublicKeysResultAsBytes = MemoryMarshal.Cast<ulong, byte>(cHashPublicKeysResult);
            DeencryptData(dataAsBytes, cHashPublicKeysResultAsBytes);

            var result = dataAsBytes.Slice(4, 4).SequenceEqual(pattern);
            if (!result) continue;
            userId = i;
        }
        return userId;
    }
#endif

    #endregion
}