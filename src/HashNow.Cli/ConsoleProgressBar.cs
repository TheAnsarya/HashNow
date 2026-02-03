using System.Runtime.Versioning;

namespace HashNow.Cli;

/// <summary>
/// A console-based progress bar for displaying hash computation progress in CLI mode.
/// </summary>
/// <remarks>
/// <para>
/// This class provides a text-based progress bar that renders in the console.
/// It uses carriage returns to update the display in place.
/// </para>
/// <para>
/// Example output:
/// <code>
/// [██████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░] 45%
/// </code>
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed class ConsoleProgressBar : IDisposable {
	#region Constants

	/// <summary>
	/// Width of the progress bar in characters.
	/// </summary>
	private const int BarWidth = 50;

	/// <summary>
	/// Character used for completed portion.
	/// </summary>
	private const char FilledChar = '█';

	/// <summary>
	/// Character used for remaining portion.
	/// </summary>
	private const char EmptyChar = '░';

	#endregion

	#region Fields

	private readonly bool _useColor;
	private int _lastPercent = -1;
	private bool _disposed;

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="ConsoleProgressBar"/> class.
	/// </summary>
	/// <param name="useColor">Whether to use console colors.</param>
	public ConsoleProgressBar(bool useColor = true) {
		_useColor = useColor;
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Updates the progress bar display.
	/// </summary>
	/// <param name="progress">Progress value between 0.0 and 1.0.</param>
	public void Update(double progress) {
		int percent = (int)(progress * 100);
		percent = Math.Min(100, Math.Max(0, percent));

		// Only update if percentage changed (reduce flicker)
		if (percent == _lastPercent) {
			return;
		}
		_lastPercent = percent;

		int filled = (int)(progress * BarWidth);
		int empty = BarWidth - filled;

		// Build the bar
		var bar = new string(FilledChar, filled) + new string(EmptyChar, empty);

		// Write to console
		Console.Write("\r");

		if (_useColor) {
			Console.Write("[");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write(bar[..filled]);
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write(bar[filled..]);
			Console.ResetColor();
			Console.Write($"] {percent,3}%  ");
		} else {
			Console.Write($"[{bar}] {percent,3}%  ");
		}
	}

	/// <summary>
	/// Marks the progress bar as complete and moves to the next line.
	/// </summary>
	public void Complete() {
		Update(1.0);
		Console.WriteLine();
	}

	/// <summary>
	/// Clears the progress bar line.
	/// </summary>
	public void Clear() {
		Console.Write("\r" + new string(' ', BarWidth + 10) + "\r");
	}

	#endregion

	#region IDisposable

	/// <inheritdoc/>
	public void Dispose() {
		if (!_disposed) {
			Complete();
			_disposed = true;
		}
	}

	#endregion

	#region Static Helper

	/// <summary>
	/// Creates a progress callback action for use with hash operations.
	/// </summary>
	/// <param name="useColor">Whether to use console colors.</param>
	/// <returns>A tuple containing the progress bar and a callback action.</returns>
	public static (ConsoleProgressBar Bar, Action<double> Callback) Create(bool useColor = true) {
		var bar = new ConsoleProgressBar(useColor);
		return (bar, progress => bar.Update(progress));
	}

	#endregion
}
