using System.Buffers;
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
				// Emit progress at integer percent boundaries to reduce per-iteration math.
				var progressStep = totalBytes / 100.0;
				var nextPercent = 1;
				var nextProgressAt = progressStep;
				int bytesThisRead;
				while ((bytesThisRead = stream.Read(buffer, 0, BufferSize)) > 0) {
					cancellationToken.ThrowIfCancellationRequested();
					ProcessChunk(buffer.AsSpan(0, bytesThisRead));
					bytesRead += bytesThisRead;
					while (nextPercent < 100 && bytesRead >= nextProgressAt) {
						progress(nextPercent);
						nextPercent++;
						nextProgressAt = progressStep * nextPercent;
					}
				}
				// Always report 100% at the end
				progress(100.0);
			}

			sw.Stop();

			// Finalize all hashes and materialize directly from StreamHash keys.
			var batchResults = _batchHasher.FinalizeAll();

			string Hash(string streamHashName) {
				if (batchResults.TryGetValue(streamHashName, out var value)) {
					return value;
				}

				throw new InvalidOperationException($"Missing hash result for '{streamHashName}'.");
			}

			return new FileHashResult {
				FileName = fileInfo.Name,
				FullPath = fileInfo.FullName,
				SizeBytes = fileInfo.Length,
				SizeFormatted = FileHasher.FormatFileSize(fileInfo.Length),
				CreatedUtc = fileInfo.CreationTimeUtc.ToString("O"),
				ModifiedUtc = fileInfo.LastWriteTimeUtc.ToString("O"),

				// Checksums & CRCs
				Crc32 = Hash("CRC32"),
				Crc32C = Hash("CRC32C"),
				Crc64 = Hash("CRC64"),
				Crc16Ccitt = Hash("CRC16-CCITT"),
				Crc16Modbus = Hash("CRC16-MODBUS"),
				Crc16Usb = Hash("CRC16-USB"),
				Adler32 = Hash("Adler-32"),
				Fletcher16 = Hash("Fletcher-16"),
				Fletcher32 = Hash("Fletcher-32"),

				// Non-Crypto Fast Hashes
				XxHash32 = Hash("xxHash32"),
				XxHash64 = Hash("xxHash64"),
				XxHash3 = Hash("xxHash3"),
				XxHash128 = Hash("xxHash128"),
				Murmur3_32 = Hash("MurmurHash3-32"),
				Murmur3_128 = Hash("MurmurHash3-128"),
				CityHash64 = Hash("CityHash64"),
				CityHash128 = Hash("CityHash128"),
				FarmHash64 = Hash("FarmHash64"),
				SpookyV2_128 = Hash("SpookyHash128"),
				SipHash24 = Hash("SipHash-2-4"),
				HighwayHash64 = Hash("HighwayHash64"),
				MetroHash64 = Hash("MetroHash64"),
				MetroHash128 = Hash("MetroHash128"),
				Wyhash64 = Hash("wyhash64"),
				Fnv1a32 = Hash("FNV-1a-32"),
				Fnv1a64 = Hash("FNV-1a-64"),
				Djb2 = Hash("DJB2"),
				Djb2a = Hash("DJB2a"),
				Sdbm = Hash("SDBM"),
				LoseLose = Hash("lose-lose"),

				// MD Family
				Md2 = Hash("MD2"),
				Md4 = Hash("MD4"),
				Md5 = Hash("MD5"),

				// SHA-1/2 Family
				Sha0 = Hash("SHA-0"),
				Sha1 = Hash("SHA-1"),
				Sha224 = Hash("SHA-224"),
				Sha256 = Hash("SHA-256"),
				Sha384 = Hash("SHA-384"),
				Sha512 = Hash("SHA-512"),
				Sha512_224 = Hash("SHA-512/224"),
				Sha512_256 = Hash("SHA-512/256"),

				// SHA-3 & Keccak
				Sha3_224 = Hash("SHA3-224"),
				Sha3_256 = Hash("SHA3-256"),
				Sha3_384 = Hash("SHA3-384"),
				Sha3_512 = Hash("SHA3-512"),
				Keccak256 = Hash("Keccak-256"),
				Keccak512 = Hash("Keccak-512"),

				// BLAKE Family
				Blake256 = Hash("BLAKE-256"),
				Blake512 = Hash("BLAKE-512"),
				Blake2b = Hash("BLAKE2b"),
				Blake2s = Hash("BLAKE2s"),
				Blake3 = Hash("BLAKE3"),

				// RIPEMD Family
				Ripemd128 = Hash("RIPEMD-128"),
				Ripemd160 = Hash("RIPEMD-160"),
				Ripemd256 = Hash("RIPEMD-256"),
				Ripemd320 = Hash("RIPEMD-320"),

				// Other Crypto Hashes
				Whirlpool = Hash("Whirlpool"),
				Tiger192 = Hash("Tiger-192"),
				Gost94 = Hash("GOST-94"),
				Streebog256 = Hash("Streebog-256"),
				Streebog512 = Hash("Streebog-512"),
				Skein256 = Hash("Skein-256"),
				Skein512 = Hash("Skein-512"),
				Skein1024 = Hash("Skein-1024"),
				Groestl256 = Hash("Grøstl-256"),
				Groestl512 = Hash("Grøstl-512"),
				Jh256 = Hash("JH-256"),
				Jh512 = Hash("JH-512"),
				KangarooTwelve = Hash("KangarooTwelve"),
				Sm3 = Hash("SM3"),

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
