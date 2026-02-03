# HashNow

**Right-click any file in Windows Explorer to instantly generate CRC32, MD5, SHA1, SHA256, and SHA512 hashes to JSON.**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)

## Features

- **Instant Hashing** - Right-click any file and select "Hash this file now"
- **Multiple Algorithms** - CRC32, MD5, SHA1, SHA256, SHA512
- **JSON Output** - Creates `{filename}.hashes.json` with all hashes and metadata
- **Fast** - Streams large files efficiently with 1MB buffer
- **Portable** - Single-file executable, no dependencies

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

The generated JSON file contains:

```json
{
  "fileName": "example.zip",
  "fullPath": "C:\\Downloads\\example.zip",
  "sizeBytes": 1048576,
  "sizeFormatted": "1 MB",
  "createdUtc": "2025-02-03T10:30:00.0000000Z",
  "modifiedUtc": "2025-02-03T10:30:00.0000000Z",
  "crc32": "a1b2c3d4",
  "md5": "d41d8cd98f00b204e9800998ecf8427e",
  "sha1": "da39a3ee5e6b4b0d3255bfef95601890afd80709",
  "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
  "sha512": "cf83e1357eefb8bdf1542850d66d8007...",
  "hashedAtUtc": "2025-02-03T10:30:15.0000000Z",
  "generatedBy": "HashNow v1.0.0"
}
```

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

# Run tests
dotnet test

# Publish single-file executable
dotnet publish src/HashNow.Cli/HashNow.Cli.csproj -c Release -r win-x64 --self-contained
```

The published executable will be in `src/HashNow.Cli/bin/Release/net10.0-windows/win-x64/publish/`.

## Project Structure

```
HashNow/
├── src/
│   ├── HashNow.Core/       # Core hashing library
│   │   ├── FileHasher.cs   # Main hashing logic
│   │   └── FileHashResult.cs # Result model
│   └── HashNow.Cli/        # CLI application
│       ├── Program.cs      # Entry point
│       └── ContextMenuInstaller.cs # Registry integration
├── docs/                   # Documentation
└── ~docs/                  # Development documentation
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

Created by [TheAnsarya](https://github.com/TheAnsarya)
