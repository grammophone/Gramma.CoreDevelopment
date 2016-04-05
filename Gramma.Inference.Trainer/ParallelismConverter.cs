using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Gramma.Inference.Trainer
{
	public class ParallelismConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) throw new ArgumentNullException("value");

			int parallelism = (int)value;

			if (parallelism == 0)
				return "full";
			else
				return parallelism.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) throw new ArgumentNullException("value");

			var stringValue = (string)value;

			int parallelism;

			if (!int.TryParse(stringValue, out parallelism))
			{
				return 0;
			}

			return parallelism;
		}

		#endregion
	}
}
