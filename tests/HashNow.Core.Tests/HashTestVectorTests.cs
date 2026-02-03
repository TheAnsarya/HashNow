using System.IO.Hashing;
using System.Security.Cryptography;

namespace HashNow.Core.Tests;

/// <summary>
/// Test vectors with known input/output pairs for all hash algorithms.
/// These values are verified against reference implementations.
/// </summary>
public class HashTestVectorTests {
	// Test input: empty data
	private static readonly byte[] EmptyData = [];

	// Test input: single null byte
	private static readonly byte[] SingleZero = [0x00];

	// Test input: "abc" in ASCII
	private static readonly byte[] AbcData = "abc"u8.ToArray();

	// Test input: "Hello, World!" in ASCII
	private static readonly byte[] HelloWorld = "Hello, World!"u8.ToArray();

	#region Empty Data Test Vectors

	[Theory]
	[InlineData("00000000")] // CRC32
	public void Crc32_Empty_MatchesExpected(string expected) {
		Assert.Equal(expected, FileHasher.ComputeCrc32(EmptyData));
	}

	[Theory]
	[InlineData("d41d8cd98f00b204e9800998ecf8427e")] // MD5
	public void Md5_Empty_MatchesExpected(string expected) {
		Assert.Equal(expected, FileHasher.ComputeMd5(EmptyData));
	}

	[Theory]
	[InlineData("da39a3ee5e6b4b0d3255bfef95601890afd80709")] // SHA1
	public void Sha1_Empty_MatchesExpected(string expected) {
		Assert.Equal(expected, FileHasher.ComputeSha1(EmptyData));
	}

	[Theory]
	[InlineData("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")] // SHA256
	public void Sha256_Empty_MatchesExpected(string expected) {
		Assert.Equal(expected, FileHasher.ComputeSha256(EmptyData));
	}

	[Theory]
	[InlineData("38b060a751ac96384cd9327eb1b1e36a21fdb71114be07434c0cc7bf63f6e1da274edebfe76f65fbd51ad2f14898b95b")] // SHA384
	public void Sha384_Empty_MatchesExpected(string expected) {
		Assert.Equal(expected, FileHasher.ComputeSha384(EmptyData));
	}

	[Theory]
	[InlineData("cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e")] // SHA512
	public void Sha512_Empty_MatchesExpected(string expected) {
		Assert.Equal(expected, FileHasher.ComputeSha512(EmptyData));
	}

	#endregion

	#region "abc" Test Vectors (NIST Standard Test)

	[Fact]
	public void Md5_Abc_MatchesExpected() {
		Assert.Equal("900150983cd24fb0d6963f7d28e17f72", FileHasher.ComputeMd5(AbcData));
	}

	[Fact]
	public void Sha1_Abc_MatchesExpected() {
		Assert.Equal("a9993e364706816aba3e25717850c26c9cd0d89d", FileHasher.ComputeSha1(AbcData));
	}

	[Fact]
	public void Sha256_Abc_MatchesExpected() {
		Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", FileHasher.ComputeSha256(AbcData));
	}

	[Fact]
	public void Sha384_Abc_MatchesExpected() {
		Assert.Equal("cb00753f45a35e8bb5a03d699ac65007272c32ab0eded1631a8b605a43ff5bed8086072ba1e7cc2358baeca134c825a7", FileHasher.ComputeSha384(AbcData));
	}

	[Fact]
	public void Sha512_Abc_MatchesExpected() {
		Assert.Equal("ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f", FileHasher.ComputeSha512(AbcData));
	}

	[Fact]
	public void Sha3_256_Abc_MatchesExpected() {
		// SHA3-256("abc")
		Assert.Equal("3a985da74fe225b2045c172d6bd390bd855f086e3e9d525b46bfe24511431532", FileHasher.ComputeSha3_256(AbcData));
	}

	[Fact]
	public void Sha3_512_Abc_MatchesExpected() {
		// SHA3-512("abc")
		Assert.Equal("b751850b1a57168a5693cd924b6b096e08f621827444f70d884f5d0240d2712e10e116e9192af3c91a7ec57647e3934057340b4cf408d5a56592f8274eec53f0", FileHasher.ComputeSha3_512(AbcData));
	}

	[Fact]
	public void Blake2b_Abc_ProducesValidHash() {
		var hash = FileHasher.ComputeBlake2b(AbcData);
		Assert.Equal(128, hash.Length); // 64 bytes = 128 hex chars
		Assert.NotEqual(FileHasher.ComputeBlake2b(EmptyData), hash);
	}

	[Fact]
	public void Blake2s_Abc_ProducesValidHash() {
		var hash = FileHasher.ComputeBlake2s(AbcData);
		Assert.Equal(64, hash.Length); // 32 bytes = 64 hex chars
		Assert.NotEqual(FileHasher.ComputeBlake2s(EmptyData), hash);
	}

	[Fact]
	public void Blake3_Abc_ProducesValidHash() {
		var hash = FileHasher.ComputeBlake3(AbcData);
		Assert.Equal(64, hash.Length); // 32 bytes = 64 hex chars
		Assert.NotEqual(FileHasher.ComputeBlake3(EmptyData), hash);
	}

	[Fact]
	public void Ripemd160_Abc_MatchesExpected() {
		// RIPEMD-160("abc") = 8eb208f7e05d987a9b044a8e98c6b087f15a0bfc
		Assert.Equal("8eb208f7e05d987a9b044a8e98c6b087f15a0bfc", FileHasher.ComputeRipemd160(AbcData));
	}

	[Fact]
	public void Whirlpool_Abc_ProducesValidHash() {
		var hash = FileHasher.ComputeWhirlpool(AbcData);
		Assert.Equal(128, hash.Length); // 64 bytes = 128 hex chars
	}

	[Fact]
	public void Tiger192_Abc_ProducesValidHash() {
		var hash = FileHasher.ComputeTiger192(AbcData);
		Assert.Equal(48, hash.Length); // 24 bytes = 48 hex chars
	}

	[Fact]
	public void Sm3_Abc_MatchesExpected() {
		// SM3("abc") = 66c7f0f462eeedd9d1f2d46bdc10e4e24167c4875cf2f7a2297da02b8f4ba8e0
		Assert.Equal("66c7f0f462eeedd9d1f2d46bdc10e4e24167c4875cf2f7a2297da02b8f4ba8e0", FileHasher.ComputeSm3(AbcData));
	}

	#endregion

	#region "Hello, World!" Test Vectors

	[Fact]
	public void Md5_HelloWorld_MatchesExpected() {
		Assert.Equal("65a8e27d8879283831b664bd8b7f0ad4", FileHasher.ComputeMd5(HelloWorld));
	}

	[Fact]
	public void Sha256_HelloWorld_MatchesExpected() {
		Assert.Equal("dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f", FileHasher.ComputeSha256(HelloWorld));
	}

	#endregion

	#region Single Zero Byte Test Vectors

	[Fact]
	public void Md5_SingleZero_MatchesExpected() {
		Assert.Equal("93b885adfe0da089cdf634904fd59f71", FileHasher.ComputeMd5(SingleZero));
	}

	[Fact]
	public void Sha256_SingleZero_MatchesExpected() {
		Assert.Equal("6e340b9cffb37a989ca544e6bb780a2c78901d3fb33738768511a30617afa01d", FileHasher.ComputeSha256(SingleZero));
	}

	#endregion

	#region Cross-Verification with System Libraries

	[Theory]
	[InlineData("test data for verification")]
	[InlineData("another test string")]
	[InlineData("")]
	public void Md5_MatchesSystemCryptography(string input) {
		var data = System.Text.Encoding.UTF8.GetBytes(input);
		var expected = Convert.ToHexStringLower(MD5.HashData(data));
		Assert.Equal(expected, FileHasher.ComputeMd5(data));
	}

	[Theory]
	[InlineData("test data for verification")]
	[InlineData("another test string")]
	[InlineData("")]
	public void Sha256_MatchesSystemCryptography(string input) {
		var data = System.Text.Encoding.UTF8.GetBytes(input);
		var expected = Convert.ToHexStringLower(SHA256.HashData(data));
		Assert.Equal(expected, FileHasher.ComputeSha256(data));
	}

	[Theory]
	[InlineData("test data for verification")]
	[InlineData("another test string")]
	[InlineData("")]
	public void Sha512_MatchesSystemCryptography(string input) {
		var data = System.Text.Encoding.UTF8.GetBytes(input);
		var expected = Convert.ToHexStringLower(SHA512.HashData(data));
		Assert.Equal(expected, FileHasher.ComputeSha512(data));
	}

	[Theory]
	[InlineData("test data for verification")]
	[InlineData("another test string")]
	[InlineData("")]
	public void Crc32_MatchesSystemIOHashing(string input) {
		var data = System.Text.Encoding.UTF8.GetBytes(input);
		var expected = Convert.ToHexStringLower(Crc32.Hash(data));
		Assert.Equal(expected, FileHasher.ComputeCrc32(data));
	}

	[Theory]
	[InlineData("test data for verification")]
	[InlineData("another test string")]
	[InlineData("")]
	public void XxHash64_MatchesSystemIOHashing(string input) {
		var data = System.Text.Encoding.UTF8.GetBytes(input);
		var expected = Convert.ToHexStringLower(XxHash64.Hash(data));
		Assert.Equal(expected, FileHasher.ComputeXxHash64(data));
	}

	[Theory]
	[InlineData("test data for verification")]
	[InlineData("another test string")]
	[InlineData("")]
	public void XxHash3_MatchesSystemIOHashing(string input) {
		var data = System.Text.Encoding.UTF8.GetBytes(input);
		var expected = Convert.ToHexStringLower(XxHash3.Hash(data));
		Assert.Equal(expected, FileHasher.ComputeXxHash3(data));
	}

	#endregion

	#region Hash Length Verification for All Algorithms

	[Fact]
	public void AllAlgorithms_ProduceCorrectLengths() {
		var data = AbcData;

		// Checksums
		Assert.Equal(8, FileHasher.ComputeCrc32(data).Length);      // 4 bytes
		Assert.Equal(8, FileHasher.ComputeCrc32C(data).Length);     // 4 bytes
		Assert.Equal(16, FileHasher.ComputeCrc64(data).Length);     // 8 bytes
		Assert.Equal(8, FileHasher.ComputeAdler32(data).Length);    // 4 bytes
		Assert.Equal(4, FileHasher.ComputeFletcher16(data).Length); // 2 bytes
		Assert.Equal(8, FileHasher.ComputeFletcher32(data).Length); // 4 bytes

		// Fast hashes
		Assert.Equal(8, FileHasher.ComputeXxHash32(data).Length);   // 4 bytes
		Assert.Equal(16, FileHasher.ComputeXxHash64(data).Length);  // 8 bytes
		Assert.Equal(16, FileHasher.ComputeXxHash3(data).Length);   // 8 bytes
		Assert.Equal(32, FileHasher.ComputeXxHash128(data).Length); // 16 bytes
		Assert.Equal(8, FileHasher.ComputeMurmur3_32(data).Length); // 4 bytes
		Assert.Equal(32, FileHasher.ComputeMurmur3_128(data).Length); // 16 bytes
		Assert.Equal(16, FileHasher.ComputeSipHash24(data).Length); // 8 bytes

		// Cryptographic
		Assert.Equal(32, FileHasher.ComputeMd2(data).Length);       // 16 bytes
		Assert.Equal(32, FileHasher.ComputeMd4(data).Length);       // 16 bytes
		Assert.Equal(32, FileHasher.ComputeMd5(data).Length);       // 16 bytes
		Assert.Equal(40, FileHasher.ComputeSha1(data).Length);      // 20 bytes
		Assert.Equal(56, FileHasher.ComputeSha224(data).Length);    // 28 bytes
		Assert.Equal(64, FileHasher.ComputeSha256(data).Length);    // 32 bytes
		Assert.Equal(96, FileHasher.ComputeSha384(data).Length);    // 48 bytes
		Assert.Equal(128, FileHasher.ComputeSha512(data).Length);   // 64 bytes
		Assert.Equal(56, FileHasher.ComputeSha512_224(data).Length); // 28 bytes
		Assert.Equal(64, FileHasher.ComputeSha512_256(data).Length); // 32 bytes

		// SHA3
		Assert.Equal(56, FileHasher.ComputeSha3_224(data).Length);  // 28 bytes
		Assert.Equal(64, FileHasher.ComputeSha3_256(data).Length);  // 32 bytes
		Assert.Equal(96, FileHasher.ComputeSha3_384(data).Length);  // 48 bytes
		Assert.Equal(128, FileHasher.ComputeSha3_512(data).Length); // 64 bytes
		Assert.Equal(64, FileHasher.ComputeKeccak256(data).Length); // 32 bytes
		Assert.Equal(128, FileHasher.ComputeKeccak512(data).Length); // 64 bytes

		// BLAKE
		Assert.Equal(64, FileHasher.ComputeBlake256(data).Length);  // 32 bytes
		Assert.Equal(128, FileHasher.ComputeBlake512(data).Length); // 64 bytes
		Assert.Equal(128, FileHasher.ComputeBlake2b(data).Length);  // 64 bytes
		Assert.Equal(64, FileHasher.ComputeBlake2s(data).Length);   // 32 bytes
		Assert.Equal(64, FileHasher.ComputeBlake3(data).Length);    // 32 bytes

		// RIPEMD
		Assert.Equal(32, FileHasher.ComputeRipemd128(data).Length); // 16 bytes
		Assert.Equal(40, FileHasher.ComputeRipemd160(data).Length); // 20 bytes
		Assert.Equal(64, FileHasher.ComputeRipemd256(data).Length); // 32 bytes
		Assert.Equal(80, FileHasher.ComputeRipemd320(data).Length); // 40 bytes

		// Other crypto
		Assert.Equal(128, FileHasher.ComputeWhirlpool(data).Length);  // 64 bytes
		Assert.Equal(48, FileHasher.ComputeTiger192(data).Length);    // 24 bytes
		Assert.Equal(64, FileHasher.ComputeGost94(data).Length);      // 32 bytes
		Assert.Equal(64, FileHasher.ComputeStreebog256(data).Length); // 32 bytes
		Assert.Equal(128, FileHasher.ComputeStreebog512(data).Length); // 64 bytes
		Assert.Equal(64, FileHasher.ComputeSkein256(data).Length);    // 32 bytes
		Assert.Equal(128, FileHasher.ComputeSkein512(data).Length);   // 64 bytes
		Assert.Equal(256, FileHasher.ComputeSkein1024(data).Length);  // 128 bytes
		Assert.Equal(64, FileHasher.ComputeSm3(data).Length);         // 32 bytes
		Assert.Equal(64, FileHasher.ComputeKangarooTwelve(data).Length); // 32 bytes
	}

	#endregion

	#region Determinism Tests

	[Fact]
	public void AllAlgorithms_AreDeterministic() {
		var data = "determinism test data"u8.ToArray();

		// Run each algorithm twice and verify same result
		Assert.Equal(FileHasher.ComputeCrc32(data), FileHasher.ComputeCrc32(data));
		Assert.Equal(FileHasher.ComputeMd5(data), FileHasher.ComputeMd5(data));
		Assert.Equal(FileHasher.ComputeSha256(data), FileHasher.ComputeSha256(data));
		Assert.Equal(FileHasher.ComputeSha3_256(data), FileHasher.ComputeSha3_256(data));
		Assert.Equal(FileHasher.ComputeBlake3(data), FileHasher.ComputeBlake3(data));
		Assert.Equal(FileHasher.ComputeXxHash64(data), FileHasher.ComputeXxHash64(data));
		Assert.Equal(FileHasher.ComputeWhirlpool(data), FileHasher.ComputeWhirlpool(data));
		Assert.Equal(FileHasher.ComputeSm3(data), FileHasher.ComputeSm3(data));
	}

	#endregion

	#region Uniqueness Tests

	[Fact]
	public void DifferentInputs_ProduceDifferentHashes() {
		var data1 = "input one"u8.ToArray();
		var data2 = "input two"u8.ToArray();

		Assert.NotEqual(FileHasher.ComputeMd5(data1), FileHasher.ComputeMd5(data2));
		Assert.NotEqual(FileHasher.ComputeSha256(data1), FileHasher.ComputeSha256(data2));
		Assert.NotEqual(FileHasher.ComputeBlake3(data1), FileHasher.ComputeBlake3(data2));
		Assert.NotEqual(FileHasher.ComputeXxHash64(data1), FileHasher.ComputeXxHash64(data2));
	}

	[Fact]
	public void SingleBitDifference_ProducesDifferentHashes() {
		var data1 = new byte[] { 0b00000000 };
		var data2 = new byte[] { 0b00000001 };

		Assert.NotEqual(FileHasher.ComputeMd5(data1), FileHasher.ComputeMd5(data2));
		Assert.NotEqual(FileHasher.ComputeSha256(data1), FileHasher.ComputeSha256(data2));
		Assert.NotEqual(FileHasher.ComputeCrc32(data1), FileHasher.ComputeCrc32(data2));
	}

	#endregion
}
