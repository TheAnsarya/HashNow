# Changelog

All notable changes to HashNow will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.3] - 2026-02-05

### Fixed
- Fixed silent failure when double-clicking exe in Windows Explorer
- Fixed `DetectDoubleClickLaunch()` method throwing IOException when no console attached
- Improved parent process detection to run before console checks
- Changed default behavior to assume GUI mode when detection fails

## [1.3.2] - 2026-02-04

### Fixed
- **No Console Windows** - Changed to WinExe to suppress console when launched from Explorer
- Double-click installation now shows only GUI dialogs (MessageBox)
- Context menu progress now shows only GUI progress dialog
- Console still works when invoked from command line

## [1.3.1] - 2026-02-04

### Changed
- Documentation improvements
- Session and chat logs added for development history

## [1.3.0] - 2026-02-05

### Changed
- **HashFacade Refactoring** - All 70 algorithms now use StreamHash's unified HashFacade API
- **Simplified Dependencies** - Removed direct BouncyCastle, Blake3, and Blake2Fast dependencies
- **Single Dependency** - Only depends on StreamHash 1.6.3 (which transitively includes all hash libraries)
- **Cleaner Codebase** - FileHasher and StreamingHasher completely rewritten using HashFacade
- **Unified API** - All compute methods delegate to `HashFacade.ComputeHashHex()` and `HashFacade.CreateStreaming()`

### Technical
- Removed: `BouncyCastle.Cryptography`, `Blake3`, `SauceControl.Blake2Fast`
- FileHasher: Now ~500 lines (was 1800+) - all methods delegate to HashFacade
- StreamingHasher: Now ~330 lines (was 600+) - uses HashFacade.CreateStreaming()
- All 108 tests pass

## [1.2.0] - 2026-02-05

### Added
- **13 New Hash Algorithms** - Expanded from 58 to 70 total algorithms
- **StreamHash Integration** - Now uses StreamHash 1.6.3 for streaming implementations
- **New CRC16 Variants**:
	- CRC16-CCITT (Polynomial 0x1021)
	- CRC16-MODBUS (Industrial protocol)
	- CRC16-USB (USB protocol)
- **New Fast Hash Algorithms**:
	- MetroHash64 (Very fast 64-bit)
	- MetroHash128 (128-bit variant)
	- Wyhash64 (One of the fastest hashes)
	- FNV-1a 32/64 (Simple, fast)
	- DJB2 (Dan Bernstein's hash)
	- DJB2a (XOR variant)
	- SDBM (Database hash)
	- LoseLose (Simple byte sum)

### Changed
- **Streaming Architecture** - All non-crypto hashes now use StreamHash streaming implementations
- **Updated Dependencies** - StreamHash 1.6.3 replaces individual hash library implementations

## [1.0.3] - 2026-02-03

### Added
- **GUI Progress Dialog** - Visual progress bar for large files (>3 seconds) when launched from context menu
- **Console Progress Bar** - Beautiful text-based progress bar with color for CLI usage
- **GUI Install Dialogs** - MessageBox dialogs for double-click installation prompts
- **Parent Process Detection** - Distinguishes between double-click and command-line launch

### Changed
- **Improved Icon** - Larger hash symbol (#) with better anti-aliasing on diagonal strokes
- **Smarter UI Mode Selection** - Uses GUI dialogs when double-clicked, console prompts when run from terminal

### Technical
- Added Windows Forms support for progress dialog
- New `ProgressDialog.cs` - Modal dialog with progress bar and cancel button
- New `ConsoleProgressBar.cs` - Text-based progress bar with Unicode block characters
- New `GuiDialogs.cs` - MessageBox wrapper for installation prompts
- Added System.Management for parent process detection

## [1.0.2] - 2026-02-03

### Added
- **Custom Application Icon** - Blue button with white hash symbol (#) for exe and context menu
- **Blank Lines Between JSON Sections** - Improved readability with visual separation between:
	- File Metadata
	- Checksums & CRCs
	- Non-Crypto Fast Hashes
	- Cryptographic Hashes
	- Other Crypto Hashes
	- Hashing Metadata
- **Trailing Newline** - JSON files now end with a blank line for better compatibility

### Changed
- **JSON Output Format** - Now includes blank lines between logical sections for easier reading

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
