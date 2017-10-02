using System.Windows;
using System.Windows.Documents;

namespace Daytimer.Controls.Friction
{
	static class Friction
	{
		public const int TimerInterval = 15;
		public const int FrictionDivisor = 4;
		public const double FrictionTolerance = 0.5;

		public static void ScrollToSelection(this FrictionRichTextBox rtb)
		{
			double verticalOffset = rtb.VerticalOffset;
			double horizontalOffset = rtb.HorizontalOffset;

			// Rectangle corresponding to the coordinates of the selected text.
			Rect screenPos = rtb.Selection.Start.GetCharacterRect(LogicalDirection.Forward);

			Rect adjustedPos = screenPos;
			adjustedPos.Offset(horizontalOffset, verticalOffset);

			Rect rtbPos = new Rect(horizontalOffset, verticalOffset, rtb.ActualWidth, rtb.ActualHeight);

			if (rtbPos.Contains(adjustedPos))
				return;

			double vertical = screenPos.Top + verticalOffset;
			double horizontal = screenPos.Left + horizontalOffset;

			// The vertical - half the size of the RichtextBox to keep the selection centered.
			rtb.ScrollToVerticalPixel(vertical - rtb.ActualHeight / 2);
			rtb.ScrollToHorizontalPixel(horizontal - rtb.ActualWidth / 2);
		}
	}
}
