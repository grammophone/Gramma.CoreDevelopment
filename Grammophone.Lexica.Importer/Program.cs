using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

using Grammophone.Lexica.Configuration;
using Grammophone.Lexica.Sources;

namespace Grammophone.Lexica.Importer
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length != 2)
				{
					Error.WriteLine("Two arguments are required.");
					PrintUsage();

					return;
				}

				string lexiconName = args[0].Trim();
				string filename = args[1].Trim();

				foreach (var sourceSet in LexicaEnvironment.Setup.LexiconSourceSets)
				{
					if (sourceSet.Name == lexiconName)
					{
						ImportSourceSet(sourceSet, filename);

						return;
					}
				}

				Error.WriteLine($"No lexicon has been defined with name '{lexiconName}'");
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
		}

		private static void HandleException(Exception exception, int level = 0)
		{
			var aggregateException = exception as AggregateException;

			if (aggregateException != null)
			{
				if (aggregateException.InnerExceptions.Count == 1)
				{
					HandleException(aggregateException.InnerExceptions[0]);

					return;
				}

				Error.Write(new String('\t', level));
				Error.WriteLine($"Exception type: AggregateException");

				foreach (var innerException in aggregateException.InnerExceptions)
				{
					HandleException(innerException, level + 1);
				}

				return;
			}

			Error.Write(new String('\t', level));
			Error.WriteLine($"Exception type: {exception.GetType().FullName}, Message: '{exception.Message}'");

			if (exception.InnerException != null)
			{
				Error.Write(new String('\t', level));
				Error.WriteLine("Inner Exception:");

				HandleException(exception.InnerException, level + 1);
			}
		}

		private static void ImportSourceSet(LexiconSourceSet sourceSet, string filename)
		{
			var importTask = sourceSet.ImportLexiconAsync();

			LexicaEnvironment.SaveLexiconAsync(importTask.Result, filename).Wait();
		}

		private static void PrintUsage()
		{
			WriteLine("Usage:");
			WriteLine("Grammophone.LexicaImporter <lexicon name> <output filename>");
		}
	}
}
