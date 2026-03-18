# Linux Installation Guide

## Step 1: Download

Download the latest Linux release from the [Releases page](https://github.com/TheAnsarya/HashNow/releases/latest):

- **x64**: `HashNow-Linux-x64-vX.Y.Z.tar.gz`
- **ARM64**: `HashNow-Linux-ARM64-vX.Y.Z.tar.gz`

## Step 2: Extract and Install

Extract the tarball and place `HashNow` somewhere on your PATH (e.g. `~/.local/bin/`):

```bash
# x64
tar xzf HashNow-Linux-x64-*.tar.gz
cp HashNow-Linux-x64/HashNow ~/.local/bin/
chmod +x ~/.local/bin/HashNow

# ARM64
tar xzf HashNow-Linux-ARM64-*.tar.gz
cp HashNow-Linux-ARM64/HashNow ~/.local/bin/
chmod +x ~/.local/bin/HashNow
```

Make sure `~/.local/bin` is in your PATH. Add this to `~/.bashrc` or `~/.profile` if needed:

```bash
export PATH="$HOME/.local/bin:$PATH"
```

## Step 3: Install Context Menu Integration

HashNow auto-detects your installed file managers and installs for all of them:

```bash
HashNow --install
```

Supported file managers:

| File Manager | Desktop Environment | Integration Type |
|-------------|-------------------|-----------------|
| **Nautilus** (GNOME Files) | GNOME | Script in `~/.local/share/nautilus/scripts/` |
| **Nemo** | Cinnamon | Action in `~/.local/share/nemo/actions/` |
| **Dolphin** | KDE Plasma | Service menu in `~/.local/share/kio/servicemenus/` |
| **Thunar** | Xfce | Custom action via `thunar --quit && thunar` |

All installs are user-level — no `sudo` required.

## Step 4: Hash a File

Right-click any file in your file manager and select **"Hash this file now"**.

A progress indicator appears while hashing:

- **GTK desktops** (GNOME, Xfce, Cinnamon): zenity progress bar with percentage and cancel button
- **KDE Plasma**: kdialog progress bar with cancel button
- **No GUI available**: falls back to silent hashing

The output `{filename}.hashes.json` appears in the same directory as the original file.

## Command-Line Usage

HashNow also works from the terminal:

```bash
# Hash a file
HashNow myfile.zip

# Hash multiple files
HashNow file1.iso file2.zip file3.bin

# Check installation status
HashNow --status

# Show help
HashNow --help
```

## Uninstalling

```bash
HashNow --uninstall
```

This removes all file manager integration scripts/actions. No elevation required.
