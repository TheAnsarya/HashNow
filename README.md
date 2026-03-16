# HashNow

**Right-click any file in Windows Explorer to instantly generate 70 different hashes to JSON.**

[![License: Unlicense](https://img.shields.io/badge/License-Unlicense-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-296%20passing-brightgreen)](tests/)
[![Release](https://img.shields.io/github/v/release/TheAnsarya/HashNow)](https://github.com/TheAnsarya/HashNow/releases/latest)

## 📥 Download

**[Download HashNow v1.4.0](https://github.com/TheAnsarya/HashNow/releases/latest)** - Windows single-file executable

## 🚀 Quick Start

1. **Download** `HashNow.exe` from [Releases](https://github.com/TheAnsarya/HashNow/releases/latest)
2. **Double-click** `HashNow.exe` - it will prompt to install the context menu
3. **Right-click any file** → Select **"Hash this file now"**
4. **Find** `{filename}.hashes.json` in the same folder

> **Tip:** If prompted by Windows SmartScreen, click "More info" → "Run anyway"

## ✨ Features

- **70 Algorithms** in 4 categories (checksums, fast non-crypto, cryptographic, other crypto)
- **Single Pass** — all 70 hashes computed in one file read
- **Parallel Processing** — all algorithms run concurrently
- **JSON Output** — tab-indented, organized by category with metadata
- **Progress Dialog** — shows progress bar with percentage, closes automatically
- **Cancellation** — cancel button stops hashing immediately
- **Reusable Library** — `HashNow.Core` can be used in any .NET project
- **Powered by [StreamHash](https://www.nuget.org/packages/StreamHash)** — all 70 algorithms in pure native C#
- **Public Domain** — [The Unlicense](LICENSE), free for any use

## 📋 How to Use

### Auto-Install (Easiest)

1. **Double-click** `HashNow.exe`
2. Windows may show a SmartScreen prompt — click **"More info"** → **"Run anyway"**
3. HashNow detects it was launched without arguments and offers to install the context menu
4. Click **Yes** when prompted for administrator privileges (required for registry access)
5. A confirmation dialog appears — you're done!

Now right-click any file in Explorer → **"Hash this file now"**

### Explorer Context Menu

After installation, the context menu appears on every file type:

1. **Right-click** any file in Windows Explorer
2. Select **"Hash this file now"**
3. A progress dialog appears showing the hashing progress
4. When complete, find `{filename}.hashes.json` in the same folder as the original file

The progress dialog shows:

- File name being hashed
- Progress bar (0–100%)
- Percentage complete
- **Cancel** button to abort at any time

### Command Line

```powershell
# Hash a single file
HashNow.exe myfile.zip

# Hash multiple files (each gets its own .hashes.json)
HashNow.exe file1.iso file2.zip file3.bin

# Install context menu (requires admin)
HashNow.exe --install

# Uninstall context menu
HashNow.exe --uninstall

# Check if context menu is installed
HashNow.exe --status

# Show help
HashNow.exe --help

# Show version
HashNow.exe --version
```

### Context Menu Management

| Command | Description | Requires Admin |
|---------|-------------|:--------------:|
| `--install` | Register "Hash this file now" in Explorer | Yes |
| `--uninstall` | Remove context menu entry | Yes |
| `--status` | Check if context menu is correctly installed | No |

The context menu entry is stored in the Windows registry under `HKEY_CLASSES_ROOT\*\shell\HashNow` and applies to all file types.

## 📊 Output Format

HashNow creates `{filename}.hashes.json` next to the original file. The JSON uses **tab indentation** with **blank lines between sections** for readability:

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
	...

	"md5": "d41d8cd98f00b204e9800998ecf8427e",
	"sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
	"blake3": "af1349b9f5f9a1a6a0404dea36dcc949...",
	...

	"hashedAtUtc": "2025-02-05T10:30:15Z",
	"durationMs": 1003,
	"generatedBy": "HashNow v1.4.0",
	"algorithmCount": 70
}
```

**Output details:**

- All hash values in **lowercase hexadecimal**
- File metadata at top (name, path, size, timestamps)
- Hashes organized by category: checksums → fast → crypto → other
- Timing and version metadata at bottom

## 🔐 Hash Algorithms (70)

| Category | Count | Algorithms |
|----------|:-----:|------------|
| **Checksums** | 9 | CRC32, CRC32C, CRC64, CRC16 (CCITT/MODBUS/USB), Adler32, Fletcher16, Fletcher32 |
| **Fast Non-Crypto** | 22 | xxHash (32/64/3/128), MurmurHash3 (32/128), CityHash (64/128), FarmHash64, SpookyV2, SipHash, HighwayHash64, MetroHash (64/128), Wyhash64, FNV-1a (32/64), DJB2, DJB2a, SDBM, LoseLose |
| **Cryptographic** | 28 | MD5, SHA-1/256/384/512, SHA-512/256, SHA3 (256/384/512), BLAKE2b/2s, BLAKE3, RIPEMD (128/160/256/320), Whirlpool, Tiger, SM3, GOST, Streebog, HAVAL |
| **Other Crypto** | 11 | SHAKE128/256, Keccak (224/256/384/512), Skein (256/512/1024), Groestl-256, JH-256 |

For the full algorithm list with output sizes and notes, see [Algorithm Roadmap](docs/ALGORITHM_ROADMAP.md).

## 📚 Using the Core Library

The `HashNow.Core` NuGet package can be used in any .NET project:

```csharp
using HashNow.Core;

// Hash a file and get all 70 hashes
var result = FileHasher.HashFile("myfile.zip");
Console.WriteLine($"SHA256: {result.Sha256}");
Console.WriteLine($"BLAKE3: {result.Blake3}");

// Save results to JSON
FileHasher.SaveResult(result, "myfile.zip.hashes.json");
```

## 🏗️ Building from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows (for context menu features)

### Build

```bash
git clone https://github.com/TheAnsarya/HashNow.git
cd HashNow
dotnet build
dotnet test
```

### Publish

```bash
dotnet publish src/HashNow.Cli -c Release -r win-x64 --self-contained true -o publish
```

## ⚡ Performance

All 70 hashes are computed in a **single file read** with parallel processing:

- **1 MB buffer** with ArrayPool memory management
- **Single pass** — file is read once, all algorithms fed concurrently
- **Powered by [StreamHash](https://www.nuget.org/packages/StreamHash)** — native C# with zero unsafe code

For detailed benchmark results, see [Performance](docs/PERFORMANCE.md). For per-algorithm data, see [StreamHash Benchmarks](https://github.com/TheAnsarya/StreamHash/blob/main/docs/benchmarks.md).

## 📖 Documentation

- [📝 Changelog](CHANGELOG.md) — version history and release notes
- [📊 Algorithm Roadmap](docs/ALGORITHM_ROADMAP.md) — all 70 algorithms with implementation status
- [⚡ Performance](docs/PERFORMANCE.md) — benchmarks, architecture, and optimization details
- [🧪 Manual Testing Guide](docs/MANUAL_TESTING.md) — step-by-step testing procedures
- [📈 StreamHash Benchmarks](https://github.com/TheAnsarya/StreamHash/blob/main/docs/benchmarks.md) — per-algorithm performance data

## 📄 License

[The Unlicense](LICENSE) — Public domain, free for any use.
