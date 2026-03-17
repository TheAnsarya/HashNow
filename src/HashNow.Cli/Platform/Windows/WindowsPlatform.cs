using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using HashNow.Core;
using Microsoft.Win32;

namespace HashNow.Cli.Platform.Windows;

/// <summary>
/// Windows platform integration using Registry-based Explorer context menu and WinForms dialogs.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class WindowsPlatform : IPlatformIntegration {
	#region Constants

	/// <summary>
	/// The registry key path under HKEY_CLASSES_ROOT for the context menu entry.
	/// </summary>
	private const string RegistryKeyPath = @"*\shell\HashNow";

	/// <summary>
	/// The text displayed in the context menu.
	/// </summary>
	private const string MenuText = "Hash this file now";

	/// <summary>
	/// Environment variable name used to detect Explorer-launched instances.
	/// </summary>
	private const string ExplorerEnvVar = "HASHNOW_EXPLORER";

	/// <summary>
	/// Time threshold in milliseconds before showing progress UI.
	/// </summary>
	private const long ProgressUiThresholdMs = 0;

	private const string AppTitle = "HashNow";

	#endregion

	#region P/Invoke

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool AttachConsole(int dwProcessId);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool AllocConsole();

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool FreeConsole();

	private const int ATTACH_PARENT_PROCESS = -1;

	#endregion

	#region IPlatformIntegration Properties

	/// <inheritdoc/>
	public string PlatformName => "Windows";

	/// <inheritdoc/>
	public bool IsElevated {
		get {
			try {
				using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
				var principal = new System.Security.Principal.WindowsPrincipal(identity);
				return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
			} catch {
				return false;
			}
		}
	}

	/// <inheritdoc/>
	public bool SupportsGuiProgress => true;

	#endregion

	#region Initialization

	/// <inheritdoc/>
	public void Initialize(string[] args) {
		// Initialize WinForms for proper MessageBox rendering and DPI scaling
		Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		// Attach to console if running from command line
		if (args.Length > 0 || !DetectDoubleClickLaunch()) {
			AttachConsole(ATTACH_PARENT_PROCESS);
			RedirectConsoleStreams();
		}
	}

	/// <summary>
	/// Redirects .NET Console streams to the attached console.
	/// </summary>
	private static void RedirectConsoleStreams() {
		try {
			Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
			Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
			Console.SetIn(new StreamReader(Console.OpenStandardInput()));
		} catch {
			// If stream redirection fails, console output won't work but GUI still will
		}
	}

	#endregion

	#region Context Menu Install/Uninstall

	/// <inheritdoc/>
	public bool Install(string executablePath) {
		using var key = Registry.ClassesRoot.CreateSubKey(RegistryKeyPath);
		if (key is null) {
			throw new InvalidOperationException(
				"Failed to create registry key. Ensure the application is running with administrator privileges.");
		}

		key.SetValue("", MenuText);
		key.SetValue("Icon", $"\"{executablePath}\",0");

		using var commandKey = key.CreateSubKey("command");
		if (commandKey is null) {
			throw new InvalidOperationException(
				"Failed to create command registry key. Ensure the application is running with administrator privileges.");
		}

		commandKey.SetValue("", $"\"{executablePath}\" \"%1\"");
		return true;
	}

	/// <inheritdoc/>
	public bool Uninstall() {
		try {
			Registry.ClassesRoot.DeleteSubKeyTree(RegistryKeyPath, throwOnMissingSubKey: false);
			return true;
		} catch (ArgumentException) {
			return true; // Already uninstalled
		}
	}

	/// <inheritdoc/>
	public bool IsInstalled() {
		using var key = Registry.ClassesRoot.OpenSubKey(RegistryKeyPath);
		return key is not null;
	}

	/// <inheritdoc/>
	public bool IsInstalledCorrectly(string executablePath) {
		var command = GetInstalledCommand();
		if (command is null) return false;
		return command.Contains(executablePath, StringComparison.OrdinalIgnoreCase);
	}

	/// <inheritdoc/>
	public string? GetInstalledCommand() {
		using var key = Registry.ClassesRoot.OpenSubKey(RegistryKeyPath + @"\command");
		return key?.GetValue("") as string;
	}

	#endregion

	#region Dialogs

	/// <summary>
	/// Shows a MessageBox using a topmost hidden owner form.
	/// </summary>
	private static DialogResult ShowForeground(
		string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon) {
		using var owner = new Form {
			TopMost = true,
			ShowInTaskbar = false,
			FormBorderStyle = FormBorderStyle.None,
			StartPosition = FormStartPosition.CenterScreen,
			Size = Size.Empty,
		};
		owner.Handle.ToString(); // Force handle creation
		return MessageBox.Show(owner, message, title, buttons, icon);
	}

	/// <inheritdoc/>
	public bool AskYesNo(string title, string message) {
		var result = ShowForeground(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		return result == DialogResult.Yes;
	}

	/// <inheritdoc/>
	public void ShowInfo(string title, string message) =>
		ShowForeground(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

	/// <inheritdoc/>
	public void ShowError(string title, string message) =>
		ShowForeground(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

	/// <inheritdoc/>
	public void ShowSuccess(string title, string message) =>
		ShowForeground(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

	/// <inheritdoc/>
	public (bool ShouldRestart, bool ShouldInstall, bool Cancelled) ShowInstallPrompts(
		bool isInstalled, bool isCorrect) {
		if (isInstalled && isCorrect) {
			ShowInfo("HashNow - Already Installed",
				"HashNow is already installed!\n\n" +
				"Right-click any file in Windows Explorer and select\n" +
				"\"Hash this file now\" to compute hashes.");
			return (false, false, false);
		}

		if (!IsElevated) {
			string message = isInstalled && !isCorrect
				? "HashNow context menu needs to be updated.\n" +
				  "The executable has moved since installation.\n\n" +
				  "Administrator privileges are required.\n" +
				  "Restart as Administrator?"
				: "HashNow is not installed in the context menu.\n\n" +
				  "Administrator privileges are required.\n" +
				  "Restart as Administrator?";

			bool restart = AskYesNo("HashNow - Installation Required", message);
			return (restart, false, !restart);
		}

		string installMsg = isInstalled && !isCorrect
			? "HashNow context menu needs to be updated.\n" +
			  "The executable has moved since installation.\n\n" +
			  "Install the context menu now?"
			: "HashNow is not installed in the context menu.\n\n" +
			  "Install the context menu now?";

		bool install = AskYesNo("HashNow - Install Context Menu", installMsg);
		return (false, install, !install);
	}

	/// <inheritdoc/>
	public void ShowInstallResult(bool success, string? errorMessage = null) {
		if (success) {
			ShowSuccess("HashNow - Installation Complete",
				"Context menu installed successfully!\n\n" +
				"Right-click any file in Windows Explorer and select\n" +
				"\"Hash this file now\" to compute all 70 hashes.\n\n" +
				"A .hashes.json file will be created next to the original.");
		} else if (errorMessage?.Contains("Administrator") == true) {
			ShowError("HashNow - Error",
				"Administrator privileges required.\n\n" +
				"Please right-click HashNow.exe and select\n" +
				"'Run as administrator' to install the context menu.");
		} else {
			ShowError("HashNow - Error",
				$"Error installing context menu:\n\n{errorMessage ?? "Unknown error"}");
		}
	}

	#endregion

	#region Progress

	/// <inheritdoc/>
	public async Task<FileHashResult?> HashFileWithProgress(string filePath, CancellationToken ct = default) {
		var fileInfo = new FileInfo(filePath);
		var estimatedMs = FileHasher.EstimateHashDurationMs(fileInfo.Length);

		if (estimatedMs <= ProgressUiThresholdMs) {
			return await FileHasher.HashFileAsync(filePath, cancellationToken: ct);
		}

		// Show WinForms progress dialog
		FileHashResult? result = null;
		var wasCancelled = false;

		var success = await ProgressDialog.ShowDialogAsync(
			filePath,
			async (progressCallback, cancellationToken) => {
				try {
					result = await FileHasher.HashFileAsync(filePath, progressCallback, cancellationToken);
				} catch (OperationCanceledException) {
					wasCancelled = true;
					throw;
				}
			});

		return success && !wasCancelled ? result : null;
	}

	#endregion

	#region Elevation

	/// <inheritdoc/>
	public bool RelaunchElevated(string[] args) {
		try {
			var exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
			if (string.IsNullOrEmpty(exePath)) {
				ShowError(AppTitle, "Could not determine executable path.");
				return false;
			}

			var startInfo = new ProcessStartInfo {
				FileName = exePath,
				Arguments = "--gui-install",
				UseShellExecute = true,
				Verb = "runas"
			};

			Process.Start(startInfo);
			Environment.Exit(0);
			return true;
		} catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223) {
			return false; // User cancelled UAC
		} catch (Exception ex) {
			ShowError(AppTitle, $"Error requesting elevation: {ex.Message}");
			return false;
		}
	}

	#endregion

	#region Launch Detection

	/// <inheritdoc/>
	public bool DetectDoubleClickLaunch() {
		try {
			using var currentProcess = Process.GetCurrentProcess();
			using var parentProcess = GetParentProcess(currentProcess);

			if (parentProcess is not null) {
				var parentName = parentProcess.ProcessName.ToLowerInvariant();
				if (parentName == "explorer") return true;
				if (parentName is "cmd" or "powershell" or "pwsh" or "windowsterminal"
					or "conhost" or "code" or "devenv") {
					return false;
				}
			}

			try {
				if (Console.IsInputRedirected) return false;
			} catch (IOException) {
				return true;
			}

			return true;
		} catch {
			return true;
		}
	}

	/// <inheritdoc/>
	public bool DetectFileManagerLaunch() {
		if (Environment.GetEnvironmentVariable(ExplorerEnvVar) == "1") return true;
		return Console.IsInputRedirected;
	}

	/// <summary>
	/// Gets the parent process using WMI.
	/// </summary>
	private static Process? GetParentProcess(Process process) {
		try {
			using var query = new System.Management.ManagementObjectSearcher(
				$"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {process.Id}");
			var result = query.Get().Cast<System.Management.ManagementObject>().FirstOrDefault();
			if (result?["ParentProcessId"] is uint parentId) {
				return Process.GetProcessById((int)parentId);
			}
		} catch {
			// Ignore errors in parent process detection
		}
		return null;
	}

	#endregion

	#region Status

	/// <inheritdoc/>
	public IReadOnlyList<string> GetStatusDetails() {
		var details = new List<string>();
		bool isInstalled = IsInstalled();

		var exePath = Environment.ProcessPath ?? "Unknown";
		bool isCorrect = IsInstalledCorrectly(exePath);

		if (isInstalled && isCorrect) {
			details.Add("✓ Context menu: Installed (current)");
		} else if (isInstalled) {
			details.Add("⚠ Context menu: Installed (outdated path)");
			details.Add("  The executable has moved. Run --install to update.");
		} else {
			details.Add("✗ Context menu: Not installed");
			details.Add("  Run --install or double-click the exe to install.");
		}

		details.Add($"Executable: {exePath}");
		details.Add($"Admin:      {(IsElevated ? "Yes" : "No")}");

		var command = GetInstalledCommand();
		if (command is not null) {
			details.Add($"Registered: {command}");
		}

		return details;
	}

	#endregion
}
