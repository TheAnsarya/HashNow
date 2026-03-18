# 🔐 Introducing HashNow — Hash Any File in One Click (70 Algorithms!)

Hey everyone! I just released **HashNow v1.5.1** — a free, open-source file hashing utility that computes **70 hash algorithms simultaneously** in a single file read. Right-click any file → get a JSON file with all hashes instantly.

## What makes it awesome?

🚀 **70 algorithms at once** — all computed in **one pass** over the file with parallel processing

⚡ **Fast** — SIMD acceleration, 1MB streaming I/O, ArrayPool memory management

📁 **Right-click integration** — adds "Hash with HashNow" to your file manager's context menu:
- **Windows**: Explorer context menu
- **Linux**: Nautilus, Nemo, Dolphin, Thunar
- **macOS**: Finder Quick Action

🎯 **Single self-contained binary** — no .NET runtime install needed, just download and go

📋 **JSON output** — clean, structured output file right next to your original file

## All 70 Hash Algorithms

**Checksums & CRCs (9)**
CRC32, CRC32C, CRC64, CRC16-CCITT, CRC16-MODBUS, CRC16-USB, Adler-32, Fletcher-16, Fletcher-32

**Non-Crypto Fast Hashes (21)**
xxHash32, xxHash64, xxHash3, xxHash128, MurmurHash3-32, MurmurHash3-128, CityHash64, CityHash128, FarmHash64, SpookyHash V2, SipHash-2-4, HighwayHash64, MetroHash64, MetroHash128, wyhash64, FNV-1a-32, FNV-1a-64, DJB2, DJB2a, SDBM, Lose Lose

**Cryptographic Hashes (26)**
MD2, MD4, MD5, SHA-0, SHA-1, SHA-224, SHA-256, SHA-384, SHA-512, SHA-512/224, SHA-512/256, SHA3-224, SHA3-256, SHA3-384, SHA3-512, Keccak-256, Keccak-512, BLAKE-256, BLAKE-512, BLAKE2b, BLAKE2s, BLAKE3, RIPEMD-128, RIPEMD-160, RIPEMD-256, RIPEMD-320

**Other Crypto Hashes (14)**
Whirlpool, Tiger-192, GOST R 34.11-94, Streebog-256, Streebog-512, Skein-256, Skein-512, Skein-1024, Grøstl-256, Grøstl-512, JH-256, JH-512, KangarooTwelve, SM3

## Downloads

**v1.5.1 Release**: <https://github.com/TheAnsarya/HashNow/releases/tag/v1.5.1>

| Platform | Download | Size |
|----------|----------|------|
| Windows x64 | `HashNow-Windows-x64-v1.5.1.exe` | ~50 MB |
| Linux x64 | `HashNow-Linux-x64-v1.5.1.tar.gz` | ~31 MB |
| Linux ARM64 | `HashNow-Linux-ARM64-v1.5.1.tar.gz` | ~29 MB |
| macOS ARM64 (Apple Silicon) | `HashNow-macOS-ARM64-v1.5.1.tar.gz` | ~29 MB |

## Powered by StreamHash

All 70 hash algorithms are implemented in **StreamHash** — a pure C# streaming hash library with SIMD acceleration. If you're a .NET developer, you can use it in your own projects:

```
dotnet add package StreamHash --version 1.11.2
```

🔗 **NuGet**: <https://www.nuget.org/packages/StreamHash>
🔗 **GitHub**: <https://github.com/TheAnsarya/StreamHash>

## 🙏 Calling All macOS and Linux Users — I Need Your Help!

I only have Windows, so I can't test the Linux and macOS builds myself. If you're running **macOS** or **Linux**, I'd really appreciate it if you could:

1. **Download** the release for your platform from the link above
2. **Try installing it** — extract the archive, run the install script (`install.sh` on Linux or `Install HashNow.command` on macOS)
3. **Try hashing a file** — right-click a file in your file manager and look for the HashNow option
4. **Take screenshots!** 📸 — Screenshots of the install process, the context menu integration, and the hash output would be incredibly helpful for documentation
5. **Report any issues** — if something doesn't work, let me know or open an issue on GitHub: <https://github.com/TheAnsarya/HashNow/issues>

Any feedback, screenshots, or bug reports would be hugely appreciated. I want to make sure this works great on all platforms but I need real-world testing from Mac and Linux users.

Both projects are **public domain** (The Unlicense) — free to use, modify, and share however you want.

Thanks! 🎉
