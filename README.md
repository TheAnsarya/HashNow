# HashNow

**Right-click any file in Windows Explorer to instantly generate 13 different hashes to JSON.**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-31%20passing-brightgreen)](tests/)

## Features

- **Instant Hashing** - Right-click any file and select "Hash this file now"
- **13 Algorithms** - CRC32, CRC64, MD5, SHA1, SHA256, SHA384, SHA512, SHA3-256/384/512*, XXHash3, XXHash64, XXHash128
- **Single Pass** - Computes all hashes in one efficient file read
- **JSON Output** - Creates `{filename}.hashes.json` with all hashes and metadata
- **Fast** - Streams large files with 1MB buffer and ArrayPool memory management
- **Progress Reporting** - Shows progress for large files (>3 seconds estimated)
- **Portable** - Single-file executable, no dependencies

*SHA3 requires Windows 11 24H2+ or OpenSSL 1.1.1+

## Installation

### Download

Download the latest release from [Releases](https://github.com/TheAnsarya/HashNow/releases).

### Install Context Menu

1. Run **as Administrator**:
   ```
   HashNow.exe --install
   ```
2. Right-click any file in Explorer
3. Select **"Hash this file now"**

### Uninstall Context Menu

```
HashNow.exe --uninstall
```

## Usage

### From Explorer (Recommended)

1. Right-click any file
2. Select **"Hash this file now"**
3. Find the generated `{filename}.hashes.json` in the same folder

### From Command Line

```bash
# Hash a single file
HashNow.exe myfile.zip

# Hash multiple files
HashNow.exe file1.iso file2.zip file3.bin

# Show help
HashNow.exe --help

# Show version
HashNow.exe --version
```

## Output Format

The generated JSON file contains all 13 hashes:

```json
{
  "fileName": "example.zip",
  "fullPath": "C:\\Downloads\\example.zip",
  "sizeBytes": 1048576,
  "sizeFormatted": "1 MB",
  "createdUtc": "2025-02-03T10:30:00.0000000Z",
  "modifiedUtc": "2025-02-03T10:30:00.0000000Z",
  "crc32": "a1b2c3d4",
  "crc64": "0123456789abcdef",
  "md5": "d41d8cd98f00b204e9800998ecf8427e",
  "sha1": "da39a3ee5e6b4b0d3255bfef95601890afd80709",
  "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
  "sha384": "38b060a751ac9638...",
  "sha512": "cf83e1357eefb8bdf1542850d66d8007...",
  "sha3_256": "a7ffc6f8bf1ed76651c14756a061d662...",
  "sha3_384": "0c63a75b845e4f7d01107d852e4c2485...",
  "sha3_512": "a69f73cca23a9ac5c8b567dc185a756e...",
  "xxHash3": "1234567890abcdef",
  "xxHash64": "fedcba0987654321",
  "xxHash128": "0123456789abcdef0123456789abcdef",
  "hashedAtUtc": "2025-02-03T10:30:15.0000000Z",
  "durationMs": 523
}
```

## Hash Algorithms

| Algorithm | Output Size | Type | Notes |
|-----------|-------------|------|-------|
| CRC32 | 4 bytes | Checksum | Standard CRC-32 |
| CRC64 | 8 bytes | Checksum | CRC-64/ECMA-182 |
| MD5 | 16 bytes | Crypto | Legacy, fast |
| SHA1 | 20 bytes | Crypto | Legacy |
| SHA256 | 32 bytes | Crypto | **Recommended** |
| SHA384 | 48 bytes | Crypto | SHA-2 family |
| SHA512 | 64 bytes | Crypto | SHA-2 family |
| SHA3-256 | 32 bytes | Crypto | Keccak-based* |
| SHA3-384 | 48 bytes | Crypto | Keccak-based* |
| SHA3-512 | 64 bytes | Crypto | Keccak-based* |
| XXHash3 | 8 bytes | Fast | Extremely fast |
| XXHash64 | 8 bytes | Fast | Very fast |
| XXHash128 | 16 bytes | Fast | Fast, larger output |

*SHA3 requires Windows 11 24H2+ or OpenSSL 1.1.1+

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

# Run tests (31 tests)
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
│   ├── HashNow.Core/         # Core library (FileHasher, FileHashResult)
│   └── HashNow.Cli/          # Command-line interface
├── tests/
│   └── HashNow.Core.Tests/   # 31 xUnit tests
├── benchmarks/
│   └── HashNow.Benchmarks/   # BenchmarkDotNet performance tests
└── publish/                  # Published executable
```

## Performance

All 13 hashes are computed in a **single file read** for maximum efficiency:

- **1 MB buffer** reduces system calls
- **ArrayPool** minimizes GC pressure
- **Streaming** handles files of any size
- **Single pass** - no multiple file reads

Typical throughput: ~300-500 MB/s depending on disk speed.

## License

MIT License - see [LICENSE](LICENSE) for details.

## Changelog

### v1.1.0
- Added 13 hash algorithms (up from 5)
- Added CRC64, SHA384, SHA3-256/384/512, XXHash3/64/128
- Single-pass computation for all hashes
- Progress reporting for large files
- Duration tracking (durationMs in output)
- Platform detection for SHA3 support
- 31 unit tests with comprehensive coverage
- BenchmarkDotNet benchmarks

### v1.0.0
- Initial release
- CRC32, MD5, SHA1, SHA256, SHA512
- Windows Explorer context menu integration
- JSON output format
