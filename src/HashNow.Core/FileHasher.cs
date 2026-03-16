using System.Text.Json;

namespace HashNow.Core;

/// <summary>
/// High-performance file hasher supporting 70 hash algorithms computed via StreamHash HashFacade.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="FileHasher"/> is the core class for computing cryptographic and non-cryptographic
/// hash values for files and byte arrays. It supports 70 different algorithms via StreamHash's
/// HashFacade, organized into four categories:
/// </para>
/// <list type="bullet">
///   <item><description><strong>Checksums &amp; CRCs (9):</strong> CRC32, CRC32C, CRC64, CRC16 (3 variants), Adler-32, Fletcher-16, Fletcher-32</description></item>
///   <item><description><strong>Fast Non-Crypto (22):</strong> xxHash family, MurmurHash3, CityHash, FarmHash, SpookyHash, SipHash, HighwayHash, MetroHash, wyhash, FNV-1a, DJB2, SDBM, LoseLose</description></item>
///   <item><description><strong>Cryptographic (25):</strong> MD family, SHA family, SHA-3, Keccak, BLAKE family, RIPEMD family</description></item>
///   <item><description><strong>Other Crypto (14):</strong> Whirlpool, Tiger, GOST, Streebog, Skein, Groestl, JH, KangarooTwelve, SM3</description></item>
/// </list>
/// <para>
/// All hashing is delegated to StreamHash's HashFacade for consistent implementation.
/// </para>
/// </remarks>
public static class FileHasher {
	#region Constants

	/// <summary>
	/// The current version of the HashNow library.
	/// </summary>
	public const string Version = "1.4.1";

	/// <summary>
	/// The total number of hash algorithms supported.
	/// </summary>
	public const int AlgorithmCount = 70;

	/// <summary>
	/// Default buffer size for file reading operations (1 MB).
	/// </summary>
	private const int DefaultBufferSize = 1024 * 1024;

	/// <summary>
	/// Shared JSON serializer options configured for tab-indented output.
	/// </summary>
	private static readonly JsonSerializerOptions JsonOptions = new() {
		WriteIndented = true,
		IndentCharacter = '\t',
		IndentSize = 1,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	#endregion

	#region Public Compute Methods (String output)

	/// <summary>Computes a hash using the specified algorithm.</summary>
	/// <param name="algorithm">The hash algorithm to use.</param>
	/// <param name="data">The data to hash.</param>
	/// <returns>The hash value as a lowercase hexadecimal string.</returns>
	public static string ComputeHash(HashAlgorithm algorithm, ReadOnlySpan<byte> data)
		=> HashFacade.ComputeHashHex(algorithm, data);

	/// <summary>Computes a hash using the specified algorithm and returns bytes.</summary>
	/// <param name="algorithm">The hash algorithm to use.</param>
	/// <param name="data">The data to hash.</param>
	/// <returns>The hash value as a byte array.</returns>
	public static byte[] GetHashBytes(HashAlgorithm algorithm, ReadOnlySpan<byte> data)
		=> HashFacade.ComputeHash(algorithm, data);

	// ========== Checksums & CRCs ==========

	/// <summary>Computes the CRC-32 checksum of the specified data.</summary>
	public static string ComputeCrc32(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Crc32, data);

	/// <summary>Computes the CRC-32C (Castagnoli) checksum of the specified data.</summary>
	public static string ComputeCrc32C(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Crc32C, data);

	/// <summary>Computes the CRC-64 checksum of the specified data.</summary>
	public static string ComputeCrc64(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Crc64, data);

	/// <summary>Computes the CRC-16-CCITT checksum of the specified data.</summary>
	public static string ComputeCrc16Ccitt(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Crc16Ccitt, data);

	/// <summary>Computes the CRC-16-MODBUS checksum of the specified data.</summary>
	public static string ComputeCrc16Modbus(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Crc16Modbus, data);

	/// <summary>Computes the CRC-16-USB checksum of the specified data.</summary>
	public static string ComputeCrc16Usb(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Crc16Usb, data);

	/// <summary>Computes the Adler-32 checksum of the specified data.</summary>
	public static string ComputeAdler32(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Adler32, data);

	/// <summary>Computes the Fletcher-16 checksum of the specified data.</summary>
	public static string ComputeFletcher16(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Fletcher16, data);

	/// <summary>Computes the Fletcher-32 checksum of the specified data.</summary>
	public static string ComputeFletcher32(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Fletcher32, data);

	// ========== Non-Crypto Fast Hashes ==========

	/// <summary>Computes the xxHash32 hash of the specified data.</summary>
	public static string ComputeXxHash32(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.XxHash32, data);

	/// <summary>Computes the xxHash64 hash of the specified data.</summary>
	public static string ComputeXxHash64(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.XxHash64, data);

	/// <summary>Computes the xxHash3 (XXH3) hash of the specified data.</summary>
	public static string ComputeXxHash3(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.XxHash3, data);

	/// <summary>Computes the xxHash128 hash of the specified data.</summary>
	public static string ComputeXxHash128(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.XxHash128, data);

	/// <summary>Computes the MurmurHash3 32-bit hash of the specified data.</summary>
	public static string ComputeMurmur3_32(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.MurmurHash3_32, data);

	/// <summary>Computes the MurmurHash3 128-bit hash of the specified data.</summary>
	public static string ComputeMurmur3_128(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.MurmurHash3_128, data);

	/// <summary>Computes the CityHash64 hash of the specified data.</summary>
	public static string ComputeCityHash64(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.CityHash64, data);

	/// <summary>Computes the CityHash128 hash of the specified data.</summary>
	public static string ComputeCityHash128(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.CityHash128, data);

	/// <summary>Computes the FarmHash64 hash of the specified data.</summary>
	public static string ComputeFarmHash64(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.FarmHash64, data);

	/// <summary>Computes the SpookyHash V2 128-bit hash of the specified data.</summary>
	public static string ComputeSpookyV2_128(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.SpookyHash128, data);

	/// <summary>Computes the SipHash-2-4 hash of the specified data.</summary>
	public static string ComputeSipHash24(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.SipHash24, data);

	/// <summary>Computes the HighwayHash64 hash of the specified data.</summary>
	public static string ComputeHighwayHash64(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.HighwayHash64, data);

	/// <summary>Computes the MetroHash64 hash of the specified data.</summary>
	public static string ComputeMetroHash64(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.MetroHash64, data);

	/// <summary>Computes the MetroHash128 hash of the specified data.</summary>
	public static string ComputeMetroHash128(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.MetroHash128, data);

	/// <summary>Computes the wyhash64 hash of the specified data.</summary>
	public static string ComputeWyhash64(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Wyhash64, data);

	/// <summary>Computes the FNV-1a 32-bit hash of the specified data.</summary>
	public static string ComputeFnv1a32(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Fnv1a32, data);

	/// <summary>Computes the FNV-1a 64-bit hash of the specified data.</summary>
	public static string ComputeFnv1a64(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Fnv1a64, data);

	/// <summary>Computes the DJB2 hash of the specified data.</summary>
	public static string ComputeDjb2(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Djb2, data);

	/// <summary>Computes the DJB2a (XOR variant) hash of the specified data.</summary>
	public static string ComputeDjb2a(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Djb2a, data);

	/// <summary>Computes the SDBM hash of the specified data.</summary>
	public static string ComputeSdbm(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sdbm, data);

	/// <summary>Computes the LoseLose hash of the specified data.</summary>
	public static string ComputeLoseLose(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.LoseLose, data);

	// ========== Cryptographic Hashes ==========

	/// <summary>Computes the MD2 hash of the specified data.</summary>
	public static string ComputeMd2(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Md2, data);

	/// <summary>Computes the MD4 hash of the specified data.</summary>
	public static string ComputeMd4(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Md4, data);

	/// <summary>Computes the MD5 hash of the specified data.</summary>
	public static string ComputeMd5(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Md5, data);

	/// <summary>Computes the SHA-0 hash of the specified data.</summary>
	public static string ComputeSha0(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sha0, data);

	/// <summary>Computes the SHA-1 hash of the specified data.</summary>
	public static string ComputeSha1(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sha1, data);

	/// <summary>Computes the SHA-224 hash of the specified data.</summary>
	public static string ComputeSha224(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sha224, data);

	/// <summary>Computes the SHA-256 hash of the specified data.</summary>
	public static string ComputeSha256(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sha256, data);

	/// <summary>Computes the SHA-384 hash of the specified data.</summary>
	public static string ComputeSha384(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sha384, data);

	/// <summary>Computes the SHA-512 hash of the specified data.</summary>
	public static string ComputeSha512(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sha512, data);

	/// <summary>Computes the SHA-512/224 hash of the specified data.</summary>
	public static string ComputeSha512_224(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sha512_224, data);

	/// <summary>Computes the SHA-512/256 hash of the specified data.</summary>
	public static string ComputeSha512_256(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sha512_256, data);

	/// <summary>Computes the SHA3-224 hash of the specified data.</summary>
	public static string ComputeSha3_224(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sha3_224, data);

	/// <summary>Computes the SHA3-256 hash of the specified data.</summary>
	public static string ComputeSha3_256(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sha3_256, data);

	/// <summary>Computes the SHA3-384 hash of the specified data.</summary>
	public static string ComputeSha3_384(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sha3_384, data);

	/// <summary>Computes the SHA3-512 hash of the specified data.</summary>
	public static string ComputeSha3_512(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sha3_512, data);

	/// <summary>Computes the Keccak-256 hash of the specified data.</summary>
	public static string ComputeKeccak256(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Keccak256, data);

	/// <summary>Computes the Keccak-512 hash of the specified data.</summary>
	public static string ComputeKeccak512(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Keccak512, data);

	/// <summary>Computes the BLAKE-256 hash of the specified data.</summary>
	public static string ComputeBlake256(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Blake256, data);

	/// <summary>Computes the BLAKE-512 hash of the specified data.</summary>
	public static string ComputeBlake512(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Blake512, data);

	/// <summary>Computes the BLAKE2b hash of the specified data.</summary>
	public static string ComputeBlake2b(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Blake2b, data);

	/// <summary>Computes the BLAKE2s hash of the specified data.</summary>
	public static string ComputeBlake2s(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Blake2s, data);

	/// <summary>Computes the BLAKE3 hash of the specified data.</summary>
	public static string ComputeBlake3(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Blake3, data);

	/// <summary>Computes the RIPEMD-128 hash of the specified data.</summary>
	public static string ComputeRipemd128(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Ripemd128, data);

	/// <summary>Computes the RIPEMD-160 hash of the specified data.</summary>
	public static string ComputeRipemd160(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Ripemd160, data);

	/// <summary>Computes the RIPEMD-256 hash of the specified data.</summary>
	public static string ComputeRipemd256(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Ripemd256, data);

	/// <summary>Computes the RIPEMD-320 hash of the specified data.</summary>
	public static string ComputeRipemd320(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Ripemd320, data);

	/// <summary>Computes the Whirlpool hash of the specified data.</summary>
	public static string ComputeWhirlpool(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Whirlpool, data);

	/// <summary>Computes the Tiger-192 hash of the specified data.</summary>
	public static string ComputeTiger192(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Tiger192, data);

	/// <summary>Computes the GOST 34.11-94 hash of the specified data.</summary>
	public static string ComputeGost94(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Gost94, data);

	/// <summary>Computes the Streebog-256 hash of the specified data.</summary>
	public static string ComputeStreebog256(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Streebog256, data);

	/// <summary>Computes the Streebog-512 hash of the specified data.</summary>
	public static string ComputeStreebog512(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Streebog512, data);

	/// <summary>Computes the Skein-256 hash of the specified data.</summary>
	public static string ComputeSkein256(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Skein256, data);

	/// <summary>Computes the Skein-512 hash of the specified data.</summary>
	public static string ComputeSkein512(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Skein512, data);

	/// <summary>Computes the Skein-1024 hash of the specified data.</summary>
	public static string ComputeSkein1024(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Skein1024, data);

	/// <summary>Computes the Groestl-256 hash of the specified data.</summary>
	public static string ComputeGroestl256(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Groestl256, data);

	/// <summary>Computes the Groestl-512 hash of the specified data.</summary>
	public static string ComputeGroestl512(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Groestl512, data);

	/// <summary>Computes the JH-256 hash of the specified data.</summary>
	public static string ComputeJh256(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Jh256, data);

	/// <summary>Computes the JH-512 hash of the specified data.</summary>
	public static string ComputeJh512(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Jh512, data);

	/// <summary>Computes the KangarooTwelve hash of the specified data.</summary>
	public static string ComputeKangarooTwelve(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.KangarooTwelve, data);

	/// <summary>Computes the SM3 hash of the specified data.</summary>
	public static string ComputeSm3(ReadOnlySpan<byte> data) => HashFacade.ComputeHashHex(HashAlgorithm.Sm3, data);

	#endregion

	#region Byte Array Get Methods

	/// <summary>Gets the CRC-32 checksum bytes.</summary>
	public static byte[] GetCrc32Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Crc32, data);

	/// <summary>Gets the CRC-32C checksum bytes.</summary>
	public static byte[] GetCrc32CBytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Crc32C, data);

	/// <summary>Gets the CRC-64 checksum bytes.</summary>
	public static byte[] GetCrc64Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Crc64, data);

	/// <summary>Gets the CRC-16-CCITT checksum bytes.</summary>
	public static byte[] GetCrc16CcittBytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Crc16Ccitt, data);

	/// <summary>Gets the CRC-16-MODBUS checksum bytes.</summary>
	public static byte[] GetCrc16ModbusBytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Crc16Modbus, data);

	/// <summary>Gets the CRC-16-USB checksum bytes.</summary>
	public static byte[] GetCrc16UsbBytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Crc16Usb, data);

	/// <summary>Gets the Adler-32 checksum bytes.</summary>
	public static byte[] GetAdler32Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Adler32, data);

	/// <summary>Gets the Fletcher-16 checksum bytes.</summary>
	public static byte[] GetFletcher16Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Fletcher16, data);

	/// <summary>Gets the Fletcher-32 checksum bytes.</summary>
	public static byte[] GetFletcher32Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Fletcher32, data);

	/// <summary>Gets the xxHash32 bytes.</summary>
	public static byte[] GetXxHash32Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.XxHash32, data);

	/// <summary>Gets the xxHash64 bytes.</summary>
	public static byte[] GetXxHash64Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.XxHash64, data);

	/// <summary>Gets the xxHash3 bytes.</summary>
	public static byte[] GetXxHash3Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.XxHash3, data);

	/// <summary>Gets the xxHash128 bytes.</summary>
	public static byte[] GetXxHash128Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.XxHash128, data);

	/// <summary>Gets the MD5 bytes.</summary>
	public static byte[] GetMd5Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Md5, data);

	/// <summary>Gets the SHA-1 bytes.</summary>
	public static byte[] GetSha1Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Sha1, data);

	/// <summary>Gets the SHA-256 bytes.</summary>
	public static byte[] GetSha256Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Sha256, data);

	/// <summary>Gets the SHA-384 bytes.</summary>
	public static byte[] GetSha384Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Sha384, data);

	/// <summary>Gets the SHA-512 bytes.</summary>
	public static byte[] GetSha512Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Sha512, data);

	/// <summary>Gets the BLAKE2b bytes.</summary>
	public static byte[] GetBlake2bBytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Blake2b, data);

	/// <summary>Gets the BLAKE2s bytes.</summary>
	public static byte[] GetBlake2sBytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Blake2s, data);

	/// <summary>Gets the BLAKE3 bytes.</summary>
	public static byte[] GetBlake3Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Blake3, data);

	/// <summary>Gets the SHA3-256 bytes.</summary>
	public static byte[] GetSha3_256Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Sha3_256, data);

	/// <summary>Gets the KangarooTwelve bytes.</summary>
	public static byte[] GetKangarooTwelveBytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.KangarooTwelve, data);

	/// <summary>Gets the SM3 bytes.</summary>
	public static byte[] GetSm3Bytes(ReadOnlySpan<byte> data) => HashFacade.ComputeHash(HashAlgorithm.Sm3, data);

	#endregion

	#region File Hashing

	/// <summary>
	/// Hashes a file and returns all 70 hash values.
	/// </summary>
	/// <param name="filePath">Path to the file to hash.</param>
	/// <returns>Complete hash results for the file.</returns>
	public static FileHashResult HashFile(string filePath) {
		using var hasher = new StreamingHasher();
		return hasher.HashFile(filePath);
	}

	/// <summary>
	/// Hashes a file with progress reporting.
	/// </summary>
	/// <param name="filePath">Path to the file to hash.</param>
	/// <param name="progress">Progress callback (0.0 to 1.0).</param>
	/// <returns>Complete hash results for the file.</returns>
	public static FileHashResult HashFile(string filePath, Action<double> progress) {
		using var hasher = new StreamingHasher();
		return hasher.HashFile(filePath, progress);
	}

	/// <summary>
	/// Hashes a file asynchronously with progress reporting.
	/// </summary>
	/// <param name="filePath">Path to the file to hash.</param>
	/// <param name="progress">Optional progress callback (0.0 to 1.0).</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Complete hash results for the file.</returns>
	public static Task<FileHashResult> HashFileAsync(
		string filePath,
		Action<double>? progress = null,
		CancellationToken cancellationToken = default) {
		return Task.Run(() => {
			using var hasher = new StreamingHasher();
			return hasher.HashFile(filePath, progress, cancellationToken);
		}, cancellationToken);
	}

	#endregion

	#region JSON Output

	/// <summary>
	/// Saves hash results to a JSON file.
	/// </summary>
	/// <param name="result">The hash results to save.</param>
	/// <param name="outputPath">Path to the output JSON file.</param>
	public static void SaveResult(FileHashResult result, string outputPath) {
		string json = JsonSerializer.Serialize(result, JsonOptions);
		// Add blank lines between sections for readability
		json = AddBlankLinesBetweenSections(json);
		// Ensure trailing newline
		if (!json.EndsWith('\n')) {
			json += Environment.NewLine;
		}

		File.WriteAllText(outputPath, json);
	}

	/// <summary>
	/// Saves hash results to a JSON file asynchronously.
	/// </summary>
	/// <param name="result">The hash results to save.</param>
	/// <param name="outputPath">Path to the output JSON file.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public static async Task SaveResultAsync(
		FileHashResult result,
		string outputPath,
		CancellationToken cancellationToken = default) {
		string json = JsonSerializer.Serialize(result, JsonOptions);
		json = AddBlankLinesBetweenSections(json);
		if (!json.EndsWith('\n')) {
			json += Environment.NewLine;
		}

		await File.WriteAllTextAsync(outputPath, json, cancellationToken);
	}

	#endregion

	#region Utility Methods

	/// <summary>
	/// Adds blank lines between JSON sections for improved readability.
	/// </summary>
	/// <param name="json">The JSON string to format.</param>
	/// <returns>Formatted JSON with blank lines between sections.</returns>
	private static string AddBlankLinesBetweenSections(string json) {
		// Section break markers: the last property in each section
		// After metadata → checksums, after checksums → non-crypto, after non-crypto → crypto, after crypto → other crypto
		ReadOnlySpan<string> sectionBreakProperties = ["modifiedUtc", "crc16Usb", "loseLose", "ripemd320"];

		foreach (string prop in sectionBreakProperties) {
			// Find the property line and insert a blank line after it
			string search = $"\"{prop}\":";
			int idx = json.IndexOf(search, StringComparison.Ordinal);
			if (idx < 0) continue;

			// Find the end of this line (after the comma and newline)
			int lineEnd = json.IndexOf('\n', idx);
			if (lineEnd < 0) continue;

			// Insert an extra newline to create a blank line
			json = string.Concat(json.AsSpan(0, lineEnd + 1), Environment.NewLine, json.AsSpan(lineEnd + 1));
		}

		return json;
	}

	/// <summary>
	/// Formats a file size in bytes to a human-readable string.
	/// </summary>
	/// <param name="bytes">The file size in bytes.</param>
	/// <returns>Formatted string like "1.5 MB" or "256 KB".</returns>
	public static string FormatFileSize(long bytes) {
		string[] sizes = ["B", "KB", "MB", "GB", "TB"];
		int order = 0;
		double size = bytes;

		while (size >= 1024 && order < sizes.Length - 1) {
			order++;
			size /= 1024;
		}

		return order == 0
			? $"{bytes} {sizes[order]}"
			: $"{size:0.##} {sizes[order]}";
	}

	/// <summary>
	/// Estimates hash computation duration based on file size.
	/// </summary>
	/// <param name="fileSizeBytes">The file size in bytes.</param>
	/// <returns>Estimated duration in milliseconds.</returns>
	/// <remarks>
	/// Based on empirical measurements: ~200 MB/s throughput for all 70 hashes.
	/// Actual performance may vary based on hardware.
	/// </remarks>
	public static long EstimateHashDurationMs(long fileSizeBytes) {
		// Approximate throughput: 200 MB/s for computing all 70 hashes
		const double bytesPerMillisecond = 200_000; // 200 KB/ms = 200 MB/s
		return (long)(fileSizeBytes / bytesPerMillisecond);
	}

	#endregion
}
