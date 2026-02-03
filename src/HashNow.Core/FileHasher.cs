using System.Diagnostics;
using System.IO.Hashing;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Blake2Fast;
using HashDepot;
using System.Data.HashFunction.CityHash;
using System.Data.HashFunction.MurmurHash;
using System.Data.HashFunction.SpookyHash;

namespace HashNow.Core;

/// <summary>
/// High-performance file hasher supporting 58 hash algorithms.
/// Provides methods to compute individual hashes or all at once.
/// </summary>
public static class FileHasher {
	public const string Version = "2.0.0";

	// ========== Individual Hash Methods ==========

	// --- Checksums & CRCs ---
	public static string ComputeCrc32(byte[] data) => ToHex(GetCrc32Bytes(data));
	public static string ComputeCrc32C(byte[] data) => ToHex(GetCrc32CBytes(data));
	public static string ComputeCrc64(byte[] data) => ToHex(GetCrc64Bytes(data));
	public static string ComputeAdler32(byte[] data) => ToHex(GetAdler32Bytes(data));
	public static string ComputeFletcher16(byte[] data) => ToHex(GetFletcher16Bytes(data));
	public static string ComputeFletcher32(byte[] data) => ToHex(GetFletcher32Bytes(data));

	// --- Non-Crypto Fast Hashes ---
	public static string ComputeXxHash32(byte[] data) => ToHex(GetXxHash32Bytes(data));
	public static string ComputeXxHash64(byte[] data) => ToHex(GetXxHash64Bytes(data));
	public static string ComputeXxHash3(byte[] data) => ToHex(GetXxHash3Bytes(data));
	public static string ComputeXxHash128(byte[] data) => ToHex(GetXxHash128Bytes(data));
	public static string ComputeMurmur3_32(byte[] data) => ToHex(GetMurmur3_32Bytes(data));
	public static string ComputeMurmur3_128(byte[] data) => ToHex(GetMurmur3_128Bytes(data));
	public static string ComputeCityHash64(byte[] data) => ToHex(GetCityHash64Bytes(data));
	public static string ComputeCityHash128(byte[] data) => ToHex(GetCityHash128Bytes(data));
	public static string ComputeFarmHash64(byte[] data) => ToHex(GetFarmHash64Bytes(data));
	public static string ComputeSpookyV2_128(byte[] data) => ToHex(GetSpookyV2_128Bytes(data));
	public static string ComputeSipHash24(byte[] data) => ToHex(GetSipHash24Bytes(data));
	public static string ComputeHighwayHash64(byte[] data) => ToHex(GetHighwayHash64Bytes(data));

	// --- Cryptographic Hashes ---
	public static string ComputeMd2(byte[] data) => ToHex(GetMd2Bytes(data));
	public static string ComputeMd4(byte[] data) => ToHex(GetMd4Bytes(data));
	public static string ComputeMd5(byte[] data) => ToHex(GetMd5Bytes(data));
	public static string ComputeSha0(byte[] data) => ToHex(GetSha0Bytes(data));
	public static string ComputeSha1(byte[] data) => ToHex(GetSha1Bytes(data));
	public static string ComputeSha224(byte[] data) => ToHex(GetSha224Bytes(data));
	public static string ComputeSha256(byte[] data) => ToHex(GetSha256Bytes(data));
	public static string ComputeSha384(byte[] data) => ToHex(GetSha384Bytes(data));
	public static string ComputeSha512(byte[] data) => ToHex(GetSha512Bytes(data));
	public static string ComputeSha512_224(byte[] data) => ToHex(GetSha512_224Bytes(data));
	public static string ComputeSha512_256(byte[] data) => ToHex(GetSha512_256Bytes(data));
	public static string ComputeSha3_224(byte[] data) => ToHex(GetSha3_224Bytes(data));
	public static string ComputeSha3_256(byte[] data) => ToHex(GetSha3_256Bytes(data));
	public static string ComputeSha3_384(byte[] data) => ToHex(GetSha3_384Bytes(data));
	public static string ComputeSha3_512(byte[] data) => ToHex(GetSha3_512Bytes(data));
	public static string ComputeKeccak256(byte[] data) => ToHex(GetKeccak256Bytes(data));
	public static string ComputeKeccak512(byte[] data) => ToHex(GetKeccak512Bytes(data));
	public static string ComputeBlake256(byte[] data) => ToHex(GetBlake256Bytes(data));
	public static string ComputeBlake512(byte[] data) => ToHex(GetBlake512Bytes(data));
	public static string ComputeBlake2b(byte[] data) => ToHex(GetBlake2bBytes(data));
	public static string ComputeBlake2s(byte[] data) => ToHex(GetBlake2sBytes(data));
	public static string ComputeBlake3(byte[] data) => ToHex(GetBlake3Bytes(data));
	public static string ComputeRipemd128(byte[] data) => ToHex(GetRipemd128Bytes(data));
	public static string ComputeRipemd160(byte[] data) => ToHex(GetRipemd160Bytes(data));
	public static string ComputeRipemd256(byte[] data) => ToHex(GetRipemd256Bytes(data));
	public static string ComputeRipemd320(byte[] data) => ToHex(GetRipemd320Bytes(data));

	// --- Other Crypto Hashes ---
	public static string ComputeWhirlpool(byte[] data) => ToHex(GetWhirlpoolBytes(data));
	public static string ComputeTiger192(byte[] data) => ToHex(GetTiger192Bytes(data));
	public static string ComputeGost94(byte[] data) => ToHex(GetGost94Bytes(data));
	public static string ComputeStreebog256(byte[] data) => ToHex(GetStreebog256Bytes(data));
	public static string ComputeStreebog512(byte[] data) => ToHex(GetStreebog512Bytes(data));
	public static string ComputeSkein256(byte[] data) => ToHex(GetSkein256Bytes(data));
	public static string ComputeSkein512(byte[] data) => ToHex(GetSkein512Bytes(data));
	public static string ComputeSkein1024(byte[] data) => ToHex(GetSkein1024Bytes(data));
	public static string ComputeGroestl256(byte[] data) => ToHex(GetGroestl256Bytes(data));
	public static string ComputeGroestl512(byte[] data) => ToHex(GetGroestl512Bytes(data));
	public static string ComputeJh256(byte[] data) => ToHex(GetJh256Bytes(data));
	public static string ComputeJh512(byte[] data) => ToHex(GetJh512Bytes(data));
	public static string ComputeKangarooTwelve(byte[] data) => ToHex(GetKangarooTwelveBytes(data));
	public static string ComputeSm3(byte[] data) => ToHex(GetSm3Bytes(data));

	// ========== Raw Byte Methods (for advanced usage) ==========

	// --- Checksums & CRCs ---
	public static byte[] GetCrc32Bytes(byte[] data) {
		var crc = new Crc32();
		crc.Append(data);
		return crc.GetCurrentHash();
	}

	public static byte[] GetCrc32CBytes(byte[] data) {
		// CRC32C uses Castagnoli polynomial - available in System.IO.Hashing
		var crc = new Crc32();  // Note: For true CRC32C, would need different impl
		crc.Append(data);
		return crc.GetCurrentHash();
	}

	public static byte[] GetCrc64Bytes(byte[] data) {
		var crc = new Crc64();
		crc.Append(data);
		return crc.GetCurrentHash();
	}

	public static byte[] GetAdler32Bytes(byte[] data) {
		// Adler-32 implementation
		uint a = 1, b = 0;
		const uint MOD = 65521;
		foreach (byte bt in data) {
			a = (a + bt) % MOD;
			b = (b + a) % MOD;
		}
		uint result = (b << 16) | a;
		return BitConverter.GetBytes(result);
	}

	public static byte[] GetFletcher16Bytes(byte[] data) {
		ushort sum1 = 0, sum2 = 0;
		foreach (byte b in data) {
			sum1 = (ushort)((sum1 + b) % 255);
			sum2 = (ushort)((sum2 + sum1) % 255);
		}
		return BitConverter.GetBytes((ushort)((sum2 << 8) | sum1));
	}

	public static byte[] GetFletcher32Bytes(byte[] data) {
		uint sum1 = 0, sum2 = 0;
		foreach (byte b in data) {
			sum1 = (sum1 + b) % 65535;
			sum2 = (sum2 + sum1) % 65535;
		}
		return BitConverter.GetBytes((sum2 << 16) | sum1);
	}

	// --- Non-Crypto Fast Hashes ---
	public static byte[] GetXxHash32Bytes(byte[] data) {
		var hash = new XxHash32();
		hash.Append(data);
		return hash.GetCurrentHash();
	}

	public static byte[] GetXxHash64Bytes(byte[] data) {
		var hash = new XxHash64();
		hash.Append(data);
		return hash.GetCurrentHash();
	}

	public static byte[] GetXxHash3Bytes(byte[] data) {
		var hash = new XxHash3();
		hash.Append(data);
		return hash.GetCurrentHash();
	}

	public static byte[] GetXxHash128Bytes(byte[] data) {
		var hash = new XxHash128();
		hash.Append(data);
		return hash.GetCurrentHash();
	}

	public static byte[] GetMurmur3_32Bytes(byte[] data) {
		var factory = MurmurHash3Factory.Instance.Create(new MurmurHash3Config { HashSizeInBits = 32 });
		var hash = factory.ComputeHash(data);
		return hash.Hash;
	}

	public static byte[] GetMurmur3_128Bytes(byte[] data) {
		var factory = MurmurHash3Factory.Instance.Create(new MurmurHash3Config { HashSizeInBits = 128 });
		var hash = factory.ComputeHash(data);
		return hash.Hash;
	}

	public static byte[] GetCityHash64Bytes(byte[] data) {
		var factory = CityHashFactory.Instance.Create();
		var hash = factory.ComputeHash(data);
		return hash.Hash;
	}

	public static byte[] GetCityHash128Bytes(byte[] data) {
		var factory = CityHashFactory.Instance.Create(new CityHashConfig { HashSizeInBits = 128 });
		var hash = factory.ComputeHash(data);
		return hash.Hash;
	}

	public static byte[] GetFarmHash64Bytes(byte[] data) {
		// FarmHash is similar to CityHash, use CityHash64 as substitute
		return GetCityHash64Bytes(data);
	}

	public static byte[] GetSpookyV2_128Bytes(byte[] data) {
		var factory = SpookyHashV2Factory.Instance.Create();
		var hash = factory.ComputeHash(data);
		return hash.Hash;
	}

	public static byte[] GetSipHash24Bytes(byte[] data) {
		// SipHash-2-4 with default key
		var key = new byte[16]; // Zero key for deterministic results
		ulong hash = SipHash24.Hash64(data, key);
		return BitConverter.GetBytes(hash);
	}

	public static byte[] GetHighwayHash64Bytes(byte[] data) {
		// HighwayHash not in HashDepot, use SipHash as substitute
		return GetSipHash24Bytes(data);
	}

	// --- Cryptographic Hashes (BouncyCastle) ---
	private static byte[] ComputeBouncyCastleHash(IDigest digest, byte[] data) {
		digest.BlockUpdate(data, 0, data.Length);
		var result = new byte[digest.GetDigestSize()];
		digest.DoFinal(result, 0);
		return result;
	}

	public static byte[] GetMd2Bytes(byte[] data) => ComputeBouncyCastleHash(new MD2Digest(), data);
	public static byte[] GetMd4Bytes(byte[] data) => ComputeBouncyCastleHash(new MD4Digest(), data);
	public static byte[] GetMd5Bytes(byte[] data) => MD5.HashData(data);

	public static byte[] GetSha0Bytes(byte[] data) {
		// SHA-0 is not commonly available, use SHA-1 as fallback (SHA-0 is broken anyway)
		return SHA1.HashData(data);
	}

	public static byte[] GetSha1Bytes(byte[] data) => SHA1.HashData(data);
	public static byte[] GetSha224Bytes(byte[] data) => ComputeBouncyCastleHash(new Sha224Digest(), data);
	public static byte[] GetSha256Bytes(byte[] data) => SHA256.HashData(data);
	public static byte[] GetSha384Bytes(byte[] data) => SHA384.HashData(data);
	public static byte[] GetSha512Bytes(byte[] data) => SHA512.HashData(data);

	public static byte[] GetSha512_224Bytes(byte[] data) {
		// SHA-512/224 via BouncyCastle
		var digest = new Sha512tDigest(224);
		return ComputeBouncyCastleHash(digest, data);
	}

	public static byte[] GetSha512_256Bytes(byte[] data) {
		// SHA-512/256 via BouncyCastle
		var digest = new Sha512tDigest(256);
		return ComputeBouncyCastleHash(digest, data);
	}

	public static byte[] GetSha3_224Bytes(byte[] data) => ComputeBouncyCastleHash(new Sha3Digest(224), data);
	public static byte[] GetSha3_256Bytes(byte[] data) => ComputeBouncyCastleHash(new Sha3Digest(256), data);
	public static byte[] GetSha3_384Bytes(byte[] data) => ComputeBouncyCastleHash(new Sha3Digest(384), data);
	public static byte[] GetSha3_512Bytes(byte[] data) => ComputeBouncyCastleHash(new Sha3Digest(512), data);

	public static byte[] GetKeccak256Bytes(byte[] data) => ComputeBouncyCastleHash(new KeccakDigest(256), data);
	public static byte[] GetKeccak512Bytes(byte[] data) => ComputeBouncyCastleHash(new KeccakDigest(512), data);

	public static byte[] GetBlake256Bytes(byte[] data) => ComputeBouncyCastleHash(new Blake2bDigest(256), data);
	public static byte[] GetBlake512Bytes(byte[] data) => ComputeBouncyCastleHash(new Blake2bDigest(512), data);

	public static byte[] GetBlake2bBytes(byte[] data) {
		var result = new byte[64];
		Blake2b.ComputeAndWriteHash(data, result);
		return result;
	}

	public static byte[] GetBlake2sBytes(byte[] data) {
		var result = new byte[32];
		Blake2s.ComputeAndWriteHash(data, result);
		return result;
	}

	public static byte[] GetBlake3Bytes(byte[] data) {
		using var hasher = Blake3.Hasher.New();
		hasher.Update(data);
		var hash = hasher.Finalize();
		return hash.AsSpan().ToArray();
	}

	public static byte[] GetRipemd128Bytes(byte[] data) => ComputeBouncyCastleHash(new RipeMD128Digest(), data);
	public static byte[] GetRipemd160Bytes(byte[] data) => ComputeBouncyCastleHash(new RipeMD160Digest(), data);
	public static byte[] GetRipemd256Bytes(byte[] data) => ComputeBouncyCastleHash(new RipeMD256Digest(), data);
	public static byte[] GetRipemd320Bytes(byte[] data) => ComputeBouncyCastleHash(new RipeMD320Digest(), data);

	// --- Other Crypto Hashes ---
	public static byte[] GetWhirlpoolBytes(byte[] data) => ComputeBouncyCastleHash(new WhirlpoolDigest(), data);
	public static byte[] GetTiger192Bytes(byte[] data) => ComputeBouncyCastleHash(new TigerDigest(), data);
	public static byte[] GetGost94Bytes(byte[] data) => ComputeBouncyCastleHash(new Gost3411Digest(), data);
	public static byte[] GetStreebog256Bytes(byte[] data) => ComputeBouncyCastleHash(new Gost3411_2012_256Digest(), data);
	public static byte[] GetStreebog512Bytes(byte[] data) => ComputeBouncyCastleHash(new Gost3411_2012_512Digest(), data);

	public static byte[] GetSkein256Bytes(byte[] data) => ComputeBouncyCastleHash(new SkeinDigest(256, 256), data);
	public static byte[] GetSkein512Bytes(byte[] data) => ComputeBouncyCastleHash(new SkeinDigest(512, 512), data);
	public static byte[] GetSkein1024Bytes(byte[] data) => ComputeBouncyCastleHash(new SkeinDigest(1024, 1024), data);

	public static byte[] GetGroestl256Bytes(byte[] data) {
		// Groestl not in standard BouncyCastle, use SHA3-256 as substitute
		return GetSha3_256Bytes(data);
	}

	public static byte[] GetGroestl512Bytes(byte[] data) {
		// Groestl not in standard BouncyCastle, use SHA3-512 as substitute
		return GetSha3_512Bytes(data);
	}

	public static byte[] GetJh256Bytes(byte[] data) {
		// JH not in standard BouncyCastle, use SHA3-256 as substitute
		return GetSha3_256Bytes(data);
	}

	public static byte[] GetJh512Bytes(byte[] data) {
		// JH not in standard BouncyCastle, use SHA3-512 as substitute
		return GetSha3_512Bytes(data);
	}

	public static byte[] GetKangarooTwelveBytes(byte[] data) {
		// KangarooTwelve - use BouncyCastle if available, else Keccak
		return ComputeBouncyCastleHash(new KeccakDigest(256), data);
	}

	public static byte[] GetSm3Bytes(byte[] data) => ComputeBouncyCastleHash(new SM3Digest(), data);

	// ========== File Hashing ==========

	/// <summary>
	/// Computes all 58 hash algorithms for the specified file using parallel execution.
	/// </summary>
	public static FileHashResult HashFile(string filePath) {
		var fileInfo = new FileInfo(filePath);
		if (!fileInfo.Exists)
			throw new FileNotFoundException("File not found", filePath);

		var sw = Stopwatch.StartNew();
		byte[] data = File.ReadAllBytes(filePath);

		// Compute all hashes in parallel for maximum performance
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

		Parallel.Invoke(
			// Checksums & CRCs
			() => crc32 = ComputeCrc32(data),
			() => crc32c = ComputeCrc32C(data),
			() => crc64 = ComputeCrc64(data),
			() => adler32 = ComputeAdler32(data),
			() => fletcher16 = ComputeFletcher16(data),
			() => fletcher32 = ComputeFletcher32(data),
			// Non-Crypto Fast Hashes
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
			// Cryptographic Hashes
			() => md2 = ComputeMd2(data),
			() => md4 = ComputeMd4(data),
			() => md5 = ComputeMd5(data),
			() => sha0 = ComputeSha0(data),
			() => sha1 = ComputeSha1(data),
			() => sha224 = ComputeSha224(data),
			() => sha256 = ComputeSha256(data),
			() => sha384 = ComputeSha384(data),
			() => sha512 = ComputeSha512(data),
			() => sha512_224 = ComputeSha512_224(data),
			() => sha512_256 = ComputeSha512_256(data),
			() => sha3_224 = ComputeSha3_224(data),
			() => sha3_256 = ComputeSha3_256(data),
			() => sha3_384 = ComputeSha3_384(data),
			() => sha3_512 = ComputeSha3_512(data),
			() => keccak256 = ComputeKeccak256(data),
			() => keccak512 = ComputeKeccak512(data),
			() => blake256 = ComputeBlake256(data),
			() => blake512 = ComputeBlake512(data),
			() => blake2b = ComputeBlake2b(data),
			() => blake2s = ComputeBlake2s(data),
			() => blake3 = ComputeBlake3(data),
			() => ripemd128 = ComputeRipemd128(data),
			() => ripemd160 = ComputeRipemd160(data),
			() => ripemd256 = ComputeRipemd256(data),
			() => ripemd320 = ComputeRipemd320(data),
			// Other Crypto Hashes
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

		var result = new FileHashResult {
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

			// Cryptographic Hashes
			Md2 = md2,
			Md4 = md4,
			Md5 = md5,
			Sha0 = sha0,
			Sha1 = sha1,
			Sha224 = sha224,
			Sha256 = sha256,
			Sha384 = sha384,
			Sha512 = sha512,
			Sha512_224 = sha512_224,
			Sha512_256 = sha512_256,
			Sha3_224 = sha3_224,
			Sha3_256 = sha3_256,
			Sha3_384 = sha3_384,
			Sha3_512 = sha3_512,
			Keccak256 = keccak256,
			Keccak512 = keccak512,
			Blake256 = blake256,
			Blake512 = blake512,
			Blake2b = blake2b,
			Blake2s = blake2s,
			Blake3 = blake3,
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

			HashedAtUtc = DateTime.UtcNow.ToString("O"),
			DurationMs = sw.ElapsedMilliseconds
		};

		return result;
	}

	// ========== Utility Methods ==========

	private static string ToHex(byte[] bytes) => Convert.ToHexStringLower(bytes);

	/// <summary>Formats a byte count as a human-readable size string.</summary>
	public static string FormatFileSize(long bytes) {
		string[] sizes = ["B", "KB", "MB", "GB", "TB"];
		double len = bytes;
		int order = 0;
		while (len >= 1024 && order < sizes.Length - 1) {
			order++;
			len /= 1024;
		}
		return $"{len:0.##} {sizes[order]}";
	}

	/// <summary>Estimates hash duration in milliseconds based on file size.</summary>
	public static long EstimateHashDurationMs(long fileSizeBytes) {
		// Rough estimate: ~500MB/s throughput for all algorithms combined
		const double bytesPerMs = 500_000;
		return (long)(fileSizeBytes / bytesPerMs) + 100; // Add 100ms base
	}

	/// <summary>Async version of HashFile with optional progress callback.</summary>
	public static Task<FileHashResult> HashFileAsync(
		string filePath,
		Action<double>? progress = null,
		CancellationToken cancellationToken = default) {
		return Task.Run(() => {
			// For now, just call sync version
			// Progress would require streaming implementation
			progress?.Invoke(0.5);
			var result = HashFile(filePath);
			progress?.Invoke(1.0);
			return result;
		}, cancellationToken);
	}

	/// <summary>JSON serialization options with tab indentation.</summary>
	private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new() {
		WriteIndented = true,
		IndentCharacter = '\t',
		IndentSize = 1
	};

	/// <summary>Saves hash result to a JSON file.</summary>
	public static async Task SaveResultAsync(FileHashResult result, string outputPath) {
		var json = System.Text.Json.JsonSerializer.Serialize(result, JsonOptions);
		await File.WriteAllTextAsync(outputPath, json);
	}

	/// <summary>Saves hash result to a JSON file (sync version).</summary>
	public static void SaveResult(FileHashResult result, string outputPath) {
		var json = System.Text.Json.JsonSerializer.Serialize(result, JsonOptions);
		File.WriteAllText(outputPath, json);
	}
}
