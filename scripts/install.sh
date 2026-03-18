#!/bin/bash
# ────────────────────────────────────────────────────────────────
# HashNow Easy Installer for Linux
# Double-click this file in your file manager to install HashNow.
#
# What this script does:
#   1. Copies the HashNow binary to ~/.local/bin/
#   2. Adds ~/.local/bin to your PATH if needed
#   3. Installs file manager integration (right-click menu)
#
# No sudo required — everything installs to your home directory.
# ────────────────────────────────────────────────────────────────

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BINARY="$SCRIPT_DIR/HashNow"
INSTALL_DIR="$HOME/.local/bin"
APP_NAME="HashNow"

# ── Dialog helpers ──────────────────────────────────────────────
# Try zenity (GTK), then kdialog (KDE), then terminal fallback.

show_info() {
	local title="$1" msg="$2"
	if command -v zenity &>/dev/null; then
		zenity --info --title="$title" --text="$msg" --width=420 2>/dev/null && return
	fi
	if command -v kdialog &>/dev/null; then
		kdialog --title "$title" --msgbox "$msg" 2>/dev/null && return
	fi
	echo "$msg"
}

show_error() {
	local title="$1" msg="$2"
	if command -v zenity &>/dev/null; then
		zenity --error --title="$title" --text="$msg" --width=420 2>/dev/null && return
	fi
	if command -v kdialog &>/dev/null; then
		kdialog --title "$title" --error "$msg" 2>/dev/null && return
	fi
	echo "ERROR: $msg" >&2
}

ask_yes_no() {
	local title="$1" msg="$2"
	if command -v zenity &>/dev/null; then
		zenity --question --title="$title" --text="$msg" --width=420 2>/dev/null
		return $?
	fi
	if command -v kdialog &>/dev/null; then
		kdialog --title "$title" --yesno "$msg" 2>/dev/null
		return $?
	fi
	# Terminal fallback
	read -rp "$msg [Y/n] " response
	[[ "$response" =~ ^[Yy]$ ]] || [[ -z "$response" ]]
}

# ── Pre-flight checks ──────────────────────────────────────────

if [ ! -f "$BINARY" ]; then
	show_error "$APP_NAME" \
		"HashNow binary not found!\n\nMake sure this install script is in the same folder as the HashNow binary.\n\nExpected location:\n$BINARY"
	exit 1
fi

# ── Welcome dialog ─────────────────────────────────────────────

ask_yes_no "$APP_NAME — Install" \
	"Welcome to HashNow!\n\nHashNow computes 70 hash algorithms for any file.\nRight-click any file in your file manager to hash it instantly.\n\nThis will:\n  • Copy HashNow to ~/.local/bin/\n  • Add right-click menu to your file manager\n\nInstall now?" || {
	exit 0
}

# ── Install binary ─────────────────────────────────────────────

chmod +x "$BINARY"
mkdir -p "$INSTALL_DIR"
cp "$BINARY" "$INSTALL_DIR/HashNow"
chmod +x "$INSTALL_DIR/HashNow"

# ── Add to PATH if needed ──────────────────────────────────────

if [[ ":$PATH:" != *":$INSTALL_DIR:"* ]]; then
	SHELL_CONFIG=""
	if [ -f "$HOME/.bashrc" ]; then
		SHELL_CONFIG="$HOME/.bashrc"
	elif [ -f "$HOME/.profile" ]; then
		SHELL_CONFIG="$HOME/.profile"
	fi

	if [ -n "$SHELL_CONFIG" ]; then
		if ! grep -q '.local/bin' "$SHELL_CONFIG" 2>/dev/null; then
			{
				echo ''
				echo '# Added by HashNow installer'
				echo 'export PATH="$HOME/.local/bin:$PATH"'
			} >> "$SHELL_CONFIG"
		fi
	fi
	export PATH="$INSTALL_DIR:$PATH"
fi

# ── Install file manager integration ───────────────────────────
# --gui-install shows native dialogs for the result (zenity/kdialog)

"$INSTALL_DIR/HashNow" --gui-install
