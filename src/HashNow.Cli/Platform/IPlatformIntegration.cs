using HashNow.Core;

namespace HashNow.Cli.Platform;

/// <summary>
/// Defines platform-specific operations for file manager context menu integration and user interaction.
/// </summary>
/// <remarks>
/// <para>
/// Each platform (Windows, Linux, macOS) implements this interface to provide:
/// </para>
/// <list type="bullet">
///   <item><description>File manager context menu installation/uninstallation</description></item>
///   <item><description>User interaction dialogs (GUI or terminal-based)</description></item>
///   <item><description>Progress display for long-running hash operations</description></item>
///   <item><description>Privilege elevation for operations that require it</description></item>
/// </list>
/// </remarks>
internal interface IPlatformIntegration {
	/// <summary>
	/// Gets the platform display name (e.g., "Windows", "Linux (GNOME)", "macOS").
	/// </summary>
	string PlatformName { get; }

	/// <summary>
	/// Gets whether the current process has elevated/administrator privileges.
	/// </summary>
	bool IsElevated { get; }

	/// <summary>
	/// Gets whether the platform supports GUI progress dialogs.
	/// </summary>
	bool SupportsGuiProgress { get; }

	/// <summary>
	/// Installs context menu / file manager integration for the given executable.
	/// </summary>
	/// <param name="executablePath">Path to the HashNow executable.</param>
	/// <returns><see langword="true"/> if installation succeeded.</returns>
	bool Install(string executablePath);

	/// <summary>
	/// Removes context menu / file manager integration.
	/// </summary>
	/// <returns><see langword="true"/> if uninstallation succeeded.</returns>
	bool Uninstall();

	/// <summary>
	/// Checks if context menu / file manager integration is currently installed.
	/// </summary>
	/// <returns><see langword="true"/> if installed.</returns>
	bool IsInstalled();

	/// <summary>
	/// Checks if the installed integration points to the correct executable.
	/// </summary>
	/// <param name="executablePath">Expected path to the HashNow executable.</param>
	/// <returns><see langword="true"/> if installed and pointing to the correct path.</returns>
	bool IsInstalledCorrectly(string executablePath);

	/// <summary>
	/// Gets the currently registered command, or <see langword="null"/> if not installed.
	/// </summary>
	string? GetInstalledCommand();

	/// <summary>
	/// Shows a yes/no dialog using the platform-appropriate method (GUI or terminal).
	/// </summary>
	/// <param name="title">Dialog title.</param>
	/// <param name="message">Dialog message.</param>
	/// <returns><see langword="true"/> if user chose yes.</returns>
	bool AskYesNo(string title, string message);

	/// <summary>
	/// Shows an information message using the platform-appropriate method.
	/// </summary>
	/// <param name="title">Dialog title.</param>
	/// <param name="message">Message text.</param>
	void ShowInfo(string title, string message);

	/// <summary>
	/// Shows an error message using the platform-appropriate method.
	/// </summary>
	/// <param name="title">Dialog title.</param>
	/// <param name="message">Error message text.</param>
	void ShowError(string title, string message);

	/// <summary>
	/// Shows a success message using the platform-appropriate method.
	/// </summary>
	/// <param name="title">Dialog title.</param>
	/// <param name="message">Success message text.</param>
	void ShowSuccess(string title, string message);

	/// <summary>
	/// Runs a hashing operation with platform-appropriate progress display.
	/// </summary>
	/// <param name="filePath">Path to the file to hash.</param>
	/// <param name="ct">Cancellation token.</param>
	/// <returns>The hash result, or <see langword="null"/> if cancelled.</returns>
	Task<FileHashResult?> HashFileWithProgress(string filePath, CancellationToken ct = default);

	/// <summary>
	/// Relaunches the current process with elevated privileges.
	/// </summary>
	/// <param name="args">Arguments for the relaunched process.</param>
	/// <returns><see langword="true"/> if relaunch was initiated.</returns>
	bool RelaunchElevated(string[] args);

	/// <summary>
	/// Detects if the application was launched by double-clicking (vs command line).
	/// </summary>
	/// <returns><see langword="true"/> if launched by double-click or file manager.</returns>
	bool DetectDoubleClickLaunch();

	/// <summary>
	/// Detects if the application was launched from the file manager context menu.
	/// </summary>
	/// <returns><see langword="true"/> if launched from file manager integration.</returns>
	bool DetectFileManagerLaunch();

	/// <summary>
	/// Performs platform-specific initialization (e.g., WinForms init, console attachment).
	/// </summary>
	/// <param name="args">Command-line arguments.</param>
	void Initialize(string[] args);

	/// <summary>
	/// Gets install prompt information for the platform.
	/// </summary>
	/// <param name="isInstalled">Whether currently installed.</param>
	/// <param name="isCorrect">Whether installed correctly.</param>
	/// <returns>Tuple: (ShouldRestart, ShouldInstall, Cancelled).</returns>
	(bool ShouldRestart, bool ShouldInstall, bool Cancelled) ShowInstallPrompts(
		bool isInstalled, bool isCorrect);

	/// <summary>
	/// Shows the installation result dialog.
	/// </summary>
	/// <param name="success">Whether installation succeeded.</param>
	/// <param name="errorMessage">Error message if failed.</param>
	void ShowInstallResult(bool success, string? errorMessage = null);

	/// <summary>
	/// Gets details about installed file manager integrations (for --status command).
	/// </summary>
	/// <returns>List of status lines describing the installation state.</returns>
	IReadOnlyList<string> GetStatusDetails();
}
