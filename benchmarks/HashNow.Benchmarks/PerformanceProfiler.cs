using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using HashNow.Core;

namespace HashNow.Benchmarks;

/// <summary>
/// Benchmarks for profiling actual file hashing performance.
/// </summary>
[MemoryDiagnoser]
public class PerformanceProfiler {
	private string _tempFile50MB = string.Empty;
	private string _tempFile1MB = string.Empty;
	private string _tempFile100KB = string.Empty;

	[GlobalSetup]
	public void Setup() {
		// Create test files
		_tempFile100KB = Path.GetTempFileName();
		_tempFile1MB = Path.GetTempFileName();
		_tempFile50MB = Path.GetTempFileName();

		// Generate random data
		var random = new Random(42);
		var data100KB = new byte[100 * 1024];
		var data1MB = new byte[1024 * 1024];
		var data50MB = new byte[50 * 1024 * 1024];

		random.NextBytes(data100KB);
		random.NextBytes(data1MB);
		random.NextBytes(data50MB);

		File.WriteAllBytes(_tempFile100KB, data100KB);
		File.WriteAllBytes(_tempFile1MB, data1MB);
		File.WriteAllBytes(_tempFile50MB, data50MB);
	}

	[GlobalCleanup]
	public void Cleanup() {
		File.Delete(_tempFile100KB);
		File.Delete(_tempFile1MB);
		File.Delete(_tempFile50MB);
	}

	[Benchmark]
	public FileHashResult Hash_100KB_File() {
		return FileHasher.HashFile(_tempFile100KB);
	}

	[Benchmark]
	public FileHashResult Hash_1MB_File() {
		return FileHasher.HashFile(_tempFile1MB);
	}

	[Benchmark]
	public FileHashResult Hash_50MB_File() {
		return FileHasher.HashFile(_tempFile50MB);
	}
}
