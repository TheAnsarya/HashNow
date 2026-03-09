namespace HashNow.Core.Tests;

/// <summary>
/// Edge case and boundary condition tests for hash algorithms.
/// Tests unusual inputs, large data patterns, and boundary sizes.
/// </summary>
public class EdgeCaseTests : IDisposable {
	private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"HashNow_EdgeCase_{Guid.NewGuid():N}");

	public EdgeCaseTests() {
		Directory.CreateDirectory(_tempDir);
	}

	public void Dispose() {
		if (Directory.Exists(_tempDir)) {
			Directory.Delete(_tempDir, recursive: true);
		}
		GC.SuppressFinalize(this);
	}

	private string CreateTempFile(string name, byte[] content) {
		var path = Path.Combine(_tempDir, name);
		File.WriteAllBytes(path, content);
		return path;
	}
	#region Single Byte Variations

	[Fact]
	public void AllAlgorithms_SingleZeroByte_DifferFromEmpty() {
		byte[] empty = [];
		byte[] singleZero = [0x00];

		// Each algorithm should distinguish empty from single zero byte
		Assert.NotEqual(FileHasher.ComputeMd5(empty), FileHasher.ComputeMd5(singleZero));
		Assert.NotEqual(FileHasher.ComputeSha256(empty), FileHasher.ComputeSha256(singleZero));
		Assert.NotEqual(FileHasher.ComputeSha512(empty), FileHasher.ComputeSha512(singleZero));
		Assert.NotEqual(FileHasher.ComputeBlake3(empty), FileHasher.ComputeBlake3(singleZero));
		Assert.NotEqual(FileHasher.ComputeXxHash64(empty), FileHasher.ComputeXxHash64(singleZero));
		Assert.NotEqual(FileHasher.ComputeWhirlpool(empty), FileHasher.ComputeWhirlpool(singleZero));
	}

	[Theory]
	[InlineData(0x00)]
	[InlineData(0x01)]
	[InlineData(0x7f)]
	[InlineData(0x80)]
	[InlineData(0xfe)]
	[InlineData(0xff)]
	public void Sha256_SingleByte_ProducesUniqueResults(byte value) {
		byte[] data = [value];
		var hash = FileHasher.ComputeSha256(data);
		Assert.Equal(64, hash.Length);
		Assert.Matches("^[0-9a-f]{64}$", hash);
	}

	[Fact]
	public void AllSingleByteHashes_AreUnique() {
		// All 256 possible single bytes should produce 256 unique SHA-256 hashes
		var hashes = new HashSet<string>();
		for (int i = 0; i < 256; i++) {
			byte[] data = [(byte)i];
			hashes.Add(FileHasher.ComputeSha256(data));
		}
		Assert.Equal(256, hashes.Count);
	}

	#endregion

	#region Boundary Block Sizes

	// Many hash algorithms use 64-byte blocks (MD5, SHA-1, SHA-256, etc.)
	[Theory]
	[InlineData(63)]  // one less than block
	[InlineData(64)]  // exact block boundary
	[InlineData(65)]  // one more than block
	[InlineData(127)] // just under two blocks
	[InlineData(128)] // exact two blocks (also SHA-512 block)
	[InlineData(129)] // one over two blocks
	[InlineData(255)] // just under 4 blocks
	[InlineData(256)] // exact 4 blocks
	public void Sha256_BlockBoundaries_ProduceValidHashes(int size) {
		var data = new byte[size];
		Random.Shared.NextBytes(data);
		var hash = FileHasher.ComputeSha256(data);
		Assert.Equal(64, hash.Length);
		Assert.Matches("^[0-9a-f]{64}$", hash);
	}

	// SHA-512 uses 128-byte blocks
	[Theory]
	[InlineData(127)]
	[InlineData(128)]
	[InlineData(129)]
	[InlineData(255)]
	[InlineData(256)]
	[InlineData(257)]
	public void Sha512_BlockBoundaries_ProduceValidHashes(int size) {
		var data = new byte[size];
		Random.Shared.NextBytes(data);
		var hash = FileHasher.ComputeSha512(data);
		Assert.Equal(128, hash.Length);
		Assert.Matches("^[0-9a-f]{128}$", hash);
	}

	// SHA3 uses different block sizes (rate): SHA3-256 rate=136 bytes
	[Theory]
	[InlineData(135)]
	[InlineData(136)]
	[InlineData(137)]
	[InlineData(271)]
	[InlineData(272)]
	public void Sha3_256_RateBoundaries_ProduceValidHashes(int size) {
		var data = new byte[size];
		Random.Shared.NextBytes(data);
		var hash = FileHasher.ComputeSha3_256(data);
		Assert.Equal(64, hash.Length);
	}

	#endregion

	#region Repeated Byte Patterns

	[Theory]
	[InlineData(1)]
	[InlineData(100)]
	[InlineData(1000)]
	[InlineData(10000)]
	public void Sha256_AllZeroBuffers_ProduceDifferentHashes(int length) {
		var data = new byte[length]; // all zeros
		var hash = FileHasher.ComputeSha256(data);
		Assert.Equal(64, hash.Length);

		// Different length zero buffers should produce different hashes
		var shorterData = new byte[length > 1 ? length - 1 : 0];
		Assert.NotEqual(FileHasher.ComputeSha256(shorterData), hash);
	}

	[Fact]
	public void Sha256_AllOnesBuffer_DiffersFromAllZeros() {
		var zeros = new byte[1024];
		var ones = new byte[1024];
		Array.Fill(ones, (byte)0xff);
		Assert.NotEqual(FileHasher.ComputeSha256(zeros), FileHasher.ComputeSha256(ones));
	}

	#endregion

	#region Large Data Handling

	[Fact]
	public void AllAlgorithms_1MB_ProduceValidHashes() {
		// 1MB of random data - tests ArrayPool and buffer handling
		var data = new byte[1024 * 1024];
		Random.Shared.NextBytes(data);

		var hashes = new Dictionary<string, string> {
			["CRC32"] = FileHasher.ComputeCrc32(data),
			["MD5"] = FileHasher.ComputeMd5(data),
			["SHA256"] = FileHasher.ComputeSha256(data),
			["SHA512"] = FileHasher.ComputeSha512(data),
			["BLAKE3"] = FileHasher.ComputeBlake3(data),
			["XXHash64"] = FileHasher.ComputeXxHash64(data),
			["Whirlpool"] = FileHasher.ComputeWhirlpool(data),
		};

		foreach (var (name, hash) in hashes) {
			Assert.Matches("^[0-9a-f]+$", hash);
			Assert.True(hash.Length > 0, $"{name} produced empty hash");
		}
	}

	#endregion

	#region Determinism Under Stress

	[Fact]
	public void AllAlgorithms_RepeatedCalls_AreDeterministic() {
		var data = "determinism test input"u8.ToArray();
		const int iterations = 100;

		var md5First = FileHasher.ComputeMd5(data);
		var shaFirst = FileHasher.ComputeSha256(data);
		var blake3First = FileHasher.ComputeBlake3(data);

		for (int i = 0; i < iterations; i++) {
			Assert.Equal(md5First, FileHasher.ComputeMd5(data));
			Assert.Equal(shaFirst, FileHasher.ComputeSha256(data));
			Assert.Equal(blake3First, FileHasher.ComputeBlake3(data));
		}
	}

	#endregion

	#region Near-Collision Tests (Avalanche Effect)

	[Fact]
	public void Sha256_SingleBitFlip_ProducesDifferentHash() {
		var data1 = "Hello, World!"u8.ToArray();
		var data2 = (byte[])data1.Clone();
		data2[0] ^= 0x01; // flip one bit

		var hash1 = FileHasher.ComputeSha256(data1);
		var hash2 = FileHasher.ComputeSha256(data2);
		Assert.NotEqual(hash1, hash2);

		// Count differing hex chars - good hash should differ in roughly half
		int diffs = hash1.Zip(hash2, (a, b) => a != b ? 1 : 0).Sum();
		Assert.True(diffs >= 10, $"Avalanche effect too weak: only {diffs}/64 chars differ");
	}

	[Fact]
	public void Blake3_SingleBitFlip_ProducesDifferentHash() {
		var data1 = "BLAKE3 avalanche test"u8.ToArray();
		var data2 = (byte[])data1.Clone();
		data2[^1] ^= 0x80; // flip high bit of last byte

		Assert.NotEqual(FileHasher.ComputeBlake3(data1), FileHasher.ComputeBlake3(data2));
	}

	#endregion

	#region Null and Empty Input Safety

	[Fact]
	public void AllAlgorithms_EmptyArray_DoNotThrow() {
		byte[] empty = [];

		// These should all succeed without throwing
		FileHasher.ComputeCrc32(empty);
		FileHasher.ComputeMd5(empty);
		FileHasher.ComputeSha1(empty);
		FileHasher.ComputeSha256(empty);
		FileHasher.ComputeSha512(empty);
		FileHasher.ComputeSha3_256(empty);
		FileHasher.ComputeBlake3(empty);
		FileHasher.ComputeXxHash64(empty);
		FileHasher.ComputeWhirlpool(empty);
		FileHasher.ComputeRipemd160(empty);
		FileHasher.ComputeKeccak256(empty);
		FileHasher.ComputeSm3(empty);
		FileHasher.ComputeAdler32(empty);
		FileHasher.ComputeMurmur3_32(empty);
	}

	#endregion

	#region 1MB Buffer Boundary Tests

	[Theory]
	[InlineData(1024 * 1024 - 1)] // 1MB - 1
	[InlineData(1024 * 1024)]     // 1MB exact
	[InlineData(1024 * 1024 + 1)] // 1MB + 1
	public void Sha256_1MB_Boundaries_ProduceValidHashes(int size) {
		var data = new byte[size];
		Random.Shared.NextBytes(data);
		var hash = FileHasher.ComputeSha256(data);
		Assert.Equal(64, hash.Length);
		Assert.Matches("^[0-9a-f]{64}$", hash);
	}

	[Theory]
	[InlineData(1024 * 1024 - 1)]
	[InlineData(1024 * 1024)]
	[InlineData(1024 * 1024 + 1)]
	public async Task HashFileAsync_1MB_Boundaries_ProduceValidResults(int size) {
		var data = new byte[size];
		Random.Shared.NextBytes(data);
		var path = CreateTempFile($"boundary_{size}.bin", data);

		var result = await FileHasher.HashFileAsync(path);

		Assert.Equal(size, result.SizeBytes);
		Assert.NotNull(result.Sha256);
		Assert.Equal(64, result.Sha256.Length);
		Assert.NotNull(result.Blake3);
		Assert.NotNull(result.Md5);
	}

	#endregion

	#region Read-Only File Tests

	[Fact]
	public async Task HashFileAsync_ReadOnlyFile_Succeeds() {
		var data = "read-only file content"u8.ToArray();
		var path = CreateTempFile("readonly.bin", data);
		File.SetAttributes(path, FileAttributes.ReadOnly);

		try {
			var result = await FileHasher.HashFileAsync(path);
			Assert.Equal(data.Length, result.SizeBytes);
			Assert.NotNull(result.Sha256);
		} finally {
			File.SetAttributes(path, FileAttributes.Normal);
		}
	}

	#endregion

	#region Unicode and Special Filename Tests

	[Fact]
	public async Task HashFileAsync_UnicodeFilename_Succeeds() {
		var data = "unicode filename test"u8.ToArray();
		var path = CreateTempFile("tëst_ünïcödé_файл.bin", data);

		var result = await FileHasher.HashFileAsync(path);
		Assert.Equal(data.Length, result.SizeBytes);
		Assert.NotNull(result.Sha256);
	}

	[Fact]
	public async Task HashFileAsync_EmojiFilename_Succeeds() {
		var data = "emoji filename test"u8.ToArray();
		var path = CreateTempFile("test_🔒_hash_✅.bin", data);

		var result = await FileHasher.HashFileAsync(path);
		Assert.Equal(data.Length, result.SizeBytes);
		Assert.NotNull(result.Sha256);
	}

	[Fact]
	public async Task HashFileAsync_SpacesInFilename_Succeeds() {
		var data = "spaces in name"u8.ToArray();
		var path = CreateTempFile("file with spaces and (parens).bin", data);

		var result = await FileHasher.HashFileAsync(path);
		Assert.Equal(data.Length, result.SizeBytes);
		Assert.NotNull(result.Sha256);
	}

	#endregion

	#region All-Zeros and All-0xFF File Tests

	[Theory]
	[InlineData(0, 4096)]
	[InlineData(0xff, 4096)]
	public async Task HashFileAsync_RepeatedByte_ProduceValidResults(byte fillByte, int size) {
		var data = new byte[size];
		Array.Fill(data, fillByte);
		var path = CreateTempFile($"fill_{fillByte:x2}_{size}.bin", data);

		var result = await FileHasher.HashFileAsync(path);
		Assert.Equal(size, result.SizeBytes);
		Assert.NotNull(result.Sha256);
		Assert.NotNull(result.Blake3);
	}

	#endregion

	#region Cross-Algorithm Uniqueness

	[Fact]
	public void DifferentAlgorithms_SameInput_ProduceDifferentHashes() {
		var data = "cross-algorithm uniqueness"u8.ToArray();

		// Only compare algorithms with same digest length (256 bits = 64 hex)
		var sha256 = FileHasher.ComputeSha256(data);
		var sha3_256 = FileHasher.ComputeSha3_256(data);
		var blake2s = FileHasher.ComputeBlake2s(data);
		var keccak256 = FileHasher.ComputeKeccak256(data);
		var sm3 = FileHasher.ComputeSm3(data);

		var set = new HashSet<string> { sha256, sha3_256, blake2s, keccak256, sm3 };
		Assert.Equal(5, set.Count);
	}

	[Fact]
	public void DifferentAlgorithms_512bit_ProduceDifferentHashes() {
		var data = "512-bit uniqueness"u8.ToArray();

		var sha512 = FileHasher.ComputeSha512(data);
		var sha3_512 = FileHasher.ComputeSha3_512(data);
		var blake2b = FileHasher.ComputeBlake2b(data);
		var keccak512 = FileHasher.ComputeKeccak512(data);
		var whirlpool = FileHasher.ComputeWhirlpool(data);
		var streebog512 = FileHasher.ComputeStreebog512(data);

		var set = new HashSet<string> { sha512, sha3_512, blake2b, keccak512, whirlpool, streebog512 };
		Assert.Equal(6, set.Count);
	}

	#endregion
}
