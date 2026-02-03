using System.Buffers;
using System.Diagnostics;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text.Json;

namespace HashNow.Core;

/// <summary>
/// Provides methods for computing file hashes using multiple algorithms in a single pass.
/// Supports: CRC32, CRC64, MD5, SHA1, SHA256, SHA384, SHA512, XXHash3, XXHash64, XXHash128.
/// SHA3 variants are supported only on Windows 11 24H2+ or systems with OpenSSL 1.1.1+.
/// </summary>
public static class FileHasher {
private const int BufferSize = 1024 * 1024; // 1MB buffer for streaming

/// <summary>
/// Current version of HashNow.
/// </summary>
public const string Version = "1.1.0";

/// <summary>
/// Indicates whether SHA3 is supported on this platform.
/// </summary>
public static bool IsSha3Supported { get; } = CheckSha3Support();

private static bool CheckSha3Support() {
try {
using var _ = SHA3_256.Create();
return true;
} catch (PlatformNotSupportedException) {
return false;
}
}

/// <summary>
/// Computes all hashes for the specified file.
/// </summary>
/// <param name="filePath">Path to the file to hash.</param>
/// <param name="progress">Optional progress callback (0.0 to 1.0).</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>A FileHashResult containing all computed hashes and metadata.</returns>
public static async Task<FileHashResult> HashFileAsync(
string filePath,
Action<double>? progress = null,
CancellationToken cancellationToken = default) {
var fileInfo = new FileInfo(filePath);
if (!fileInfo.Exists) {
throw new FileNotFoundException("File not found", filePath);
}

var stopwatch = Stopwatch.StartNew();
var startTime = DateTime.UtcNow;

// Compute all hashes in a single file read
var hashes = await ComputeAllHashesAsync(
filePath,
fileInfo.Length,
progress,
cancellationToken);

stopwatch.Stop();

return new FileHashResult {
FileName = fileInfo.Name,
FullPath = fileInfo.FullName,
SizeBytes = fileInfo.Length,
SizeFormatted = FormatFileSize(fileInfo.Length),
CreatedUtc = fileInfo.CreationTimeUtc.ToString("o"),
ModifiedUtc = fileInfo.LastWriteTimeUtc.ToString("o"),
Crc32 = hashes.Crc32,
Crc64 = hashes.Crc64,
Md5 = hashes.Md5,
Sha1 = hashes.Sha1,
Sha256 = hashes.Sha256,
Sha384 = hashes.Sha384,
Sha512 = hashes.Sha512,
Sha3_256 = hashes.Sha3_256,
Sha3_384 = hashes.Sha3_384,
Sha3_512 = hashes.Sha3_512,
XxHash3 = hashes.XxHash3,
XxHash64 = hashes.XxHash64,
XxHash128 = hashes.XxHash128,
HashedAtUtc = startTime.ToString("o"),
DurationMs = stopwatch.ElapsedMilliseconds
};
}

/// <summary>
/// Estimates how long hashing will take based on file size.
/// </summary>
/// <param name="fileSize">File size in bytes.</param>
/// <returns>Estimated duration in milliseconds.</returns>
public static long EstimateHashDurationMs(long fileSize) {
// Rough estimate: ~500 MB/s throughput for all algorithms combined
const double bytesPerMs = 500_000;
return (long)(fileSize / bytesPerMs);
}

/// <summary>
/// Saves the hash result to a JSON file adjacent to the original file.
/// </summary>
/// <param name="result">The hash result to save.</param>
/// <param name="outputPath">Optional output path. If null, saves as {originalFile}.hashes.json</param>
public static async Task SaveResultAsync(FileHashResult result, string? outputPath = null) {
var path = outputPath ?? result.FullPath + ".hashes.json";
var options = new JsonSerializerOptions {
WriteIndented = true,
PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
var json = JsonSerializer.Serialize(result, options);
await File.WriteAllTextAsync(path, json);
}

private record struct AllHashes(
string Crc32, string Crc64, string Md5, string Sha1,
string Sha256, string Sha384, string Sha512,
string Sha3_256, string Sha3_384, string Sha3_512,
string XxHash3, string XxHash64, string XxHash128);

private static async Task<AllHashes> ComputeAllHashesAsync(
string filePath,
long fileSize,
Action<double>? progress,
CancellationToken cancellationToken) {
// Non-cryptographic (no IDisposable)
var crc32 = new Crc32();
var crc64 = new Crc64();
var xxHash3 = new XxHash3();
var xxHash64 = new XxHash64();
var xxHash128 = new XxHash128();

// Cryptographic (IDisposable)
using var md5 = MD5.Create();
using var sha1 = SHA1.Create();
using var sha256 = SHA256.Create();
using var sha384 = SHA384.Create();
using var sha512 = SHA512.Create();

// SHA3 - only create if supported
SHA3_256? sha3_256 = null;
SHA3_384? sha3_384 = null;
SHA3_512? sha3_512 = null;

if (IsSha3Supported) {
sha3_256 = SHA3_256.Create();
sha3_384 = SHA3_384.Create();
sha3_512 = SHA3_512.Create();
}

try {
var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
try {
await using var stream = new FileStream(
filePath,
FileMode.Open,
FileAccess.Read,
FileShare.Read,
BufferSize,
FileOptions.Asynchronous | FileOptions.SequentialScan);

long totalRead = 0;
int bytesRead;

while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, BufferSize), cancellationToken)) > 0) {
var chunk = buffer.AsSpan(0, bytesRead);

// Update all non-cryptographic hashes
crc32.Append(chunk);
crc64.Append(chunk);
xxHash3.Append(chunk);
xxHash64.Append(chunk);
xxHash128.Append(chunk);

// Update all cryptographic hashes
md5.TransformBlock(buffer, 0, bytesRead, null, 0);
sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
sha384.TransformBlock(buffer, 0, bytesRead, null, 0);
sha512.TransformBlock(buffer, 0, bytesRead, null, 0);

// Update SHA3 if supported
sha3_256?.TransformBlock(buffer, 0, bytesRead, null, 0);
sha3_384?.TransformBlock(buffer, 0, bytesRead, null, 0);
sha3_512?.TransformBlock(buffer, 0, bytesRead, null, 0);

totalRead += bytesRead;
progress?.Invoke(fileSize > 0 ? (double)totalRead / fileSize : 1.0);
}

// Finalize cryptographic transforms
md5.TransformFinalBlock([], 0, 0);
sha1.TransformFinalBlock([], 0, 0);
sha256.TransformFinalBlock([], 0, 0);
sha384.TransformFinalBlock([], 0, 0);
sha512.TransformFinalBlock([], 0, 0);

sha3_256?.TransformFinalBlock([], 0, 0);
sha3_384?.TransformFinalBlock([], 0, 0);
sha3_512?.TransformFinalBlock([], 0, 0);

return new AllHashes(
Crc32: Convert.ToHexStringLower(crc32.GetCurrentHash()),
Crc64: Convert.ToHexStringLower(crc64.GetCurrentHash()),
Md5: Convert.ToHexStringLower(md5.Hash!),
Sha1: Convert.ToHexStringLower(sha1.Hash!),
Sha256: Convert.ToHexStringLower(sha256.Hash!),
Sha384: Convert.ToHexStringLower(sha384.Hash!),
Sha512: Convert.ToHexStringLower(sha512.Hash!),
Sha3_256: sha3_256 is not null ? Convert.ToHexStringLower(sha3_256.Hash!) : "not supported",
Sha3_384: sha3_384 is not null ? Convert.ToHexStringLower(sha3_384.Hash!) : "not supported",
Sha3_512: sha3_512 is not null ? Convert.ToHexStringLower(sha3_512.Hash!) : "not supported",
XxHash3: Convert.ToHexStringLower(xxHash3.GetCurrentHash()),
XxHash64: Convert.ToHexStringLower(xxHash64.GetCurrentHash()),
XxHash128: Convert.ToHexStringLower(xxHash128.GetCurrentHash())
);
} finally {
ArrayPool<byte>.Shared.Return(buffer);
}
} finally {
sha3_256?.Dispose();
sha3_384?.Dispose();
sha3_512?.Dispose();
}
}

/// <summary>
/// Formats a byte count as a human-readable string.
/// </summary>
/// <param name="bytes">The byte count to format.</param>
/// <returns>A human-readable string like "1.5 MB".</returns>
public static string FormatFileSize(long bytes) {
string[] sizes = ["B", "KB", "MB", "GB", "TB", "PB"];
int order = 0;
double size = bytes;

while (size >= 1024 && order < sizes.Length - 1) {
order++;
size /= 1024;
}

return $"{size:0.##} {sizes[order]}";
}
}
