using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for IntegerDropDown.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class IntegerDropDown : ComboBox
	{
		public IntegerDropDown()
		{
			InitializeComponent();
		}

		private string _originalText;

		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnGotKeyboardFocus(e);
			_originalText = Text;
		}

		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnLostKeyboardFocus(e);

			try { int.Parse(Text); }
			catch { Text = _originalText; }
		}
	}
}
