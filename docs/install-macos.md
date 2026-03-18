# macOS Installation Guide

## Step 1: Download

Download the latest macOS release from the [Releases page](https://github.com/TheAnsarya/HashNow/releases/latest):

- **ARM64 (Apple Silicon)**: `HashNow-macOS-ARM64-vX.Y.Z.tar.gz`

## Step 2: Extract and Install

Extract the tarball and place `HashNow` somewhere on your PATH (e.g. `/usr/local/bin/`):

```bash
tar xzf HashNow-macOS-ARM64-*.tar.gz
sudo cp HashNow-macOS-ARM64/HashNow /usr/local/bin/
chmod +x /usr/local/bin/HashNow
```

Alternatively, install to a user-local directory:

```bash
mkdir -p ~/.local/bin
cp HashNow-macOS-ARM64/HashNow ~/.local/bin/
chmod +x ~/.local/bin/HashNow
```

Make sure the directory is in your PATH. Add this to `~/.zshrc` if needed:

```bash
export PATH="$HOME/.local/bin:$PATH"
```

## Step 3: Install Finder Quick Action

```bash
HashNow --install
```

This installs an Automator workflow to `~/Library/Services/`. No admin privileges required.

## Step 4: Hash a File

Right-click any file in Finder and choose **Quick Actions → Hash this file now**.

A progress dialog appears while hashing:

- **osascript dialog**: native macOS dialog with a **Cancel** button (always available)
- **zenity progress bar**: if installed via Homebrew (`brew install zenity`), provides a GTK-style progress bar with percentage

The output `{filename}.hashes.json` appears next to the original file.

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

This removes the Finder Quick Action workflow. No elevation required.
