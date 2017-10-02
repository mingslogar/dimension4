using System.Windows.Media;

namespace Daytimer.Functions.Theme
{
	public class Theme
	{
		public Theme(string name, Brush brush)
		{
			Name = name;
			Brush = brush;
		}

		public string Name
		{
			get;
			internal set;
		}

		public Brush Brush
		{
			get;
			internal set;
		}
	}
}
