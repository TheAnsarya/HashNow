using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using HashNow.Core;

namespace HashNow.Cli;

/// <summary>
/// HashNow CLI entry point - Computes 58 hash algorithms for files.
/// </summary>
/// <remarks>
/// <para>
/// This is the main entry point for the HashNow command-line application.
/// It provides a simple, user-friendly interface for computing file hashes.
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
///       <strong>Explorer Mode:</strong> Invoked via right-click context menu.
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
/// Creates a <c>{filename}.hashes.json</c> file containing all 58 hash values
/// and file metadata in a human-readable, tab-indented JSON format.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// # Hash a file from command line
/// HashNow myfile.zip
///
/// # Install context menu (requires admin)
/// HashNow --install
///
/// # Double-click the exe to auto-install (prompts for admin)
/// </code>
/// </example>
[SupportedOSPlatform("windows")]
internal static class Program {
	#region Constants

	/// <summary>
	/// Time threshold in milliseconds before showing progress UI for long operations.
	/// </summary>
	/// <remarks>
	/// If estimated hash time exceeds this value, we show progress feedback to the user.
	/// Set to 3000ms (3 seconds) as a reasonable threshold for user patience.
	/// </remarks>
	private const long ProgressUiThresholdMs = 3000;

	/// <summary>
	/// Environment variable name used to detect Explorer-launched instances.
	/// </summary>
	/// <remarks>
	/// This can be set to "1" to force Explorer mode behavior (no console output).
	/// </remarks>
	private const string ExplorerEnvVar = "HASHNOW_EXPLORER";

	#endregion

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
	///   <item><description><c>--install</c>: Install Explorer context menu</description></item>
	///   <item><description><c>--uninstall</c>: Remove Explorer context menu</description></item>
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
		// No arguments provided - auto-install mode
		// This enables "double-click to install" behavior
		if (args.Length == 0) {
			return HandleNoArguments();
		}

		// Parse command-line switches
		var firstArg = args[0];

		// Help command
		if (IsHelpSwitch(firstArg)) {
			ShowUsage();
			return 0;
		}

		// Version command
		if (IsVersionSwitch(firstArg)) {
			Console.WriteLine($"HashNow v{FileHasher.Version}");
			return 0;
		}

		// Install context menu command
		if (firstArg.Equals("--install", StringComparison.OrdinalIgnoreCase)) {
			return InstallContextMenu();
		}

		// Uninstall context menu command
		if (firstArg.Equals("--uninstall", StringComparison.OrdinalIgnoreCase)) {
			return UninstallContextMenu();
		}

		// Status check command
		if (firstArg.Equals("--status", StringComparison.OrdinalIgnoreCase)) {
			return ShowStatus();
		}

		// Detect if launched from Explorer (no attached console or env var set)
		bool isFromExplorer = DetectExplorerLaunch();

		// Process all file arguments
		var exitCode = 0;
		foreach (var filePath in args) {
			// Skip any arguments that look like switches
			if (filePath.StartsWith('-') || filePath.StartsWith('/')) {
				continue;
			}

			var result = await ProcessFileAsync(filePath, isFromExplorer);
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
	/// <returns>Exit code indicating success or failure.</returns>
	/// <remarks>
	/// <para>
	/// This method implements the "double-click to install" feature:
	/// </para>
	/// <list type="number">
	///   <item><description>Check if context menu is already installed correctly</description></item>
	///   <item><description>If installed: Show success message and exit</description></item>
	///   <item><description>If not installed: Prompt user and attempt installation</description></item>
	///   <item><description>If not admin: Offer to restart with elevation</description></item>
	/// </list>
	/// </remarks>
	private static int HandleNoArguments() {
		// First, check current installation status
		bool isInstalled = ContextMenuInstaller.IsInstalled();
		bool isCorrect = ContextMenuInstaller.IsInstalledCorrectly();

		// Banner
		PrintBanner();

		if (isInstalled && isCorrect) {
			// Already installed correctly
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("✓ HashNow is already installed!");
			Console.ResetColor();
			Console.WriteLine();
			Console.WriteLine("Right-click any file in Windows Explorer and select");
			Console.WriteLine("\"Hash this file now\" to compute hashes.");
			Console.WriteLine();
			ShowUsageHint();
			return 0;
		}

		if (isInstalled && !isCorrect) {
			// Installed but pointing to wrong location (exe was moved)
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("⚠ HashNow context menu needs to be updated.");
			Console.ResetColor();
			Console.WriteLine("The executable has moved since installation.");
			Console.WriteLine();
		} else {
			// Not installed at all
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("HashNow is not installed in the context menu.");
			Console.ResetColor();
			Console.WriteLine();
		}

		// Check if we're running as admin
		if (!IsRunningAsAdmin()) {
			Console.WriteLine("Administrator privileges are required to install.");
			Console.WriteLine();
			Console.Write("Would you like to restart as Administrator? [Y/n] ");

			var key = Console.ReadKey(true);
			Console.WriteLine();

			if (key.Key != ConsoleKey.N) {
				// Restart with elevation
				return RestartAsAdmin();
			} else {
				Console.WriteLine("Installation cancelled.");
				return 2;
			}
		}

		// We have admin rights - prompt to install
		Console.Write("Install the context menu now? [Y/n] ");
		var response = Console.ReadKey(true);
		Console.WriteLine();

		if (response.Key == ConsoleKey.N) {
			Console.WriteLine("Installation cancelled.");
			ShowUsageHint();
			return 2;
		}

		// Perform installation
		return InstallContextMenu();
	}

	/// <summary>
	/// Prints the application banner with version and description.
	/// </summary>
	private static void PrintBanner() {
		Console.WriteLine();
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine($"╔══════════════════════════════════════════════════╗");
		Console.WriteLine($"║          HashNow v{FileHasher.Version,-10}                    ║");
		Console.WriteLine($"║       Instant File Hashing (58 algorithms)       ║");
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
	/// Checks if the current process has administrator privileges.
	/// </summary>
	/// <returns><see langword="true"/> if running elevated; otherwise, <see langword="false"/>.</returns>
	private static bool IsRunningAsAdmin() {
		try {
			using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
			var principal = new System.Security.Principal.WindowsPrincipal(identity);
			return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
		} catch {
			return false;
		}
	}

	/// <summary>
	/// Restarts the application with administrator privileges via UAC prompt.
	/// </summary>
	/// <returns>Exit code (typically doesn't return as process exits).</returns>
	private static int RestartAsAdmin() {
		try {
			var exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
			if (string.IsNullOrEmpty(exePath)) {
				Console.Error.WriteLine("Error: Could not determine executable path.");
				return 1;
			}

			var startInfo = new ProcessStartInfo {
				FileName = exePath,
				UseShellExecute = true,
				Verb = "runas" // This triggers UAC elevation prompt
			};

			Process.Start(startInfo);

			// Exit current (non-elevated) process
			Environment.Exit(0);
			return 0;
		} catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223) {
			// ERROR_CANCELLED - User declined UAC prompt
			Console.WriteLine("Elevation cancelled by user.");
			return 2;
		} catch (Exception ex) {
			Console.Error.WriteLine($"Error requesting elevation: {ex.Message}");
			return 1;
		}
	}

	#endregion

	#region File Processing

	/// <summary>
	/// Processes a single file, computing all hashes and saving results.
	/// </summary>
	/// <param name="filePath">Path to the file to hash.</param>
	/// <param name="isFromExplorer">
	/// <see langword="true"/> if launched from Explorer (minimal console output);
	/// <see langword="false"/> for full console output.
	/// </param>
	/// <returns>Exit code: 0 for success, 1 for error.</returns>
	private static async Task<int> ProcessFileAsync(string filePath, bool isFromExplorer) {
		try {
			// Validate file exists
			if (!File.Exists(filePath)) {
				if (!isFromExplorer) {
					Console.Error.WriteLine($"Error: File not found: {filePath}");
				}
				return 1;
			}

			var fileInfo = new FileInfo(filePath);
			var estimatedMs = FileHasher.EstimateHashDurationMs(fileInfo.Length);

			if (!isFromExplorer) {
				// Console mode - show detailed progress and results
				return await ProcessFileConsoleMode(filePath, fileInfo);
			} else {
				// Explorer mode - silent or minimal UI
				return await ProcessFileExplorerMode(filePath, fileInfo, estimatedMs);
			}
		} catch (UnauthorizedAccessException) {
			if (!isFromExplorer) {
				Console.Error.WriteLine($"Error: Access denied to file: {filePath}");
			}
			return 1;
		} catch (IOException ex) {
			if (!isFromExplorer) {
				Console.Error.WriteLine($"Error reading file {filePath}: {ex.Message}");
			}
			return 1;
		} catch (Exception ex) {
			if (!isFromExplorer) {
				Console.Error.WriteLine($"Error processing {filePath}: {ex.Message}");
			}
			return 1;
		}
	}

	/// <summary>
	/// Processes a file with full console output (progress, results, timing).
	/// </summary>
	/// <param name="filePath">Path to the file.</param>
	/// <param name="fileInfo">FileInfo object for the file.</param>
	/// <returns>Exit code.</returns>
	private static async Task<int> ProcessFileConsoleMode(string filePath, FileInfo fileInfo) {
		// Display file info header
		Console.WriteLine($"Hashing: {fileInfo.Name} ({FileHasher.FormatFileSize(fileInfo.Length)})");

		// Hash with progress callback
		var result = await FileHasher.HashFileAsync(
			filePath,
			progress => {
				// Update progress on same line using carriage return
				Console.Write($"\rProgress: {progress:P0}  ");
			});

		// Clear progress line and show completion
		Console.WriteLine("\rProgress: 100%    ");

		// Save results to JSON
		var outputPath = filePath + ".hashes.json";
		await FileHasher.SaveResultAsync(result, outputPath);

		// Print all hash results to console
		PrintResults(result);

		// Show summary
		Console.WriteLine($"Saved:  {outputPath}");
		Console.WriteLine($"Time:   {result.DurationMs}ms");
		Console.WriteLine();

		return 0;
	}

	/// <summary>
	/// Processes a file silently or with minimal UI (Explorer mode).
	/// </summary>
	/// <param name="filePath">Path to the file.</param>
	/// <param name="fileInfo">FileInfo object for the file.</param>
	/// <param name="estimatedMs">Estimated processing time in milliseconds.</param>
	/// <returns>Exit code.</returns>
	private static async Task<int> ProcessFileExplorerMode(string filePath, FileInfo fileInfo, long estimatedMs) {
		if (estimatedMs > ProgressUiThresholdMs) {
			// Long operation - could show a progress dialog here
			// For now, just process in background
			await ProcessWithProgressDialogAsync(filePath, fileInfo);
		} else {
			// Short operation - process silently
			var result = await FileHasher.HashFileAsync(filePath);
			var outputPath = filePath + ".hashes.json";
			await FileHasher.SaveResultAsync(result, outputPath);
		}

		return 0;
	}

	/// <summary>
	/// Processes a file with optional progress dialog for long operations.
	/// </summary>
	/// <param name="filePath">Path to the file.</param>
	/// <param name="fileInfo">FileInfo object for the file.</param>
	/// <remarks>
	/// This method is designed to support a future progress dialog UI.
	/// Currently processes silently but tracks progress internally.
	/// </remarks>
	private static async Task ProcessWithProgressDialogAsync(string filePath, FileInfo fileInfo) {
		// Create cancellation source for potential user cancellation
		using var cts = new CancellationTokenSource();

		// Track progress for potential UI updates
		double currentProgress = 0;

		// Start hashing task with progress callback
		var hashTask = FileHasher.HashFileAsync(
			filePath,
			progress => currentProgress = progress,
			cts.Token);

		// Monitor progress while hashing
		var sw = Stopwatch.StartNew();
		bool dialogShown = false;

		while (!hashTask.IsCompleted) {
			await Task.Delay(100, CancellationToken.None);

			// Placeholder for future progress dialog integration
			if (sw.ElapsedMilliseconds > ProgressUiThresholdMs && !dialogShown) {
				dialogShown = true;
				// TODO: Could show a WinForms/WPF progress dialog here
				// For now, continue processing silently
			}
		}

		// Save results
		var result = await hashTask;
		var outputPath = filePath + ".hashes.json";
		await FileHasher.SaveResultAsync(result, outputPath);
	}

	/// <summary>
	/// Detects if the application was launched from Windows Explorer.
	/// </summary>
	/// <returns><see langword="true"/> if launched from Explorer; otherwise, <see langword="false"/>.</returns>
	/// <remarks>
	/// Detection methods:
	/// <list type="bullet">
	///   <item><description>Check for <c>HASHNOW_EXPLORER</c> environment variable</description></item>
	///   <item><description>Check if input is redirected (no attached console)</description></item>
	/// </list>
	/// </remarks>
	private static bool DetectExplorerLaunch() {
		// Check explicit environment variable
		if (Environment.GetEnvironmentVariable(ExplorerEnvVar) == "1") {
			return true;
		}

		// Check if running without console input (typical for Explorer launch)
		return Console.IsInputRedirected;
	}

	#endregion

	#region Results Display

	/// <summary>
	/// Prints all hash results to the console in a formatted layout.
	/// </summary>
	/// <param name="result">The hash result to display.</param>
	/// <remarks>
	/// Results are organized by category with aligned columns for readability.
	/// </remarks>
	private static void PrintResults(FileHashResult result) {
		// Checksums & CRCs
		Console.WriteLine("--- Checksums & CRCs ---");
		Console.WriteLine($"CRC32:      {result.Crc32}");
		Console.WriteLine($"CRC32C:     {result.Crc32C}");
		Console.WriteLine($"CRC64:      {result.Crc64}");
		Console.WriteLine($"Adler-32:   {result.Adler32}");
		Console.WriteLine($"Fletcher16: {result.Fletcher16}");
		Console.WriteLine($"Fletcher32: {result.Fletcher32}");

		// Fast Non-Crypto Hashes
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

		// MD Family
		Console.WriteLine("--- MD Family ---");
		Console.WriteLine($"MD2:        {result.Md2}");
		Console.WriteLine($"MD4:        {result.Md4}");
		Console.WriteLine($"MD5:        {result.Md5}");

		// SHA-1/2 Family
		Console.WriteLine("--- SHA-1/2 Family ---");
		Console.WriteLine($"SHA-0:      {result.Sha0}");
		Console.WriteLine($"SHA-1:      {result.Sha1}");
		Console.WriteLine($"SHA-224:    {result.Sha224}");
		Console.WriteLine($"SHA-256:    {result.Sha256}");
		Console.WriteLine($"SHA-384:    {result.Sha384}");
		Console.WriteLine($"SHA-512:    {result.Sha512}");
		Console.WriteLine($"SHA512/224: {result.Sha512_224}");
		Console.WriteLine($"SHA512/256: {result.Sha512_256}");

		// SHA-3 & Keccak
		Console.WriteLine("--- SHA-3 & Keccak ---");
		Console.WriteLine($"SHA3-224:   {result.Sha3_224}");
		Console.WriteLine($"SHA3-256:   {result.Sha3_256}");
		Console.WriteLine($"SHA3-384:   {result.Sha3_384}");
		Console.WriteLine($"SHA3-512:   {result.Sha3_512}");
		Console.WriteLine($"Keccak-256: {result.Keccak256}");
		Console.WriteLine($"Keccak-512: {result.Keccak512}");

		// BLAKE Family
		Console.WriteLine("--- BLAKE Family ---");
		Console.WriteLine($"BLAKE-256:  {result.Blake256}");
		Console.WriteLine($"BLAKE-512:  {result.Blake512}");
		Console.WriteLine($"BLAKE2b:    {result.Blake2b}");
		Console.WriteLine($"BLAKE2s:    {result.Blake2s}");
		Console.WriteLine($"BLAKE3:     {result.Blake3}");

		// RIPEMD Family
		Console.WriteLine("--- RIPEMD Family ---");
		Console.WriteLine($"RIPEMD-128: {result.Ripemd128}");
		Console.WriteLine($"RIPEMD-160: {result.Ripemd160}");
		Console.WriteLine($"RIPEMD-256: {result.Ripemd256}");
		Console.WriteLine($"RIPEMD-320: {result.Ripemd320}");

		// Other Crypto Hashes
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
	/// Installs the Windows Explorer context menu entry.
	/// </summary>
	/// <returns>Exit code: 0 for success, 1 for error.</returns>
	private static int InstallContextMenu() {
		try {
			ContextMenuInstaller.Install();

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("✓ Context menu installed successfully!");
			Console.ResetColor();
			Console.WriteLine();
			Console.WriteLine("Right-click any file in Windows Explorer and select");
			Console.WriteLine("\"Hash this file now\" to compute all 58 hashes.");
			Console.WriteLine();
			Console.WriteLine("A .hashes.json file will be created next to the original.");

			return 0;
		} catch (UnauthorizedAccessException) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine("✗ Error: Administrator privileges required.");
			Console.ResetColor();
			Console.Error.WriteLine();
			Console.Error.WriteLine("Please run HashNow as Administrator to install the context menu.");
			Console.Error.WriteLine("Right-click HashNow.exe and select 'Run as administrator'.");
			return 1;
		} catch (Exception ex) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine($"✗ Error installing context menu: {ex.Message}");
			Console.ResetColor();
			return 1;
		}
	}

	/// <summary>
	/// Removes the Windows Explorer context menu entry.
	/// </summary>
	/// <returns>Exit code: 0 for success, 1 for error.</returns>
	private static int UninstallContextMenu() {
		try {
			ContextMenuInstaller.Uninstall();

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("✓ Context menu removed successfully!");
			Console.ResetColor();

			return 0;
		} catch (UnauthorizedAccessException) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine("✗ Error: Administrator privileges required.");
			Console.ResetColor();
			Console.Error.WriteLine();
			Console.Error.WriteLine("Please run HashNow as Administrator to uninstall the context menu.");
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
	/// <returns>Exit code: 0 always (informational only).</returns>
	private static int ShowStatus() {
		PrintBanner();

		bool isInstalled = ContextMenuInstaller.IsInstalled();
		bool isCorrect = ContextMenuInstaller.IsInstalledCorrectly();

		Console.WriteLine("Installation Status:");
		Console.WriteLine("────────────────────");

		if (isInstalled && isCorrect) {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("✓ Context menu: Installed (current)");
			Console.ResetColor();
		} else if (isInstalled) {
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("⚠ Context menu: Installed (outdated path)");
			Console.ResetColor();
			Console.WriteLine("  The executable has moved. Run --install to update.");
		} else {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("✗ Context menu: Not installed");
			Console.ResetColor();
			Console.WriteLine("  Run --install or double-click the exe to install.");
		}

		Console.WriteLine();
		Console.WriteLine($"Executable: {Environment.ProcessPath ?? "Unknown"}");
		Console.WriteLine($"Admin:      {(IsRunningAsAdmin() ? "Yes" : "No")}");
		Console.WriteLine($"Version:    {FileHasher.Version}");
		Console.WriteLine($"Algorithms: {FileHasher.AlgorithmCount}");

		var command = ContextMenuInstaller.GetInstalledCommand();
		if (command != null) {
			Console.WriteLine($"Registered: {command}");
		}

		return 0;
	}

	#endregion

	#region Command-Line Parsing Helpers

	/// <summary>
	/// Checks if an argument is a help switch.
	/// </summary>
	/// <param name="arg">The argument to check.</param>
	/// <returns><see langword="true"/> if it's a help switch.</returns>
	private static bool IsHelpSwitch(string arg) =>
		arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
		arg.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
		arg.Equals("/?", StringComparison.OrdinalIgnoreCase) ||
		arg.Equals("/help", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Checks if an argument is a version switch.
	/// </summary>
	/// <param name="arg">The argument to check.</param>
	/// <returns><see langword="true"/> if it's a version switch.</returns>
	private static bool IsVersionSwitch(string arg) =>
		arg.Equals("--version", StringComparison.OrdinalIgnoreCase) ||
		arg.Equals("-v", StringComparison.OrdinalIgnoreCase) ||
		arg.Equals("/version", StringComparison.OrdinalIgnoreCase);

	#endregion

	#region Usage Display

	/// <summary>
	/// Displays the full usage information and help text.
	/// </summary>
	private static void ShowUsage() {
		Console.WriteLine($@"HashNow v{FileHasher.Version} - Instant File Hashing ({FileHasher.AlgorithmCount} algorithms)

Usage:
  HashNow                              Auto-install: check and prompt to install
  HashNow <file> [file2] [file3] ...   Hash one or more files
  HashNow --install                    Install Explorer context menu (requires admin)
  HashNow --uninstall                  Remove Explorer context menu (requires admin)
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
  Just double-click HashNow.exe and follow the prompts!
  Administrator privileges are required for context menu registration.

Examples:
  HashNow                              Install via interactive prompt
  HashNow myfile.zip                   Hash a single file
  HashNow *.iso                        Hash multiple files (shell expansion)
  HashNow ""C:\My Files\doc.pdf""        Hash file with spaces in path
  HashNow --install                    Add right-click menu option
");
	}

	#endregion
}
