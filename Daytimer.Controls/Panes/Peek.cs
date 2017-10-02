using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace Daytimer.Controls.Panes
{
	[ComVisible(false)]
	public class Peek : UserControl
	{
		#region Constructors

		public Peek()
		{

		}

		#endregion

		#region Public Properties

		virtual public string Source
		{
			get { return null; }
		}

		#endregion

		#region Public Methods

		virtual public void Load()
		{

		}

		#endregion
	}
}
