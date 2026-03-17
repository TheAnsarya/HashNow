namespace HashNow.Cli.Platform;

/// <summary>
/// Factory for creating platform-specific integration implementations.
/// </summary>
/// <remarks>
/// <para>
/// Uses runtime OS detection to return the appropriate platform implementation:
/// </para>
/// <list type="bullet">
///   <item><description>Windows: Registry-based Explorer context menu + WinForms dialogs</description></item>
///   <item><description>Linux: File manager scripts/actions (Nautilus, Nemo, Dolphin, Thunar)</description></item>
///   <item><description>macOS: Finder Quick Actions (Automator workflows)</description></item>
/// </list>
/// </remarks>
internal static class PlatformFactory {
	/// <summary>
	/// Creates the appropriate platform integration for the current operating system.
	/// </summary>
	/// <returns>An <see cref="IPlatformIntegration"/> for the current platform.</returns>
	/// <exception cref="PlatformNotSupportedException">
	/// Thrown when the current operating system is not supported.
	/// </exception>
	public static IPlatformIntegration Create() {
#if WINDOWS
		if (OperatingSystem.IsWindows()) {
			return new Windows.WindowsPlatform();
		}
#else
		if (OperatingSystem.IsLinux()) {
			return new Linux.LinuxPlatform();
		}

		if (OperatingSystem.IsMacOS()) {
			return new MacOS.MacOSPlatform();
		}
#endif

		throw new PlatformNotSupportedException(
			"HashNow is only supported on Windows, Linux, and macOS.");
	}
}
