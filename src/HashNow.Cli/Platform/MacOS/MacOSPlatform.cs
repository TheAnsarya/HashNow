using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using HashNow.Core;

namespace HashNow.Cli.Platform.MacOS;

/// <summary>
/// macOS platform integration using Finder Quick Actions (Automator workflows).
/// </summary>
[SupportedOSPlatform("macos")]
internal sealed class MacOSPlatform : IPlatformIntegration {
	#region Constants

	/// <summary>
	/// Name of the Quick Action workflow.
	/// </summary>
	private const string WorkflowName = "Hash this file now";

	/// <summary>
	/// Directory where Quick Action workflows are stored.
	/// </summary>
	private static string ServicesDir =>
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			"Library", "Services");

	/// <summary>
	/// Full path to the workflow bundle.
	/// </summary>
	private static string WorkflowPath =>
		Path.Combine(ServicesDir, $"{WorkflowName}.workflow");

	#endregion

	#region IPlatformIntegration Properties

	/// <inheritdoc/>
	public string PlatformName => "macOS";

	/// <inheritdoc/>
	public bool IsElevated {
		get {
			try {
				using var proc = Process.Start(new ProcessStartInfo {
					FileName = "id",
					Arguments = "-u",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				});
				if (proc is null) return false;
				var output = proc.StandardOutput.ReadToEnd().Trim();
				proc.WaitForExit();
				return output == "0";
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
		// No special initialization needed on macOS
	}

	#endregion

	#region Install/Uninstall

	/// <inheritdoc/>
	public bool Install(string executablePath) {
		// Create the workflow bundle directory structure
		var contentsDir = Path.Combine(WorkflowPath, "Contents");
		Directory.CreateDirectory(contentsDir);

		// Create Info.plist
		var infoPlist = CreateInfoPlist();
		File.WriteAllText(
			Path.Combine(contentsDir, "Info.plist"),
			infoPlist,
			new UTF8Encoding(false));

		// Create document.wflow
		var workflow = CreateDocumentWflow(executablePath);
		File.WriteAllText(
			Path.Combine(contentsDir, "document.wflow"),
			workflow,
			new UTF8Encoding(false));

		return true;
	}

	/// <inheritdoc/>
	public bool Uninstall() {
		if (Directory.Exists(WorkflowPath)) {
			Directory.Delete(WorkflowPath, recursive: true);
			return true;
		}
		return false;
	}

	/// <inheritdoc/>
	public bool IsInstalled() => Directory.Exists(WorkflowPath);

	/// <inheritdoc/>
	public bool IsInstalledCorrectly(string executablePath) {
		var wflowPath = Path.Combine(WorkflowPath, "Contents", "document.wflow");
		if (!File.Exists(wflowPath)) return false;
		var content = File.ReadAllText(wflowPath);
		return content.Contains(executablePath, StringComparison.Ordinal);
	}

	/// <inheritdoc/>
	public string? GetInstalledCommand() {
		var wflowPath = Path.Combine(WorkflowPath, "Contents", "document.wflow");
		if (!File.Exists(wflowPath)) return null;
		return File.ReadAllText(wflowPath);
	}

	#endregion

	#region Workflow Generation

	/// <summary>
	/// Creates the Info.plist for the Quick Action workflow.
	/// </summary>
	private static string CreateInfoPlist() {
		return """
			<?xml version="1.0" encoding="UTF-8"?>
			<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
			<plist version="1.0">
			<dict>
				<key>NSServices</key>
				<array>
					<dict>
						<key>NSMenuItem</key>
						<dict>
							<key>default</key>
							<string>Hash this file now</string>
						</dict>
						<key>NSMessage</key>
						<string>runWorkflowAsService</string>
						<key>NSSendFileTypes</key>
						<array>
							<string>public.item</string>
						</array>
					</dict>
				</array>
			</dict>
			</plist>
			""";
	}

	/// <summary>
	/// Creates the document.wflow (Automator workflow) that runs HashNow.
	/// </summary>
	private static string CreateDocumentWflow(string executablePath) {
		// Escape for XML
		var escapedPath = executablePath
			.Replace("&", "&amp;")
			.Replace("<", "&lt;")
			.Replace(">", "&gt;")
			.Replace("\"", "&quot;");

		return $"""
			<?xml version="1.0" encoding="UTF-8"?>
			<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
			<plist version="1.0">
			<dict>
				<key>AMApplicationBuild</key>
				<string>523</string>
				<key>AMApplicationVersion</key>
				<string>2.10</string>
				<key>AMDocumentVersion</key>
				<string>2</string>
				<key>actions</key>
				<array>
					<dict>
						<key>action</key>
						<dict>
							<key>AMAccepts</key>
							<dict>
								<key>Container</key>
								<string>List</string>
								<key>Optional</key>
								<false/>
								<key>Types</key>
								<array>
									<string>com.apple.cocoa.path</string>
								</array>
							</dict>
							<key>AMActionVersion</key>
							<string>1.0.2</string>
							<key>AMApplication</key>
							<array>
								<string>Automator</string>
							</array>
							<key>AMCategory</key>
							<string>AMCategoryUtilities</string>
							<key>AMIconName</key>
							<string>Automator</string>
							<key>AMName</key>
							<string>Run Shell Script</string>
							<key>AMProvides</key>
							<dict>
								<key>Container</key>
								<string>List</string>
								<key>Types</key>
								<array>
									<string>com.apple.cocoa.string</string>
								</array>
							</dict>
							<key>ActionBundlePath</key>
							<string>/System/Library/Automator/Run Shell Script.action</string>
							<key>ActionName</key>
							<string>Run Shell Script</string>
							<key>ActionParameters</key>
							<dict>
								<key>COMMAND_STRING</key>
								<string>for f in "$@"; do
				"{escapedPath}" "$f"
			done</string>
								<key>CheckedForUserDefaultShell</key>
								<true/>
								<key>inputMethod</key>
								<integer>1</integer>
								<key>shell</key>
								<string>/bin/bash</string>
								<key>source</key>
								<string></string>
							</dict>
							<key>BundleIdentifier</key>
							<string>com.apple.RunShellScript</string>
							<key>CFBundleVersion</key>
							<string>1.0.2</string>
							<key>CanShowSelectedItemsWhenRun</key>
							<false/>
							<key>CanShowWhenRun</key>
							<true/>
							<key>Category</key>
							<array>
								<string>AMCategoryUtilities</string>
							</array>
							<key>Class Name</key>
							<string>RunShellScriptAction</string>
							<key>InputUUID</key>
							<string>A1A1A1A1-B2B2-C3C3-D4D4-E5E5E5E5E5E5</string>
							<key>Keywords</key>
							<array>
								<string>Shell</string>
								<string>Script</string>
								<string>Command</string>
								<string>Run</string>
							</array>
							<key>OutputUUID</key>
							<string>F6F6F6F6-A7A7-B8B8-C9C9-D0D0D0D0D0D0</string>
							<key>UUID</key>
							<string>12345678-ABCD-EF01-2345-6789ABCDEF01</string>
							<key>UnlocalizedApplications</key>
							<array>
								<string>Automator</string>
							</array>
							<key>arguments</key>
							<dict/>
							<key>isViewVisible</key>
							<integer>1</integer>
						</dict>
					</dict>
				</array>
				<key>connectors</key>
				<dict/>
				<key>workflowMetaData</key>
				<dict>
					<key>workflowTypeIdentifier</key>
					<string>com.apple.Automator.servicesMenu</string>
				</dict>
			</dict>
			</plist>
			""";
	}

	#endregion

	#region Dialogs (osascript-based with terminal fallback)

	/// <inheritdoc/>
	public bool AskYesNo(string title, string message) {
		if (TryOsascriptQuestion(title, message, out var result)) {
			return result;
		}

		// Terminal fallback
		Console.WriteLine();
		Console.WriteLine(message);
		Console.Write("[Y/n] ");
		var key = Console.ReadKey(true);
		Console.WriteLine();
		return key.Key != ConsoleKey.N;
	}

	/// <inheritdoc/>
	public void ShowInfo(string title, string message) {
		if (TryOsascriptInfo(title, message)) return;
		Console.WriteLine(message);
	}

	/// <inheritdoc/>
	public void ShowError(string title, string message) {
		if (TryOsascriptError(title, message)) return;
		Console.Error.WriteLine(message);
	}

	/// <inheritdoc/>
	public void ShowSuccess(string title, string message) => ShowInfo(title, message);

	/// <inheritdoc/>
	public (bool ShouldRestart, bool ShouldInstall, bool Cancelled) ShowInstallPrompts(
		bool isInstalled, bool isCorrect) {
		if (isInstalled && isCorrect) {
			ShowInfo("HashNow",
				"HashNow is already installed!\n\n" +
				"Right-click any file in Finder and look under\n" +
				"Quick Actions to find \"Hash this file now\".");
			return (false, false, false);
		}

		string message = isInstalled && !isCorrect
			? "HashNow Finder integration needs to be updated.\nInstall now?"
			: "HashNow is not installed in Finder.\nInstall now?";

		bool install = AskYesNo("HashNow", message);
		return (false, install, !install);
	}

	/// <inheritdoc/>
	public void ShowInstallResult(bool success, string? errorMessage = null) {
		if (success) {
			ShowSuccess("HashNow",
				"Quick Action installed successfully!\n\n" +
				"Right-click any file in Finder → Quick Actions →\n" +
				"\"Hash this file now\" to compute all 70 hashes.");
		} else {
			ShowError("HashNow",
				$"Error installing Quick Action:\n{errorMessage ?? "Unknown error"}");
		}
	}

	#endregion

	#region osascript helpers

	private static bool TryOsascriptQuestion(string title, string message, out bool result) {
		result = false;
		try {
			// Escape for AppleScript
			var escapedMsg = message.Replace("\"", "\\\"").Replace("\n", "\\n");
			var escapedTitle = title.Replace("\"", "\\\"");

			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "osascript",
				Arguments = $"-e 'display dialog \"{escapedMsg}\" with title \"{escapedTitle}\" buttons {{\"No\", \"Yes\"}} default button \"Yes\"'",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			});
			if (proc is null) return false;
			var output = proc.StandardOutput.ReadToEnd();
			proc.WaitForExit();
			result = proc.ExitCode == 0 && output.Contains("Yes");
			return true;
		} catch {
			return false;
		}
	}

	private static bool TryOsascriptInfo(string title, string message) {
		try {
			var escapedMsg = message.Replace("\"", "\\\"").Replace("\n", "\\n");
			var escapedTitle = title.Replace("\"", "\\\"");

			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "osascript",
				Arguments = $"-e 'display dialog \"{escapedMsg}\" with title \"{escapedTitle}\" buttons {{\"OK\"}} default button \"OK\"'",
				UseShellExecute = false,
				CreateNoWindow = true
			});
			proc?.WaitForExit();
			return true;
		} catch {
			return false;
		}
	}

	private static bool TryOsascriptError(string title, string message) {
		try {
			var escapedMsg = message.Replace("\"", "\\\"").Replace("\n", "\\n");
			var escapedTitle = title.Replace("\"", "\\\"");

			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "osascript",
				Arguments = $"-e 'display dialog \"{escapedMsg}\" with title \"{escapedTitle}\" with icon stop buttons {{\"OK\"}} default button \"OK\"'",
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
		// Try zenity first (available via Homebrew: brew install zenity)
		if (IsCommandAvailable("zenity")) {
			return await HashWithZenityProgress(filePath, ct);
		}

		// Use osascript dialog with cancel button (always available on macOS)
		return await HashWithOsascriptProgress(filePath, ct);
	}

	/// <summary>
	/// Hashes a file with a zenity progress dialog (if installed via Homebrew).
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
	/// Hashes a file with an osascript dialog that shows progress status and a cancel button.
	/// The dialog displays the file name and a "Computing hashes..." message.
	/// Clicking Cancel or closing the dialog stops the hashing operation.
	/// </summary>
	private static async Task<FileHashResult?> HashWithOsascriptProgress(
		string filePath, CancellationToken ct) {
		var fileName = Path.GetFileName(filePath);
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

		// Show a dialog with Cancel button that blocks osascript until dismissed
		var escapedFileName = EscapeAppleScript(fileName);
		var script = $"display dialog \"Hashing: {escapedFileName}\\n\\n" +
					 "Computing 70 hash algorithms...\\n" +
					 "This dialog will close automatically when complete.\" " +
					 "with title \"HashNow\" " +
					 "buttons {\"Cancel\"} " +
					 "default button \"Cancel\" " +
					 "giving up after 3600";

		var psi = new ProcessStartInfo {
			FileName = "osascript",
			ArgumentList = { "-e", script },
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		using var osascript = Process.Start(psi);
		if (osascript is null) {
			return await FileHasher.HashFileAsync(filePath, cancellationToken: ct);
		}

		// Monitor dialog for cancel (user clicks Cancel → osascript exits with code 1)
		_ = Task.Run(() => {
			osascript.WaitForExit();
			if (!cts.IsCancellationRequested) {
				cts.Cancel();
			}
		}, CancellationToken.None);

		FileHashResult? result = null;
		try {
			result = await FileHasher.HashFileAsync(filePath, cancellationToken: cts.Token);

			// Hash completed — kill the dialog
			if (!osascript.HasExited) {
				osascript.Kill();
			}
		} catch (OperationCanceledException) {
			if (!osascript.HasExited) {
				osascript.Kill();
			}
			return null;
		}

		return result;
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

	/// <summary>
	/// Escapes a string for use in AppleScript.
	/// </summary>
	private static string EscapeAppleScript(string value) =>
		value.Replace("\\", "\\\\").Replace("\"", "\\\"");

	#endregion

	#region Elevation

	/// <inheritdoc/>
	public bool RelaunchElevated(string[] args) {
		// macOS Quick Actions install is user-level, no elevation needed
		// But provide sudo fallback
		try {
			var exePath = Environment.ProcessPath;
			if (string.IsNullOrEmpty(exePath)) return false;

			var argString = string.Join(" ", args.Select(a => $"\"{a}\""));
			// Use osascript to get sudo via GUI
			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "osascript",
				Arguments = $"-e 'do shell script \"\\\"{exePath}\\\" {argString}\" with administrator privileges'",
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
		// On macOS, launched from Finder if parent is launchd or Finder
		try {
			using var proc = Process.Start(new ProcessStartInfo {
				FileName = "ps",
				Arguments = $"-p {Environment.ProcessId} -o ppid=",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			});
			if (proc is null) return false;
			var ppidStr = proc.StandardOutput.ReadToEnd().Trim();
			proc.WaitForExit();

			if (int.TryParse(ppidStr, out var ppid)) {
				using var parentProc = Process.Start(new ProcessStartInfo {
					FileName = "ps",
					Arguments = $"-p {ppid} -o comm=",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				});
				if (parentProc is null) return false;
				var parentName = parentProc.StandardOutput.ReadToEnd().Trim();
				parentProc.WaitForExit();

				return parentName.Contains("Finder") || parentName.Contains("launchd")
					|| parentName.Contains("open");
			}
		} catch { }
		return false;
	}

	/// <inheritdoc/>
	public bool DetectFileManagerLaunch() => DetectDoubleClickLaunch();

	#endregion

	#region Status

	/// <inheritdoc/>
	public IReadOnlyList<string> GetStatusDetails() {
		var details = new List<string>();
		var exePath = Environment.ProcessPath ?? "Unknown";

		if (IsInstalled()) {
			if (IsInstalledCorrectly(exePath)) {
				details.Add("✓ Finder Quick Action: Installed (current)");
			} else {
				details.Add("⚠ Finder Quick Action: Installed (outdated path)");
				details.Add("  Run --install to update.");
			}
		} else {
			details.Add("✗ Finder Quick Action: Not installed");
			details.Add("  Run --install to add Quick Action support.");
		}

		details.Add("");
		details.Add("Quick Actions appear in Finder → right-click → Quick Actions");
		details.Add($"Workflow: {WorkflowPath}");
		details.Add($"Executable: {exePath}");

		return details;
	}

	#endregion
}
