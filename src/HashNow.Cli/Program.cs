using HashNow.Cli.Platform;
using HashNow.Core;
using System.Linq;

namespace HashNow.Cli;

/// <summary>
/// HashNow CLI entry point - Computes 70 hash algorithms for files.
/// </summary>
/// <remarks>
/// <para>
/// This is the main entry point for the HashNow command-line application.
/// It provides a simple, user-friendly interface for computing file hashes
/// across Windows, Linux, and macOS.
/// </para>
/// <para>
/// <strong>Usage Modes:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Console Mode:</strong> Run from command prompt with file arguments.
///       Shows progress and prints results to console.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>File Manager Mode:</strong> Invoked via right-click context menu.
///       Silently creates JSON file next to the original.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Double-Click Mode:</strong> Running without arguments checks for
///       context menu installation and prompts to install if missing.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Output:</strong>
/// Creates a <c>{filename}.hashes.json</c> file containing all 70 hash values
/// and file metadata in a human-readable, tab-indented JSON format.
/// </para>
/// </remarks>
internal static class Program {
	private enum HashExecutionMode {
		Full,
		Basic
	}

	#region Main Entry Point

	/// <summary>
	/// Application entry point.
	/// </summary>
	/// <param name="args">
	/// Command-line arguments:
	/// <list type="bullet">
	///   <item><description>No args: Auto-install mode (check/prompt for context menu)</description></item>
	///   <item><description><c>--help</c>, <c>-h</c>, <c>/?</c>: Show usage information</description></item>
	///   <item><description><c>--version</c>, <c>-v</c>: Show version number</description></item>
	///   <item><description><c>--install</c>: Install file manager context menu</description></item>
	///   <item><description><c>--uninstall</c>: Remove file manager context menu</description></item>
	///   <item><description><c>--status</c>: Check context menu installation status</description></item>
	///   <item><description>File path(s): Hash the specified files</description></item>
	/// </list>
	/// </param>
	/// <returns>
	/// Exit code:
	/// <list type="bullet">
	///   <item><description><c>0</c>: Success</description></item>
	///   <item><description><c>1</c>: Error (file not found, permission denied, etc.)</description></item>
	///   <item><description><c>2</c>: User cancelled operation</description></item>
	/// </list>
	/// </returns>
	private static async Task<int> Main(string[] args) {
		// Create platform-specific integration
		var platform = PlatformFactory.Create();

		// Perform platform-specific initialization (e.g., WinForms init, console attachment)
		platform.Initialize(args);

		// No arguments provided - auto-install mode
		if (args.Length == 0) {
			return HandleNoArguments(platform);
		}

		// Parse command-line switches
		var firstArg = args[0];

		if (IsHelpSwitch(firstArg)) {
			ShowUsage();
			return 0;
		}

		if (IsVersionSwitch(firstArg)) {
			Console.WriteLine($"HashNow v{FileHasher.Version}");
			return 0;
		}

		if (IsInstallSwitch(firstArg)) {
			if (!TryGetInstallMode(args, out var installMode, out var modeError)) {
				Console.Error.WriteLine(modeError);
				return 1;
			}

			return InstallContextMenu(platform, installMode);
		}

		if (firstArg.Equals("--gui-install", StringComparison.OrdinalIgnoreCase)) {
			if (!TryGetInstallMode(args, out var installMode, out var modeError)) {
				platform.ShowInstallResult(false, modeError);
				return 1;
			}

			if (!HasExplicitInstallMode(args)) {
				var (confirmed, selectedMode) = platform.PromptInstallModeGui();
				if (!confirmed) {
					return 2;
				}

				installMode = selectedMode;
			}

			return InstallContextMenuGui(platform, installMode);
		}

		if (firstArg.Equals("--uninstall", StringComparison.OrdinalIgnoreCase)) {
			if (!TryGetInstallMode(args, out var installMode, out var modeError)) {
				Console.Error.WriteLine(modeError);
				return 1;
			}

			return UninstallContextMenu(platform, installMode);
		}

		if (firstArg.Equals("--status", StringComparison.OrdinalIgnoreCase)) {
			return ShowStatus(platform);
		}

		var hashMode = GetHashExecutionMode(args);

		// Detect if launched from file manager context menu
		bool isFromFileManager = platform.DetectFileManagerLaunch();

		// Process all file arguments
		var exitCode = 0;
		foreach (var filePath in args) {
			// Skip any arguments that look like switches
			if (filePath.StartsWith('-') || filePath.StartsWith('/')) {
				continue;
			}

			var result = await ProcessFileAsync(filePath, isFromFileManager, platform, hashMode);
			if (result != 0) {
				exitCode = result;
			}
		}

		return exitCode;
	}

	#endregion

	#region Auto-Install Logic

	/// <summary>
	/// Handles the case when no arguments are provided (double-click behavior).
	/// </summary>
	private static int HandleNoArguments(IPlatformIntegration platform) {
		var exePath = GetExecutablePath();
		bool isInstalled = platform.IsInstalled();
		bool isCorrect = platform.IsInstalledCorrectly(exePath);
		bool isDoubleClick = platform.DetectDoubleClickLaunch();

		if (isDoubleClick) {
			return HandleNoArgumentsGui(platform, isInstalled, isCorrect);
		} else {
			return HandleNoArgumentsConsole(platform, isInstalled, isCorrect);
		}
	}

	/// <summary>
	/// Handles no-arguments mode with GUI dialogs (double-click).
	/// </summary>
	private static int HandleNoArgumentsGui(IPlatformIntegration platform,
		bool isInstalled, bool isCorrect) {
		var (shouldRestart, shouldInstall, installMode, cancelled) =
			platform.ShowInstallPrompts(isInstalled, isCorrect);

		if (cancelled) return 2;

		if (shouldRestart) {
			return platform.RelaunchElevated(Array.Empty<string>()) ? 0 : 2;
		}

		if (shouldInstall) {
			return InstallContextMenuGui(platform, installMode);
		}

		return 0;
	}

	/// <summary>
	/// Handles no-arguments mode with console prompts (CLI).
	/// </summary>
	private static int HandleNoArgumentsConsole(IPlatformIntegration platform,
		bool isInstalled, bool isCorrect) {
		PrintBanner();

		if (isInstalled && isCorrect) {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("✓ HashNow is already installed!");
			Console.ResetColor();
			Console.WriteLine();
			Console.WriteLine($"Right-click any file and select \"{GetMenuItemText()}\" or \"{GetMenuItemText()} (full)\"");
			Console.WriteLine("to compute basic or full hashes.");
			Console.WriteLine();
			ShowUsageHint();
			return 0;
		}

		if (isInstalled && !isCorrect) {
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("⚠ HashNow file manager integration needs to be updated.");
			Console.ResetColor();
			Console.WriteLine("The executable has moved since installation.");
			Console.WriteLine();
		} else {
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("HashNow is not installed in your file manager.");
			Console.ResetColor();
			Console.WriteLine();
		}

		// Windows requires elevation for registry access
		if (OperatingSystem.IsWindows() && !platform.IsElevated) {
			Console.WriteLine("Administrator privileges are required to install.");
			Console.WriteLine();
			Console.Write("Would you like to restart as Administrator? [Y/n] ");
			var key = Console.ReadKey(true);
			Console.WriteLine();

			if (key.Key != ConsoleKey.N) {
				return platform.RelaunchElevated(["--install"]) ? 0 : 2;
			}
			Console.WriteLine("Installation cancelled.");
			return 2;
		}

		Console.Write("Install/update the context menu now? [Y/n] ");
		var response = Console.ReadKey(true);
		Console.WriteLine();

		if (response.Key == ConsoleKey.N) {
			Console.WriteLine("Installation cancelled.");
			ShowUsageHint();
			return 2;
		}

		var installMode = PromptInstallModeConsole();
		return InstallContextMenu(platform, installMode);
	}

	/// <summary>
	/// Installs context menu with GUI result dialog.
	/// </summary>
	private static int InstallContextMenuGui(
		IPlatformIntegration platform,
		ContextMenuInstallMode installMode = ContextMenuInstallMode.Both) {
		try {
			var exePath = GetExecutablePath();
			platform.Install(exePath, installMode);
			platform.ShowInstallResult(true);
			return 0;
		} catch (UnauthorizedAccessException) {
			platform.ShowInstallResult(false, "Administrator privileges required.");
			return 1;
		} catch (Exception ex) {
			platform.ShowInstallResult(false, ex.Message);
			return 1;
		}
	}

	#endregion

	#region File Processing

	/// <summary>
	/// Processes a single file, computing all hashes and saving results.
	/// </summary>
	private static async Task<int> ProcessFileAsync(
		string filePath,
		bool isFromFileManager,
		IPlatformIntegration platform,
		HashExecutionMode mode) {
		try {
			if (!File.Exists(filePath)) {
				if (!isFromFileManager) {
					Console.Error.WriteLine($"Error: File not found: {filePath}");
				}
				return 1;
			}

			var fileInfo = new FileInfo(filePath);

			if (!isFromFileManager) {
				return await ProcessFileConsoleMode(filePath, fileInfo, mode);
			} else {
				return await ProcessFileFileManagerMode(filePath, fileInfo, platform, mode);
			}
		} catch (UnauthorizedAccessException) {
			if (!isFromFileManager) {
				Console.Error.WriteLine($"Error: Access denied to file: {filePath}");
			}
			return 1;
		} catch (IOException ex) {
			if (!isFromFileManager) {
				Console.Error.WriteLine($"Error reading file {filePath}: {ex.Message}");
			}
			return 1;
		} catch (Exception ex) {
			if (!isFromFileManager) {
				Console.Error.WriteLine($"Error processing {filePath}: {ex.Message}");
			}
			return 1;
		}
	}

	/// <summary>
	/// Processes a file with full console output (progress bar, results, timing).
	/// </summary>
	private static async Task<int> ProcessFileConsoleMode(
		string filePath,
		FileInfo fileInfo,
		HashExecutionMode mode) {
		var modeLabel = mode == HashExecutionMode.Basic ? "basic 5" : $"{FileHasher.AlgorithmCount}";
		Console.WriteLine($"Hashing ({modeLabel}): {fileInfo.Name} ({FileHasher.FormatFileSize(fileInfo.Length)})");

		using var progressBar = new ConsoleProgressBar(useColor: true);

		if (mode == HashExecutionMode.Basic) {
			var basicResult = await FileHasher.HashFileBasicAsync(
				filePath,
				progress => progressBar.Update(progress));

			progressBar.Complete();

			var basicOutputPath = filePath + ".hashes.json";
			await FileHasher.SaveResultAsync(basicResult, basicOutputPath);

			Console.WriteLine($"MD5:    {basicResult.Md5}");
			Console.WriteLine($"CRC32:  {basicResult.Crc32}");
			Console.WriteLine($"SHA-1:  {basicResult.Sha1}");
			Console.WriteLine($"SHA256: {basicResult.Sha256}");
			Console.WriteLine($"SHA512: {basicResult.Sha512}");
			Console.WriteLine($"Saved:  {basicOutputPath}");
			Console.WriteLine($"Time:   {basicResult.DurationMs}ms");
			Console.WriteLine();
			return 0;
		}

		var result = await FileHasher.HashFileAsync(filePath, progress => progressBar.Update(progress));

		progressBar.Complete();

		var outputPath = filePath + ".hashes.json";
		await FileHasher.SaveResultAsync(result, outputPath);

		PrintResults(result);

		Console.WriteLine($"Saved:  {outputPath}");
		Console.WriteLine($"Time:   {result.DurationMs}ms");
		Console.WriteLine();

		return 0;
	}

	/// <summary>
	/// Processes a file from file manager mode with platform-appropriate progress.
	/// </summary>
	private static async Task<int> ProcessFileFileManagerMode(
		string filePath,
		FileInfo fileInfo,
		IPlatformIntegration platform,
		HashExecutionMode mode) {
		if (mode == HashExecutionMode.Basic) {
			var basicResult = await FileHasher.HashFileBasicAsync(filePath);
			var basicOutputPath = filePath + ".hashes.json";
			await FileHasher.SaveResultAsync(basicResult, basicOutputPath);
			return 0;
		}

		// Always use platform progress — each platform handles its own fallback chain
		// (Windows: WinForms dialog, Linux: zenity/kdialog, macOS: osascript/zenity)
		var result = await platform.HashFileWithProgress(filePath);

		if (result is not null) {
			var outputPath = filePath + ".hashes.json";
			await FileHasher.SaveResultAsync(result, outputPath);
			return 0;
		}

		return 2; // Cancelled
	}

	#endregion

	#region Results Display

	/// <summary>
	/// Prints all hash results to the console in a formatted layout.
	/// </summary>
	private static void PrintResults(FileHashResult result) {
		Console.WriteLine("--- Checksums & CRCs ---");
		Console.WriteLine($"CRC32:      {result.Crc32}");
		Console.WriteLine($"CRC32C:     {result.Crc32C}");
		Console.WriteLine($"CRC64:      {result.Crc64}");
		Console.WriteLine($"Adler-32:   {result.Adler32}");
		Console.WriteLine($"Fletcher16: {result.Fletcher16}");
		Console.WriteLine($"Fletcher32: {result.Fletcher32}");

		Console.WriteLine("--- Fast Non-Crypto Hashes ---");
		Console.WriteLine($"XXH32:      {result.XxHash32}");
		Console.WriteLine($"XXH64:      {result.XxHash64}");
		Console.WriteLine($"XXH3:       {result.XxHash3}");
		Console.WriteLine($"XXH128:     {result.XxHash128}");
		Console.WriteLine($"Murmur3-32: {result.Murmur3_32}");
		Console.WriteLine($"Murmur3-128:{result.Murmur3_128}");
		Console.WriteLine($"City64:     {result.CityHash64}");
		Console.WriteLine($"City128:    {result.CityHash128}");
		Console.WriteLine($"Farm64:     {result.FarmHash64}");
		Console.WriteLine($"SpookyV2:   {result.SpookyV2_128}");
		Console.WriteLine($"SipHash24:  {result.SipHash24}");
		Console.WriteLine($"Highway64:  {result.HighwayHash64}");

		Console.WriteLine("--- MD Family ---");
		Console.WriteLine($"MD2:        {result.Md2}");
		Console.WriteLine($"MD4:        {result.Md4}");
		Console.WriteLine($"MD5:        {result.Md5}");

		Console.WriteLine("--- SHA-1/2 Family ---");
		Console.WriteLine($"SHA-0:      {result.Sha0}");
		Console.WriteLine($"SHA-1:      {result.Sha1}");
		Console.WriteLine($"SHA-224:    {result.Sha224}");
		Console.WriteLine($"SHA-256:    {result.Sha256}");
		Console.WriteLine($"SHA-384:    {result.Sha384}");
		Console.WriteLine($"SHA-512:    {result.Sha512}");
		Console.WriteLine($"SHA512/224: {result.Sha512_224}");
		Console.WriteLine($"SHA512/256: {result.Sha512_256}");

		Console.WriteLine("--- SHA-3 & Keccak ---");
		Console.WriteLine($"SHA3-224:   {result.Sha3_224}");
		Console.WriteLine($"SHA3-256:   {result.Sha3_256}");
		Console.WriteLine($"SHA3-384:   {result.Sha3_384}");
		Console.WriteLine($"SHA3-512:   {result.Sha3_512}");
		Console.WriteLine($"Keccak-256: {result.Keccak256}");
		Console.WriteLine($"Keccak-512: {result.Keccak512}");

		Console.WriteLine("--- BLAKE Family ---");
		Console.WriteLine($"BLAKE-256:  {result.Blake256}");
		Console.WriteLine($"BLAKE-512:  {result.Blake512}");
		Console.WriteLine($"BLAKE2b:    {result.Blake2b}");
		Console.WriteLine($"BLAKE2s:    {result.Blake2s}");
		Console.WriteLine($"BLAKE3:     {result.Blake3}");

		Console.WriteLine("--- RIPEMD Family ---");
		Console.WriteLine($"RIPEMD-128: {result.Ripemd128}");
		Console.WriteLine($"RIPEMD-160: {result.Ripemd160}");
		Console.WriteLine($"RIPEMD-256: {result.Ripemd256}");
		Console.WriteLine($"RIPEMD-320: {result.Ripemd320}");

		Console.WriteLine("--- Other Crypto Hashes ---");
		Console.WriteLine($"Whirlpool:  {result.Whirlpool}");
		Console.WriteLine($"Tiger-192:  {result.Tiger192}");
		Console.WriteLine($"GOST-94:    {result.Gost94}");
		Console.WriteLine($"Streebog256:{result.Streebog256}");
		Console.WriteLine($"Streebog512:{result.Streebog512}");
		Console.WriteLine($"Skein-256:  {result.Skein256}");
		Console.WriteLine($"Skein-512:  {result.Skein512}");
		Console.WriteLine($"Skein-1024: {result.Skein1024}");
		Console.WriteLine($"Groestl-256:{result.Groestl256}");
		Console.WriteLine($"Groestl-512:{result.Groestl512}");
		Console.WriteLine($"JH-256:     {result.Jh256}");
		Console.WriteLine($"JH-512:     {result.Jh512}");
		Console.WriteLine($"K12:        {result.KangarooTwelve}");
		Console.WriteLine($"SM3:        {result.Sm3}");
	}

	#endregion

	#region Context Menu Commands

	/// <summary>
	/// Installs the file manager context menu entry.
	/// </summary>
	private static int InstallContextMenu(
		IPlatformIntegration platform,
		ContextMenuInstallMode installMode = ContextMenuInstallMode.Both) {
		try {
			var exePath = GetExecutablePath();
			platform.Install(exePath, installMode);

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("✓ Context menu installed successfully!");
			Console.ResetColor();
			Console.WriteLine();
			Console.WriteLine(GetInstallSuccessSummary(installMode));
			Console.WriteLine();
			Console.WriteLine("A .hashes.json file will be created next to the original.");

			return 0;
		} catch (UnauthorizedAccessException) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine("✗ Error: Administrator/root privileges required.");
			Console.ResetColor();
			if (OperatingSystem.IsWindows()) {
				Console.Error.WriteLine();
				Console.Error.WriteLine("Please run HashNow as Administrator to install the context menu.");
				Console.Error.WriteLine("Right-click HashNow.exe and select 'Run as administrator'.");
			}
			return 1;
		} catch (Exception ex) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine($"✗ Error installing context menu: {ex.Message}");
			Console.ResetColor();
			return 1;
		}
	}

	/// <summary>
	/// Removes the file manager context menu entry.
	/// </summary>
	private static int UninstallContextMenu(
		IPlatformIntegration platform,
		ContextMenuInstallMode installMode = ContextMenuInstallMode.Both) {
		try {
			platform.Uninstall(installMode);

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("✓ Context menu removed successfully!");
			Console.ResetColor();

			return 0;
		} catch (UnauthorizedAccessException) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine("✗ Error: Administrator/root privileges required.");
			Console.ResetColor();
			return 1;
		} catch (Exception ex) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine($"✗ Error uninstalling context menu: {ex.Message}");
			Console.ResetColor();
			return 1;
		}
	}

	/// <summary>
	/// Shows the current installation status.
	/// </summary>
	private static int ShowStatus(IPlatformIntegration platform) {
		PrintBanner();

		Console.WriteLine("Installation Status:");
		Console.WriteLine("────────────────────");
		Console.WriteLine($"Platform:   {platform.PlatformName}");

		foreach (var detail in platform.GetStatusDetails()) {
			if (detail.StartsWith('✓')) {
				Console.ForegroundColor = ConsoleColor.Green;
			} else if (detail.StartsWith('⚠')) {
				Console.ForegroundColor = ConsoleColor.Yellow;
			} else if (detail.StartsWith('✗')) {
				Console.ForegroundColor = ConsoleColor.Red;
			}
			Console.WriteLine(detail);
			Console.ResetColor();
		}

		Console.WriteLine();
		Console.WriteLine($"Version:    {FileHasher.Version}");
		Console.WriteLine($"Algorithms: {FileHasher.AlgorithmCount}");

		return 0;
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Prints the application banner.
	/// </summary>
	private static void PrintBanner() {
		Console.WriteLine();
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine($"╔══════════════════════════════════════════════════╗");
		Console.WriteLine($"║          HashNow v{FileHasher.Version,-10}                    ║");
		Console.WriteLine($"║       Instant File Hashing (70 algorithms)       ║");
		Console.WriteLine($"╚══════════════════════════════════════════════════╝");
		Console.ResetColor();
		Console.WriteLine();
	}

	/// <summary>
	/// Shows a brief hint about command-line usage.
	/// </summary>
	private static void ShowUsageHint() {
		Console.ForegroundColor = ConsoleColor.DarkGray;
		Console.WriteLine("Tip: Run 'HashNow --help' for command-line usage.");
		Console.ResetColor();
	}

	/// <summary>
	/// Gets the full path to the currently running executable.
	/// </summary>
	private static string GetExecutablePath() {
		var exePath = Environment.ProcessPath;

		if (string.IsNullOrEmpty(exePath)) {
			exePath = Process.GetCurrentProcess().MainModule?.FileName;
		}

		if (string.IsNullOrEmpty(exePath)) {
			var exeName = OperatingSystem.IsWindows() ? "HashNow.exe" : "HashNow";
			exePath = Path.Combine(AppContext.BaseDirectory, exeName);
		}

		return exePath;
	}

	/// <summary>
	/// Gets the menu item text.
	/// </summary>
	private static string GetMenuItemText() => "Hash this file now";

	/// <summary>
	/// Gets the hash execution mode from command-line arguments.
	/// </summary>
	private static HashExecutionMode GetHashExecutionMode(string[] args) {
		return args.Any(arg => arg.Equals("--basic", StringComparison.OrdinalIgnoreCase))
			? HashExecutionMode.Basic
			: HashExecutionMode.Full;
	}

	/// <summary>
	/// Prompts for context menu installation mode in console mode.
	/// </summary>
	private static ContextMenuInstallMode PromptInstallModeConsole() {
		Console.WriteLine("Choose which context menu items to install:");
		Console.WriteLine("  1) Full only   (Hash this file now (full) - all 70+)");
		Console.WriteLine("  2) Basic only  (Hash this file now - CRC32/MD5/SHA-1/SHA-256/SHA-512)");
		Console.WriteLine("  3) Both");
		Console.Write("Select [3]: ");

		var input = Console.ReadLine()?.Trim();
		return input switch {
			"1" => ContextMenuInstallMode.Full,
			"2" => ContextMenuInstallMode.Basic,
			_ => ContextMenuInstallMode.Both
		};
	}

	/// <summary>
	/// Gets a user-facing install success summary for the selected mode.
	/// </summary>
	private static string GetInstallSuccessSummary(ContextMenuInstallMode mode) {
		return mode switch {
			ContextMenuInstallMode.Full => "Installed: \"Hash this file now (full)\" for all 70+ algorithms.",
			ContextMenuInstallMode.Basic => "Installed: \"Hash this file now\" for basic 5 algorithms (CRC32/MD5/SHA-1/SHA-256/SHA-512).",
			_ => "Installed both: \"Hash this file now\" (basic 5) and \"Hash this file now (full)\" (all 70+)."
		};
	}

	/// <summary>
	/// Parses install mode options from CLI args.
	/// </summary>
	private static bool TryGetInstallMode(
		string[] args,
		out ContextMenuInstallMode mode,
		out string? error) {
		mode = ContextMenuInstallMode.Both;
		error = null;

		if (args.Any(arg => arg.Equals("--install-full", StringComparison.OrdinalIgnoreCase))) {
			mode = ContextMenuInstallMode.Full;
			return true;
		}

		if (args.Any(arg => arg.Equals("--install-basic", StringComparison.OrdinalIgnoreCase))) {
			mode = ContextMenuInstallMode.Basic;
			return true;
		}

		if (args.Any(arg => arg.Equals("--install-both", StringComparison.OrdinalIgnoreCase))) {
			mode = ContextMenuInstallMode.Both;
			return true;
		}

		var menuIndex = Array.FindIndex(args, arg => arg.Equals("--menu", StringComparison.OrdinalIgnoreCase));
		if (menuIndex < 0) {
			return true;
		}

		if (menuIndex == args.Length - 1) {
			error = "Error: --menu requires a value: full, basic, or both.";
			return false;
		}

		var value = args[menuIndex + 1];
		if (value.Equals("full", StringComparison.OrdinalIgnoreCase)) {
			mode = ContextMenuInstallMode.Full;
			return true;
		}

		if (value.Equals("basic", StringComparison.OrdinalIgnoreCase)) {
			mode = ContextMenuInstallMode.Basic;
			return true;
		}

		if (value.Equals("both", StringComparison.OrdinalIgnoreCase)) {
			mode = ContextMenuInstallMode.Both;
			return true;
		}

		error = $"Error: Invalid --menu value '{value}'. Use full, basic, or both.";
		return false;
	}

	/// <summary>
	/// Checks if an argument is any install switch.
	/// </summary>
	private static bool IsInstallSwitch(string arg) =>
		arg.Equals("--install", StringComparison.OrdinalIgnoreCase) ||
		arg.Equals("--install-full", StringComparison.OrdinalIgnoreCase) ||
		arg.Equals("--install-basic", StringComparison.OrdinalIgnoreCase) ||
		arg.Equals("--install-both", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Checks whether CLI arguments explicitly select a context menu install mode.
	/// </summary>
	private static bool HasExplicitInstallMode(string[] args) {
		if (args.Any(arg => arg.Equals("--install-full", StringComparison.OrdinalIgnoreCase)
			|| arg.Equals("--install-basic", StringComparison.OrdinalIgnoreCase)
			|| arg.Equals("--install-both", StringComparison.OrdinalIgnoreCase))) {
			return true;
		}

		return Array.FindIndex(args, arg => arg.Equals("--menu", StringComparison.OrdinalIgnoreCase)) >= 0;
	}

	/// <summary>
	/// Checks if an argument is a help switch.
	/// </summary>
	private static bool IsHelpSwitch(string arg) =>
		arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
		arg.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
		arg.Equals("/?", StringComparison.OrdinalIgnoreCase) ||
		arg.Equals("/help", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Checks if an argument is a version switch.
	/// </summary>
	private static bool IsVersionSwitch(string arg) =>
		arg.Equals("--version", StringComparison.OrdinalIgnoreCase) ||
		arg.Equals("-v", StringComparison.OrdinalIgnoreCase) ||
		arg.Equals("/version", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Displays the full usage information.
	/// </summary>
	private static void ShowUsage() {
		var platformHint = OperatingSystem.IsWindows()
			? "Windows Explorer"
			: OperatingSystem.IsMacOS()
				? "Finder"
				: "your file manager";

		Console.WriteLine($@"HashNow v{FileHasher.Version} - Instant File Hashing ({FileHasher.AlgorithmCount} algorithms)

Usage:
  HashNow                              Auto-install: check and prompt to install
	HashNow <file> [file2] [file3] ...   Hash one or more files (full mode)
	HashNow --basic <file> [file2] ...   Hash one or more files (basic 5 algorithms)
	HashNow --install [--menu <mode>]    Install file manager context menu
	HashNow --install-full               Install only full context menu item
	HashNow --install-basic              Install only basic context menu item
	HashNow --install-both               Install both context menu items
	HashNow --uninstall [--menu <mode>]  Remove context menu item(s)
  HashNow --status                     Show installation status
  HashNow --help                       Show this help
  HashNow --version                    Show version

Algorithms ({FileHasher.AlgorithmCount} total, computed in parallel):
  Checksums:   CRC32, CRC32C, CRC64, Adler-32, Fletcher-16, Fletcher-32
  Fast:        XXHash32/64/3/128, MurmurHash3, CityHash, FarmHash, SpookyV2, SipHash
  MD Family:   MD2, MD4, MD5
  SHA-1/2:     SHA-0, SHA-1, SHA-224/256/384/512, SHA-512/224, SHA-512/256
  SHA-3:       SHA3-224/256/384/512, Keccak-256/512
  BLAKE:       BLAKE-256/512, BLAKE2b/2s, BLAKE3
  RIPEMD:      RIPEMD-128/160/256/320
  Other:       Whirlpool, Tiger, GOST, Streebog, Skein, Groestl, JH, K12, SM3

Output:
  Creates {{filename}}.hashes.json containing all hashes and metadata.
  JSON is formatted with tab indentation for readability.

Quick Install:
  Just run HashNow without arguments and follow the prompts!
	Right-click any file in {platformHint} to hash it.

Context Menu Modes:
	full:  Hash this file now (full) -> all {FileHasher.AlgorithmCount} algorithms
	basic: Hash this file now        -> CRC32, MD5, SHA-1, SHA-256, SHA-512
	both:  Install both entries

Examples:
  HashNow                              Install via interactive prompt
  HashNow myfile.zip                   Hash a single file
	HashNow --basic myfile.zip           Hash with basic 5 algorithms
  HashNow *.iso                        Hash multiple files (shell expansion)
	HashNow --install --menu both        Add both right-click menu options
");
	}

	#endregion
}
