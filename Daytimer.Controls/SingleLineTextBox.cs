using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace Daytimer.Controls
{
	/// <summary>
	/// A text box which only does not allow any newline characters.
	/// </summary>
	[ComVisible(false)]
	public class SingleLineTextBox : TextBox
	{
		#region Constructor

		static SingleLineTextBox()
		{

		}

		public SingleLineTextBox()
		{
			AcceptsReturn = false;
			MaxLines = 1;
		}

		#endregion

		#region Protected Methods

		protected override void OnTextChanged(TextChangedEventArgs e)
		{
			base.OnTextChanged(e);

			Text = Text.Replace("\r\n", " ").Replace('\n', ' ');
		}

		#endregion
	}
}
