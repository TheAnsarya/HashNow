using System.Runtime.Versioning;

namespace HashNow.Cli;

/// <summary>
/// Provides GUI message dialogs for user interaction in double-click mode.
/// </summary>
/// <remarks>
/// <para>
/// This class wraps Windows Forms MessageBox dialogs to provide a graphical
/// user interface when the application is run by double-clicking (no console).
/// </para>
/// <para>
/// Console prompts are still used when running from a command-line terminal.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
internal static class GuiDialogs {
	#region Constants

	private const string AppTitle = "HashNow";

	#endregion

	#region Public Methods

	/// <summary>
	/// Shows a Yes/No question dialog.
	/// </summary>
	/// <param name="message">The question to display.</param>
	/// <param name="title">Optional title (defaults to "HashNow").</param>
	/// <returns><see langword="true"/> if user clicked Yes; otherwise, <see langword="false"/>.</returns>
	public static bool AskYesNo(string message, string? title = null) {
		var result = MessageBox.Show(
			message,
			title ?? AppTitle,
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Question);

		return result == DialogResult.Yes;
	}

	/// <summary>
	/// Shows an information message dialog.
	/// </summary>
	/// <param name="message">The message to display.</param>
	/// <param name="title">Optional title (defaults to "HashNow").</param>
	public static void ShowInfo(string message, string? title = null) {
		MessageBox.Show(
			message,
			title ?? AppTitle,
			MessageBoxButtons.OK,
			MessageBoxIcon.Information);
	}

	/// <summary>
	/// Shows a success message dialog.
	/// </summary>
	/// <param name="message">The message to display.</param>
	/// <param name="title">Optional title (defaults to "HashNow").</param>
	public static void ShowSuccess(string message, string? title = null) {
		MessageBox.Show(
			message,
			title ?? AppTitle,
			MessageBoxButtons.OK,
			MessageBoxIcon.Information);
	}

	/// <summary>
	/// Shows an error message dialog.
	/// </summary>
	/// <param name="message">The error message to display.</param>
	/// <param name="title">Optional title (defaults to "HashNow").</param>
	public static void ShowError(string message, string? title = null) {
		MessageBox.Show(
			message,
			title ?? AppTitle,
			MessageBoxButtons.OK,
			MessageBoxIcon.Error);
	}

	/// <summary>
	/// Shows a warning message dialog.
	/// </summary>
	/// <param name="message">The warning message to display.</param>
	/// <param name="title">Optional title (defaults to "HashNow").</param>
	public static void ShowWarning(string message, string? title = null) {
		MessageBox.Show(
			message,
			title ?? AppTitle,
			MessageBoxButtons.OK,
			MessageBoxIcon.Warning);
	}

	/// <summary>
	/// Shows the installation prompts using GUI dialogs.
	/// </summary>
	/// <param name="isRunningAsAdmin">Whether the current process has admin rights.</param>
	/// <param name="isInstalled">Whether the context menu is already installed.</param>
	/// <param name="isCorrect">Whether the installed path matches the current executable.</param>
	/// <returns>
	/// A tuple indicating:
	/// <list type="bullet">
	///   <item><description><c>ShouldRestart</c>: True if user wants to restart as admin</description></item>
	///   <item><description><c>ShouldInstall</c>: True if user wants to install</description></item>
	///   <item><description><c>Cancelled</c>: True if user cancelled</description></item>
	/// </list>
	/// </returns>
	public static (bool ShouldRestart, bool ShouldInstall, bool Cancelled) ShowInstallPrompts(
		bool isRunningAsAdmin,
		bool isInstalled,
		bool isCorrect) {
		// Already installed correctly
		if (isInstalled && isCorrect) {
			ShowInfo(
				"HashNow is already installed!\n\n" +
				"Right-click any file in Windows Explorer and select\n" +
				"\"Hash this file now\" to compute hashes.",
				"HashNow - Already Installed");
			return (false, false, false);
		}

		// Not admin - ask to elevate
		if (!isRunningAsAdmin) {
			string message = isInstalled && !isCorrect
				? "HashNow context menu needs to be updated.\n" +
				  "The executable has moved since installation.\n\n" +
				  "Administrator privileges are required.\n" +
				  "Restart as Administrator?"
				: "HashNow is not installed in the context menu.\n\n" +
				  "Administrator privileges are required.\n" +
				  "Restart as Administrator?";

			bool restart = AskYesNo(message, "HashNow - Installation Required");
			return (restart, false, !restart);
		}

		// Has admin - ask to install
		string installMessage = isInstalled && !isCorrect
			? "HashNow context menu needs to be updated.\n" +
			  "The executable has moved since installation.\n\n" +
			  "Install the context menu now?"
			: "HashNow is not installed in the context menu.\n\n" +
			  "Install the context menu now?";

		bool install = AskYesNo(installMessage, "HashNow - Install Context Menu");
		return (false, install, !install);
	}

	/// <summary>
	/// Shows the installation result dialog.
	/// </summary>
	/// <param name="success">Whether installation succeeded.</param>
	/// <param name="errorMessage">Error message if failed.</param>
	public static void ShowInstallResult(bool success, string? errorMessage = null) {
		if (success) {
			ShowSuccess(
				"Context menu installed successfully!\n\n" +
				"Right-click any file in Windows Explorer and select\n" +
				"\"Hash this file now\" to compute all 58 hashes.\n\n" +
				"A .hashes.json file will be created next to the original.",
				"HashNow - Installation Complete");
		} else if (errorMessage?.Contains("Administrator") == true) {
			ShowError(
				"Administrator privileges required.\n\n" +
				"Please right-click HashNow.exe and select\n" +
				"'Run as administrator' to install the context menu.",
				"HashNow - Error");
		} else {
			ShowError(
				$"Error installing context menu:\n\n{errorMessage ?? "Unknown error"}",
				"HashNow - Error");
		}
	}

	#endregion
}
