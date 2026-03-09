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

			if (progress is null || totalBytes <= 0) {
				int bytesThisRead;
				while ((bytesThisRead = stream.Read(buffer, 0, BufferSize)) > 0) {
					cancellationToken.ThrowIfCancellationRequested();
					ProcessChunk(buffer.AsSpan(0, bytesThisRead));
					bytesRead += bytesThisRead;
				}
			} else {
				// Throttle progress callbacks to fire only when progress changes by >= 1%
				var progressScale = 100.0 / totalBytes;
				double lastReportedProgress = -1.0;
				int bytesThisRead;
				while ((bytesThisRead = stream.Read(buffer, 0, BufferSize)) > 0) {
					cancellationToken.ThrowIfCancellationRequested();
					ProcessChunk(buffer.AsSpan(0, bytesThisRead));
					bytesRead += bytesThisRead;
					double currentProgress = bytesRead * progressScale;
					if (currentProgress - lastReportedProgress >= 1.0) {
						progress(currentProgress);
						lastReportedProgress = currentProgress;
					}
				}
				// Always report 100% at the end
				progress(100.0);
			}

			sw.Stop();

			// Finalize all hashes with a zero-allocation property-name view.
			var results = new HashResultsView(_batchHasher.FinalizeAll());

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
	/// Provides lazy property-name lookups over StreamHash batch results.
	/// </summary>
	private readonly struct HashResultsView(IReadOnlyDictionary<string, string> batchResults) {
		private readonly IReadOnlyDictionary<string, string> _batchResults = batchResults;

		public string this[string propertyName] {
			get {
				var streamHashName = propertyName switch {
					"Crc32" => "CRC32",
					"Crc32C" => "CRC32C",
					"Crc64" => "CRC64",
					"Crc16Ccitt" => "CRC16-CCITT",
					"Crc16Modbus" => "CRC16-MODBUS",
					"Crc16Usb" => "CRC16-USB",
					"Adler32" => "Adler-32",
					"Fletcher16" => "Fletcher-16",
					"Fletcher32" => "Fletcher-32",
					"XxHash32" => "xxHash32",
					"XxHash64" => "xxHash64",
					"XxHash3" => "xxHash3",
					"XxHash128" => "xxHash128",
					"Murmur3_32" => "MurmurHash3-32",
					"Murmur3_128" => "MurmurHash3-128",
					"CityHash64" => "CityHash64",
					"CityHash128" => "CityHash128",
					"FarmHash64" => "FarmHash64",
					"SpookyV2_128" => "SpookyHash128",
					"SipHash24" => "SipHash-2-4",
					"HighwayHash64" => "HighwayHash64",
					"MetroHash64" => "MetroHash64",
					"MetroHash128" => "MetroHash128",
					"Wyhash64" => "wyhash64",
					"Fnv1a32" => "FNV-1a-32",
					"Fnv1a64" => "FNV-1a-64",
					"Djb2" => "DJB2",
					"Djb2a" => "DJB2a",
					"Sdbm" => "SDBM",
					"LoseLose" => "lose-lose",
					"Md2" => "MD2",
					"Md4" => "MD4",
					"Md5" => "MD5",
					"Sha0" => "SHA-0",
					"Sha1" => "SHA-1",
					"Sha224" => "SHA-224",
					"Sha256" => "SHA-256",
					"Sha384" => "SHA-384",
					"Sha512" => "SHA-512",
					"Sha512_224" => "SHA-512/224",
					"Sha512_256" => "SHA-512/256",
					"Sha3_224" => "SHA3-224",
					"Sha3_256" => "SHA3-256",
					"Sha3_384" => "SHA3-384",
					"Sha3_512" => "SHA3-512",
					"Keccak256" => "Keccak-256",
					"Keccak512" => "Keccak-512",
					"Blake256" => "BLAKE-256",
					"Blake512" => "BLAKE-512",
					"Blake2b" => "BLAKE2b",
					"Blake2s" => "BLAKE2s",
					"Blake3" => "BLAKE3",
					"Ripemd128" => "RIPEMD-128",
					"Ripemd160" => "RIPEMD-160",
					"Ripemd256" => "RIPEMD-256",
					"Ripemd320" => "RIPEMD-320",
					"Whirlpool" => "Whirlpool",
					"Tiger192" => "Tiger-192",
					"Gost94" => "GOST-94",
					"Streebog256" => "Streebog-256",
					"Streebog512" => "Streebog-512",
					"Skein256" => "Skein-256",
					"Skein512" => "Skein-512",
					"Skein1024" => "Skein-1024",
					"Groestl256" => "Grøstl-256",
					"Groestl512" => "Grøstl-512",
					"Jh256" => "JH-256",
					"Jh512" => "JH-512",
					"KangarooTwelve" => "KangarooTwelve",
					"Sm3" => "SM3",
					_ => throw new InvalidOperationException($"Unsupported hash property mapping: {propertyName}.")
				};

				if (_batchResults.TryGetValue(streamHashName, out var value)) {
					return value;
				}

				throw new InvalidOperationException($"Missing hash result for '{streamHashName}' while populating '{propertyName}'.");
			}
		}
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
