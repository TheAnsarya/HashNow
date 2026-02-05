using System.IO.Hashing;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Blake2Fast;
using Blake3;
using HashNow.Core;
using Org.BouncyCastle.Crypto.Digests;

namespace HashNow.Benchmarks;

/// <summary>
/// Benchmarks for comparing individual hash algorithms and overall throughput.
/// Compares parallel (all 70 algorithms) vs sequential execution.
/// </summary>
[MemoryDiagnoser]
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

	#region HashNow Full Hash (All 70 Algorithms - Parallel)

	[Benchmark(Description = "HashNow Parallel 58 (1 KB)")]
	public async Task<FileHashResult> HashNow_SmallFile() {
		return await FileHasher.HashFileAsync(_smallFilePath);
	}

	[Benchmark(Description = "HashNow Parallel 58 (1 MB)")]
	public async Task<FileHashResult> HashNow_MediumFile() {
		return await FileHasher.HashFileAsync(_mediumFilePath);
	}

	[Benchmark(Baseline = true, Description = "HashNow Parallel 58 (10 MB)")]
	public async Task<FileHashResult> HashNow_LargeFile() {
		return await FileHasher.HashFileAsync(_largeFilePath);
	}

	#endregion

	#region Category Benchmarks (10 MB)

	[Benchmark(Description = "Checksums Only (6 algs, 10 MB)")]
	public void ChecksumsOnly() {
		FileHasher.ComputeCrc32(_largeData);
		FileHasher.ComputeCrc32C(_largeData);
		FileHasher.ComputeCrc64(_largeData);
		FileHasher.ComputeAdler32(_largeData);
		FileHasher.ComputeFletcher16(_largeData);
		FileHasher.ComputeFletcher32(_largeData);
	}

	[Benchmark(Description = "Fast Hashes Only (12 algs, 10 MB)")]
	public void FastHashesOnly() {
		FileHasher.ComputeXxHash32(_largeData);
		FileHasher.ComputeXxHash64(_largeData);
		FileHasher.ComputeXxHash3(_largeData);
		FileHasher.ComputeXxHash128(_largeData);
		FileHasher.ComputeMurmur3_32(_largeData);
		FileHasher.ComputeMurmur3_128(_largeData);
		FileHasher.ComputeCityHash64(_largeData);
		FileHasher.ComputeCityHash128(_largeData);
		FileHasher.ComputeFarmHash64(_largeData);
		FileHasher.ComputeSpookyV2_128(_largeData);
		FileHasher.ComputeSipHash24(_largeData);
		FileHasher.ComputeHighwayHash64(_largeData);
	}

	[Benchmark(Description = "Crypto MD/SHA Only (12 algs, 10 MB)")]
	public void CryptoMdShaOnly() {
		FileHasher.ComputeMd2(_largeData);
		FileHasher.ComputeMd4(_largeData);
		FileHasher.ComputeMd5(_largeData);
		FileHasher.ComputeSha1(_largeData);
		FileHasher.ComputeSha224(_largeData);
		FileHasher.ComputeSha256(_largeData);
		FileHasher.ComputeSha384(_largeData);
		FileHasher.ComputeSha512(_largeData);
		FileHasher.ComputeSha512_224(_largeData);
		FileHasher.ComputeSha512_256(_largeData);
		FileHasher.ComputeSha0(_largeData);
	}

	[Benchmark(Description = "SHA3/Keccak Only (6 algs, 10 MB)")]
	public void Sha3KeccakOnly() {
		FileHasher.ComputeSha3_224(_largeData);
		FileHasher.ComputeSha3_256(_largeData);
		FileHasher.ComputeSha3_384(_largeData);
		FileHasher.ComputeSha3_512(_largeData);
		FileHasher.ComputeKeccak256(_largeData);
		FileHasher.ComputeKeccak512(_largeData);
	}

	[Benchmark(Description = "BLAKE Family Only (5 algs, 10 MB)")]
	public void BlakeFamilyOnly() {
		FileHasher.ComputeBlake256(_largeData);
		FileHasher.ComputeBlake512(_largeData);
		FileHasher.ComputeBlake2b(_largeData);
		FileHasher.ComputeBlake2s(_largeData);
		FileHasher.ComputeBlake3(_largeData);
	}

	#endregion

	#region Individual Algorithm Benchmarks (10 MB)

	[Benchmark(Description = "CRC32 (10 MB)")]
	public byte[] Crc32Only() {
		return Crc32.Hash(_largeData);
	}

	[Benchmark(Description = "CRC64 (10 MB)")]
	public byte[] Crc64Only() {
		return Crc64.Hash(_largeData);
	}

	[Benchmark(Description = "MD5 (10 MB)")]
	public byte[] Md5Only() {
		return MD5.HashData(_largeData);
	}

	[Benchmark(Description = "SHA1 (10 MB)")]
	public byte[] Sha1Only() {
		return SHA1.HashData(_largeData);
	}

	[Benchmark(Description = "SHA256 (10 MB)")]
	public byte[] Sha256Only() {
		return SHA256.HashData(_largeData);
	}

	[Benchmark(Description = "SHA384 (10 MB)")]
	public byte[] Sha384Only() {
		return SHA384.HashData(_largeData);
	}

	[Benchmark(Description = "SHA512 (10 MB)")]
	public byte[] Sha512Only() {
		return SHA512.HashData(_largeData);
	}

	[Benchmark(Description = "XXHash3 (10 MB)")]
	public byte[] XxHash3Only() {
		return XxHash3.Hash(_largeData);
	}

	[Benchmark(Description = "XXHash64 (10 MB)")]
	public byte[] XxHash64Only() {
		return XxHash64.Hash(_largeData);
	}

	[Benchmark(Description = "XXHash128 (10 MB)")]
	public byte[] XxHash128Only() {
		return XxHash128.Hash(_largeData);
	}

	[Benchmark(Description = "BLAKE3 (10 MB)")]
	public byte[] Blake3Only() {
		return Hasher.Hash(_largeData).AsSpan().ToArray();
	}

	[Benchmark(Description = "BLAKE2b (10 MB)")]
	public byte[] Blake2bOnly() {
		return Blake2b.ComputeHash(_largeData);
	}

	[Benchmark(Description = "BLAKE2s (10 MB)")]
	public byte[] Blake2sOnly() {
		return Blake2s.ComputeHash(_largeData);
	}

	[Benchmark(Description = "SHA3-256 (10 MB)")]
	public byte[] Sha3_256Only() {
		var digest = new Sha3Digest(256);
		digest.BlockUpdate(_largeData, 0, _largeData.Length);
		var result = new byte[32];
		digest.DoFinal(result, 0);
		return result;
	}

	[Benchmark(Description = "Whirlpool (10 MB)")]
	public byte[] WhirlpoolOnly() {
		var digest = new WhirlpoolDigest();
		digest.BlockUpdate(_largeData, 0, _largeData.Length);
		var result = new byte[64];
		digest.DoFinal(result, 0);
		return result;
	}

	#endregion

	#region Speed Comparison (Fastest vs Slowest)

	[Benchmark(Description = "Fastest: XXHash3 (10 MB)")]
	public string FastestHash() {
		return FileHasher.ComputeXxHash3(_largeData);
	}

	[Benchmark(Description = "Slowest: MD2 (10 MB)")]
	public string SlowestHash() {
		return FileHasher.ComputeMd2(_largeData);
	}

	#endregion
}
