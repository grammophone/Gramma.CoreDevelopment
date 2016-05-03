using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Grammophone.EnnounInference;
using Grammophone.EnnounInference.Sentences;
using Grammophone.EnnounInference.Words;
using Grammophone.LanguageModel.Provision;
using System.Threading.Tasks;
using Grammophone.GenericContentModel;

namespace Grammophone.EnnounInference.Evaluator
{
	public class ParagraphProvider
	{
		#region Auxilliary classes

		/// <summary>
		/// Used to pair words to their lemma inference.
		/// </summary>
		private class WordInference
		{
			/// <summary>
			/// Create.
			/// </summary>
			/// <param name="word">The word being examined.</param>
			/// <param name="lemmaInference">The inference for the word or null if no inference was possible.</param>
			public WordInference(string word, LemmaInference lemmaInference)
			{
				if (word == null) throw new ArgumentNullException("word");

				this.Word = word;
				this.LemmaInference = lemmaInference;
			}

			/// <summary>
			/// The word being examined.
			/// </summary>
			public string Word { get; private set; }

			/// <summary>
			/// The inference for the word or null if no inference was possible.
			/// </summary>
			public LemmaInference LemmaInference { get; private set; }
		}

		#endregion

		#region Private fields

		private Dictionary<Run, LemmaInference> runsToInferencesDictionary;

		private Paragraph paragraph;

		private string text;

		private IEnumerable<ITextProcessorStage> textProcessorStages;

		private InferenceResource inferenceResource;

		private Window parentWindow;

		#endregion

		#region Construction

		public ParagraphProvider(Window parentWindow, string text, InferenceResource inferenceResource, IEnumerable<ITextProcessorStage> textProcessorStages)
		{
			if (parentWindow == null) throw new ArgumentNullException("parentWindow");
			if (text == null) throw new ArgumentNullException("text");
			if (inferenceResource == null) throw new ArgumentNullException("inferenceResource");
			if (textProcessorStages == null) throw new ArgumentNullException("textProcessorStages");

			if (inferenceResource.SentenceClassifier == null)
				throw new ArgumentException("The inference resource is not trained or properly loaded.", "inferenceResource");

			this.parentWindow = parentWindow;
			this.text = text;
			this.inferenceResource = inferenceResource;
			this.textProcessorStages = textProcessorStages;
		}

		public ParagraphProvider(Window parentWindow, string text, InferenceResource inferenceResource)
			: this(
			parentWindow, 
			text, 
			inferenceResource, 
			new ITextProcessorStage[] { new CharacterNormalizationStage(), new HyphenationTextProcessorStage() })
		{

		}

		#endregion

		#region Public properties

		public Paragraph Paragraph
		{
			get
			{
				return paragraph;
			}
		}

		public IEnumerable<KeyValuePair<Run, LemmaInference>> RunEntries
		{
			get
			{
				return runsToInferencesDictionary;
			}
		}

		public LemmaInference this[Run run]
		{
			get
			{
				if (runsToInferencesDictionary == null) return null;

				LemmaInference inference;

				if (runsToInferencesDictionary.TryGetValue(run, out inference))
				{
					return inference;
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region Public methods

		public Task<Paragraph> GetParagraphAsync()
		{
			// If we have the paragraph ready, return a trivial task.
			if (paragraph != null)
			{
				return Task.FromResult(paragraph);
			}

			ProgressWindow progressWindow = new ProgressWindow();

			progressWindow.Owner = parentWindow;

			progressWindow.Show();

			// The other task infers the sentences and creates the paragraph.
			var processTask = Task.Factory.StartNew(() =>
			{
				var runsToInferencesDictionary = new Dictionary<Run, LemmaInference>();

				foreach (var stage in this.textProcessorStages)
				{
					this.text = stage.Process(this.text);
				}

				var languageProvider = this.inferenceResource.LanguageProvider;

				var sentenceBreaker = languageProvider.SentenceBreaker;

				var sentenceBuilder = new StringBuilder();

				var sentences = new List<string>();

				bool previousWasTerminator = false;

				for (int i = 0; i < this.text.Length; i++)
				{
					char c = this.text[i];

					if (sentenceBreaker.IsSentenceDelimiter(c))
					{
						previousWasTerminator = true;
					}
					else
					{
						if (previousWasTerminator)
						{
							sentences.Add(sentenceBuilder.ToString());

							sentenceBuilder.Clear();

							previousWasTerminator = false;
						}
					}

					sentenceBuilder.Append(c);
				}

				if (sentenceBuilder.Length > 0)
				{
					var lastSentence = sentenceBuilder.ToString().Trim();

					if (lastSentence.Length > 0)
					{
						sentences.Add(lastSentence);
					}
				}

				var trimmedSentences = (from sentence in sentences
															 let trimmedSentence = sentence.Trim(' ', '\r', '\n')
															 where trimmedSentence.Length > 0
															 select trimmedSentence).ToArray();

				// Run inference.
				var inferredSentences = new WordInference[trimmedSentences.Length][];

				progressWindow.Minimum = 0;
				progressWindow.Maximum = trimmedSentences.Length;

				for (int i = 0; i < trimmedSentences.Length; i++)
				{
					progressWindow.Value = i + 1;

					var sentence = trimmedSentences[i];

					var words = sentenceBreaker.Break(sentence);

					var lemmata = inferenceResource.SentenceClassifier.InferLemmata(words);

					var inferredSentence = new WordInference[words.Length];
					inferredSentences[i] = inferredSentence;

					for (int j = 0; j < words.Length; j++)
					{
						var word = words[j];
						var lemma = (lemmata != null ? lemmata[j] : null);

						inferredSentence[j] = new WordInference(word, lemma);
					}
				}

				// Map the inference results to a paragraph attached to the UI thread.

				Paragraph newParagraph = null;

				parentWindow.Dispatcher.Invoke((Action)delegate
				{
					newParagraph = new Paragraph();

					for (int i = 0; i < inferredSentences.Length; i++)
					{
						var inferredSentence = inferredSentences[i];

						for (int j = 0; j < inferredSentence.Length; j++)
						{
							var wordInference = inferredSentence[j];

							var word = wordInference.Word;

							var lemmaInference = wordInference.LemmaInference;

							var run = new Run(word + " ");

							if (lemmaInference != null)
								runsToInferencesDictionary[run] = lemmaInference;
							else
								run.Foreground = Brushes.DarkRed;

							newParagraph.Inlines.Add(run);
						}
					}
				});

				progressWindow.SafeClose();

				this.paragraph = newParagraph;

				this.runsToInferencesDictionary = runsToInferencesDictionary;

				return newParagraph;
			},
			TaskCreationOptions.LongRunning);

			AppendExceptionHandler(processTask, progressWindow);

			return processTask;
		}

		#endregion

		#region Private methods

		private Task AppendExceptionHandler(Task task, ProgressWindow progressWindow)
		{
			if (task == null) throw new ArgumentNullException("task");

			return task.ContinueWith(failedTask =>
				{
					MessageBox.Show(TranslateException(failedTask.Exception));
				},
				TaskContinuationOptions.OnlyOnFaulted);

		}

		private string TranslateException(AggregateException aggregateException)
		{
			aggregateException.Flatten();

			if (aggregateException.InnerExceptions.Count == 1) return TranslateException(aggregateException.InnerExceptions[0]);

			var stringBuilder = new StringBuilder();

			stringBuilder.Append(TranslateException((Exception)aggregateException));

			foreach (var exception in aggregateException.InnerExceptions)
			{
				stringBuilder.Append(TranslateException(exception));
			}

			return stringBuilder.ToString();
		}

		private string TranslateException(Exception exception)
		{
			if (exception == null) return String.Empty;

			return String.Format("Exception type: {0}, Message: \"{1}\", Source: {2}.\r", exception.GetType().Name, exception.Message, exception.Source);
		}

		#endregion
	}
}
