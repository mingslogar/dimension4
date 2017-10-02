using System.Windows;

namespace Daytimer.PrintHelpers
{
	public struct PaperSize
	{
		public PaperSize(string description, Size size)
		{
			_description = description;
			_size = size;
		}

		private string _description;
		private Size _size;

		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}

		public Size Size
		{
			get { return _size; }
			set { _size = value; }
		}
	}
}
