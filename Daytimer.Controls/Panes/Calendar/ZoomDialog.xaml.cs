using Daytimer.Dialogs;
using Daytimer.Functions;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for ZoomDialog.xaml
	/// </summary>
	public partial class ZoomDialog : DialogBase
	{
		public ZoomDialog(double value)
		{
			InitializeComponent();
			SelectedZoom = value;
			InitializeClockLabels();
		}

		private double _selectedZoom;

		public double SelectedZoom
		{
			get { return _selectedZoom; }
			set
			{
				_selectedZoom = value;

				if (value == 50)
					(radioGrid.Children[4] as RadioButton).IsChecked = true;
				else if (value == 100)
					(radioGrid.Children[3] as RadioButton).IsChecked = true;
				else if (value == 200)
					(radioGrid.Children[2] as RadioButton).IsChecked = true;
				else if (value == 300)
					(radioGrid.Children[1] as RadioButton).IsChecked = true;
				else if (value == 600)
					(radioGrid.Children[0] as RadioButton).IsChecked = true;

				clockGrid.Height = (int)((int)(1056 * _selectedZoom / 100) / 528) * 528;
			}
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			if (IsLoaded)
			{
				int i = radioGrid.Children.IndexOf(sender as RadioButton);
				SelectedZoom = i == 0 ? 600 : i == 1 ? 300 : i == 2 ? 200 : i == 3 ? 100 : i == 4 ? 50 : _selectedZoom;
			}
		}

		private void InitializeClockLabels()
		{
			for (int i = 0; i < 24; i++)
			{
				RowDefinition row = new RowDefinition();
				row.Height = new GridLength(1, GridUnitType.Star);
				clockTimesGrid.RowDefinitions.Add(row);

				Border border2 = new Border();
				border2.SetResourceReference(Border.BorderBrushProperty, "Gray");

				if (i < 23)
					border2.BorderThickness = new Thickness(0, 0, 0, 1);
				else
					// We still want to add the border, otherwise some other
					// functions which loop through would have to handle for
					// the border being nonexistent.
					border2.BorderThickness = new Thickness(0);

				Grid.SetRow(border2, i);
				clockTimesGrid.Children.Add(border2);

				TextControl text = new TextControl();
				text.FontSize = 15;
				text.Name = "h" + i.ToString();
				text.IsHourDisplay = true;
				Grid.SetRow(text, i);
				clockTimesGrid.Children.Add(text);
			}

			UpdateTimeFormat();
		}

		private void UpdateTimeFormat()
		{
			try
			{
				bool twelvehour = Settings.TimeFormat == TimeFormat.Standard;

				bool amTaken = false;
				bool pmTaken = false;

				Rect thisBounds = new Rect(0, 0, scrollViewer.ActualWidth, scrollViewer.ActualHeight);

				for (int i = 0; i < 24; i++)
				{
					TextControl text = clockTimesGrid.Children[i * 2 + 1] as TextControl;

					// Give a +1px vertical tolerance to handle for layout rounding.
					Rect textBounds = new Rect(text.TranslatePoint(new Point(0, 1), scrollViewer),
						text.TranslatePoint(new Point(text.ActualWidth, text.ActualHeight), scrollViewer));

					if (thisBounds.Contains(textBounds))
					{
						if (twelvehour)
						{
							if (i < 12)
								if (!amTaken)
								{
									amTaken = true;
									text.DisplayText = (i != 0 ? i : 12).ToString() + "AM";
								}
								else
									text.DisplayText = (i != 0 ? i : 12).ToString();
							else
								if (!pmTaken)
								{
									pmTaken = true;
									text.DisplayText = (i != 12 ? i - 12 : 12).ToString() + "PM";
								}
								else
									text.DisplayText = (i != 12 ? i - 12 : 12).ToString();
						}
						else
							text.DisplayText = i.ToString().PadLeft(2, '0');
					}

					// TODO: This is temporary - once layout is fixed to 22px increments
					//		this will no longer be necessary.
					else if (thisBounds.IntersectsWith(textBounds))
					{
						if (twelvehour)
						{
							if (i < 12)
								text.DisplayText = (i != 0 ? i : 12).ToString();
							else
								text.DisplayText = (i != 12 ? i - 12 : 12).ToString();
						}
						else
							text.DisplayText = i.ToString().PadLeft(2, '0');
					}
				}
			}
			catch
			{
				// VS Designer
			}
		}

		private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			UpdateTimeFormat();
		}
	}
}
