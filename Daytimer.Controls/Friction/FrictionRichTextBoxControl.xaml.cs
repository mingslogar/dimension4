using Daytimer.Dialogs;
using Daytimer.Functions;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Daytimer.Controls.Friction
{
	/// <summary>
	/// Interaction logic for FrictionRichTextBoxControl.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class FrictionRichTextBoxControl : FrictionRichTextBox
	{
		public FrictionRichTextBoxControl()
		{
			InitializeComponent();
			Loaded += FrictionTextBoxControl_Loaded;
		}

		private void FrictionTextBoxControl_Loaded(object sender, RoutedEventArgs e)
		{
			ScrollViewer sv = Template.FindName("PART_ContentHost", this) as ScrollViewer;

			if (sv != null)
			{
				sv.ScrollChanged += FrictionTextBoxControl_ScrollChanged;
				sv.LayoutUpdated += sv_LayoutUpdated;
				LayoutScrollBars();
			}
		}

		private void scrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			ScrollToVerticalPixel(e.NewValue * (ExtentHeight - ViewportHeight));
		}

		private void horizontalScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			ScrollToHorizontalPixel(e.NewValue * (ExtentWidth - ViewportWidth));
		}

		private void FrictionTextBoxControl_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.VerticalChange != 0)
			{
				double newValue = VerticalOffset / (ExtentHeight - ViewportHeight);

				if (!double.IsNaN(newValue) && !double.IsInfinity(newValue) && !scrollBar.IsMouseCaptureWithin)
				{
					// Prevent "jumping" of the scroll bar when the friction equation
					// runs and scrolls the content.
					if ((e.VerticalChange > 0 && newValue > scrollBar.Value)
						|| e.VerticalChange < 0 && newValue < scrollBar.Value)
						scrollBar.Value = newValue;
				}
			}
			else if (e.HorizontalChange != 0)
			{
				double newValue = HorizontalOffset / (ExtentWidth - ViewportWidth);

				if (!double.IsNaN(newValue) && !double.IsInfinity(newValue) && !horizontalScrollBar.IsMouseCaptureWithin)
				{
					// Prevent "jumping" of the scroll bar when the friction equation
					// runs and scrolls the content.
					if ((e.HorizontalChange > 0 && newValue > horizontalScrollBar.Value)
						|| e.HorizontalChange < 0 && newValue < horizontalScrollBar.Value)
						horizontalScrollBar.Value = newValue;
				}
			}
		}

		private void sv_LayoutUpdated(object sender, EventArgs e)
		{
			if (IsLoaded)
				LayoutScrollBars();
		}

		private void LayoutScrollBars()
		{
			if (VerticalScrollBarVisibility == ScrollBarVisibility.Auto
				|| VerticalScrollBarVisibility == ScrollBarVisibility.Visible)
			{
				double scrollBarViewportSize = ViewportHeight / (ExtentHeight - ViewportHeight);

				if (!double.IsNaN(scrollBarViewportSize) && !double.IsInfinity(scrollBarViewportSize) && scrollBarViewportSize >= 0)
				{
					scrollBar.Visibility = Visibility.Visible;
					scrollBar.ViewportSize = scrollBarViewportSize;
					scrollBar.SmallChange = (FontFamily.LineSpacing * FontSize) / (ExtentHeight - ViewportHeight);
					scrollBar.LargeChange = ViewportHeight / (ExtentHeight - ViewportHeight);
					scrollBar.Maximum = 1;
				}
				else
				{
					if (VerticalScrollBarVisibility == ScrollBarVisibility.Auto)
						scrollBar.Visibility = Visibility.Collapsed;
					else
						scrollBar.Maximum = 0;
				}
			}
			else
				scrollBar.Visibility = Visibility.Collapsed;

			if (HorizontalScrollBarVisibility == ScrollBarVisibility.Auto)
			{
				double scrollBarViewportSize = ViewportWidth / (ExtentWidth - ViewportWidth);

				if (!double.IsNaN(scrollBarViewportSize) && !double.IsInfinity(scrollBarViewportSize) && scrollBarViewportSize >= 0)
				{
					horizontalScrollBar.Visibility = Visibility.Visible;
					horizontalScrollBar.ViewportSize = scrollBarViewportSize;
					horizontalScrollBar.SmallChange = FontSize / (ExtentWidth - ViewportWidth);
					horizontalScrollBar.LargeChange = ViewportWidth / (ExtentWidth - ViewportWidth);
					horizontalScrollBar.Maximum = 1;
				}
				else
				{
					if (HorizontalScrollBarVisibility == ScrollBarVisibility.Auto)
						horizontalScrollBar.Visibility = Visibility.Collapsed;
					else
						horizontalScrollBar.Maximum = 0;
				}
			}
			else
				horizontalScrollBar.Visibility = Visibility.Collapsed;
		}

		private ScrollBar scrollBar
		{
			get { return Template.FindName("scrollBar", this) as ScrollBar; }
		}

		private ScrollBar horizontalScrollBar
		{
			get { return Template.FindName("horizontalScrollBar", this) as ScrollBar; }
		}

		#region Find/Replace

		public void ShowSearch()
		{
			SearchUI search = (SearchUI)Template.FindName("searchUI", this);
			search.SearchMode = SearchMode.Find;
			search.IsOpen = true;
		}

		public void ShowReplace()
		{
			SearchUI search = (SearchUI)Template.FindName("searchUI", this);
			search.SearchMode = SearchMode.Replace;
			search.IsOpen = true;
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);

			if (e.Key == Key.F && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
			{
				if (!IsReadOnly && Experiments.DocumentSearch)
				{
					ShowSearch();
					e.Handled = true;
				}
			}
		}

		//private void searchUI_SearchExecuted(object sender, SearchExecutedEventArgs e)
		//{
		//	// Try first to find a match starting at the current selection position.
		//	string all = new TextRange(Selection.Start, Document.ContentEnd).Text;
		//	string query = e.Query;

		//	if (query == null)
		//		return;

		//	if (!e.Options.HasFlag(SearchOptions.Regex))
		//		query = Regex.Escape(query);

		//	if (e.Options.HasFlag(SearchOptions.WholeWord))
		//		query = "\\b" + query + "\\b";

		//	Match match = Regex.Match(all, query, !e.Options.HasFlag(SearchOptions.MatchCase) ? RegexOptions.IgnoreCase : RegexOptions.None);

		//	// If we couldn't find a match, re-run the search from the beginning of the document,
		//	// and include the entire document.
		//	if (!match.Success)
		//	{
		//		all = new TextRange(Document.ContentStart, Document.ContentEnd).Text;
		//		match = Regex.Match(all, query, !e.Options.HasFlag(SearchOptions.MatchCase) ? RegexOptions.IgnoreCase : RegexOptions.None);
		//	}

		//	if (match.Success)
		//	{
		//		Selection.Select(Document.ContentStart.GetPositionAtOffset(match.Index + 2),
		//			Document.ContentStart.GetPositionAtOffset(match.Index + match.Length + 2));
		//		Focus();
		//	}
		//	else
		//	{
		//		TaskDialog dlg = new TaskDialog(Window.GetWindow(this), "Find", "The query you requested was not found.", MessageType.Error);
		//		dlg.ShowDialog();
		//	}
		//}

		private void searchUI_SearchExecuted(object sender, SearchExecutedEventArgs e)
		{
			TextRange match = Find(e);

			if (match != null)
			{
				Selection.Select(match.Start, match.End);
				this.ScrollToSelection();
				Focus();
			}
			else
				ShowNotFoundMessage(e.Query);
		}

		private void searchUI_ReplaceExecuted(object sender, ReplaceExecutedEventArgs e)
		{
			if (e.Query == e.Replace)
				return;

			if (e.Mode == ReplaceMode.Next)
			{
				TextRange match = Find(e);

				if (match != null)
				{
					BeginChange();

					match.Text = e.Replace;
					Selection.Select(match.Start, match.End);
					this.ScrollToSelection();

					EndChange();
				}
				else
					ShowNotFoundMessage(e.Query);
			}
			else
			{
				TextRange[] matches = FindAll(e);
				int length = matches.Length;

				if (length > 0)
				{
					BeginChange();

					foreach (TextRange each in matches)
						each.Text = e.Replace;

					EndChange();

					new TaskDialog(Window.GetWindow(this), "Replace", "We replaced " + length.ToString() + " occurrence" + (length != 1 ? "s" : "") + " of your query in the document.", MessageType.Information).ShowDialog();
				}
				else
					ShowNotFoundMessage(e.Query);
			}

			//TextRange match = Find(e);

			//if (e.Mode == ReplaceMode.Next)
			//{
			//	if (match != null)
			//	{
			//		BeginChange();

			//		match.Text = e.Replace;
			//		Selection.Select(match.Start, match.End);

			//		EndChange();

			//		Focus();
			//	}
			//	else
			//		ShowNotFoundMessage(e.Query);
			//}
			//else
			//{
			//	int count = 0;

			//	if (match != null)
			//	{
			//		BeginChange();

			//		while (match != null)
			//		{
			//			count++;

			//			match.Text = e.Replace;
			//			Selection.Select(match.End, match.End);
			//			match = Find(e);
			//		}

			//		if (count > 0)
			//			EndChange();
			//		else
			//			new TaskDialog(Window.GetWindow(this), "Replace", "We replaced " + count.ToString() + " occurrence" + (count != 1 ? "s" : "") + " of your query in the document.", MessageType.Error).ShowDialog();
			//	}
			//	else
			//		ShowNotFoundMessage(e.Query);
			//}
		}

		private TextRangeStack _cachedStack = null;

		private TextRange Find(SearchExecutedEventArgs e)
		{
			string query = e.Query;

			if (query == null)
				return null;

			if (!e.Options.HasFlag(SearchOptions.Regex))
				query = Regex.Escape(query);

			if (e.Options.HasFlag(SearchOptions.WholeWord))
				query = "\\b" + query + "\\b";

			try
			{
				return Document.Find(query, Selection.End, !e.Options.HasFlag(SearchOptions.MatchCase) ? RegexOptions.IgnoreCase : RegexOptions.None, out _cachedStack, _cachedStack);
			}
			catch (ArgumentException)
			{
				// Invalid Regex.
				query = Regex.Escape(query);
				return Document.Find(query, Selection.End, !e.Options.HasFlag(SearchOptions.MatchCase) ? RegexOptions.IgnoreCase : RegexOptions.None, out _cachedStack, _cachedStack);
			}

			//TextRange match = FindQueryFromPosition(Selection.End, query, !e.Options.HasFlag(SearchOptions.MatchCase) ? RegexOptions.IgnoreCase : RegexOptions.None);

			//// If we couldn't find a match, re-run the search from the beginning of the document,
			//// and include the entire document.
			//if (match == null)
			//	match = FindQueryFromPosition(Document.ContentStart, query, !e.Options.HasFlag(SearchOptions.MatchCase) ? RegexOptions.IgnoreCase : RegexOptions.None);

			//return match;
		}

		private TextRange[] FindAll(SearchExecutedEventArgs e)
		{
			string query = e.Query;

			if (query == null)
				return null;

			if (!e.Options.HasFlag(SearchOptions.Regex))
				query = Regex.Escape(query);

			if (e.Options.HasFlag(SearchOptions.WholeWord))
				query = "\\b" + query + "\\b";

			try
			{
				return Document.FindAll(query, !e.Options.HasFlag(SearchOptions.MatchCase) ? RegexOptions.IgnoreCase : RegexOptions.None, out _cachedStack, _cachedStack);
			}
			catch (ArgumentException)
			{
				// Invalid Regex.
				query = Regex.Escape(query);
				return Document.FindAll(query, !e.Options.HasFlag(SearchOptions.MatchCase) ? RegexOptions.IgnoreCase : RegexOptions.None, out _cachedStack, _cachedStack);
			}
		}

		//private TextRange FindQueryFromPosition(TextPointer position, string query, RegexOptions options)
		//{
		//	while (position != null)
		//	{
		//		if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
		//		{
		//			string textRun = position.GetTextInRun(LogicalDirection.Forward);
		//			Match match = Regex.Match(textRun, query, options);

		//			if (match.Success)
		//			{
		//				TextPointer start = position.GetPositionAtOffset(match.Index);
		//				TextPointer end = start.GetPositionAtOffset(match.Length);
		//				return new TextRange(start, end);
		//			}
		//		}

		//		position = position.GetNextContextPosition(LogicalDirection.Forward);
		//	}

		//	return null;
		//}

		private void ShowNotFoundMessage(string query)
		{
			new TaskDialog(Window.GetWindow(this), "Not Found", "We couldn't find your query, " + query + ", in the current document.", MessageType.Error).ShowDialog();
		}

		protected override void OnTextChanged(TextChangedEventArgs e)
		{
			base.OnTextChanged(e);
			_cachedStack = null;
		}

		#endregion
	}
}
