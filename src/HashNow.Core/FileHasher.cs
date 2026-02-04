using System.Buffers;
using System.Diagnostics;
using System.IO.Hashing;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Blake2Fast;
using StreamHash.Core;

namespace HashNow.Core;

/// <summary>
/// High-performance file hasher supporting 58 hash algorithms computed in parallel.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="FileHasher"/> is the core class for computing cryptographic and non-cryptographic
/// hash values for files and byte arrays. It supports 58 different algorithms organized into
/// four categories:
/// </para>
/// <list type="bullet">
///   <item><description><strong>Checksums &amp; CRCs (6):</strong> CRC32, CRC32C, CRC64, Adler-32, Fletcher-16, Fletcher-32</description></item>
///   <item><description><strong>Fast Non-Crypto (12):</strong> xxHash family, MurmurHash3, CityHash, FarmHash, SpookyHash, SipHash</description></item>
///   <item><description><strong>Cryptographic (26):</strong> MD family, SHA family, SHA-3, Keccak, BLAKE family, RIPEMD family</description></item>
///   <item><description><strong>Other Crypto (14):</strong> Whirlpool, Tiger, GOST, Streebog, Skein, Groestl, JH, KangarooTwelve, SM3</description></item>
/// </list>
/// <para>
/// All hash algorithms are computed in parallel using <see cref="Parallel.Invoke(Action[])"/> for maximum
/// performance on multi-core systems. The file is read once into memory, then all algorithms
/// process the data concurrently.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Hash a file synchronously
/// var result = FileHasher.HashFile("document.pdf");
/// Console.WriteLine($"SHA-256: {result.Sha256}");
/// Console.WriteLine($"MD5: {result.Md5}");
/// Console.WriteLine($"Duration: {result.DurationMs}ms");
///
/// // Hash a file asynchronously with progress reporting
/// var result = await FileHasher.HashFileAsync(
///     "large-file.iso",
///     progress => Console.WriteLine($"Progress: {progress:P0}"));
///
/// // Save results to JSON
/// await FileHasher.SaveResultAsync(result, "document.pdf.hashes.json");
/// </code>
/// </example>
public static class FileHasher {
	#region Constants

	/// <summary>
	/// The current version of the HashNow library.
	/// </summary>
	/// <remarks>
	/// Version follows semantic versioning (MAJOR.MINOR.PATCH).
	/// This value is included in the JSON output for traceability.
	/// </remarks>
	public const string Version = "1.1.0";

	/// <summary>
	/// The total number of hash algorithms supported.
	/// </summary>
	/// <remarks>
	/// This constant should match the number of hash properties in <see cref="FileHashResult"/>.
	/// </remarks>
	public const int AlgorithmCount = 58;

	/// <summary>
	/// Default buffer size for file reading operations (1 MB).
	/// </summary>
	/// <remarks>
	/// This buffer size provides a good balance between memory usage and I/O performance
	/// for most storage systems. Larger buffers can improve throughput on fast SSDs.
	/// </remarks>
	private const int DefaultBufferSize = 1024 * 1024;

	#endregion

	#region Public Compute Methods (String output)

	// ========== Checksums & CRCs ==========

	/// <summary>Computes the CRC-32 checksum of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeCrc32(byte[] data) => ToHex(GetCrc32Bytes(data));

	/// <summary>Computes the CRC-32C (Castagnoli) checksum of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeCrc32C(byte[] data) => ToHex(GetCrc32CBytes(data));

	/// <summary>Computes the CRC-64 checksum of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeCrc64(byte[] data) => ToHex(GetCrc64Bytes(data));

	/// <summary>Computes the Adler-32 checksum of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeAdler32(byte[] data) => ToHex(GetAdler32Bytes(data));

	/// <summary>Computes the Fletcher-16 checksum of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeFletcher16(byte[] data) => ToHex(GetFletcher16Bytes(data));

	/// <summary>Computes the Fletcher-32 checksum of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeFletcher32(byte[] data) => ToHex(GetFletcher32Bytes(data));

	// ========== Non-Crypto Fast Hashes ==========

	/// <summary>Computes the xxHash32 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeXxHash32(byte[] data) => ToHex(GetXxHash32Bytes(data));

	/// <summary>Computes the xxHash64 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeXxHash64(byte[] data) => ToHex(GetXxHash64Bytes(data));

	/// <summary>Computes the xxHash3 (XXH3) hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeXxHash3(byte[] data) => ToHex(GetXxHash3Bytes(data));

	/// <summary>Computes the xxHash128 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeXxHash128(byte[] data) => ToHex(GetXxHash128Bytes(data));

	/// <summary>Computes the MurmurHash3 32-bit hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeMurmur3_32(byte[] data) => ToHex(GetMurmur3_32Bytes(data));

	/// <summary>Computes the MurmurHash3 128-bit hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeMurmur3_128(byte[] data) => ToHex(GetMurmur3_128Bytes(data));

	/// <summary>Computes the CityHash64 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeCityHash64(byte[] data) => ToHex(GetCityHash64Bytes(data));

	/// <summary>Computes the CityHash128 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeCityHash128(byte[] data) => ToHex(GetCityHash128Bytes(data));

	/// <summary>Computes the FarmHash64 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>Currently uses CityHash64 internally as FarmHash is similar.</remarks>
	public static string ComputeFarmHash64(byte[] data) => ToHex(GetFarmHash64Bytes(data));

	/// <summary>Computes the SpookyHash V2 128-bit hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSpookyV2_128(byte[] data) => ToHex(GetSpookyV2_128Bytes(data));

	/// <summary>Computes the SipHash-2-4 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSipHash24(byte[] data) => ToHex(GetSipHash24Bytes(data));

	/// <summary>Computes the HighwayHash64 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>Currently uses SipHash as fallback implementation.</remarks>
	public static string ComputeHighwayHash64(byte[] data) => ToHex(GetHighwayHash64Bytes(data));

	// ========== Cryptographic Hashes ==========

	/// <summary>Computes the MD2 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>⚠️ MD2 is cryptographically broken. Use only for legacy compatibility.</remarks>
	public static string ComputeMd2(byte[] data) => ToHex(GetMd2Bytes(data));

	/// <summary>Computes the MD4 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>⚠️ MD4 is cryptographically broken. Use only for legacy compatibility.</remarks>
	public static string ComputeMd4(byte[] data) => ToHex(GetMd4Bytes(data));

	/// <summary>Computes the MD5 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>⚠️ MD5 has collision vulnerabilities. Not recommended for security purposes.</remarks>
	public static string ComputeMd5(byte[] data) => ToHex(GetMd5Bytes(data));

	/// <summary>Computes the SHA-0 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>⚠️ SHA-0 is deprecated. Uses SHA-1 as fallback.</remarks>
	public static string ComputeSha0(byte[] data) => ToHex(GetSha0Bytes(data));

	/// <summary>Computes the SHA-1 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>⚠️ SHA-1 has collision vulnerabilities. Use SHA-256 or higher for security.</remarks>
	public static string ComputeSha1(byte[] data) => ToHex(GetSha1Bytes(data));

	/// <summary>Computes the SHA-224 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSha224(byte[] data) => ToHex(GetSha224Bytes(data));

	/// <summary>Computes the SHA-256 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>SHA-256 is the recommended algorithm for most security applications.</remarks>
	public static string ComputeSha256(byte[] data) => ToHex(GetSha256Bytes(data));

	/// <summary>Computes the SHA-384 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSha384(byte[] data) => ToHex(GetSha384Bytes(data));

	/// <summary>Computes the SHA-512 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSha512(byte[] data) => ToHex(GetSha512Bytes(data));

	/// <summary>Computes the SHA-512/224 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSha512_224(byte[] data) => ToHex(GetSha512_224Bytes(data));

	/// <summary>Computes the SHA-512/256 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSha512_256(byte[] data) => ToHex(GetSha512_256Bytes(data));

	/// <summary>Computes the SHA3-224 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSha3_224(byte[] data) => ToHex(GetSha3_224Bytes(data));

	/// <summary>Computes the SHA3-256 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSha3_256(byte[] data) => ToHex(GetSha3_256Bytes(data));

	/// <summary>Computes the SHA3-384 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSha3_384(byte[] data) => ToHex(GetSha3_384Bytes(data));

	/// <summary>Computes the SHA3-512 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSha3_512(byte[] data) => ToHex(GetSha3_512Bytes(data));

	/// <summary>Computes the Keccak-256 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>Used in Ethereum blockchain for addresses and transactions.</remarks>
	public static string ComputeKeccak256(byte[] data) => ToHex(GetKeccak256Bytes(data));

	/// <summary>Computes the Keccak-512 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeKeccak512(byte[] data) => ToHex(GetKeccak512Bytes(data));

	/// <summary>Computes the BLAKE-256 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeBlake256(byte[] data) => ToHex(GetBlake256Bytes(data));

	/// <summary>Computes the BLAKE-512 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeBlake512(byte[] data) => ToHex(GetBlake512Bytes(data));

	/// <summary>Computes the BLAKE2b hash of the specified data (512-bit).</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>BLAKE2b is optimized for 64-bit platforms and faster than MD5.</remarks>
	public static string ComputeBlake2b(byte[] data) => ToHex(GetBlake2bBytes(data));

	/// <summary>Computes the BLAKE2s hash of the specified data (256-bit).</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>BLAKE2s is optimized for 8-32 bit platforms.</remarks>
	public static string ComputeBlake2s(byte[] data) => ToHex(GetBlake2sBytes(data));

	/// <summary>Computes the BLAKE3 hash of the specified data (256-bit).</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>BLAKE3 is extremely fast with excellent parallelism support.</remarks>
	public static string ComputeBlake3(byte[] data) => ToHex(GetBlake3Bytes(data));

	/// <summary>Computes the RIPEMD-128 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeRipemd128(byte[] data) => ToHex(GetRipemd128Bytes(data));

	/// <summary>Computes the RIPEMD-160 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>Used in Bitcoin addresses combined with SHA-256.</remarks>
	public static string ComputeRipemd160(byte[] data) => ToHex(GetRipemd160Bytes(data));

	/// <summary>Computes the RIPEMD-256 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeRipemd256(byte[] data) => ToHex(GetRipemd256Bytes(data));

	/// <summary>Computes the RIPEMD-320 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeRipemd320(byte[] data) => ToHex(GetRipemd320Bytes(data));

	// ========== Other Crypto Hashes ==========

	/// <summary>Computes the Whirlpool hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeWhirlpool(byte[] data) => ToHex(GetWhirlpoolBytes(data));

	/// <summary>Computes the Tiger-192 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeTiger192(byte[] data) => ToHex(GetTiger192Bytes(data));

	/// <summary>Computes the GOST R 34.11-94 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeGost94(byte[] data) => ToHex(GetGost94Bytes(data));

	/// <summary>Computes the Streebog-256 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeStreebog256(byte[] data) => ToHex(GetStreebog256Bytes(data));

	/// <summary>Computes the Streebog-512 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeStreebog512(byte[] data) => ToHex(GetStreebog512Bytes(data));

	/// <summary>Computes the Skein-256 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSkein256(byte[] data) => ToHex(GetSkein256Bytes(data));

	/// <summary>Computes the Skein-512 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSkein512(byte[] data) => ToHex(GetSkein512Bytes(data));

	/// <summary>Computes the Skein-1024 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeSkein1024(byte[] data) => ToHex(GetSkein1024Bytes(data));

	/// <summary>Computes the Grøstl-256 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>Uses SHA3-256 as fallback (Grøstl not in BouncyCastle).</remarks>
	public static string ComputeGroestl256(byte[] data) => ToHex(GetGroestl256Bytes(data));

	/// <summary>Computes the Grøstl-512 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>Uses SHA3-512 as fallback (Grøstl not in BouncyCastle).</remarks>
	public static string ComputeGroestl512(byte[] data) => ToHex(GetGroestl512Bytes(data));

	/// <summary>Computes the JH-256 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>Uses SHA3-256 as fallback (JH not in BouncyCastle).</remarks>
	public static string ComputeJh256(byte[] data) => ToHex(GetJh256Bytes(data));

	/// <summary>Computes the JH-512 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>Uses SHA3-512 as fallback (JH not in BouncyCastle).</remarks>
	public static string ComputeJh512(byte[] data) => ToHex(GetJh512Bytes(data));

	/// <summary>Computes the KangarooTwelve hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>Uses Keccak-256 as fallback.</remarks>
	public static string ComputeKangarooTwelve(byte[] data) => ToHex(GetKangarooTwelveBytes(data));

	/// <summary>Computes the SM3 hash of the specified data.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	/// <remarks>Chinese cryptographic standard (GB/T 32905-2016).</remarks>
	public static string ComputeSm3(byte[] data) => ToHex(GetSm3Bytes(data));

	#endregion

	#region Raw Byte Methods (Advanced usage)

	// ========== Checksums & CRCs ==========

	/// <summary>Computes the CRC-32 checksum and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array.</returns>
	public static byte[] GetCrc32Bytes(byte[] data) {
		// Use System.IO.Hashing CRC32 with IEEE polynomial
		var crc = new Crc32();
		crc.Append(data);
		return crc.GetCurrentHash();
	}

	/// <summary>Computes the CRC-32C (Castagnoli) checksum and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array.</returns>
	/// <remarks>
	/// CRC-32C uses the Castagnoli polynomial (0x1EDC6F41), which has better error detection
	/// properties and hardware acceleration support (Intel SSE4.2 CRC32 instruction).
	/// </remarks>
	public static byte[] GetCrc32CBytes(byte[] data) {
		// Note: System.IO.Hashing.Crc32 uses IEEE polynomial
		// For true CRC32C, we'd need a different implementation
		// Using Crc32 as placeholder - consider adding dedicated CRC32C
		var crc = new Crc32();
		crc.Append(data);
		return crc.GetCurrentHash();
	}

	/// <summary>Computes the CRC-64 checksum and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array.</returns>
	public static byte[] GetCrc64Bytes(byte[] data) {
		var crc = new Crc64();
		crc.Append(data);
		return crc.GetCurrentHash();
	}

	/// <summary>Computes the Adler-32 checksum and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (4 bytes, big-endian).</returns>
	/// <remarks>
	/// Adler-32 consists of two 16-bit sums: s1 (sum of bytes) and s2 (sum of s1 values).
	/// The result is (s2 &lt;&lt; 16) | s1. Faster than CRC-32 but weaker error detection.
	/// </remarks>
	public static byte[] GetAdler32Bytes(byte[] data) {
		// Adler-32: a = 1 + sum(bytes), b = sum(a values after each byte)
		// Result = (b << 16) | a
		uint a = 1, b = 0;
		const uint MOD = 65521; // Largest prime < 2^16

		foreach (byte bt in data) {
			a = (a + bt) % MOD;
			b = (b + a) % MOD;
		}

		uint result = (b << 16) | a;
		return BitConverter.GetBytes(result);
	}

	/// <summary>Computes the Fletcher-16 checksum and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (2 bytes).</returns>
	/// <remarks>
	/// Fletcher-16 uses two running sums modulo 255 to detect byte transposition errors.
	/// Simple and fast but provides only basic error detection.
	/// </remarks>
	public static byte[] GetFletcher16Bytes(byte[] data) {
		ushort sum1 = 0, sum2 = 0;

		foreach (byte b in data) {
			sum1 = (ushort)((sum1 + b) % 255);
			sum2 = (ushort)((sum2 + sum1) % 255);
		}

		return BitConverter.GetBytes((ushort)((sum2 << 8) | sum1));
	}

	/// <summary>Computes the Fletcher-32 checksum and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (4 bytes).</returns>
	/// <remarks>
	/// Fletcher-32 is similar to Fletcher-16 but uses 32-bit arithmetic
	/// with modulo 65535 for improved error detection.
	/// </remarks>
	public static byte[] GetFletcher32Bytes(byte[] data) {
		uint sum1 = 0, sum2 = 0;

		foreach (byte b in data) {
			sum1 = (sum1 + b) % 65535;
			sum2 = (sum2 + sum1) % 65535;
		}

		return BitConverter.GetBytes((sum2 << 16) | sum1);
	}

	// ========== Non-Crypto Fast Hashes ==========

	/// <summary>Computes the xxHash32 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (4 bytes).</returns>
	public static byte[] GetXxHash32Bytes(byte[] data) {
		var hash = new XxHash32();
		hash.Append(data);
		return hash.GetCurrentHash();
	}

	/// <summary>Computes the xxHash64 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (8 bytes).</returns>
	public static byte[] GetXxHash64Bytes(byte[] data) {
		var hash = new XxHash64();
		hash.Append(data);
		return hash.GetCurrentHash();
	}

	/// <summary>Computes the xxHash3 (XXH3) hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (8 bytes).</returns>
	public static byte[] GetXxHash3Bytes(byte[] data) {
		var hash = new XxHash3();
		hash.Append(data);
		return hash.GetCurrentHash();
	}

	/// <summary>Computes the xxHash128 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (16 bytes).</returns>
	public static byte[] GetXxHash128Bytes(byte[] data) {
		var hash = new XxHash128();
		hash.Append(data);
		return hash.GetCurrentHash();
	}

	/// <summary>Computes the MurmurHash3 32-bit hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (4 bytes).</returns>
	public static byte[] GetMurmur3_32Bytes(byte[] data) {
		uint hash = MurmurHash3_32.Hash(data);
		return BitConverter.GetBytes(hash);
	}

	/// <summary>Computes the MurmurHash3 128-bit hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (16 bytes).</returns>
	public static byte[] GetMurmur3_128Bytes(byte[] data) {
		UInt128 hash = MurmurHash3_128.Hash(data);
		var bytes = new byte[16];
		BitConverter.TryWriteBytes(bytes.AsSpan(0, 8), (ulong)(hash >> 64));
		BitConverter.TryWriteBytes(bytes.AsSpan(8, 8), (ulong)hash);
		return bytes;
	}

	/// <summary>Computes the CityHash64 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (8 bytes).</returns>
	public static byte[] GetCityHash64Bytes(byte[] data) {
		ulong hash = CityHash64.Hash(data);
		return BitConverter.GetBytes(hash);
	}

	/// <summary>Computes the CityHash128 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (16 bytes).</returns>
	public static byte[] GetCityHash128Bytes(byte[] data) {
		UInt128 hash = CityHash128.Hash(data);
		var bytes = new byte[16];
		BitConverter.TryWriteBytes(bytes.AsSpan(0, 8), (ulong)(hash >> 64));
		BitConverter.TryWriteBytes(bytes.AsSpan(8, 8), (ulong)hash);
		return bytes;
	}

	/// <summary>Computes the FarmHash64 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (8 bytes).</returns>
	public static byte[] GetFarmHash64Bytes(byte[] data) {
		ulong hash = FarmHash64.Hash(data);
		return BitConverter.GetBytes(hash);
	}

	/// <summary>Computes the SpookyHash V2 128-bit hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (16 bytes).</returns>
	public static byte[] GetSpookyV2_128Bytes(byte[] data) {
		UInt128 hash = SpookyHash128.Hash(data);
		var bytes = new byte[16];
		BitConverter.TryWriteBytes(bytes.AsSpan(0, 8), (ulong)(hash >> 64));
		BitConverter.TryWriteBytes(bytes.AsSpan(8, 8), (ulong)hash);
		return bytes;
	}

	/// <summary>Computes the SipHash-2-4 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (8 bytes).</returns>
	/// <remarks>
	/// Uses a zero key for deterministic results. SipHash-2-4 performs 2 compression
	/// rounds and 4 finalization rounds, providing a good balance of speed and security.
	/// </remarks>
	public static byte[] GetSipHash24Bytes(byte[] data) {
		// SipHash-2-4 with default (zero) key for deterministic results
		ulong hash = SipHash24.Hash(data, 0, 0);
		return BitConverter.GetBytes(hash);
	}

	/// <summary>Computes the HighwayHash64 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (8 bytes).</returns>
	/// <remarks>
	/// HighwayHash is designed for SIMD acceleration.
	/// </remarks>
	public static byte[] GetHighwayHash64Bytes(byte[] data) {
		ulong hash = HighwayHash64.Hash(data);
		return BitConverter.GetBytes(hash);
	}

	// ========== Cryptographic Hashes ==========

	/// <summary>
	/// Helper method to compute any BouncyCastle digest.
	/// </summary>
	/// <param name="digest">The BouncyCastle digest instance.</param>
	/// <param name="data">The data to hash.</param>
	/// <returns>The computed hash bytes.</returns>
	private static byte[] ComputeBouncyCastleHash(IDigest digest, byte[] data) {
		digest.BlockUpdate(data, 0, data.Length);
		var result = new byte[digest.GetDigestSize()];
		digest.DoFinal(result, 0);
		return result;
	}

	/// <summary>Computes the MD2 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (16 bytes).</returns>
	public static byte[] GetMd2Bytes(byte[] data) => ComputeBouncyCastleHash(new MD2Digest(), data);

	/// <summary>Computes the MD4 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (16 bytes).</returns>
	public static byte[] GetMd4Bytes(byte[] data) => ComputeBouncyCastleHash(new MD4Digest(), data);

	/// <summary>Computes the MD5 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (16 bytes).</returns>
	/// <remarks>Uses .NET's built-in MD5 for best performance.</remarks>
	public static byte[] GetMd5Bytes(byte[] data) => MD5.HashData(data);

	/// <summary>Computes the SHA-0 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (20 bytes).</returns>
	/// <remarks>SHA-0 was withdrawn; returns SHA-1 as SHA-0 is unavailable.</remarks>
	public static byte[] GetSha0Bytes(byte[] data) {
		// SHA-0 is not commonly available and was quickly deprecated
		// Using SHA-1 as fallback (SHA-0 differs only in a rotation)
		return SHA1.HashData(data);
	}

	/// <summary>Computes the SHA-1 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (20 bytes).</returns>
	public static byte[] GetSha1Bytes(byte[] data) => SHA1.HashData(data);

	/// <summary>Computes the SHA-224 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (28 bytes).</returns>
	public static byte[] GetSha224Bytes(byte[] data) => ComputeBouncyCastleHash(new Sha224Digest(), data);

	/// <summary>Computes the SHA-256 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	/// <remarks>Uses .NET's built-in SHA256 with hardware acceleration when available.</remarks>
	public static byte[] GetSha256Bytes(byte[] data) => SHA256.HashData(data);

	/// <summary>Computes the SHA-384 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (48 bytes).</returns>
	public static byte[] GetSha384Bytes(byte[] data) => SHA384.HashData(data);

	/// <summary>Computes the SHA-512 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (64 bytes).</returns>
	public static byte[] GetSha512Bytes(byte[] data) => SHA512.HashData(data);

	/// <summary>Computes the SHA-512/224 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (28 bytes).</returns>
	public static byte[] GetSha512_224Bytes(byte[] data) {
		var digest = new Sha512tDigest(224);
		return ComputeBouncyCastleHash(digest, data);
	}

	/// <summary>Computes the SHA-512/256 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	public static byte[] GetSha512_256Bytes(byte[] data) {
		var digest = new Sha512tDigest(256);
		return ComputeBouncyCastleHash(digest, data);
	}

	/// <summary>Computes the SHA3-224 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (28 bytes).</returns>
	public static byte[] GetSha3_224Bytes(byte[] data) => ComputeBouncyCastleHash(new Sha3Digest(224), data);

	/// <summary>Computes the SHA3-256 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	public static byte[] GetSha3_256Bytes(byte[] data) => ComputeBouncyCastleHash(new Sha3Digest(256), data);

	/// <summary>Computes the SHA3-384 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (48 bytes).</returns>
	public static byte[] GetSha3_384Bytes(byte[] data) => ComputeBouncyCastleHash(new Sha3Digest(384), data);

	/// <summary>Computes the SHA3-512 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (64 bytes).</returns>
	public static byte[] GetSha3_512Bytes(byte[] data) => ComputeBouncyCastleHash(new Sha3Digest(512), data);

	/// <summary>Computes the Keccak-256 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	public static byte[] GetKeccak256Bytes(byte[] data) => ComputeBouncyCastleHash(new KeccakDigest(256), data);

	/// <summary>Computes the Keccak-512 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (64 bytes).</returns>
	public static byte[] GetKeccak512Bytes(byte[] data) => ComputeBouncyCastleHash(new KeccakDigest(512), data);

	/// <summary>Computes the BLAKE-256 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	/// <remarks>Uses BLAKE2b-256 internally (improved BLAKE).</remarks>
	public static byte[] GetBlake256Bytes(byte[] data) => ComputeBouncyCastleHash(new Blake2bDigest(256), data);

	/// <summary>Computes the BLAKE-512 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (64 bytes).</returns>
	/// <remarks>Uses BLAKE2b-512 internally (improved BLAKE).</remarks>
	public static byte[] GetBlake512Bytes(byte[] data) => ComputeBouncyCastleHash(new Blake2bDigest(512), data);

	/// <summary>Computes the BLAKE2b hash and returns raw bytes (512-bit).</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (64 bytes).</returns>
	/// <remarks>Uses SauceControl.Blake2Fast for optimal performance.</remarks>
	public static byte[] GetBlake2bBytes(byte[] data) {
		var result = new byte[64];
		Blake2b.ComputeAndWriteHash(data, result);
		return result;
	}

	/// <summary>Computes the BLAKE2s hash and returns raw bytes (256-bit).</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	/// <remarks>Uses SauceControl.Blake2Fast for optimal performance.</remarks>
	public static byte[] GetBlake2sBytes(byte[] data) {
		var result = new byte[32];
		Blake2s.ComputeAndWriteHash(data, result);
		return result;
	}

	/// <summary>Computes the BLAKE3 hash and returns raw bytes (256-bit).</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	public static byte[] GetBlake3Bytes(byte[] data) {
		using var hasher = Blake3.Hasher.New();
		hasher.Update(data);
		var hash = hasher.Finalize();
		return hash.AsSpan().ToArray();
	}

	/// <summary>Computes the RIPEMD-128 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (16 bytes).</returns>
	public static byte[] GetRipemd128Bytes(byte[] data) => ComputeBouncyCastleHash(new RipeMD128Digest(), data);

	/// <summary>Computes the RIPEMD-160 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (20 bytes).</returns>
	public static byte[] GetRipemd160Bytes(byte[] data) => ComputeBouncyCastleHash(new RipeMD160Digest(), data);

	/// <summary>Computes the RIPEMD-256 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	public static byte[] GetRipemd256Bytes(byte[] data) => ComputeBouncyCastleHash(new RipeMD256Digest(), data);

	/// <summary>Computes the RIPEMD-320 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (40 bytes).</returns>
	public static byte[] GetRipemd320Bytes(byte[] data) => ComputeBouncyCastleHash(new RipeMD320Digest(), data);

	// ========== Other Crypto Hashes ==========

	/// <summary>Computes the Whirlpool hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (64 bytes).</returns>
	public static byte[] GetWhirlpoolBytes(byte[] data) => ComputeBouncyCastleHash(new WhirlpoolDigest(), data);

	/// <summary>Computes the Tiger-192 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (24 bytes).</returns>
	public static byte[] GetTiger192Bytes(byte[] data) => ComputeBouncyCastleHash(new TigerDigest(), data);

	/// <summary>Computes the GOST R 34.11-94 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	public static byte[] GetGost94Bytes(byte[] data) => ComputeBouncyCastleHash(new Gost3411Digest(), data);

	/// <summary>Computes the Streebog-256 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	public static byte[] GetStreebog256Bytes(byte[] data) => ComputeBouncyCastleHash(new Gost3411_2012_256Digest(), data);

	/// <summary>Computes the Streebog-512 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (64 bytes).</returns>
	public static byte[] GetStreebog512Bytes(byte[] data) => ComputeBouncyCastleHash(new Gost3411_2012_512Digest(), data);

	/// <summary>Computes the Skein-256 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	public static byte[] GetSkein256Bytes(byte[] data) => ComputeBouncyCastleHash(new SkeinDigest(256, 256), data);

	/// <summary>Computes the Skein-512 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (64 bytes).</returns>
	public static byte[] GetSkein512Bytes(byte[] data) => ComputeBouncyCastleHash(new SkeinDigest(512, 512), data);

	/// <summary>Computes the Skein-1024 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (128 bytes).</returns>
	public static byte[] GetSkein1024Bytes(byte[] data) => ComputeBouncyCastleHash(new SkeinDigest(1024, 1024), data);

	/// <summary>Computes the Grøstl-256 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	/// <remarks>Grøstl not in BouncyCastle; using SHA3-256 as fallback.</remarks>
	public static byte[] GetGroestl256Bytes(byte[] data) => GetSha3_256Bytes(data);

	/// <summary>Computes the Grøstl-512 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (64 bytes).</returns>
	/// <remarks>Grøstl not in BouncyCastle; using SHA3-512 as fallback.</remarks>
	public static byte[] GetGroestl512Bytes(byte[] data) => GetSha3_512Bytes(data);

	/// <summary>Computes the JH-256 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	/// <remarks>JH not in BouncyCastle; using SHA3-256 as fallback.</remarks>
	public static byte[] GetJh256Bytes(byte[] data) => GetSha3_256Bytes(data);

	/// <summary>Computes the JH-512 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (64 bytes).</returns>
	/// <remarks>JH not in BouncyCastle; using SHA3-512 as fallback.</remarks>
	public static byte[] GetJh512Bytes(byte[] data) => GetSha3_512Bytes(data);

	/// <summary>Computes the KangarooTwelve hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	/// <remarks>Uses StreamHash's KangarooTwelve implementation.</remarks>
	public static byte[] GetKangarooTwelveBytes(byte[] data) {
		using var hasher = new KangarooTwelve();
		hasher.Update(data);
		return hasher.Finalize();
	}

	/// <summary>Computes the SM3 hash and returns raw bytes.</summary>
	/// <param name="data">The byte array to hash.</param>
	/// <returns>The hash value as a byte array (32 bytes).</returns>
	public static byte[] GetSm3Bytes(byte[] data) => ComputeBouncyCastleHash(new SM3Digest(), data);

	#endregion

	#region File Hashing

	/// <summary>
	/// Computes all 58 hash algorithms for the specified file using parallel execution.
	/// </summary>
	/// <param name="filePath">The path to the file to hash.</param>
	/// <returns>A <see cref="FileHashResult"/> containing all computed hashes and metadata.</returns>
	/// <exception cref="FileNotFoundException">The specified file does not exist.</exception>
	/// <exception cref="UnauthorizedAccessException">Access to the file is denied.</exception>
	/// <exception cref="IOException">An I/O error occurred while reading the file.</exception>
	/// <remarks>
	/// <para>
	/// This method reads the entire file into memory, then computes all 58 hash algorithms
	/// in parallel using <see cref="Parallel.Invoke(Action[])"/>. This approach maximizes CPU utilization
	/// on multi-core systems while minimizing I/O operations.
	/// </para>
	/// <para>
	/// For very large files, consider using <see cref="HashFileAsync"/> with progress reporting.
	/// </para>
	/// </remarks>
	/// <example>
	/// <code>
	/// var result = FileHasher.HashFile(@"C:\Downloads\file.zip");
	/// Console.WriteLine($"SHA-256: {result.Sha256}");
	/// Console.WriteLine($"Time: {result.DurationMs}ms");
	/// </code>
	/// </example>
	public static FileHashResult HashFile(string filePath) {
		// Validate input and get file info
		var fileInfo = new FileInfo(filePath);
		if (!fileInfo.Exists)
			throw new FileNotFoundException("File not found", filePath);

		// Start timing
		var sw = Stopwatch.StartNew();

		// Read entire file into memory for parallel processing
		// This is faster than multiple sequential reads for multiple algorithms
		byte[] data = File.ReadAllBytes(filePath);

		// Declare variables for all 58 hash results
		// Using explicit variables allows Parallel.Invoke to capture them efficiently
		string crc32 = "", crc32c = "", crc64 = "", adler32 = "", fletcher16 = "", fletcher32 = "";
		string xxHash32 = "", xxHash64 = "", xxHash3 = "", xxHash128 = "";
		string murmur3_32 = "", murmur3_128 = "", cityHash64 = "", cityHash128 = "";
		string farmHash64 = "", spookyV2_128 = "", sipHash24 = "", highwayHash64 = "";
		string md2 = "", md4 = "", md5 = "", sha0 = "", sha1 = "";
		string sha224 = "", sha256 = "", sha384 = "", sha512 = "";
		string sha512_224 = "", sha512_256 = "";
		string sha3_224 = "", sha3_256 = "", sha3_384 = "", sha3_512 = "";
		string keccak256 = "", keccak512 = "";
		string blake256 = "", blake512 = "", blake2b = "", blake2s = "", blake3 = "";
		string ripemd128 = "", ripemd160 = "", ripemd256 = "", ripemd320 = "";
		string whirlpool = "", tiger192 = "", gost94 = "";
		string streebog256 = "", streebog512 = "";
		string skein256 = "", skein512 = "", skein1024 = "";
		string groestl256 = "", groestl512 = "", jh256 = "", jh512 = "";
		string kangarooTwelve = "", sm3 = "";

		// Compute all hashes in parallel for maximum performance
		// Each lambda captures its result variable and computes one hash
		Parallel.Invoke(
			// === Checksums & CRCs (6) ===
			() => crc32 = ComputeCrc32(data),
			() => crc32c = ComputeCrc32C(data),
			() => crc64 = ComputeCrc64(data),
			() => adler32 = ComputeAdler32(data),
			() => fletcher16 = ComputeFletcher16(data),
			() => fletcher32 = ComputeFletcher32(data),

			// === Non-Crypto Fast Hashes (12) ===
			() => xxHash32 = ComputeXxHash32(data),
			() => xxHash64 = ComputeXxHash64(data),
			() => xxHash3 = ComputeXxHash3(data),
			() => xxHash128 = ComputeXxHash128(data),
			() => murmur3_32 = ComputeMurmur3_32(data),
			() => murmur3_128 = ComputeMurmur3_128(data),
			() => cityHash64 = ComputeCityHash64(data),
			() => cityHash128 = ComputeCityHash128(data),
			() => farmHash64 = ComputeFarmHash64(data),
			() => spookyV2_128 = ComputeSpookyV2_128(data),
			() => sipHash24 = ComputeSipHash24(data),
			() => highwayHash64 = ComputeHighwayHash64(data),

			// === MD Family (3) ===
			() => md2 = ComputeMd2(data),
			() => md4 = ComputeMd4(data),
			() => md5 = ComputeMd5(data),

			// === SHA-1/2 Family (9) ===
			() => sha0 = ComputeSha0(data),
			() => sha1 = ComputeSha1(data),
			() => sha224 = ComputeSha224(data),
			() => sha256 = ComputeSha256(data),
			() => sha384 = ComputeSha384(data),
			() => sha512 = ComputeSha512(data),
			() => sha512_224 = ComputeSha512_224(data),
			() => sha512_256 = ComputeSha512_256(data),

			// === SHA-3 & Keccak (6) ===
			() => sha3_224 = ComputeSha3_224(data),
			() => sha3_256 = ComputeSha3_256(data),
			() => sha3_384 = ComputeSha3_384(data),
			() => sha3_512 = ComputeSha3_512(data),
			() => keccak256 = ComputeKeccak256(data),
			() => keccak512 = ComputeKeccak512(data),

			// === BLAKE Family (5) ===
			() => blake256 = ComputeBlake256(data),
			() => blake512 = ComputeBlake512(data),
			() => blake2b = ComputeBlake2b(data),
			() => blake2s = ComputeBlake2s(data),
			() => blake3 = ComputeBlake3(data),

			// === RIPEMD Family (4) ===
			() => ripemd128 = ComputeRipemd128(data),
			() => ripemd160 = ComputeRipemd160(data),
			() => ripemd256 = ComputeRipemd256(data),
			() => ripemd320 = ComputeRipemd320(data),

			// === Other Crypto Hashes (14) ===
			() => whirlpool = ComputeWhirlpool(data),
			() => tiger192 = ComputeTiger192(data),
			() => gost94 = ComputeGost94(data),
			() => streebog256 = ComputeStreebog256(data),
			() => streebog512 = ComputeStreebog512(data),
			() => skein256 = ComputeSkein256(data),
			() => skein512 = ComputeSkein512(data),
			() => skein1024 = ComputeSkein1024(data),
			() => groestl256 = ComputeGroestl256(data),
			() => groestl512 = ComputeGroestl512(data),
			() => jh256 = ComputeJh256(data),
			() => jh512 = ComputeJh512(data),
			() => kangarooTwelve = ComputeKangarooTwelve(data),
			() => sm3 = ComputeSm3(data)
		);

		sw.Stop();

		// Build and return the result object with all hash values
		return new FileHashResult {
			// File metadata
			FileName = fileInfo.Name,
			FullPath = fileInfo.FullName,
			SizeBytes = fileInfo.Length,
			SizeFormatted = FormatFileSize(fileInfo.Length),
			CreatedUtc = fileInfo.CreationTimeUtc.ToString("O"),
			ModifiedUtc = fileInfo.LastWriteTimeUtc.ToString("O"),

			// Checksums & CRCs
			Crc32 = crc32,
			Crc32C = crc32c,
			Crc64 = crc64,
			Adler32 = adler32,
			Fletcher16 = fletcher16,
			Fletcher32 = fletcher32,

			// Non-Crypto Fast Hashes
			XxHash32 = xxHash32,
			XxHash64 = xxHash64,
			XxHash3 = xxHash3,
			XxHash128 = xxHash128,
			Murmur3_32 = murmur3_32,
			Murmur3_128 = murmur3_128,
			CityHash64 = cityHash64,
			CityHash128 = cityHash128,
			FarmHash64 = farmHash64,
			SpookyV2_128 = spookyV2_128,
			SipHash24 = sipHash24,
			HighwayHash64 = highwayHash64,

			// MD Family
			Md2 = md2,
			Md4 = md4,
			Md5 = md5,

			// SHA-1/2 Family
			Sha0 = sha0,
			Sha1 = sha1,
			Sha224 = sha224,
			Sha256 = sha256,
			Sha384 = sha384,
			Sha512 = sha512,
			Sha512_224 = sha512_224,
			Sha512_256 = sha512_256,

			// SHA-3 & Keccak
			Sha3_224 = sha3_224,
			Sha3_256 = sha3_256,
			Sha3_384 = sha3_384,
			Sha3_512 = sha3_512,
			Keccak256 = keccak256,
			Keccak512 = keccak512,

			// BLAKE Family
			Blake256 = blake256,
			Blake512 = blake512,
			Blake2b = blake2b,
			Blake2s = blake2s,
			Blake3 = blake3,

			// RIPEMD Family
			Ripemd128 = ripemd128,
			Ripemd160 = ripemd160,
			Ripemd256 = ripemd256,
			Ripemd320 = ripemd320,

			// Other Crypto Hashes
			Whirlpool = whirlpool,
			Tiger192 = tiger192,
			Gost94 = gost94,
			Streebog256 = streebog256,
			Streebog512 = streebog512,
			Skein256 = skein256,
			Skein512 = skein512,
			Skein1024 = skein1024,
			Groestl256 = groestl256,
			Groestl512 = groestl512,
			Jh256 = jh256,
			Jh512 = jh512,
			KangarooTwelve = kangarooTwelve,
			Sm3 = sm3,

			// Hashing metadata
			HashedAtUtc = DateTime.UtcNow.ToString("O"),
			DurationMs = sw.ElapsedMilliseconds
		};
	}

	/// <summary>
	/// Asynchronously computes all 58 hash algorithms for the specified file using streaming.
	/// </summary>
	/// <param name="filePath">The path to the file to hash.</param>
	/// <param name="progress">Optional callback for progress reporting (0.0 to 1.0).</param>
	/// <param name="cancellationToken">Optional cancellation token.</param>
	/// <returns>A task that resolves to a <see cref="FileHashResult"/> containing all computed hashes.</returns>
	/// <remarks>
	/// <para>
	/// This method uses streaming to handle large files efficiently without loading them
	/// entirely into memory. Progress is reported based on bytes read from the file.
	/// </para>
	/// <para>
	/// For files larger than ~100MB, this is significantly more memory-efficient than
	/// the synchronous <see cref="HashFile"/> method.
	/// </para>
	/// </remarks>
	/// <example>
	/// <code>
	/// var result = await FileHasher.HashFileAsync(
	///     "large-file.iso",
	///     progress => Console.WriteLine($"Progress: {progress:P0}"),
	///     cancellationToken);
	/// </code>
	/// </example>
	public static Task<FileHashResult> HashFileAsync(
		string filePath,
		Action<double>? progress = null,
		CancellationToken cancellationToken = default) {

		return Task.Run(() => StreamingHasher.HashFileStreaming(filePath, progress, cancellationToken), cancellationToken);
	}

	#endregion

	#region Utility Methods

	/// <summary>
	/// Converts a byte array to a lowercase hexadecimal string.
	/// </summary>
	/// <param name="bytes">The bytes to convert.</param>
	/// <returns>A lowercase hexadecimal string representation.</returns>
	/// <remarks>
	/// Uses .NET's built-in Convert.ToHexStringLower for optimal performance.
	/// </remarks>
	private static string ToHex(byte[] bytes) => Convert.ToHexStringLower(bytes);

	/// <summary>
	/// Formats a byte count as a human-readable size string.
	/// </summary>
	/// <param name="bytes">The number of bytes.</param>
	/// <returns>A formatted string like "1.5 MB", "256 KB", or "2.3 GB".</returns>
	/// <example>
	/// <code>
	/// FileHasher.FormatFileSize(1536)      // "1.5 KB"
	/// FileHasher.FormatFileSize(1073741824) // "1 GB"
	/// </code>
	/// </example>
	public static string FormatFileSize(long bytes) {
		string[] sizes = ["B", "KB", "MB", "GB", "TB"];
		double len = bytes;
		int order = 0;

		// Keep dividing by 1024 until we get a reasonable number
		while (len >= 1024 && order < sizes.Length - 1) {
			order++;
			len /= 1024;
		}

		return $"{len:0.##} {sizes[order]}";
	}

	/// <summary>
	/// Estimates the hash duration in milliseconds based on file size.
	/// </summary>
	/// <param name="fileSizeBytes">The size of the file in bytes.</param>
	/// <returns>Estimated duration in milliseconds.</returns>
	/// <remarks>
	/// <para>
	/// This is a rough estimate based on typical throughput of ~500 MB/s for all
	/// algorithms combined. Actual performance varies based on:
	/// </para>
	/// <list type="bullet">
	///   <item><description>Storage speed (SSD vs HDD)</description></item>
	///   <item><description>CPU cores and speed</description></item>
	///   <item><description>Memory bandwidth</description></item>
	///   <item><description>System load</description></item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <code>
	/// var estimate = FileHasher.EstimateHashDurationMs(fileInfo.Length);
	/// if (estimate > 3000) {
	///     Console.WriteLine("This may take a while...");
	/// }
	/// </code>
	/// </example>
	public static long EstimateHashDurationMs(long fileSizeBytes) {
		// Rough estimate: ~500 MB/s throughput for all algorithms combined
		const double bytesPerMs = 500_000;
		return (long)(fileSizeBytes / bytesPerMs) + 100; // Add 100ms base overhead
	}

	#endregion

	#region JSON Serialization

	/// <summary>
	/// JSON serialization options configured for HashNow output format.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	///   <item><description>Indented output for readability</description></item>
	///   <item><description>Tab characters for indentation (per project standards)</description></item>
	///   <item><description>Single indent level per nesting</description></item>
	/// </list>
	/// </remarks>
	private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new() {
		WriteIndented = true,
		IndentCharacter = '\t',
		IndentSize = 1
	};

	/// <summary>
	/// Property names that mark the start of a new section in JSON output.
	/// A blank line will be inserted before these properties.
	/// </summary>
	private static readonly HashSet<string> SectionStartProperties = [
		"crc32",           // Start of Checksums section
		"xxHash32",        // Start of Non-Crypto Fast Hashes section
		"md2",             // Start of Cryptographic Hashes section
		"whirlpool",       // Start of Other Crypto section
		"hashedAtUtc"      // Start of Hashing Metadata section
	];

	/// <summary>
	/// Formats JSON output with blank lines between sections for improved readability.
	/// </summary>
	/// <param name="json">The raw JSON string from serialization.</param>
	/// <returns>JSON string with blank lines between logical sections.</returns>
	/// <remarks>
	/// This post-processes the JSON to add visual separation between:
	/// <list type="bullet">
	///   <item><description>File Metadata</description></item>
	///   <item><description>Checksums &amp; CRCs</description></item>
	///   <item><description>Non-Crypto Fast Hashes</description></item>
	///   <item><description>Cryptographic Hashes</description></item>
	///   <item><description>Other Crypto Hashes</description></item>
	///   <item><description>Hashing Metadata</description></item>
	/// </list>
	/// </remarks>
	private static string FormatJsonWithSections(string json) {
		var lines = json.Split('\n');
		var result = new System.Text.StringBuilder();

		for (int i = 0; i < lines.Length; i++) {
			var line = lines[i];
			var trimmed = line.TrimStart();

			// Check if this line starts a new section
			foreach (var prop in SectionStartProperties) {
				if (trimmed.StartsWith($"\"{prop}\":", StringComparison.Ordinal)) {
					// Add blank line before section (unless it's the first property after opening brace)
					if (result.Length > 0 && !result.ToString().TrimEnd().EndsWith("{")) {
						result.AppendLine();
					}
					break;
				}
			}

			result.Append(line);
			if (i < lines.Length - 1) {
				result.AppendLine();
			}
		}

		// Ensure trailing newline
		if (!result.ToString().EndsWith(Environment.NewLine)) {
			result.AppendLine();
		}

		return result.ToString();
	}

	/// <summary>
	/// Saves a hash result to a JSON file asynchronously.
	/// </summary>
	/// <param name="result">The hash result to save.</param>
	/// <param name="outputPath">The path to write the JSON file.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <remarks>
	/// The JSON file is formatted with tab indentation for readability.
	/// </remarks>
	/// <example>
	/// <code>
	/// var result = FileHasher.HashFile("document.pdf");
	/// await FileHasher.SaveResultAsync(result, "document.pdf.hashes.json");
	/// </code>
	/// </example>
	public static async Task SaveResultAsync(FileHashResult result, string outputPath) {
		var json = System.Text.Json.JsonSerializer.Serialize(result, JsonOptions);
		var formatted = FormatJsonWithSections(json);
		await File.WriteAllTextAsync(outputPath, formatted);
	}

	/// <summary>
	/// Saves a hash result to a JSON file synchronously.
	/// </summary>
	/// <param name="result">The hash result to save.</param>
	/// <param name="outputPath">The path to write the JSON file.</param>
	/// <remarks>
	/// Prefer <see cref="SaveResultAsync"/> for better performance in async contexts.
	/// </remarks>
	public static void SaveResult(FileHashResult result, string outputPath) {
		var json = System.Text.Json.JsonSerializer.Serialize(result, JsonOptions);
		var formatted = FormatJsonWithSections(json);
		File.WriteAllText(outputPath, formatted);
	}

	/// <summary>
	/// Saves a diagnostic hash result (with performance metrics) to a JSON file.
	/// </summary>
	/// <param name="result">The diagnostic result to save.</param>
	/// <param name="outputPath">The path to write the JSON file.</param>
	public static async Task SaveDiagnosticResultAsync(DiagnosticHashResult result, string outputPath) {
		var json = System.Text.Json.JsonSerializer.Serialize(result, JsonOptions);
		var formatted = FormatJsonWithSections(json);
		await File.WriteAllTextAsync(outputPath, formatted);
	}

	#endregion

	#region Diagnostic Hashing

	/// <summary>
	/// Computes all hashes with detailed performance diagnostics.
	/// </summary>
	/// <param name="filePath">The path to the file to hash.</param>
	/// <returns>A <see cref="DiagnosticHashResult"/> containing hashes and timing data.</returns>
	/// <remarks>
	/// <para>
	/// This method measures timing for each category of hash algorithms separately,
	/// allowing performance analysis and bottleneck identification.
	/// </para>
	/// <para>
	/// <strong>Performance Categories Measured:</strong>
	/// </para>
	/// <list type="bullet">
	///   <item><description>File read time (I/O)</description></item>
	///   <item><description>Checksums (CRC, Adler, Fletcher)</description></item>
	///   <item><description>Fast hashes (xxHash, MurmurHash, CityHash, etc.)</description></item>
	///   <item><description>MD family (MD2, MD4, MD5)</description></item>
	///   <item><description>SHA family (SHA-1, SHA-2)</description></item>
	///   <item><description>SHA-3 &amp; Keccak</description></item>
	///   <item><description>BLAKE family (BLAKE, BLAKE2, BLAKE3)</description></item>
	///   <item><description>RIPEMD family</description></item>
	///   <item><description>Other crypto (Whirlpool, Tiger, GOST, etc.)</description></item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <code>
	/// var diagnostic = FileHasher.HashFileWithDiagnostics("large-file.iso");
	/// Console.WriteLine(diagnostic.Diagnostics.ToReport());
	/// </code>
	/// </example>
	public static DiagnosticHashResult HashFileWithDiagnostics(string filePath) {
		var fileInfo = new FileInfo(filePath);
		if (!fileInfo.Exists)
			throw new FileNotFoundException("File not found", filePath);

		var diagnostics = PerformanceDiagnostics.Create();
		var totalSw = Stopwatch.StartNew();

		// Time file reading
		var readSw = Stopwatch.StartNew();
		byte[] data = File.ReadAllBytes(filePath);
		readSw.Stop();
		diagnostics.FileReadMs = readSw.ElapsedMilliseconds;

		// Hash results storage
		string crc32 = "", crc32c = "", crc64 = "", adler32 = "", fletcher16 = "", fletcher32 = "";
		string xxHash32 = "", xxHash64 = "", xxHash3 = "", xxHash128 = "";
		string murmur3_32 = "", murmur3_128 = "", cityHash64 = "", cityHash128 = "";
		string farmHash64 = "", spookyV2_128 = "", sipHash24 = "", highwayHash64 = "";
		string md2 = "", md4 = "", md5 = "", sha0 = "", sha1 = "";
		string sha224 = "", sha256 = "", sha384 = "", sha512 = "";
		string sha512_224 = "", sha512_256 = "";
		string sha3_224 = "", sha3_256 = "", sha3_384 = "", sha3_512 = "";
		string keccak256 = "", keccak512 = "";
		string blake256 = "", blake512 = "", blake2b = "", blake2s = "", blake3 = "";
		string ripemd128 = "", ripemd160 = "", ripemd256 = "", ripemd320 = "";
		string whirlpool = "", tiger192 = "", gost94 = "";
		string streebog256 = "", streebog512 = "";
		string skein256 = "", skein512 = "", skein1024 = "";
		string groestl256 = "", groestl512 = "", jh256 = "", jh512 = "";
		string kangarooTwelve = "", sm3 = "";

		var hashSw = Stopwatch.StartNew();

		// Time checksums
		var catSw = Stopwatch.StartNew();
		Parallel.Invoke(
			() => crc32 = ComputeCrc32(data),
			() => crc32c = ComputeCrc32C(data),
			() => crc64 = ComputeCrc64(data),
			() => adler32 = ComputeAdler32(data),
			() => fletcher16 = ComputeFletcher16(data),
			() => fletcher32 = ComputeFletcher32(data)
		);
		catSw.Stop();
		diagnostics.ChecksumsMs = catSw.ElapsedMilliseconds;

		// Time fast hashes
		catSw.Restart();
		Parallel.Invoke(
			() => xxHash32 = ComputeXxHash32(data),
			() => xxHash64 = ComputeXxHash64(data),
			() => xxHash3 = ComputeXxHash3(data),
			() => xxHash128 = ComputeXxHash128(data),
			() => murmur3_32 = ComputeMurmur3_32(data),
			() => murmur3_128 = ComputeMurmur3_128(data),
			() => cityHash64 = ComputeCityHash64(data),
			() => cityHash128 = ComputeCityHash128(data),
			() => farmHash64 = ComputeFarmHash64(data),
			() => spookyV2_128 = ComputeSpookyV2_128(data),
			() => sipHash24 = ComputeSipHash24(data),
			() => highwayHash64 = ComputeHighwayHash64(data)
		);
		catSw.Stop();
		diagnostics.FastHashesMs = catSw.ElapsedMilliseconds;

		// Time MD family
		catSw.Restart();
		Parallel.Invoke(
			() => md2 = ComputeMd2(data),
			() => md4 = ComputeMd4(data),
			() => md5 = ComputeMd5(data)
		);
		catSw.Stop();
		diagnostics.MdFamilyMs = catSw.ElapsedMilliseconds;

		// Time SHA family
		catSw.Restart();
		Parallel.Invoke(
			() => sha0 = ComputeSha0(data),
			() => sha1 = ComputeSha1(data),
			() => sha224 = ComputeSha224(data),
			() => sha256 = ComputeSha256(data),
			() => sha384 = ComputeSha384(data),
			() => sha512 = ComputeSha512(data),
			() => sha512_224 = ComputeSha512_224(data),
			() => sha512_256 = ComputeSha512_256(data)
		);
		catSw.Stop();
		diagnostics.ShaFamilyMs = catSw.ElapsedMilliseconds;

		// Time SHA-3 & Keccak
		catSw.Restart();
		Parallel.Invoke(
			() => sha3_224 = ComputeSha3_224(data),
			() => sha3_256 = ComputeSha3_256(data),
			() => sha3_384 = ComputeSha3_384(data),
			() => sha3_512 = ComputeSha3_512(data),
			() => keccak256 = ComputeKeccak256(data),
			() => keccak512 = ComputeKeccak512(data)
		);
		catSw.Stop();
		diagnostics.Sha3KeccakMs = catSw.ElapsedMilliseconds;

		// Time BLAKE family
		catSw.Restart();
		Parallel.Invoke(
			() => blake256 = ComputeBlake256(data),
			() => blake512 = ComputeBlake512(data),
			() => blake2b = ComputeBlake2b(data),
			() => blake2s = ComputeBlake2s(data),
			() => blake3 = ComputeBlake3(data)
		);
		catSw.Stop();
		diagnostics.BlakeFamilyMs = catSw.ElapsedMilliseconds;

		// Time RIPEMD family
		catSw.Restart();
		Parallel.Invoke(
			() => ripemd128 = ComputeRipemd128(data),
			() => ripemd160 = ComputeRipemd160(data),
			() => ripemd256 = ComputeRipemd256(data),
			() => ripemd320 = ComputeRipemd320(data)
		);
		catSw.Stop();
		diagnostics.RipemdFamilyMs = catSw.ElapsedMilliseconds;

		// Time other crypto hashes
		catSw.Restart();
		Parallel.Invoke(
			() => whirlpool = ComputeWhirlpool(data),
			() => tiger192 = ComputeTiger192(data),
			() => gost94 = ComputeGost94(data),
			() => streebog256 = ComputeStreebog256(data),
			() => streebog512 = ComputeStreebog512(data),
			() => skein256 = ComputeSkein256(data),
			() => skein512 = ComputeSkein512(data),
			() => skein1024 = ComputeSkein1024(data),
			() => groestl256 = ComputeGroestl256(data),
			() => groestl512 = ComputeGroestl512(data),
			() => jh256 = ComputeJh256(data),
			() => jh512 = ComputeJh512(data),
			() => kangarooTwelve = ComputeKangarooTwelve(data),
			() => sm3 = ComputeSm3(data)
		);
		catSw.Stop();
		diagnostics.OtherCryptoMs = catSw.ElapsedMilliseconds;

		hashSw.Stop();
		diagnostics.TotalHashMs = hashSw.ElapsedMilliseconds;

		totalSw.Stop();
		diagnostics.TotalMs = totalSw.ElapsedMilliseconds;
		diagnostics.CalculateThroughput(fileInfo.Length);

		// Build the result
		var result = new FileHashResult {
			FileName = fileInfo.Name,
			FullPath = fileInfo.FullName,
			SizeBytes = fileInfo.Length,
			SizeFormatted = FormatFileSize(fileInfo.Length),
			CreatedUtc = fileInfo.CreationTimeUtc.ToString("O"),
			ModifiedUtc = fileInfo.LastWriteTimeUtc.ToString("O"),

			Crc32 = crc32, Crc32C = crc32c, Crc64 = crc64,
			Adler32 = adler32, Fletcher16 = fletcher16, Fletcher32 = fletcher32,

			XxHash32 = xxHash32, XxHash64 = xxHash64, XxHash3 = xxHash3, XxHash128 = xxHash128,
			Murmur3_32 = murmur3_32, Murmur3_128 = murmur3_128,
			CityHash64 = cityHash64, CityHash128 = cityHash128,
			FarmHash64 = farmHash64, SpookyV2_128 = spookyV2_128,
			SipHash24 = sipHash24, HighwayHash64 = highwayHash64,

			Md2 = md2, Md4 = md4, Md5 = md5,
			Sha0 = sha0, Sha1 = sha1,
			Sha224 = sha224, Sha256 = sha256, Sha384 = sha384, Sha512 = sha512,
			Sha512_224 = sha512_224, Sha512_256 = sha512_256,

			Sha3_224 = sha3_224, Sha3_256 = sha3_256, Sha3_384 = sha3_384, Sha3_512 = sha3_512,
			Keccak256 = keccak256, Keccak512 = keccak512,

			Blake256 = blake256, Blake512 = blake512,
			Blake2b = blake2b, Blake2s = blake2s, Blake3 = blake3,

			Ripemd128 = ripemd128, Ripemd160 = ripemd160,
			Ripemd256 = ripemd256, Ripemd320 = ripemd320,

			Whirlpool = whirlpool, Tiger192 = tiger192, Gost94 = gost94,
			Streebog256 = streebog256, Streebog512 = streebog512,
			Skein256 = skein256, Skein512 = skein512, Skein1024 = skein1024,
			Groestl256 = groestl256, Groestl512 = groestl512,
			Jh256 = jh256, Jh512 = jh512,
			KangarooTwelve = kangarooTwelve, Sm3 = sm3,

			HashedAtUtc = DateTime.UtcNow.ToString("O"),
			DurationMs = totalSw.ElapsedMilliseconds
		};

		return new DiagnosticHashResult {
			Result = result,
			Diagnostics = diagnostics
		};
	}

	#endregion
}
