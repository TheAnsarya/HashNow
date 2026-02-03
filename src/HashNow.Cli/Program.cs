using System.Diagnostics;
using System.Runtime.Versioning;
using HashNow.Core;

namespace HashNow.Cli;

/// <summary>
/// HashNow CLI - Compute file hashes and save to JSON.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class Program {
// Threshold in milliseconds before showing progress UI
private const long ProgressUiThresholdMs = 3000;

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
Console.WriteLine($"HashNow v{FileHasher.Version}");
return 0;
}

if (args[0].Equals("--install", StringComparison.OrdinalIgnoreCase)) {
return InstallContextMenu();
}

if (args[0].Equals("--uninstall", StringComparison.OrdinalIgnoreCase)) {
return UninstallContextMenu();
}

// Check if running from Explorer (no console window or detached)
bool isFromExplorer = Console.IsInputRedirected || Environment.GetEnvironmentVariable("HASHNOW_EXPLORER") == "1";

// Process file(s)
var exitCode = 0;
foreach (var filePath in args) {
var result = await ProcessFileAsync(filePath, isFromExplorer);
if (result != 0) exitCode = result;
}

return exitCode;
}

private static async Task<int> ProcessFileAsync(string filePath, bool isFromExplorer) {
try {
if (!File.Exists(filePath)) {
if (!isFromExplorer) {
Console.Error.WriteLine($"Error: File not found: {filePath}");
}
return 1;
}

var fileInfo = new FileInfo(filePath);
var estimatedMs = FileHasher.EstimateHashDurationMs(fileInfo.Length);

if (!isFromExplorer) {
// Console mode - show inline progress
Console.WriteLine($"Hashing: {fileInfo.Name} ({FileHasher.FormatFileSize(fileInfo.Length)})");

var result = await FileHasher.HashFileAsync(
filePath,
progress => {
Console.Write($"\rProgress: {progress:P0}  ");
});

Console.WriteLine("\rProgress: 100%    ");

var outputPath = filePath + ".hashes.json";
await FileHasher.SaveResultAsync(result, outputPath);

PrintResults(result);
Console.WriteLine($"Saved:  {outputPath}");
Console.WriteLine($"Time:   {result.DurationMs}ms");
Console.WriteLine();
} else {
// Explorer mode - background with optional progress dialog
if (estimatedMs > ProgressUiThresholdMs) {
// Long operation - show progress dialog
await ProcessWithProgressDialogAsync(filePath, fileInfo);
} else {
// Short operation - silent background
var result = await FileHasher.HashFileAsync(filePath);
var outputPath = filePath + ".hashes.json";
await FileHasher.SaveResultAsync(result, outputPath);
}
}

return 0;
} catch (Exception ex) {
if (!isFromExplorer) {
Console.Error.WriteLine($"Error processing {filePath}: {ex.Message}");
}
return 1;
}
}

private static async Task ProcessWithProgressDialogAsync(string filePath, FileInfo fileInfo) {
// Use a simple approach: spawn a background task and poll progress
// For now, just process with a completion notification
var cts = new CancellationTokenSource();
double currentProgress = 0;

var hashTask = FileHasher.HashFileAsync(
filePath,
p => currentProgress = p,
cts.Token);

// Start a timer to show progress if taking too long
var sw = Stopwatch.StartNew();
bool dialogShown = false;

while (!hashTask.IsCompleted) {
await Task.Delay(100);

if (sw.ElapsedMilliseconds > ProgressUiThresholdMs && !dialogShown) {
dialogShown = true;
// Could show a WinForms/WPF progress dialog here
// For now, just continue processing silently
}
}

var result = await hashTask;
var outputPath = filePath + ".hashes.json";
await FileHasher.SaveResultAsync(result, outputPath);
}

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
Console.WriteLine($@"HashNow v{FileHasher.Version} - Instant File Hashing (58 algorithms)

Usage:
  HashNow <file> [file2] [file3] ...   Hash one or more files
  HashNow --install                    Install Explorer context menu (requires admin)
  HashNow --uninstall                  Remove Explorer context menu (requires admin)
  HashNow --help                       Show this help
  HashNow --version                    Show version

Algorithms (58 total, computed in single pass):
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

Examples:
  HashNow myfile.zip                   Hash a single file
  HashNow *.iso                        Hash multiple files
  HashNow --install                    Add right-click menu option
");
}
}
