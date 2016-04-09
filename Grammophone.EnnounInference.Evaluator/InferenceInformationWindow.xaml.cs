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
using Grammophone.Windows;

namespace Grammophone.EnnounInference.Evaluator
{
	/// <summary>
	/// Interaction logic for InferenceInformationWindow.xaml
	/// </summary>
	public partial class InferenceInformationWindow : Window
	{
		public InferenceInformationWindow()
		{
			InitializeComponent();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			GlassHelper.RegisterGlassHandling(this, GlassHelper.Margins.Full);
		}

	}
}
