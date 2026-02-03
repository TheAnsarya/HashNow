# HashNow v1.0.2 Release Notes

**Release Date:** February 3, 2026

## 🎉 Highlights

This release improves **visual appearance** and **JSON readability**:

- **Custom icon** - Bright blue button with white hash symbol (#)
- **Better JSON formatting** - Blank lines between sections for easier reading

## ✨ New Features

### Custom Application Icon
HashNow now has a distinctive icon - a bright blue rounded button with a white hash symbol (#). This icon appears:
- On the executable file
- In the Windows Explorer context menu
- In the taskbar when running

### Improved JSON Output Format
The generated `.hashes.json` files now include blank lines between logical sections:

```json
{
	"fileName": "example.zip",
	"fullPath": "C:\\Downloads\\example.zip",
	"sizeBytes": 1048576,
	"sizeFormatted": "1 MB",
	"createdUtc": "2026-02-03T10:30:00Z",
	"modifiedUtc": "2026-02-03T10:30:00Z",

	"crc32": "a1b2c3d4",
	"crc32c": "12345678",
	...

	"xxHash32": "abcd1234",
	"xxHash64": "1234567890abcdef",
	...

	"md5": "d41d8cd98f00b204e9800998ecf8427e",
	"sha256": "e3b0c44298fc1c149afbf4c8996fb924...",
	...

	"whirlpool": "19fa61d75522a4669b44e39c1d2e1726...",
	...

	"hashedAtUtc": "2026-02-03T10:30:15Z",
	"durationMs": 1003,
	"generatedBy": "HashNow v1.0.2",
	"algorithmCount": 58
}
```

### Trailing Newline
JSON files now end with a blank line for better compatibility with text editors and version control systems.

## 📦 Download

- [HashNow.exe](https://github.com/TheAnsarya/HashNow/releases/download/v1.0.2/HashNow.exe) - Single-file Windows executable

## 🚀 Quick Start

1. Download `HashNow.exe`
2. Double-click it
3. Say "Yes" to install the context menu
4. Right-click any file → "Hash this file now"

## 📋 Full Changelog

See [CHANGELOG.md](CHANGELOG.md) for complete details.

---

**License:** [The Unlicense](LICENSE) (Public Domain)
