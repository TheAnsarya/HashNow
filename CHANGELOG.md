# Changelog

All notable changes to HashNow will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-02

### Added
- **58 Hash Algorithms** across 4 categories:
	- Checksums (6): CRC32, CRC32C, CRC64, Adler32, Fletcher16, Fletcher32
	- Fast Non-Crypto (12): xxHash family, MurmurHash3, CityHash, FarmHash, SpookyHash, SipHash, HighwayHash
	- Cryptographic (26): MD family, SHA family (0/1/2/3), BLAKE family, RIPEMD family
	- Other Crypto (14): Whirlpool, Tiger, SM3, GOST variants, Streebog, Skein, KangarooTwelve
- **Parallel hash computation** - All 58 algorithms run concurrently using `Parallel.Invoke()`
- **JSON output** with tab indentation (never spaces)
- **Windows Explorer context menu** integration - Right-click any file → "Hash this file now"
- **Progress reporting** for large files (>3 seconds estimated)
- **Comprehensive test suite** - 92 xUnit tests including test vectors for all algorithms
- **Performance benchmarks** - BenchmarkDotNet suite for comparing algorithms
- **Manual testing guide** - Step-by-step checklist in `docs/MANUAL_TESTING.md`
- **Cross-verification** - Tests verify against System.Security.Cryptography and System.IO.Hashing
- **Memory efficiency** - ArrayPool for memory management, single-pass file reading

### Changed
- **License**: Changed to **The Unlicense** (public domain) - free for any use
- **Indentation**: All files use **tabs** (never spaces), including JSON output
- **Modern .NET**: Targets .NET 10 with C# 14 features
- **Code patterns**: File-scoped namespaces, pattern matching, collection expressions
- **Hash output**: All hex values are lowercase

### Technical Details
- Built with .NET 10, C# 14
- Uses System.Text.Json with tab indentation
- Single-pass file reading with 1MB buffer
- ArrayPool memory management for reduced GC pressure
- Nullable reference types enabled
- Cross-platform library (HashNow.Core) reusable in any .NET project

### Dependencies
- Blake3 2.2.0
- BouncyCastle.Cryptography 2.6.2
- HashDepot 3.2.0
- murmurhash 1.0.3
- SauceControl.Blake2Fast 2.0.0
- System.Data.HashFunction.* 2.0.0
- System.IO.Hashing 10.0.2

### Known Issues
- Context menu requires administrator privileges to install
- Windows-only (context menu integration)

## [Unreleased]

### Planned
- macOS and Linux support
- GUI application
- Hash comparison features
- File verification against known checksums
- Batch file hashing
- Custom algorithm selection
