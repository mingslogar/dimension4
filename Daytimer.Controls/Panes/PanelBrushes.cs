using System.Windows.Media;

namespace Daytimer.Controls.Panes
{
	class PanelBrushes
	{
		static PanelBrushes()
		{
			LightGrayBrushTransparent = new SolidColorBrush(Color.FromArgb(15, 0, 0, 0));
			LightGrayBrushTransparent.Freeze();

			LightGrayBrushOpaque = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
			LightGrayBrushOpaque.Freeze();
		}

		/// <summary>
		/// Gets a brush A: 15; R: 0; G: 0; B: 0. Overlayed on white this is
		/// equivalent to R: 240; G: 240; B: 240.
		/// </summary>
		public static SolidColorBrush LightGrayBrushTransparent
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a brush R: 240; G: 240; B: 240.
		/// </summary>
		public static SolidColorBrush LightGrayBrushOpaque
		{
			get;
			private set;
		}
	}
}
