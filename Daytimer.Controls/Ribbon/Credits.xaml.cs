using Daytimer.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Resources;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for Credits.xaml
	/// </summary>
	public partial class Credits : DialogBase
	{
		public Credits()
		{
			InitializeComponent();
		}

		private void Link_Click(object sender, RoutedEventArgs e)
		{
			ContentControl _sender = ((ContentControl)sender);
			_sender.Content = (string)_sender.Content == "Show License" ? "Hide License" : "Show License";

			UIElement parent = (UIElement)_sender.Parent;
			FlowDocumentScrollViewer fdsv = (FlowDocumentScrollViewer)stackPanel.Children[stackPanel.Children.IndexOf(parent) + 1];

			fdsv.Visibility = fdsv.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

			if (fdsv.Document == null)
			{
				Uri uri = new Uri("pack://application:,,,/Daytimer.Controls;component/Ribbon/Licenses/" + (string)fdsv.Tag + ".xaml", UriKind.Absolute);
				StreamResourceInfo info = Application.GetResourceStream(uri);
				fdsv.Document = (FlowDocument)XamlReader.Load(info.Stream);
			}
		}
	}
}
