using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grammophone.EnnounInference.Evaluator
{
	class CharacterNormalizationStage : ITextProcessorStage
	{
		public string Process(string input)
		{
			if (input == null) throw new ArgumentNullException("input");

			return input
				.Replace('˙', '·'); // Correct the type of upper stop.
		}
	}
}
