using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
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
using System.Xaml;

using Grammophone.EnnounInference;
using Grammophone.EnnounInference.Configuration;
using Grammophone.Windows;
using Grammophone.LanguageModel.TrainingSources;

namespace Grammophone.EnnounInference.Trainer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Command definitions

		public static RoutedUICommand TrainCommand =
			new RoutedUICommand("Train All", "Train Everything", typeof(MainWindow));

		public static RoutedUICommand TrainTaggedWordsCommand =
			new RoutedUICommand("Train Tagged Word Forms", "Train words classifier bank", typeof(MainWindow));

		public static RoutedUICommand TrainSentencesCommand =
			new RoutedUICommand("Train Tagged Sentences", "Train sentence classifier", typeof(MainWindow));

		public static RoutedUICommand LoadInferenceResourceCommand =
			new RoutedUICommand("Load Whole Inference Resource", "Load whole inference resource", typeof(MainWindow));

		public static RoutedUICommand LoadTaggedWordsTrainingCommand =
			new RoutedUICommand("Load Tagged Word Forms Training", "Load words classifier bank", typeof(MainWindow));

		public static RoutedUICommand LoadSentencesTrainingCommand =
			new RoutedUICommand("Load Sentence Training", "Load sentence classifier", typeof(MainWindow));

		public static RoutedUICommand SaveInferenceResourceCommand =
			new RoutedUICommand("Save Whole Inference Resource", "Save whole inference resource", typeof(MainWindow));

		public static RoutedUICommand SaveTaggedWordsTrainingCommand =
			new RoutedUICommand("Save Tagged Word Forms Training", "Save words classifier bank", typeof(MainWindow));

		public static RoutedUICommand SaveSentencesTrainingCommand =
			new RoutedUICommand("Save Sentence Training", "Save sentence classifier", typeof(MainWindow));

		public static RoutedUICommand ValidateConfiguredSentencesSetCommand =
			new RoutedUICommand("Validate Configured Sentences Set", "Validate sentences using the sets defined in configuration", typeof(MainWindow));

		public static RoutedUICommand ValidateSentencesSetCommand =
			new RoutedUICommand("Validate a Sentences Set...", "Supply a user-defined set for sentences validation", typeof(MainWindow));

		public static RoutedUICommand ValidateConfiguredWordsSetCommand =
			new RoutedUICommand("Validate Configured Words Set", "Validate Word classifiers against the sets defined in configuration", typeof(MainWindow));

		public static RoutedUICommand ValidateWordsSetCommand =
			new RoutedUICommand("Validate Words Set...", "Validate Word classifiers against the sets defined in configuration", typeof(MainWindow));

		#endregion

		#region Private fields

		private TrainingOptionsDefinition trainingOptionsDefinition;

		private string optionsFilename;

		private bool isRunning;

		#endregion

		#region Construction

		public MainWindow()
		{
			try
			{
				NewTrainingOptions();

				InitializeComponent();

				isRunning = false;

				Trace.Listeners.Add(new TextBoxTraceListener(traceTextBox));
				//Trace.Listeners.Add(new ConsoleTraceListener());

				languagesComboBox.ItemsSource = InferenceEnvironment.Setup.TrainingSets;
				languagesComboBox.SelectedValue = null;

				CommandManager.InvalidateRequerySuggested();

			}
			catch (Exception ex)
			{
				ShowError(ex);
				Close();
			}
		}

		#endregion

		#region Protected methods

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = isRunning;
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			SetupGlass();
		}

		#endregion

		#region Private methods

		private void NewTrainingOptions()
		{
			this.trainingOptionsDefinition = new TrainingOptionsDefinition();
			this.DataContext = this.trainingOptionsDefinition;
			this.optionsFilename = String.Empty;

			UpdateWindowTitle();
		}

		private void OpenTrainingOptions()
		{
			try
			{
				var dialog = new Microsoft.Win32.OpenFileDialog();

				SetupTrainingOptionsFileDialog(dialog, this.optionsFilename);

				if (dialog.ShowDialog() == true)
				{
					OpenTrainingOptions(dialog.FileName);

					this.optionsFilename = dialog.FileName;

					UpdateWindowTitle();
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private void OpenTrainingOptions(string filename)
		{
			if (filename == null) throw new ArgumentNullException("filename");

			try
			{
				var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

				using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var deserialized = formatter.Deserialize(stream) as TrainingOptionsDefinition;

					if (trainingOptionsDefinition == null)
					{
						throw new ApplicationException("The contents of the file are not a Training Options Definition.");
					}

					this.trainingOptionsDefinition = deserialized;
					this.DataContext = deserialized;

					Trace.WriteLine(String.Format("Opened file \"{0}\".", filename));
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}

		}

		private void SaveTrainingOptions(string filename)
		{
			if (filename == null) throw new ArgumentNullException("filename");

			try
			{
				var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

				using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					formatter.Serialize(stream, trainingOptionsDefinition);
				}

				Trace.WriteLine(String.Format("Saved training options file \"{0}\".", filename));
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private void SaveTrainingOptionsAs()
		{
			try
			{
				var dialog = new Microsoft.Win32.SaveFileDialog();

				SetupTrainingOptionsFileDialog(dialog, optionsFilename);

				if (dialog.ShowDialog() == true)
				{
					SaveTrainingOptions(dialog.FileName);

					optionsFilename = dialog.FileName;

					UpdateWindowTitle();
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private void SaveInferenceResource()
		{
			if (isRunning) return;

			var inferenceResource = GetInferenceResource();

			if (inferenceResource == null) return;

			try
			{
				var dialog = new Microsoft.Win32.SaveFileDialog();

				SetupInferenceResourceFileDialog(dialog);

				if (dialog.ShowDialog() == true)
				{
					isRunning = true;

					var task = StartTimedTask(() =>
						{
							Trace.WriteLine(String.Format("Saving inference resource file \"{0}\".", dialog.FileName));

							inferenceResource.Save(dialog.FileName);

							Trace.WriteLine(String.Format("Saved inference resource file \"{0}\".", dialog.FileName));
						});
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private void SaveTaggedWordsTraining()
		{
			if (isRunning) return;

			var inferenceResource = GetInferenceResource();

			if (inferenceResource == null) return;

			try
			{
				var dialog = new Microsoft.Win32.SaveFileDialog();

				SetupTaggedWordsTrainingFileDialog(dialog);

				if (dialog.ShowDialog() == true)
				{
					isRunning = true;

					var task = StartTimedTask(() =>
						{
							Trace.WriteLine(String.Format("Saving tagged words training file \"{0}\".", dialog.FileName));

							inferenceResource.SaveWordClassifierBank(dialog.FileName);

							Trace.WriteLine(String.Format("Saved tagged words training file \"{0}\".", dialog.FileName));
						});
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private void SaveSentencesTraining()
		{
			if (isRunning) return;

			var inferenceResource = GetInferenceResource();

			if (inferenceResource == null) return;

			try
			{
				var dialog = new Microsoft.Win32.SaveFileDialog();

				SetupSentencesTrainingFileDialog(dialog);

				if (dialog.ShowDialog() == true)
				{
					isRunning = true;

					var task = StartTimedTask(() =>
						{
							Trace.WriteLine(String.Format("Saving sentences training file \"{0}\".", dialog.FileName));

							inferenceResource.SaveSentenceClassifier(dialog.FileName);

							Trace.WriteLine(String.Format("Saved sentences training file \"{0}\".", dialog.FileName));
						});
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private void LoadInferenceResource()
		{
			if (isRunning) return;

			try
			{
				var dialog = new Microsoft.Win32.OpenFileDialog();

				SetupInferenceResourceFileDialog(dialog);

				if (dialog.ShowDialog() == true)
				{
					isRunning = true;

					var task = StartTimedTask(() =>
						{
							Trace.WriteLine(String.Format("Loading inference resource file '{0}'.", dialog.FileName));

							var languageProvider = GetLanguageProvider();

							if (languageProvider == null)
							{
								throw new ApplicationException("No language is specified.");
							}

							InferenceEnvironment.LoadInferenceResource(languageProvider, dialog.FileName);

							Trace.WriteLine(String.Format("Loaded inference resource file '{0}'.", dialog.FileName));
						});
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private void LoadTaggedWordsTraining()
		{
			if (isRunning) return;

			try
			{
				var dialog = new Microsoft.Win32.OpenFileDialog();

				SetupTaggedWordsTrainingFileDialog(dialog);

				if (dialog.ShowDialog() == true)
				{
					isRunning = true;

					var task = StartTimedTask(() =>
						{
							Trace.WriteLine(String.Format("Loading tagged words training file '{0}'.", dialog.FileName));

							var inferenceResource = GetOrCreateInferenceResource();

							inferenceResource.LoadWordClassifierBank(dialog.FileName);

							Trace.WriteLine(String.Format("Loaded tagged words training file '{0}'.", dialog.FileName));
						});
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private void LoadSentencesTraining()
		{
			if (isRunning) return;

			try
			{
				var dialog = new Microsoft.Win32.OpenFileDialog();

				SetupSentencesTrainingFileDialog(dialog);

				if (dialog.ShowDialog() == true)
				{
					isRunning = true;

					var task = StartTimedTask(() =>
						{
							Trace.WriteLine(String.Format("Loading sentences training file '{0}'.", dialog.FileName));

							var inferenceResource = GetOrCreateInferenceResource();

							inferenceResource.LoadSentenceClassifier(dialog.FileName);

							Trace.WriteLine(String.Format("Loaded sentences training file '{0}'.", dialog.FileName));
						});
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private void Train()
		{
			if (isRunning) return;

			isRunning = true;
			CommandManager.InvalidateRequerySuggested();

			var task = StartTimedTask(() =>
				{
					EnnounInference.InferenceResource inferenceResource = GetOrCreateInferenceResource();

					if (trainingOptionsDefinition.UseCrossValidation)
					{
						var grid = trainingOptionsDefinition.GetOptionsGrid();

						inferenceResource.OptimalTrain(
							grid, 
							trainingOptionsDefinition.FoldCount, 
							trainingOptionsDefinition.WordDropout, 
							trainingOptionsDefinition.WordDecimation,
							trainingOptionsDefinition.TagBiGramsDropout,
							trainingOptionsDefinition.SentencesStride,
							trainingOptionsDefinition.DegreeOfParallelism);
					}
					else
					{
						var options = trainingOptionsDefinition.GetOptions();

						inferenceResource.Train(
							options, 
							trainingOptionsDefinition.WordDropout, 
							trainingOptionsDefinition.WordDecimation,
							trainingOptionsDefinition.TagBiGramsDropout,
							trainingOptionsDefinition.SentencesStride,
							trainingOptionsDefinition.DegreeOfParallelism);
					}
				});
		}

		private void TrainTaggedWords()
		{
			if (isRunning) return;

			isRunning = true;
			CommandManager.InvalidateRequerySuggested();

			var task = StartTimedTask(() =>
			{
				EnnounInference.InferenceResource inferenceResource = GetOrCreateInferenceResource();

				if (trainingOptionsDefinition.UseCrossValidation)
				{
					var grid = trainingOptionsDefinition.GetOptionsGrid();

					inferenceResource.OptimalTrainWordClassifierBank(
						grid.WordClassifierTrainingOptionsGrid, 
						trainingOptionsDefinition.FoldCount, 
						trainingOptionsDefinition.WordDropout, 
						trainingOptionsDefinition.WordDecimation,
						trainingOptionsDefinition.DegreeOfParallelism);
				}
				else
				{
					var options = trainingOptionsDefinition.GetOptions();

					inferenceResource.TrainWordClassifierBank(
						options.WordClassifierTrainingOptions, 
						trainingOptionsDefinition.WordDropout, 
						trainingOptionsDefinition.WordDecimation,
						trainingOptionsDefinition.DegreeOfParallelism);
				}
			});
		}

		private void TrainSentences()
		{
			if (isRunning) return;

			isRunning = true;
			CommandManager.InvalidateRequerySuggested();

			var task = StartTimedTask(() =>
			{
				EnnounInference.InferenceResource inferenceResource = GetOrCreateInferenceResource();

				if (inferenceResource.WordClassifierBank == null)
				{
					throw new ApplicationException("Word training is missing.");
				}

				var options = trainingOptionsDefinition.GetOptions();

				inferenceResource.TrainSentences(
					options.SentenceClassifierTrainingOptions,
					trainingOptionsDefinition.TagBiGramsDropout,
					trainingOptionsDefinition.SentencesStride,
					trainingOptionsDefinition.DegreeOfParallelism);
			});
		}

		private void AppendTaskTerminationHandlers(Task task)
		{
			task.ContinueWith(t =>
				{
					TraceException(t.Exception);

					var aggregateException = t.Exception as AggregateException;

					this.Dispatcher.Invoke((Action)delegate
					{
						if (aggregateException != null)
						{
							aggregateException.Flatten();

							if (aggregateException.InnerException != null)
							{
								ShowError(aggregateException.InnerException);

								return;
							}

						}

						ShowError(t.Exception);
					});
				},
				TaskContinuationOptions.OnlyOnFaulted
			);

			task.ContinueWith(t =>
				{
					isRunning = false;

					this.Dispatcher.Invoke((Action)delegate
					{
						CommandManager.InvalidateRequerySuggested();
					});
				},
				TaskContinuationOptions.ExecuteSynchronously
			);
		}

		private EnnounInference.InferenceResource GetOrCreateInferenceResource()
		{
			var languageProvider = GetLanguageProvider();

			if (languageProvider == null)
			{
				throw new ApplicationException(
					String.Format("No language selected.", languageProvider));
			}

			EnnounInference.InferenceResource inferenceResource;

			if (!InferenceEnvironment.InferenceResources.ContainsKey(languageProvider))
			{
				inferenceResource = new InferenceResource(languageProvider);
				InferenceEnvironment.InferenceResources.Add(inferenceResource);
			}
			else
			{
				inferenceResource = InferenceEnvironment.InferenceResources[languageProvider];
			}

			return inferenceResource;
		}

		private LanguageModel.Provision.LanguageProvider GetLanguageProvider()
		{
			var languageProviderKey = trainingOptionsDefinition.LanguageProviderKey;

			if (languageProviderKey == null) return null;

			if (!InferenceEnvironment.Setup.LanguageProviders.ContainsKey(languageProviderKey)) return null;

			var languageProvider = InferenceEnvironment.Setup.LanguageProviders[languageProviderKey];
			
			return languageProvider;
		}

		private EnnounInference.InferenceResource GetInferenceResource()
		{
			var languageProvider = GetLanguageProvider();

			if (languageProvider == null)
			{
				return null;
			}

			if (!InferenceEnvironment.InferenceResources.ContainsKey(languageProvider))
			{
				return null;
			}
			else
			{
				return InferenceEnvironment.InferenceResources[languageProvider];
			}
		}

		private void UpdateWindowTitle()
		{
			if (this.optionsFilename.Length > 0)
				this.Title = "Training Options Definition - " + optionsFilename;
			else
				this.Title = "Training Options Definition";
		}

		private static void SetupTrainingOptionsFileDialog(Microsoft.Win32.FileDialog dialog, string filename)
		{
			SetupFileDialog(dialog, filename, "*.trainoptions", "Training Options (*.trainoptions)|*.trainoptions");
		}

		private static void SetupInferenceResourceFileDialog(Microsoft.Win32.FileDialog dialog)
		{
			SetupFileDialog(dialog, String.Empty, "*.inference", "Inference Resource (*.inference)|*.inference");
		}

		private static void SetupTaggedWordsTrainingFileDialog(Microsoft.Win32.FileDialog dialog)
		{
			SetupFileDialog(dialog, String.Empty, "*.twtraining", "Tagged Words Training (*.twtraining)|*.twtraining");
		}

		private static void SetupUntaggedWordsTrainingFileDialog(Microsoft.Win32.FileDialog dialog)
		{
			SetupFileDialog(dialog, String.Empty, "*.uwtraining", "Untagged Words Training (*.uwtraining)|*.uwtraining");
		}

		private static void SetupSentencesTrainingFileDialog(Microsoft.Win32.FileDialog dialog)
		{
			SetupFileDialog(dialog, String.Empty, "*.straining", "Sentences Training (*.straining)|*.straining");
		}

		private static void SetupXamlFileDialog(Microsoft.Win32.FileDialog dialog)
		{
			SetupFileDialog(dialog, String.Empty, "*.xaml", "XAML file (*.xaml)|*.xaml");
		}

		private static void SetupFileDialog(Microsoft.Win32.FileDialog dialog, string filename, string defaultExt, string filter)
		{
			if (dialog == null) throw new ArgumentNullException("dialog");
			if (filename == null) throw new ArgumentNullException("filename");
			if (defaultExt == null) throw new ArgumentNullException("defaultExt");
			if (filter == null) throw new ArgumentNullException("filter");

			dialog.DefaultExt = defaultExt;
			dialog.Filter = filter;

			if (filename.Length > 0) dialog.FileName = filename;
		}

		private void ShowError(Exception ex)
		{
			MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private void TraceException(Exception exception)
		{
			if (exception == null) throw new ArgumentNullException("exception");

			var aggregateException = exception as AggregateException;

			if (aggregateException != null)
			{
				aggregateException.Flatten();

				foreach (var innerException in aggregateException.InnerExceptions)
				{
					TraceException(innerException);
				}
			}

			if (exception.Source != null && exception.Source.Length > 0)
				Trace.WriteLine(String.Format("Exception: {0} (Source: {1})", exception.Message, exception.Source));
			else
				Trace.WriteLine(String.Format("Exception: {0}", exception.Message));
		}

		private void SetupGlass()
		{
			GlassHelper.RegisterGlassHandling(this, GlassHelper.Margins.Full);
		}

		private Task<T> StartTimedTask<T>(Func<T> action)
		{
			var stopwatch = new Stopwatch();

			var mainTask = Task.Factory.StartNew(action);

			AppendTaskTerminationHandlers(mainTask);

			mainTask.ContinueWith(t =>
			{
				stopwatch.Stop();
				Trace.WriteLine(String.Format("Elapsed time: {0} ({1} seconds).", stopwatch.Elapsed, stopwatch.Elapsed.TotalSeconds));
			},
				TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);

			return mainTask;
		}

		private Task StartTimedTask(Action action)
		{
			var stopwatch = new Stopwatch();

			stopwatch.Start();

			var mainTask = Task.Factory.StartNew(action);

			AppendTaskTerminationHandlers(mainTask);

			mainTask.ContinueWith(t =>
			{
				stopwatch.Stop();
				Trace.WriteLine(String.Format("Elapsed time: {0} ({1} seconds).", stopwatch.Elapsed, stopwatch.Elapsed.TotalSeconds));
			},
				TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);

			return mainTask;
		}

		private bool IsWordsClassifierBankReady()
		{
			if (isRunning) return false;

			var languageProvider = GetLanguageProvider();

			if (languageProvider == null) return false;

			if (!InferenceEnvironment.InferenceResources.ContainsKey(languageProvider)) return false;

			var inferenceResource = InferenceEnvironment.InferenceResources[languageProvider];

			return
				inferenceResource.WordClassifierBank != null;
		}

		private bool IsInferenceResourceReady()
		{
			if (isRunning) return false;

			var languageProvider = GetLanguageProvider();

			if (languageProvider == null) return false;

			if (!InferenceEnvironment.InferenceResources.ContainsKey(languageProvider)) return false;

			var inferenceResource = InferenceEnvironment.InferenceResources[languageProvider];

			return 
				inferenceResource.WordClassifierBank != null && 
				inferenceResource.SentenceClassifier != null;
		}

		private void ValidateConfiguredSentencesSet()
		{
			if (!IsInferenceResourceReady()) return;

			var task = StartTimedTask(delegate()
			{
				isRunning = true;

				var languageProvider = GetLanguageProvider();

				var inferenceResource = InferenceEnvironment.InferenceResources[languageProvider];

				var sentenceClassifier = inferenceResource.SentenceClassifier;

				if (sentenceClassifier == null) return;

				var validationResult = sentenceClassifier.Validate(trainingOptionsDefinition.DegreeOfParallelism);

				Trace.WriteLine(validationResult.ToString());
			});
		}

		private void ValidateSentencesSet()
		{
			if (!IsInferenceResourceReady()) return;

			try
			{
				var dialog = new Microsoft.Win32.OpenFileDialog();

				SetupXamlFileDialog(dialog);

				if (dialog.ShowDialog() == true)
				{
					var languageProvider = GetLanguageProvider();

					var validationSet = SourceSet.LoadFromXAML<ValidationSet>(dialog.FileName, languageProvider);

					var task = StartTimedTask(delegate()
					{
						isRunning = true;

						var inferenceResource = InferenceEnvironment.InferenceResources[languageProvider];

						var sentenceClassifier = inferenceResource.SentenceClassifier;

						if (sentenceClassifier == null) return;

						var validationResult = sentenceClassifier.Validate(validationSet, trainingOptionsDefinition.DegreeOfParallelism);

						Trace.WriteLine(validationResult.ToString());
					});
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}

		}

		private void ValidateConfiguredWordsSet()
		{
			if (isRunning || !IsWordsClassifierBankReady()) return;

			StartTimedTask(() =>
			{
				isRunning = true;

				var inferenceResource = GetInferenceResource();

				if (inferenceResource == null || inferenceResource.WordClassifierBank == null) return;

				double score = inferenceResource.WordClassifierBank.ValidateClassifiers(this.trainingOptionsDefinition.DegreeOfParallelism);

				Trace.WriteLine(String.Format("Average score over training data: {0:F4}", score));
			});
		}

		private void ValidateWordsSet()
		{
			if (isRunning || !IsWordsClassifierBankReady()) return;

			try
			{
				var dialog = new Microsoft.Win32.OpenFileDialog();

				SetupXamlFileDialog(dialog);

				if (dialog.ShowDialog() == true)
				{
					var languageProvider = GetLanguageProvider();

					var validationSet = SourceSet.LoadFromXAML<ValidationSet>(dialog.FileName, languageProvider);

					var task = StartTimedTask(delegate()
					{
						isRunning = true;

						var inferenceResource = InferenceEnvironment.InferenceResources[languageProvider];

						if (inferenceResource == null || inferenceResource.WordClassifierBank == null) return;

						var wordClassifierBank = inferenceResource.WordClassifierBank;

						if (wordClassifierBank == null) return;

						double score = wordClassifierBank.ValidateClassifiers(validationSet, trainingOptionsDefinition.DegreeOfParallelism);

						Trace.WriteLine(String.Format("Average score over training data: {0:F4}", score));
					});
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}

		}

		#endregion

		#region Event handlers

		private void ExecuteNew(object sender, ExecutedRoutedEventArgs e)
		{
			NewTrainingOptions();
		}

		private void ExecuteOpen(object sender, ExecutedRoutedEventArgs e)
		{
			OpenTrainingOptions();
		}

		private void ExecuteSave(object sender, ExecutedRoutedEventArgs e)
		{
			if (this.optionsFilename.Length == 0)
			{
				SaveTrainingOptionsAs();
			}
			else
			{
				SaveTrainingOptions(this.optionsFilename);
			}
		}

		private void ExecuteSaveAs(object sender, ExecutedRoutedEventArgs e)
		{
			SaveTrainingOptionsAs();
		}

		private void ExecuteClose(object sender, ExecutedRoutedEventArgs e)
		{
			this.Close();
		}

		private void ExecuteTrain(object sender, ExecutedRoutedEventArgs e)
		{
			Train();
		}

		private void CanExecuteOpenCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			if (isRunning)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;
		}

		private void CanExecuteSaveCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			if (!formGrid.AreAllValidated())
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;
		}

		private void CanExecuteCloseCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			if (isRunning)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;

		}

		private void CanExecuteNewCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			if (isRunning)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;
		}

		private void CanExecuteTrain(object sender, CanExecuteRoutedEventArgs e)
		{
			if (isRunning || !formGrid.AreAllValidated())
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;
		}

		private void CanExecuteTrainTaggedWords(object sender, CanExecuteRoutedEventArgs e)
		{
			if (isRunning || !formGrid.AreAllValidated())
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;
		}

		private void ExecuteTrainTaggedWords(object sender, ExecutedRoutedEventArgs e)
		{
			TrainTaggedWords();
		}

		private void CanExecuteTrainTaggedSentences(object sender, CanExecuteRoutedEventArgs e)
		{
			if (isRunning || !formGrid.AreAllValidated())
			{
				e.CanExecute = false;
				return;
			}

			var languageProvider = GetLanguageProvider();

			if (languageProvider == null || !InferenceEnvironment.InferenceResources.ContainsKey(languageProvider))
			{
				e.CanExecute = false;
				return;
			}

			var inferenceResource = InferenceEnvironment.InferenceResources[languageProvider];

			if (inferenceResource.WordClassifierBank == null)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;
		}

		private void ExecuteTrainSentences(object sender, ExecutedRoutedEventArgs e)
		{
			TrainSentences();
		}

		private void CanExecuteSaveInferenceResource(object sender, CanExecuteRoutedEventArgs e)
		{
			if (isRunning)
			{
				e.CanExecute = false;
				return;
			}

			var inferenceResource = GetInferenceResource();

			if (inferenceResource == null)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;
		}

		private void ExecuteSaveInferenceResource(object sender, ExecutedRoutedEventArgs e)
		{
			SaveInferenceResource();
		}

		private void CanExecuteSaveTaggedWordsTraining(object sender, CanExecuteRoutedEventArgs e)
		{
			if (isRunning)
			{
				e.CanExecute = false;
				return;
			}

			var inferenceResource = GetInferenceResource();

			if (inferenceResource == null)
			{
				e.CanExecute = false;
				return;
			}

			if (inferenceResource.WordClassifierBank == null)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;
		}

		private void ExecuteSaveTaggedWordsTraining(object sender, ExecutedRoutedEventArgs e)
		{
			SaveTaggedWordsTraining();
		}

		private void CanExecuteSaveSentencesTraining(object sender, CanExecuteRoutedEventArgs e)
		{
			if (isRunning)
			{
				e.CanExecute = false;
				return;
			}

			var inferenceResource = GetInferenceResource();

			if (inferenceResource == null)
			{
				e.CanExecute = false;
				return;
			}

			if (inferenceResource.SentenceClassifier == null)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;
		}

		private void ExecuteSaveSentencesTraining(object sender, ExecutedRoutedEventArgs e)
		{
			SaveSentencesTraining();
		}

		private void CanExecuteLoadInferenceResource(object sender, CanExecuteRoutedEventArgs e)
		{
			var languageProvider = GetLanguageProvider();

			if (isRunning || languageProvider == null)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;
		}

		private void ExecuteLoadInferenceResource(object sender, ExecutedRoutedEventArgs e)
		{
			LoadInferenceResource();
		}

		private void CanExecuteLoadTaggedWordsTraining(object sender, CanExecuteRoutedEventArgs e)
		{
			var languageProvider = GetLanguageProvider();

			if (isRunning || languageProvider == null)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;
		}

		private void ExecuteLoadTaggedWordsTraining(object sender, ExecutedRoutedEventArgs e)
		{
			LoadTaggedWordsTraining();
		}

		private void CanExecuteLoadSentencesTraining(object sender, CanExecuteRoutedEventArgs e)
		{
			var languageProvider = GetLanguageProvider();

			if (isRunning || languageProvider == null)
			{
				e.CanExecute = false;
				return;
			}

			var inferenceResource = GetInferenceResource();

			if (inferenceResource == null)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = (inferenceResource.WordClassifierBank != null);
		}

		private void ExecuteLoadSentencesTraining(object sender, ExecutedRoutedEventArgs e)
		{
			LoadSentencesTraining();
		}

		private void CanExecuteValidateConfiguredSentencesSet(object sender, CanExecuteRoutedEventArgs e)
		{
			if (isRunning)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = IsInferenceResourceReady();
		}

		private void ExecuteValidateConfiguredSentencesSet(object sender, ExecutedRoutedEventArgs e)
		{
			ValidateConfiguredSentencesSet();
		}

		private void CanExecuteValidateSentencesSet(object sender, CanExecuteRoutedEventArgs e)
		{
			if (isRunning)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = IsInferenceResourceReady();
		}

		private void ExecuteValidateSentencesSet(object sender, ExecutedRoutedEventArgs e)
		{
			ValidateSentencesSet();
		}

		private void ExecuteValidateConfiguredWordsSet(object sender, ExecutedRoutedEventArgs e)
		{
			ValidateConfiguredWordsSet();
		}

		private void ExecuteValidateWordsSet(object sender, ExecutedRoutedEventArgs e)
		{
			ValidateWordsSet();
		}

		private void CanExecuteValidateConfiguredWordsSet(object sender, CanExecuteRoutedEventArgs e)
		{
			if (isRunning)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = IsWordsClassifierBankReady();
		}

		private void CanExecuteValidateWordsSet(object sender, CanExecuteRoutedEventArgs e)
		{
			if (isRunning)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = IsWordsClassifierBankReady();
		}

		#endregion

	}
}
