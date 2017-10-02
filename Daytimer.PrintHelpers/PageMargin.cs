using System.Windows;

namespace Daytimer.PrintHelpers
{
	public struct PageMargin
	{
		public PageMargin(string description, Thickness margin)
		{
			_description = description;
			_margin = margin;
		}

		private string _description;
		private Thickness _margin;

		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}

		public Thickness Margin
		{
			get { return _margin; }
			set { _margin = value; }
		}
	}
}
