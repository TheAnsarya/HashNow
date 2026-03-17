namespace HashNow.Core.Tests;

/// <summary>
/// Tests verifying that the streaming hasher produces results consistent
/// with the one-shot FileHasher methods for the same input data.
/// </summary>
public class StreamingConsistencyTests {
	/// <summary>
	/// Compares streaming (file-based) hash results against one-shot (byte[]-based) results.
	/// Creates a temp file, hashes it via StreamingHasher, then hashes the same bytes via FileHasher.
	/// </summary>
	[Fact]
	public void StreamingHasher_MatchesOneShotHasher_ForSmallFile() {
		var data = "Streaming consistency test data for all algorithms"u8.ToArray();
		var tempFile = Path.GetTempFileName();

		try {
			File.WriteAllBytes(tempFile, data);

			using var streamer = new StreamingHasher();
			var result = streamer.HashFile(tempFile);

			// Verify a representative subset of algorithms match one-shot
			Assert.Equal(FileHasher.ComputeCrc32(data), result.Crc32);
			Assert.Equal(FileHasher.ComputeMd5(data), result.Md5);
			Assert.Equal(FileHasher.ComputeSha1(data), result.Sha1);
			Assert.Equal(FileHasher.ComputeSha256(data), result.Sha256);
			Assert.Equal(FileHasher.ComputeSha512(data), result.Sha512);
			Assert.Equal(FileHasher.ComputeSha3_256(data), result.Sha3_256);
			Assert.Equal(FileHasher.ComputeBlake3(data), result.Blake3);
			Assert.Equal(FileHasher.ComputeBlake2b(data), result.Blake2b);
			Assert.Equal(FileHasher.ComputeBlake2s(data), result.Blake2s);
			Assert.Equal(FileHasher.ComputeXxHash64(data), result.XxHash64);
			Assert.Equal(FileHasher.ComputeMurmur3_128(data), result.Murmur3_128);
			Assert.Equal(FileHasher.ComputeRipemd160(data), result.Ripemd160);
			Assert.Equal(FileHasher.ComputeWhirlpool(data), result.Whirlpool);
			Assert.Equal(FileHasher.ComputeSm3(data), result.Sm3);
			Assert.Equal(FileHasher.ComputeKeccak256(data), result.Keccak256);
			Assert.Equal(FileHasher.ComputeAdler32(data), result.Adler32);
		} finally {
			File.Delete(tempFile);
		}
	}

	/// <summary>
	/// Tests that streaming hashing works across multiple buffer reads (data larger than BufferSize).
	/// Uses a 2MB file to ensure at least 2 chunks are processed.
	/// </summary>
	[Fact]
	public void StreamingHasher_MultipleChunks_ProducesCorrectResults() {
		// 2MB of patterned data (bigger than 1MB buffer)
		var data = new byte[2 * 1024 * 1024];
		for (int i = 0; i < data.Length; i++) {
			data[i] = (byte)(i % 251); // prime modulus for non-repeating pattern within 251 bytes
		}

		var tempFile = Path.GetTempFileName();
		try {
			File.WriteAllBytes(tempFile, data);

			using var streamer = new StreamingHasher();
			var result = streamer.HashFile(tempFile);

			// Key algorithms must match one-shot
			Assert.Equal(FileHasher.ComputeSha256(data), result.Sha256);
			Assert.Equal(FileHasher.ComputeSha512(data), result.Sha512);
			Assert.Equal(FileHasher.ComputeBlake3(data), result.Blake3);
			Assert.Equal(FileHasher.ComputeMd5(data), result.Md5);
			Assert.Equal(FileHasher.ComputeXxHash64(data), result.XxHash64);
		} finally {
			File.Delete(tempFile);
		}
	}

	/// <summary>
	/// Tests streaming hashing of an empty file.
	/// </summary>
	[Fact]
	public void StreamingHasher_EmptyFile_MatchesOneShotEmpty() {
		byte[] empty = [];
		var tempFile = Path.GetTempFileName();

		try {
			File.WriteAllBytes(tempFile, empty);

			using var streamer = new StreamingHasher();
			var result = streamer.HashFile(tempFile);

			Assert.Equal(FileHasher.ComputeSha256(empty), result.Sha256);
			Assert.Equal(FileHasher.ComputeMd5(empty), result.Md5);
			Assert.Equal(FileHasher.ComputeCrc32(empty), result.Crc32);
			Assert.Equal(FileHasher.ComputeBlake3(empty), result.Blake3);
		} finally {
			File.Delete(tempFile);
		}
	}

	/// <summary>
	/// Tests that the progress callback fires and reports reasonable values.
	/// </summary>
	[Fact]
	public void StreamingHasher_ProgressCallback_ReportsProgress() {
		var data = new byte[1024 * 1024]; // 1MB - exactly one buffer read
		Random.Shared.NextBytes(data);
		var tempFile = Path.GetTempFileName();

		try {
			File.WriteAllBytes(tempFile, data);

			var progressValues = new List<double>();
			using var streamer = new StreamingHasher();
			streamer.HashFile(tempFile, progress: p => progressValues.Add(p));

			Assert.NotEmpty(progressValues);
			Assert.All(progressValues, p => Assert.InRange(p, 0.0, 1.0));
			// Last progress should be 1.0 (100%)
			Assert.True(progressValues[^1] >= 0.99, $"Final progress was {progressValues[^1]}");
		} finally {
			File.Delete(tempFile);
		}
	}

	/// <summary>
	/// Tests that cancellation works during hashing.
	/// </summary>
	[Fact]
	public void StreamingHasher_Cancellation_ThrowsOperationCanceled() {
		// Use a moderately sized file
		var data = new byte[4 * 1024 * 1024]; // 4MB
		Random.Shared.NextBytes(data);
		var tempFile = Path.GetTempFileName();

		try {
			File.WriteAllBytes(tempFile, data);

			using var cts = new CancellationTokenSource();
			cts.Cancel(); // Cancel immediately

			using var streamer = new StreamingHasher();
			Assert.Throws<OperationCanceledException>(() =>
				streamer.HashFile(tempFile, cancellationToken: cts.Token));
		} finally {
			File.Delete(tempFile);
		}
	}

	/// <summary>
	/// Tests metadata fields in the result.
	/// </summary>
	[Fact]
	public void StreamingHasher_Result_ContainsCorrectMetadata() {
		var data = "metadata test"u8.ToArray();
		var tempFile = Path.GetTempFileName();

		try {
			File.WriteAllBytes(tempFile, data);

			using var streamer = new StreamingHasher();
			var result = streamer.HashFile(tempFile);

			Assert.Equal(Path.GetFileName(tempFile), result.FileName);
			Assert.Equal(Path.GetFullPath(tempFile), result.FullPath);
			Assert.Equal(data.Length, result.SizeBytes);
			Assert.NotNull(result.SizeFormatted);
			Assert.NotNull(result.HashedAtUtc);
			Assert.True(result.DurationMs >= 0);
		} finally {
			File.Delete(tempFile);
		}
	}
}
