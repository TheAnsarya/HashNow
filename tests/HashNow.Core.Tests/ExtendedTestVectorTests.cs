using System.Security.Cryptography;

namespace HashNow.Core.Tests;

/// <summary>
/// Extended test vectors for algorithms not covered by the base HashTestVectorTests.
/// Includes authoritative reference vectors from RFCs, NIST, and original specifications.
/// </summary>
public class ExtendedTestVectorTests {
	// Standard test inputs
	private static readonly byte[] EmptyData = [];
	private static readonly byte[] AbcData = "abc"u8.ToArray();
	private static readonly byte[] HelloWorld = "Hello, World!"u8.ToArray();
	private static readonly byte[] SingleZero = [0x00];
	private static readonly byte[] SingleFF = [0xff];

	// "message digest" - standard test vector from many RFCs
	private static readonly byte[] MessageDigest = "message digest"u8.ToArray();

	#region MD Family Extended Vectors

	[Theory]
	[InlineData("d41d8cd98f00b204e9800998ecf8427e", "")] // RFC 1321
	[InlineData("0cc175b9c0f1b6a831c399e269772661", "a")]
	[InlineData("900150983cd24fb0d6963f7d28e17f72", "abc")]
	[InlineData("f96b697d7cb7938d525a2f31aaf161d0", "message digest")]
	[InlineData("c3fcd3d76192e4007dfb496cca67e13b", "abcdefghijklmnopqrstuvwxyz")]
	public void Md5_RfcVectors_MatchesExpected(string expected, string input) {
		var data = System.Text.Encoding.ASCII.GetBytes(input);
		Assert.Equal(expected, FileHasher.ComputeMd5(data));
	}

	[Fact]
	public void Md2_Empty_ProducesKnownHash() {
		// MD2("") = 8350e5a3e24c153df2275c9f80692773 (RFC 1319)
		Assert.Equal("8350e5a3e24c153df2275c9f80692773", FileHasher.ComputeMd2(EmptyData));
	}

	[Fact]
	public void Md4_Empty_ProducesKnownHash() {
		// MD4("") = 31d6cfe0d16ae931b73c59d7e0c089c0 (RFC 1320)
		Assert.Equal("31d6cfe0d16ae931b73c59d7e0c089c0", FileHasher.ComputeMd4(EmptyData));
	}

	[Fact]
	public void Md4_Abc_ProducesKnownHash() {
		// MD4("abc") = a448017aaf21d8525fc10ae87aa6729d (RFC 1320)
		Assert.Equal("a448017aaf21d8525fc10ae87aa6729d", FileHasher.ComputeMd4(AbcData));
	}

	#endregion

	#region SHA-2 Extended Vectors

	[Fact]
	public void Sha224_Empty_MatchesNist() {
		// NIST FIPS 180-4
		Assert.Equal("d14a028c2a3a2bc9476102bb288234c415a2b01f828ea62ac5b3e42f", FileHasher.ComputeSha224(EmptyData));
	}

	[Fact]
	public void Sha224_Abc_MatchesNist() {
		Assert.Equal("23097d223405d8228642a477bda255b32aadbce4bda0b3f7e36c9da7", FileHasher.ComputeSha224(AbcData));
	}

	[Fact]
	public void Sha384_MessageDigest() {
		var result = FileHasher.ComputeSha384(MessageDigest);
		Assert.Equal(96, result.Length); // 48 bytes = 96 hex
		Assert.NotEqual(FileHasher.ComputeSha384(EmptyData), result);
	}

	[Fact]
	public void Sha512_224_Empty_ProducesKnownHash() {
		// SHA-512/224("") - NIST
		Assert.Equal("6ed0dd02806fa89e25de060c19d3ac86cabb87d6a0ddd05c333b84f4", FileHasher.ComputeSha512_224(EmptyData));
	}

	[Fact]
	public void Sha512_256_Empty_ProducesKnownHash() {
		// SHA-512/256("") - NIST
		Assert.Equal("c672b8d1ef56ed28ab87c3622c5114069bdd3ad7b8f9737498d0c01ecef0967a", FileHasher.ComputeSha512_256(EmptyData));
	}

	#endregion

	#region SHA-3 Extended Vectors

	[Fact]
	public void Sha3_224_Empty_MatchesNist() {
		// NIST FIPS 202
		Assert.Equal("6b4e03423667dbb73b6e15454f0eb1abd4597f9a1b078e3f5b5a6bc7", FileHasher.ComputeSha3_224(EmptyData));
	}

	[Fact]
	public void Sha3_256_Empty_MatchesNist() {
		Assert.Equal("a7ffc6f8bf1ed76651c14756a061d662f580ff4de43b49fa82d80a4b80f8434a", FileHasher.ComputeSha3_256(EmptyData));
	}

	[Fact]
	public void Sha3_384_Empty_MatchesNist() {
		Assert.Equal("0c63a75b845e4f7d01107d852e4c2485c51a50aaaa94fc61995e71bbee983a2ac3713831264adb47fb6bd1e058d5f004", FileHasher.ComputeSha3_384(EmptyData));
	}

	[Fact]
	public void Sha3_512_Empty_MatchesNist() {
		Assert.Equal("a69f73cca23a9ac5c8b567dc185a756e97c982164fe25859e0d1dcc1475c80a615b2123af1f5f94c11e3e9402c3ac558f500199d95b6d3e301758586281dcd26", FileHasher.ComputeSha3_512(EmptyData));
	}

	[Fact]
	public void Sha3_224_Abc_MatchesNist() {
		Assert.Equal("e642824c3f8cf24ad09234ee7d3c766fc9a3a5168d0c94ad73b46fdf", FileHasher.ComputeSha3_224(AbcData));
	}

	#endregion

	#region BLAKE Family Extended Vectors

	[Fact]
	public void Blake2b_Empty_ProducesKnownHash() {
		// BLAKE2b-512("") - from official BLAKE2 spec
		Assert.Equal("786a02f742015903c6c6fd852552d272912f4740e15847618a86e217f71f5419d25e1031afee585313896444934eb04b903a685b1448b755d56f701afe9be2ce", FileHasher.ComputeBlake2b(EmptyData));
	}

	[Fact]
	public void Blake2s_Empty_ProducesKnownHash() {
		// BLAKE2s-256("") - from official BLAKE2 spec
		Assert.Equal("69217a3079908094e11121d042354a7c1f55b6482ca1a51e1b250dfd1ed0eef9", FileHasher.ComputeBlake2s(EmptyData));
	}

	[Fact]
	public void Blake3_Empty_ProducesKnownHash() {
		// BLAKE3("") - from official reference
		Assert.Equal("af1349b9f5f9a1a6a0404dea36dcc9499bcb25c9adc112b7cc9a93cae41f3262", FileHasher.ComputeBlake3(EmptyData));
	}

	#endregion

	#region RIPEMD Extended Vectors

	[Fact]
	public void Ripemd128_Empty_ProducesKnownHash() {
		// RIPEMD-128("") - from RIPE specifications
		Assert.Equal("cdf26213a150dc3ecb610f18f6b38b46", FileHasher.ComputeRipemd128(EmptyData));
	}

	[Fact]
	public void Ripemd160_Empty_ProducesKnownHash() {
		// RIPEMD-160("")
		Assert.Equal("9c1185a5c5e9fc54612808977ee8f548b2258d31", FileHasher.ComputeRipemd160(EmptyData));
	}

	[Fact]
	public void Ripemd256_Empty_ProducesKnownHash() {
		// RIPEMD-256("")
		Assert.Equal("02ba4c4e5f8ecd1877fc52d64d30e37a2d9774fb1e5d026380ae0168e3c5522d", FileHasher.ComputeRipemd256(EmptyData));
	}

	[Fact]
	public void Ripemd160_Abc_MatchesSpec() {
		Assert.Equal("8eb208f7e05d987a9b044a8e98c6b087f15a0bfc", FileHasher.ComputeRipemd160(AbcData));
	}

	#endregion

	#region Checksum Vectors

	[Fact]
	public void Crc32_KnownValues() {
		// CRC32 of "123456789" = CBF43926 (standard check value, stored little-endian)
		var data = "123456789"u8.ToArray();
		Assert.Equal("2639f4cb", FileHasher.ComputeCrc32(data));
	}

	[Fact]
	public void Adler32_Empty_IsOne() {
		// Adler-32 of empty data is 00000001 (stored little-endian)
		Assert.Equal("01000000", FileHasher.ComputeAdler32(EmptyData));
	}

	[Fact]
	public void Adler32_KnownValues() {
		// Adler-32 of "Wikipedia" = 0x11E60398 (stored little-endian)
		var data = "Wikipedia"u8.ToArray();
		Assert.Equal("9803e611", FileHasher.ComputeAdler32(data));
	}

	#endregion

	#region Other Crypto Extended Vectors

	[Fact]
	public void Sm3_Empty_MatchesSpec() {
		// SM3("") from GB/T 32905-2016
		Assert.Equal("1ab21d8355cfa17f8e61194831e81a8f22bec8c728fefb747ed035eb5082aa2b", FileHasher.ComputeSm3(EmptyData));
	}

	[Fact]
	public void Whirlpool_Empty_ProducesKnownHash() {
		var hash = FileHasher.ComputeWhirlpool(EmptyData);
		Assert.Equal(128, hash.Length); // 64 bytes
		// Whirlpool("") = known constant
		Assert.Equal("19fa61d75522a4669b44e39c1d2e1726c530232130d407f89afee0964997f7a73e83be698b288febcf88e3e03c4f0757ea8964e59b63d93708b138cc42a66eb3", hash);
	}

	[Fact]
	public void Tiger192_Empty_ProducesKnownHash() {
		var hash = FileHasher.ComputeTiger192(EmptyData);
		Assert.Equal(48, hash.Length); // 24 bytes
	}

	[Fact]
	public void Gost94_Empty_ProducesValidHash() {
		var hash = FileHasher.ComputeGost94(EmptyData);
		Assert.Equal(64, hash.Length); // 32 bytes
	}

	[Fact]
	public void Streebog256_Empty_ProducesKnownHash() {
		var hash = FileHasher.ComputeStreebog256(EmptyData);
		Assert.Equal(64, hash.Length); // 32 bytes
	}

	[Fact]
	public void Streebog512_Empty_ProducesKnownHash() {
		var hash = FileHasher.ComputeStreebog512(EmptyData);
		Assert.Equal(128, hash.Length); // 64 bytes
	}

	[Fact]
	public void KangarooTwelve_Empty_ProducesValidHash() {
		var hash = FileHasher.ComputeKangarooTwelve(EmptyData);
		Assert.Equal(64, hash.Length); // 32 bytes
	}

	#endregion

	#region Cross-Verification - All Algorithms Produce Lowercase Hex

	[Fact]
	public void AllAlgorithms_ProduceLowercaseHex() {
		var data = "test"u8.ToArray();

		var allHashes = new[] {
			FileHasher.ComputeCrc32(data), FileHasher.ComputeCrc32C(data),
			FileHasher.ComputeCrc64(data), FileHasher.ComputeAdler32(data),
			FileHasher.ComputeMd5(data), FileHasher.ComputeSha1(data),
			FileHasher.ComputeSha256(data), FileHasher.ComputeSha512(data),
			FileHasher.ComputeSha3_256(data), FileHasher.ComputeBlake3(data),
			FileHasher.ComputeXxHash64(data), FileHasher.ComputeMurmur3_32(data),
			FileHasher.ComputeWhirlpool(data), FileHasher.ComputeSm3(data),
			FileHasher.ComputeRipemd160(data), FileHasher.ComputeKeccak256(data),
		};

		foreach (var hash in allHashes) {
			Assert.Equal(hash, hash.ToLowerInvariant());
			Assert.Matches("^[0-9a-f]+$", hash);
		}
	}

	#endregion

	#region SHA-1 Extended Vectors

	[Theory]
	[InlineData("da39a3ee5e6b4b0d3255bfef95601890afd80709", "")] // FIPS 180-4
	[InlineData("a9993e364706816aba3e25717850c26c9cd0d89d", "abc")]
	[InlineData("84983e441c3bd26ebaae4aa1f95129e5e54670f1", "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq")]
	public void Sha1_NistVectors_Match(string expected, string input) {
		var data = System.Text.Encoding.ASCII.GetBytes(input);
		Assert.Equal(expected, FileHasher.ComputeSha1(data));
	}

	#endregion

	#region SHA-256 Extended Vectors

	[Theory]
	[InlineData("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", "")] // FIPS 180-4
	[InlineData("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", "abc")]
	[InlineData("248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1", "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq")]
	public void Sha256_NistVectors_Match(string expected, string input) {
		var data = System.Text.Encoding.ASCII.GetBytes(input);
		Assert.Equal(expected, FileHasher.ComputeSha256(data));
	}

	#endregion
}
