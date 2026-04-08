# Dual Context Menu + Basic Mode Plan

## Scope

- Add a second Windows context menu entry.
- Rename existing full entry to include ` (full)`.
- Keep basic entry label as `Hash this file now`.
- Add install/uninstall mode selection (`full`, `basic`, `both`).
- Add basic hashing runtime mode with five algorithms: CRC32, MD5, SHA-1, SHA-256, SHA-512.

## Implementation Breakdown

1. Platform contract updates

- Introduce `ContextMenuInstallMode` enum.
- Extend `IPlatformIntegration.Install` and `IPlatformIntegration.Uninstall` with install mode.

2. Windows integration changes

- Register full key: `HKEY_CLASSES_ROOT\*\shell\HashNowFull`.
- Register basic key: `HKEY_CLASSES_ROOT\*\shell\HashNowBasic`.
- Full command: `"HashNow.exe" "%1"`.
- Basic command: `"HashNow.exe" --basic "%1"`.
- Extend status reporting to show both entries independently.

3. Core hashing changes

- Add `BasicFileHashResult` model.
- Add `FileHasher.HashFileBasic` and `HashFileBasicAsync`.
- Add JSON save overloads for `BasicFileHashResult`.

4. CLI parsing and UX

- Support `--basic <file>` runtime mode.
- Support `--install --menu full|basic|both`.
- Add convenience switches: `--install-full`, `--install-basic`, `--install-both`.
- Support mode-aware `--uninstall --menu ...`.

5. Validation

- Add core tests for basic mode output parity and metadata.
- Add benchmark method comparing full vs basic for 10 MB.
- Run full test suite and benchmark slice.

## Acceptance Criteria

- Windows Explorer can show full-only, basic-only, or both menu items.
- Full menu still computes all 70+ algorithms.
- Basic menu computes only CRC32, MD5, SHA-1, SHA-256, SHA-512.
- `--status` clearly reports both entries.
- Docs and changelog reflect new behavior.
