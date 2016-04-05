using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gramma.Inference.Evaluator
{
	public interface ITextProcessorStage
	{
		string Process(string input);
	}
}
