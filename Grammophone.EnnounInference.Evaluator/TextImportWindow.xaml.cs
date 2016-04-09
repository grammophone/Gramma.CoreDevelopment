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
	/// Interaction logic for TextImportWindow.xaml
	/// </summary>
	public partial class TextImportWindow : Window
	{
		private string text;

		public TextImportWindow()
		{
			InitializeComponent();

			text = String.Empty;

			this.DataContext = this;
		}

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				text = value;
			}
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			GlassHelper.RegisterGlassHandling(this, GlassHelper.Margins.Full);
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

	}
}
