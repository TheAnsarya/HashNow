using System.Buffers;
using System.Diagnostics;
using StreamHash.Core;
using StreamHash.Core.Abstractions;

namespace HashNow.Core;

/// <summary>
/// Streaming file hasher using StreamHash's batch API for parallel processing of all 70 algorithms.
/// </summary>
/// <remarks>
/// <para>
/// This class provides memory-efficient hashing for large files by:
/// </para>
/// <list type="bullet">
///   <item><description>Reading the file once in chunks (default 1MB)</description></item>
///   <item><description>Feeding each chunk to all hash algorithms simultaneously via batch API</description></item>
///   <item><description>Reporting progress based on bytes read</description></item>
///   <item><description>Using ArrayPool to minimize GC pressure</description></item>
/// </list>
/// <para>
/// All 70 algorithms are computed in a single pass through the file using StreamHash's
/// unified batch streaming interface with parallel processing for 8-16x speedup.
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

	/// <summary>
	/// Batch hasher for parallel processing of all 70 algorithms.
	/// </summary>
	private readonly IMultiStreamingHashBytes _batchHasher;

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes all hash algorithm states using StreamHash batch API.
	/// </summary>
	public StreamingHasher() {
		_batchHasher = HashFacade.CreateAllStreaming();
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Hashes a file and returns all 70 hash values.
	/// </summary>
	/// <param name="filePath">Path to the file to hash.</param>
	/// <param name="progress">Optional progress callback (0-100%).</param>
	/// <param name="cancellationToken">Cancellation token to abort the operation.</param>
	/// <returns>Complete hash results for the file.</returns>
	public FileHashResult HashFile(string filePath, Action<double>? progress = null, CancellationToken cancellationToken = default) {
		var fileInfo = new FileInfo(filePath);
		var sw = Stopwatch.StartNew();

		// Read file in chunks
		byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
		try {
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan);

			long totalBytes = stream.Length;
			long bytesRead = 0;

			int bytesThisRead;
			while ((bytesThisRead = stream.Read(buffer, 0, BufferSize)) > 0) {
				cancellationToken.ThrowIfCancellationRequested();
				
				// Process chunk through all 70 algorithms
				ProcessChunk(buffer.AsSpan(0, bytesThisRead));
				
				bytesRead += bytesThisRead;
				if (progress != null && totalBytes > 0) {
					double percent = (bytesRead * 100.0) / totalBytes;
					progress(percent);
				}
			}

			sw.Stop();

			// Finalize all hashes
			var results = FinalizeAll();

			return new FileHashResult {
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
				Crc16Ccitt = results["Crc16Ccitt"],
				Crc16Modbus = results["Crc16Modbus"],
				Crc16Usb = results["Crc16Usb"],
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
				MetroHash64 = results["MetroHash64"],
				MetroHash128 = results["MetroHash128"],
				Wyhash64 = results["Wyhash64"],
				Fnv1a32 = results["Fnv1a32"],
				Fnv1a64 = results["Fnv1a64"],
				Djb2 = results["Djb2"],
				Djb2a = results["Djb2a"],
				Sdbm = results["Sdbm"],
				LoseLose = results["LoseLose"],

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
		} finally {
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Processes a data chunk through all hash algorithms.
	/// </summary>
	private void ProcessChunk(ReadOnlySpan<byte> data) {
		_batchHasher.Update(data);
	}

	/// <summary>
	/// Finalizes all hashes and returns the results as a dictionary.
	/// </summary>
	/// <returns>Dictionary mapping FileHashResult property names to hex string results.</returns>
	private Dictionary<string, string> FinalizeAll() {
		var batchResults = _batchHasher.FinalizeAll();
		var results = new Dictionary<string, string>(70);

		// Map StreamHash algorithm names to FileHashResult property names
		// StreamHash returns keys like "MurmurHash3-32", we need "Murmur3_32"
		foreach (var kvp in batchResults) {
			string propertyName = MapStreamHashNameToPropertyName(kvp.Key);
			results[propertyName] = kvp.Value;
		}

		return results;
	}

	/// <summary>
	/// Maps StreamHash algorithm names to FileHashResult property names.
	/// </summary>
	private static string MapStreamHashNameToPropertyName(string streamHashName) {
		return streamHashName switch {
			// Checksums (exact matches)
			"CRC32" => "Crc32",
			"CRC32C" => "Crc32C",
			"CRC64" => "Crc64",
			"CRC16-CCITT" => "Crc16Ccitt",
			"CRC16-MODBUS" => "Crc16Modbus",
			"CRC16-USB" => "Crc16Usb",
			"Adler-32" => "Adler32",
			"Fletcher-16" => "Fletcher16",
			"Fletcher-32" => "Fletcher32",

			// Non-Crypto Fast (exact matches mostly)
			"xxHash32" => "XxHash32",
			"xxHash64" => "XxHash64",
			"xxHash3" => "XxHash3",
			"xxHash128" => "XxHash128",
			"MurmurHash3-32" => "Murmur3_32",
			"MurmurHash3-128" => "Murmur3_128",
			"CityHash64" => "CityHash64",
			"CityHash128" => "CityHash128",
			"FarmHash64" => "FarmHash64",
			"SpookyHash128" => "SpookyV2_128",
			"SipHash-2-4" => "SipHash24",
			"HighwayHash64" => "HighwayHash64",
			"MetroHash64" => "MetroHash64",
			"MetroHash128" => "MetroHash128",
			"wyhash64" => "Wyhash64",
			"FNV-1a-32" => "Fnv1a32",
			"FNV-1a-64" => "Fnv1a64",
			"DJB2" => "Djb2",
			"DJB2a" => "Djb2a",
			"SDBM" => "Sdbm",
			"lose-lose" => "LoseLose",

			// MD Family
			"MD2" => "Md2",
			"MD4" => "Md4",
			"MD5" => "Md5",

			// SHA-1/2 Family
			"SHA-0" => "Sha0",
			"SHA-1" => "Sha1",
			"SHA-224" => "Sha224",
			"SHA-256" => "Sha256",
			"SHA-384" => "Sha384",
			"SHA-512" => "Sha512",
			"SHA-512/224" => "Sha512_224",
			"SHA-512/256" => "Sha512_256",

			// SHA-3 & Keccak
			"SHA3-224" => "Sha3_224",
			"SHA3-256" => "Sha3_256",
			"SHA3-384" => "Sha3_384",
			"SHA3-512" => "Sha3_512",
			"Keccak-256" => "Keccak256",
			"Keccak-512" => "Keccak512",

			// BLAKE Family
			"BLAKE-256" => "Blake256",
			"BLAKE-512" => "Blake512",
			"BLAKE2b" => "Blake2b",
			"BLAKE2s" => "Blake2s",
			"BLAKE3" => "Blake3",

			// RIPEMD Family
			"RIPEMD-128" => "Ripemd128",
			"RIPEMD-160" => "Ripemd160",
			"RIPEMD-256" => "Ripemd256",
			"RIPEMD-320" => "Ripemd320",

			// Other Crypto
			"Whirlpool" => "Whirlpool",
			"Tiger-192" => "Tiger192",
			"GOST-94" => "Gost94",
			"Streebog-256" => "Streebog256",
			"Streebog-512" => "Streebog512",
			"Skein-256" => "Skein256",
			"Skein-512" => "Skein512",
			"Skein-1024" => "Skein1024",
			"Grøstl-256" => "Groestl256",
			"Grøstl-512" => "Groestl512",
			"JH-256" => "Jh256",
			"JH-512" => "Jh512",
			"KangarooTwelve" => "KangarooTwelve",
			"SM3" => "Sm3",

			// If unmapped, return as-is (should not happen)
			_ => streamHashName
		};
	}

	#endregion

	#region IDisposable

	/// <summary>
	/// Disposes the batch hasher.
	/// </summary>
	public void Dispose() {
		_batchHasher?.Dispose();
	}

	#endregion
}
