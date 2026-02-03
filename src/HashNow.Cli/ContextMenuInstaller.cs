using System.Runtime.Versioning;
using Microsoft.Win32;
using System.Diagnostics;

namespace HashNow.Cli;

/// <summary>
/// Handles Windows Explorer context menu registration via the registry.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class ContextMenuInstaller {
private const string RegistryKeyPath = @"*\shell\HashNow";
private const string MenuText = "Hash this file now";

/// <summary>
/// Installs the HashNow context menu entry for all file types.
/// Requires administrator privileges.
/// </summary>
public static void Install() {
var exePath = GetExecutablePath();

// Create the context menu entry under HKEY_CLASSES_ROOT\*\shell\HashNow
using var key = Registry.ClassesRoot.CreateSubKey(RegistryKeyPath);
if (key == null) {
throw new InvalidOperationException("Failed to create registry key");
}

// Set the menu item text
key.SetValue("", MenuText);

// Set icon (use the exe itself as icon source)
key.SetValue("Icon", $"\"{exePath}\",0");

// Create the command subkey
using var commandKey = key.CreateSubKey("command");
if (commandKey == null) {
throw new InvalidOperationException("Failed to create command registry key");
}

// Set the command to run when clicked
commandKey.SetValue("", $"\"{exePath}\" \"%1\"");
}

/// <summary>
/// Removes the HashNow context menu entry.
/// Requires administrator privileges.
/// </summary>
public static void Uninstall() {
try {
Registry.ClassesRoot.DeleteSubKeyTree(RegistryKeyPath, throwOnMissingSubKey: false);
} catch (ArgumentException) {
// Key doesn't exist, that's fine
}
}

/// <summary>
/// Checks if the context menu is currently installed.
/// </summary>
public static bool IsInstalled() {
using var key = Registry.ClassesRoot.OpenSubKey(RegistryKeyPath);
return key != null;
}

private static string GetExecutablePath() {
// Get the path to the currently running executable
var exePath = Environment.ProcessPath;
if (string.IsNullOrEmpty(exePath)) {
exePath = Process.GetCurrentProcess().MainModule?.FileName;
}
if (string.IsNullOrEmpty(exePath)) {
exePath = AppContext.BaseDirectory + "HashNow.exe";
}
return exePath;
}
}
