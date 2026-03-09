# HashNow Project - AI Copilot Directives

## Project Overview

**HashNow** is a Windows file hashing utility that computes 70+ hash algorithms and outputs results to JSON. Features Explorer context menu integration for instant right-click hashing. Powered by StreamHash for all hash algorithm implementations.

**Home Folder:** `C:\Users\me\source\repos\HashNow`

## GitHub Issue Management

### ⚠️ CRITICAL: Always Create Issues on GitHub Directly

**NEVER just document issues in markdown files.** Always create actual GitHub issues using the `gh` CLI:

```powershell
# Create an issue
gh issue create --repo TheAnsarya/HashNow --title "Issue Title" --body "Description" --label "label1,label2"

# Add labels
gh issue edit <number> --repo TheAnsarya/HashNow --add-label "label"

# Close issue
gh issue close <number> --repo TheAnsarya/HashNow --comment "Completed in commit abc123"
```

### Required Labels

- `performance` - Performance related
- `bug` - Bug fixes
- `enhancement` - New features
- `documentation` - Documentation updates
- `investigation` - Research/analysis tasks
- `high-priority`, `medium-priority`, `low-priority` - Priority levels
- `testing` - Test improvements
- `accuracy` - Hash accuracy related

### ⚠️ MANDATORY: Issue-First Workflow

**Always create GitHub issues BEFORE starting implementation work.** This is non-negotiable.

1. **Before Implementation:**
	- Create a GitHub issue describing the planned work
	- Include scope, approach, and acceptance criteria
	- Add appropriate labels

2. **During Implementation:**
	- Reference issue number in commits: `git commit -m "Fix JSON output - #12"`
	- Update issue with progress comments if work spans multiple sessions
	- Add sub-issues for discovered work

3. **After Implementation:**
	- Close issue with completion comment including commit hash
	- Link related issues if applicable

### ⚠️ MANDATORY: Prompt Tracking for AI-Created Issues

When creating GitHub issues from AI prompts, **IMMEDIATELY** add the original user prompt as the **FIRST comment** right after creating the issue:

```powershell
# Create issue
$issueUrl = gh issue create --repo TheAnsarya/HashNow --title "Description" --body "Details" --label "label"
$issueNum = ($issueUrl -split '/')[-1]

# IMMEDIATELY add prompt as first comment (before any other work)
gh issue comment $issueNum --repo TheAnsarya/HashNow --body "Prompt for work:
<original user prompt that triggered this work>"
```

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

### File Formatting (CRITICAL)

**ALL files must follow these rules:**

- **Encoding:** UTF-8 with BOM
- **Line Endings:** CRLF (Windows style)
- **Indentation:** TABS only, NEVER spaces
- **Final Newline:** Always include a blank line at the end of every file
- **Trailing Whitespace:** Remove from all lines

**Markdown Files:**
- Format using `.editorconfig` rules
- Use markdownlint with **MD010 disabled** (hard tabs are REQUIRED, not forbidden)
- All markdown files must have proper heading hierarchy
- Include blank line at end of file

### ⚠️ MANDATORY: Fix Markdownlint Warnings

Always fix markdownlint warnings when creating or editing markdown files.

Minimum required rules to enforce:

- **MD022** - Blank lines above and below headings
- **MD031** - Blank lines around fenced code blocks
- **MD032** - Blank lines around lists
- **MD047** - File ends with a single newline

Generate markdown with correct spacing by default so additional cleanup is not needed.

### ⚠️ MANDATORY: Documentation Link-Tree

Every markdown file must be reachable from the main `README.md` through a maintained link-tree.

- Update `README.md` when adding new documentation
- Update intermediate index pages when reorganizing docs
- Do not leave orphan markdown files

**When creating or editing files:**
1. Always use tabs for indentation
2. Always add a blank line at the end
3. Always use UTF-8 encoding with BOM
4. Always use CRLF line endings
5. Never leave trailing whitespace

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

- Single-pass file reading for all 70+ hashes
- 1MB buffer with ArrayPool memory management
- Stream large files efficiently
- Progress reporting for files taking >3 seconds

### ⚠️ ABSOLUTE RULE: Hash Accuracy is Sacred

**NEVER sacrifice hash correctness for performance.** This is the #1 non-negotiable rule.

- **Every performance optimization MUST be correctness-preserving** — if a change could alter hash output in any way, it is rejected
- **Run ALL tests after every change** — all xUnit tests must pass before any commit
- **Benchmark BEFORE and AFTER** — prove the improvement with data, not just theory
- **When in doubt, don't optimize** — a slower correct hasher is infinitely better than a faster broken one

#### Verification Checklist (for EVERY performance change):

1. All xUnit tests pass (`dotnet test`)
2. Benchmark shows measurable improvement (BenchmarkDotNet)
3. Change is semantics-preserving (same hash outputs)
4. No new warnings in build output
5. Test vectors still match reference implementations

#### Focused Validation Commands (StreamingHasher)

- Finalize/progress correctness checks:
	- `dotnet test tests/HashNow.Core.Tests/HashNow.Core.Tests.csproj -c Release --filter "HashFileAsync_ProgressValues_AreMonotonicAndBounded|HashFileAsync_FinalizeMapping_MapsRepresentativeAliases"`
- Finalize-heavy microbenchmark:
	- `dotnet run --project benchmarks/HashNow.Benchmarks/HashNow.Benchmarks.csproj -c Release -- --filter "*FileHasherBenchmarks*HashNow_EmptyFile*" --job short`

#### Types of Safe Performance Changes:

- **Buffer size tuning** — different ArrayPool sizes for different file ranges
- **Avoiding copies** — use `Span<T>` instead of array copies
- **Eliminating allocations** — pool or reuse objects in hot paths
- **Parallel processing** — multiple hash algorithms concurrently
- **I/O optimization** — async reads, larger buffers, sequential access hints

#### Types of DANGEROUS Changes (require extra scrutiny):

- Anything touching hash algorithm implementations (use StreamHash)
- Buffer management changes that affect data fed to hash algorithms
- Reordering or parallelizing in ways that could skip bytes
- Changes to JSON serialization that could alter output format

## Problem-Solving Philosophy

### ⚠️ NEVER GIVE UP on Hard Problems

When a task is complex or seems difficult:

1. **NEVER declare something "too hard" or "not worth it"** and close the issue
2. **Break it down** — Create multiple smaller sub-issues for research, prototyping, and incremental progress
3. **Research first** — Create research issues to investigate approaches, alternatives, and prior art
4. **Document everything** — Create docs, code-plans, and analysis documents in `~docs/`
5. **Prototype** — Create spike/prototype branches to test approaches before committing
6. **Incremental progress** — Even partial progress is valuable
7. **Create issues for future work** — If something can't be done now, create well-documented issues with clear context for later

### What "Closing Too Soon" Looks Like

- "This is deeply integrated, keeping as-is" — Instead: break it into phases
- "Migration cost-prohibitive" — Instead: create research issues and prototype
- "High regression risk" — Instead: create test plan and incremental migration
- Close only when the work is **actually complete** or **truly impossible** (not just hard)

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
