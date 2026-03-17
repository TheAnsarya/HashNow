using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using HashNow.Core;

namespace HashNow.Cli.Platform.Linux;

/// <summary>
/// Linux platform integration with support for multiple file managers:
/// Nautilus (GNOME), Nemo (Cinnamon), Dolphin (KDE), and Thunar (XFCE).
/// </summary>
[SupportedOSPlatform("linux")]
internal sealed class LinuxPlatform : IPlatformIntegration {
	#region Constants

	/// <summary>
	/// Menu item name used across all file managers.
	/// </summary>
	private const string MenuItemName = "Hash this file now";

	/// <summary>
	/// Nautilus script name (displayed as the menu item text).
	/// </summary>
	private const string NautilusScriptName = "Hash this file now";

	/// <summary>
	/// Nemo action filename.
	/// </summary>
	private const string NemoActionFile = "hashnow.nemo_action";

	/// <summary>
	/// Dolphin service menu filename.
	/// </summary>
	private const string DolphinServiceFile = "hashnow.desktop";

	/// <summary>
	/// Thunar custom action XML element marker.
	/// </summary>
	private const string ThunarActionMarker = "<!-- HashNow -->";

	#endregion

	#region IPlatformIntegration Properties

	/// <inheritdoc/>
	public string PlatformName {
		get {
			var managers = GetInstalledFileManagers();
			if (managers.Count > 0) {
				return $"Linux ({string.Join(", ", managers)})";
			}
			return "Linux";
		}
	}

	/// <inheritdoc/>
	public bool IsElevated {
		get {
			// Check if running as root (uid 0)
			try {
				var euid = Environment.GetEnvironmentVariable("EUID");
				if (euid == "0") return true;

				using var proc = Process.Start(new ProcessStartInfo {
					FileName = "id",
					Arguments = "-u",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				});
				if (proc is not null) {
					var output = proc.StandardOutput.ReadToEnd().Trim();
					proc.WaitForExit();
					return output == "0";
				}
			} catch { }
			return false;
		}
	}

	/// <inheritdoc/>
	public bool SupportsGuiProgress =>
		IsCommandAvailable("zenity") || IsCommandAvailable("kdialog");

	#endregion

	#region Initialization

	/// <inheritdoc/>
	public void Initialize(string[] args) {
		// No special initialization needed on Linux
	}

	#endregion

	#region Install/Uninstall

	/// <inheritdoc/>
	public bool Install(string executablePath) {
		var managers = GetInstalledFileManagers();
		bool anyInstalled = false;

		foreach (var manager in managers) {
			try {
				bool result = manager switch {
					"Nautilus" => InstallNautilus(executablePath),
					"Nemo" => InstallNemo(executablePath),
					"Dolphin" => InstallDolphin(executablePath),
					"Thunar" => InstallThunar(executablePath),
					_ => false
				};
				if (result) anyInstalled = true;
			} catch (Exception ex) {
				Console.Error.WriteLine($"Warning: Failed to install for {manager}: {ex.Message}");
			}
		}

		if (!anyInstalled && managers.Count == 0) {
			// No known file managers detected, install Nautilus script as default
			anyInstalled = InstallNautilus(executablePath);
		}

		return anyInstalled;
	}

	/// <inheritdoc/>
	public bool Uninstall() {
		bool anyRemoved = false;

		if (UninstallNautilus()) anyRemoved = true;
		if (UninstallNemo()) anyRemoved = true;
		if (UninstallDolphin()) anyRemoved = true;
		if (UninstallThunar()) anyRemoved = true;

		return anyRemoved;
	}

	/// <inheritdoc/>
	public bool IsInstalled() {
		return IsNautilusInstalled() ||
			   IsNemoInstalled() ||
			   IsDolphinInstalled() ||
			   IsThunarInstalled();
	}

	/// <inheritdoc/>
	public bool IsInstalledCorrectly(string executablePath) {
		// Check if any installed integration points to the correct executable
		var command = GetInstalledCommand();
		if (command is null) return false;
		return command.Contains(executablePath, StringComparison.Ordinal);
	}

	/// <inheritdoc/>
	public string? GetInstalledCommand() {
		// Check Nautilus script
		var nautilusPath = GetNautilusScriptPath();
		if (File.Exists(nautilusPath)) {
			return File.ReadAllText(nautilusPath);
		}

		// Check Nemo action
		var nemoPath = GetNemoActionPath();
		if (File.Exists(nemoPath)) {
			var content = File.ReadAllText(nemoPath);
			var execLine = content.Split('\n')
				.FirstOrDefault(l => l.StartsWith("Exec=", StringComparison.Ordinal));
			return execLine?["Exec=".Length..];
		}

		// Check Dolphin service menu
		var dolphinPath = GetDolphinServicePath();
		if (File.Exists(dolphinPath)) {
			var content = File.ReadAllText(dolphinPath);
			var execLine = content.Split('\n')
				.FirstOrDefault(l => l.StartsWith("Exec=", StringComparison.Ordinal));
			return execLine?["Exec=".Length..];
		}

		return null;
	}

	#endregion

	#region File Manager Detection

	/// <summary>
	/// Detects which file managers are installed on the system.
	/// </summary>
	private static List<string> GetInstalledFileManagers() {
		var managers = new List<string>();

		if (IsCommandAvailable("nautilus")) managers.Add("Nautilus");
		if (IsCommandAvailable("nemo")) managers.Add("Nemo");
		if (IsCommandAvailable("dolphin")) managers.Add("Dolphin");
		if (IsCommandAvailable("thunar")) managers.Add("Thunar");

		return managers;
	}

	/// <summary>
	/// Checks if a command is available on the system PATH.
	/// </summary>
	private static bool IsCommandAvailable(string command) {
		try {
			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "which",
				Arguments = command,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			});
			if (proc is null) return false;
			proc.WaitForExit();
			return proc.ExitCode == 0;
		} catch {
			return false;
		}
	}

	#endregion

	#region Nautilus (GNOME)

	private static string GetNautilusScriptPath() {
		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(home, ".local", "share", "nautilus", "scripts", NautilusScriptName);
	}

	private static bool IsNautilusInstalled() => File.Exists(GetNautilusScriptPath());

	private static bool InstallNautilus(string executablePath) {
		var scriptPath = GetNautilusScriptPath();
		var scriptDir = Path.GetDirectoryName(scriptPath)!;
		Directory.CreateDirectory(scriptDir);

		var script = $"""
			#!/bin/bash
			# HashNow - Compute 70 hash algorithms for files
			# Installed by HashNow for Nautilus/GNOME Files
			IFS=$'\n'
			for file in $NAUTILUS_SCRIPT_SELECTED_FILE_PATHS; do
				"{executablePath}" "$file"
			done
			""";

		File.WriteAllText(scriptPath, script, new UTF8Encoding(false));

		// Make executable
		SetExecutable(scriptPath);
		return true;
	}

	private static bool UninstallNautilus() {
		var path = GetNautilusScriptPath();
		if (File.Exists(path)) {
			File.Delete(path);
			return true;
		}
		return false;
	}

	#endregion

	#region Nemo (Cinnamon)

	private static string GetNemoActionPath() {
		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(home, ".local", "share", "nemo", "actions", NemoActionFile);
	}

	private static bool IsNemoInstalled() => File.Exists(GetNemoActionPath());

	private static bool InstallNemo(string executablePath) {
		var actionPath = GetNemoActionPath();
		var actionDir = Path.GetDirectoryName(actionPath)!;
		Directory.CreateDirectory(actionDir);

		var content = $"""
			[Nemo Action]
			Name={MenuItemName}
			Comment=Compute 70 hash algorithms and save to JSON
			Exec="{executablePath}" %F
			Icon-Name=document-properties
			Selection=s
			Extensions=any;
			""";

		File.WriteAllText(actionPath, content, new UTF8Encoding(false));
		return true;
	}

	private static bool UninstallNemo() {
		var path = GetNemoActionPath();
		if (File.Exists(path)) {
			File.Delete(path);
			return true;
		}
		return false;
	}

	#endregion

	#region Dolphin (KDE)

	private static string GetDolphinServicePath() {
		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(home, ".local", "share", "kio", "servicemenus", DolphinServiceFile);
	}

	private static bool IsDolphinInstalled() => File.Exists(GetDolphinServicePath());

	private static bool InstallDolphin(string executablePath) {
		var servicePath = GetDolphinServicePath();
		var serviceDir = Path.GetDirectoryName(servicePath)!;
		Directory.CreateDirectory(serviceDir);

		var content = $"""
			[Desktop Entry]
			Type=Service
			MimeType=application/octet-stream;
			Actions=hashfile

			[Desktop Action hashfile]
			Name={MenuItemName}
			Icon=document-properties
			Exec="{executablePath}" %f
			""";

		File.WriteAllText(servicePath, content, new UTF8Encoding(false));
		return true;
	}

	private static bool UninstallDolphin() {
		var path = GetDolphinServicePath();
		if (File.Exists(path)) {
			File.Delete(path);
			return true;
		}
		return false;
	}

	#endregion

	#region Thunar (XFCE)

	private static string GetThunarConfigPath() {
		var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		return Path.Combine(home, ".config", "Thunar", "uca.xml");
	}

	private static bool IsThunarInstalled() {
		var path = GetThunarConfigPath();
		if (!File.Exists(path)) return false;
		var content = File.ReadAllText(path);
		return content.Contains(ThunarActionMarker, StringComparison.Ordinal);
	}

	private static bool InstallThunar(string executablePath) {
		var configPath = GetThunarConfigPath();
		var configDir = Path.GetDirectoryName(configPath)!;
		Directory.CreateDirectory(configDir);

		var actionXml = $"""
			{ThunarActionMarker}
			<action>
				<icon>document-properties</icon>
				<name>{MenuItemName}</name>
				<command>"{executablePath}" %f</command>
				<description>Compute 70 hash algorithms and save to JSON</description>
				<patterns>*</patterns>
				<other-files/>
			</action>
			""";

		if (File.Exists(configPath)) {
			var content = File.ReadAllText(configPath);
			// Remove existing HashNow action if present
			content = RemoveThunarAction(content);
			// Insert before closing </actions> tag
			var insertPos = content.LastIndexOf("</actions>", StringComparison.Ordinal);
			if (insertPos >= 0) {
				content = content.Insert(insertPos, actionXml + "\n");
				File.WriteAllText(configPath, content, new UTF8Encoding(false));
				return true;
			}
		}

		// Create new uca.xml if it doesn't exist
		var newContent = $"""
			<?xml version="1.0" encoding="UTF-8"?>
			<actions>
			{actionXml}
			</actions>
			""";
		File.WriteAllText(configPath, newContent, new UTF8Encoding(false));
		return true;
	}

	private static bool UninstallThunar() {
		var configPath = GetThunarConfigPath();
		if (!File.Exists(configPath)) return false;

		var content = File.ReadAllText(configPath);
		if (!content.Contains(ThunarActionMarker, StringComparison.Ordinal)) return false;

		content = RemoveThunarAction(content);
		File.WriteAllText(configPath, content, new UTF8Encoding(false));
		return true;
	}

	/// <summary>
	/// Removes the HashNow action block from Thunar's uca.xml content.
	/// </summary>
	private static string RemoveThunarAction(string content) {
		var markerIdx = content.IndexOf(ThunarActionMarker, StringComparison.Ordinal);
		if (markerIdx < 0) return content;

		// Find the closing </action> tag after the marker
		var endTag = "</action>";
		var endIdx = content.IndexOf(endTag, markerIdx, StringComparison.Ordinal);
		if (endIdx < 0) return content;

		endIdx += endTag.Length;
		// Also remove trailing newline
		if (endIdx < content.Length && content[endIdx] == '\n') endIdx++;

		return content.Remove(markerIdx, endIdx - markerIdx);
	}

	#endregion

	#region Dialogs (Terminal-based)

	/// <inheritdoc/>
	public bool AskYesNo(string title, string message) {
		// Try zenity first for GUI dialog
		if (TryZenityQuestion(title, message, out var result)) {
			return result;
		}

		// Try kdialog
		if (TryKdialogQuestion(title, message, out result)) {
			return result;
		}

		// Fall back to terminal prompt
		Console.WriteLine();
		Console.WriteLine(message);
		Console.Write("[Y/n] ");
		var key = Console.ReadKey(true);
		Console.WriteLine();
		return key.Key != ConsoleKey.N;
	}

	/// <inheritdoc/>
	public void ShowInfo(string title, string message) {
		if (TryZenityInfo(title, message)) return;
		if (TryKdialogInfo(title, message)) return;
		Console.WriteLine(message);
	}

	/// <inheritdoc/>
	public void ShowError(string title, string message) {
		if (TryZenityError(title, message)) return;
		if (TryKdialogError(title, message)) return;
		Console.Error.WriteLine(message);
	}

	/// <inheritdoc/>
	public void ShowSuccess(string title, string message) => ShowInfo(title, message);

	/// <inheritdoc/>
	public (bool ShouldRestart, bool ShouldInstall, bool Cancelled) ShowInstallPrompts(
		bool isInstalled, bool isCorrect) {
		if (isInstalled && isCorrect) {
			ShowInfo("HashNow", "HashNow is already installed for your file manager(s).");
			return (false, false, false);
		}

		// No elevation needed on Linux (user-level install)
		string message = isInstalled && !isCorrect
			? "HashNow file manager integration needs to be updated.\nInstall now?"
			: "HashNow is not installed in your file manager.\nInstall now?";

		bool install = AskYesNo("HashNow", message);
		return (false, install, !install);
	}

	/// <inheritdoc/>
	public void ShowInstallResult(bool success, string? errorMessage = null) {
		if (success) {
			var managers = GetInstalledFileManagers();
			ShowSuccess("HashNow",
				$"Context menu installed for: {string.Join(", ", managers)}\n\n" +
				"Right-click any file in your file manager and select\n" +
				$"\"{MenuItemName}\" to compute all 70 hashes.");
		} else {
			ShowError("HashNow",
				$"Error installing context menu:\n{errorMessage ?? "Unknown error"}");
		}
	}

	#endregion

	#region zenity/kdialog helpers

	private static bool TryZenityQuestion(string title, string message, out bool result) {
		result = false;
		if (!IsCommandAvailable("zenity")) return false;
		try {
			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "zenity",
				ArgumentList = { "--question", "--title", title, "--text", message },
				UseShellExecute = false,
				CreateNoWindow = true
			});
			if (proc is null) return false;
			proc.WaitForExit();
			result = proc.ExitCode == 0;
			return true;
		} catch {
			return false;
		}
	}

	private static bool TryZenityInfo(string title, string message) {
		if (!IsCommandAvailable("zenity")) return false;
		try {
			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "zenity",
				ArgumentList = { "--info", "--title", title, "--text", message },
				UseShellExecute = false,
				CreateNoWindow = true
			});
			proc?.WaitForExit();
			return true;
		} catch {
			return false;
		}
	}

	private static bool TryZenityError(string title, string message) {
		if (!IsCommandAvailable("zenity")) return false;
		try {
			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "zenity",
				ArgumentList = { "--error", "--title", title, "--text", message },
				UseShellExecute = false,
				CreateNoWindow = true
			});
			proc?.WaitForExit();
			return true;
		} catch {
			return false;
		}
	}

	private static bool TryKdialogQuestion(string title, string message, out bool result) {
		result = false;
		if (!IsCommandAvailable("kdialog")) return false;
		try {
			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "kdialog",
				ArgumentList = { "--title", title, "--yesno", message },
				UseShellExecute = false,
				CreateNoWindow = true
			});
			if (proc is null) return false;
			proc.WaitForExit();
			result = proc.ExitCode == 0;
			return true;
		} catch {
			return false;
		}
	}

	private static bool TryKdialogInfo(string title, string message) {
		if (!IsCommandAvailable("kdialog")) return false;
		try {
			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "kdialog",
				ArgumentList = { "--title", title, "--msgbox", message },
				UseShellExecute = false,
				CreateNoWindow = true
			});
			proc?.WaitForExit();
			return true;
		} catch {
			return false;
		}
	}

	private static bool TryKdialogError(string title, string message) {
		if (!IsCommandAvailable("kdialog")) return false;
		try {
			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "kdialog",
				ArgumentList = { "--title", title, "--error", message },
				UseShellExecute = false,
				CreateNoWindow = true
			});
			proc?.WaitForExit();
			return true;
		} catch {
			return false;
		}
	}

	#endregion

	#region Progress

	/// <inheritdoc/>
	public async Task<FileHashResult?> HashFileWithProgress(string filePath, CancellationToken ct = default) {
		// Try zenity GUI progress (GTK - GNOME, Xfce, etc.)
		if (IsCommandAvailable("zenity")) {
			return await HashWithZenityProgress(filePath, ct);
		}

		// Try kdialog GUI progress (KDE)
		if (IsCommandAvailable("kdialog")) {
			return await HashWithKdialogProgress(filePath, ct);
		}

		// Fallback: hash without visible progress (no terminal in file manager mode)
		return await FileHasher.HashFileAsync(filePath, cancellationToken: ct);
	}

	/// <summary>
	/// Hashes a file with a zenity progress dialog showing live percentage and cancel button.
	/// </summary>
	private static async Task<FileHashResult?> HashWithZenityProgress(
		string filePath, CancellationToken ct) {
		var fileName = Path.GetFileName(filePath);
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

		var psi = new ProcessStartInfo {
			FileName = "zenity",
			ArgumentList = {
				"--progress",
				"--title", "HashNow — Computing Hashes...",
				"--text", $"Hashing: {fileName}",
				"--percentage", "0",
				"--auto-close",
				"--width", "400"
			},
			RedirectStandardInput = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		using var zenity = Process.Start(psi);
		if (zenity is null) {
			return await FileHasher.HashFileAsync(filePath, cancellationToken: ct);
		}

		// Monitor zenity for cancel (user clicks Cancel → exit code 1)
		_ = Task.Run(() => {
			zenity.WaitForExit();
			if (zenity.ExitCode != 0 && !cts.IsCancellationRequested) {
				cts.Cancel();
			}
		}, CancellationToken.None);

		FileHashResult? result = null;
		try {
			int lastPercent = -1;
			result = await FileHasher.HashFileAsync(filePath, progress => {
				int percent = Math.Clamp((int)(progress * 100), 0, 100);
				if (percent != lastPercent && !zenity.HasExited) {
					lastPercent = percent;
					try {
						zenity.StandardInput.WriteLine(percent);
						zenity.StandardInput.Flush();
					} catch {
						// zenity already exited (cancelled or closed)
					}
				}
			}, cts.Token);

			// Hash completed — close zenity
			if (!zenity.HasExited) {
				try {
					zenity.StandardInput.WriteLine(100);
					zenity.StandardInput.Close();
				} catch {
					// zenity already closed
				}
			}
		} catch (OperationCanceledException) {
			if (!zenity.HasExited) {
				zenity.Kill();
			}
			return null;
		}

		return result;
	}

	/// <summary>
	/// Hashes a file with a kdialog progress dialog showing live percentage and cancel button.
	/// </summary>
	private static async Task<FileHashResult?> HashWithKdialogProgress(
		string filePath, CancellationToken ct) {
		var fileName = Path.GetFileName(filePath);
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

		// kdialog --progressbar returns a D-Bus reference for updates
		var psi = new ProcessStartInfo {
			FileName = "kdialog",
			ArgumentList = {
				"--title", "HashNow",
				"--progressbar", $"Hashing: {fileName}",
				"100"
			},
			RedirectStandardOutput = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		using var kdialog = Process.Start(psi);
		if (kdialog is null) {
			return await FileHasher.HashFileAsync(filePath, cancellationToken: ct);
		}

		var dbusRef = kdialog.StandardOutput.ReadToEnd().Trim();
		kdialog.WaitForExit();

		// Parse D-Bus service and path (format: "org.kde.kdialog-NNNNN /ProgressDialog")
		var parts = dbusRef.Split(' ', 2);
		if (parts.Length < 2) {
			return await FileHasher.HashFileAsync(filePath, cancellationToken: ct);
		}
		var dbusService = parts[0];
		var dbusPath = parts[1];

		FileHashResult? result = null;
		try {
			int lastPercent = -1;
			result = await FileHasher.HashFileAsync(filePath, progress => {
				int percent = Math.Clamp((int)(progress * 100), 0, 100);
				if (percent != lastPercent) {
					lastPercent = percent;
					// Update progress via qdbus
					try {
						using var update = Process.Start(new ProcessStartInfo {
							FileName = "qdbus",
							ArgumentList = {
								dbusService, dbusPath,
								"Set", "", "value", percent.ToString()
							},
							UseShellExecute = false,
							CreateNoWindow = true,
							RedirectStandardOutput = true
						});
						update?.WaitForExit();
					} catch {
						// qdbus call failed — dialog may have been closed
					}

					// Check if user cancelled
					try {
						using var check = Process.Start(new ProcessStartInfo {
							FileName = "qdbus",
							ArgumentList = {
								dbusService, dbusPath, "wasCancelled"
							},
							RedirectStandardOutput = true,
							UseShellExecute = false,
							CreateNoWindow = true
						});
						if (check is not null) {
							var cancelled = check.StandardOutput.ReadToEnd().Trim();
							check.WaitForExit();
							if (cancelled == "true" && !cts.IsCancellationRequested) {
								cts.Cancel();
							}
						}
					} catch {
						// qdbus call failed
					}
				}
			}, cts.Token);

			// Close the dialog
			KdialogClose(dbusService, dbusPath);
		} catch (OperationCanceledException) {
			KdialogClose(dbusService, dbusPath);
			return null;
		}

		return result;
	}

	/// <summary>
	/// Closes a kdialog progress dialog via D-Bus.
	/// </summary>
	private static void KdialogClose(string dbusService, string dbusPath) {
		try {
			using var close = Process.Start(new ProcessStartInfo {
				FileName = "qdbus",
				ArgumentList = { dbusService, dbusPath, "close" },
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true
			});
			close?.WaitForExit();
		} catch {
			// Dialog may already be closed
		}
	}

	#endregion

	#region Elevation

	/// <inheritdoc/>
	public bool RelaunchElevated(string[] args) {
		// Linux context menu installs are user-level, no elevation needed
		// But provide pkexec fallback if truly needed
		try {
			var exePath = Environment.ProcessPath;
			if (string.IsNullOrEmpty(exePath)) return false;

			var argString = string.Join(" ", args.Select(a => $"\"{a}\""));
			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "pkexec",
				Arguments = $"\"{exePath}\" {argString}",
				UseShellExecute = false
			});
			if (proc is null) return false;
			proc.WaitForExit();
			Environment.Exit(proc.ExitCode);
			return true;
		} catch {
			return false;
		}
	}

	#endregion

	#region Launch Detection

	/// <inheritdoc/>
	public bool DetectDoubleClickLaunch() {
		// On Linux, check if parent is a file manager
		try {
			var ppidStr = File.ReadAllText($"/proc/{Environment.ProcessId}/stat")
				.Split(' ')[3]; // 4th field is PPID
			if (int.TryParse(ppidStr, out var ppid)) {
				var parentComm = File.ReadAllText($"/proc/{ppid}/comm").Trim();
				return parentComm is "nautilus" or "nemo" or "dolphin" or "thunar"
					or "pcmanfm" or "caja";
			}
		} catch { }
		return false;
	}

	/// <inheritdoc/>
	public bool DetectFileManagerLaunch() {
		// Check environment variables set by file managers
		if (Environment.GetEnvironmentVariable("NAUTILUS_SCRIPT_SELECTED_FILE_PATHS") is not null) {
			return true;
		}
		return DetectDoubleClickLaunch();
	}

	#endregion

	#region Status

	/// <inheritdoc/>
	public IReadOnlyList<string> GetStatusDetails() {
		var details = new List<string>();
		var managers = GetInstalledFileManagers();
		var exePath = Environment.ProcessPath ?? "Unknown";

		if (managers.Count > 0) {
			details.Add($"Detected file managers: {string.Join(", ", managers)}");
		} else {
			details.Add("No supported file managers detected.");
		}

		if (IsNautilusInstalled()) details.Add("✓ Nautilus (GNOME): Installed");
		if (IsNemoInstalled()) details.Add("✓ Nemo (Cinnamon): Installed");
		if (IsDolphinInstalled()) details.Add("✓ Dolphin (KDE): Installed");
		if (IsThunarInstalled()) details.Add("✓ Thunar (XFCE): Installed");

		if (!IsInstalled()) {
			details.Add("✗ No file manager integration installed");
			details.Add("  Run --install to add context menu support.");
		}

		details.Add($"Executable: {exePath}");
		details.Add($"Root:       {(IsElevated ? "Yes" : "No")}");

		return details;
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Makes a file executable via chmod +x.
	/// </summary>
	private static void SetExecutable(string path) {
		try {
			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "chmod",
				Arguments = $"+x \"{path}\"",
				UseShellExecute = false,
				CreateNoWindow = true
			});
			proc?.WaitForExit();
		} catch { }
	}

	#endregion
}
