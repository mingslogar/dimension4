using Daytimer.Dialogs;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace CharMap
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class CharMapDialog : DialogBase
	{
		public CharMapDialog()
		{
			InitializeComponent();
			Loaded += MainWindow_Loaded;
		}

		private List<char> allChars = new List<char>();

		private char _selected;

		public char Selected
		{
			get { return _selected; }
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			//for (int i = 0; i <= 0xfffc; i++)0xffee
			for (int i = 0x20; i <= 0xfe23; i++)// 0xff
			{
				char c = (char)i;

				if (IsValidChar(c))
					allChars.Add(c);
			}

			length = allChars.Count;
			PopulateDisplay(120);
		}

		private bool IsValidChar(char c)
		{
			UnicodeCategory cat = char.GetUnicodeCategory(c);

			if (cat == UnicodeCategory.OtherNotAssigned ||
				cat == UnicodeCategory.Control ||
				cat == UnicodeCategory.OtherLetter ||
				cat == UnicodeCategory.OtherNumber ||
				cat == UnicodeCategory.OtherSymbol ||
				cat == UnicodeCategory.PrivateUse ||
				cat == UnicodeCategory.Surrogate)
				return false;

			return true;
		}

		int counter = 0;
		int length;

		private void PopulateDisplay(int num)
		{
			int max = counter + num < length ? counter + num : length;
			for (; counter < max; counter++)
			{
				char c = allChars[counter];
				//UnicodeCategory cat = char.GetUnicodeCategory(c);

				RadioButton tb = new RadioButton();
				tb.Content = c;
				//tb.ToolTip = cat.ToString() + " (" + i.ToString() + ")";
				wrapPanel.Children.Add(tb);
			}
		}

		private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.VerticalChange > 0 && e.VerticalOffset >= scrollViewer.ScrollableHeight - 28)
			{
				int numPerRow = (int)(wrapPanel.ActualWidth / 2);
				int rows = (int)(e.VerticalChange / 28);
				rows = rows > 0 ? rows : 1;
				PopulateDisplay(rows * numPerRow);
			}
		}

		private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.NewSize.Height * e.NewSize.Width > e.PreviousSize.Height * e.PreviousSize.Width)
			{
				int numPerRow = (int)(wrapPanel.ActualWidth / 2);
				int rows = (int)((e.NewSize.Height - e.PreviousSize.Height) / 28);
				rows = rows > 0 ? rows : 1;
				PopulateDisplay(rows * numPerRow);
			}
		}

		private void scrollBar_ValueChanged(object sender, RoutedEventArgs e)
		{
			scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + ((ValueChangedEventArgs)e).Value);
		}

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			okButton.IsEnabled = true;

			char c = (char)((ContentControl)sender).Content;
			_selected = c;
			characterCode.Text = "Character code: " + Conversion.Hex(c).PadLeft(4, '0');
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			RaiseInsertEvent();
		}

		#region RoutedEvents

		public static readonly RoutedEvent InsertEvent = EventManager.RegisterRoutedEvent(
			"Insert", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CharMapDialog));

		public event RoutedEventHandler Insert
		{
			add { AddHandler(InsertEvent, value); }
			remove { RemoveHandler(InsertEvent, value); }
		}

		private void RaiseInsertEvent()
		{
			RoutedEventArgs newEventArgs = new RoutedEventArgs(CharMapDialog.InsertEvent);
			RaiseEvent(newEventArgs);
		}

		#endregion
	}
}
