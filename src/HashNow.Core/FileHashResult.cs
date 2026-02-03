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

// ========== Standard Hashes ==========

/// <summary>CRC32 hash (8 hex characters).</summary>
[JsonPropertyName("crc32")]
public required string Crc32 { get; init; }

/// <summary>CRC64 hash (16 hex characters).</summary>
[JsonPropertyName("crc64")]
public required string Crc64 { get; init; }

/// <summary>MD5 hash (32 hex characters).</summary>
[JsonPropertyName("md5")]
public required string Md5 { get; init; }

// ========== SHA-1 Family ==========

/// <summary>SHA1 hash (40 hex characters).</summary>
[JsonPropertyName("sha1")]
public required string Sha1 { get; init; }

// ========== SHA-2 Family ==========

/// <summary>SHA256 hash (64 hex characters).</summary>
[JsonPropertyName("sha256")]
public required string Sha256 { get; init; }

/// <summary>SHA384 hash (96 hex characters).</summary>
[JsonPropertyName("sha384")]
public required string Sha384 { get; init; }

/// <summary>SHA512 hash (128 hex characters).</summary>
[JsonPropertyName("sha512")]
public required string Sha512 { get; init; }

// ========== SHA-3 Family ==========

/// <summary>SHA3-256 hash (64 hex characters).</summary>
[JsonPropertyName("sha3_256")]
public required string Sha3_256 { get; init; }

/// <summary>SHA3-384 hash (96 hex characters).</summary>
[JsonPropertyName("sha3_384")]
public required string Sha3_384 { get; init; }

/// <summary>SHA3-512 hash (128 hex characters).</summary>
[JsonPropertyName("sha3_512")]
public required string Sha3_512 { get; init; }

// ========== Fast Non-Cryptographic Hashes ==========

/// <summary>XXHash3 64-bit hash (16 hex characters). Extremely fast.</summary>
[JsonPropertyName("xxHash3")]
public required string XxHash3 { get; init; }

/// <summary>XXHash64 hash (16 hex characters). Fast non-cryptographic hash.</summary>
[JsonPropertyName("xxHash64")]
public required string XxHash64 { get; init; }

/// <summary>XXHash128 hash (32 hex characters). Fast non-cryptographic hash.</summary>
[JsonPropertyName("xxHash128")]
public required string XxHash128 { get; init; }

// ========== Metadata ==========

/// <summary>Timestamp when hashing was performed (UTC ISO 8601).</summary>
[JsonPropertyName("hashedAtUtc")]
public required string HashedAtUtc { get; init; }

/// <summary>Duration in milliseconds to compute all hashes.</summary>
[JsonPropertyName("durationMs")]
public required long DurationMs { get; init; }

/// <summary>HashNow version that generated this result.</summary>
[JsonPropertyName("generatedBy")]
public string GeneratedBy { get; init; } = "HashNow v1.1.0";
}
