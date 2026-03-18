# Windows Installation Guide

## Step 1: Download

**[Download HashNow](https://github.com/TheAnsarya/HashNow/releases/latest)** — single self-contained `.exe`, no installer needed.

Download `HashNow-Windows-x64-vX.Y.Z.exe` from the [Releases page](https://github.com/TheAnsarya/HashNow/releases/latest) and save it somewhere permanent (e.g. `C:\Tools\HashNow.exe`). The context menu entry points to wherever you put the file, so don't move it after installing.

| ![Download HashNow from GitHub Releases](images/download-release.png) |
|---|

Your browser may warn that the file "isn't commonly downloaded." This is normal for new executables — click the keep/download option:

| ![Browser warning that HashNow.exe isn't commonly downloaded](images/hashnow-isnt-commonly-downloaded.png) |
|---|

Click **Keep** or **Keep anyway** to save the file:

| ![Click Keep to save the download](images/hashnow-isnt-commonly-downloaded-keep.png) |
|---|

| ![Confirm keeping the download](images/hashnow-isnt-commonly-downloaded-keep-anyways.png) |
|---|

## Step 2: Unblock the File

Windows marks downloaded files as blocked. Before running, right-click `HashNow.exe` and select **Properties**:

| ![Right-click HashNow.exe and select Properties](images/properties-window-contextmenu.png) |
|---|

At the bottom of the **General** tab, check the **Unblock** checkbox and click **OK**:

| ![Properties dialog showing the Unblock checkbox](images/properties-window-unblock-before.png) |
|---|

| ![Unblock checkbox checked](images/properties-window-unblock.png) |
|---|

## Step 3: Install Context Menu

Double-click `HashNow.exe`. On first launch (with no arguments), HashNow detects it was launched directly and offers to install the Explorer context menu:

| ![HashNow auto-install prompt](images/auto-install-prompt.png) |
|---|

Click **Yes** to install. A Windows UAC prompt will appear requesting administrator privileges (required to write to the Windows registry):

| ![UAC elevation prompt](images/uac-prompt.png) |
|---|

Click **Yes** to grant admin access. A confirmation dialog confirms the installation:

| ![Context menu installed successfully](images/install-success.png) |
|---|

You're done! The context menu is now available on all file types in Explorer.

## Step 4: Hash a File

Right-click any file in Windows Explorer and select **"Hash this file now"**:

| ![Explorer right-click context menu showing Hash this file now](images/context-menu.png) |
|---|

A progress dialog appears while hashing:

| ![Progress dialog showing hashing progress with percentage and cancel button](images/progress-dialog.png) |
|---|

The dialog shows the file name, a progress bar (0–100%), percentage complete, and a **Cancel** button to abort at any time. It closes automatically when done.

## Step 5: View Results

Find `{filename}.hashes.json` in the same folder as the original file:

| ![Explorer showing the generated .hashes.json file next to the original](images/output-file-explorer.png) |
|---|

Open the JSON file to see all 70 hashes organized by category:

| ![JSON output file contents showing hash values](images/json-output.png) |
|---|

## Uninstalling

To remove the context menu, run from an administrator command prompt:

```powershell
HashNow --uninstall
```
