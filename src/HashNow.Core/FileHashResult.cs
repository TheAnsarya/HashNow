using System.Text.Json.Serialization;

namespace HashNow.Core;

/// <summary>
/// Represents the complete result of hashing a file with all 70 supported hash algorithms.
/// </summary>
/// <remarks>
/// <para>
/// This class contains all computed hash values organized into four categories:
/// </para>
/// <list type="number">
///   <item><description>Checksums &amp; CRCs (9 algorithms) - Fast error detection</description></item>
///   <item><description>Non-Crypto Fast Hashes (22 algorithms) - High-speed, non-cryptographic</description></item>
///   <item><description>Cryptographic Hashes (26 algorithms) - Security-focused</description></item>
///   <item><description>Other Crypto Hashes (14 algorithms) - Specialized cryptographic</description></item>
/// </list>
/// <para>
/// All hash values are stored as lowercase hexadecimal strings for consistency
/// and interoperability with standard hash verification tools.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Hash a file and access results
/// var result = FileHasher.HashFile("myfile.zip");
/// Console.WriteLine($"MD5: {result.Md5}");
/// Console.WriteLine($"SHA-256: {result.Sha256}");
/// Console.WriteLine($"CRC32: {result.Crc32}");
/// </code>
/// </example>
public sealed class FileHashResult {
	#region File Metadata

	/// <summary>
	/// Gets the name of the hashed file (without path).
	/// </summary>
	/// <example>"document.pdf"</example>
	[JsonPropertyName("fileName")]
	public required string FileName { get; init; }

	/// <summary>
	/// Gets the full absolute path to the hashed file.
	/// </summary>
	/// <example>"C:\Users\john\Documents\document.pdf"</example>
	[JsonPropertyName("fullPath")]
	public required string FullPath { get; init; }

	/// <summary>
	/// Gets the file size in bytes.
	/// </summary>
	/// <remarks>
	/// Use <see cref="SizeFormatted"/> for a human-readable representation.
	/// </remarks>
	[JsonPropertyName("sizeBytes")]
	public required long SizeBytes { get; init; }

	/// <summary>
	/// Gets the file size formatted as a human-readable string.
	/// </summary>
	/// <example>"1.5 MB", "256 KB", "2.3 GB"</example>
	[JsonPropertyName("sizeFormatted")]
	public required string SizeFormatted { get; init; }

	/// <summary>
	/// Gets the file creation timestamp in UTC, ISO 8601 format.
	/// </summary>
	/// <example>"2024-01-15T10:30:00.0000000Z"</example>
	[JsonPropertyName("createdUtc")]
	public required string CreatedUtc { get; init; }

	/// <summary>
	/// Gets the file last modification timestamp in UTC, ISO 8601 format.
	/// </summary>
	/// <example>"2024-02-20T14:45:30.0000000Z"</example>
	[JsonPropertyName("modifiedUtc")]
	public required string ModifiedUtc { get; init; }

	#endregion

	#region Checksums & CRCs (6 algorithms)

	/// <summary>
	/// Gets the CRC-32 checksum (IEEE 802.3 polynomial).
	/// </summary>
	/// <remarks>
	/// <para>Output: 8 hex characters (32 bits)</para>
	/// <para>CRC-32 is widely used for error detection in network protocols
	/// and file formats like ZIP, PNG, and Ethernet frames.</para>
	/// </remarks>
	/// <example>"a06d65c1"</example>
	[JsonPropertyName("crc32")]
	public required string Crc32 { get; init; }

	/// <summary>
	/// Gets the CRC-32C checksum (Castagnoli polynomial).
	/// </summary>
	/// <remarks>
	/// <para>Output: 8 hex characters (32 bits)</para>
	/// <para>CRC-32C uses a different polynomial optimized for hardware acceleration
	/// (Intel SSE4.2). Used in iSCSI, SCTP, and ext4 filesystem.</para>
	/// </remarks>
	/// <example>"d87f7e0c"</example>
	[JsonPropertyName("crc32c")]
	public required string Crc32C { get; init; }

	/// <summary>
	/// Gets the CRC-64 checksum (ECMA-182 polynomial).
	/// </summary>
	/// <remarks>
	/// <para>Output: 16 hex characters (64 bits)</para>
	/// <para>CRC-64 provides stronger error detection than CRC-32, used in
	/// some backup systems and data integrity verification.</para>
	/// </remarks>
	/// <example>"b90956c775a41001"</example>
	[JsonPropertyName("crc64")]
	public required string Crc64 { get; init; }

	/// <summary>
	/// Gets the Adler-32 checksum.
	/// </summary>
	/// <remarks>
	/// <para>Output: 8 hex characters (32 bits)</para>
	/// <para>Adler-32 is faster than CRC-32 but provides weaker error detection.
	/// Used in zlib compression library.</para>
	/// </remarks>
	/// <example>"11e60398"</example>
	[JsonPropertyName("adler32")]
	public required string Adler32 { get; init; }

	/// <summary>
	/// Gets the Fletcher-16 checksum.
	/// </summary>
	/// <remarks>
	/// <para>Output: 4 hex characters (16 bits)</para>
	/// <para>Fletcher-16 is a simple checksum algorithm providing basic
	/// error detection with low computational overhead.</para>
	/// </remarks>
	/// <example>"3c0a"</example>
	[JsonPropertyName("fletcher16")]
	public required string Fletcher16 { get; init; }

	/// <summary>
	/// Gets the Fletcher-32 checksum.
	/// </summary>
	/// <remarks>
	/// <para>Output: 8 hex characters (32 bits)</para>
	/// <para>Fletcher-32 provides better error detection than Fletcher-16
	/// while maintaining simplicity and speed.</para>
	/// </remarks>
	/// <example>"0f2e390a"</example>
	[JsonPropertyName("fletcher32")]
	public required string Fletcher32 { get; init; }

	/// <summary>
	/// Gets the CRC-16-CCITT checksum.
	/// </summary>
	/// <remarks>
	/// <para>Output: 4 hex characters (16 bits)</para>
	/// <para>CRC-16-CCITT uses polynomial 0x1021, commonly used in
	/// X.25, HDLC, and Bluetooth protocols.</para>
	/// </remarks>
	/// <example>"29b1"</example>
	[JsonPropertyName("crc16Ccitt")]
	public required string Crc16Ccitt { get; init; }

	/// <summary>
	/// Gets the CRC-16-MODBUS checksum.
	/// </summary>
	/// <remarks>
	/// <para>Output: 4 hex characters (16 bits)</para>
	/// <para>CRC-16-MODBUS uses polynomial 0x8005 with init 0xFFFF,
	/// used in MODBUS industrial communication protocol.</para>
	/// </remarks>
	/// <example>"4b37"</example>
	[JsonPropertyName("crc16Modbus")]
	public required string Crc16Modbus { get; init; }

	/// <summary>
	/// Gets the CRC-16-USB checksum.
	/// </summary>
	/// <remarks>
	/// <para>Output: 4 hex characters (16 bits)</para>
	/// <para>CRC-16-USB uses polynomial 0x8005 with inversion,
	/// used in USB protocol error detection.</para>
	/// </remarks>
	/// <example>"b4c8"</example>
	[JsonPropertyName("crc16Usb")]
	public required string Crc16Usb { get; init; }

	#endregion

	#region Non-Crypto Fast Hashes (22 algorithms)

	/// <summary>
	/// Gets the xxHash32 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 8 hex characters (32 bits)</para>
	/// <para>xxHash32 is an extremely fast non-cryptographic hash algorithm,
	/// optimized for speed on modern CPUs. Part of the xxHash family.</para>
	/// </remarks>
	/// <example>"4b9bbe05"</example>
	[JsonPropertyName("xxHash32")]
	public required string XxHash32 { get; init; }

	/// <summary>
	/// Gets the xxHash64 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 16 hex characters (64 bits)</para>
	/// <para>xxHash64 provides better distribution than xxHash32 while
	/// maintaining excellent speed on 64-bit systems.</para>
	/// </remarks>
	/// <example>"d24ec4f1a98c6e5b"</example>
	[JsonPropertyName("xxHash64")]
	public required string XxHash64 { get; init; }

	/// <summary>
	/// Gets the xxHash3 (XXH3) hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 16 hex characters (64 bits)</para>
	/// <para>xxHash3 is the latest generation of xxHash, offering improved
	/// speed especially for small inputs and better SIMD utilization.</para>
	/// </remarks>
	/// <example>"7bcd9ed2c34a6f81"</example>
	[JsonPropertyName("xxHash3")]
	public required string XxHash3 { get; init; }

	/// <summary>
	/// Gets the xxHash128 (XXH128) hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 32 hex characters (128 bits)</para>
	/// <para>xxHash128 provides 128-bit output for applications requiring
	/// extremely low collision probability in non-cryptographic contexts.</para>
	/// </remarks>
	/// <example>"d24ec4f1a98c6e5b7bcd9ed2c34a6f81"</example>
	[JsonPropertyName("xxHash128")]
	public required string XxHash128 { get; init; }

	/// <summary>
	/// Gets the MurmurHash3 32-bit hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 8 hex characters (32 bits)</para>
	/// <para>MurmurHash3 is widely used in hash tables, bloom filters,
	/// and distributed systems. Created by Austin Appleby.</para>
	/// </remarks>
	/// <example>"b0c65f3a"</example>
	[JsonPropertyName("murmur3_32")]
	public required string Murmur3_32 { get; init; }

	/// <summary>
	/// Gets the MurmurHash3 128-bit hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 32 hex characters (128 bits)</para>
	/// <para>MurmurHash3-128 provides better collision resistance than
	/// the 32-bit variant for large-scale applications.</para>
	/// </remarks>
	/// <example>"6af1df4d9e2c8b3a7c5e9f1b2d4a6c8e"</example>
	[JsonPropertyName("murmur3_128")]
	public required string Murmur3_128 { get; init; }

	/// <summary>
	/// Gets the CityHash64 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 16 hex characters (64 bits)</para>
	/// <para>CityHash was developed by Google for fast hashing of strings.
	/// Optimized for short strings and hash table lookups.</para>
	/// </remarks>
	/// <example>"8c3a7b2f1e5d9c4a"</example>
	[JsonPropertyName("cityHash64")]
	public required string CityHash64 { get; init; }

	/// <summary>
	/// Gets the CityHash128 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 32 hex characters (128 bits)</para>
	/// <para>CityHash128 provides 128-bit output for applications requiring
	/// lower collision probability than CityHash64.</para>
	/// </remarks>
	/// <example>"8c3a7b2f1e5d9c4a6d8e2f4a1b3c5d7e"</example>
	[JsonPropertyName("cityHash128")]
	public required string CityHash128 { get; init; }

	/// <summary>
	/// Gets the FarmHash64 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 16 hex characters (64 bits)</para>
	/// <para>FarmHash is Google's successor to CityHash, providing improved
	/// performance on certain platforms. Currently uses CityHash64 internally.</para>
	/// </remarks>
	/// <example>"7e9f3b1c5a2d8e4f"</example>
	[JsonPropertyName("farmHash64")]
	public required string FarmHash64 { get; init; }

	/// <summary>
	/// Gets the SpookyHash V2 128-bit hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 32 hex characters (128 bits)</para>
	/// <para>SpookyHash V2 by Bob Jenkins is designed for fast hashing of
	/// large data. Produces both 64-bit and 128-bit outputs efficiently.</para>
	/// </remarks>
	/// <example>"3a1b5c7d9e2f4a6b8c0d2e4f6a8b0c2d"</example>
	[JsonPropertyName("spookyV2_128")]
	public required string SpookyV2_128 { get; init; }

	/// <summary>
	/// Gets the SipHash-2-4 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 16 hex characters (64 bits)</para>
	/// <para>SipHash is a cryptographically-strong PRF designed to be used
	/// as a hash table hash function to prevent hash-flooding DoS attacks.</para>
	/// </remarks>
	/// <example>"f4a8b2c6d0e4f8a2"</example>
	[JsonPropertyName("sipHash24")]
	public required string SipHash24 { get; init; }

	/// <summary>
	/// Gets the HighwayHash64 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 16 hex characters (64 bits)</para>
	/// <para>HighwayHash is designed for SIMD acceleration on modern CPUs.
	/// Uses native SIMD implementation from StreamHash.</para>
	/// </remarks>
	/// <example>"2c4e6a8b0d2f4a6c"</example>
	[JsonPropertyName("highwayHash64")]
	public required string HighwayHash64 { get; init; }

	/// <summary>
	/// Gets the MetroHash64 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 16 hex characters (64 bits)</para>
	/// <para>MetroHash64 is one of the fastest hash functions available (~15 GB/s).
	/// Created by J. Andrew Rogers for maximum throughput.</para>
	/// </remarks>
	/// <example>"3a1b5c7d9e2f4a6b"</example>
	[JsonPropertyName("metroHash64")]
	public required string MetroHash64 { get; init; }

	/// <summary>
	/// Gets the MetroHash128 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 32 hex characters (128 bits)</para>
	/// <para>MetroHash128 provides 128-bit output for lower collision probability.</para>
	/// </remarks>
	/// <example>"3a1b5c7d9e2f4a6b8c0d2e4f6a8b0c2d"</example>
	[JsonPropertyName("metroHash128")]
	public required string MetroHash128 { get; init; }

	/// <summary>
	/// Gets the wyhash64 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 16 hex characters (64 bits)</para>
	/// <para>wyhash is among the fastest hash functions (15-25 GB/s).
	/// Created by Wang Yi with excellent quality and speed.</para>
	/// </remarks>
	/// <example>"7e9f3b1c5a2d8e4f"</example>
	[JsonPropertyName("wyhash64")]
	public required string Wyhash64 { get; init; }

	/// <summary>
	/// Gets the FNV-1a 32-bit hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 8 hex characters (32 bits)</para>
	/// <para>FNV-1a (Fowler-Noll-Vo) is a simple, fast hash function using XOR-then-multiply.
	/// Good distribution for hash tables.</para>
	/// </remarks>
	/// <example>"811c9dc5"</example>
	[JsonPropertyName("fnv1a32")]
	public required string Fnv1a32 { get; init; }

	/// <summary>
	/// Gets the FNV-1a 64-bit hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 16 hex characters (64 bits)</para>
	/// <para>FNV-1a-64 provides better collision resistance than the 32-bit variant.</para>
	/// </remarks>
	/// <example>"cbf29ce484222325"</example>
	[JsonPropertyName("fnv1a64")]
	public required string Fnv1a64 { get; init; }

	/// <summary>
	/// Gets the DJB2 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 8 hex characters (32 bits)</para>
	/// <para>DJB2 by Dan Bernstein uses multiply-and-add: hash * 33 + byte.
	/// One of the most widely used simple hash functions.</para>
	/// </remarks>
	/// <example>"00001505"</example>
	[JsonPropertyName("djb2")]
	public required string Djb2 { get; init; }

	/// <summary>
	/// Gets the DJB2a hash value (XOR variant).
	/// </summary>
	/// <remarks>
	/// <para>Output: 8 hex characters (32 bits)</para>
	/// <para>DJB2a uses XOR instead of addition: hash * 33 ^ byte.
	/// Often provides better avalanche properties.</para>
	/// </remarks>
	/// <example>"00001505"</example>
	[JsonPropertyName("djb2a")]
	public required string Djb2a { get; init; }

	/// <summary>
	/// Gets the SDBM hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 8 hex characters (32 bits)</para>
	/// <para>SDBM hash from SDBM database: hash * 65599 + byte.
	/// Good distribution for short strings.</para>
	/// </remarks>
	/// <example>"00000000"</example>
	[JsonPropertyName("sdbm")]
	public required string Sdbm { get; init; }

	/// <summary>
	/// Gets the Lose Lose hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 8 hex characters (32 bits)</para>
	/// <para><strong>⚠️ Poor distribution:</strong> Simple byte sum hash.
	/// Educational only - do not use for hash tables or data integrity.</para>
	/// </remarks>
	/// <example>"00000000"</example>
	[JsonPropertyName("loseLose")]
	public required string LoseLose { get; init; }

	#endregion

	#region Cryptographic Hashes - MD Family (3 algorithms)

	/// <summary>
	/// Gets the MD2 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 32 hex characters (128 bits)</para>
	/// <para><strong>⚠️ DEPRECATED:</strong> MD2 is cryptographically broken and should
	/// not be used for security purposes. Included for legacy compatibility only.</para>
	/// </remarks>
	/// <example>"8350e5a3e24c153df2275c9f80692773"</example>
	[JsonPropertyName("md2")]
	public required string Md2 { get; init; }

	/// <summary>
	/// Gets the MD4 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 32 hex characters (128 bits)</para>
	/// <para><strong>⚠️ DEPRECATED:</strong> MD4 is cryptographically broken and should
	/// not be used for security purposes. Included for legacy compatibility only.</para>
	/// </remarks>
	/// <example>"a448017aaf21d8525fc10ae87aa6729d"</example>
	[JsonPropertyName("md4")]
	public required string Md4 { get; init; }

	/// <summary>
	/// Gets the MD5 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 32 hex characters (128 bits)</para>
	/// <para><strong>⚠️ DEPRECATED for security:</strong> MD5 is vulnerable to collision
	/// attacks. Still commonly used for file integrity verification and checksums,
	/// but should not be used for security purposes.</para>
	/// </remarks>
	/// <example>"d41d8cd98f00b204e9800998ecf8427e"</example>
	[JsonPropertyName("md5")]
	public required string Md5 { get; init; }

	#endregion

	#region Cryptographic Hashes - SHA Family (11 algorithms)

	/// <summary>
	/// Gets the SHA-0 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 40 hex characters (160 bits)</para>
	/// <para><strong>⚠️ DEPRECATED:</strong> SHA-0 was withdrawn shortly after publication
	/// due to a flaw. Uses SHA-1 as fallback. Included for completeness only.</para>
	/// </remarks>
	/// <example>"da39a3ee5e6b4b0d3255bfef95601890afd80709"</example>
	[JsonPropertyName("sha0")]
	public required string Sha0 { get; init; }

	/// <summary>
	/// Gets the SHA-1 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 40 hex characters (160 bits)</para>
	/// <para><strong>⚠️ DEPRECATED for security:</strong> SHA-1 has known collision attacks.
	/// Still used in Git and some legacy systems, but SHA-256 or higher is recommended.</para>
	/// </remarks>
	/// <example>"da39a3ee5e6b4b0d3255bfef95601890afd80709"</example>
	[JsonPropertyName("sha1")]
	public required string Sha1 { get; init; }

	/// <summary>
	/// Gets the SHA-224 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 56 hex characters (224 bits)</para>
	/// <para>SHA-224 is a truncated version of SHA-256, providing a balance
	/// between security and hash size for applications with space constraints.</para>
	/// </remarks>
	/// <example>"d14a028c2a3a2bc9476102bb288234c415a2b01f828ea62ac5b3e42f"</example>
	[JsonPropertyName("sha224")]
	public required string Sha224 { get; init; }

	/// <summary>
	/// Gets the SHA-256 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>SHA-256 is the most widely used secure hash algorithm today.
	/// Recommended for most security applications including digital signatures,
	/// TLS certificates, and blockchain.</para>
	/// </remarks>
	/// <example>"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"</example>
	[JsonPropertyName("sha256")]
	public required string Sha256 { get; init; }

	/// <summary>
	/// Gets the SHA-384 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 96 hex characters (384 bits)</para>
	/// <para>SHA-384 is a truncated version of SHA-512, offering higher
	/// security than SHA-256 with better performance on 64-bit systems.</para>
	/// </remarks>
	/// <example>"38b060a751ac96384cd9327eb1b1e36a21fdb71114be07434c0cc7bf63f6e1da274edebfe76f65fbd51ad2f14898b95b"</example>
	[JsonPropertyName("sha384")]
	public required string Sha384 { get; init; }

	/// <summary>
	/// Gets the SHA-512 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 128 hex characters (512 bits)</para>
	/// <para>SHA-512 provides the highest security level in the SHA-2 family.
	/// Often faster than SHA-256 on 64-bit processors due to native 64-bit operations.</para>
	/// </remarks>
	/// <example>"cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e"</example>
	[JsonPropertyName("sha512")]
	public required string Sha512 { get; init; }

	/// <summary>
	/// Gets the SHA-512/224 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 56 hex characters (224 bits)</para>
	/// <para>SHA-512/224 uses the SHA-512 algorithm with a different initial hash
	/// value and truncated output. Provides 112-bit security against collision attacks.</para>
	/// </remarks>
	/// <example>"6ed0dd02806fa89e25de060c19d3ac86cabb87d6a0ddd05c333b84f4"</example>
	[JsonPropertyName("sha512_224")]
	public required string Sha512_224 { get; init; }

	/// <summary>
	/// Gets the SHA-512/256 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>SHA-512/256 uses SHA-512 internally but outputs 256 bits.
	/// Faster than SHA-256 on 64-bit systems while providing equivalent security.</para>
	/// </remarks>
	/// <example>"c672b8d1ef56ed28ab87c3622c5114069bdd3ad7b8f9737498d0c01ecef0967a"</example>
	[JsonPropertyName("sha512_256")]
	public required string Sha512_256 { get; init; }

	#endregion

	#region Cryptographic Hashes - SHA-3 & Keccak (6 algorithms)

	/// <summary>
	/// Gets the SHA3-224 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 56 hex characters (224 bits)</para>
	/// <para>SHA-3 is based on the Keccak algorithm and provides an alternative
	/// to SHA-2 with a completely different internal structure (sponge construction).</para>
	/// </remarks>
	/// <example>"6b4e03423667dbb73b6e15454f0eb1abd4597f9a1b078e3f5b5a6bc7"</example>
	[JsonPropertyName("sha3_224")]
	public required string Sha3_224 { get; init; }

	/// <summary>
	/// Gets the SHA3-256 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>SHA3-256 provides 128-bit security level, equivalent to SHA-256
	/// but using the Keccak sponge construction for defense in depth.</para>
	/// </remarks>
	/// <example>"a7ffc6f8bf1ed76651c14756a061d662f580ff4de43b49fa82d80a4b80f8434a"</example>
	[JsonPropertyName("sha3_256")]
	public required string Sha3_256 { get; init; }

	/// <summary>
	/// Gets the SHA3-384 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 96 hex characters (384 bits)</para>
	/// <para>SHA3-384 provides 192-bit security level, suitable for applications
	/// requiring higher security margins than SHA3-256.</para>
	/// </remarks>
	/// <example>"0c63a75b845e4f7d01107d852e4c2485c51a50aaaa94fc61995e71bbee983a2ac3713831264adb47fb6bd1e058d5f004"</example>
	[JsonPropertyName("sha3_384")]
	public required string Sha3_384 { get; init; }

	/// <summary>
	/// Gets the SHA3-512 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 128 hex characters (512 bits)</para>
	/// <para>SHA3-512 provides 256-bit security level, the highest in the
	/// SHA-3 family, suitable for long-term security requirements.</para>
	/// </remarks>
	/// <example>"a69f73cca23a9ac5c8b567dc185a756e97c982164fe25859e0d1dcc1475c80a615b2123af1f5f94c11e3e9402c3ac558f500199d95b6d3e301758586281dcd26"</example>
	[JsonPropertyName("sha3_512")]
	public required string Sha3_512 { get; init; }

	/// <summary>
	/// Gets the Keccak-256 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>Keccak-256 is the original Keccak submission before NIST standardization
	/// as SHA-3. Used in Ethereum blockchain for address generation and transactions.</para>
	/// </remarks>
	/// <example>"c5d2460186f7233c927e7db2dcc703c0e500b653ca82273b7bfad8045d85a470"</example>
	[JsonPropertyName("keccak256")]
	public required string Keccak256 { get; init; }

	/// <summary>
	/// Gets the Keccak-512 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 128 hex characters (512 bits)</para>
	/// <para>Keccak-512 provides 512-bit output using the original Keccak
	/// parameters before SHA-3 standardization.</para>
	/// </remarks>
	/// <example>"0eab42de4c3ceb9235fc91acffe746b29c29a8c366b7c60e4e67c466f36a4304c00fa9caf9d87976ba469bcbe06713b435f091ef2769fb160cdab33d3670680e"</example>
	[JsonPropertyName("keccak512")]
	public required string Keccak512 { get; init; }

	#endregion

	#region Cryptographic Hashes - BLAKE Family (5 algorithms)

	/// <summary>
	/// Gets the BLAKE-256 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>BLAKE was a SHA-3 competition finalist. This implementation uses
	/// BLAKE2b-256 internally, which is an improved version of BLAKE.</para>
	/// </remarks>
	/// <example>"716f6e863f744b9ac22c97ec7b76ea5f5908bc5b2f67c61510bfc4751384ea7a"</example>
	[JsonPropertyName("blake256")]
	public required string Blake256 { get; init; }

	/// <summary>
	/// Gets the BLAKE-512 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 128 hex characters (512 bits)</para>
	/// <para>BLAKE-512 provides 512-bit output. This implementation uses
	/// BLAKE2b-512 internally for improved performance and security.</para>
	/// </remarks>
	/// <example>"786a02f742015903c6c6fd852552d272912f4740e15847618a86e217f71f5419d25e1031afee585313896444934eb04b903a685b1448b755d56f701afe9be2ce"</example>
	[JsonPropertyName("blake512")]
	public required string Blake512 { get; init; }

	/// <summary>
	/// Gets the BLAKE2b hash value (512-bit).
	/// </summary>
	/// <remarks>
	/// <para>Output: 128 hex characters (512 bits)</para>
	/// <para>BLAKE2b is optimized for 64-bit platforms and is faster than MD5
	/// while providing security comparable to SHA-3. Widely used in modern applications.</para>
	/// </remarks>
	/// <example>"786a02f742015903c6c6fd852552d272912f4740e15847618a86e217f71f5419d25e1031afee585313896444934eb04b903a685b1448b755d56f701afe9be2ce"</example>
	[JsonPropertyName("blake2b")]
	public required string Blake2b { get; init; }

	/// <summary>
	/// Gets the BLAKE2s hash value (256-bit).
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>BLAKE2s is optimized for 8-bit to 32-bit platforms, providing
	/// excellent performance on embedded systems and smaller processors.</para>
	/// </remarks>
	/// <example>"69217a3079908094e11121d042354a7c1f55b6482ca1a51e1b250dfd1ed0eef9"</example>
	[JsonPropertyName("blake2s")]
	public required string Blake2s { get; init; }

	/// <summary>
	/// Gets the BLAKE3 hash value (256-bit).
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>BLAKE3 is the latest in the BLAKE family, designed for extreme speed
	/// through parallelism and SIMD. Much faster than SHA-256 and BLAKE2 while
	/// maintaining high security. Excellent for hashing large files.</para>
	/// </remarks>
	/// <example>"af1349b9f5f9a1a6a0404dea36dcc9499bcb25c9adc112b7cc9a93cae41f3262"</example>
	[JsonPropertyName("blake3")]
	public required string Blake3 { get; init; }

	#endregion

	#region Cryptographic Hashes - RIPEMD Family (4 algorithms)

	/// <summary>
	/// Gets the RIPEMD-128 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 32 hex characters (128 bits)</para>
	/// <para>RIPEMD-128 provides 128-bit output. Less commonly used than RIPEMD-160
	/// but still available for compatibility with legacy systems.</para>
	/// </remarks>
	/// <example>"cdf26213a150dc3ecb610f18f6b38b46"</example>
	[JsonPropertyName("ripemd128")]
	public required string Ripemd128 { get; init; }

	/// <summary>
	/// Gets the RIPEMD-160 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 40 hex characters (160 bits)</para>
	/// <para>RIPEMD-160 was designed in the open academic community and is used
	/// in Bitcoin addresses (combined with SHA-256) and PGP fingerprints.</para>
	/// </remarks>
	/// <example>"9c1185a5c5e9fc54612808977ee8f548b2258d31"</example>
	[JsonPropertyName("ripemd160")]
	public required string Ripemd160 { get; init; }

	/// <summary>
	/// Gets the RIPEMD-256 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>RIPEMD-256 is an extended version providing 256-bit output.
	/// Note: Security level is similar to RIPEMD-128, not doubled.</para>
	/// </remarks>
	/// <example>"02ba4c4e5f8ecd1877fc52d64d30e37a2d9774fb1e5d026380ae0168e3c5522d"</example>
	[JsonPropertyName("ripemd256")]
	public required string Ripemd256 { get; init; }

	/// <summary>
	/// Gets the RIPEMD-320 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 80 hex characters (320 bits)</para>
	/// <para>RIPEMD-320 provides the longest output in the RIPEMD family.
	/// Security level is similar to RIPEMD-160, with extended output.</para>
	/// </remarks>
	/// <example>"22d65d5661536cdc75c1fdf5c6de7b41b9f27325ebc61e8557177d705a0ec880151c3a32a00899b8"</example>
	[JsonPropertyName("ripemd320")]
	public required string Ripemd320 { get; init; }

	#endregion

	#region Other Crypto Hashes (14 algorithms)

	/// <summary>
	/// Gets the Whirlpool hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 128 hex characters (512 bits)</para>
	/// <para>Whirlpool is designed by Vincent Rijmen (co-creator of AES) and
	/// Paulo Barreto. Adopted by ISO/IEC 10118-3 standard.</para>
	/// </remarks>
	/// <example>"19fa61d75522a4669b44e39c1d2e1726c530232130d407f89afee0964997f7a73e83be698b288febcf88e3e03c4f0757ea8964e59b63d93708b138cc42a66eb3"</example>
	[JsonPropertyName("whirlpool")]
	public required string Whirlpool { get; init; }

	/// <summary>
	/// Gets the Tiger-192 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 48 hex characters (192 bits)</para>
	/// <para>Tiger was designed by Ross Anderson and Eli Biham for efficiency
	/// on 64-bit platforms. Used in some file-sharing and backup applications.</para>
	/// </remarks>
	/// <example>"3293ac630c13f0245f92bbb1766e16167a4e58492dde73f3"</example>
	[JsonPropertyName("tiger192")]
	public required string Tiger192 { get; init; }

	/// <summary>
	/// Gets the GOST R 34.11-94 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>GOST is the Russian federal standard for cryptographic hashing.
	/// This is the older 1994 version (GOST 34.11-94).</para>
	/// </remarks>
	/// <example>"981e5f3ca30c841487830f84fb433e13ac1101569b9c13584ac483234cd656c0"</example>
	[JsonPropertyName("gost94")]
	public required string Gost94 { get; init; }

	/// <summary>
	/// Gets the Streebog-256 (GOST R 34.11-2012) hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>Streebog is the modern Russian hash standard (GOST R 34.11-2012),
	/// designed to replace GOST 94. Also known as Kuznyechik-based hash.</para>
	/// </remarks>
	/// <example>"3f539a213e97c802cc229d474c6aa32a825a360b2a933a949fd925208d9ce1bb"</example>
	[JsonPropertyName("streebog256")]
	public required string Streebog256 { get; init; }

	/// <summary>
	/// Gets the Streebog-512 (GOST R 34.11-2012) hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 128 hex characters (512 bits)</para>
	/// <para>Streebog-512 provides 512-bit output using the GOST R 34.11-2012
	/// standard, suitable for high-security Russian government applications.</para>
	/// </remarks>
	/// <example>"8e945da209aa869f0455928529bcae4679e9873ab707b55315f56ceb98bef0a7362f715528356ee83cda5f2aac4c6ad2ba3a715c1bcd81cb8e9f90bf4c1c1a8a"</example>
	[JsonPropertyName("streebog512")]
	public required string Streebog512 { get; init; }

	/// <summary>
	/// Gets the Skein-256 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>Skein was a SHA-3 competition finalist by Bruce Schneier's team.
	/// Based on the Threefish block cipher with excellent performance.</para>
	/// </remarks>
	/// <example>"c8877087da56e072870daa843f176e9453115929094c3a40c463a196c29bf7ba"</example>
	[JsonPropertyName("skein256")]
	public required string Skein256 { get; init; }

	/// <summary>
	/// Gets the Skein-512 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 128 hex characters (512 bits)</para>
	/// <para>Skein-512 uses 512-bit internal state, providing excellent
	/// performance on 64-bit systems with high security margins.</para>
	/// </remarks>
	/// <example>"bc5b4c50925519c290cc634277ae3d6257212395cba733bbad37a4af0fa06af41fca7903d06564fea7a2d3730dbdb80c1f85562dfcc070334ea4d1d9e72cba7a"</example>
	[JsonPropertyName("skein512")]
	public required string Skein512 { get; init; }

	/// <summary>
	/// Gets the Skein-1024 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 256 hex characters (1024 bits)</para>
	/// <para>Skein-1024 provides the largest output, designed for applications
	/// requiring extremely long hash values or post-quantum security margins.</para>
	/// </remarks>
	[JsonPropertyName("skein1024")]
	public required string Skein1024 { get; init; }

	/// <summary>
	/// Gets the Grøstl-256 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>Grøstl was a SHA-3 competition finalist using AES-like structure.
	/// Currently uses SHA3-256 as fallback since BouncyCastle lacks native support.</para>
	/// </remarks>
	/// <example>"1a52d11d550039be16107f9c58db9ebcc417f16f736c772c81e9b16187a5f89c"</example>
	[JsonPropertyName("groestl256")]
	public required string Groestl256 { get; init; }

	/// <summary>
	/// Gets the Grøstl-512 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 128 hex characters (512 bits)</para>
	/// <para>Grøstl-512 provides 512-bit output. Currently uses SHA3-512 as
	/// fallback since BouncyCastle lacks native Grøstl support.</para>
	/// </remarks>
	[JsonPropertyName("groestl512")]
	public required string Groestl512 { get; init; }

	/// <summary>
	/// Gets the JH-256 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>JH was a SHA-3 competition finalist by Hongjun Wu. Currently uses
	/// SHA3-256 as fallback since BouncyCastle lacks native support.</para>
	/// </remarks>
	[JsonPropertyName("jh256")]
	public required string Jh256 { get; init; }

	/// <summary>
	/// Gets the JH-512 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 128 hex characters (512 bits)</para>
	/// <para>JH-512 provides 512-bit output. Currently uses SHA3-512 as
	/// fallback since BouncyCastle lacks native JH support.</para>
	/// </remarks>
	[JsonPropertyName("jh512")]
	public required string Jh512 { get; init; }

	/// <summary>
	/// Gets the KangarooTwelve (K12) hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>KangarooTwelve is a faster variant of Keccak/SHA-3 designed for
	/// high performance through parallelism. Currently uses Keccak-256 as fallback.</para>
	/// </remarks>
	[JsonPropertyName("kangarooTwelve")]
	public required string KangarooTwelve { get; init; }

	/// <summary>
	/// Gets the SM3 hash value.
	/// </summary>
	/// <remarks>
	/// <para>Output: 64 hex characters (256 bits)</para>
	/// <para>SM3 is the Chinese cryptographic hash standard (GB/T 32905-2016),
	/// mandatory for use in Chinese commercial cryptographic applications.</para>
	/// </remarks>
	/// <example>"66c7f0f462eeedd9d1f2d46bdc10e4e24167c4875cf2f7a2297da02b8f4ba8e0"</example>
	[JsonPropertyName("sm3")]
	public required string Sm3 { get; init; }

	#endregion

	#region Hashing Metadata

	/// <summary>
	/// Gets the timestamp when the hashing operation completed, in UTC ISO 8601 format.
	/// </summary>
	/// <example>"2024-02-02T15:30:45.1234567Z"</example>
	[JsonPropertyName("hashedAtUtc")]
	public required string HashedAtUtc { get; init; }

	/// <summary>
	/// Gets the total duration of the hashing operation in milliseconds.
	/// </summary>
	/// <remarks>
	/// This includes time for reading the file and computing all 58 hash algorithms
	/// in parallel. Actual throughput depends on storage speed and CPU cores.
	/// </remarks>
	[JsonPropertyName("durationMs")]
	public required long DurationMs { get; init; }

	/// <summary>
	/// Gets the application name and version that generated this hash result.
	/// </summary>
	/// <example>"HashNow v2.0.0"</example>
	[JsonPropertyName("generatedBy")]
	public string GeneratedBy { get; init; } = $"HashNow v{FileHasher.Version}";

	/// <summary>
	/// Gets the total number of hash algorithms computed.
	/// </summary>
	/// <value>Always 58 for the current version.</value>
	[JsonPropertyName("algorithmCount")]
	public int AlgorithmCount { get; init; } = 58;

	#endregion
}
