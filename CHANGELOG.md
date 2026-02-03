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

## [1.0.1] - 2026-02-03

### Added
- **Comprehensive XML Documentation** - Every class, method, property, and field now has detailed XML documentation
- **Auto-Install on Double-Click** - Running the exe without arguments now prompts to install context menu
- **UAC Elevation Support** - Application can auto-restart with admin privileges for installation
- **Performance Diagnostics** - New `HashFileWithDiagnostics()` method for detailed timing analysis
- **PerformanceDiagnostics Class** - Tracks timing per algorithm category (checksums, fast hashes, SHA, BLAKE, etc.)
- **Installation Status Check** - New `--status` command shows context menu installation status
- **Colored Console Output** - Success/error messages now use green/red/yellow coloring
- **Pretty Banner** - Application shows a nice ASCII banner when run without args
- **108 Unit Tests** - Expanded test coverage from 92 to 108 tests

### Changed
- **Double-click behavior** - Now prompts for context menu installation instead of showing help
- **Error messages** - More descriptive error messages with color coding
- **Code organization** - All source files reorganized with #regions and inline comments

### Technical
- All 4 source files fully documented (FileHasher.cs, FileHashResult.cs, ContextMenuInstaller.cs, Program.cs)
- Added ContextMenuInstaller.IsInstalledCorrectly() to detect when exe path changed
- Added ContextMenuInstaller.GetInstalledCommand() to retrieve registered command
- Batch file hashing
- Custom algorithm selection
