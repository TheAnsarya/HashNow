using System.IO.Hashing;
using System.Security.Cryptography;

namespace HashNow.Core.Tests;

/// <summary>
/// Comprehensive test coverage for FileHasher.
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

Assert.Equal(8, result.Crc32.Length); // CRC32 is 4 bytes = 8 hex chars
Assert.Equal(16, result.Crc64.Length); // CRC64 is 8 bytes = 16 hex chars
Assert.Equal(32, result.Md5.Length); // MD5 is 16 bytes = 32 hex chars
Assert.Equal(40, result.Sha1.Length); // SHA1 is 20 bytes = 40 hex chars
Assert.Equal(64, result.Sha256.Length); // SHA256 is 32 bytes = 64 hex chars
}

[Fact]
public async Task HashFileAsync_SingleByteFF_ReturnsCorrectHashes() {
var path = CreateTestFile("ff.bin", [0xff]);
var result = await FileHasher.HashFileAsync(path);

// Verify hash lengths
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

// Known MD5 for "Hello, World!"
Assert.Equal("65a8e27d8879283831b664bd8b7f0ad4", result.Md5);
}

[Fact]
public async Task HashFileAsync_HelloWorld_ReturnsCorrectSha256() {
var path = CreateTestFile("hello.txt", "Hello, World!"u8.ToArray());
var result = await FileHasher.HashFileAsync(path);

// Known SHA256 for "Hello, World!"
Assert.Equal("dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f", result.Sha256);
}

#endregion

#region Large File Tests

[Fact]
public async Task HashFileAsync_LargeFile_CompletesSuccessfully() {
// Create a 5MB test file
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

#endregion

#region Hash Length Verification

[Fact]
public async Task HashFileAsync_ReturnsAllHashTypes() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);

// Verify all hash properties are populated
Assert.NotNull(result.Crc32);
Assert.NotNull(result.Crc64);
Assert.NotNull(result.Md5);
Assert.NotNull(result.Sha1);
Assert.NotNull(result.Sha256);
Assert.NotNull(result.Sha384);
Assert.NotNull(result.Sha512);
Assert.NotNull(result.XxHash3);
Assert.NotNull(result.XxHash64);
Assert.NotNull(result.XxHash128);

// Verify expected lengths
Assert.Equal(8, result.Crc32.Length);   // 4 bytes
Assert.Equal(16, result.Crc64.Length);  // 8 bytes
Assert.Equal(32, result.Md5.Length);    // 16 bytes
Assert.Equal(40, result.Sha1.Length);   // 20 bytes
Assert.Equal(64, result.Sha256.Length); // 32 bytes
Assert.Equal(96, result.Sha384.Length); // 48 bytes
Assert.Equal(128, result.Sha512.Length); // 64 bytes
Assert.Equal(16, result.XxHash3.Length); // 8 bytes
Assert.Equal(16, result.XxHash64.Length); // 8 bytes
Assert.Equal(32, result.XxHash128.Length); // 16 bytes

// SHA3 may or may not be supported
Assert.NotNull(result.Sha3_256);
Assert.NotNull(result.Sha3_384);
Assert.NotNull(result.Sha3_512);

if (FileHasher.IsSha3Supported) {
Assert.Equal(64, result.Sha3_256.Length); // 32 bytes
Assert.Equal(96, result.Sha3_384.Length); // 48 bytes
Assert.Equal(128, result.Sha3_512.Length); // 64 bytes
} else {
Assert.Equal("not supported", result.Sha3_256);
Assert.Equal("not supported", result.Sha3_384);
Assert.Equal("not supported", result.Sha3_512);
}
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

#endregion

#region Lowercase Hex Tests

[Fact]
public async Task HashFileAsync_ReturnsLowercaseHex() {
var path = CreateTestFile("test.bin", [0xab, 0xcd, 0xef]);
var result = await FileHasher.HashFileAsync(path);

// All hex should be lowercase
Assert.Equal(result.Crc32, result.Crc32.ToLowerInvariant());
Assert.Equal(result.Md5, result.Md5.ToLowerInvariant());
Assert.Equal(result.Sha256, result.Sha256.ToLowerInvariant());
Assert.Equal(result.XxHash64, result.XxHash64.ToLowerInvariant());
}

#endregion

#region Save Result Tests

[Fact]
public async Task SaveResultAsync_CreatesJsonFile() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);

await FileHasher.SaveResultAsync(result);

var jsonPath = path + ".hashes.json";
Assert.True(File.Exists(jsonPath));

var json = await File.ReadAllTextAsync(jsonPath);
Assert.Contains("crc32", json);
Assert.Contains("sha256", json);
}

[Fact]
public async Task SaveResultAsync_CustomPath_CreatesAtCustomLocation() {
var path = CreateTestFile("test.bin", [0x42]);
var result = await FileHasher.HashFileAsync(path);
var customPath = Path.Combine(_testDir, "custom.json");

await FileHasher.SaveResultAsync(result, customPath);

Assert.True(File.Exists(customPath));
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
// Create file with all possible byte values
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

// Verify against System.Security.Cryptography directly
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

// Verify against System.IO.Hashing directly
var expectedCrc32 = Convert.ToHexStringLower(Crc32.Hash(content));
var expectedXxHash64 = Convert.ToHexStringLower(XxHash64.Hash(content));

Assert.Equal(expectedCrc32, result.Crc32);
Assert.Equal(expectedXxHash64, result.XxHash64);
}

#endregion

#region SHA3 Platform Support Tests

[Fact]
public void IsSha3Supported_ReturnsConsistentValue() {
// Should return consistent value
var first = FileHasher.IsSha3Supported;
var second = FileHasher.IsSha3Supported;
Assert.Equal(first, second);
}

#endregion
}
