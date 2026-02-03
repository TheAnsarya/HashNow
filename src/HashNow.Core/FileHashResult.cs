using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HashNow.Core;

/// <summary>
/// Represents the result of hashing a file, containing all computed hashes and file metadata.
/// </summary>
public sealed class FileHashResult {
/// <summary>File name without path.</summary>
[JsonPropertyName("fileName")]
public required string FileName { get; init; }

/// <summary>Full path to the original file.</summary>
[JsonPropertyName("fullPath")]
public required string FullPath { get; init; }

/// <summary>File size in bytes.</summary>
[JsonPropertyName("sizeBytes")]
public required long SizeBytes { get; init; }

/// <summary>Human-readable file size.</summary>
[JsonPropertyName("sizeFormatted")]
public required string SizeFormatted { get; init; }

/// <summary>File creation timestamp (UTC ISO 8601).</summary>
[JsonPropertyName("createdUtc")]
public required string CreatedUtc { get; init; }

/// <summary>File last modified timestamp (UTC ISO 8601).</summary>
[JsonPropertyName("modifiedUtc")]
public required string ModifiedUtc { get; init; }

/// <summary>CRC32 hash (8 hex characters).</summary>
[JsonPropertyName("crc32")]
public required string Crc32 { get; init; }

/// <summary>MD5 hash (32 hex characters).</summary>
[JsonPropertyName("md5")]
public required string Md5 { get; init; }

/// <summary>SHA1 hash (40 hex characters).</summary>
[JsonPropertyName("sha1")]
public required string Sha1 { get; init; }

/// <summary>SHA256 hash (64 hex characters).</summary>
[JsonPropertyName("sha256")]
public required string Sha256 { get; init; }

/// <summary>SHA512 hash (128 hex characters).</summary>
[JsonPropertyName("sha512")]
public required string Sha512 { get; init; }

/// <summary>Timestamp when hashing was performed (UTC ISO 8601).</summary>
[JsonPropertyName("hashedAtUtc")]
public required string HashedAtUtc { get; init; }

/// <summary>HashNow version that generated this result.</summary>
[JsonPropertyName("generatedBy")]
public string GeneratedBy { get; init; } = "HashNow v1.0.0";
}
