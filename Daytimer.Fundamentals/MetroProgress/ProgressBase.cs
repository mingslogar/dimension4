using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace Daytimer.Fundamentals.MetroProgress
{
	/// <summary>
	/// The base class for metro progress bars
	/// </summary>
	[ComVisible(false)]
	public abstract class ProgressBase : Control
	{
		#region Public Fields

		public bool IsAnimationRunning
		{
			get;
			internal set;
		}

		#endregion

		#region Public Methods

		abstract public void Start();
		abstract public void Stop();
		abstract public void Reset();

		#endregion
	}
}
