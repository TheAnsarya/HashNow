# HashNow Project - AI Copilot Directives

## Project Overview

**HashNow** is a Windows file hashing utility that computes 58+ hash algorithms and outputs results to JSON. Features Explorer context menu integration for instant right-click hashing.

**Home Folder:** `C:\Users\me\source\repos\HashNow`

# HashNow Project - AI Copilot Directives

## Project Overview

**HashNow** is a Windows file hashing utility that computes 58+ hash algorithms and outputs results to JSON. Features Explorer context menu integration for instant right-click hashing.

**Home Folder:** `C:\Users\me\source\repos\HashNow`

## ⚠️ CRITICAL: Always Use Latest Modern Versions

**ALWAYS use the most modern, latest versions of everything:**

- **.NET 10** (not .NET 9, 8, 7, or older)
- **C# 14** (latest language version)
- **Visual Studio 2026** (if applicable)
- **Latest NuGet packages** - Always check for and use newest stable versions
- **Node.js LTS** (latest long-term support)
- **React 19+** (if using React)
- **TypeScript 5+** (if using TypeScript)
- **Modern code patterns:**
	- File-scoped namespaces
	- Primary constructors
	- Collection expressions `[item1, item2]`
	- Pattern matching with `when` guards
	- Nullable reference types
	- Spans and Memory<T>
	- `async`/`await` best practices
	- ArrayPool for memory pooling
	- Modern JSON serialization (System.Text.Json)
	- LINQ optimizations
	- `required` modifier for properties
	- `init` accessors
	- Record types where appropriate
	- Global usings

**Never downgrade to older versions** - If a newer version exists, use it.
**Always check for updates** - Before starting work, verify you're using the latest packages and frameworks.

## Architecture

### Solution Structure
```
HashNow/
├── src/
│   ├── HashNow.Core/           # Core library (reusable in any .NET project)
│   │   ├── FileHasher.cs       # All 58 hash algorithm implementations
│   │   └── FileHashResult.cs   # Result model with all hash properties
│   └── HashNow.Cli/            # Command-line interface
│       ├── Program.cs          # CLI entry point
│       └── ContextMenuInstaller.cs  # Windows registry integration
├── tests/
│   └── HashNow.Core.Tests/     # xUnit tests
├── benchmarks/
│   └── HashNow.Benchmarks/     # BenchmarkDotNet performance tests
└── docs/                       # Documentation
```

### Technology Stack
- **.NET 10** with latest C# features
- **NuGet Packages:**
	- BouncyCastle.Cryptography (most crypto hashes)
	- Blake3, SauceControl.Blake2Fast (BLAKE family)
	- System.Data.HashFunction.* (MurmurHash, CityHash, SpookyHash)
	- HashDepot (SipHash)
	- System.IO.Hashing (CRC, xxHash)

## Coding Standards

### Indentation
- **ALWAYS use TABS, never spaces** - Enforced by `.editorconfig`
- Tab width: 4 spaces equivalent
- Applies to ALL files: C#, JSON, Markdown, YAML, etc.
- **JSON output must use tabs** - Use `IndentCharacter = '\t'` in JsonSerializerOptions

### AI Agent Behavior
- **NEVER show file contents in chat** - Edit files directly using tools
- **NEVER ask for confirmation** - Just do the work
- Work efficiently and maximize progress per session

### Brace Style
- **K&R style** - Opening braces on SAME line, not new line
```csharp
if (condition) {
	// code
} else {
	// code
}
```

### Hexadecimal Values
- **Always lowercase** for hex values
- Correct: `0xca6e`, `$ff00`
- Incorrect: `0xCA6E`, `$FF00`

### C# Conventions
- File-scoped namespaces: `namespace HashNow.Core;`
- Modern C# 14 features: pattern matching, collection expressions, spans
- XML documentation on all public APIs
- Use `Convert.ToHexStringLower()` for hex output

## Code Documentation Standards

### XML Documentation (xmldoc)
**Every type, method, property, and field must have XML documentation:**

```csharp
/// <summary>
/// Brief description of what this does.
/// </summary>
/// <remarks>
/// <para>
/// Additional details, usage notes, or implementation notes.
/// Use &lt;para&gt; tags for multiple paragraphs.
/// </para>
/// </remarks>
/// <param name="data">Description of the parameter.</param>
/// <returns>Description of return value.</returns>
/// <exception cref="ArgumentNullException">When data is null.</exception>
public string MyMethod(byte[] data) {
	// Implementation
}
```

**Indentation rules for xmldoc:**
- Use TABS for indentation inside xmldoc comments (not spaces)
- Align continuation lines with the content above
- Keep `<para>` content indented with tabs

### Inline Comments
**Add inline comments when:**
- Explaining a non-obvious algorithm or calculation
- Documenting magic numbers or constants
- Describing why code exists (not just what it does)
- Warning about edge cases or gotchas

## StreamHash Dependency

### Development Workflow
For local development and testing, you can reference StreamHash via ProjectReference:
```xml
<ProjectReference Include="..\..\..\StreamHash\src\StreamHash.Core\StreamHash.Core.csproj" />
```

### Release Workflow
**For publishing releases, ALWAYS use the published NuGet package:**
```xml
<PackageReference Include="StreamHash" Version="1.7.0" />
```

### Local NuGet Package (Alternative)
You can build a local NuGet package without publishing:
```bash
cd C:\Users\me\source\repos\StreamHash
dotnet pack src/StreamHash.Core -c Release -o ./nupkgs
```
Then add the local folder as a NuGet source:
```bash
dotnet nuget add source C:\Users\me\source\repos\StreamHash\nupkgs --name LocalStreamHash
```

## Licensing

### ⚠️ IMPORTANT: Use The Unlicense

**All code in this project uses The Unlicense (public domain).**

When adding new files or projects:
1. Use The Unlicense for all original code
2. If incorporating GPL/LGPL code, keep that code separate and properly attributed
3. If using a library with restrictive license, document it clearly
4. Never use proprietary or copyleft licenses for HashNow code itself

The Unlicense text:
> This is free and unencumbered software released into the public domain.

### Third-Party Libraries
Current dependencies are all permissively licensed:
- BouncyCastle: MIT
- Blake3: Apache 2.0 / MIT
- System.Data.HashFunction.*: MIT
- HashDepot: MIT

## Build & Test

```bash
# Build
dotnet build HashNow.slnx

# Run tests
dotnet test

# Run CLI
dotnet run --project src/HashNow.Cli -- myfile.zip

# Install context menu (requires admin)
dotnet run --project src/HashNow.Cli -- --install
```

## Hash Algorithm Categories

### 1. Checksums & CRCs (6)
CRC32, CRC32C, CRC64, Adler-32, Fletcher-16, Fletcher-32

### 2. Fast Non-Crypto (12)
xxHash32/64/3/128, MurmurHash3-32/128, CityHash64/128, FarmHash64, SpookyV2, SipHash-2-4, HighwayHash64

### 3. Cryptographic (26)
MD2, MD4, MD5, SHA-0/1, SHA-224/256/384/512, SHA-512/224/256, SHA3-224/256/384/512, Keccak-256/512, BLAKE-256/512, BLAKE2b/2s, BLAKE3, RIPEMD-128/160/256/320

### 4. Other Crypto (14)
Whirlpool, Tiger-192, GOST-94, Streebog-256/512, Skein-256/512/1024, Groestl-256/512, JH-256/512, KangarooTwelve, SM3

## Git Workflow

### Commit Messages
Use conventional commits:
- `feat:` - New features
- `fix:` - Bug fixes
- `docs:` - Documentation
- `test:` - Tests
- `perf:` - Performance
- `chore:` - Maintenance

### Releases
Tag releases as `vX.Y.Z` (e.g., `v2.0.0`)

## Documentation

### `docs/`
- `ALGORITHM_ROADMAP.md` - Algorithm implementation status
- Add API documentation for library users

### `~docs/` (Development)
- Session logs
- Plans and notes

## Context Menu Integration

The CLI supports Windows Explorer integration:
- `HashNow --install` - Add "Hash this file now" to right-click menu
- `HashNow --uninstall` - Remove context menu item
- Clicking generates `{filename}.hashes.json` next to the file

## Performance Goals

- Single-pass file reading for all 58 hashes
- 1MB buffer with ArrayPool memory management
- Stream large files efficiently
- Progress reporting for files taking >3 seconds

## ⚠️ CRITICAL: Don't Half-Ass It

**Always do the whole thing. Don't quit at 80%.**

- If you can't complete something now, create a GitHub issue for later
- Never leave work partially done without tracking
- If a label doesn't exist, CREATE IT, then add it to the issue
- If you encounter blockers, document them and create issues
- Complete all follow-up tasks (docs, tests, issues, labels)

**GitHub Issue Management:**
- **ALWAYS create missing labels** - Never skip labels because they don't exist
- Use `gh label create` to create missing labels first
- Then create/update the issue with proper labels
- Labels should include: `performance`, `bug`, `enhancement`, `documentation`, `investigation`, `high-priority`, `medium-priority`, `low-priority`
