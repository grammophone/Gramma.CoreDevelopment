using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Grammophone.EnnounInference;
using Grammophone.Windows;

namespace Grammophone.EnnounInference.Evaluator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Command definitions

		public static RoutedUICommand ChooseInferenceResourceCommand =
			new RoutedUICommand("Choose Inference Resource...", "Choose inference resource", typeof(MainWindow));

		public static RoutedUICommand ImportCommand =
			new RoutedUICommand("Import Text...", "Import text", typeof(MainWindow));

		#endregion

		#region Private fields

		private InferenceResourceLoader inferenceResourceLoader;

		private InferenceResource inferenceResource;

		private ParagraphProvider paragraphProvider;

		private InferenceInformationWindow inferenceInformationWindow;

		private bool isProcessing;

		#endregion

		#region Construction

		public MainWindow()
		{
			InitializeComponent();

			inferenceResourceLoader = new InferenceResourceLoader(this);
		}

		#endregion

		#region Protected methods

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			// TODO: Enable this line when finished.
			//if (!OpenInferenceResource()) this.Close();

			GlassHelper.RegisterGlassHandling(this, GlassHelper.Margins.Full);
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!e.Cancel) e.Cancel = isProcessing;
		}

		#endregion

		#region Private methods

		private bool OpenInferenceResource()
		{
			try
			{
				var providerPickerWindow = new InferenceResourceProviderPickerWindow();

				providerPickerWindow.Owner = this;

				if (providerPickerWindow.ShowDialog() == true)
				{
					var provider = providerPickerWindow.SelectedInferenceResourceProvider;

					if (provider != null)
					{
						inferenceResource = inferenceResourceLoader.Load(provider);

						return inferenceResource != null;
					}
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}

			return false;
		}

		private bool EnsureOpenInferenceResource()
		{
			if (inferenceResource == null)
			{
				return OpenInferenceResource();
			}
			else
			{
				return true;
			}
		}

		private bool Open()
		{
			if (!EnsureOpenInferenceResource()) return false;

			try
			{
				var dialog = new Microsoft.Win32.OpenFileDialog();

				dialog.DefaultExt = "*.txt";
				dialog.Filter = "Text file (*.txt)|*.txt";

				if (dialog.ShowDialog(this) == false) return false;

				using (var reader = new System.IO.StreamReader(dialog.FileName))
				{
					string text = reader.ReadToEnd();

					ImportText(text);

					return true;
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}

			return false;
		}

		private void ImportText(string text)
		{
			if (text == null) throw new ArgumentNullException("text");

			if (inferenceResource == null) throw new ApplicationException("No inference resource has been selected.");
			if (inferenceResource.SentenceClassifier == null) throw new ApplicationException("The inference resource has not been loaded properly.");

			var paragraphProvider = new ParagraphProvider(this, text, inferenceResource);

			isProcessing = true;

			var inferredParagraphTask = paragraphProvider.GetParagraphAsync();

			var paragraphAssignmentTask = inferredParagraphTask.ContinueWith(task =>
				{

					this.Dispatcher.BeginInvoke((Action)delegate
					{
						var document = new FlowDocument(task.Result);

						document.FontFamily = documentReader.FontFamily;

						if (this.paragraphProvider != null)
						{
							DetachRunHandlers();
							this.paragraphProvider = null;
						}

						this.paragraphProvider = paragraphProvider;

						documentReader.Document = document;

						AttachRunHandlers();
					});
				});

			paragraphAssignmentTask.ContinueWith(task =>
				{
					isProcessing = false;
				},
				TaskContinuationOptions.ExecuteSynchronously);
		}

		private bool ImportText()
		{
			if (!EnsureOpenInferenceResource()) return false;

			try
			{
				TextImportWindow importWindow = new TextImportWindow();

				importWindow.Owner = this;

				if (importWindow.ShowDialog() == true)
				{
					ImportText(importWindow.Text);

					return true;
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}

			return false;
		}

		private void DetachRunHandlers()
		{
			if (paragraphProvider == null) return;

			foreach (var runEntry in paragraphProvider.RunEntries)
			{
				runEntry.Key.MouseEnter -= new MouseEventHandler(OnRunMouseEnter);
				runEntry.Key.MouseLeave -= new MouseEventHandler(OnRunMouseLeave);
			}

			if (inferenceInformationWindow != null)
			{
				inferenceInformationWindow.Close();
				inferenceInformationWindow = null;
			}
		}

		private void AttachRunHandlers()
		{
			if (paragraphProvider == null) return;

			foreach (var runEntry in paragraphProvider.RunEntries)
			{
				runEntry.Key.MouseEnter += new MouseEventHandler(OnRunMouseEnter);
				runEntry.Key.MouseLeave += new MouseEventHandler(OnRunMouseLeave);
			}
		}

		private void OnRunMouseEnter(object sender, MouseEventArgs e)
		{
			if (isProcessing) return;

			var run = sender as Run;

			if (run == null) return;

			var inference = paragraphProvider[run];

			if (inference == null) return;

			if (inferenceInformationWindow == null)
			{
				inferenceInformationWindow = new InferenceInformationWindow();
				inferenceInformationWindow.Owner = this;

				inferenceInformationWindow.Closed += OnInferenceInformationWindowClosed;
			}

			inferenceInformationWindow.DataContext = inference;

			Rect runRectangle = run.ElementStart.GetCharacterRect(LogicalDirection.Forward);

			Point bottomPosition = new Point(runRectangle.Left, runRectangle.Bottom);

			bottomPosition = documentReader.PointToScreen(bottomPosition);

			inferenceInformationWindow.Top = bottomPosition.Y + 2;
			inferenceInformationWindow.Left = bottomPosition.X;

			inferenceInformationWindow.Show();

			run.Background = Brushes.DarkSalmon;
		}

		private void OnInferenceInformationWindowClosed(object sender, EventArgs eventArgs)
		{
			inferenceInformationWindow.Closed -= this.OnInferenceInformationWindowClosed;
			inferenceInformationWindow = null;
		}

		private void OnRunMouseLeave(object sender, MouseEventArgs e)
		{
			var run = sender as Run;

			if (run != null)
			{
				// Queue this to the UI message pump, because, if executed directly,
				// It has the strange effect of hiding the "inference information window"!
				this.Dispatcher.BeginInvoke((Action)delegate
					{
						if (run.Background != Brushes.Transparent) run.Background = Brushes.Transparent;
					});
			}

			if (inferenceInformationWindow != null)
			{
				inferenceInformationWindow.Hide();
			}
		}

		private void ShowError(Exception ex)
		{
			MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		#region Event handling

		private void ExecuteClose(object sender, ExecutedRoutedEventArgs e)
		{
			this.Close();
		}

		private void ExecuteOpen(object sender, ExecutedRoutedEventArgs e)
		{
			Open();
		}

		private void ExecuteImport(object sender, ExecutedRoutedEventArgs e)
		{
			ImportText();
		}

		private void ExecuteChooseInferenceResource(object sender, ExecutedRoutedEventArgs e)
		{
			OpenInferenceResource();
		}

		private void CanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = !isProcessing;
		}

		#endregion

		#endregion

	}
}
