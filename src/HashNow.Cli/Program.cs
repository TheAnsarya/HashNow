using HashNow.Core;

namespace HashNow.Cli;

/// <summary>
/// HashNow CLI - Compute file hashes and save to JSON.
/// </summary>
internal static class Program {
private static async Task<int> Main(string[] args) {
if (args.Length == 0) {
ShowUsage();
return 1;
}

// Handle special commands
if (args[0].Equals("--help", StringComparison.OrdinalIgnoreCase) ||
args[0].Equals("-h", StringComparison.OrdinalIgnoreCase) ||
args[0].Equals("/?", StringComparison.OrdinalIgnoreCase)) {
ShowUsage();
return 0;
}

if (args[0].Equals("--version", StringComparison.OrdinalIgnoreCase) ||
args[0].Equals("-v", StringComparison.OrdinalIgnoreCase)) {
Console.WriteLine("HashNow v1.0.0");
return 0;
}

if (args[0].Equals("--install", StringComparison.OrdinalIgnoreCase)) {
return InstallContextMenu();
}

if (args[0].Equals("--uninstall", StringComparison.OrdinalIgnoreCase)) {
return UninstallContextMenu();
}

// Process file(s)
var exitCode = 0;
foreach (var filePath in args) {
var result = await ProcessFileAsync(filePath);
if (result != 0) exitCode = result;
}

return exitCode;
}

private static async Task<int> ProcessFileAsync(string filePath) {
try {
if (!File.Exists(filePath)) {
Console.Error.WriteLine($"Error: File not found: {filePath}");
return 1;
}

var fileInfo = new FileInfo(filePath);
Console.WriteLine($"Hashing: {fileInfo.Name} ({FormatSize(fileInfo.Length)})");

var result = await FileHasher.HashFileAsync(
filePath,
progress => {
Console.Write($"\rProgress: {progress:P0}  ");
});

Console.WriteLine("\rProgress: 100%    ");

var outputPath = filePath + ".hashes.json";
await FileHasher.SaveResultAsync(result, outputPath);

Console.WriteLine($"CRC32:  {result.Crc32}");
Console.WriteLine($"MD5:    {result.Md5}");
Console.WriteLine($"SHA1:   {result.Sha1}");
Console.WriteLine($"SHA256: {result.Sha256}");
Console.WriteLine($"SHA512: {result.Sha512}");
Console.WriteLine($"Saved:  {outputPath}");
Console.WriteLine();

return 0;
} catch (Exception ex) {
Console.Error.WriteLine($"Error processing {filePath}: {ex.Message}");
return 1;
}
}

private static int InstallContextMenu() {
try {
ContextMenuInstaller.Install();
Console.WriteLine("Context menu installed successfully!");
Console.WriteLine("Right-click any file in Explorer and select 'Hash this file now'");
return 0;
} catch (UnauthorizedAccessException) {
Console.Error.WriteLine("Error: Administrator privileges required.");
Console.Error.WriteLine("Please run as Administrator to install the context menu.");
return 1;
} catch (Exception ex) {
Console.Error.WriteLine($"Error installing context menu: {ex.Message}");
return 1;
}
}

private static int UninstallContextMenu() {
try {
ContextMenuInstaller.Uninstall();
Console.WriteLine("Context menu removed successfully!");
return 0;
} catch (UnauthorizedAccessException) {
Console.Error.WriteLine("Error: Administrator privileges required.");
Console.Error.WriteLine("Please run as Administrator to uninstall the context menu.");
return 1;
} catch (Exception ex) {
Console.Error.WriteLine($"Error uninstalling context menu: {ex.Message}");
return 1;
}
}

private static void ShowUsage() {
Console.WriteLine(@"HashNow v1.0.0 - Instant File Hashing

Usage:
  HashNow <file> [file2] [file3] ...   Hash one or more files
  HashNow --install                    Install Explorer context menu (requires admin)
  HashNow --uninstall                  Remove Explorer context menu (requires admin)
  HashNow --help                       Show this help
  HashNow --version                    Show version

Output:
  Creates {filename}.hashes.json containing:
  - CRC32, MD5, SHA1, SHA256, SHA512 hashes
  - File size, timestamps, and metadata

Examples:
  HashNow myfile.zip                   Hash a single file
  HashNow *.iso                        Hash multiple files
  HashNow --install                    Add right-click menu option
");
}

private static string FormatSize(long bytes) {
string[] sizes = ["B", "KB", "MB", "GB", "TB"];
int order = 0;
double size = bytes;
while (size >= 1024 && order < sizes.Length - 1) {
order++;
size /= 1024;
}
return $"{size:0.##} {sizes[order]}";
}
}
