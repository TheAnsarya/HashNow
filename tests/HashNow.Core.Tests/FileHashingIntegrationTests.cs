using System.Text;

namespace HashNow.Core.Tests;

/// <summary>
/// Integration tests for HashFileAsync with various file scenarios.
/// </summary>
public class FileHashingIntegrationTests {

	[Fact]
	public async Task HashFile_EmptyFile_AllHashesPresent() {
		var tempFile = Path.GetTempFileName();
		try {
			var result = await FileHasher.HashFileAsync(tempFile);

			// Verify all hash properties are present
			Assert.NotNull(result.Crc32);
			Assert.NotNull(result.Md5);
			Assert.NotNull(result.Sha256);
			Assert.NotNull(result.Blake3);
			Assert.NotNull(result.XxHash3);
			Assert.NotNull(result.KangarooTwelve);

			// Verify metadata
			Assert.Equal(0, result.SizeBytes);
			Assert.Equal("0 B", result.SizeFormatted);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task HashFile_1KBFile_CompletesQuickly() {
		var tempFile = Path.GetTempFileName();
		try {
			await File.WriteAllBytesAsync(tempFile, new byte[1024]);

			var sw = System.Diagnostics.Stopwatch.StartNew();
			var result = await FileHasher.HashFileAsync(tempFile);
			sw.Stop();

			Assert.Equal(1024, result.SizeBytes);
			Assert.Equal("1 KB", result.SizeFormatted);
			Assert.True(sw.ElapsedMilliseconds < 1000, $"Took {sw.ElapsedMilliseconds}ms");
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task HashFile_1MBFile_ReportsProgress() {
		var tempFile = Path.GetTempFileName();
		try {
			var data = new byte[1024 * 1024];
			Random.Shared.NextBytes(data);
			await File.WriteAllBytesAsync(tempFile, data);

			var result = await FileHasher.HashFileAsync(tempFile);

			Assert.Equal(1024 * 1024, result.SizeBytes);
			Assert.Equal("1 MB", result.SizeFormatted);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Theory]
	[InlineData(100)]
	[InlineData(1000)]
	[InlineData(10000)]
	[InlineData(100000)]
	public async Task HashFile_VariousSizes_AllProduceDifferentHashes(int size) {
		var tempFile = Path.GetTempFileName();
		try {
			var data = new byte[size];
			Random.Shared.NextBytes(data);
			await File.WriteAllBytesAsync(tempFile, data);

			var result = await FileHasher.HashFileAsync(tempFile);

			Assert.Equal(size, result.SizeBytes);
			Assert.NotEmpty(result.Sha256);
			Assert.NotEqual("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", result.Sha256);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task HashFile_BinaryData_HandlesAllByteValues() {
		var tempFile = Path.GetTempFileName();
		try {
			var data = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
			await File.WriteAllBytesAsync(tempFile, data);

			var result = await FileHasher.HashFileAsync(tempFile);

			Assert.Equal(256, result.SizeBytes);
			Assert.NotEmpty(result.Md5);
			Assert.NotEmpty(result.Sha256);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task HashFile_TextFile_UTF8_ProducesCorrectHashes() {
		var tempFile = Path.GetTempFileName();
		try {
			// Write without BOM
			await File.WriteAllBytesAsync(tempFile, Encoding.UTF8.GetBytes("Hello, World!"));

			var result = await FileHasher.HashFileAsync(tempFile);

			// Known MD5 for "Hello, World!"
			Assert.Equal("65a8e27d8879283831b664bd8b7f0ad4", result.Md5);
			// Known SHA256
			Assert.Equal("dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f", result.Sha256);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task HashFile_UnicodeFile_HandlesCorrectly() {
		var tempFile = Path.GetTempFileName();
		try {
			await File.WriteAllTextAsync(tempFile, "Hello, 世界! 🌍", Encoding.UTF8);

			var result = await FileHasher.HashFileAsync(tempFile);

			Assert.True(result.SizeBytes > 0);
			Assert.NotEmpty(result.Sha256);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task HashFile_RepeatedData_DetectsPattern() {
		var tempFile = Path.GetTempFileName();
		try {
			var pattern = Encoding.ASCII.GetBytes("ABCD");
			var repeated = Enumerable.Repeat(pattern, 1000).SelectMany(x => x).ToArray();
			await File.WriteAllBytesAsync(tempFile, repeated);

			var result = await FileHasher.HashFileAsync(tempFile);

			Assert.Equal(4000, result.SizeBytes);
			Assert.NotEmpty(result.Sha256);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task HashFile_NullBytes_HandlesCorrectly() {
		var tempFile = Path.GetTempFileName();
		try {
			var nulls = new byte[1000]; // All zeros
			await File.WriteAllBytesAsync(tempFile, nulls);

			var result = await FileHasher.HashFileAsync(tempFile);

			Assert.Equal(1000, result.SizeBytes);
			Assert.NotEmpty(result.Sha256);
			// Should not be the same as empty file
			Assert.NotEqual("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", result.Sha256);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task SaveResult_CreatesValidJson() {
		var tempFile = Path.GetTempFileName();
		try {
			await File.WriteAllTextAsync(tempFile, "test data");
			var result = await FileHasher.HashFileAsync(tempFile);

			var jsonPath = tempFile + ".hashes.json";
			await FileHasher.SaveResultAsync(result, jsonPath);
			Assert.True(File.Exists(jsonPath));

			var jsonContent = await File.ReadAllTextAsync(jsonPath);
			Assert.Contains("\"fileName\":", jsonContent);
			Assert.Contains("\"sha256\":", jsonContent);
			Assert.Contains("\t", jsonContent); // Tab indentation

			File.Delete(jsonPath);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task HashFile_ConcurrentCalls_AllSucceed() {
		var tasks = Enumerable.Range(0, 10).Select(async i => {
			var tempFile = Path.GetTempFileName();
			try {
				await File.WriteAllBytesAsync(tempFile, new byte[100 * i]);
				return await FileHasher.HashFileAsync(tempFile);
			} finally {
				File.Delete(tempFile);
			}
		});

		var results = await Task.WhenAll(tasks);

		Assert.Equal(10, results.Length);
		Assert.All(results, r => Assert.NotNull(r.Sha256));
	}

	[Fact]
	public async Task HashFile_DifferentExtensions_AllWork() {
		var extensions = new[] { ".txt", ".bin", ".dat", ".iso", ".zip", ".exe", ".dll" };

		foreach (var ext in extensions) {
			var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ext);
			try {
				await File.WriteAllBytesAsync(tempFile, new byte[100]);
				var result = await FileHasher.HashFileAsync(tempFile);

				Assert.NotNull(result);
				Assert.EndsWith(ext, result.FileName);
			} finally {
				File.Delete(tempFile);
			}
		}
	}

	[Fact]
	public async Task HashFile_SpecialCharactersInPath_HandlesCorrectly() {
		var tempDir = Path.Combine(Path.GetTempPath(), $"test {Guid.NewGuid()}");
		Directory.CreateDirectory(tempDir);

		try {
			var specialFile = Path.Combine(tempDir, "file with spaces & symbols!@#.txt");
			await File.WriteAllTextAsync(specialFile, "test");

			var result = await FileHasher.HashFileAsync(specialFile);

			Assert.Contains("spaces", result.FileName);
			Assert.NotEmpty(result.Sha256);

			File.Delete(specialFile);
		} finally {
			Directory.Delete(tempDir, true);
		}
	}
}
