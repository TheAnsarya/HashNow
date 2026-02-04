# HashNow

**Right-click any file in Windows Explorer to instantly generate 70 different hashes to JSON.**

[![License: Unlicense](https://img.shields.io/badge/License-Unlicense-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-108%20passing-brightgreen)](tests/)
[![Release](https://img.shields.io/github/v/release/TheAnsarya/HashNow)](https://github.com/TheAnsarya/HashNow/releases/latest)

## 📥 Download

**[Download HashNow v1.2.0](https://github.com/TheAnsarya/HashNow/releases/latest)** - Windows single-file executable

## 🚀 Quick Start

1. **Download** `HashNow.exe` from [Releases](https://github.com/TheAnsarya/HashNow/releases/latest)
2. **Double-click** `HashNow.exe` - it will prompt to install the context menu
3. **Right-click any file** → Select **"Hash this file now"**
4. **Find** `{filename}.hashes.json` in the same folder

> **Tip:** If prompted by Windows SmartScreen, click "More info" → "Run anyway"

## Features

- **Instant Hashing** - Right-click any file and select "Hash this file now"
- **70 Algorithms** - Comprehensive coverage across 4 categories:
	- **Checksums** (9): CRC32, CRC32C, CRC64, CRC16 (3 variants), Adler32, Fletcher16, Fletcher32
	- **Fast Non-Crypto** (22): xxHash family, MurmurHash3, CityHash, FarmHash, SpookyHash, SipHash, HighwayHash, MetroHash, Wyhash, FNV-1a, DJB2, SDBM, LoseLose
	- **Cryptographic** (26): MD family, SHA family (0/1/2/3), BLAKE family, RIPEMD family
	- **Other Crypto** (14): Whirlpool, Tiger, SM3, GOST variants, Streebog, Skein, KangarooTwelve
- **Parallel Processing** - All 70 algorithms run concurrently for maximum speed
- **Single Pass** - Computes all hashes in one efficient file read
- **JSON Output** - Creates `{filename}.hashes.json` with tab indentation
- **Fast** - Streams large files with 1MB buffer and ArrayPool memory management
- **Progress Reporting** - Shows progress for large files (>3 seconds estimated)
- **Reusable Library** - HashNow.Core can be used in any .NET project
- **Powered by StreamHash** - Uses [StreamHash](https://www.nuget.org/packages/StreamHash) for all 70 algorithms
- **Public Domain** - The Unlicense, free for any use

## 📋 Usage

### Quickest Way: Double-Click

1. **Double-click** `HashNow.exe`
2. **Say yes** to install the context menu
3. **Done!** Right-click any file → "Hash this file now"

### Explorer Context Menu

1. Right-click any file
2. Select **"Hash this file now"**
3. Find the generated `{filename}.hashes.json` in the same folder

### Command Line

```powershell
# Double-click or run without args to auto-install
HashNow.exe

# Hash a single file
HashNow.exe myfile.zip

# Hash multiple files
HashNow.exe file1.iso file2.zip file3.bin

# Install context menu (requires admin)
HashNow.exe --install

# Uninstall context menu
HashNow.exe --uninstall

# Check installation status
HashNow.exe --status

# Show help
HashNow.exe --help

# Show version
HashNow.exe --version
```

## 📊 Output Format

The generated JSON file contains all 70 hashes organized by category:

```json
{
	"fileName": "example.zip",
	"fullPath": "C:\\Downloads\\example.zip",
	"sizeBytes": 1048576,
	"sizeFormatted": "1 MB",
	"createdUtc": "2025-02-03T10:30:00Z",
	"modifiedUtc": "2025-02-03T10:30:00Z",

	"crc32": "a1b2c3d4",
	"crc32C": "12345678",
	"crc64": "0123456789abcdef",
	"crc16Ccitt": "1234",
	"crc16Modbus": "5678",
	"crc16Usb": "9abc",
	"adler32": "abcd1234",
	"fletcher16": "1234",
	"fletcher32": "12345678",

	"xxHash32": "12345678",
	"xxHash3": "1234567890abcdef",
	"xxHash64": "fedcba0987654321",
	"xxHash128": "0123456789abcdef0123456789abcdef",
	"murmurHash3_32": "12345678",
	"murmurHash3_128": "0123456789abcdef0123456789abcdef",
	"cityHash64": "1234567890abcdef",
	"cityHash128": "0123456789abcdef0123456789abcdef",
	"farmHash64": "1234567890abcdef",
	"spookyV2_128": "0123456789abcdef0123456789abcdef",
	"sipHash24": "1234567890abcdef",
	"highwayHash64": "1234567890abcdef",
	"metroHash64": "1234567890abcdef",
	"metroHash128": "0123456789abcdef0123456789abcdef",
	"wyhash64": "1234567890abcdef",
	"fnv1a32": "12345678",
	"fnv1a64": "1234567890abcdef",
	"djb2": "12345678",
	"djb2a": "12345678",
	"sdbm": "12345678",
	"loseLose": "00001234",

	"md5": "d41d8cd98f00b204e9800998ecf8427e",
	"sha1": "da39a3ee5e6b4b0d3255bfef95601890afd80709",
	"sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
	"sha384": "38b060a751ac9638...",
	"sha512": "cf83e1357eefb8bdf1542850d66d8007...",
	"sha512_256": "c672b8d1ef56ed28...",
	"sha3_224": "6b4e03423667dbb73b6e15454f0eb1abd4597f9a1b078e3f5b5a6bc7",
	"sha3_256": "a7ffc6f8bf1ed76651c14756a061d662...",
	"sha3_384": "0c63a75b845e4f7d01107d852e4c2485...",
	"sha3_512": "a69f73cca23a9ac5c8b567dc185a756e...",
	"blake256": "0e5751c026e543b2e8ab2eb06099daa1...",
	"blake512": "786a02f742015903...",
	"blake2b": "786a02f742015903c6c6fd852552d272...",
	"blake2s": "69217a3079908094e11121d042354a7c...",
	"blake3": "af1349b9f5f9a1a6a0404dea36dcc949...",
	"ripemd128": "cdf26213a150dc3ecb610f18f6b38b46",
	"ripemd160": "9c1185a5c5e9fc54612808977ee8f548b2258d31",
	"ripemd256": "02ba4c4e5f8ecd1877fc52d64d30e37a...",
	"ripemd320": "22d65d5661536cdc75c1fdf5c6de7b41...",
	"whirlpool": "19fa61d75522a4669b44e39c1d2e1726...",
	"tiger192": "3293ac630c13f0245f92bbb1766e1616...",
	"sm3": "66c7f0f462eeedd9d1f2d46bdc10e4e24167c4875cf2f7a2297da02b8f4ba8e0",
	"gost94": "ce85b99cc46752fffee35cab9a7b0278...",
	"streebog256": "3f539a213e97c802cc229d474c6aa32a...",
	"streebog512": "b1c5648b78c1ce1d52f5a63f2f2a4d9b...",
	"skein256": "39ccc4554a8b31853b9de7a1fe638a24...",
	"skein512": "bc5b4c50925519c290cc634277ae3d6257212395...",
	"skein1024": "0fff9563bb3279289227ac77d319b6fff8d7e9f0...",
	"kangarooTwelve": "1ac2d450fc3b4205d19da7bfca1b37513c0803577ac7167f06fe2ce1f0ef39e5",

	"hashedAtUtc": "2025-02-03T10:30:15Z",
	"durationMs": 1003,
	"generatedBy": "HashNow 1.2.0",
	"algorithmCount": 70
}
```

## Hash Algorithms

### Checksums (9)
| Algorithm | Output | Notes |
|-----------|--------|-------|
| CRC32 | 4 bytes | Standard CRC-32 |
| CRC32C | 4 bytes | Castagnoli variant (SSE4.2 accelerated) |
| CRC64 | 8 bytes | CRC-64/ECMA-182 |
| CRC16-CCITT | 2 bytes | Polynomial 0x1021 |
| CRC16-MODBUS | 2 bytes | Industrial protocol |
| CRC16-USB | 2 bytes | USB protocol |
| Adler32 | 4 bytes | Fast checksum, used in zlib |
| Fletcher16 | 2 bytes | Simple checksum |
| Fletcher32 | 4 bytes | Simple checksum |

### Fast Non-Cryptographic (22)
| Algorithm | Output | Notes |
|-----------|--------|-------|
| xxHash32 | 4 bytes | Fast 32-bit |
| xxHash3 | 8 bytes | Extremely fast, SIMD optimized |
| xxHash64 | 8 bytes | Very fast 64-bit |
| xxHash128 | 16 bytes | Fast 128-bit |
| MurmurHash3-32 | 4 bytes | Popular fast hash |
| MurmurHash3-128 | 16 bytes | 128-bit variant |
| CityHash64 | 8 bytes | Google's fast hash |
| CityHash128 | 16 bytes | 128-bit variant |
| FarmHash64 | 8 bytes | Google's successor to CityHash |
| SpookyHash V2 | 16 bytes | Bob Jenkins' 128-bit hash |
| SipHash-2-4 | 8 bytes | DoS-resistant |
| HighwayHash64 | 8 bytes | Google's SIMD-accelerated hash |
| MetroHash64 | 8 bytes | Very fast 64-bit |
| MetroHash128 | 16 bytes | 128-bit variant |
| Wyhash64 | 8 bytes | One of the fastest hashes |
| FNV-1a 32 | 4 bytes | Simple, fast |
| FNV-1a 64 | 8 bytes | 64-bit variant |
| DJB2 | 4 bytes | Dan Bernstein's hash |
| DJB2a | 4 bytes | XOR variant |
| SDBM | 4 bytes | Database hash |
| LoseLose | 4 bytes | Simple byte sum |

### Cryptographic (28)
| Algorithm | Output | Notes |
|-----------|--------|-------|
| MD5 | 16 bytes | Legacy, fast |
| SHA1 | 20 bytes | Legacy |
| SHA256 | 32 bytes | **Recommended** |
| SHA384 | 48 bytes | SHA-2 family |
| SHA512 | 64 bytes | SHA-2 family |
| SHA512/256 | 32 bytes | Truncated SHA-512 |
| SHA3-256 | 32 bytes | Keccak-based |
| SHA3-384 | 48 bytes | Keccak-based |
| SHA3-512 | 64 bytes | Keccak-based |
| BLAKE2b-256 | 32 bytes | Fast, secure |
| BLAKE2b-384 | 48 bytes | Fast, secure |
| BLAKE2b-512 | 64 bytes | Fast, secure |
| BLAKE2s-128 | 16 bytes | Optimized for 32-bit |
| BLAKE2s-256 | 32 bytes | Optimized for 32-bit |
| RIPEMD-160 | 20 bytes | Bitcoin addresses |
| Whirlpool | 64 bytes | AES-based |
| Tiger | 24 bytes | Optimized for 64-bit |
| Tiger2 | 24 bytes | Tiger variant |
| SM3 | 32 bytes | Chinese standard |
| GOST 34.11 | 32 bytes | Russian standard |
| GOST 34.11-2012/256 | 32 bytes | Streebog-256 |
| GOST 34.11-2012/512 | 64 bytes | Streebog-512 |
| Streebog-256 | 32 bytes | GOST alias |
| Streebog-512 | 64 bytes | GOST alias |
| HAVAL-256-5 | 32 bytes | 5-pass variant |

### Other Crypto (11)
| Algorithm | Output | Notes |
|-----------|--------|-------|
| SHAKE128 | variable | XOF, 256-bit output |
| SHAKE256 | variable | XOF, 512-bit output |
| Keccak-224 | 28 bytes | Pre-SHA3 |
| Keccak-256 | 32 bytes | Ethereum |
| Keccak-384 | 48 bytes | Pre-SHA3 |
| Keccak-512 | 64 bytes | Pre-SHA3 |
| Skein-256 | 32 bytes | SHA-3 finalist |
| Skein-512 | 64 bytes | SHA-3 finalist |
| Skein-1024 | 128 bytes | Large state |
| Groestl-256 | 32 bytes | SHA-3 finalist |
| JH-256 | 32 bytes | SHA-3 finalist |

## Building from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows (for context menu features)

### Build

```bash
# Clone the repository
git clone https://github.com/TheAnsarya/HashNow.git
cd HashNow

# Build
dotnet build

# Run tests (108 tests)
dotnet test

# Publish self-contained executable
dotnet publish src/HashNow.Cli -c Release -r win-x64 --self-contained true -o publish
```

### Run Benchmarks

```bash
dotnet run -c Release --project benchmarks/HashNow.Benchmarks -- --filter "*"
```

## Project Structure

```
HashNow/
├── src/
│   ├── HashNow.Core/              # Core library (reusable in any project)
│   │   ├── FileHasher.cs          # 70 hash algorithm implementations
│   │   ├── FileHashResult.cs      # Result model with all 70 properties
│   │   └── PerformanceDiagnostics.cs  # Timing analysis by category
│   └── HashNow.Cli/               # Command-line interface
│       ├── Program.cs             # CLI entry point with auto-install
│       └── ContextMenuInstaller.cs  # Windows Explorer integration
├── tests/
│   └── HashNow.Core.Tests/        # 108 xUnit tests
├── benchmarks/
│   └── HashNow.Benchmarks/        # BenchmarkDotNet performance tests
├── docs/
│   └── ALGORITHM_ROADMAP.md       # Algorithm implementation status
└── publish/                       # Published executable
```

## Using the Core Library

The `HashNow.Core` library can be referenced in any .NET project:

```csharp
using HashNow.Core;

// Hash a file and get all 70 hashes
var result = FileHasher.HashFile("myfile.zip");
Console.WriteLine($"SHA256: {result.Sha256}");
Console.WriteLine($"BLAKE3: {result.Blake3}");

// Or compute individual hashes
string sha256 = FileHasher.ComputeSha256(fileBytes);
string blake3 = FileHasher.ComputeBlake3(fileBytes);
byte[] sha256Bytes = FileHasher.GetSha256Bytes(fileBytes);

// Save results to JSON
FileHasher.SaveResult(result, "myfile.zip.hashes.json");
```

## Performance

All 70 hashes are computed in a **single file read** for maximum efficiency:

- **1 MB buffer** reduces system calls
- **ArrayPool** minimizes GC pressure
- **Streaming** handles files of any size
- **Single pass** - no multiple file reads
- **StreamHash** provides streaming implementations for non-crypto hashes
- **BouncyCastle** provides optimized crypto implementations

Typical throughput: ~300-500 MB/s depending on disk speed.
70 algorithms on a 5KB file: ~1000ms (most time in algorithm initialization).

## License

[The Unlicense](LICENSE) - Public domain, free for any use.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for full history.

### v1.2.0 (2026-02-05)
- **70 hash algorithms** - Up from 58, powered by StreamHash 1.6.3
- **New fast hashes** - MetroHash64/128, Wyhash64, FNV-1a 32/64, DJB2, DJB2a, SDBM, LoseLose
- **New CRC16 variants** - CRC16-CCITT, CRC16-MODBUS, CRC16-USB
- **StreamHash integration** - Streaming implementations for all non-crypto hashes

### v1.0.2 (2026-02-03)
- **Custom application icon** - Blue button with white hash symbol (#)
- **Blank lines between JSON sections** - Improved readability
- **Trailing newline** - JSON files end with blank line

### v1.0.1 (2026-02-03)
- **Auto-install on double-click** - Just double-click to install context menu (prompts for UAC)
- **`--status` command** - Check if context menu is installed correctly
- **Performance diagnostics** - New `PerformanceDiagnostics` class for timing analysis
- **Comprehensive XML documentation** - All public and private members documented
- **Colored console output** - Better visual feedback during installation
- **108 unit tests** (up from 92)

### v1.0.0 (2026-02-02)
- Initial release with 58 hash algorithms
- Windows Explorer context menu integration
- Parallel hash computation
- JSON output with tab indentation
