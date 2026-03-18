#!/bin/bash
# ────────────────────────────────────────────────────────────────
# HashNow Easy Installer for macOS
# Double-click this file in Finder to install HashNow.
#
# The .command extension makes this file double-clickable in
# Finder — it opens Terminal briefly to run the installer, but
# all interaction happens through native macOS dialogs.
#
# What this script does:
#   1. Removes macOS Gatekeeper quarantine from HashNow
#   2. Copies the HashNow binary to ~/.local/bin/
#   3. Adds ~/.local/bin to your PATH if needed
#   4. Installs Finder Quick Action (right-click menu)
#
# No admin password required — everything installs to your
# home directory.
# ────────────────────────────────────────────────────────────────

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BINARY="$SCRIPT_DIR/HashNow"
INSTALL_DIR="$HOME/.local/bin"
APP_NAME="HashNow"

# ── Dialog helpers (osascript — always available on macOS) ─────

show_info() {
	local msg="$1"
	osascript -e "display dialog \"$msg\" with title \"$APP_NAME\" buttons {\"OK\"} default button \"OK\"" 2>/dev/null || echo "$msg"
}

show_error() {
	local msg="$1"
	osascript -e "display dialog \"$msg\" with title \"$APP_NAME\" with icon stop buttons {\"OK\"} default button \"OK\"" 2>/dev/null || echo "ERROR: $msg" >&2
}

# Returns 0 if user clicks Install, 1 if Cancel
ask_install() {
	local msg="$1"
	osascript -e "display dialog \"$msg\" with title \"$APP_NAME\" buttons {\"Cancel\", \"Install\"} default button \"Install\"" 2>/dev/null
	return $?
}

# ── Pre-flight checks ──────────────────────────────────────────

if [ ! -f "$BINARY" ]; then
	show_error "HashNow binary not found!\\n\\nMake sure this installer is in the same folder as the HashNow binary.\\n\\nExpected location:\\n$BINARY"
	exit 1
fi

# ── Welcome dialog ─────────────────────────────────────────────

ask_install "Welcome to HashNow!\\n\\nHashNow computes 70 hash algorithms for any file. Right-click any file in Finder to hash it instantly.\\n\\nThis will:\\n  • Copy HashNow to ~/.local/bin/\\n  • Add a Finder Quick Action (right-click menu)\\n\\nNo admin password required." || {
	exit 0
}

# ── Remove Gatekeeper quarantine ───────────────────────────────

xattr -d com.apple.quarantine "$BINARY" 2>/dev/null || true

# ── Install binary ─────────────────────────────────────────────

chmod +x "$BINARY"
mkdir -p "$INSTALL_DIR"
cp "$BINARY" "$INSTALL_DIR/HashNow"
chmod +x "$INSTALL_DIR/HashNow"

# Remove quarantine from the installed copy too
xattr -d com.apple.quarantine "$INSTALL_DIR/HashNow" 2>/dev/null || true

# ── Add to PATH if needed ──────────────────────────────────────

if [[ ":$PATH:" != *":$INSTALL_DIR:"* ]]; then
	SHELL_CONFIG="$HOME/.zshrc"

	if [ ! -f "$SHELL_CONFIG" ]; then
		# macOS defaults to zsh; create .zshrc if it doesn't exist
		touch "$SHELL_CONFIG"
	fi

	if ! grep -q '.local/bin' "$SHELL_CONFIG" 2>/dev/null; then
		{
			echo ''
			echo '# Added by HashNow installer'
			echo 'export PATH="$HOME/.local/bin:$PATH"'
		} >> "$SHELL_CONFIG"
	fi
	export PATH="$INSTALL_DIR:$PATH"
fi

# ── Install Finder Quick Action ────────────────────────────────
# --gui-install shows native macOS dialogs for the result (osascript)

"$INSTALL_DIR/HashNow" --gui-install
