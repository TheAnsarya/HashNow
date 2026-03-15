using System.Text.Json;
using BenchmarkDotNet.Attributes;
using HashNow.Core;

namespace HashNow.Benchmarks;

/// <summary>
/// Benchmarks for JSON serialization and file I/O operations.
/// Measures overhead of JSON output generation and progress callback.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class JsonAndIoBenchmarks {
	private FileHashResult _result = null!;
	private string _tempJsonPath = null!;

	[GlobalSetup]
	public void Setup() {
		// Create a minimal result to benchmark serialization
		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, "benchmark test data for JSON serialization");
		_result = FileHasher.HashFile(tempFile);
		File.Delete(tempFile);
		_tempJsonPath = Path.GetTempFileName();
	}

	[GlobalCleanup]
	public void Cleanup() {
		if (File.Exists(_tempJsonPath))
			File.Delete(_tempJsonPath);
	}

	[Benchmark(Description = "JSON serialize FileHashResult")]
	public string JsonSerialize() {
		return JsonSerializer.Serialize(_result, new JsonSerializerOptions {
			WriteIndented = true,
			IndentCharacter = '\t',
			IndentSize = 1,
		});
	}

	[Benchmark(Description = "SaveResult to disk")]
	public void SaveResultToDisk() {
		FileHasher.SaveResult(_result, _tempJsonPath);
	}
}

/// <summary>
/// Benchmarks measuring progress callback overhead.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 2, iterationCount: 3)]
public class ProgressOverheadBenchmarks {
	private string _tempFile = null!;

	[Params(1024 * 1024)] // 1MB
	public int FileSize { get; set; }

	[GlobalSetup]
	public void Setup() {
		_tempFile = Path.GetTempFileName();
		var data = new byte[FileSize];
		Random.Shared.NextBytes(data);
		File.WriteAllBytes(_tempFile, data);
	}

	[GlobalCleanup]
	public void Cleanup() {
		if (File.Exists(_tempFile))
			File.Delete(_tempFile);
	}

	[Benchmark(Baseline = true, Description = "Hash without progress")]
	public FileHashResult HashNoProgress() {
		return FileHasher.HashFile(_tempFile);
	}

	[Benchmark(Description = "Hash with progress callback")]
	public FileHashResult HashWithProgress() {
		return FileHasher.HashFile(_tempFile, progress: _ => { });
	}
}
