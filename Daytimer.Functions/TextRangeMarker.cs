using System.Windows.Documents;

namespace Daytimer.Functions
{
	/// <summary>
	/// Contains a TextRange and information about where the TextRange is positioned in
	/// the overall string.
	/// </summary>
	struct TextRangeMarker
	{
		public TextRangeMarker(TextRange textRange, int start, int end)
		{
			TextRange = textRange;
			Start = start;
			End = end;
		}

		public TextRange TextRange;

		/// <summary>
		/// The starting position of the TextRange.
		/// </summary>
		public int Start;

		/// <summary>
		/// The ending position of the TextRange.
		/// </summary>
		public int End;
	}
}
