using System.Buffers;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text.Json;

namespace HashNow.Core;

/// <summary>
/// Provides methods for computing file hashes (CRC32, MD5, SHA1, SHA256, SHA512).
/// </summary>
public static class FileHasher {
private const int BufferSize = 1024 * 1024; // 1MB buffer for streaming

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

var startTime = DateTime.UtcNow;

// Compute all hashes in parallel using a single file read
var (crc32, md5, sha1, sha256, sha512) = await ComputeAllHashesAsync(
filePath,
fileInfo.Length,
progress,
cancellationToken);

return new FileHashResult {
FileName = fileInfo.Name,
FullPath = fileInfo.FullName,
SizeBytes = fileInfo.Length,
SizeFormatted = FormatFileSize(fileInfo.Length),
CreatedUtc = fileInfo.CreationTimeUtc.ToString("o"),
ModifiedUtc = fileInfo.LastWriteTimeUtc.ToString("o"),
Crc32 = crc32,
Md5 = md5,
Sha1 = sha1,
Sha256 = sha256,
Sha512 = sha512,
HashedAtUtc = startTime.ToString("o")
};
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

private static async Task<(string crc32, string md5, string sha1, string sha256, string sha512)> ComputeAllHashesAsync(
string filePath,
long fileSize,
Action<double>? progress,
CancellationToken cancellationToken) {
var crc32 = new Crc32();
using var md5 = MD5.Create();
using var sha1 = SHA1.Create();
using var sha256 = SHA256.Create();
using var sha512 = SHA512.Create();

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

// Update all hash algorithms
crc32.Append(chunk);
md5.TransformBlock(buffer, 0, bytesRead, null, 0);
sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
sha512.TransformBlock(buffer, 0, bytesRead, null, 0);

totalRead += bytesRead;
progress?.Invoke((double)totalRead / fileSize);
}

// Finalize transforms
md5.TransformFinalBlock([], 0, 0);
sha1.TransformFinalBlock([], 0, 0);
sha256.TransformFinalBlock([], 0, 0);
sha512.TransformFinalBlock([], 0, 0);

return (
crc32: Convert.ToHexStringLower(crc32.GetCurrentHash()),
md5: Convert.ToHexStringLower(md5.Hash!),
sha1: Convert.ToHexStringLower(sha1.Hash!),
sha256: Convert.ToHexStringLower(sha256.Hash!),
sha512: Convert.ToHexStringLower(sha512.Hash!)
);
} finally {
ArrayPool<byte>.Shared.Return(buffer);
}
}

private static string FormatFileSize(long bytes) {
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
