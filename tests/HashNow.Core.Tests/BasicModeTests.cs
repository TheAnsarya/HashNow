namespace HashNow.Core.Tests;

/// <summary>
/// Tests for basic-five hashing mode.
/// </summary>
public sealed class BasicModeTests {
	[Fact]
	public async Task HashFileBasicAsync_MatchesFullMode_ForBasicAlgorithms() {
		var tempFile = Path.GetTempFileName();
		try {
			await File.WriteAllTextAsync(tempFile, "HashNow basic mode parity test.");

			var full = await FileHasher.HashFileAsync(tempFile);
			var basic = await FileHasher.HashFileBasicAsync(tempFile);

			Assert.Equal(full.Md5, basic.Md5);
			Assert.Equal(full.Crc32, basic.Crc32);
			Assert.Equal(full.Sha1, basic.Sha1);
			Assert.Equal(full.Sha256, basic.Sha256);
			Assert.Equal(full.Sha512, basic.Sha512);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task HashFileBasicAsync_SetsExpectedMetadata() {
		var tempFile = Path.GetTempFileName();
		try {
			await File.WriteAllBytesAsync(tempFile, [0x01, 0x02, 0x03, 0x04, 0x05]);

			var basic = await FileHasher.HashFileBasicAsync(tempFile);

			Assert.Equal(5, basic.SizeBytes);
			Assert.Equal(FileHasher.BasicAlgorithmCount, basic.AlgorithmCount);
			Assert.Equal("basic", basic.HashMode);
			Assert.StartsWith($"HashNow v{FileHasher.Version}", basic.GeneratedBy, StringComparison.Ordinal);
		} finally {
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task SaveResultAsync_BasicMode_WritesExpectedJsonFields() {
		var tempFile = Path.GetTempFileName();
		var jsonPath = tempFile + ".hashes.json";

		try {
			await File.WriteAllTextAsync(tempFile, "json serialization test");
			var basic = await FileHasher.HashFileBasicAsync(tempFile);
			await FileHasher.SaveResultAsync(basic, jsonPath);

			var json = await File.ReadAllTextAsync(jsonPath);
			Assert.Contains("\"md5\":", json, StringComparison.Ordinal);
			Assert.Contains("\"crc32\":", json, StringComparison.Ordinal);
			Assert.Contains("\"sha1\":", json, StringComparison.Ordinal);
			Assert.Contains("\"sha256\":", json, StringComparison.Ordinal);
			Assert.Contains("\"sha512\":", json, StringComparison.Ordinal);
			Assert.Contains("\"hashMode\": \"basic\"", json, StringComparison.Ordinal);
			Assert.Contains("\"algorithmCount\": 5", json, StringComparison.Ordinal);
		} finally {
			if (File.Exists(jsonPath)) {
				File.Delete(jsonPath);
			}

			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task SaveResultAsync_BasicMode_InsertsBlankLineBeforeMetadata() {
		var tempFile = Path.GetTempFileName();
		var jsonPath = tempFile + ".hashes.json";

		try {
			await File.WriteAllTextAsync(tempFile, "basic metadata spacing");
			var basic = await FileHasher.HashFileBasicAsync(tempFile);
			await FileHasher.SaveResultAsync(basic, jsonPath);

			var json = await File.ReadAllTextAsync(jsonPath);
			var expectedSeparator = $"\"sha512\": \"{basic.Sha512}\",{Environment.NewLine}{Environment.NewLine}\t\"hashedAtUtc\":";
			Assert.Contains(expectedSeparator, json, StringComparison.Ordinal);
		} finally {
			if (File.Exists(jsonPath)) {
				File.Delete(jsonPath);
			}

			File.Delete(tempFile);
		}
	}
}
