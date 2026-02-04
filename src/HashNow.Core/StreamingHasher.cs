using System.Buffers;
using System.Diagnostics;
using StreamHash.Core;

namespace HashNow.Core;

/// <summary>
/// Streaming file hasher that uses StreamHash's HashFacade for all 70 algorithms.
/// </summary>
/// <remarks>
/// <para>
/// This class provides memory-efficient hashing for large files by:
/// </para>
/// <list type="bullet">
///   <item><description>Reading the file once in chunks (default 1MB)</description></item>
///   <item><description>Feeding each chunk to all hash algorithms simultaneously via HashFacade</description></item>
///   <item><description>Reporting progress based on bytes read</description></item>
///   <item><description>Using ArrayPool to minimize GC pressure</description></item>
/// </list>
/// <para>
/// All 70 algorithms are computed in a single pass through the file using StreamHash's
/// unified streaming interface.
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
	/// Dictionary of all streaming hashers, keyed by algorithm.
	/// </summary>
	private readonly Dictionary<HashAlgorithm, IStreamingHashBytes> _hashers;

	/// <summary>
	/// List of all algorithms for iteration.
	/// </summary>
	private static readonly HashAlgorithm[] AllAlgorithms = Enum.GetValues<HashAlgorithm>();

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes all hash algorithm states using HashFacade.
	/// </summary>
	public StreamingHasher() {
		_hashers = new Dictionary<HashAlgorithm, IStreamingHashBytes>(AllAlgorithms.Length);

		foreach (var algorithm in AllAlgorithms) {
			_hashers[algorithm] = HashFacade.CreateStreaming(algorithm);
		}
	}

	#endregion

	#region Streaming Methods

	/// <summary>
	/// Processes a chunk of data, updating all hash states.
	/// </summary>
	/// <param name="data">The data chunk to process.</param>
	public void ProcessChunk(ReadOnlySpan<byte> data) {
		foreach (var hasher in _hashers.Values) {
			hasher.Update(data);
		}
	}

	/// <summary>
	/// Finalizes all hashes and returns the results as a dictionary.
	/// </summary>
	/// <returns>Dictionary mapping algorithm names to hex string results.</returns>
	public Dictionary<string, string> FinalizeAll() {
		var results = new Dictionary<string, string>(_hashers.Count);

		foreach (var kvp in _hashers) {
			string name = GetResultKey(kvp.Key);
			results[name] = kvp.Value.FinalizeHex();
		}

		return results;
	}

	/// <summary>
	/// Maps HashAlgorithm enum to FileHashResult property name.
	/// </summary>
	private static string GetResultKey(HashAlgorithm algorithm) {
		return algorithm switch {
			// Checksums
			HashAlgorithm.Crc32 => "Crc32",
			HashAlgorithm.Crc32C => "Crc32C",
			HashAlgorithm.Crc64 => "Crc64",
			HashAlgorithm.Crc16Ccitt => "Crc16Ccitt",
			HashAlgorithm.Crc16Modbus => "Crc16Modbus",
			HashAlgorithm.Crc16Usb => "Crc16Usb",
			HashAlgorithm.Adler32 => "Adler32",
			HashAlgorithm.Fletcher16 => "Fletcher16",
			HashAlgorithm.Fletcher32 => "Fletcher32",

			// Non-Crypto Fast
			HashAlgorithm.XxHash32 => "XxHash32",
			HashAlgorithm.XxHash64 => "XxHash64",
			HashAlgorithm.XxHash3 => "XxHash3",
			HashAlgorithm.XxHash128 => "XxHash128",
			HashAlgorithm.MurmurHash3_32 => "Murmur3_32",
			HashAlgorithm.MurmurHash3_128 => "Murmur3_128",
			HashAlgorithm.CityHash64 => "CityHash64",
			HashAlgorithm.CityHash128 => "CityHash128",
			HashAlgorithm.FarmHash64 => "FarmHash64",
			HashAlgorithm.SpookyHash128 => "SpookyV2_128",
			HashAlgorithm.SipHash24 => "SipHash24",
			HashAlgorithm.HighwayHash64 => "HighwayHash64",
			HashAlgorithm.MetroHash64 => "MetroHash64",
			HashAlgorithm.MetroHash128 => "MetroHash128",
			HashAlgorithm.Wyhash64 => "Wyhash64",
			HashAlgorithm.Fnv1a32 => "Fnv1a32",
			HashAlgorithm.Fnv1a64 => "Fnv1a64",
			HashAlgorithm.Djb2 => "Djb2",
			HashAlgorithm.Djb2a => "Djb2a",
			HashAlgorithm.Sdbm => "Sdbm",
			HashAlgorithm.LoseLose => "LoseLose",

			// MD Family
			HashAlgorithm.Md2 => "Md2",
			HashAlgorithm.Md4 => "Md4",
			HashAlgorithm.Md5 => "Md5",

			// SHA-1/2 Family
			HashAlgorithm.Sha0 => "Sha0",
			HashAlgorithm.Sha1 => "Sha1",
			HashAlgorithm.Sha224 => "Sha224",
			HashAlgorithm.Sha256 => "Sha256",
			HashAlgorithm.Sha384 => "Sha384",
			HashAlgorithm.Sha512 => "Sha512",
			HashAlgorithm.Sha512_224 => "Sha512_224",
			HashAlgorithm.Sha512_256 => "Sha512_256",

			// SHA-3 & Keccak
			HashAlgorithm.Sha3_224 => "Sha3_224",
			HashAlgorithm.Sha3_256 => "Sha3_256",
			HashAlgorithm.Sha3_384 => "Sha3_384",
			HashAlgorithm.Sha3_512 => "Sha3_512",
			HashAlgorithm.Keccak256 => "Keccak256",
			HashAlgorithm.Keccak512 => "Keccak512",

			// BLAKE Family
			HashAlgorithm.Blake256 => "Blake256",
			HashAlgorithm.Blake512 => "Blake512",
			HashAlgorithm.Blake2b => "Blake2b",
			HashAlgorithm.Blake2s => "Blake2s",
			HashAlgorithm.Blake3 => "Blake3",

			// RIPEMD Family
			HashAlgorithm.Ripemd128 => "Ripemd128",
			HashAlgorithm.Ripemd160 => "Ripemd160",
			HashAlgorithm.Ripemd256 => "Ripemd256",
			HashAlgorithm.Ripemd320 => "Ripemd320",

			// Other Crypto
			HashAlgorithm.Whirlpool => "Whirlpool",
			HashAlgorithm.Tiger192 => "Tiger192",
			HashAlgorithm.Gost94 => "Gost94",
			HashAlgorithm.Streebog256 => "Streebog256",
			HashAlgorithm.Streebog512 => "Streebog512",
			HashAlgorithm.Skein256 => "Skein256",
			HashAlgorithm.Skein512 => "Skein512",
			HashAlgorithm.Skein1024 => "Skein1024",
			HashAlgorithm.Groestl256 => "Groestl256",
			HashAlgorithm.Groestl512 => "Groestl512",
			HashAlgorithm.Jh256 => "Jh256",
			HashAlgorithm.Jh512 => "Jh512",
			HashAlgorithm.KangarooTwelve => "KangarooTwelve",
			HashAlgorithm.Sm3 => "Sm3",

			_ => algorithm.ToString()
		};
	}

	#endregion

	#region File Hashing

	/// <summary>
	/// Hashes a file using streaming and returns a FileHashResult.
	/// </summary>
	/// <param name="filePath">Path to the file to hash.</param>
	/// <param name="progress">Optional progress callback (0.0 to 1.0).</param>
	/// <returns>Complete hash results for the file.</returns>
	public FileHashResult HashFile(string filePath, Action<double>? progress = null) {
		var fileInfo = new FileInfo(filePath);
		var sw = Stopwatch.StartNew();

		// Read file in chunks
		byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
		try {
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan);

			long totalBytes = stream.Length;
			long bytesRead = 0;

			int read;
			while ((read = stream.Read(buffer, 0, BufferSize)) > 0) {
				ProcessChunk(buffer.AsSpan(0, read));
				bytesRead += read;
				progress?.Invoke((double)bytesRead / totalBytes);
			}
		} finally {
			ArrayPool<byte>.Shared.Return(buffer);
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
	}

	#endregion

	#region IDisposable

	/// <inheritdoc/>
	public void Dispose() {
		foreach (var hasher in _hashers.Values) {
			hasher.Dispose();
		}
		_hashers.Clear();
	}

	#endregion
}
