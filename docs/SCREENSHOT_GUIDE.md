# Screenshot Guide

Instructions for capturing screenshots used in HashNow documentation. Each screenshot should be saved to `docs/images/` with the exact filename listed below.

All screenshots should be:

- **PNG format** (`.png`)
- **Cropped tight** — include only the relevant window/dialog, not the entire desktop
- **High DPI** — use 100% or 150% display scaling for crisp images (avoid 200%+ which makes images oversized)
- **Clean background** — close unrelated windows, use a neutral desktop wallpaper

## Screenshots to Capture

### 1. `download-release.png` — GitHub Releases Page

**Where:** GitHub → [HashNow Releases](https://github.com/TheAnsarya/HashNow/releases/latest)

**What to capture:** The release page showing the release title, description, and the **Assets** section with `HashNow.exe` visible as a downloadable file.

**How:**

1. Open the latest release page in a browser
2. Scroll so the release title and Assets section are both visible
3. Capture the browser content area (not the full browser chrome)

---

### 2. `hashnow-isnt-commonly-downloaded.png` — Browser Download Warning

**Where:** Browser download bar/notification after downloading `HashNow.exe`

**What to capture:** The browser warning that the file "isn't commonly downloaded" or similar security prompt.

**How:**

1. Download `HashNow.exe` from the GitHub Releases page
2. Screenshot the browser's warning/notification about the download

---

### 3. `hashnow-isnt-commonly-downloaded-keep.png` — Keep Download

**Where:** Browser download bar/notification

**What to capture:** Clicking the **Keep** option on the browser's download warning.

**How:**

1. When the download warning appears, click the keep/menu option
2. Screenshot showing the "Keep" button or option

---

### 4. `hashnow-isnt-commonly-downloaded-keep-anyways.png` — Confirm Keep

**Where:** Browser download bar/notification

**What to capture:** The final confirmation to keep the downloaded file.

**How:**

1. After clicking Keep, if a second confirmation appears, screenshot it
2. This shows the "Keep anyway" option

---

### 5. `properties-window-contextmenu.png` — Open File Properties

**Where:** Windows Explorer, right-clicking `HashNow.exe`

**What to capture:** The right-click context menu with **Properties** highlighted, showing how to access the file properties.

**How:**

1. Right-click `HashNow.exe` in Explorer
2. Screenshot the context menu with "Properties" visible

---

### 6. `properties-window-unblock-before.png` — Unblock Checkbox (Before)

**Where:** `HashNow.exe` Properties dialog, General tab

**What to capture:** The Properties dialog showing the **Unblock** checkbox at the bottom of the General tab, before it's been checked.

**How:**

1. Right-click `HashNow.exe` → Properties
2. Look at the bottom of the General tab for the "Unblock" checkbox
3. Screenshot with the checkbox unchecked

---

### 7. `properties-window-unblock.png` — Unblock Checkbox (Checked)

**Where:** `HashNow.exe` Properties dialog, General tab

**What to capture:** The Properties dialog with the **Unblock** checkbox checked, showing the file is now unblocked.

**How:**

1. Check the Unblock checkbox
2. Screenshot showing it checked (before clicking OK)

---

### 8. `auto-install-prompt.png` — Install Context Menu Prompt

**Where:** First launch with no arguments (double-click `HashNow.exe`)

**What to capture:** The dialog box asking whether to install the Explorer context menu. It should show a Yes/No prompt.

**How:**

1. Uninstall the context menu first: `HashNow.exe --uninstall`
2. Double-click `HashNow.exe` (no arguments)
3. Screenshot the prompt dialog

---

### 9. `uac-prompt.png` — UAC Elevation Dialog

**Where:** After clicking Yes on the install prompt

**What to capture:** The Windows User Account Control dialog asking for elevated permissions.

**How:**

1. Trigger the install prompt (see step 3 above)
2. Click Yes
3. Screenshot the UAC dialog
4. Click Yes to proceed (or No to cancel — either way you have the screenshot)

> **Note:** UAC dialogs can be difficult to screenshot with normal tools because they run on a secure desktop. Use the Print Screen key (it captures through the secure desktop on some configurations) or a phone camera as a fallback.

---

### 10. `install-success.png` — Installation Complete

**Where:** After UAC is approved and the context menu is registered

**What to capture:** The confirmation dialog saying the context menu was installed successfully.

**How:**

1. Complete the install flow (steps 3–4 above)
2. Screenshot the success confirmation dialog

---

### 11. `context-menu.png` — Explorer Right-Click Menu

**Where:** Windows Explorer, right-clicking any file

**What to capture:** The right-click context menu with **"Hash this file now"** highlighted or visible. Use the classic context menu (not the Windows 11 simplified menu).

**How:**

1. Open Windows Explorer
2. Navigate to a folder with a recognizable file (e.g., a `.zip` or `.exe`)
3. Right-click the file
4. If on Windows 11 and the classic menu isn't shown, click "Show more options" first
5. Screenshot with "Hash this file now" visible in the menu

> **Tip:** Hold Shift and right-click to get the full classic context menu on Windows 11.

---

### 12. `progress-dialog.png` — Hashing Progress

**Where:** While hashing a file via context menu or CLI

**What to capture:** The progress dialog showing a file being hashed, with the progress bar partially filled (not 0% or 100%).

**How:**

1. Find a large file (100+ MB) so the dialog is visible long enough
2. Right-click the file → "Hash this file now"
3. Quickly screenshot while the progress bar is between 20–80%
4. The dialog shows: file name, progress bar, percentage, and Cancel button

> **Tip:** Use a large ISO or video file to give yourself time to capture the screenshot.

---

### 13. `output-file-explorer.png` — Output File in Explorer

**Where:** Windows Explorer, same folder as the hashed file

**What to capture:** The Explorer view showing both the original file and the `.hashes.json` file next to it.

**How:**

1. Hash any file (e.g., `example.zip`)
2. Open the folder in Explorer
3. Sort by name so both `example.zip` and `example.zip.hashes.json` are adjacent
4. Screenshot showing both files

---

### 14. `json-output.png` — JSON File Contents

**Where:** A text editor (VS Code, Notepad++, or even Notepad) showing the `.hashes.json` file

**What to capture:** The beginning of the JSON file showing the metadata fields and the first few hash values. The tab indentation and category organization should be visible.

**How:**

1. Open any `.hashes.json` file in a text editor with syntax highlighting (VS Code recommended)
2. Scroll to the top
3. Screenshot showing approximately the first 30–40 lines (metadata + some hash values)
4. Make sure the JSON syntax highlighting shows the structure clearly

---

### 15. `cli-hash-output.png` — CLI Hashing Output

**Where:** Terminal/PowerShell running `HashNow.exe` on a file

**What to capture:** The terminal output when hashing a file from the command line, showing the file path, progress, and completion message.

**How:**

1. Open PowerShell or Windows Terminal
2. Run: `.\HashNow.exe somefile.zip`
3. Screenshot the terminal showing the complete output

---

### 16. `cli-help.png` — CLI Help Output

**Where:** Terminal/PowerShell running `HashNow.exe --help`

**What to capture:** The full help text showing all available commands and usage information.

**How:**

1. Open PowerShell or Windows Terminal
2. Run: `.\HashNow.exe --help`
3. Screenshot the terminal showing the complete help output

---

## File Checklist

Save all screenshots to `docs/images/` with these exact filenames:

| # | Filename | Description |
|:-:|----------|-------------|
| 1 | `download-release.png` | GitHub Releases page with Assets |
| 2 | `hashnow-isnt-commonly-downloaded.png` | Browser download warning |
| 3 | `hashnow-isnt-commonly-downloaded-keep.png` | Clicking Keep on download warning |
| 4 | `hashnow-isnt-commonly-downloaded-keep-anyways.png` | Confirming keep download |
| 5 | `properties-window-contextmenu.png` | Right-click → Properties on exe |
| 6 | `properties-window-unblock-before.png` | Properties dialog with Unblock unchecked |
| 7 | `properties-window-unblock.png` | Properties dialog with Unblock checked |
| 8 | `auto-install-prompt.png` | Context menu install Yes/No prompt |
| 9 | `uac-prompt.png` | UAC elevation dialog |
| 10 | `install-success.png` | Installation success confirmation |
| 11 | `context-menu.png` | Explorer right-click showing "Hash this file now" |
| 12 | `progress-dialog.png` | Progress bar mid-hash (20–80%) |
| 13 | `output-file-explorer.png` | Explorer showing original + .hashes.json |
| 14 | `json-output.png` | JSON file contents in a text editor |
| 15 | `cli-hash-output.png` | Terminal output of hashing a file |
| 16 | `cli-help.png` | Terminal output of --help command |
