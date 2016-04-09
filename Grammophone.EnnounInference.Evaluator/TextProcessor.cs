using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grammophone.EnnounInference.Evaluator
{
	class TextProcessor
	{
		private IList<ITextProcessorStage> stages = new List<ITextProcessorStage>();

		public IList<ITextProcessorStage> Stages
		{
			get
			{
				return stages;
			}
		}

		public static TextProcessor Default
		{
			get
			{
				var processor = new TextProcessor();

				processor.Stages.Add(new HyphenationTextProcessorStage());

				return processor;
			}
		}

		public string Execute(string text)
		{
			if (text == null) throw new ArgumentNullException("text");

			foreach (var stage in this.Stages)
			{
				text = stage.Process(text);
			}

			return text;
		}
	}
}
