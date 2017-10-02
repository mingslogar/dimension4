using Daytimer.Fundamentals;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Samples.CustomControls
{
	/// <summary>
	/// Interaction logic for ColorPickerDialog.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class ColorPickerDialog : OfficeWindow
	{
		public ColorPickerDialog()
		{
			InitializeComponent();
		}

		public Color SelectedColor
		{
			get { return cPicker.SelectedColor; }
			set { cPicker.SelectedColor = value; }
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}