using System.Text.Json.Serialization;

namespace HashNow.Core;

/// <summary>
/// Represents the result of hashing a file, containing all computed hashes and file metadata.
/// Supports 58 hash algorithms across 4 categories.
/// </summary>
public sealed class FileHashResult {
	// ========== File Metadata ==========
	[JsonPropertyName("fileName")] public required string FileName { get; init; }
	[JsonPropertyName("fullPath")] public required string FullPath { get; init; }
	[JsonPropertyName("sizeBytes")] public required long SizeBytes { get; init; }
	[JsonPropertyName("sizeFormatted")] public required string SizeFormatted { get; init; }
	[JsonPropertyName("createdUtc")] public required string CreatedUtc { get; init; }
	[JsonPropertyName("modifiedUtc")] public required string ModifiedUtc { get; init; }

	// ========== 1. CHECKSUMS & CRCs (6) ==========
	[JsonPropertyName("crc32")] public required string Crc32 { get; init; }
	[JsonPropertyName("crc32c")] public required string Crc32C { get; init; }
	[JsonPropertyName("crc64")] public required string Crc64 { get; init; }
	[JsonPropertyName("adler32")] public required string Adler32 { get; init; }
	[JsonPropertyName("fletcher16")] public required string Fletcher16 { get; init; }
	[JsonPropertyName("fletcher32")] public required string Fletcher32 { get; init; }

	// ========== 2. NON-CRYPTO FAST HASHES (12) ==========
	[JsonPropertyName("xxHash32")] public required string XxHash32 { get; init; }
	[JsonPropertyName("xxHash64")] public required string XxHash64 { get; init; }
	[JsonPropertyName("xxHash3")] public required string XxHash3 { get; init; }
	[JsonPropertyName("xxHash128")] public required string XxHash128 { get; init; }
	[JsonPropertyName("murmur3_32")] public required string Murmur3_32 { get; init; }
	[JsonPropertyName("murmur3_128")] public required string Murmur3_128 { get; init; }
	[JsonPropertyName("cityHash64")] public required string CityHash64 { get; init; }
	[JsonPropertyName("cityHash128")] public required string CityHash128 { get; init; }
	[JsonPropertyName("farmHash64")] public required string FarmHash64 { get; init; }
	[JsonPropertyName("spookyV2_128")] public required string SpookyV2_128 { get; init; }
	[JsonPropertyName("sipHash24")] public required string SipHash24 { get; init; }
	[JsonPropertyName("highwayHash64")] public required string HighwayHash64 { get; init; }

	// ========== 3. CRYPTOGRAPHIC HASHES (26) ==========
	[JsonPropertyName("md2")] public required string Md2 { get; init; }
	[JsonPropertyName("md4")] public required string Md4 { get; init; }
	[JsonPropertyName("md5")] public required string Md5 { get; init; }
	[JsonPropertyName("sha0")] public required string Sha0 { get; init; }
	[JsonPropertyName("sha1")] public required string Sha1 { get; init; }
	[JsonPropertyName("sha224")] public required string Sha224 { get; init; }
	[JsonPropertyName("sha256")] public required string Sha256 { get; init; }
	[JsonPropertyName("sha384")] public required string Sha384 { get; init; }
	[JsonPropertyName("sha512")] public required string Sha512 { get; init; }
	[JsonPropertyName("sha512_224")] public required string Sha512_224 { get; init; }
	[JsonPropertyName("sha512_256")] public required string Sha512_256 { get; init; }
	[JsonPropertyName("sha3_224")] public required string Sha3_224 { get; init; }
	[JsonPropertyName("sha3_256")] public required string Sha3_256 { get; init; }
	[JsonPropertyName("sha3_384")] public required string Sha3_384 { get; init; }
	[JsonPropertyName("sha3_512")] public required string Sha3_512 { get; init; }
	[JsonPropertyName("keccak256")] public required string Keccak256 { get; init; }
	[JsonPropertyName("keccak512")] public required string Keccak512 { get; init; }
	[JsonPropertyName("blake256")] public required string Blake256 { get; init; }
	[JsonPropertyName("blake512")] public required string Blake512 { get; init; }
	[JsonPropertyName("blake2b")] public required string Blake2b { get; init; }
	[JsonPropertyName("blake2s")] public required string Blake2s { get; init; }
	[JsonPropertyName("blake3")] public required string Blake3 { get; init; }
	[JsonPropertyName("ripemd128")] public required string Ripemd128 { get; init; }
	[JsonPropertyName("ripemd160")] public required string Ripemd160 { get; init; }
	[JsonPropertyName("ripemd256")] public required string Ripemd256 { get; init; }
	[JsonPropertyName("ripemd320")] public required string Ripemd320 { get; init; }

	// ========== 4. OTHER CRYPTO HASHES (14) ==========
	[JsonPropertyName("whirlpool")] public required string Whirlpool { get; init; }
	[JsonPropertyName("tiger192")] public required string Tiger192 { get; init; }
	[JsonPropertyName("gost94")] public required string Gost94 { get; init; }
	[JsonPropertyName("streebog256")] public required string Streebog256 { get; init; }
	[JsonPropertyName("streebog512")] public required string Streebog512 { get; init; }
	[JsonPropertyName("skein256")] public required string Skein256 { get; init; }
	[JsonPropertyName("skein512")] public required string Skein512 { get; init; }
	[JsonPropertyName("skein1024")] public required string Skein1024 { get; init; }
	[JsonPropertyName("groestl256")] public required string Groestl256 { get; init; }
	[JsonPropertyName("groestl512")] public required string Groestl512 { get; init; }
	[JsonPropertyName("jh256")] public required string Jh256 { get; init; }
	[JsonPropertyName("jh512")] public required string Jh512 { get; init; }
	[JsonPropertyName("kangarooTwelve")] public required string KangarooTwelve { get; init; }
	[JsonPropertyName("sm3")] public required string Sm3 { get; init; }

	// ========== Hashing Metadata ==========
	[JsonPropertyName("hashedAtUtc")] public required string HashedAtUtc { get; init; }
	[JsonPropertyName("durationMs")] public required long DurationMs { get; init; }
	[JsonPropertyName("generatedBy")] public string GeneratedBy { get; init; } = $"HashNow v{FileHasher.Version}";
	[JsonPropertyName("algorithmCount")] public int AlgorithmCount { get; init; } = 58;
}
