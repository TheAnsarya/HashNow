namespace HashNow.Core;

/// <summary>
/// Represents the result of hashing a file with the basic five algorithms.
/// </summary>
public sealed class BasicFileHashResult {
	/// <summary>
	/// Gets the name of the hashed file.
	/// </summary>
	[JsonPropertyName("fileName")]
	public required string FileName { get; init; }

	/// <summary>
	/// Gets the full absolute file path.
	/// </summary>
	[JsonPropertyName("fullPath")]
	public required string FullPath { get; init; }

	/// <summary>
	/// Gets the file size in bytes.
	/// </summary>
	[JsonPropertyName("sizeBytes")]
	public required long SizeBytes { get; init; }

	/// <summary>
	/// Gets the file size in human-readable format.
	/// </summary>
	[JsonPropertyName("sizeFormatted")]
	public required string SizeFormatted { get; init; }

	/// <summary>
	/// Gets the file creation timestamp in UTC ISO-8601 format.
	/// </summary>
	[JsonPropertyName("createdUtc")]
	public required string CreatedUtc { get; init; }

	/// <summary>
	/// Gets the file modification timestamp in UTC ISO-8601 format.
	/// </summary>
	[JsonPropertyName("modifiedUtc")]
	public required string ModifiedUtc { get; init; }

	/// <summary>
	/// Gets the CRC-32 checksum.
	/// </summary>
	[JsonPropertyName("crc32")]
	public required string Crc32 { get; init; }

	/// <summary>
	/// Gets the MD5 hash value.
	/// </summary>
	[JsonPropertyName("md5")]
	public required string Md5 { get; init; }

	/// <summary>
	/// Gets the SHA-1 hash value.
	/// </summary>
	[JsonPropertyName("sha1")]
	public required string Sha1 { get; init; }

	/// <summary>
	/// Gets the SHA-256 hash value.
	/// </summary>
	[JsonPropertyName("sha256")]
	public required string Sha256 { get; init; }

	/// <summary>
	/// Gets the SHA-512 hash value.
	/// </summary>
	[JsonPropertyName("sha512")]
	public required string Sha512 { get; init; }

	/// <summary>
	/// Gets when hashing completed.
	/// </summary>
	[JsonPropertyName("hashedAtUtc")]
	public required string HashedAtUtc { get; init; }

	/// <summary>
	/// Gets the hashing duration in milliseconds.
	/// </summary>
	[JsonPropertyName("durationMs")]
	public required long DurationMs { get; init; }

	/// <summary>
	/// Gets the app version that generated this output.
	/// </summary>
	[JsonPropertyName("generatedBy")]
	public string GeneratedBy { get; init; } = $"HashNow v{FileHasher.Version}";

	/// <summary>
	/// Gets the number of algorithms computed for this mode.
	/// </summary>
	[JsonPropertyName("algorithmCount")]
	public int AlgorithmCount { get; init; } = FileHasher.BasicAlgorithmCount;

	/// <summary>
	/// Gets the hash mode identifier.
	/// </summary>
	[JsonPropertyName("hashMode")]
	public string HashMode { get; init; } = "basic";
}
