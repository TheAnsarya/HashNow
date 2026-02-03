using System.Diagnostics;
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
/// The progress bar uses a delayed display strategy: it only appears if the operation
/// takes more than 2 seconds AND progress is less than 60%. This avoids visual clutter
/// for fast operations while providing feedback for slower ones.
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

	/// <summary>
	/// Delay in milliseconds before showing the progress bar.
	/// </summary>
	private const long DisplayDelayMs = 2000;

	/// <summary>
	/// Maximum progress (0.0-1.0) at which the progress bar will still be shown.
	/// If progress is >= this value when delay expires, the bar is not shown.
	/// </summary>
	private const double MaxProgressForDisplay = 0.60;

	#endregion

	#region Fields

	private readonly bool _useColor;
	private readonly Stopwatch _stopwatch;
	private int _lastPercent = -1;
	private bool _disposed;
	private bool _isVisible;
	private bool _displayDecisionMade;

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="ConsoleProgressBar"/> class.
	/// </summary>
	/// <param name="useColor">Whether to use console colors.</param>
	public ConsoleProgressBar(bool useColor = true) {
		_useColor = useColor;
		_stopwatch = Stopwatch.StartNew();
		_isVisible = false;
		_displayDecisionMade = false;
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Updates the progress bar display.
	/// </summary>
	/// <param name="progress">Progress value between 0.0 and 1.0.</param>
	/// <remarks>
	/// The progress bar only becomes visible if:
	/// <list type="bullet">
	///   <item><description>At least 2 seconds have elapsed since creation</description></item>
	///   <item><description>Progress is still less than 60%</description></item>
	/// </list>
	/// Once visible, the bar stays visible until completion. If the operation completes
	/// quickly (under 2 seconds or reaches 60%+ before 2 seconds), no bar is shown.
	/// </remarks>
	public void Update(double progress) {
		int percent = (int)(progress * 100);
		percent = Math.Min(100, Math.Max(0, percent));

		// Check if we should start displaying the progress bar
		if (!_displayDecisionMade) {
			var elapsed = _stopwatch.ElapsedMilliseconds;

			// If 2 seconds have passed and we're under 60%, show the bar
			if (elapsed >= DisplayDelayMs) {
				_displayDecisionMade = true;
				_isVisible = progress < MaxProgressForDisplay;
			}
			// If progress is already >= 60% before 2 seconds, never show
			else if (progress >= MaxProgressForDisplay) {
				_displayDecisionMade = true;
				_isVisible = false;
			}
		}

		// Don't render if not visible
		if (!_isVisible) {
			return;
		}

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
	/// <remarks>
	/// Only outputs a newline if the progress bar was actually displayed.
	/// </remarks>
	public void Complete() {
		if (_isVisible) {
			Update(1.0);
			Console.WriteLine();
		}
	}

	/// <summary>
	/// Clears the progress bar line (only if visible).
	/// </summary>
	public void Clear() {
		if (_isVisible) {
			Console.Write("\r" + new string(' ', BarWidth + 10) + "\r");
		}
	}

	#endregion

	#region IDisposable

	/// <inheritdoc/>
	public void Dispose() {
		if (!_disposed) {
			_stopwatch.Stop();
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
