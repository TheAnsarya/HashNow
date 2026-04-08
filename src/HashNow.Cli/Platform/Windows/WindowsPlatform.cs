using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using HashNow.Cli.Platform;
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
	/// The registry key path for the full context menu entry.
	/// </summary>
	private const string FullRegistryKeyPath = @"*\shell\HashNowFull";

	/// <summary>
	/// The registry key path for the basic context menu entry.
	/// </summary>
	private const string BasicRegistryKeyPath = @"*\shell\HashNowBasic";

	/// <summary>
	/// Legacy key path used by pre-dual-menu versions.
	/// </summary>
	private const string LegacyRegistryKeyPath = @"*\shell\HashNow";

	/// <summary>
	/// The text displayed in the context menu.
	/// </summary>
	private const string FullMenuText = "Hash this file now (full)";

	/// <summary>
	/// Label for the basic context menu item.
	/// </summary>
	private const string BasicMenuText = "Hash this file now";

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
	public bool Install(string executablePath, ContextMenuInstallMode mode = ContextMenuInstallMode.Both) {
		// Always remove legacy single-entry key from older versions to prevent duplicate menu items.
		DeleteMenuEntry(LegacyRegistryKeyPath);

		if (mode is ContextMenuInstallMode.Full or ContextMenuInstallMode.Both) {
			InstallMenuEntry(FullRegistryKeyPath, FullMenuText, $"\"{executablePath}\" \"%1\"", executablePath);
		}

		if (mode is ContextMenuInstallMode.Basic or ContextMenuInstallMode.Both) {
			InstallMenuEntry(BasicRegistryKeyPath, BasicMenuText, $"\"{executablePath}\" --basic \"%1\"", executablePath);
		}

		if (mode == ContextMenuInstallMode.Full) {
			DeleteMenuEntry(BasicRegistryKeyPath);
		}

		if (mode == ContextMenuInstallMode.Basic) {
			DeleteMenuEntry(FullRegistryKeyPath);
		}

		return true;
	}

	/// <inheritdoc/>
	public bool Uninstall(ContextMenuInstallMode mode = ContextMenuInstallMode.Both) {
		DeleteMenuEntry(LegacyRegistryKeyPath);

		if (mode is ContextMenuInstallMode.Full or ContextMenuInstallMode.Both) {
			DeleteMenuEntry(FullRegistryKeyPath);
		}

		if (mode is ContextMenuInstallMode.Basic or ContextMenuInstallMode.Both) {
			DeleteMenuEntry(BasicRegistryKeyPath);
		}

		return true;
	}

	/// <inheritdoc/>
	public bool IsInstalled() {
		return IsEntryInstalled(FullRegistryKeyPath)
			|| IsEntryInstalled(BasicRegistryKeyPath)
			|| IsEntryInstalled(LegacyRegistryKeyPath);
	}

	/// <inheritdoc/>
	public bool IsInstalledCorrectly(string executablePath) {
		var fullInstalled = IsEntryInstalled(FullRegistryKeyPath);
		var basicInstalled = IsEntryInstalled(BasicRegistryKeyPath);

		if (!fullInstalled && !basicInstalled) {
			return false;
		}

		var fullCorrect = !fullInstalled || IsEntryCorrect(
			FullRegistryKeyPath,
			executablePath,
			mustContainBasicSwitch: false);

		var basicCorrect = !basicInstalled || IsEntryCorrect(
			BasicRegistryKeyPath,
			executablePath,
			mustContainBasicSwitch: true);

		return fullCorrect && basicCorrect;
	}

	/// <inheritdoc/>
	public string? GetInstalledCommand() {
		return GetEntryCommand(FullRegistryKeyPath) ?? GetEntryCommand(BasicRegistryKeyPath);
	}

	/// <summary>
	/// Gets the installed command for the full context menu entry.
	/// </summary>
	private static string? GetInstalledFullCommand() => GetEntryCommand(FullRegistryKeyPath);

	/// <summary>
	/// Gets the installed command for the basic context menu entry.
	/// </summary>
	private static string? GetInstalledBasicCommand() => GetEntryCommand(BasicRegistryKeyPath);

	/// <summary>
	/// Gets the command registered under a specific registry entry.
	/// </summary>
	private static string? GetEntryCommand(string registryPath) {
		using var key = Registry.ClassesRoot.OpenSubKey(registryPath + @"\command");
		return key?.GetValue("") as string;
	}

	/// <summary>
	/// Checks whether a specific context menu entry is installed.
	/// </summary>
	private static bool IsEntryInstalled(string registryPath) {
		using var key = Registry.ClassesRoot.OpenSubKey(registryPath);
		return key is not null;
	}

	/// <summary>
	/// Validates a specific context menu command entry.
	/// </summary>
	private static bool IsEntryCorrect(string registryPath, string executablePath, bool mustContainBasicSwitch) {
		var command = GetEntryCommand(registryPath);
		if (string.IsNullOrWhiteSpace(command)) {
			return false;
		}

		var hasExe = command.Contains(executablePath, StringComparison.OrdinalIgnoreCase);
		var hasBasicSwitch = command.Contains(" --basic ", StringComparison.OrdinalIgnoreCase)
			|| command.Contains("\" --basic \"", StringComparison.OrdinalIgnoreCase)
			|| command.Contains(" --basic\"", StringComparison.OrdinalIgnoreCase);

		return mustContainBasicSwitch ? hasExe && hasBasicSwitch : hasExe && !hasBasicSwitch;
	}

	/// <summary>
	/// Creates or updates a context menu entry.
	/// </summary>
	private static void InstallMenuEntry(string registryPath, string menuText, string command, string executablePath) {
		using var key = Registry.ClassesRoot.CreateSubKey(registryPath);
		if (key is null) {
			throw new InvalidOperationException(
				"Failed to create registry key. Ensure the application is running with administrator privileges.");
		}

		key.SetValue("", menuText);
		key.SetValue("Icon", $"\"{executablePath}\",0");

		using var commandKey = key.CreateSubKey("command");
		if (commandKey is null) {
			throw new InvalidOperationException(
				"Failed to create command registry key. Ensure the application is running with administrator privileges.");
		}

		commandKey.SetValue("", command);
	}

	/// <summary>
	/// Deletes a context menu entry if it exists.
	/// </summary>
	private static void DeleteMenuEntry(string registryPath) {
		try {
			Registry.ClassesRoot.DeleteSubKeyTree(registryPath, throwOnMissingSubKey: false);
		} catch (ArgumentException) {
			// Already removed
		}
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
	public (bool ShouldRestart, bool ShouldInstall, ContextMenuInstallMode InstallMode, bool Cancelled) ShowInstallPrompts(
		bool isInstalled, bool isCorrect) {
		if (isInstalled && isCorrect) {
			ShowInfo("HashNow - Already Installed",
				"HashNow is already installed!\n\n" +
				"Right-click any file in Windows Explorer and select\n" +
				"\"Hash this file now\" or\n" +
				"\"Hash this file now (full)\".");
			return (false, false, ContextMenuInstallMode.Both, false);
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
			return (restart, false, ContextMenuInstallMode.Both, !restart);
		}

		string installMsg = isInstalled && !isCorrect
			? "HashNow context menu needs to be updated.\n" +
			  "The executable has moved since installation.\n\n" +
			  "Install the context menu now?"
			: "HashNow is not installed in the context menu.\n\n" +
			  "Install the context menu now?";

		bool install = AskYesNo("HashNow - Install Context Menu", installMsg);
		if (!install) {
			return (false, false, ContextMenuInstallMode.Both, true);
		}

		var (confirmed, mode) = ShowInstallModeSelectionDialog();
		return (false, confirmed, mode, !confirmed);
	}

	/// <summary>
	/// Shows a GUI dialog to choose which context menu entries to install.
	/// </summary>
	private static (bool Confirmed, ContextMenuInstallMode Mode) ShowInstallModeSelectionDialog() {
		using var form = new Form {
			Text = "HashNow - Choose Context Menu Mode",
			StartPosition = FormStartPosition.CenterScreen,
			FormBorderStyle = FormBorderStyle.FixedDialog,
			MaximizeBox = false,
			MinimizeBox = false,
			ShowInTaskbar = false,
			TopMost = true,
			ClientSize = new Size(500, 170)
		};

		var label = new Label {
			AutoSize = false,
			Location = new Point(16, 14),
			Size = new Size(468, 56),
			Text = "Choose which HashNow context menu items to install:\n\nBasic includes: CRC32, MD5, SHA-1, SHA-256, SHA-512"
		};

		var combo = new ComboBox {
			DropDownStyle = ComboBoxStyle.DropDownList,
			Location = new Point(16, 78),
			Size = new Size(468, 28)
		};

		combo.Items.Add("Both (basic + full)");
		combo.Items.Add("Basic only");
		combo.Items.Add("Full only");
		combo.SelectedIndex = 0;

		var okButton = new Button {
			Text = "Install",
			DialogResult = DialogResult.OK,
			Location = new Point(316, 124),
			Size = new Size(80, 28)
		};

		var cancelButton = new Button {
			Text = "Cancel",
			DialogResult = DialogResult.Cancel,
			Location = new Point(404, 124),
			Size = new Size(80, 28)
		};

		form.Controls.Add(label);
		form.Controls.Add(combo);
		form.Controls.Add(okButton);
		form.Controls.Add(cancelButton);
		form.AcceptButton = okButton;
		form.CancelButton = cancelButton;

		var result = form.ShowDialog();
		if (result != DialogResult.OK) {
			return (false, ContextMenuInstallMode.Both);
		}

		var mode = combo.SelectedIndex switch {
			1 => ContextMenuInstallMode.Basic,
			2 => ContextMenuInstallMode.Full,
			_ => ContextMenuInstallMode.Both
		};

		return (true, mode);
	}

	/// <inheritdoc/>
	public (bool Confirmed, ContextMenuInstallMode InstallMode) PromptInstallModeGui() {
		return ShowInstallModeSelectionDialog();
	}

	/// <inheritdoc/>
	public void ShowInstallResult(bool success, string? errorMessage = null) {
		if (success) {
			ShowSuccess("HashNow - Installation Complete",
				"Context menu setup completed successfully!\n\n" +
				"Right-click any file in Windows Explorer and select\n" +
				"\"Hash this file now\" (basic 5) or\n" +
				"\"Hash this file now (full)\" (all 70+).\n\n" +
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
				Arguments = string.Join(" ", ["--gui-install", .. args]),
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
		var fullInstalled = IsEntryInstalled(FullRegistryKeyPath);
		var basicInstalled = IsEntryInstalled(BasicRegistryKeyPath);
		var legacyInstalled = IsEntryInstalled(LegacyRegistryKeyPath);

		var exePath = Environment.ProcessPath ?? "Unknown";

		if (!fullInstalled && !basicInstalled) {
			details.Add("✗ Context menu (full): Not installed");
			details.Add("✗ Context menu (basic): Not installed");
			details.Add("  Run --install --menu both to install both entries.");
		} else {
			AddEntryStatus(details, "full", FullRegistryKeyPath, exePath, mustContainBasicSwitch: false);
			AddEntryStatus(details, "basic", BasicRegistryKeyPath, exePath, mustContainBasicSwitch: true);
		}

		if (legacyInstalled) {
			details.Add("⚠ Context menu (legacy): Installed (old single-entry key detected)");
			details.Add("  Run --install or --uninstall --menu both to remove legacy entry.");
		}

		details.Add($"Executable: {exePath}");
		details.Add($"Admin:      {(IsElevated ? "Yes" : "No")}");

		var fullCommand = GetInstalledFullCommand();
		if (fullCommand is not null) {
			details.Add($"Registered (full):  {fullCommand}");
		}

		var basicCommand = GetInstalledBasicCommand();
		if (basicCommand is not null) {
			details.Add($"Registered (basic): {basicCommand}");
		}

		return details;
	}

	/// <summary>
	/// Adds status details for a specific context menu entry.
	/// </summary>
	private static void AddEntryStatus(
		List<string> details,
		string label,
		string registryPath,
		string exePath,
		bool mustContainBasicSwitch) {
		if (!IsEntryInstalled(registryPath)) {
			details.Add($"✗ Context menu ({label}): Not installed");
			return;
		}

		if (IsEntryCorrect(registryPath, exePath, mustContainBasicSwitch)) {
			details.Add($"✓ Context menu ({label}): Installed (current)");
		} else {
			details.Add($"⚠ Context menu ({label}): Installed (outdated path or command)");
			details.Add($"  Run --install --menu {label} to repair this entry.");
		}
	}

	#endregion
}
