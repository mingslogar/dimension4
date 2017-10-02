using Daytimer.Controls.Panes;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Daytimer.Controls.WeekView
{
	/// <summary>
	/// Interaction logic for ClockGrid.xaml
	/// </summary>
	public partial class ClockGrid : Grid
	{
		public ClockGrid()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty ShowLinesProperty = DependencyProperty.Register(
			"ShowLines", typeof(bool), typeof(ClockGrid), new PropertyMetadata(true));

		public bool ShowLines
		{
			get { return (bool)GetValue(ShowLinesProperty); }
			set { SetValue(ShowLinesProperty, value); }
		}

		public void UpdateWorkHours(bool noWorkAllDay, TimeSpan startWork, TimeSpan endWork)
		{
			bgGrid.Children.Clear();

			if (noWorkAllDay)
			{
				Background = PanelBrushes.LightGrayBrushTransparent;
				return;
			}
			else
				Background = Brushes.Transparent;

			double hourHeight = ((FrameworkElement)Parent).Height / 24;

			if (startWork < endWork)
			{
				//
				// Start work
				//
				Rectangle rectStart = new Rectangle();
				rectStart.Fill = PanelBrushes.LightGrayBrushTransparent;
				rectStart.Height = startWork.Hours * hourHeight + startWork.Minutes * hourHeight / 60;
				rectStart.VerticalAlignment = VerticalAlignment.Top;
				rectStart.Margin = new Thickness(0, -1, 0, 0);
				bgGrid.Children.Add(rectStart);

				//
				// End work
				//
				Rectangle rectEnd = new Rectangle();
				rectEnd.Fill = PanelBrushes.LightGrayBrushTransparent;
				rectEnd.Height = (24 - endWork.Hours) * hourHeight - endWork.Minutes * hourHeight / 60;
				rectEnd.VerticalAlignment = VerticalAlignment.Bottom;
				bgGrid.Children.Add(rectEnd);
			}
			else
			{
				//
				// One rectangle for single no-work section
				//
				Rectangle rect = new Rectangle();
				rect.Fill = PanelBrushes.LightGrayBrushTransparent;
				rect.Height = (startWork.Hours - endWork.Hours) * hourHeight + (startWork.Minutes - endWork.Minutes) * hourHeight / 60;
				rect.Margin = new Thickness(0, endWork.Hours * hourHeight - endWork.Minutes * hourHeight / 60, 0, 0);
				rect.VerticalAlignment = VerticalAlignment.Top;
				bgGrid.Children.Add(rect);
			}
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			if (sizeInfo.HeightChanged)
			{
				if (!ShowLines)
					return;

				double newHeight = sizeInfo.NewSize.Height;

				int step;

				if (newHeight < 1056)
					step = 1;
				else if (newHeight < 2112)
					step = 2;
				else if (newHeight < 3168)
					step = 4;
				else if (newHeight < 6336)
					step = 6;
				else
					step = 12;

				int totalBars = 24 * step - 1;

				if (totalBars != clockGrid.Children.Count)
				{
					clockGrid.Children.Clear();
					clockGrid.RowDefinitions.Clear();

					Style hour = (Style)FindResource("HourLine");
					Style halfHour = (Style)FindResource("HalfHourLine");

					for (int i = 0; i < totalBars; i++)
					{
						clockGrid.RowDefinitions.Add(new RowDefinition());

						Line line = new Line();
						Grid.SetRow(line, i);

						if (i % step == step - 1)
							line.Style = hour;
						else
							line.Style = halfHour;

						clockGrid.Children.Add(line);
					}

					clockGrid.RowDefinitions.Add(new RowDefinition());
				}
			}
		}
	}
}
