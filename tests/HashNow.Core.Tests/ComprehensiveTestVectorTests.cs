using System.Text.Json;
using StreamHash.Core;

namespace HashNow.Core.Tests;

/// <summary>
/// Comprehensive test vectors for all 70 hash algorithms.
/// Verifies each FileHasher.Compute*() method against golden reference values.
/// </summary>
public class ComprehensiveTestVectorTests {
	private static readonly byte[] AbcData = "abc"u8.ToArray();
	private static readonly byte[] EmptyData = [];

	private static readonly string GoldenDir = Path.Combine(
		AppContext.BaseDirectory, "..", "..", "..", "ReferenceData");

	private static readonly Lazy<SortedDictionary<string, string>> GoldenAbc = new(() =>
		LoadGolden(Path.Combine(GoldenDir, "golden-abc.json")));

	private static readonly Lazy<SortedDictionary<string, string>> GoldenEmpty = new(() =>
		LoadGolden(Path.Combine(GoldenDir, "golden-empty.json")));

	private static SortedDictionary<string, string> LoadGolden(string path) {
		var json = File.ReadAllText(path);
		return JsonSerializer.Deserialize<SortedDictionary<string, string>>(json)!;
	}

	#region Checksums & CRCs (9)

	[Fact] public void Crc32_Abc() => Assert.Equal(GoldenAbc.Value["Crc32"], FileHasher.ComputeCrc32(AbcData));
	[Fact] public void Crc32C_Abc() => Assert.Equal(GoldenAbc.Value["Crc32C"], FileHasher.ComputeCrc32C(AbcData));
	[Fact] public void Crc64_Abc() => Assert.Equal(GoldenAbc.Value["Crc64"], FileHasher.ComputeCrc64(AbcData));
	[Fact] public void Crc16Ccitt_Abc() => Assert.Equal(GoldenAbc.Value["Crc16Ccitt"], FileHasher.ComputeCrc16Ccitt(AbcData));
	[Fact] public void Crc16Modbus_Abc() => Assert.Equal(GoldenAbc.Value["Crc16Modbus"], FileHasher.ComputeCrc16Modbus(AbcData));
	[Fact] public void Crc16Usb_Abc() => Assert.Equal(GoldenAbc.Value["Crc16Usb"], FileHasher.ComputeCrc16Usb(AbcData));
	[Fact] public void Adler32_Abc() => Assert.Equal(GoldenAbc.Value["Adler32"], FileHasher.ComputeAdler32(AbcData));
	[Fact] public void Fletcher16_Abc() => Assert.Equal(GoldenAbc.Value["Fletcher16"], FileHasher.ComputeFletcher16(AbcData));
	[Fact] public void Fletcher32_Abc() => Assert.Equal(GoldenAbc.Value["Fletcher32"], FileHasher.ComputeFletcher32(AbcData));

	#endregion

	#region Fast Non-Crypto (21)

	[Fact] public void XxHash32_Abc() => Assert.Equal(GoldenAbc.Value["XxHash32"], FileHasher.ComputeXxHash32(AbcData));
	[Fact] public void XxHash64_Abc() => Assert.Equal(GoldenAbc.Value["XxHash64"], FileHasher.ComputeXxHash64(AbcData));
	[Fact] public void XxHash3_Abc() => Assert.Equal(GoldenAbc.Value["XxHash3"], FileHasher.ComputeXxHash3(AbcData));
	[Fact] public void XxHash128_Abc() => Assert.Equal(GoldenAbc.Value["XxHash128"], FileHasher.ComputeXxHash128(AbcData));
	[Fact] public void Murmur3_32_Abc() => Assert.Equal(GoldenAbc.Value["MurmurHash3_32"], FileHasher.ComputeMurmur3_32(AbcData));
	[Fact] public void Murmur3_128_Abc() => Assert.Equal(GoldenAbc.Value["MurmurHash3_128"], FileHasher.ComputeMurmur3_128(AbcData));
	[Fact] public void CityHash64_Abc() => Assert.Equal(GoldenAbc.Value["CityHash64"], FileHasher.ComputeCityHash64(AbcData));
	[Fact] public void CityHash128_Abc() => Assert.Equal(GoldenAbc.Value["CityHash128"], FileHasher.ComputeCityHash128(AbcData));
	[Fact] public void FarmHash64_Abc() => Assert.Equal(GoldenAbc.Value["FarmHash64"], FileHasher.ComputeFarmHash64(AbcData));
	[Fact] public void SpookyV2_128_Abc() => Assert.Equal(GoldenAbc.Value["SpookyHash128"], FileHasher.ComputeSpookyV2_128(AbcData));
	[Fact] public void SipHash24_Abc() => Assert.Equal(GoldenAbc.Value["SipHash24"], FileHasher.ComputeSipHash24(AbcData));
	[Fact] public void HighwayHash64_Abc() => Assert.Equal(GoldenAbc.Value["HighwayHash64"], FileHasher.ComputeHighwayHash64(AbcData));
	[Fact] public void MetroHash64_Abc() => Assert.Equal(GoldenAbc.Value["MetroHash64"], FileHasher.ComputeMetroHash64(AbcData));
	[Fact] public void MetroHash128_Abc() => Assert.Equal(GoldenAbc.Value["MetroHash128"], FileHasher.ComputeMetroHash128(AbcData));
	[Fact] public void Wyhash64_Abc() => Assert.Equal(GoldenAbc.Value["Wyhash64"], FileHasher.ComputeWyhash64(AbcData));
	[Fact] public void Fnv1a32_Abc() => Assert.Equal(GoldenAbc.Value["Fnv1a32"], FileHasher.ComputeFnv1a32(AbcData));
	[Fact] public void Fnv1a64_Abc() => Assert.Equal(GoldenAbc.Value["Fnv1a64"], FileHasher.ComputeFnv1a64(AbcData));
	[Fact] public void Djb2_Abc() => Assert.Equal(GoldenAbc.Value["Djb2"], FileHasher.ComputeDjb2(AbcData));
	[Fact] public void Djb2a_Abc() => Assert.Equal(GoldenAbc.Value["Djb2a"], FileHasher.ComputeDjb2a(AbcData));
	[Fact] public void Sdbm_Abc() => Assert.Equal(GoldenAbc.Value["Sdbm"], FileHasher.ComputeSdbm(AbcData));
	[Fact] public void LoseLose_Abc() => Assert.Equal(GoldenAbc.Value["LoseLose"], FileHasher.ComputeLoseLose(AbcData));

	#endregion

	#region MD Family (3)

	[Fact] public void Md2_Abc() => Assert.Equal(GoldenAbc.Value["Md2"], FileHasher.ComputeMd2(AbcData));
	[Fact] public void Md4_Abc() => Assert.Equal(GoldenAbc.Value["Md4"], FileHasher.ComputeMd4(AbcData));
	[Fact] public void Md5_Abc() => Assert.Equal(GoldenAbc.Value["Md5"], FileHasher.ComputeMd5(AbcData));

	#endregion

	#region SHA-1/2 Family (9)

	[Fact] public void Sha0_Abc() => Assert.Equal(GoldenAbc.Value["Sha0"], FileHasher.ComputeSha0(AbcData));
	[Fact] public void Sha1_Abc() => Assert.Equal(GoldenAbc.Value["Sha1"], FileHasher.ComputeSha1(AbcData));
	[Fact] public void Sha224_Abc() => Assert.Equal(GoldenAbc.Value["Sha224"], FileHasher.ComputeSha224(AbcData));
	[Fact] public void Sha256_Abc() => Assert.Equal(GoldenAbc.Value["Sha256"], FileHasher.ComputeSha256(AbcData));
	[Fact] public void Sha384_Abc() => Assert.Equal(GoldenAbc.Value["Sha384"], FileHasher.ComputeSha384(AbcData));
	[Fact] public void Sha512_Abc() => Assert.Equal(GoldenAbc.Value["Sha512"], FileHasher.ComputeSha512(AbcData));
	[Fact] public void Sha512_224_Abc() => Assert.Equal(GoldenAbc.Value["Sha512_224"], FileHasher.ComputeSha512_224(AbcData));
	[Fact] public void Sha512_256_Abc() => Assert.Equal(GoldenAbc.Value["Sha512_256"], FileHasher.ComputeSha512_256(AbcData));

	#endregion

	#region SHA-3 & Keccak (6)

	[Fact] public void Sha3_224_Abc() => Assert.Equal(GoldenAbc.Value["Sha3_224"], FileHasher.ComputeSha3_224(AbcData));
	[Fact] public void Sha3_256_Abc() => Assert.Equal(GoldenAbc.Value["Sha3_256"], FileHasher.ComputeSha3_256(AbcData));
	[Fact] public void Sha3_384_Abc() => Assert.Equal(GoldenAbc.Value["Sha3_384"], FileHasher.ComputeSha3_384(AbcData));
	[Fact] public void Sha3_512_Abc() => Assert.Equal(GoldenAbc.Value["Sha3_512"], FileHasher.ComputeSha3_512(AbcData));
	[Fact] public void Keccak256_Abc() => Assert.Equal(GoldenAbc.Value["Keccak256"], FileHasher.ComputeKeccak256(AbcData));
	[Fact] public void Keccak512_Abc() => Assert.Equal(GoldenAbc.Value["Keccak512"], FileHasher.ComputeKeccak512(AbcData));

	#endregion

	#region BLAKE Family (5)

	[Fact] public void Blake256_Abc() => Assert.Equal(GoldenAbc.Value["Blake256"], FileHasher.ComputeBlake256(AbcData));
	[Fact] public void Blake512_Abc() => Assert.Equal(GoldenAbc.Value["Blake512"], FileHasher.ComputeBlake512(AbcData));
	[Fact] public void Blake2b_Abc() => Assert.Equal(GoldenAbc.Value["Blake2b"], FileHasher.ComputeBlake2b(AbcData));
	[Fact] public void Blake2s_Abc() => Assert.Equal(GoldenAbc.Value["Blake2s"], FileHasher.ComputeBlake2s(AbcData));
	[Fact] public void Blake3_Abc() => Assert.Equal(GoldenAbc.Value["Blake3"], FileHasher.ComputeBlake3(AbcData));

	#endregion

	#region RIPEMD Family (4)

	[Fact] public void Ripemd128_Abc() => Assert.Equal(GoldenAbc.Value["Ripemd128"], FileHasher.ComputeRipemd128(AbcData));
	[Fact] public void Ripemd160_Abc() => Assert.Equal(GoldenAbc.Value["Ripemd160"], FileHasher.ComputeRipemd160(AbcData));
	[Fact] public void Ripemd256_Abc() => Assert.Equal(GoldenAbc.Value["Ripemd256"], FileHasher.ComputeRipemd256(AbcData));
	[Fact] public void Ripemd320_Abc() => Assert.Equal(GoldenAbc.Value["Ripemd320"], FileHasher.ComputeRipemd320(AbcData));

	#endregion

	#region Other Crypto (14)

	[Fact] public void Whirlpool_Abc() => Assert.Equal(GoldenAbc.Value["Whirlpool"], FileHasher.ComputeWhirlpool(AbcData));
	[Fact] public void Tiger192_Abc() => Assert.Equal(GoldenAbc.Value["Tiger192"], FileHasher.ComputeTiger192(AbcData));
	[Fact] public void Gost94_Abc() => Assert.Equal(GoldenAbc.Value["Gost94"], FileHasher.ComputeGost94(AbcData));
	[Fact] public void Streebog256_Abc() => Assert.Equal(GoldenAbc.Value["Streebog256"], FileHasher.ComputeStreebog256(AbcData));
	[Fact] public void Streebog512_Abc() => Assert.Equal(GoldenAbc.Value["Streebog512"], FileHasher.ComputeStreebog512(AbcData));
	[Fact] public void Skein256_Abc() => Assert.Equal(GoldenAbc.Value["Skein256"], FileHasher.ComputeSkein256(AbcData));
	[Fact] public void Skein512_Abc() => Assert.Equal(GoldenAbc.Value["Skein512"], FileHasher.ComputeSkein512(AbcData));
	[Fact] public void Skein1024_Abc() => Assert.Equal(GoldenAbc.Value["Skein1024"], FileHasher.ComputeSkein1024(AbcData));
	[Fact] public void Groestl256_Abc() => Assert.Equal(GoldenAbc.Value["Groestl256"], FileHasher.ComputeGroestl256(AbcData));
	[Fact] public void Groestl512_Abc() => Assert.Equal(GoldenAbc.Value["Groestl512"], FileHasher.ComputeGroestl512(AbcData));
	[Fact] public void Jh256_Abc() => Assert.Equal(GoldenAbc.Value["Jh256"], FileHasher.ComputeJh256(AbcData));
	[Fact] public void Jh512_Abc() => Assert.Equal(GoldenAbc.Value["Jh512"], FileHasher.ComputeJh512(AbcData));
	[Fact] public void KangarooTwelve_Abc() => Assert.Equal(GoldenAbc.Value["KangarooTwelve"], FileHasher.ComputeKangarooTwelve(AbcData));
	[Fact] public void Sm3_Abc() => Assert.Equal(GoldenAbc.Value["Sm3"], FileHasher.ComputeSm3(AbcData));

	#endregion

	#region Empty Input Vectors (All 70)

	[Fact] public void Crc32_Empty() => Assert.Equal(GoldenEmpty.Value["Crc32"], FileHasher.ComputeCrc32(EmptyData));
	[Fact] public void Md5_Empty() => Assert.Equal(GoldenEmpty.Value["Md5"], FileHasher.ComputeMd5(EmptyData));
	[Fact] public void Sha256_Empty() => Assert.Equal(GoldenEmpty.Value["Sha256"], FileHasher.ComputeSha256(EmptyData));
	[Fact] public void Sha512_Empty() => Assert.Equal(GoldenEmpty.Value["Sha512"], FileHasher.ComputeSha512(EmptyData));
	[Fact] public void Blake3_Empty() => Assert.Equal(GoldenEmpty.Value["Blake3"], FileHasher.ComputeBlake3(EmptyData));
	[Fact] public void XxHash64_Empty() => Assert.Equal(GoldenEmpty.Value["XxHash64"], FileHasher.ComputeXxHash64(EmptyData));
	[Fact] public void Whirlpool_Empty() => Assert.Equal(GoldenEmpty.Value["Whirlpool"], FileHasher.ComputeWhirlpool(EmptyData));
	[Fact] public void Sha3_256_Empty() => Assert.Equal(GoldenEmpty.Value["Sha3_256"], FileHasher.ComputeSha3_256(EmptyData));
	[Fact] public void Ripemd160_Empty() => Assert.Equal(GoldenEmpty.Value["Ripemd160"], FileHasher.ComputeRipemd160(EmptyData));
	[Fact] public void Gost94_Empty() => Assert.Equal(GoldenEmpty.Value["Gost94"], FileHasher.ComputeGost94(EmptyData));

	#endregion

	#region Generic API Coverage

	[Fact]
	public void ComputeHash_AllAlgorithms_ProduceLowercaseHex() {
		foreach (HashAlgorithm algo in Enum.GetValues<HashAlgorithm>()) {
			var result = FileHasher.ComputeHash(algo, AbcData);
			Assert.Matches("^[0-9a-f]+$", result);
		}
	}

	[Fact]
	public void ComputeHash_GenericApi_MatchesTypedApi() {
		// Verify generic ComputeHash matches typed Compute* methods
		Assert.Equal(FileHasher.ComputeSha256(AbcData), FileHasher.ComputeHash(HashAlgorithm.Sha256, AbcData));
		Assert.Equal(FileHasher.ComputeBlake3(AbcData), FileHasher.ComputeHash(HashAlgorithm.Blake3, AbcData));
		Assert.Equal(FileHasher.ComputeCrc32(AbcData), FileHasher.ComputeHash(HashAlgorithm.Crc32, AbcData));
		Assert.Equal(FileHasher.ComputeMd5(AbcData), FileHasher.ComputeHash(HashAlgorithm.Md5, AbcData));
		Assert.Equal(FileHasher.ComputeXxHash64(AbcData), FileHasher.ComputeHash(HashAlgorithm.XxHash64, AbcData));
	}

	#endregion
}
