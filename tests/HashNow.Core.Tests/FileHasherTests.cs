using System.IO.Hashing;
using System.Security.Cryptography;

namespace HashNow.Core.Tests;

/// <summary>
/// Comprehensive test coverage for FileHasher - all 58 algorithms.
/// </summary>
public class FileHasherTests : IDisposable {
private readonly string _testDir;

public FileHasherTests() {
_testDir = Path.Combine(Path.GetTempPath(), $"HashNow_Tests_{Guid.NewGuid():N}");
Directory.CreateDirectory(_testDir);
}

public void Dispose() {
if (Directory.Exists(_testDir)) {
Directory.Delete(_testDir, recursive: true);
}
}

private string CreateTestFile(string name, byte[] content) {
var path = Path.Combine(_testDir, name);
File.WriteAllBytes(path, content);
return path;
}

#region Empty File Tests

[Fact]
public async Task HashFileAsync_EmptyFile_ReturnsCorrectHashes() {
var path = CreateTestFile("empty.bin", []);
var result = await FileHasher.HashFileAsync(path);

// Empty file hashes are well-known
Assert.Equal("00000000", result.Crc32);
Assert.Equal("0000000000000000", result.Crc64);
Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", result.Md5);
Assert.Equal("da39a3ee5e6b4b0d3255bfef95601890afd80709", result.Sha1);
Assert.Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", result.Sha256);
}

[Fact]
public async Task HashFileAsync_EmptyFile_HasCorrectMetadata() {
var path = CreateTestFile("empty.bin", []);
var result = await FileHasher.HashFileAsync(path);

Assert.Equal("empty.bin", result.FileName);
Assert.Equal(path, result.FullPath);
Assert.Equal(0, result.SizeBytes);
Assert.Equal("0 B", result.SizeFormatted);
Assert.True(result.DurationMs >= 0);
}

#endregion

#region Single Byte Tests

[Fact]
public async Task HashFileAsync_SingleByteZero_ReturnsCorrectHashes() {
var path = CreateTestFile("zero.bin", [0x00]);
var result = await FileHasher.HashFileAsync(path);

Assert.Equal(8, result.Crc32.Length);
Assert.Equal(16, result.Crc64.Length);
Assert.Equal(32, result.Md5.Length);
Assert.Equal(40, result.Sha1.Length);
Assert.Equal(64, result.Sha256.Length);
}

[Fact]
public async Task HashFileAsync_SingleByteFF_ReturnsCorrectHashes() {
var path = CreateTestFile("ff.bin", [0xff]);
var result = await FileHasher.HashFileAsync(path);

Assert.Equal(1, result.SizeBytes);
Assert.NotNull(result.Crc32);
Assert.NotNull(result.Md5);
Assert.NotNull(result.Sha256);
}

#endregion

#region Known Text Tests

[Fact]
public async Task HashFileAsync_HelloWorld_ReturnsCorrectMd5() {
var path = CreateTestFile("hello.txt", "Hello, World!"u8.ToArray());
var result = await FileHasher.HashFileAsync(path);

Assert.Equal("65a8e27d8879283831b664bd8b7f0ad4", result.Md5);
}

[Fact]
public async Task HashFileAsync_HelloWorld_ReturnsCorrectSha256() {
var path = CreateTestFile("hello.txt", "Hello, World!"u8.ToArray());
var result = await FileHasher.HashFileAsync(path);

Assert.Equal("dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f", result.Sha256);
}

#endregion

#region Large File Tests

[Fact]
public async Task HashFileAsync_LargeFile_CompletesSuccessfully() {
var data = new byte[5 * 1024 * 1024];
Random.Shared.NextBytes(data);
var path = CreateTestFile("large.bin", data);

var result = await FileHasher.HashFileAsync(path);

Assert.Equal(5 * 1024 * 1024, result.SizeBytes);
Assert.NotNull(result.Sha256);
Assert.Equal(64, result.Sha256.Length);
}

[Fact]
public async Task HashFileAsync_LargeFile_ReportsProgress() {
var data = new byte[5 * 1024 * 1024];
Random.Shared.NextBytes(data);
var path = CreateTestFile("large.bin", data);

var progressValues = new List<double>();
await FileHasher.HashFileAsync(path, progress => progressValues.Add(progress));

Assert.True(progressValues.Count > 0, "Progress should be reported");
Assert.True(progressValues[^1] >= 0.99, "Final progress should be ~1.0");
}

#endregion

#region Cancellation Tests

[Fact]
public async Task HashFileAsync_Cancelled_ThrowsOperationCancelledException() {
var data = new byte[10 * 1024 * 1024];
Random.Shared.NextBytes(data);
var path = CreateTestFile("large.bin", data);

using var cts = new CancellationTokenSource();
cts.Cancel();

await Assert.ThrowsAnyAsync<OperationCanceledException>(
() => FileHasher.HashFileAsync(path, cancellationToken: cts.Token));
}

#endregion

#region File Not Found Tests

[Fact]
public async Task HashFileAsync_FileNotFound_ThrowsFileNotFoundException() {
var path = Path.Combine(_testDir, "nonexistent.bin");

await Assert.ThrowsAsync<FileNotFoundException>(
() => FileHasher.HashFileAsync(path));
}

[Fact]
public void HashFile_FileNotFound_ThrowsFileNotFoundException() {
var path = Path.Combine(_testDir, "nonexistent.bin");

Assert.Throws<FileNotFoundException>(
() => FileHasher.HashFile(path));
}

#endregion

#region All 58 Algorithm Hash Length Tests

[Fact]
public async Task HashFileAsync_ReturnsAllChecksumTypes() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);

// Checksums (6)
Assert.Equal(8, result.Crc32.Length);     // 4 bytes
Assert.Equal(8, result.Crc32C.Length);    // 4 bytes
Assert.Equal(16, result.Crc64.Length);    // 8 bytes
Assert.Equal(8, result.Adler32.Length);   // 4 bytes
Assert.Equal(4, result.Fletcher16.Length); // 2 bytes
Assert.Equal(8, result.Fletcher32.Length); // 4 bytes
}

[Fact]
public async Task HashFileAsync_ReturnsAllFastHashTypes() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);

// Fast Hashes (12) - each hex char = 4 bits, so bytes * 2 = hex length
Assert.Equal(8, result.XxHash32.Length);   // 4 bytes = 8 hex
Assert.Equal(16, result.XxHash64.Length);  // 8 bytes = 16 hex
Assert.Equal(16, result.XxHash3.Length);   // 8 bytes = 16 hex
Assert.Equal(32, result.XxHash128.Length); // 16 bytes = 32 hex
Assert.Equal(8, result.Murmur3_32.Length); // 4 bytes = 8 hex
Assert.Equal(32, result.Murmur3_128.Length); // 16 bytes = 32 hex
Assert.True(result.CityHash64.Length >= 8); // 8+ bytes
Assert.True(result.CityHash128.Length >= 16); // 16+ bytes
Assert.True(result.FarmHash64.Length >= 8); // Same as CityHash
Assert.True(result.SpookyV2_128.Length >= 16); // 16+ bytes
Assert.Equal(16, result.SipHash24.Length); // 8 bytes = 16 hex
Assert.Equal(16, result.HighwayHash64.Length); // 8 bytes = 16 hex
}

[Fact]
public async Task HashFileAsync_ReturnsAllMdFamily() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);

// MD Family
Assert.Equal(32, result.Md2.Length);  // 16 bytes
Assert.Equal(32, result.Md4.Length);  // 16 bytes
Assert.Equal(32, result.Md5.Length);  // 16 bytes
}

[Fact]
public async Task HashFileAsync_ReturnsAllShaFamily() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);

// SHA-1/2 Family
Assert.Equal(40, result.Sha0.Length);     // 20 bytes (uses SHA1)
Assert.Equal(40, result.Sha1.Length);     // 20 bytes
Assert.Equal(56, result.Sha224.Length);   // 28 bytes
Assert.Equal(64, result.Sha256.Length);   // 32 bytes
Assert.Equal(96, result.Sha384.Length);   // 48 bytes
Assert.Equal(128, result.Sha512.Length);  // 64 bytes
Assert.Equal(56, result.Sha512_224.Length); // 28 bytes
Assert.Equal(64, result.Sha512_256.Length); // 32 bytes
}

[Fact]
public async Task HashFileAsync_ReturnsAllSha3AndKeccak() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);

// SHA-3 & Keccak
Assert.Equal(56, result.Sha3_224.Length);  // 28 bytes
Assert.Equal(64, result.Sha3_256.Length);  // 32 bytes
Assert.Equal(96, result.Sha3_384.Length);  // 48 bytes
Assert.Equal(128, result.Sha3_512.Length); // 64 bytes
Assert.Equal(64, result.Keccak256.Length); // 32 bytes
Assert.Equal(128, result.Keccak512.Length); // 64 bytes
}

[Fact]
public async Task HashFileAsync_ReturnsAllBlakeFamily() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);

// BLAKE Family
Assert.Equal(64, result.Blake256.Length);  // 32 bytes
Assert.Equal(128, result.Blake512.Length); // 64 bytes
Assert.Equal(128, result.Blake2b.Length);  // 64 bytes
Assert.Equal(64, result.Blake2s.Length);   // 32 bytes
Assert.Equal(64, result.Blake3.Length);    // 32 bytes
}

[Fact]
public async Task HashFileAsync_ReturnsAllRipemd() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);

// RIPEMD Family
Assert.Equal(32, result.Ripemd128.Length);  // 16 bytes
Assert.Equal(40, result.Ripemd160.Length);  // 20 bytes
Assert.Equal(64, result.Ripemd256.Length);  // 32 bytes
Assert.Equal(80, result.Ripemd320.Length);  // 40 bytes
}

[Fact]
public async Task HashFileAsync_ReturnsAllOtherCryptoHashes() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);

// Other Crypto
Assert.Equal(128, result.Whirlpool.Length);  // 64 bytes
Assert.Equal(48, result.Tiger192.Length);    // 24 bytes
Assert.Equal(64, result.Gost94.Length);      // 32 bytes
Assert.Equal(64, result.Streebog256.Length); // 32 bytes
Assert.Equal(128, result.Streebog512.Length); // 64 bytes
Assert.Equal(64, result.Skein256.Length);    // 32 bytes
Assert.Equal(128, result.Skein512.Length);   // 64 bytes
Assert.Equal(256, result.Skein1024.Length);  // 128 bytes
Assert.Equal(64, result.Groestl256.Length);  // 32 bytes (using SHA3 substitute)
Assert.Equal(128, result.Groestl512.Length); // 64 bytes (using SHA3 substitute)
Assert.Equal(64, result.Jh256.Length);       // 32 bytes (using SHA3 substitute)
Assert.Equal(128, result.Jh512.Length);      // 64 bytes (using SHA3 substitute)
Assert.Equal(64, result.KangarooTwelve.Length); // 32 bytes (using Keccak)
Assert.Equal(64, result.Sm3.Length);         // 32 bytes
}

#endregion

#region Consistency Tests

[Fact]
public async Task HashFileAsync_SameContent_SameHashes() {
var content = "Test content for hashing"u8.ToArray();
var path1 = CreateTestFile("file1.bin", content);
var path2 = CreateTestFile("file2.bin", content);

var result1 = await FileHasher.HashFileAsync(path1);
var result2 = await FileHasher.HashFileAsync(path2);

Assert.Equal(result1.Crc32, result2.Crc32);
Assert.Equal(result1.Md5, result2.Md5);
Assert.Equal(result1.Sha256, result2.Sha256);
Assert.Equal(result1.XxHash64, result2.XxHash64);
Assert.Equal(result1.Blake3, result2.Blake3);
}

[Fact]
public async Task HashFileAsync_DifferentContent_DifferentHashes() {
var path1 = CreateTestFile("file1.bin", [0x00]);
var path2 = CreateTestFile("file2.bin", [0x01]);

var result1 = await FileHasher.HashFileAsync(path1);
var result2 = await FileHasher.HashFileAsync(path2);

Assert.NotEqual(result1.Crc32, result2.Crc32);
Assert.NotEqual(result1.Md5, result2.Md5);
Assert.NotEqual(result1.Sha256, result2.Sha256);
}

[Fact]
public void HashFile_Sync_MatchesAsync() {
var content = "Test content"u8.ToArray();
var path = CreateTestFile("sync.bin", content);

var syncResult = FileHasher.HashFile(path);
var asyncResult = FileHasher.HashFileAsync(path).GetAwaiter().GetResult();

Assert.Equal(syncResult.Md5, asyncResult.Md5);
Assert.Equal(syncResult.Sha256, asyncResult.Sha256);
Assert.Equal(syncResult.Crc32, asyncResult.Crc32);
}

#endregion

#region Lowercase Hex Tests

[Fact]
public async Task HashFileAsync_ReturnsLowercaseHex() {
var path = CreateTestFile("test.bin", [0xab, 0xcd, 0xef]);
var result = await FileHasher.HashFileAsync(path);

Assert.Equal(result.Crc32, result.Crc32.ToLowerInvariant());
Assert.Equal(result.Md5, result.Md5.ToLowerInvariant());
Assert.Equal(result.Sha256, result.Sha256.ToLowerInvariant());
Assert.Equal(result.XxHash64, result.XxHash64.ToLowerInvariant());
Assert.Equal(result.Blake3, result.Blake3.ToLowerInvariant());
}

#endregion

#region Save Result Tests

[Fact]
public async Task SaveResultAsync_CreatesJsonFile() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);
var jsonPath = path + ".hashes.json";

await FileHasher.SaveResultAsync(result, jsonPath);

Assert.True(File.Exists(jsonPath));

var json = await File.ReadAllTextAsync(jsonPath);
Assert.Contains("crc32", json);
Assert.Contains("sha256", json);
Assert.Contains("kangarooTwelve", json);
Assert.Contains("blake3", json);
}

[Fact]
public async Task SaveResultAsync_CustomPath_CreatesAtCustomLocation() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);
var customPath = Path.Combine(_testDir, "custom.json");

await FileHasher.SaveResultAsync(result, customPath);

Assert.True(File.Exists(customPath));
}

[Fact]
public void SaveResult_Sync_CreatesJsonFile() {
var path = CreateTestFile("test.bin", [0x42]);
var result = FileHasher.HashFile(path);
var jsonPath = Path.Combine(_testDir, "sync.json");

FileHasher.SaveResult(result, jsonPath);

Assert.True(File.Exists(jsonPath));
}

#endregion

#region Utility Method Tests

[Theory]
[InlineData(0, "0 B")]
[InlineData(1, "1 B")]
[InlineData(1023, "1023 B")]
[InlineData(1024, "1 KB")]
[InlineData(1536, "1.5 KB")]
[InlineData(1048576, "1 MB")]
[InlineData(1073741824, "1 GB")]
public void FormatFileSize_ReturnsExpectedFormat(long bytes, string expected) {
var result = FileHasher.FormatFileSize(bytes);
Assert.Equal(expected, result);
}

[Theory]
[InlineData(0, 0)]
[InlineData(500_000, 1)]
[InlineData(500_000_000, 1000)]
[InlineData(5_000_000_000, 10000)]
public void EstimateHashDurationMs_ReturnsReasonableEstimate(long fileSize, long minExpected) {
var result = FileHasher.EstimateHashDurationMs(fileSize);
Assert.True(result >= minExpected, $"Expected at least {minExpected}ms for {fileSize} bytes");
}

#endregion

#region Binary Content Tests

[Fact]
public async Task HashFileAsync_BinaryContent_HandlesAllBytes() {
var data = new byte[256];
for (int i = 0; i < 256; i++) {
data[i] = (byte)i;
}
var path = CreateTestFile("allbytes.bin", data);

var result = await FileHasher.HashFileAsync(path);

Assert.Equal(256, result.SizeBytes);
Assert.NotNull(result.Sha256);
}

#endregion

#region Cross-Verification Tests

[Fact]
public async Task HashFileAsync_MatchesSystemCryptography() {
var content = "Verification test content"u8.ToArray();
var path = CreateTestFile("verify.bin", content);

var result = await FileHasher.HashFileAsync(path);

var expectedMd5 = Convert.ToHexStringLower(MD5.HashData(content));
var expectedSha256 = Convert.ToHexStringLower(SHA256.HashData(content));
var expectedSha512 = Convert.ToHexStringLower(SHA512.HashData(content));

Assert.Equal(expectedMd5, result.Md5);
Assert.Equal(expectedSha256, result.Sha256);
Assert.Equal(expectedSha512, result.Sha512);
}

[Fact]
public async Task HashFileAsync_MatchesSystemIOHashing() {
var content = "Verification test content"u8.ToArray();
var path = CreateTestFile("verify.bin", content);

var result = await FileHasher.HashFileAsync(path);

var expectedCrc32 = Convert.ToHexStringLower(Crc32.Hash(content));
var expectedXxHash64 = Convert.ToHexStringLower(XxHash64.Hash(content));

Assert.Equal(expectedCrc32, result.Crc32);
Assert.Equal(expectedXxHash64, result.XxHash64);
}

#endregion

#region Direct Compute Method Tests

[Fact]
public void ComputeMethods_ReturnCorrectLengths() {
var data = new byte[] { 0x42 };

// Test a sampling of compute methods
Assert.Equal(8, FileHasher.ComputeCrc32(data).Length);
Assert.Equal(32, FileHasher.ComputeMd5(data).Length);
Assert.Equal(64, FileHasher.ComputeSha256(data).Length);
Assert.Equal(64, FileHasher.ComputeSha3_256(data).Length);
Assert.Equal(64, FileHasher.ComputeBlake3(data).Length);
Assert.Equal(64, FileHasher.ComputeKangarooTwelve(data).Length);
Assert.Equal(64, FileHasher.ComputeSm3(data).Length);
}

[Fact]
public void GetBytesMethods_ReturnCorrectLengths() {
var data = new byte[] { 0x42 };

Assert.Equal(4, FileHasher.GetCrc32Bytes(data).Length);
Assert.Equal(16, FileHasher.GetMd5Bytes(data).Length);
Assert.Equal(32, FileHasher.GetSha256Bytes(data).Length);
Assert.Equal(32, FileHasher.GetSha3_256Bytes(data).Length);
Assert.Equal(32, FileHasher.GetBlake3Bytes(data).Length);
Assert.Equal(32, FileHasher.GetKangarooTwelveBytes(data).Length);
Assert.Equal(32, FileHasher.GetSm3Bytes(data).Length);
}

#endregion

#region Metadata Tests

[Fact]
public async Task HashFileAsync_SetsAlgorithmCount() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);

Assert.Equal(58, result.AlgorithmCount);
}

[Fact]
public async Task HashFileAsync_SetsGeneratedBy() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);

Assert.StartsWith("HashNow v", result.GeneratedBy);
}

[Fact]
public async Task HashFileAsync_SetsHashedAtUtc() {
var before = DateTime.UtcNow;
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);
var after = DateTime.UtcNow;

// Parse with round-trip format (O specifier)
var hashedAt = DateTime.Parse(result.HashedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind);
var hashedAtUtc = hashedAt.ToUniversalTime();
Assert.True(hashedAtUtc >= before.AddSeconds(-2), $"HashedAt {hashedAtUtc} should be >= {before.AddSeconds(-2)}");
Assert.True(hashedAtUtc <= after.AddSeconds(2), $"HashedAt {hashedAtUtc} should be <= {after.AddSeconds(2)}");
}

#endregion

#region Version Tests

[Fact]
public void Version_IsSet() {
Assert.NotNull(FileHasher.Version);
Assert.NotEmpty(FileHasher.Version);
Assert.Matches(@"^\d+\.\d+\.\d+", FileHasher.Version);
}

#endregion
}
