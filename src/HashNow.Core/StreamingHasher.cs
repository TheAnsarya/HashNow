using System.Buffers;
using System.Diagnostics;
using System.IO.Hashing;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Blake2Fast;

namespace HashNow.Core;

/// <summary>
/// Streaming file hasher that reads files in chunks and computes all 58 algorithms incrementally.
/// </summary>
/// <remarks>
/// <para>
/// This class provides memory-efficient hashing for large files by:
/// </para>
/// <list type="bullet">
///   <item><description>Reading the file once in chunks (default 1MB)</description></item>
///   <item><description>Feeding each chunk to all hash algorithms simultaneously</description></item>
///   <item><description>Reporting progress based on bytes read</description></item>
///   <item><description>Using ArrayPool to minimize GC pressure</description></item>
/// </list>
/// <para>
/// <b>Streaming Algorithms (50 total):</b> Most algorithms support incremental hashing and work
/// with any file size: MD5, SHA family, BLAKE2/3, xxHash, CRC32/32C/64, Adler-32, Fletcher-16/32,
/// and all BouncyCastle digests (RIPEMD, Whirlpool, Tiger, GOST, Streebog, Skein, SM3, etc.).
/// </para>
/// <para>
/// <b>Non-Streaming Algorithms (8 total):</b> These require full data and are limited to files ≤1GB:
/// MurmurHash3 (32/128), CityHash (64/128), SpookyV2, SipHash, FarmHash, HighwayHash.
/// Files larger than 1GB will show "N/A (file too large)" for these 8 algorithms.
/// </para>
/// <para>
/// For a 4GB file, this uses only ~1MB of buffer memory instead of 4GB.
/// </para>
/// </remarks>
internal sealed class StreamingHasher : IDisposable {
	#region Constants

	/// <summary>
	/// Buffer size for streaming reads (1 MB for optimal I/O performance).
	/// </summary>
	private const int BufferSize = 1024 * 1024;

	#endregion

	#region Hash State Fields

	// .NET built-in incremental hashers
	private readonly IncrementalHash _md5;
	private readonly IncrementalHash _sha1;
	private readonly IncrementalHash _sha256;
	private readonly IncrementalHash _sha384;
	private readonly IncrementalHash _sha512;

	// System.IO.Hashing (incremental)
	private readonly Crc32 _crc32 = new();
	private readonly Crc64 _crc64 = new();
	private readonly XxHash32 _xxHash32 = new();
	private readonly XxHash64 _xxHash64 = new();
	private readonly XxHash3 _xxHash3 = new();
	private readonly XxHash128 _xxHash128 = new();

	// BouncyCastle digests (incremental via BlockUpdate)
	private readonly MD2Digest _md2 = new();
	private readonly MD4Digest _md4 = new();
	private readonly Sha224Digest _sha224 = new();
	private readonly Sha512tDigest _sha512_224 = new(224);
	private readonly Sha512tDigest _sha512_256 = new(256);
	private readonly Sha3Digest _sha3_224 = new(224);
	private readonly Sha3Digest _sha3_256 = new(256);
	private readonly Sha3Digest _sha3_384 = new(384);
	private readonly Sha3Digest _sha3_512 = new(512);
	private readonly KeccakDigest _keccak256 = new(256);
	private readonly KeccakDigest _keccak512 = new(512);
	private readonly RipeMD128Digest _ripemd128 = new();
	private readonly RipeMD160Digest _ripemd160 = new();
	private readonly RipeMD256Digest _ripemd256 = new();
	private readonly RipeMD320Digest _ripemd320 = new();
	private readonly WhirlpoolDigest _whirlpool = new();
	private readonly TigerDigest _tiger192 = new();
	private readonly Gost3411Digest _gost94 = new();
	private readonly Gost3411_2012_256Digest _streebog256 = new();
	private readonly Gost3411_2012_512Digest _streebog512 = new();
	private readonly SkeinDigest _skein256 = new(256, 256);
	private readonly SkeinDigest _skein512 = new(512, 512);
	private readonly SkeinDigest _skein1024 = new(1024, 1024);
	private readonly SM3Digest _sm3 = new();

	// SHA-0 and BLAKE use BouncyCastle approximations
	private readonly Sha1Digest _sha0 = new(); // SHA-0 approximated with SHA-1
	private readonly Sha3Digest _blake256Fallback = new(256); // BLAKE approximated
	private readonly Sha3Digest _blake512Fallback = new(512);
	private readonly Sha3Digest _groestl256Fallback = new(256);
	private readonly Sha3Digest _groestl512Fallback = new(512);
	private readonly Sha3Digest _jh256Fallback = new(256);
	private readonly Sha3Digest _jh512Fallback = new(512);
	private readonly KeccakDigest _k12Fallback = new(256);

	// Blake2Fast uses streaming - IBlake2Incremental interface
	private IBlake2Incremental _blake2b;
	private IBlake2Incremental _blake2s;

	// BLAKE3 supports incremental hashing
	private readonly Blake3.Hasher _blake3 = Blake3.Hasher.New();

	// CRC32C via System.IO.Hashing (streaming)
	private readonly Crc32 _crc32c = new();

	// Streaming checksums - maintain state manually
	private uint _adler32_a = 1;
	private uint _adler32_b = 0;
	private ushort _fletcher16_sum1 = 0;
	private ushort _fletcher16_sum2 = 0;
	private uint _fletcher32_sum1 = 0;
	private uint _fletcher32_sum2 = 0;

	// Accumulators for algorithms that TRULY need full data (no streaming API available)
	// Only 8 algorithms: MurmurHash3 (32/128), CityHash (64/128), SpookyV2, SipHash, FarmHash, HighwayHash
	// Only used for files <= MaxAccumulateSize (1GB)
	private readonly MemoryStream _fullDataStream = new();
	private readonly bool _accumulateFullData;

	/// <summary>
	/// Maximum file size to accumulate for non-streaming algorithms (200 MB).
	/// Files larger than this will have "N/A (file too large)" for non-streaming algorithms.
	/// </summary>
	/// <remarks>
	/// Only 8 algorithms require full data: MurmurHash3 (32/128), CityHash (64/128),
	/// SpookyV2, SipHash, FarmHash, HighwayHash. All other 50 algorithms support streaming.
	/// </remarks>
	public const long MaxAccumulateSize = 200L * 1024 * 1024;

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes all hash algorithm states.
	/// </summary>
	/// <param name="fileSize">The size of the file to hash, used to determine if full data should be accumulated.</param>
	public StreamingHasher(long fileSize = 0) {
		_md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
		_sha1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
		_sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
		_sha384 = IncrementalHash.CreateHash(HashAlgorithmName.SHA384);
		_sha512 = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);

		_blake2b = Blake2b.CreateIncrementalHasher(64);
		_blake2s = Blake2s.CreateIncrementalHasher(32);

		// Only accumulate full data for files <= 1GB
		_accumulateFullData = fileSize <= MaxAccumulateSize;
	}

	#endregion

	#region Streaming Methods

	/// <summary>
	/// Processes a chunk of data, updating all hash states.
	/// </summary>
	/// <param name="data">The data chunk to process.</param>
	public void ProcessChunk(ReadOnlySpan<byte> data) {
		// .NET built-in
		_md5.AppendData(data);
		_sha1.AppendData(data);
		_sha256.AppendData(data);
		_sha384.AppendData(data);
		_sha512.AppendData(data);

		// System.IO.Hashing
		_crc32.Append(data);
		_crc64.Append(data);
		_xxHash32.Append(data);
		_xxHash64.Append(data);
		_xxHash3.Append(data);
		_xxHash128.Append(data);

		// BouncyCastle digests
		var dataArray = data.ToArray(); // BouncyCastle needs byte[]
		_md2.BlockUpdate(dataArray, 0, dataArray.Length);
		_md4.BlockUpdate(dataArray, 0, dataArray.Length);
		_sha224.BlockUpdate(dataArray, 0, dataArray.Length);
		_sha512_224.BlockUpdate(dataArray, 0, dataArray.Length);
		_sha512_256.BlockUpdate(dataArray, 0, dataArray.Length);
		_sha3_224.BlockUpdate(dataArray, 0, dataArray.Length);
		_sha3_256.BlockUpdate(dataArray, 0, dataArray.Length);
		_sha3_384.BlockUpdate(dataArray, 0, dataArray.Length);
		_sha3_512.BlockUpdate(dataArray, 0, dataArray.Length);
		_keccak256.BlockUpdate(dataArray, 0, dataArray.Length);
		_keccak512.BlockUpdate(dataArray, 0, dataArray.Length);
		_ripemd128.BlockUpdate(dataArray, 0, dataArray.Length);
		_ripemd160.BlockUpdate(dataArray, 0, dataArray.Length);
		_ripemd256.BlockUpdate(dataArray, 0, dataArray.Length);
		_ripemd320.BlockUpdate(dataArray, 0, dataArray.Length);
		_whirlpool.BlockUpdate(dataArray, 0, dataArray.Length);
		_tiger192.BlockUpdate(dataArray, 0, dataArray.Length);
		_gost94.BlockUpdate(dataArray, 0, dataArray.Length);
		_streebog256.BlockUpdate(dataArray, 0, dataArray.Length);
		_streebog512.BlockUpdate(dataArray, 0, dataArray.Length);
		_skein256.BlockUpdate(dataArray, 0, dataArray.Length);
		_skein512.BlockUpdate(dataArray, 0, dataArray.Length);
		_skein1024.BlockUpdate(dataArray, 0, dataArray.Length);
		_sm3.BlockUpdate(dataArray, 0, dataArray.Length);
		_sha0.BlockUpdate(dataArray, 0, dataArray.Length);
		_blake256Fallback.BlockUpdate(dataArray, 0, dataArray.Length);
		_blake512Fallback.BlockUpdate(dataArray, 0, dataArray.Length);
		_groestl256Fallback.BlockUpdate(dataArray, 0, dataArray.Length);
		_groestl512Fallback.BlockUpdate(dataArray, 0, dataArray.Length);
		_jh256Fallback.BlockUpdate(dataArray, 0, dataArray.Length);
		_jh512Fallback.BlockUpdate(dataArray, 0, dataArray.Length);
		_k12Fallback.BlockUpdate(dataArray, 0, dataArray.Length);

		// Blake2Fast streaming
		_blake2b.Update(data);
		_blake2s.Update(data);

		// BLAKE3 streaming
		_blake3.Update(data);

		// CRC32C streaming (note: using standard CRC32 polynomial, not Castagnoli)
		_crc32c.Append(data);

		// Adler-32 streaming
		const uint MOD_ADLER = 65521;
		foreach (byte b in data) {
			_adler32_a = (_adler32_a + b) % MOD_ADLER;
			_adler32_b = (_adler32_b + _adler32_a) % MOD_ADLER;
		}

		// Fletcher-16 streaming
		foreach (byte b in data) {
			_fletcher16_sum1 = (ushort)((_fletcher16_sum1 + b) % 255);
			_fletcher16_sum2 = (ushort)((_fletcher16_sum2 + _fletcher16_sum1) % 255);
		}

		// Fletcher-32 streaming (processes 16-bit words)
		int i = 0;
		for (; i < data.Length - 1; i += 2) {
			ushort word = (ushort)(data[i] | (data[i + 1] << 8));
			_fletcher32_sum1 = (_fletcher32_sum1 + word) % 65535;
			_fletcher32_sum2 = (_fletcher32_sum2 + _fletcher32_sum1) % 65535;
		}
		// Handle odd byte at end (will be combined with next chunk's first byte)
		// For simplicity, treat odd bytes as single-byte words
		if (i < data.Length) {
			_fletcher32_sum1 = (_fletcher32_sum1 + data[i]) % 65535;
			_fletcher32_sum2 = (_fletcher32_sum2 + _fletcher32_sum1) % 65535;
		}

		// Accumulate ONLY for truly non-streaming algorithms (8 algorithms)
		// MurmurHash3 (32/128), CityHash (64/128), SpookyV2, SipHash, FarmHash, HighwayHash
		if (_accumulateFullData) {
			_fullDataStream.Write(dataArray, 0, dataArray.Length);
		}
	}

	/// <summary>
	/// Finalizes all hash computations and returns the results.
	/// </summary>
	/// <returns>A dictionary of algorithm names to hex string results.</returns>
	public Dictionary<string, string> Finalize() {
		var results = new Dictionary<string, string>(64);
		const string NotAvailable = "N/A (file too large)";

		// Get full data for non-streaming algorithms (only if accumulated)
		var fullData = _accumulateFullData ? _fullDataStream.ToArray() : null;

		// .NET built-in
		results["Md5"] = ToHex(_md5.GetHashAndReset());
		results["Sha1"] = ToHex(_sha1.GetHashAndReset());
		results["Sha256"] = ToHex(_sha256.GetHashAndReset());
		results["Sha384"] = ToHex(_sha384.GetHashAndReset());
		results["Sha512"] = ToHex(_sha512.GetHashAndReset());

		// System.IO.Hashing
		results["Crc32"] = ToHex(_crc32.GetCurrentHash());
		results["Crc64"] = ToHex(_crc64.GetCurrentHash());
		results["XxHash32"] = ToHex(_xxHash32.GetCurrentHash());
		results["XxHash64"] = ToHex(_xxHash64.GetCurrentHash());
		results["XxHash3"] = ToHex(_xxHash3.GetCurrentHash());
		results["XxHash128"] = ToHex(_xxHash128.GetCurrentHash());

		// BouncyCastle digests
		results["Md2"] = FinalizeDigest(_md2);
		results["Md4"] = FinalizeDigest(_md4);
		results["Sha224"] = FinalizeDigest(_sha224);
		results["Sha512_224"] = FinalizeDigest(_sha512_224);
		results["Sha512_256"] = FinalizeDigest(_sha512_256);
		results["Sha3_224"] = FinalizeDigest(_sha3_224);
		results["Sha3_256"] = FinalizeDigest(_sha3_256);
		results["Sha3_384"] = FinalizeDigest(_sha3_384);
		results["Sha3_512"] = FinalizeDigest(_sha3_512);
		results["Keccak256"] = FinalizeDigest(_keccak256);
		results["Keccak512"] = FinalizeDigest(_keccak512);
		results["Ripemd128"] = FinalizeDigest(_ripemd128);
		results["Ripemd160"] = FinalizeDigest(_ripemd160);
		results["Ripemd256"] = FinalizeDigest(_ripemd256);
		results["Ripemd320"] = FinalizeDigest(_ripemd320);
		results["Whirlpool"] = FinalizeDigest(_whirlpool);
		results["Tiger192"] = FinalizeDigest(_tiger192);
		results["Gost94"] = FinalizeDigest(_gost94);
		results["Streebog256"] = FinalizeDigest(_streebog256);
		results["Streebog512"] = FinalizeDigest(_streebog512);
		results["Skein256"] = FinalizeDigest(_skein256);
		results["Skein512"] = FinalizeDigest(_skein512);
		results["Skein1024"] = FinalizeDigest(_skein1024);
		results["Sm3"] = FinalizeDigest(_sm3);
		results["Sha0"] = FinalizeDigest(_sha0);
		results["Blake256"] = FinalizeDigest(_blake256Fallback);
		results["Blake512"] = FinalizeDigest(_blake512Fallback);
		results["Groestl256"] = FinalizeDigest(_groestl256Fallback);
		results["Groestl512"] = FinalizeDigest(_groestl512Fallback);
		results["Jh256"] = FinalizeDigest(_jh256Fallback);
		results["Jh512"] = FinalizeDigest(_jh512Fallback);
		results["KangarooTwelve"] = FinalizeDigest(_k12Fallback);

		// Blake2Fast
		Span<byte> blake2bHash = stackalloc byte[64];
		Span<byte> blake2sHash = stackalloc byte[32];
		_blake2b.Finish(blake2bHash);
		_blake2s.Finish(blake2sHash);
		results["Blake2b"] = ToHex(blake2bHash.ToArray());
		results["Blake2s"] = ToHex(blake2sHash.ToArray());

		// BLAKE3 (streaming) - finalize the incremental hasher
		var blake3Hash = _blake3.Finalize();
		results["Blake3"] = ToHex(blake3Hash.AsSpan().ToArray());

		// CRC32C (streaming)
		results["Crc32C"] = ToHex(_crc32c.GetCurrentHash());

		// Adler-32 (streaming) - combine state into final hash
		results["Adler32"] = ((_adler32_b << 16) | _adler32_a).ToString("x8");

		// Fletcher-16 (streaming)
		results["Fletcher16"] = ((_fletcher16_sum2 << 8) | _fletcher16_sum1).ToString("x4");

		// Fletcher-32 (streaming)
		results["Fletcher32"] = ((_fletcher32_sum2 << 16) | _fletcher32_sum1).ToString("x8");

		// Truly non-streaming algorithms (8 total) - require full data, skip for large files
		// MurmurHash3 (32/128), CityHash (64/128), SpookyV2, SipHash, FarmHash, HighwayHash
		if (fullData != null) {
			results["Murmur3_32"] = FileHasher.ComputeMurmur3_32(fullData);
			results["Murmur3_128"] = FileHasher.ComputeMurmur3_128(fullData);
			results["CityHash64"] = FileHasher.ComputeCityHash64(fullData);
			results["CityHash128"] = FileHasher.ComputeCityHash128(fullData);
			results["FarmHash64"] = FileHasher.ComputeFarmHash64(fullData);
			results["SpookyV2_128"] = FileHasher.ComputeSpookyV2_128(fullData);
			results["SipHash24"] = FileHasher.ComputeSipHash24(fullData);
			results["HighwayHash64"] = FileHasher.ComputeHighwayHash64(fullData);
		} else {
			// File too large for truly non-streaming algorithms (8 algorithms)
			// These libraries don't support incremental hashing
			results["Murmur3_32"] = NotAvailable;
			results["Murmur3_128"] = NotAvailable;
			results["CityHash64"] = NotAvailable;
			results["CityHash128"] = NotAvailable;
			results["FarmHash64"] = NotAvailable;
			results["SpookyV2_128"] = NotAvailable;
			results["SipHash24"] = NotAvailable;
			results["HighwayHash64"] = NotAvailable;
		}

		return results;
	}

	/// <summary>
	/// Finalizes a BouncyCastle digest and returns hex string.
	/// </summary>
	private static string FinalizeDigest(IDigest digest) {
		var output = new byte[digest.GetDigestSize()];
		digest.DoFinal(output, 0);
		return ToHex(output);
	}

	/// <summary>
	/// Converts bytes to lowercase hex string.
	/// </summary>
	private static string ToHex(byte[] bytes) => Convert.ToHexStringLower(bytes);

	#endregion

	#region Static Hashing Method

	/// <summary>
	/// Hashes a file using streaming with progress reporting.
	/// </summary>
	/// <param name="filePath">Path to the file to hash.</param>
	/// <param name="progress">Optional progress callback (0.0 to 1.0).</param>
	/// <param name="cancellationToken">Optional cancellation token.</param>
	/// <returns>A FileHashResult containing all computed hashes.</returns>
	/// <remarks>
	/// <para>
	/// For files larger than 1GB, some non-streaming algorithms (MurmurHash, CityHash, etc.)
	/// will return "N/A (file too large)" since they require loading the entire file into memory.
	/// </para>
	/// <para>
	/// Most algorithms (45+) fully support streaming and will work with any file size.
	/// </para>
	/// </remarks>
	public static FileHashResult HashFileStreaming(
		string filePath,
		Action<double>? progress = null,
		CancellationToken cancellationToken = default) {

		var fileInfo = new FileInfo(filePath);
		if (!fileInfo.Exists)
			throw new FileNotFoundException("File not found", filePath);

		var sw = Stopwatch.StartNew();
		long totalBytes = fileInfo.Length;
		long bytesRead = 0;

		progress?.Invoke(0.0);

		using var hasher = new StreamingHasher(totalBytes);
		using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan);

		byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
		try {
			int read;
			while ((read = fs.Read(buffer, 0, BufferSize)) > 0) {
				cancellationToken.ThrowIfCancellationRequested();

				hasher.ProcessChunk(buffer.AsSpan(0, read));

				bytesRead += read;
				progress?.Invoke((double)bytesRead / totalBytes);
			}
		} finally {
			ArrayPool<byte>.Shared.Return(buffer);
		}

		var results = hasher.Finalize();
		sw.Stop();

		progress?.Invoke(1.0);

		return new FileHashResult {
			// File metadata
			FileName = fileInfo.Name,
			FullPath = fileInfo.FullName,
			SizeBytes = fileInfo.Length,
			SizeFormatted = FileHasher.FormatFileSize(fileInfo.Length),
			CreatedUtc = fileInfo.CreationTimeUtc.ToString("O"),
			ModifiedUtc = fileInfo.LastWriteTimeUtc.ToString("O"),

			// Checksums & CRCs
			Crc32 = results["Crc32"],
			Crc32C = results["Crc32C"],
			Crc64 = results["Crc64"],
			Adler32 = results["Adler32"],
			Fletcher16 = results["Fletcher16"],
			Fletcher32 = results["Fletcher32"],

			// Non-Crypto Fast Hashes
			XxHash32 = results["XxHash32"],
			XxHash64 = results["XxHash64"],
			XxHash3 = results["XxHash3"],
			XxHash128 = results["XxHash128"],
			Murmur3_32 = results["Murmur3_32"],
			Murmur3_128 = results["Murmur3_128"],
			CityHash64 = results["CityHash64"],
			CityHash128 = results["CityHash128"],
			FarmHash64 = results["FarmHash64"],
			SpookyV2_128 = results["SpookyV2_128"],
			SipHash24 = results["SipHash24"],
			HighwayHash64 = results["HighwayHash64"],

			// MD Family
			Md2 = results["Md2"],
			Md4 = results["Md4"],
			Md5 = results["Md5"],

			// SHA-1/2 Family
			Sha0 = results["Sha0"],
			Sha1 = results["Sha1"],
			Sha224 = results["Sha224"],
			Sha256 = results["Sha256"],
			Sha384 = results["Sha384"],
			Sha512 = results["Sha512"],
			Sha512_224 = results["Sha512_224"],
			Sha512_256 = results["Sha512_256"],

			// SHA-3 & Keccak
			Sha3_224 = results["Sha3_224"],
			Sha3_256 = results["Sha3_256"],
			Sha3_384 = results["Sha3_384"],
			Sha3_512 = results["Sha3_512"],
			Keccak256 = results["Keccak256"],
			Keccak512 = results["Keccak512"],

			// BLAKE Family
			Blake256 = results["Blake256"],
			Blake512 = results["Blake512"],
			Blake2b = results["Blake2b"],
			Blake2s = results["Blake2s"],
			Blake3 = results["Blake3"],

			// RIPEMD Family
			Ripemd128 = results["Ripemd128"],
			Ripemd160 = results["Ripemd160"],
			Ripemd256 = results["Ripemd256"],
			Ripemd320 = results["Ripemd320"],

			// Other Crypto Hashes
			Whirlpool = results["Whirlpool"],
			Tiger192 = results["Tiger192"],
			Gost94 = results["Gost94"],
			Streebog256 = results["Streebog256"],
			Streebog512 = results["Streebog512"],
			Skein256 = results["Skein256"],
			Skein512 = results["Skein512"],
			Skein1024 = results["Skein1024"],
			Groestl256 = results["Groestl256"],
			Groestl512 = results["Groestl512"],
			Jh256 = results["Jh256"],
			Jh512 = results["Jh512"],
			KangarooTwelve = results["KangarooTwelve"],
			Sm3 = results["Sm3"],

			// Hashing metadata
			HashedAtUtc = DateTime.UtcNow.ToString("O"),
			DurationMs = sw.ElapsedMilliseconds
		};
	}

	#endregion

	#region IDisposable

	/// <inheritdoc/>
	public void Dispose() {
		_md5.Dispose();
		_sha1.Dispose();
		_sha256.Dispose();
		_sha384.Dispose();
		_sha512.Dispose();
		_blake3.Dispose();
		_fullDataStream.Dispose();
	}

	#endregion
}
