namespace HashNow.Cli.Platform;

/// <summary>
/// Defines which context menu entries should be installed or removed.
/// </summary>
internal enum ContextMenuInstallMode {
	/// <summary>
	/// The full hashing context menu entry (all 70+ algorithms).
	/// </summary>
	Full,

	/// <summary>
	/// The basic hashing context menu entry (CRC32, MD5, SHA-1, SHA-256, SHA-512).
	/// </summary>
	Basic,

	/// <summary>
	/// Both full and basic context menu entries.
	/// </summary>
	Both
}
