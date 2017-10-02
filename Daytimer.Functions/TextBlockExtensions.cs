using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media.TextFormatting;

namespace Daytimer.Functions
{
	public static class TextBlockExtensions
	{
		//private static MethodInfo ParagraphEllipsisShownOnLine;
		//private static PropertyInfo LineCount;

		//public static bool IsTextEllipsing(this TextBlock textBlock)
		//{
		//	if (ParagraphEllipsisShownOnLine == null)
		//	{
		//		ParagraphEllipsisShownOnLine = typeof(TextBlock).GetMethod("ParagraphEllipsisShownOnLine",
		//			BindingFlags.NonPublic | BindingFlags.Instance);
		//	}

		//	if (LineCount == null)
		//	{
		//		LineCount = typeof(TextBlock).GetProperty("LineCount",
		//			BindingFlags.NonPublic | BindingFlags.Instance);
		//	}

		//	int lineCount = (int)LineCount.GetValue(textBlock, null);

		//	bool isEllipsing = (bool)ParagraphEllipsisShownOnLine.Invoke(textBlock,
		//		new object[] { lineCount - 1, textBlock.ActualWidth });

		//	return isEllipsing;
		//}

		private static MethodInfo GetLineDetails;

		/// <summary>
		/// <para>Gets if the TextBlock is ellipsing text.</para>
		/// <para>WARNING: This only works for TextBlocks with a single line of text.</para>
		/// </summary>
		/// <param name="textBlock"></param>
		/// <returns></returns>
		public static bool IsTextEllipsing(this TextBlock textBlock)
		{
			if (GetLineDetails == null)
				GetLineDetails = typeof(TextBlock).GetMethod("GetLineDetails", BindingFlags.NonPublic | BindingFlags.Instance);
			
			object[] args = new object[] { 0, 0, 0, 0, 0 };
			GetLineDetails.Invoke(textBlock, args);

			return (int)args[4] > 0;
		}
	}
}
