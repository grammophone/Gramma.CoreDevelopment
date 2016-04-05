using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Gramma.Windows;

namespace Gramma.Inference.Evaluator
{
	/// <summary>
	/// Interaction logic for ProgressWindow.xaml
	/// </summary>
	public partial class ProgressWindow : Window
	{
		#region Private fields

		private double currentValue = 0.0, minimum = 0.0, maximum = 100.0;

		private bool allowClose = false;

		#endregion

		#region Construction

		public ProgressWindow()
		{
			InitializeComponent();

			progressBar.Minimum = minimum;
			progressBar.Maximum = maximum;
			progressBar.Value = currentValue;
		}

		#endregion

		#region Public properties

		public double Value
		{
			get
			{
				return currentValue;
			}
			set
			{
				currentValue = value;

				if (progressBar.Dispatcher.CheckAccess())
				{
					progressBar.Value = currentValue;
				}
				else
				{
					progressBar.Dispatcher.BeginInvoke((Action)delegate
					{
						progressBar.Value = currentValue;
					});
				}
			}
		}

		public double Minimum
		{
			get
			{
				return minimum;
			}
			set
			{
				minimum = value;

				if (progressBar.Dispatcher.CheckAccess())
				{
					progressBar.Minimum = minimum;
				}
				else
				{
					progressBar.Dispatcher.BeginInvoke((Action)delegate
					{
						progressBar.Minimum = minimum;
					});
				}
			}
		}

		public double Maximum
		{
			get
			{
				return maximum;
			}
			set
			{
				maximum = value;

				if (progressBar.Dispatcher.CheckAccess())
				{
					progressBar.Maximum = maximum;
				}
				else
				{
					progressBar.Dispatcher.BeginInvoke((Action)delegate
					{
						progressBar.Maximum = maximum;
					});
				}
			}
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Same as Close method, but can be called from any thread.
		/// Note that the Close method does nothing on this window.
		/// </summary>
		public void SafeClose()
		{
			allowClose = true;

			if (this.Dispatcher.CheckAccess())
			{
				this.Close();
			}
			else
			{
				this.Dispatcher.BeginInvoke((Action)this.Close);
			}
		}

		#endregion

		#region Protected methods

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			GlassHelper.RegisterGlassHandling(this, GlassHelper.Margins.Full);
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!e.Cancel) e.Cancel = !allowClose;
		}

		#endregion
	}
}
