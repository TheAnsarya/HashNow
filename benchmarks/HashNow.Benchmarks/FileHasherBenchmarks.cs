using System.IO.Hashing;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HashNow.Core;

namespace HashNow.Benchmarks;

/// <summary>
/// Benchmarks for comparing individual hash algorithms and overall throughput.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class FileHasherBenchmarks {
private string _smallFilePath = null!;
private string _mediumFilePath = null!;
private string _largeFilePath = null!;
private byte[] _smallData = null!;
private byte[] _mediumData = null!;
private byte[] _largeData = null!;
private string _tempDir = null!;

[GlobalSetup]
public void Setup() {
_tempDir = Path.Combine(Path.GetTempPath(), $"HashNow_Bench_{Guid.NewGuid():N}");
Directory.CreateDirectory(_tempDir);

// Create test data
_smallData = new byte[1024]; // 1 KB
_mediumData = new byte[1024 * 1024]; // 1 MB
_largeData = new byte[10 * 1024 * 1024]; // 10 MB

Random.Shared.NextBytes(_smallData);
Random.Shared.NextBytes(_mediumData);
Random.Shared.NextBytes(_largeData);

// Write to files
_smallFilePath = Path.Combine(_tempDir, "small.bin");
_mediumFilePath = Path.Combine(_tempDir, "medium.bin");
_largeFilePath = Path.Combine(_tempDir, "large.bin");

File.WriteAllBytes(_smallFilePath, _smallData);
File.WriteAllBytes(_mediumFilePath, _mediumData);
File.WriteAllBytes(_largeFilePath, _largeData);
}

[GlobalCleanup]
public void Cleanup() {
if (Directory.Exists(_tempDir)) {
Directory.Delete(_tempDir, recursive: true);
}
}

#region HashNow Full Hash (All 10-13 Algorithms)

[Benchmark(Description = "HashNow All (1 KB)")]
public async Task<FileHashResult> HashNow_SmallFile() {
return await FileHasher.HashFileAsync(_smallFilePath);
}

[Benchmark(Description = "HashNow All (1 MB)")]
public async Task<FileHashResult> HashNow_MediumFile() {
return await FileHasher.HashFileAsync(_mediumFilePath);
}

[Benchmark(Baseline = true, Description = "HashNow All (10 MB)")]
public async Task<FileHashResult> HashNow_LargeFile() {
return await FileHasher.HashFileAsync(_largeFilePath);
}

#endregion

#region Individual Algorithm Benchmarks (In-Memory)

[Benchmark(Description = "CRC32 Only (10 MB)")]
public byte[] Crc32Only() {
return Crc32.Hash(_largeData);
}

[Benchmark(Description = "CRC64 Only (10 MB)")]
public byte[] Crc64Only() {
return Crc64.Hash(_largeData);
}

[Benchmark(Description = "MD5 Only (10 MB)")]
public byte[] Md5Only() {
return MD5.HashData(_largeData);
}

[Benchmark(Description = "SHA1 Only (10 MB)")]
public byte[] Sha1Only() {
return SHA1.HashData(_largeData);
}

[Benchmark(Description = "SHA256 Only (10 MB)")]
public byte[] Sha256Only() {
return SHA256.HashData(_largeData);
}

[Benchmark(Description = "SHA384 Only (10 MB)")]
public byte[] Sha384Only() {
return SHA384.HashData(_largeData);
}

[Benchmark(Description = "SHA512 Only (10 MB)")]
public byte[] Sha512Only() {
return SHA512.HashData(_largeData);
}

[Benchmark(Description = "XXHash3 Only (10 MB)")]
public byte[] XxHash3Only() {
return XxHash3.Hash(_largeData);
}

[Benchmark(Description = "XXHash64 Only (10 MB)")]
public byte[] XxHash64Only() {
return XxHash64.Hash(_largeData);
}

[Benchmark(Description = "XXHash128 Only (10 MB)")]
public byte[] XxHash128Only() {
return XxHash128.Hash(_largeData);
}

#endregion
}
