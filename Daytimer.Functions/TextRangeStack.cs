using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace Daytimer.Functions
{
	public class TextRangeStack
	{
		public TextRangeStack(FlowDocument document)
		{
			char[] charArray = null;
			text = GetTextInternal(document.ContentStart, document.ContentEnd, ref charArray);

			//foreach (TextElement element in document.Blocks)
			//	ProcessTextElement(element);
		}

		//private void ProcessTextElement(TextElement element)
		//{
		//	Run run = element as Run;

		//	if (run == null)
		//	{
		//		IEnumerable children = LogicalTreeHelper.GetChildren(element);

		//		foreach (object child in children)
		//		{
		//			TextElement elem = child as TextElement;

		//			if (elem != null)
		//				ProcessTextElement(elem);
		//		}
		//	}
		//	else
		//	{
		//		string addend = "";

		//		switch (run.ElementEnd.GetPointerContext(LogicalDirection.Forward))
		//		{
		//			case TextPointerContext.ElementEnd:
		//				Type elementType = run.ElementEnd.Parent.GetType();//.GetAdjacentElement(LogicalDirection.Forward).GetType();

		//				if (!typeof(Paragraph).IsAssignableFrom(elementType) &&
		//					!typeof(BlockUIContainer).IsAssignableFrom(elementType))
		//					break;

		//				addend = Environment.NewLine;
		//				break;

		//			case TextPointerContext.ElementStart:
		//				break;

		//			case TextPointerContext.EmbeddedElement:
		//				addend = " ";
		//				break;

		//			case TextPointerContext.None:
		//				break;

		//			case TextPointerContext.Text:
		//				break;
		//		}

		//		Add(new TextRange(run.ContentStart, run.ContentEnd), addend);
		//	}
		//}

		private List<TextRangeMarker> ranges = new List<TextRangeMarker>(100);
		private string text = "";

		public string Text
		{
			get { return text; }
		}

		//private void Add(TextRange textRange, string addend)
		//{
		//	int start = text.Length;
		//	text += textRange.Text + addend;
		//	ranges.Add(new TextRangeMarker(textRange, start, text.Length));
		//}

		private void Add(TextPointer rangeStart, TextPointer rangeEnd, int start, int end)
		{
			if (start != end)
				ranges.Add(new TextRangeMarker(new TextRange(rangeStart, rangeEnd), start, end));
		}

		public TextRange Select(int start, int end)
		{
			TextRangeMarker? first = BinarySearch(start);

			if (first == null)
				return null;

			TextRangeMarker? last = BinarySearch(end);

			if (last == null)
				return null;

			return new TextRange(
				first.Value.TextRange.Start.GetPositionAtOffset(start - first.Value.Start, LogicalDirection.Forward),
				last.Value.TextRange.Start.GetPositionAtOffset(end - last.Value.Start, LogicalDirection.Forward)
			);
		}

		/// <summary>
		/// Gets the TextRangeMarker which encases an integer location.
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		private TextRangeMarker? BinarySearch(int location)
		{
			int count = ranges.Count;
			int lowerbound = 0;
			int upperbound = count;

			if (count > 0)
			{
				while (true)
				{
					if (upperbound - lowerbound == 1)
					{
						int index = lowerbound;

						TextRangeMarker marker = ranges[index];

						if (location < marker.Start)
						{
							if (index > 0)
								return ranges[index - 1];
							else
								return null;
						}
						else if (location > marker.End)
						{
							if (index + 1 < count)
								return ranges[index + 1];
							else
								return null;
						}
						else
							return marker;
					}
					else
					{
						TextRangeMarker middle = ranges[lowerbound + (upperbound - lowerbound) / 2];

						if (location < middle.Start)
							upperbound -= (upperbound - lowerbound) / 2;
						else if (location > middle.End)
							lowerbound += (upperbound - lowerbound) / 2;
						else
							return middle;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the integer position of a TextPointer.
		/// </summary>
		/// <param name="pointer"></param>
		/// <returns></returns>
		public int GetPosition(TextPointer pointer)
		{
			foreach (TextRangeMarker marker in ranges)
			{
				if (marker.TextRange.Contains(pointer))
					return marker.TextRange.Start.GetOffsetToPosition(pointer) + marker.Start;
			}

			return 0;
		}

		#region Based on code from PresentationFramework.dll

		internal string GetTextInternal(TextPointer startPosition, TextPointer endPosition, ref char[] charArray)
		{
			StringBuilder textBuffer = new StringBuilder();
			Stack<int> listItemCounter = null;
			TextPointer navigator = startPosition;
			Debug.Assert(startPosition.CompareTo(endPosition) <= 0, "expecting: startPosition <= endPosition");

			while (navigator.CompareTo(endPosition) < 0)
			{
				Type elementType;
				int start = textBuffer.Length;
				TextPointer begin = navigator;

				switch (navigator.GetPointerContext(LogicalDirection.Forward))
				{
					case TextPointerContext.Text:
						{
							PlainConvertTextRun(textBuffer, ref navigator, endPosition, ref charArray);
							Add(begin, navigator, start, textBuffer.Length);
							continue;
						}

					case TextPointerContext.EmbeddedElement:
						{
							textBuffer.Append(' ');
							navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
							Add(begin, navigator, start, textBuffer.Length);
							continue;
						}

					case TextPointerContext.ElementStart:
						{
							elementType = navigator.GetAdjacentElement(LogicalDirection.Forward).GetType();

							if (!typeof(AnchoredBlock).IsAssignableFrom(elementType))
							{
								if (typeof(List).IsAssignableFrom(elementType) && (navigator is TextPointer))
									PlainConvertListStart(navigator, ref listItemCounter);
								else if (typeof(ListItem).IsAssignableFrom(elementType))
									PlainConvertListItemStart(textBuffer, navigator, ref listItemCounter);
								else
									PlainConvertAccessKey(textBuffer, navigator);
							}

							//textBuffer.Append(Environment.NewLine);
							navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
							Add(begin, navigator, start, textBuffer.Length);
							continue;
						}

					case TextPointerContext.ElementEnd:
						{
							elementType = navigator.Parent.GetType();

							if (!typeof(Paragraph).IsAssignableFrom(elementType) &&
								!typeof(BlockUIContainer).IsAssignableFrom(elementType))
								break;

							PlainConvertParagraphEnd(textBuffer, ref navigator);
							Add(begin, navigator, start, textBuffer.Length);
							continue;
						}

					default:
						Debug.Assert(false, "Unexpected value for TextPointerContext");
						continue;
				}

				if (typeof(LineBreak).IsAssignableFrom(elementType))
				{
					navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
					textBuffer.Append(Environment.NewLine);
					Add(begin, navigator, start, textBuffer.Length);
				}
				else if (typeof(List).IsAssignableFrom(elementType))
				{
					PlainConvertListEnd(ref navigator, ref listItemCounter);
					Add(begin, navigator, start, textBuffer.Length);
				}
				else
					navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);

				continue;
			}

			return textBuffer.ToString();
		}

		private static void PlainConvertAccessKey(StringBuilder textBuffer, TextPointer navigator)
		{
			//if (AccessText.HasCustomSerialization(navigator.GetAdjacentElement(LogicalDirection.Forward)))
			//{
			//	textBuffer.Append('_');
			//}
		}

		private static void PlainConvertListEnd(ref TextPointer navigator, ref Stack<int> listItemCounter)
		{
			if ((listItemCounter != null) && (listItemCounter.Count > 0))
			{
				listItemCounter.Pop();
			}
			navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
		}

		private static void PlainConvertListItemStart(StringBuilder textBuffer, TextPointer navigator, ref Stack<int> listItemCounter)
		{
			if (navigator is TextPointer)
			{
				List parent = (List)((TextPointer)navigator).Parent;
				ListItem adjacentElement = (ListItem)navigator.GetAdjacentElement(LogicalDirection.Forward);

				if (listItemCounter == null)
				{
					listItemCounter = new Stack<int>(1);
				}
				if (listItemCounter.Count == 0)
				{
					listItemCounter.Push(Array.IndexOf(adjacentElement.SiblingListItems.ToArray<ListItem>(), adjacentElement));
				}

				int item = listItemCounter.Pop();
				int num2 = (parent != null) ? parent.StartIndex : 0;
				TextMarkerStyle listMarkerStyle = (parent != null) ? parent.MarkerStyle : TextMarkerStyle.Disc;
				WriteListMarker(textBuffer, listMarkerStyle, item + num2);
				item++;
				listItemCounter.Push(item);
			}
		}

		private static void PlainConvertListStart(TextPointer navigator, ref Stack<int> listItemCounter)
		{
			List adjacentElement = (List)navigator.GetAdjacentElement(LogicalDirection.Forward);
			if (listItemCounter == null)
			{
				listItemCounter = new Stack<int>(1);
			}
			listItemCounter.Push(0);
		}

		private static void PlainConvertParagraphEnd(StringBuilder textBuffer, ref TextPointer navigator)
		{
			//	navigator = navigator.GetInsertionPosition(LogicalDirection.Backward);
			bool flag = navigator.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart;
			navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
			//	navigator = navigator.GetInsertionPosition(LogicalDirection.Forward);
			TextPointerContext pointerContext = navigator.GetPointerContext(LogicalDirection.Forward);

			if ((flag && (pointerContext == TextPointerContext.ElementEnd))
				&& typeof(TableCell).IsAssignableFrom(navigator.Parent.GetType()))
			{
				navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);

				if (navigator.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart)
					textBuffer.Append('\t');
				else
					textBuffer.Append(Environment.NewLine);
			}
			else
				textBuffer.Append(Environment.NewLine);
		}

		private static void PlainConvertTextRun(StringBuilder textBuffer, ref TextPointer navigator, TextPointer endPosition, ref char[] charArray)
		{
			int textRunLength = navigator.GetTextRunLength(LogicalDirection.Forward);
			charArray = EnsureCharArraySize(charArray, textRunLength);
			textRunLength = GetTextWithLimit(navigator, LogicalDirection.Forward, charArray, 0, textRunLength, endPosition);
			textBuffer.Append(charArray, 0, textRunLength);
			navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
		}

		private static void WriteListMarker(StringBuilder textBuffer, TextMarkerStyle listMarkerStyle, int listItemNumber)
		{
			string str = null;
			char[] chArray = null;

			switch (listMarkerStyle)
			{
				case TextMarkerStyle.None:
					str = "";
					break;

				case TextMarkerStyle.Disc:
					str = "•";
					break;

				case TextMarkerStyle.Circle:
					str = "○";
					break;

				case TextMarkerStyle.Square:
					str = "□";
					break;

				case TextMarkerStyle.Box:
					str = "■";
					break;

				case TextMarkerStyle.LowerRoman:
					str = ConvertNumberToRomanString(listItemNumber, false);
					break;

				case TextMarkerStyle.UpperRoman:
					str = ConvertNumberToRomanString(listItemNumber, true);
					break;

				case TextMarkerStyle.LowerLatin:
					chArray = ConvertNumberToString(listItemNumber, true, "abcdefghijklmnopqrstuvwxyz");
					break;

				case TextMarkerStyle.UpperLatin:
					chArray = ConvertNumberToString(listItemNumber, true, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
					break;

				case TextMarkerStyle.Decimal:
					chArray = ConvertNumberToString(listItemNumber, false, "0123456789");
					break;
			}

			if (str != null)
				textBuffer.Append(str);
			else if (chArray != null)
				textBuffer.Append(chArray, 0, chArray.Length);

			textBuffer.Append('\t');
		}

		private static string ConvertNumberToRomanString(int number, bool uppercase)
		{
			if (number > 0xf9f)
				return number.ToString(CultureInfo.InvariantCulture);

			StringBuilder builder = new StringBuilder();
			AddRomanNumeric(builder, number / 0x3e8, RomanNumerics[uppercase ? 1 : 0][0]);
			number = number % 0x3e8;
			AddRomanNumeric(builder, number / 100, RomanNumerics[uppercase ? 1 : 0][1]);
			number = number % 100;
			AddRomanNumeric(builder, number / 10, RomanNumerics[uppercase ? 1 : 0][2]);
			number = number % 10;
			AddRomanNumeric(builder, number, RomanNumerics[uppercase ? 1 : 0][3]);
			builder.Append('.');

			return builder.ToString();
		}

		private static char[] ConvertNumberToString(int number, bool oneBased, string numericSymbols)
		{
			if (oneBased)
				number--;

			Debug.Assert(number >= 0, "expecting: number >= 0");
			int length = numericSymbols.Length;

			if (number < length)
				return new char[] { numericSymbols[number], '.' };

			int num2 = oneBased ? 1 : 0;
			int index = 1;
			int num4 = length;
			int num5 = length;

			while (number >= num4)
			{
				num5 *= length;
				num4 = num5 + (num4 * num2);
				index++;
			}

			char[] chArray = new char[index + 1];
			chArray[index] = '.';

			for (int i = index - 1; i >= 0; i--)
			{
				chArray[i] = numericSymbols[number % length];
				number = (number / length) - num2;
			}

			return chArray;
		}

		private const string DecimalNumerics = "0123456789";
		private const string LowerLatinNumerics = "abcdefghijklmnopqrstuvwxyz";
		private const char NumberSuffix = '.';
		private static string[][] RomanNumerics = new string[][] { new string[] { "m??", "cdm", "xlc", "ivx" }, new string[] { "M??", "CDM", "XLC", "IVX" } };
		private const string UpperLatinNumerics = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		private static void AddRomanNumeric(StringBuilder builder, int number, string oneFiveTen)
		{
			Debug.Assert((number >= 0) && (number <= 9), "expecting: number >= 0 && number <= 9");

			if ((number >= 1) && (number <= 9))
			{
				if ((number == 4) || (number == 9))
					builder.Append(oneFiveTen[0]);

				if (number == 9)
					builder.Append(oneFiveTen[2]);
				else
				{
					if (number >= 4)
						builder.Append(oneFiveTen[1]);

					for (int i = number % 5; (i > 0) && (i < 4); i--)
						builder.Append(oneFiveTen[0]);
				}
			}
		}

		private static char[] EnsureCharArraySize(char[] charArray, int textLength)
		{
			if (charArray == null)
			{
				charArray = new char[textLength + 10];
				return charArray;
			}

			if (charArray.Length < textLength)
			{
				int num = charArray.Length * 2;

				if (num < textLength)
					num = textLength + 10;

				charArray = new char[num];
			}

			return charArray;
		}

		internal static int GetTextWithLimit(TextPointer thisPointer, LogicalDirection direction, char[] textBuffer, int startIndex, int count, TextPointer limit)
		{
			int num2;

			if (limit == null)
				return thisPointer.GetTextInRun(direction, textBuffer, startIndex, count);

			if ((direction == LogicalDirection.Forward) && (limit.CompareTo(thisPointer) <= 0))
				return 0;

			if ((direction == LogicalDirection.Backward) && (limit.CompareTo(thisPointer) >= 0))
				return 0;

			if (direction == LogicalDirection.Forward)
				num2 = Math.Min(count, thisPointer.GetOffsetToPosition(limit));
			else
				num2 = Math.Min(count, limit.GetOffsetToPosition(thisPointer));

			num2 = Math.Min(count, num2);

			return thisPointer.GetTextInRun(direction, textBuffer, startIndex, num2);
		}

		#endregion
	}
}