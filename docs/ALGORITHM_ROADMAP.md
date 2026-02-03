# HashNow Algorithm Roadmap

**Status**: v2.0.0 - All 58 algorithms implemented!

## Summary

| Category | Count | Status |
|----------|-------|--------|
| Checksums & CRCs | 6 | COMPLETE |
| Non-Crypto Fast | 12 | COMPLETE |
| Cryptographic | 26 | COMPLETE |
| Other Crypto | 14 | COMPLETE |
| **Total** | **58** | **ALL DONE** |

## NuGet Packages Used

| Package | Version | Algorithms |
|---------|---------|------------|
| System.IO.Hashing | 10.0.2 | CRC32, CRC64, XXHash family |
| BouncyCastle.Cryptography | 2.6.2 | MD2/4, SHA-224, SHA-512/t, SHA3, Keccak, BLAKE, RIPEMD, etc |
| Blake3 | 2.2.0 | BLAKE3 |
| SauceControl.Blake2Fast | 2.0.0 | BLAKE2b, BLAKE2s |
| System.Data.HashFunction.MurmurHash | 2.0.0 | MurmurHash3 |
| System.Data.HashFunction.CityHash | 2.0.0 | CityHash |
| System.Data.HashFunction.SpookyHash | 2.0.0 | SpookyHash V2 |
| HashDepot | 3.2.0 | SipHash |

## Algorithm List

### 1. Checksums & CRCs (6)
- CRC32, CRC32C, CRC64, Adler-32, Fletcher-16, Fletcher-32

### 2. Non-Crypto Fast Hashes (12)
- XXHash32, XXHash64, XXHash3, XXHash128
- MurmurHash3-32, MurmurHash3-128
- CityHash64, CityHash128, FarmHash64
- SpookyHash V2, SipHash-2-4, HighwayHash64

### 3. Cryptographic Hashes (26)
- MD2, MD4, MD5
- SHA-0, SHA-1
- SHA-224, SHA-256, SHA-384, SHA-512, SHA-512/224, SHA-512/256
- SHA3-224, SHA3-256, SHA3-384, SHA3-512
- Keccak-256, Keccak-512
- BLAKE-256, BLAKE-512, BLAKE2b, BLAKE2s, BLAKE3
- RIPEMD-128, RIPEMD-160, RIPEMD-256, RIPEMD-320

### 4. Other Crypto Hashes (14)
- Whirlpool, Tiger-192
- GOST R 34.11-94, Streebog-256, Streebog-512
- Skein-256, Skein-512, Skein-1024
- Groestl-256, Groestl-512
- JH-256, JH-512
- KangarooTwelve, SM3
