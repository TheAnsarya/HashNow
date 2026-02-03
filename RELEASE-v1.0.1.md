# HashNow v1.0.1 Release Notes

**Release Date:** February 3, 2026

## 🎉 Highlights

This release focuses on **ease of use** and **developer experience**:

- **Just double-click to install!** No more PowerShell commands needed
- **Comprehensive documentation** - Every class, method, and property is now documented
- **Performance diagnostics** - Detailed timing breakdowns by algorithm category

## ✨ New Features

### Auto-Install on Double-Click
Simply double-click `HashNow.exe` and it will:
1. Check if the context menu is already installed
2. Prompt you to install if it's missing
3. Automatically request administrator privileges (UAC)

No more running PowerShell commands - just double-click and go!

### Installation Status (`--status`)
New command to check your installation:
```
HashNow.exe --status
```
Shows whether the context menu is installed and if the registered path is correct.

### Performance Diagnostics
New `PerformanceDiagnostics` class for detailed timing analysis:
```csharp
var result = FileHasher.HashFileWithDiagnostics("largefile.iso");
Console.WriteLine(result.Diagnostics.ToReport());
```

Output shows timing breakdown by category:
- Checksums (CRC32, Adler32, etc.)
- Fast Non-Crypto (xxHash, MurmurHash, etc.)
- SHA Family
- BLAKE Family
- And more...

### Comprehensive XML Documentation
Every public and private member now has detailed XML documentation including:
- `<summary>` - What it does
- `<remarks>` - Implementation details
- `<example>` - Usage examples
- `<param>` / `<returns>` - Parameter and return documentation

### Enhanced Console Output
- **Colored messages** - Green for success, yellow for warnings, red for errors
- **ASCII banner** - Friendly welcome when running without arguments
- **Progress indicators** - Better feedback during operations

## 📊 Stats

- **108 unit tests** (up from 92)
- **3,343 lines of documentation added**
- **4 source files fully documented**

## 📦 Download

- [HashNow.exe](https://github.com/TheAnsarya/HashNow/releases/download/v1.0.1/HashNow.exe) - Single-file Windows executable

## 🚀 Quick Start

1. Download `HashNow.exe`
2. Double-click it
3. Say "Yes" to install the context menu
4. Right-click any file → "Hash this file now"

## 📋 Full Changelog

See [CHANGELOG.md](CHANGELOG.md) for complete details.

---

**License:** [The Unlicense](LICENSE) (Public Domain)
