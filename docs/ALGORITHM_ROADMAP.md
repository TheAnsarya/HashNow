# HashNow Algorithm Roadmap

**Status**: v1.4.4 — All 70 algorithms implemented!

## Summary

| Category | Count | Status |
|----------|-------|--------|
| Checksums & CRCs | 9 | COMPLETE |
| Non-Crypto Fast | 21 | COMPLETE |
| Cryptographic | 26 | COMPLETE |
| Other Crypto | 14 | COMPLETE |
| **Total** | **70** | **ALL DONE** |

## Powered by StreamHash

All 70 hash algorithms are provided by [StreamHash](https://www.nuget.org/packages/StreamHash) — a high-performance, memory-efficient streaming hash library for .NET 10+. Every algorithm is implemented in pure native C# with SIMD acceleration (AVX2, SSE4.2, AES-NI) and zero external cryptography dependencies.

| Package | Version | Purpose |
|---------|---------|---------|
| [StreamHash](https://www.nuget.org/packages/StreamHash) | 1.11.1 | All 70 hash algorithm implementations |

HashNow calls `HashFacade.ComputeHashHex()` from StreamHash for every algorithm. StreamHash handles all the internals: streaming, buffering, SIMD dispatch, and finalization. No other hash libraries are used.

## Algorithm List

### 1. Checksums & CRCs (9)

- CRC32, CRC32C, CRC64
- CRC16-CCITT, CRC16-MODBUS, CRC16-USB
- Adler-32, Fletcher-16, Fletcher-32

### 2. Non-Crypto Fast Hashes (21)

- xxHash32, xxHash64, xxHash3, xxHash128
- MurmurHash3-32, MurmurHash3-128
- CityHash64, CityHash128, FarmHash64
- SpookyHash V2, SipHash-2-4, HighwayHash64
- MetroHash64, MetroHash128, wyhash64
- FNV-1a-32, FNV-1a-64
- DJB2, DJB2a, SDBM, LoseLose

### 3. Cryptographic Hashes (26)

- MD2, MD4, MD5
- SHA-0, SHA-1, SHA-224, SHA-256, SHA-384, SHA-512
- SHA-512/224, SHA-512/256
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
