using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grammophone.EnnounInference;
using Grammophone.LanguageModel.Provision;

namespace Grammophone.EnnounInference.Trainer
{
	[Serializable]
	public class TrainingOptionsDefinition
	{
		#region Private fields

		private bool useCrossValidation = false;

		private double marginSlackStart = 1.0, marginSlackEnd = 1.0, marginSlackStepFactor = 10.0;

		private double gaussianVarianceStart = 1.0, gaussianVarianceEnd = 1.0, gaussianVarianceStepFactor = 10.0;

		private double stringExponentStart = 1.0, stringExponentEnd = 1.0, stringExponentStepFactor = 10.0;

		private int foldCount = 3;

		private string languageProviderKey;

		private double wordDropout = 1e-5;

		private int wordDecimation;

		private int degreeOfParallelism;

		private int sentencesStride;

		private double tagBiGramsDropout;

		private double crfWeightsRegularization;

		private SentencesTrainType sentencesTrainType;

		private int maxSentencesSamples;

		private double crfStepSizeCoefficient;

		private bool shuffleSentenceSamples;

		private bool condenseFeatures;

		private Grammophone.EnnounInference.Sentences.WordScoringPolicy wordScoringPolicy;

		private DecayFunctionType decayFunctionType;

		#endregion

		#region Construction

		public TrainingOptionsDefinition()
		{
			this.IncludeGaussian = true;
			this.GaussianVarianceStart = 1.0;
			this.GaussianVarianceEnd = 1.0;
			this.GaussianVarianceStepFactor = 10.0;

			this.WordDropout = 1e-5;

			this.WordDecimation = 50;

			this.DegreeOfParallelism = 0;

			this.SentencesStride = 1;
			this.TagBiGramsDropout = 1E-6;
			this.CrfWeightsRegularization = 1.0;

			this.SentencesTrainType = EnnounInference.Trainer.SentencesTrainType.Offline;

			this.maxSentencesSamples = 40000;

			this.crfStepSizeCoefficient = 40000.0;

			this.shuffleSentenceSamples = true;

			this.condenseFeatures = false;

			this.wordScoringPolicy = EnnounInference.Sentences.WordScoringPolicy.Prioritized;

			this.decayFunctionType = DecayFunctionType.Linear;
		}

		#endregion

		#region Public properties

		public bool UseCrossValidation
		{
			get
			{
				return useCrossValidation;
			}
			set
			{
				useCrossValidation = value;
			}
		}

		public double MarginSlackStart
		{
			get
			{
				return marginSlackStart;
			}
			set
			{
				if (value < 0.0) throw new ArgumentException("The slack must not be negative.");
				marginSlackStart = value;
			}
		}

		public double MarginSlackEnd
		{
			get
			{
				return marginSlackEnd;
			}
			set
			{
				if (value < 0.0) throw new ArgumentException("The slack must not be negative.");
				marginSlackEnd = value;
			}
		}

		public double MarginSlackStepFactor
		{
			get
			{
				return marginSlackStepFactor;
			}
			set
			{
				if (value <= 0.0) throw new ArgumentException("The factor must be positive.");
				marginSlackStepFactor = value;
			}
		}

		public bool IncludeGaussian { get; set; }

		public double GaussianVarianceStart
		{
			get
			{
				return gaussianVarianceStart;
			}
			set
			{
				if (value <= 0.0) throw new ArgumentException("The gaussian variance must be positive.");
				gaussianVarianceStart = value;
			}
		}

		public double GaussianVarianceEnd
		{
			get
			{
				return gaussianVarianceEnd;
			}
			set
			{
				if (value <= 0.0) throw new ArgumentException("The gaussian variance must be positive.");
				gaussianVarianceEnd = value;
			}
		}

		public double GaussianVarianceStepFactor
		{
			get
			{
				return gaussianVarianceStepFactor;
			}
			set
			{
				if (value <= 0.0) throw new ArgumentException("The factor must be positive.");
				gaussianVarianceStepFactor = value;
			}
		}

		public double StringExponentStart
		{
			get
			{
				return stringExponentStart;
			}
			set
			{
				if (value <= 0.0) throw new ArgumentException("The string exponent must be positive.");
				stringExponentStart = value;
			}
		}

		public double StringExponentEnd
		{
			get
			{
				return stringExponentEnd;
			}
			set
			{
				if (value <= 0.0) throw new ArgumentException("The string exponent must be positive.");
				stringExponentEnd = value;
			}
		}

		public double StringExponentStepFactor
		{
			get
			{
				return stringExponentStepFactor;
			}
			set
			{
				if (value <= 0.0) throw new ArgumentException("The factor must be positive.");
				stringExponentStepFactor = value;
			}
		}

		public int FoldCount
		{
			get
			{
				return foldCount;
			}
			set
			{
				if (value <= 0) throw new ArgumentException("Fold count must be positive.");
				foldCount = value;
			}
		}

		public string LanguageProviderKey
		{
			get
			{
				return languageProviderKey;
			}
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				languageProviderKey = value;
			}
		}

		public double WordDropout
		{
			get
			{
				return wordDropout;
			}
			set
			{
				if (value < 0.0 || value > 1.0) throw new ArgumentException("The value must be between 0 and 1.");

				wordDropout = value;
			}
		}

		public int WordDecimation
		{
			get
			{
				return wordDecimation;
			}
			set
			{
				if (value < 1) throw new ArgumentException("Decimation must be at least 1.");

				wordDecimation = value;
			}
		}

		public int DegreeOfParallelism
		{
			get
			{
				return degreeOfParallelism;
			}
			set
			{
				if (value < 0) throw new ArgumentException("The value must not be negative.");

				degreeOfParallelism = value;
			}
		}

		public int SentencesStride
		{
			get
			{
				return sentencesStride;
			}
			set
			{
				if (value < 1) throw new ArgumentException("The value stride must be positive.");
				sentencesStride = value;
			}
		}

		public double TagBiGramsDropout
		{
			get
			{
				return tagBiGramsDropout;
			}
			set
			{
				if (value < 0.0) throw new ArgumentException("The value must not be negative.");

				tagBiGramsDropout = value;
			}
		}

		public double CrfWeightsRegularization
		{
			get
			{
				return crfWeightsRegularization;
			}
			set
			{
				if (value < 0.0) throw new ArgumentException("The value must not be negative.");

				crfWeightsRegularization = value;
			}
		}

		public SentencesTrainType SentencesTrainType 
		{
			get
			{
				return sentencesTrainType;
			}
			set
			{
				sentencesTrainType = value;
			}
		}

		public int MaxSentencesSamples
		{
			get
			{
				return maxSentencesSamples;
			}
			set
			{
				maxSentencesSamples = value;
			}
		}

		public double CrfStepSizeCoefficient
		{
			get
			{
				return crfStepSizeCoefficient;
			}
			set
			{
				if (value <= 0.0) throw new ArgumentException("value must be positive.");
				crfStepSizeCoefficient = value;
			}
		}

		public bool ShuffleSentenceSamples
		{
			get { return shuffleSentenceSamples; }
			set { shuffleSentenceSamples = value; }
		}

		public bool CondenseSingularFeatures
		{
			get { return condenseFeatures; }
			set { condenseFeatures = value; }
		}

		public Grammophone.EnnounInference.Sentences.WordScoringPolicy WordScoringPolicy
		{
			get { return wordScoringPolicy; }
			set { wordScoringPolicy = value; }
		}

		public DecayFunctionType DecayFunctionType
		{
			get { return decayFunctionType; }
			set { decayFunctionType = value; }
		}

		#endregion

		#region Public methods

		public InferenceResource.TrainingOptionsGrid GetOptionsGrid()
		{
			var grid = new InferenceResource.TrainingOptionsGrid();

			grid.WordClassifierTrainingOptionsGrid = GetWordTrainingOptionsGrid().Take(1000).ToArray();

			grid.SentenceClassifierTrainingOptionsGrid = GetSentencesTrainingOptionsGrid().Take(100).ToArray();

			return grid;
		}

		public InferenceResource.TrainingOptions GetOptions()
		{
			var options = new InferenceResource.TrainingOptions();

			options.WordClassifierTrainingOptions = GetWordTrainingOptions();

			options.SentenceClassifierTrainingOptions = GetSentenceTrainingOptions();

			return options;
		}

		#endregion

		#region Private methods

		private EnnounInference.Words.WordClassifierTrainingOptions GetWordTrainingOptions()
		{
			var trainingOptions = new EnnounInference.Words.WordClassifierTrainingOptions();

			trainingOptions.ClassificationMarginSlack = this.MarginSlackStart;
			trainingOptions.GaussianDeviation = this.GaussianVarianceStart;
			trainingOptions.IsGaussified = this.IncludeGaussian;
			trainingOptions.StringKernelExponent = this.StringExponentStart;

			return trainingOptions;
		}

		private EnnounInference.Sentences.SentenceClassifierTrainingOptions GetSentenceTrainingOptions()
		{
			var trainingOptions = new EnnounInference.Sentences.SentenceClassifierTrainingOptions();

			trainingOptions.AnalogiesScoreOptions = null;
			
			trainingOptions.CondenseFeatures = condenseFeatures;

			trainingOptions.WordScoringPolicy = this.wordScoringPolicy;

			switch (this.SentencesTrainType)
			{
				case EnnounInference.Trainer.SentencesTrainType.Offline:
					trainingOptions.TrainingMethod = EnnounInference.Sentences.SentenceClassifierTrainingMethod.Offline;
					trainingOptions.OfflineTrainingOptions.Regularization = this.CrfWeightsRegularization;
					break;

				case EnnounInference.Trainer.SentencesTrainType.Online:
					trainingOptions.TrainingMethod = EnnounInference.Sentences.SentenceClassifierTrainingMethod.Online;
					trainingOptions.OnlineTrainingOptions.Regularization = this.CrfWeightsRegularization;
					trainingOptions.OnlineTrainingOptions.OptimizationOptions.MaxIterationsCount = this.MaxSentencesSamples;
					trainingOptions.OnlineTrainingOptions.OptimizationOptions.StepSizeCoefficient = this.CrfStepSizeCoefficient;
					trainingOptions.ShuffleTrainingSamples = this.ShuffleSentenceSamples;
					break;

				default:
					throw new ApplicationException("Unsupported sentences training type.");
			}

			switch (this.DecayFunctionType)
			{
				case DecayFunctionType.Linear:
					trainingOptions.OnlineTrainingOptions.OptimizationOptions.DecayFunction = new Grammophone.Optimization.DecayFunctions.LinearDecayFunction();
					break;
				
				case DecayFunctionType.Root:
					trainingOptions.OnlineTrainingOptions.OptimizationOptions.DecayFunction = new Grammophone.Optimization.DecayFunctions.RootDecayFunction();
					break;
				
				default:
					throw new ApplicationException("Unsupported decay function type.");
			}

			return trainingOptions;
		}

		private IEnumerable<EnnounInference.Words.WordClassifierTrainingOptions> GetWordTrainingOptionsGrid()
		{
			for (double marginSlack = this.MarginSlackStart; marginSlack <= this.MarginSlackEnd; marginSlack *= this.MarginSlackStepFactor)
			{
				for (double stringExponent = this.StringExponentStart; stringExponent <= this.StringExponentEnd; stringExponent *= this.StringExponentStepFactor)
				{
					var trainingOptions = new EnnounInference.Words.WordClassifierTrainingOptions();

					trainingOptions.ClassificationMarginSlack = marginSlack;
					trainingOptions.StringKernelExponent = stringExponent;
					trainingOptions.IsGaussified = false;

					yield return trainingOptions;

					if (this.IncludeGaussian)
					{
						for (double gaussianDeviation = this.GaussianVarianceStart; gaussianDeviation <= this.GaussianVarianceEnd; gaussianDeviation *= this.GaussianVarianceStepFactor)
						{
							trainingOptions = new EnnounInference.Words.WordClassifierTrainingOptions();

							trainingOptions.ClassificationMarginSlack = marginSlack;
							trainingOptions.StringKernelExponent = stringExponent;
							trainingOptions.IsGaussified = true;
							trainingOptions.GaussianDeviation = gaussianDeviation;

							yield return trainingOptions;
						}
					}
				}
			}
		}

		private IEnumerable<EnnounInference.Sentences.SentenceClassifierTrainingOptions> GetSentencesTrainingOptionsGrid()
		{
			yield return GetSentenceTrainingOptions();
		}

		#endregion
	}
}
