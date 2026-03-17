# HashNow Cross-Platform Plan

## Overview

Bring HashNow's "right-click → hash file → JSON output" experience to **Linux** and **macOS** alongside the existing Windows implementation. The core hashing library (`HashNow.Core`) is already platform-agnostic; the work is in the CLI, file-manager integration, and CI/CD.

## Current Architecture

```
HashNow.Core (net10.0)              ← Platform-agnostic, no changes needed
  └── StreamHash (NuGet)            ← Pure C#, cross-platform

HashNow.Cli (net10.0-windows)       ← Windows-only today
  ├── Program.cs                    ← Entry point, console attachment (P/Invoke)
  ├── ContextMenuInstaller.cs       ← Windows Registry operations
  ├── GuiDialogs.cs                 ← WinForms MessageBox wrappers
  ├── ProgressDialog.cs             ← WinForms progress dialog
  ├── ConsoleProgressBar.cs         ← Text-mode progress bar (mostly portable)
  └── HashNow.Cli.csproj            ← WinExe, UseWindowsForms, win-x64 only
```

## Target Architecture

```
HashNow.Core (net10.0)              ← No changes
  └── StreamHash (NuGet)

HashNow.Cli (net10.0)               ← Cross-platform entry point
  ├── Program.cs                    ← Unified entry point, platform dispatch
  ├── ConsoleProgressBar.cs         ← Text-mode progress (already portable)
  ├── Platform/
  │   ├── IPlatformIntegration.cs   ← Interface for platform-specific operations
  │   ├── PlatformFactory.cs        ← Factory to select runtime platform
  │   ├── Windows/
  │   │   ├── WindowsPlatform.cs    ← Windows-specific implementation
  │   │   ├── ContextMenuInstaller.cs ← Registry-based context menu
  │   │   ├── GuiDialogs.cs         ← WinForms dialogs
  │   │   └── ProgressDialog.cs     ← WinForms progress
  │   ├── Linux/
  │   │   ├── LinuxPlatform.cs      ← Linux-specific implementation
  │   │   ├── NautilusInstaller.cs  ← Nautilus/GNOME scripts
  │   │   ├── NemoInstaller.cs      ← Nemo/Cinnamon actions
  │   │   ├── DolphinInstaller.cs   ← Dolphin/KDE service menus
  │   │   └── ThunarInstaller.cs    ← Thunar/XFCE custom actions
  │   └── MacOS/
  │       ├── MacOSPlatform.cs      ← macOS-specific implementation
  │       ├── FinderExtension.cs    ← Quick Actions / Automator workflow
  │       └── ServiceInstaller.cs   ← macOS Services menu
  └── HashNow.Cli.csproj            ← Multi-TFM or conditional compilation
```

## Phase 1: Platform Abstraction Layer

### IPlatformIntegration Interface

```csharp
/// <summary>
/// Platform-specific operations for file manager integration.
/// </summary>
public interface IPlatformIntegration {
	/// <summary>Install context menu / file manager integration.</summary>
	bool Install(string executablePath);

	/// <summary>Remove context menu / file manager integration.</summary>
	bool Uninstall();

	/// <summary>Check if integration is currently installed.</summary>
	bool IsInstalled();

	/// <summary>Check if integration points to the correct executable.</summary>
	bool IsInstalledCorrectly(string executablePath);

	/// <summary>Show a yes/no dialog (GUI or terminal fallback).</summary>
	bool AskYesNo(string title, string message);

	/// <summary>Show an information message.</summary>
	void ShowInfo(string title, string message);

	/// <summary>Show an error message.</summary>
	void ShowError(string title, string message);

	/// <summary>Show a success message.</summary>
	void ShowSuccess(string title, string message);

	/// <summary>Whether the platform supports GUI progress dialogs.</summary>
	bool SupportsGuiProgress { get; }

	/// <summary>Run a hashing operation with platform-appropriate progress UI.</summary>
	Task<FileHashResult?> HashFileWithProgress(string filePath,
		CancellationToken ct = default);

	/// <summary>Whether the current process has elevated privileges.</summary>
	bool IsElevated { get; }

	/// <summary>Relaunch the current process with elevated privileges.</summary>
	bool RelaunchElevated(string[] args);

	/// <summary>Platform display name (e.g., "Windows", "Linux (GNOME)", "macOS").</summary>
	string PlatformName { get; }
}
```

### Platform Factory

```csharp
public static class PlatformFactory {
	public static IPlatformIntegration Create() {
		if (OperatingSystem.IsWindows()) return new WindowsPlatform();
		if (OperatingSystem.IsLinux()) return new LinuxPlatform();
		if (OperatingSystem.IsMacOS()) return new MacOSPlatform();
		throw new PlatformNotSupportedException();
	}
}
```

## Phase 2: Linux File Manager Integration

### Approach: Multi-Desktop Support

Linux has multiple file managers. We detect which are installed and configure all of them.

#### GNOME / Nautilus (Ubuntu, Fedora default)

**Nautilus Scripts** — simplest approach, works on all Nautilus versions:

- Create script at `~/.local/share/nautilus/scripts/Hash this file now`
- The script is a bash wrapper that invokes HashNow
- Nautilus passes selected files via `NAUTILUS_SCRIPT_SELECTED_FILE_PATHS` env var
- User accesses via right-click → Scripts → "Hash this file now"

```bash
#!/bin/bash
# ~/.local/share/nautilus/scripts/Hash this file now
IFS=$'\n'
for file in $NAUTILUS_SCRIPT_SELECTED_FILE_PATHS; do
	/path/to/HashNow "$file"
done
```

#### Nemo (Linux Mint / Cinnamon)

**Nemo Actions** — `.nemo_action` file in `~/.local/share/nemo/actions/`:

```ini
[Nemo Action]
Name=Hash this file now
Comment=Compute 70 hash algorithms and save to JSON
Exec=/path/to/HashNow %F
Icon-Name=document-properties
Selection=s
Extensions=any;
```

#### Dolphin (KDE Plasma)

**Service Menus** — `.desktop` file in `~/.local/share/kio/servicemenus/`:

```ini
[Desktop Entry]
Type=Service
ServiceTypes=KonqPopupMenu/Plugin
MimeType=application/octet-stream;
Actions=hashfile

[Desktop Action hashfile]
Name=Hash this file now
Icon=document-properties
Exec=/path/to/HashNow %f
```

#### Thunar (XFCE)

**Custom Actions** — configured via `~/.config/Thunar/uca.xml`:

```xml
<action>
	<icon>document-properties</icon>
	<name>Hash this file now</name>
	<command>/path/to/HashNow %f</command>
	<description>Compute 70 hash algorithms</description>
	<patterns>*</patterns>
	<directories/>
	<text-files/>
	<other-files/>
</action>
```

### Linux Elevation

- No elevation needed — all file manager configs are per-user (`~/.local/share/...`)
- No sudo/pkexec required for install/uninstall
- Progress: Use `ConsoleProgressBar` (text mode) — no GUI dependency

### Linux Dialogs

- Use `zenity` or `kdialog` if available for GUI prompts
- Fall back to terminal-based yes/no prompts
- `zenity --question --title="HashNow" --text="Install context menu?"`

## Phase 3: macOS Finder Integration

### Approach: Quick Actions (Automator Workflows)

macOS Quick Actions appear in Finder's right-click menu and are the sanctioned way to add Finder context menu items.

#### Quick Action Workflow

Create an Automator `.workflow` bundle at `~/Library/Services/`:

```
~/Library/Services/Hash this file now.workflow/
└── Contents/
    ├── Info.plist
    └── document.wflow
```

The workflow runs a shell script that invokes HashNow:

```bash
#!/bin/bash
for f in "$@"; do
	/path/to/HashNow "$f"
done
```

#### Alternative: macOS Services via Shell Script

Create a `.workflow` programmatically using property list XML:

- `Info.plist`: Declares the service name, input types, icon
- `document.wflow`: Contains the workflow actions (Run Shell Script)

### macOS Elevation

- No elevation needed — Quick Actions install to user's `~/Library/Services/`
- No `sudo` required

### macOS Dialogs

- Use `osascript` for native AppleScript dialogs:
  ```bash
  osascript -e 'display dialog "Install context menu?" buttons {"No", "Yes"} default button "Yes"'
  ```
- Fall back to terminal prompts

### macOS Notifications

- Use `osascript` for notifications:
  ```bash
  osascript -e 'display notification "Hashing complete" with title "HashNow"'
  ```

## Phase 4: Project File Changes

### Option A: Multi-TFM with Conditional Compilation (Recommended)

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<TargetFrameworks>net10.0-windows;net10.0</TargetFrameworks>
		<OutputType Condition="'$(TargetFramework)' == 'net10.0-windows'">WinExe</OutputType>
		<OutputType Condition="'$(TargetFramework)' != 'net10.0-windows'">Exe</OutputType>
		<UseWindowsForms Condition="'$(TargetFramework)' == 'net10.0-windows'">true</UseWindowsForms>
	</PropertyGroup>

	<!-- Windows-only references -->
	<ItemGroup Condition="'$(TargetFramework)' == 'net10.0-windows'">
		<PackageReference Include="System.Management" />
	</ItemGroup>
</Project>
```

### Option B: Runtime ID approach (single TFM)

Build for each RID separately:

```
dotnet publish -r win-x64
dotnet publish -r linux-x64
dotnet publish -r linux-arm64
dotnet publish -r osx-x64
dotnet publish -r osx-arm64
```

**Decision: Use Option A** — Multi-TFM gives compile-time safety for Windows-specific code.

## Phase 5: CI/CD Multi-Platform Builds

### GitHub Actions Workflow

Following the Nexen pattern:

```yaml
name: Build & Release HashNow

on:
  push:
    branches: ['**']
    tags: ['v*']

jobs:
  windows:
    runs-on: windows-latest
    # Build win-x64, publish single exe

  linux:
    strategy:
      matrix:
        platform:
          - { os: ubuntu-22.04, rid: linux-x64, name: x64 }
          - { os: ubuntu-22.04-arm, rid: linux-arm64, name: ARM64 }
    runs-on: ${{ matrix.platform.os }}
    # Build linux binary, publish as tarball

  macos:
    strategy:
      matrix:
        platform:
          - { os: macos-14, rid: osx-arm64, name: ARM64 }
    runs-on: ${{ matrix.platform.os }}
    # Build macOS binary, publish as zip

  test:
    runs-on: ubuntu-latest
    # Run tests (core is cross-platform)

  release:
    if: startsWith(github.ref, 'refs/tags/v')
    needs: [windows, linux, macos, test]
    # Create GitHub release with all artifacts
```

### Release Asset Naming

```
HashNow-Windows-x64-v1.5.0.exe
HashNow-Linux-x64-v1.5.0.tar.gz
HashNow-Linux-ARM64-v1.5.0.tar.gz
HashNow-macOS-ARM64-v1.5.0.tar.gz
```

## Phase 6: Documentation Updates

- Update README with Linux/macOS install instructions
- Add platform-specific screenshots (Nautilus, Dolphin, Finder)
- Update MANUAL_TESTING.md with cross-platform test procedures
- Update PERFORMANCE.md with multi-platform benchmarks

## Implementation Order

1. **Phase 1**: Platform abstraction layer + refactor Windows code into it
2. **Phase 2**: Linux integration (Nautilus + Nemo + Dolphin + Thunar)
3. **Phase 3**: macOS integration (Quick Actions)
4. **Phase 4**: Project file + conditional compilation
5. **Phase 5**: CI/CD workflows
6. **Phase 6**: Documentation
7. **Testing**: Cross-platform validation

## Risk Assessment

| Risk | Mitigation |
|------|------------|
| WinForms dependency blocks cross-platform build | Multi-TFM with conditional compilation |
| Linux file managers vary widely | Support top 4: Nautilus, Nemo, Dolphin, Thunar |
| macOS Quick Actions hard to create programmatically | Use plist XML generation |
| .NET single-file size (~50MB) | Acceptable — same as Windows version |
| ARM64 Linux/macOS testing | Use GitHub Actions ARM runners |
| No GUI on headless Linux | Fall back to terminal-only mode |
| Nautilus Scripts submenu | Limitation: Items appear under Scripts → submenu, not top-level |

## Non-Goals (This Iteration)

- GUI progress dialog on Linux/macOS (text-mode progress is fine)
- AoT compilation (future optimization)
- Package managers (apt, brew, snap — future work)
- Linux .deb/.rpm packages (future work)
- macOS .dmg installer (future work)
- Drag-and-drop support
