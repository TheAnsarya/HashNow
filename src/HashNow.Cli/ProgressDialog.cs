using System.Runtime.Versioning;

namespace HashNow.Cli;

/// <summary>
/// A modal progress dialog for displaying hash computation progress.
/// </summary>
/// <remarks>
/// <para>
/// This dialog is shown when hashing large files from Windows Explorer context menu.
/// It displays:
/// </para>
/// <list type="bullet">
///   <item><description>File name being hashed</description></item>
///   <item><description>Progress bar with percentage</description></item>
///   <item><description>Cancel button to abort the operation</description></item>
/// </list>
/// <para>
/// The dialog runs on a separate UI thread to keep the progress bar responsive
/// while the hashing operation runs in the background.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed class ProgressDialog : Form {
	#region Fields

	private readonly Label _fileLabel;
	private readonly ProgressBar _progressBar;
	private readonly Label _percentLabel;
	private readonly Button _cancelButton;
	private readonly CancellationTokenSource _cts;

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="ProgressDialog"/> class.
	/// </summary>
	/// <param name="fileName">The name of the file being hashed.</param>
	/// <param name="cts">Cancellation token source for cancel functionality.</param>
	public ProgressDialog(string fileName, CancellationTokenSource cts) {
		_cts = cts;

		// Form properties
		Text = "HashNow - Computing Hashes...";
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterScreen;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = true;
		Size = new Size(450, 180);
		Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

		// File label
		_fileLabel = new Label {
			Text = $"Hashing: {fileName}",
			Location = new Point(20, 20),
			Size = new Size(400, 25),
			AutoEllipsis = true,
			Font = new Font(Font.FontFamily, 10f, FontStyle.Bold)
		};

		// Progress bar
		_progressBar = new ProgressBar {
			Location = new Point(20, 55),
			Size = new Size(400, 30),
			Minimum = 0,
			Maximum = 100,
			Value = 0,
			Style = ProgressBarStyle.Continuous
		};

		// Percentage label
		_percentLabel = new Label {
			Text = "0%",
			Location = new Point(20, 90),
			Size = new Size(100, 20),
			Font = new Font(Font.FontFamily, 9f)
		};

		// Cancel button
		_cancelButton = new Button {
			Text = "Cancel",
			Location = new Point(335, 95),
			Size = new Size(85, 30)
		};
		_cancelButton.Click += (_, _) => {
			_cts.Cancel();
			_cancelButton.Enabled = false;
			_cancelButton.Text = "Cancelling...";
		};

		// Add controls
		Controls.Add(_fileLabel);
		Controls.Add(_progressBar);
		Controls.Add(_percentLabel);
		Controls.Add(_cancelButton);

		// Handle form closing
		FormClosing += (_, e) => {
			if (!_cts.IsCancellationRequested) {
				_cts.Cancel();
			}
		};
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Updates the progress display.
	/// </summary>
	/// <param name="progress">Progress value between 0.0 and 1.0.</param>
	public void UpdateProgress(double progress) {
		if (InvokeRequired) {
			BeginInvoke(() => UpdateProgress(progress));
			return;
		}

		int percent = (int)(progress * 100);
		_progressBar.Value = Math.Min(100, Math.Max(0, percent));
		_percentLabel.Text = $"{percent}%";
	}

	/// <summary>
	/// Shows that the operation completed.
	/// </summary>
	public void Complete() {
		if (InvokeRequired) {
			BeginInvoke(Complete);
			return;
		}

		_progressBar.Value = 100;
		_percentLabel.Text = "100% - Complete!";
		_cancelButton.Text = "Done";
		_cancelButton.Enabled = true;
		_cancelButton.Click -= null!; // Clear cancel handler
		_cancelButton.Click += (_, _) => Close();
	}

	#endregion

	#region Static Factory Method

	/// <summary>
	/// Shows a progress dialog and executes the hashing operation.
	/// </summary>
	/// <param name="filePath">Path to the file to hash.</param>
	/// <param name="hashAction">Action that performs hashing with progress callback.</param>
	/// <returns>A task that completes when the dialog closes.</returns>
	public static async Task<bool> ShowDialogAsync(
		string filePath,
		Func<Action<double>, CancellationToken, Task> hashAction) {
		using var cts = new CancellationTokenSource();
		var fileName = Path.GetFileName(filePath);
		var dialog = new ProgressDialog(fileName, cts);
		var cancelled = false;

		// Start hashing in background
		var hashTask = Task.Run(async () => {
			try {
				await hashAction(
					progress => dialog.UpdateProgress(progress),
					cts.Token);
			} catch (OperationCanceledException) {
				cancelled = true;
			}
		});

		// Show dialog (blocking until closed)
		var dialogTask = Task.Run(() => {
			Application.Run(dialog);
		});

		// Wait for hash to complete
		await hashTask;

		// Close dialog if still open
		if (!cancelled) {
			dialog.BeginInvoke(() => {
				dialog.Complete();
				// Auto-close after brief display
				Task.Delay(500).ContinueWith(_ => dialog.BeginInvoke(dialog.Close));
			});
		} else {
			dialog.BeginInvoke(dialog.Close);
		}

		await dialogTask;

		return !cancelled;
	}

	#endregion
}
