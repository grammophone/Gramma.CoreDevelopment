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
using Grammophone.EnnounInference.Configuration;
using Grammophone.EnnounInference;

namespace Grammophone.EnnounInference.Evaluator
{
	/// <summary>
	/// Interaction logic for InferenceResourceProviderPickerWindow.xaml
	/// </summary>
	public partial class InferenceResourceProviderPickerWindow : Window
	{
		public InferenceResourceProviderPickerWindow()
		{
			InitializeComponent();

			this.languageComboBox.SelectedValue = null;

			this.DataContext = this;

			this.languageComboBox.ItemsSource = InferenceEnvironment.Setup.InferenceResourceProviders;


		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			GlassHelper.RegisterGlassHandling(this, GlassHelper.Margins.Full);
		}

		public InferenceResourceProvider SelectedInferenceResourceProvider
		{
			get;
			set;
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if (!BindingHelper.AreAllValidated(this)) return;

			this.DialogResult = true;

			this.Close();
		}
	}
}
