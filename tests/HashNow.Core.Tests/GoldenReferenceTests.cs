using System.Text.Json;
using StreamHash.Core;

namespace HashNow.Core.Tests;

/// <summary>
/// Golden reference tests for regression detection.
/// Verifies that all 70 hash algorithm outputs remain stable across code changes.
/// If any hash value changes, it means something broke the algorithm implementation.
/// </summary>
public class GoldenReferenceTests {
	/// <summary>
	/// Well-known test input: "abc" (0x61 0x62 0x63).
	/// </summary>
	private static readonly byte[] AbcData = "abc"u8.ToArray();

	/// <summary>
	/// Well-known test input: empty data.
	/// </summary>
	private static readonly byte[] EmptyData = [];

	private static readonly string GoldenDir = Path.Combine(
		AppContext.BaseDirectory, "..", "..", "..", "ReferenceData");

	private static readonly string GoldenAbcPath = Path.Combine(GoldenDir, "golden-abc.json");
	private static readonly string GoldenEmptyPath = Path.Combine(GoldenDir, "golden-empty.json");

	private static readonly JsonSerializerOptions JsonOptions = new() {
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};

	/// <summary>
	/// Computes hashes for all 70 algorithms on the given data.
	/// Returns a sorted dictionary of algorithm name to hex hash string.
	/// </summary>
	private static SortedDictionary<string, string> ComputeAllHashes(byte[] data) {
		var results = new SortedDictionary<string, string>(StringComparer.Ordinal);
		foreach (var algo in Enum.GetValues<HashAlgorithm>()) {
			results[algo.ToString()] = HashFacade.ComputeHashHex(algo, data);
		}
		return results;
	}

	/// <summary>
	/// Loads a golden reference file. Returns null if file doesn't exist.
	/// </summary>
	private static SortedDictionary<string, string>? LoadGolden(string path) {
		if (!File.Exists(path)) {
			return null;
		}
		var json = File.ReadAllText(path);
		return JsonSerializer.Deserialize<SortedDictionary<string, string>>(json, JsonOptions);
	}

	/// <summary>
	/// Saves a golden reference file.
	/// </summary>
	private static void SaveGolden(string path, SortedDictionary<string, string> hashes) {
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		var json = JsonSerializer.Serialize(hashes, JsonOptions);
		File.WriteAllText(path, json);
	}

	/// <summary>
	/// Verifies current hashes match golden reference. Generates golden file if missing.
	/// </summary>
	private static void VerifyAgainstGolden(string goldenPath, byte[] data, string label) {
		var current = ComputeAllHashes(data);

		var golden = LoadGolden(goldenPath);
		if (golden is null) {
			// First run: generate the golden file
			SaveGolden(goldenPath, current);
			golden = current;
		}

		// Verify every algorithm matches
		foreach (var (algo, expectedHash) in golden) {
			Assert.True(current.ContainsKey(algo),
				$"Golden reference contains algorithm '{algo}' but current code does not");
			Assert.Equal(expectedHash, current[algo]);
		}

		// Verify no algorithms were removed
		Assert.Equal(golden.Count, current.Count);
	}

	[Fact]
	public void AllAlgorithms_AbcInput_MatchGoldenReference() {
		VerifyAgainstGolden(GoldenAbcPath, AbcData, "abc");
	}

	[Fact]
	public void AllAlgorithms_EmptyInput_MatchGoldenReference() {
		VerifyAgainstGolden(GoldenEmptyPath, EmptyData, "empty");
	}

	[Fact]
	public void AllAlgorithms_AbcInput_ProduceLowercaseHex() {
		var hashes = ComputeAllHashes(AbcData);
		foreach (var (algo, hash) in hashes) {
			Assert.Matches("^[0-9a-f]+$", hash);
			Assert.True(hash.Length > 0, $"{algo} produced empty hash");
		}
	}

	[Fact]
	public void AllAlgorithms_EmptyInput_ProduceLowercaseHex() {
		var hashes = ComputeAllHashes(EmptyData);
		foreach (var (algo, hash) in hashes) {
			Assert.Matches("^[0-9a-f]+$", hash);
			Assert.True(hash.Length > 0, $"{algo} produced empty hash");
		}
	}

	[Fact]
	public void AllAlgorithms_Count_Is70() {
		var hashes = ComputeAllHashes(AbcData);
		Assert.Equal(70, hashes.Count);
	}
}
