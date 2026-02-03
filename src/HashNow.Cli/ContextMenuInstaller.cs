using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace HashNow.Cli;

/// <summary>
/// Handles Windows Explorer context menu registration via the Windows Registry.
/// </summary>
/// <remarks>
/// <para>
/// This class provides methods to install, uninstall, and check the status of the
/// "Hash this file now" context menu entry that appears when right-clicking files
/// in Windows Explorer.
/// </para>
/// <para>
/// <strong>How it works:</strong>
/// </para>
/// <list type="number">
///   <item>
///     <description>
///       Creates a registry key at <c>HKEY_CLASSES_ROOT\*\shell\HashNow</c>.
///       The <c>*</c> wildcard means this applies to all file types.
///     </description>
///   </item>
///   <item>
///     <description>
///       Sets the default value to the menu text ("Hash this file now").
///     </description>
///   </item>
///   <item>
///     <description>
///       Sets an Icon value pointing to the HashNow executable for visual identification.
///     </description>
///   </item>
///   <item>
///     <description>
///       Creates a <c>command</c> subkey with the command line to execute when clicked.
///       The <c>%1</c> placeholder is replaced by Windows with the selected file path.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Security Requirements:</strong>
/// </para>
/// <para>
/// Modifying <c>HKEY_CLASSES_ROOT</c> requires administrator privileges. The application
/// will need to be run elevated (Run as Administrator) for install/uninstall operations.
/// </para>
/// <para>
/// <strong>Registry Structure:</strong>
/// </para>
/// <code>
/// HKEY_CLASSES_ROOT
/// └── *
///     └── shell
///         └── HashNow
///             ├── (Default) = "Hash this file now"
///             ├── Icon = "C:\path\to\HashNow.exe",0
///             └── command
///                 └── (Default) = "C:\path\to\HashNow.exe" "%1"
/// </code>
/// </remarks>
/// <example>
/// <code>
/// // Check if installed
/// if (!ContextMenuInstaller.IsInstalled()) {
///     // Install the context menu
///     ContextMenuInstaller.Install();
///     Console.WriteLine("Context menu installed!");
/// }
///
/// // Later, to remove:
/// ContextMenuInstaller.Uninstall();
/// </code>
/// </example>
[SupportedOSPlatform("windows")]
internal static class ContextMenuInstaller {
	#region Constants

	/// <summary>
	/// The registry key path under HKEY_CLASSES_ROOT for the context menu entry.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The path <c>*\shell\HashNow</c> means:
	/// </para>
	/// <list type="bullet">
	///   <item><description><c>*</c> - Applies to all file types (wildcard)</description></item>
	///   <item><description><c>shell</c> - The shell extensions container</description></item>
	///   <item><description><c>HashNow</c> - Our application's unique identifier</description></item>
	/// </list>
	/// </remarks>
	private const string RegistryKeyPath = @"*\shell\HashNow";

	/// <summary>
	/// The text displayed in the context menu.
	/// </summary>
	/// <remarks>
	/// This appears when the user right-clicks on any file in Windows Explorer.
	/// Keep it short, clear, and action-oriented.
	/// </remarks>
	private const string MenuText = "Hash this file now";

	#endregion

	#region Public Methods

	/// <summary>
	/// Installs the HashNow context menu entry for all file types.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// Thrown when the registry key cannot be created, typically due to insufficient privileges.
	/// </exception>
	/// <exception cref="UnauthorizedAccessException">
	/// Thrown when the application lacks administrator privileges.
	/// </exception>
	/// <remarks>
	/// <para>
	/// <strong>Administrator Privileges Required:</strong>
	/// This method modifies <c>HKEY_CLASSES_ROOT</c>, which requires the application
	/// to run with elevated privileges. If not elevated, an exception will be thrown.
	/// </para>
	/// <para>
	/// <strong>Idempotent Operation:</strong>
	/// This method is safe to call multiple times. If the context menu is already
	/// installed, it will be updated with the current executable path.
	/// </para>
	/// <para>
	/// <strong>What Gets Created:</strong>
	/// </para>
	/// <list type="bullet">
	///   <item><description>Registry key: <c>HKCR\*\shell\HashNow</c></description></item>
	///   <item><description>Default value: "Hash this file now"</description></item>
	///   <item><description>Icon value: Path to executable with icon index 0</description></item>
	///   <item><description>Subkey: <c>command</c> with the command to execute</description></item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <code>
	/// try {
	///     ContextMenuInstaller.Install();
	///     Console.WriteLine("✓ Context menu installed successfully!");
	/// } catch (UnauthorizedAccessException) {
	///     Console.WriteLine("✗ Please run as administrator.");
	/// }
	/// </code>
	/// </example>
	public static void Install() {
		// Get the path to the current executable for command registration
		var exePath = GetExecutablePath();

		// Create the context menu entry under HKEY_CLASSES_ROOT\*\shell\HashNow
		// CreateSubKey will create if missing or open if exists
		using var key = Registry.ClassesRoot.CreateSubKey(RegistryKeyPath);
		if (key == null) {
			throw new InvalidOperationException(
				"Failed to create registry key. Ensure the application is running with administrator privileges.");
		}

		// Set the default value - this is the menu item text shown in Explorer
		key.SetValue("", MenuText);

		// Set the icon displayed next to the menu item
		// Using the exe itself as icon source (index 0 = first icon resource)
		key.SetValue("Icon", $"\"{exePath}\",0");

		// Create the command subkey that tells Windows what to run when clicked
		using var commandKey = key.CreateSubKey("command");
		if (commandKey == null) {
			throw new InvalidOperationException(
				"Failed to create command registry key. Ensure the application is running with administrator privileges.");
		}

		// Set the command to run when the context menu item is clicked
		// %1 is replaced by Windows with the path to the selected file (quoted for paths with spaces)
		commandKey.SetValue("", $"\"{exePath}\" \"%1\"");
	}

	/// <summary>
	/// Removes the HashNow context menu entry from Windows Explorer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <strong>Administrator Privileges Required:</strong>
	/// Like <see cref="Install"/>, this method requires elevated privileges.
	/// </para>
	/// <para>
	/// <strong>Safe to Call:</strong>
	/// This method is safe to call even if the context menu is not installed.
	/// It will silently succeed in that case.
	/// </para>
	/// <para>
	/// <strong>Complete Removal:</strong>
	/// Uses <see cref="RegistryKey.DeleteSubKeyTree"/> to remove the key and all
	/// its subkeys (including the <c>command</c> subkey).
	/// </para>
	/// </remarks>
	/// <example>
	/// <code>
	/// ContextMenuInstaller.Uninstall();
	/// Console.WriteLine("Context menu removed.");
	/// </code>
	/// </example>
	public static void Uninstall() {
		try {
			// DeleteSubKeyTree removes the key and all subkeys
			// throwOnMissingSubKey: false prevents exception if already uninstalled
			Registry.ClassesRoot.DeleteSubKeyTree(RegistryKeyPath, throwOnMissingSubKey: false);
		} catch (ArgumentException) {
			// Key doesn't exist - this is fine, already uninstalled
			// ArgumentException is thrown on some Windows versions for missing keys
		}
	}

	/// <summary>
	/// Checks whether the HashNow context menu is currently installed.
	/// </summary>
	/// <returns>
	/// <see langword="true"/> if the context menu entry exists in the registry;
	/// otherwise, <see langword="false"/>.
	/// </returns>
	/// <remarks>
	/// <para>
	/// This method only checks if the registry key exists, not whether the
	/// command points to a valid executable. The executable may have been
	/// moved or deleted since installation.
	/// </para>
	/// <para>
	/// <strong>No Elevation Required:</strong>
	/// Unlike <see cref="Install"/> and <see cref="Uninstall"/>, this method
	/// only reads the registry and does not require administrator privileges.
	/// </para>
	/// </remarks>
	/// <example>
	/// <code>
	/// if (ContextMenuInstaller.IsInstalled()) {
	///     Console.WriteLine("HashNow is installed in the context menu.");
	/// } else {
	///     Console.WriteLine("HashNow is NOT installed in the context menu.");
	/// }
	/// </code>
	/// </example>
	public static bool IsInstalled() {
		// OpenSubKey returns null if the key doesn't exist
		// Using 'using' ensures the key is properly disposed if found
		using var key = Registry.ClassesRoot.OpenSubKey(RegistryKeyPath);
		return key != null;
	}

	/// <summary>
	/// Gets the currently registered command path from the context menu.
	/// </summary>
	/// <returns>
	/// The command line registered for the context menu, or <see langword="null"/>
	/// if not installed or the command value is missing.
	/// </returns>
	/// <remarks>
	/// This can be used to check if the installed command points to the current
	/// executable, which is useful for detecting when a reinstall is needed.
	/// </remarks>
	/// <example>
	/// <code>
	/// var command = ContextMenuInstaller.GetInstalledCommand();
	/// if (command != null) {
	///     Console.WriteLine($"Registered command: {command}");
	/// }
	/// </code>
	/// </example>
	public static string? GetInstalledCommand() {
		using var key = Registry.ClassesRoot.OpenSubKey(RegistryKeyPath + @"\command");
		return key?.GetValue("") as string;
	}

	/// <summary>
	/// Checks if the installed context menu points to the current executable.
	/// </summary>
	/// <returns>
	/// <see langword="true"/> if installed and pointing to current exe;
	/// <see langword="false"/> if not installed or pointing elsewhere.
	/// </returns>
	/// <remarks>
	/// This is useful for detecting when the application has been moved to a
	/// different location and needs to be reinstalled.
	/// </remarks>
	/// <example>
	/// <code>
	/// if (!ContextMenuInstaller.IsInstalledCorrectly()) {
	///     Console.WriteLine("Context menu needs to be reinstalled.");
	///     ContextMenuInstaller.Install();
	/// }
	/// </code>
	/// </example>
	public static bool IsInstalledCorrectly() {
		var command = GetInstalledCommand();
		if (command == null) return false;

		// Check if the command contains the current executable path
		var currentExe = GetExecutablePath();
		return command.Contains(currentExe, StringComparison.OrdinalIgnoreCase);
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Gets the full path to the currently running executable.
	/// </summary>
	/// <returns>The full path to the HashNow executable.</returns>
	/// <remarks>
	/// <para>
	/// This method tries multiple approaches to get the executable path:
	/// </para>
	/// <list type="number">
	///   <item>
	///     <description>
	///       <see cref="Environment.ProcessPath"/> - .NET 6+ recommended approach
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///       <see cref="Process.MainModule"/> - Process-based fallback
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///       <see cref="AppContext.BaseDirectory"/> - Last resort (may not be exact)
	///     </description>
	///   </item>
	/// </list>
	/// </remarks>
	private static string GetExecutablePath() {
		// Primary: .NET 6+ recommended approach
		var exePath = Environment.ProcessPath;

		// Fallback 1: Process-based approach
		if (string.IsNullOrEmpty(exePath)) {
			exePath = Process.GetCurrentProcess().MainModule?.FileName;
		}

		// Fallback 2: Derive from base directory (may not be exact for single-file apps)
		if (string.IsNullOrEmpty(exePath)) {
			exePath = Path.Combine(AppContext.BaseDirectory, "HashNow.exe");
		}

		return exePath;
	}

	#endregion
}
