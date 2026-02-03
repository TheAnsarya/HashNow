# HashNow Manual Testing Guide

This guide provides step-by-step manual testing procedures for HashNow v1.0.0 release verification.

## Prerequisites

- [ ] Windows 10/11 machine
- [ ] .NET 10 SDK installed
- [ ] Administrator access (for context menu tests)
- [ ] Test files prepared (see below)

## Test File Preparation

Create these test files in a `test-files` folder:

```powershell
# Create test directory
mkdir C:\HashNow-Tests
cd C:\HashNow-Tests

# Empty file (0 bytes)
New-Item -ItemType File -Name "empty.bin" -Force

# Single byte files
[byte[]]@(0x00) | Set-Content -Path "zero.bin" -AsByteStream
[byte[]]@(0xFF) | Set-Content -Path "ff.bin" -AsByteStream
[byte[]]@(0x42) | Set-Content -Path "0x42.bin" -AsByteStream

# Known text file
"Hello, World!" | Out-File -FilePath "hello.txt" -NoNewline -Encoding ASCII

# All bytes file (256 bytes)
[byte[]](0..255) | Set-Content -Path "allbytes.bin" -AsByteStream

# 1 KB random file
$rand = New-Object byte[] 1024; (New-Object Random).NextBytes($rand)
[IO.File]::WriteAllBytes("C:\HashNow-Tests\random-1kb.bin", $rand)

# 1 MB random file
$rand = New-Object byte[] (1024*1024); (New-Object Random).NextBytes($rand)
[IO.File]::WriteAllBytes("C:\HashNow-Tests\random-1mb.bin", $rand)

# 10 MB random file
$rand = New-Object byte[] (10*1024*1024); (New-Object Random).NextBytes($rand)
[IO.File]::WriteAllBytes("C:\HashNow-Tests\random-10mb.bin", $rand)
```

---

## Test Categories

### 1. CLI Basic Functionality

#### 1.1 Version Check
- [ ] Run: `HashNow --version`
- [ ] Expected: `HashNow v2.0.0` (or current version)
- [ ] Status: ___

#### 1.2 Help Display
- [ ] Run: `HashNow --help`
- [ ] Expected: Shows usage with all 58 algorithms listed
- [ ] Status: ___

#### 1.3 No Arguments
- [ ] Run: `HashNow`
- [ ] Expected: Shows usage/help
- [ ] Status: ___

---

### 2. File Hashing Tests

#### 2.1 Empty File
- [ ] Run: `HashNow empty.bin`
- [ ] Verify: `empty.bin.hashes.json` created
- [ ] Verify: JSON uses TAB indentation (not spaces)
- [ ] Verify: `sizeBytes` = 0
- [ ] Verify: `crc32` = `00000000`
- [ ] Verify: `md5` = `d41d8cd98f00b204e9800998ecf8427e`
- [ ] Status: ___

#### 2.2 Known Text (Hello, World!)
- [ ] Run: `HashNow hello.txt`
- [ ] Verify: `md5` = `65a8e27d8879283831b664bd8b7f0ad4`
- [ ] Verify: `sha256` = `dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f`
- [ ] Status: ___

#### 2.3 Single Byte (0x42)
- [ ] Run: `HashNow 0x42.bin`
- [ ] Verify: All 58 hashes present in JSON
- [ ] Verify: `sizeBytes` = 1
- [ ] Status: ___

#### 2.4 Large File (10 MB)
- [ ] Run: `HashNow random-10mb.bin`
- [ ] Verify: Completes within reasonable time (<5 seconds)
- [ ] Verify: Progress shown in console
- [ ] Verify: `durationMs` populated
- [ ] Status: ___

#### 2.5 Multiple Files
- [ ] Run: `HashNow empty.bin hello.txt 0x42.bin`
- [ ] Verify: Three `.hashes.json` files created
- [ ] Status: ___

---

### 3. JSON Output Verification

#### 3.1 Tab Indentation
- [ ] Open any `.hashes.json` in text editor
- [ ] Verify: Indentation uses TAB characters (0x09), NOT spaces
- [ ] Status: ___

#### 3.2 Required Fields Present
- [ ] `fileName` - File name without path
- [ ] `fullPath` - Absolute path
- [ ] `sizeBytes` - Numeric file size
- [ ] `sizeFormatted` - Human-readable size (e.g., "1 MB")
- [ ] `createdUtc` - ISO 8601 timestamp
- [ ] `modifiedUtc` - ISO 8601 timestamp
- [ ] `hashedAtUtc` - ISO 8601 timestamp
- [ ] `durationMs` - Processing time in milliseconds
- [ ] `generatedBy` - "HashNow vX.Y.Z"
- [ ] `algorithmCount` - 58
- [ ] Status: ___

#### 3.3 All Hash Algorithms Present (58 total)
Checksums (6):
- [ ] `crc32`, `crc32c`, `crc64`, `adler32`, `fletcher16`, `fletcher32`

Fast Non-Crypto (12):
- [ ] `xxHash32`, `xxHash64`, `xxHash3`, `xxHash128`
- [ ] `murmur3_32`, `murmur3_128`
- [ ] `cityHash64`, `cityHash128`
- [ ] `farmHash64`, `spookyV2_128`, `sipHash24`, `highwayHash64`

Cryptographic (26):
- [ ] `md2`, `md4`, `md5`
- [ ] `sha0`, `sha1`, `sha224`, `sha256`, `sha384`, `sha512`
- [ ] `sha512_224`, `sha512_256`
- [ ] `sha3_224`, `sha3_256`, `sha3_384`, `sha3_512`
- [ ] `keccak256`, `keccak512`
- [ ] `blake256`, `blake512`, `blake2b`, `blake2s`, `blake3`
- [ ] `ripemd128`, `ripemd160`, `ripemd256`, `ripemd320`

Other Crypto (14):
- [ ] `whirlpool`, `tiger192`, `gost94`
- [ ] `streebog256`, `streebog512`
- [ ] `skein256`, `skein512`, `skein1024`
- [ ] `groestl256`, `groestl512`, `jh256`, `jh512`
- [ ] `kangarooTwelve`, `sm3`

- [ ] Status: ___

#### 3.4 Lowercase Hex
- [ ] All hash values use lowercase hex (a-f, not A-F)
- [ ] Status: ___

---

### 4. Context Menu Tests (Requires Admin)

#### 4.1 Install Context Menu
- [ ] Run as Admin: `HashNow --install`
- [ ] Expected: "Context menu installed successfully!"
- [ ] Status: ___

#### 4.2 Verify Context Menu Exists
- [ ] Right-click any file in Windows Explorer
- [ ] Verify: "Hash this file now" appears in menu
- [ ] Status: ___

#### 4.3 Use Context Menu
- [ ] Right-click `hello.txt` → "Hash this file now"
- [ ] Verify: `hello.txt.hashes.json` created silently
- [ ] Verify: No console window pops up (or closes quickly)
- [ ] Status: ___

#### 4.4 Uninstall Context Menu
- [ ] Run as Admin: `HashNow --uninstall`
- [ ] Expected: "Context menu removed successfully!"
- [ ] Verify: Menu item no longer appears
- [ ] Status: ___

---

### 5. Error Handling Tests

#### 5.1 File Not Found
- [ ] Run: `HashNow nonexistent.file`
- [ ] Expected: Error message about file not found
- [ ] Expected: Exit code != 0
- [ ] Status: ___

#### 5.2 Permission Denied
- [ ] Try to hash a locked/in-use file
- [ ] Expected: Graceful error message
- [ ] Status: ___

#### 5.3 Invalid Path Characters
- [ ] Try to hash with invalid path
- [ ] Expected: Graceful error handling
- [ ] Status: ___

---

### 6. Performance Tests

#### 6.1 Parallel Execution
- [ ] Hash a 100MB+ file
- [ ] Verify: CPU usage shows multiple cores active
- [ ] Verify: Completes faster than sequential would
- [ ] Status: ___

#### 6.2 Memory Usage
- [ ] Hash a 100MB file
- [ ] Monitor memory in Task Manager
- [ ] Verify: Memory doesn't exceed file size by too much
- [ ] Status: ___

---

### 7. Consistency Tests

#### 7.1 Deterministic Output
- [ ] Hash same file twice
- [ ] Verify: All hash values identical
- [ ] Status: ___

#### 7.2 Cross-Platform Hashes
- [ ] Compare MD5/SHA256 with external tool (certutil, PowerShell)
- [ ] Run: `certutil -hashfile hello.txt MD5`
- [ ] Run: `certutil -hashfile hello.txt SHA256`
- [ ] Verify: Values match HashNow output
- [ ] Status: ___

---

## Known Hash Values Reference

### Empty File (0 bytes)
| Algorithm | Expected Value |
|-----------|----------------|
| CRC32 | `00000000` |
| MD5 | `d41d8cd98f00b204e9800998ecf8427e` |
| SHA1 | `da39a3ee5e6b4b0d3255bfef95601890afd80709` |
| SHA256 | `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855` |

### "Hello, World!" (ASCII, no newline)
| Algorithm | Expected Value |
|-----------|----------------|
| MD5 | `65a8e27d8879283831b664bd8b7f0ad4` |
| SHA256 | `dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f` |

### Single Byte 0x00
| Algorithm | Expected Value |
|-----------|----------------|
| MD5 | `93b885adfe0da089cdf634904fd59f71` |
| SHA256 | `6e340b9cffb37a989ca544e6bb780a2c78901d3fb33738768511a30617afa01d` |

---

## Test Summary

| Category | Pass | Fail | Skip |
|----------|------|------|------|
| CLI Basic | ___ | ___ | ___ |
| File Hashing | ___ | ___ | ___ |
| JSON Output | ___ | ___ | ___ |
| Context Menu | ___ | ___ | ___ |
| Error Handling | ___ | ___ | ___ |
| Performance | ___ | ___ | ___ |
| Consistency | ___ | ___ | ___ |
| **TOTAL** | ___ | ___ | ___ |

## Notes
_Add any observations, issues, or comments here:_

---

## Sign-Off

- Tested By: _______________
- Date: _______________
- Version: _______________
- Result: [ ] PASS / [ ] FAIL
