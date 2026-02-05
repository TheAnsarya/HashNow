using System.Diagnostics;
using System.Text.Json.Serialization;

namespace HashNow.Core;

/// <summary>
/// Represents detailed performance diagnostics for hash computation.
/// </summary>
/// <remarks>
/// <para>
/// This class provides granular timing information for analyzing hash performance
/// across different algorithm categories. It helps identify bottlenecks and
/// compare algorithm speeds.
/// </para>
/// <para>
/// <strong>Performance Categories:</strong>
/// </para>
/// <list type="bullet">
///   <item><description><strong>FileRead:</strong> Time to read file into memory</description></item>
///   <item><description><strong>Checksums:</strong> CRC32, CRC32C, CRC64, Adler-32, Fletcher checksums</description></item>
///   <item><description><strong>FastHashes:</strong> xxHash, MurmurHash, CityHash, etc.</description></item>
///   <item><description><strong>Cryptographic:</strong> SHA family, MD family, BLAKE, etc.</description></item>
///   <item><description><strong>OtherCrypto:</strong> Whirlpool, Tiger, GOST, Skein, etc.</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var result = FileHasher.HashFileWithDiagnostics("large-file.iso");
/// Console.WriteLine($"File read: {result.Diagnostics.FileReadMs}ms");
/// Console.WriteLine($"SHA family: {result.Diagnostics.ShaFamilyMs}ms");
/// Console.WriteLine($"Throughput: {result.Diagnostics.ThroughputMBps:F1} MB/s");
/// </code>
/// </example>
public sealed class PerformanceDiagnostics {
	#region Timing Measurements

	/// <summary>
	/// Gets or sets the time spent reading the file into memory (milliseconds).
	/// </summary>
	/// <remarks>
	/// This represents I/O time before any hashing begins.
	/// Depends on storage speed (SSD vs HDD) and file size.
	/// </remarks>
	[JsonPropertyName("fileReadMs")]
	public long FileReadMs { get; set; }

	/// <summary>
	/// Gets or sets the total hash computation time (milliseconds).
	/// </summary>
	/// <remarks>
	/// This is the time for all 70 algorithms computed in parallel.
	/// It's typically less than the sum of individual times due to parallelism.
	/// </remarks>
	[JsonPropertyName("totalHashMs")]
	public long TotalHashMs { get; set; }

	/// <summary>
	/// Gets or sets the total wall-clock time including I/O and hashing (milliseconds).
	/// </summary>
	[JsonPropertyName("totalMs")]
	public long TotalMs { get; set; }

	#endregion

	#region Category Timings

	/// <summary>
	/// Gets or sets timing for checksum algorithms (CRC32, Adler-32, Fletcher).
	/// </summary>
	[JsonPropertyName("checksumsMs")]
	public long ChecksumsMs { get; set; }

	/// <summary>
	/// Gets or sets timing for fast non-cryptographic hashes (xxHash, MurmurHash, etc.).
	/// </summary>
	[JsonPropertyName("fastHashesMs")]
	public long FastHashesMs { get; set; }

	/// <summary>
	/// Gets or sets timing for MD family hashes (MD2, MD4, MD5).
	/// </summary>
	[JsonPropertyName("mdFamilyMs")]
	public long MdFamilyMs { get; set; }

	/// <summary>
	/// Gets or sets timing for SHA-1/2 family hashes.
	/// </summary>
	[JsonPropertyName("shaFamilyMs")]
	public long ShaFamilyMs { get; set; }

	/// <summary>
	/// Gets or sets timing for SHA-3 and Keccak hashes.
	/// </summary>
	[JsonPropertyName("sha3KeccakMs")]
	public long Sha3KeccakMs { get; set; }

	/// <summary>
	/// Gets or sets timing for BLAKE family hashes.
	/// </summary>
	[JsonPropertyName("blakeFamilyMs")]
	public long BlakeFamilyMs { get; set; }

	/// <summary>
	/// Gets or sets timing for RIPEMD family hashes.
	/// </summary>
	[JsonPropertyName("ripemdFamilyMs")]
	public long RipemdFamilyMs { get; set; }

	/// <summary>
	/// Gets or sets timing for other cryptographic hashes (Whirlpool, Tiger, etc.).
	/// </summary>
	[JsonPropertyName("otherCryptoMs")]
	public long OtherCryptoMs { get; set; }

	#endregion

	#region Computed Metrics

	/// <summary>
	/// Gets the effective throughput in megabytes per second.
	/// </summary>
	/// <remarks>
	/// Calculated as (file size in MB) / (total time in seconds).
	/// Higher values indicate better performance.
	/// </remarks>
	[JsonPropertyName("throughputMBps")]
	public double ThroughputMBps { get; set; }

	/// <summary>
	/// Gets the number of parallel threads used for hashing.
	/// </summary>
	[JsonPropertyName("parallelThreads")]
	public int ParallelThreads { get; set; }

	/// <summary>
	/// Gets the processor count available on the system.
	/// </summary>
	[JsonPropertyName("processorCount")]
	public int ProcessorCount { get; set; }

	/// <summary>
	/// Gets the file size that was processed (bytes).
	/// </summary>
	[JsonPropertyName("fileSizeBytes")]
	public long FileSizeBytes { get; set; }

	#endregion

	#region Factory Methods

	/// <summary>
	/// Creates a new diagnostics instance with system information populated.
	/// </summary>
	/// <returns>A new <see cref="PerformanceDiagnostics"/> instance.</returns>
	public static PerformanceDiagnostics Create() {
		return new PerformanceDiagnostics {
			ProcessorCount = Environment.ProcessorCount,
			ParallelThreads = Math.Min(Environment.ProcessorCount, 58) // Cap at algorithm count
		};
	}

	/// <summary>
	/// Calculates and sets the throughput based on file size and total time.
	/// </summary>
	/// <param name="fileSizeBytes">The size of the file in bytes.</param>
	public void CalculateThroughput(long fileSizeBytes) {
		FileSizeBytes = fileSizeBytes;
		if (TotalMs > 0) {
			double fileSizeMB = fileSizeBytes / (1024.0 * 1024.0);
			double totalSeconds = TotalMs / 1000.0;
			ThroughputMBps = fileSizeMB / totalSeconds;
		}
	}

	#endregion

	#region Display Methods

	/// <summary>
	/// Generates a formatted performance report string.
	/// </summary>
	/// <returns>A multi-line string with performance metrics.</returns>
	public string ToReport() {
		return $@"Performance Diagnostics
═══════════════════════════════════════
File Size:       {FormatSize(FileSizeBytes)}
Total Time:      {TotalMs}ms
Throughput:      {ThroughputMBps:F1} MB/s
─────────────────────────────────────
File Read:       {FileReadMs}ms
Hash Compute:    {TotalHashMs}ms
─────────────────────────────────────
Checksums:       {ChecksumsMs}ms (CRC, Adler, Fletcher)
Fast Hashes:     {FastHashesMs}ms (xxHash, Murmur, City, etc.)
MD Family:       {MdFamilyMs}ms (MD2, MD4, MD5)
SHA Family:      {ShaFamilyMs}ms (SHA-1, SHA-2)
SHA-3/Keccak:    {Sha3KeccakMs}ms
BLAKE Family:    {BlakeFamilyMs}ms (BLAKE, BLAKE2, BLAKE3)
RIPEMD Family:   {RipemdFamilyMs}ms
Other Crypto:    {OtherCryptoMs}ms (Whirlpool, Tiger, etc.)
─────────────────────────────────────
Processors:      {ProcessorCount}
Parallel Threads:{ParallelThreads}
═══════════════════════════════════════";
	}

	/// <summary>
	/// Formats a byte size as a human-readable string.
	/// </summary>
	private static string FormatSize(long bytes) {
		string[] sizes = ["B", "KB", "MB", "GB", "TB"];
		double len = bytes;
		int order = 0;
		while (len >= 1024 && order < sizes.Length - 1) {
			order++;
			len /= 1024;
		}
		return $"{len:0.##} {sizes[order]}";
	}

	#endregion
}

/// <summary>
/// Extended hash result that includes performance diagnostics.
/// </summary>
/// <remarks>
/// This class extends the standard <see cref="FileHashResult"/> by adding
/// detailed performance metrics for analysis and optimization.
/// </remarks>
public sealed class DiagnosticHashResult {
	/// <summary>
	/// Gets the standard hash result with all computed values.
	/// </summary>
	[JsonPropertyName("result")]
	public required FileHashResult Result { get; init; }

	/// <summary>
	/// Gets the performance diagnostics for the hash operation.
	/// </summary>
	[JsonPropertyName("diagnostics")]
	public required PerformanceDiagnostics Diagnostics { get; init; }
}
