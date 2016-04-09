using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Grammophone.EnnounInference.Evaluator
{
	class HyphenationTextProcessorStage : ITextProcessorStage
	{
		#region ITextProcessor Members

		public string Process(string input)
		{
			if (input == null) throw new ArgumentNullException("input");

			var builder = new StringBuilder(input.Length);

			var reader = new StringReader(input);

			string previousWord = null;

			for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
			{
				line = line.Trim();

				if (previousWord != null) builder.Append(previousWord);

				if (line.EndsWith("-"))
				{
					int lastSpaceIndex = line.LastIndexOf(' ');

					previousWord = line.Substring(lastSpaceIndex + 1, line.Length - lastSpaceIndex - 2);

					builder.AppendLine(line.Substring(0, lastSpaceIndex).Trim());
				}
				else
				{
					builder.Append(line);
					builder.Append(' ');
					builder.AppendLine();

					previousWord = null;
				}
			}

			return builder.ToString();
		}

		#endregion
	}
}
