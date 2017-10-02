using Daytimer.Functions;
using Microsoft.Samples.CustomControls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for SplitColorPicker.xaml
	/// </summary>
	public partial class SplitColorPicker : Grid
	{
		public SplitColorPicker()
		{
			//InitializeComponent();
			//Loaded += SplitColorPicker_Loaded;
			InitializationOptions.Initialize(this, SplitColorPicker_Loaded);
		}

		private bool _firstTime = true;

		private void SplitColorPicker_Loaded(object sender, RoutedEventArgs e)
		{
			if (!_firstTime)
				return;

			_firstTime = false;

			try { Window.GetWindow(this).MouseDown += SplitColorPicker_MouseDown; }
			catch { }
		}

		private void SplitColorPicker_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (!IsMouseOver)
				IsChecked = false;
		}

		#region DependencyProperties

		public bool IsChecked
		{
			get { return (bool)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}

		public static readonly DependencyProperty IsCheckedProperty =
			DependencyProperty.Register("IsChecked", typeof(bool), typeof(SplitColorPicker),
			new UIPropertyMetadata(false, UpdateControlCallBack));

		/// <summary>
		/// Gets or sets the currently selected item in the drop down.
		/// </summary>
		public object Selected
		{
			get { return GetValue(SelectedProperty); }
			set { SetValue(SelectedProperty, value); }
		}

		public static readonly DependencyProperty SelectedProperty =
			DependencyProperty.Register("Selected", typeof(object), typeof(SplitColorPicker),
			new UIPropertyMetadata(null, UpdateControlCallBack));

		/// <summary>
		/// Gets or sets the image shown on the button.
		/// </summary>
		public ImageSource ButtonImage
		{
			get { return (ImageSource)GetValue(ButtonImageProperty); }
			set { SetValue(ButtonImageProperty, value); }
		}

		public static readonly DependencyProperty ButtonImageProperty =
			DependencyProperty.Register("ButtonImage", typeof(ImageSource), typeof(SplitColorPicker),
			new UIPropertyMetadata(null, UpdateControlCallBack));

		/// <summary>
		/// Gets or sets the selected color.
		/// </summary>
		public Brush SelectedColor
		{
			get { return (Brush)GetValue(SelectedColorProperty); }
			set { SetValue(SelectedColorProperty, value); }
		}

		public static readonly DependencyProperty SelectedColorProperty =
			DependencyProperty.Register("SelectedColor", typeof(Brush), typeof(SplitColorPicker),
			new UIPropertyMetadata(Brushes.Red, UpdateControlCallBack));

		/// <summary>
		/// Gets or sets the foreground color of the currently active text.
		/// </summary>
		public Brush ActiveColor
		{
			get { return (Brush)GetValue(ActiveColorProperty); }
			set { SetValue(ActiveColorProperty, value); }
		}

		public static readonly DependencyProperty ActiveColorProperty =
			DependencyProperty.Register("ActiveColor", typeof(Brush), typeof(SplitColorPicker),
			new UIPropertyMetadata(Brushes.Black, UpdateControlCallBack));

		/// <summary>
		/// Gets or sets whether or not to show the "No Color" button.
		/// </summary>
		public bool ShowNoColor
		{
			get { return (bool)GetValue(ShowNoColorProperty); }
			set { SetValue(ShowNoColorProperty, value); }
		}

		public static readonly DependencyProperty ShowNoColorProperty =
			DependencyProperty.Register("ShowNoColor", typeof(bool), typeof(SplitColorPicker),
			new UIPropertyMetadata(false, UpdateControlCallBack));

		/// <summary>
		/// Gets or sets whether or not to show the "Automatic" button.
		/// </summary>
		public bool ShowAutomatic
		{
			get { return (bool)GetValue(ShowAutomaticProperty); }
			set { SetValue(ShowAutomaticProperty, value); }
		}

		public static readonly DependencyProperty ShowAutomaticProperty =
			DependencyProperty.Register("ShowAutomatic", typeof(bool), typeof(SplitColorPicker),
			new UIPropertyMetadata(true, UpdateControlCallBack));

		/// <summary>
		/// Gets or sets whether or not the left button is enabled.
		/// </summary>
		public bool IsButtonEnabled
		{
			get { return (bool)GetValue(IsButtonEnabledProperty); }
			set { SetValue(IsButtonEnabledProperty, value); }
		}

		public static readonly DependencyProperty IsButtonEnabledProperty =
			DependencyProperty.Register("IsButtonEnabled", typeof(bool), typeof(SplitColorPicker),
			new UIPropertyMetadata(true));

		private static void UpdateControlCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SplitColorPicker scp = (SplitColorPicker)d;

			if (e.Property == IsCheckedProperty)
			{
				scp.PART_Overlay.BorderBrush = scp.Background =
					(bool)e.NewValue ? (SolidColorBrush)scp.FindResource("RibbonChecked")
					: (scp.IsMouseOver ? (SolidColorBrush)scp.FindResource("RibbonMouseOver")
					: Brushes.Transparent);

				scp.PART_Button.IsEnabled = !(bool)e.NewValue;

				if ((bool)e.NewValue)
					Application.Current.MainWindow.PreviewMouseLeftButtonDown += scp.MainWindow_PreviewMouseLeftButtonDown;
				else
					Application.Current.MainWindow.PreviewMouseLeftButtonDown -= scp.MainWindow_PreviewMouseLeftButtonDown;
			}
			else if (e.Property == ActiveColorProperty)
			{
				if (e.NewValue == null)
				{
					if (scp.ShowNoColor)
						scp.noColorButton.IsChecked = true;

					return;
				}

				SolidColorBrush b = (SolidColorBrush)e.NewValue;

				if (scp.ShowAutomatic && b.Color == ((SolidColorBrush)scp.automaticButton.Background).Color)
					scp.automaticButton.IsChecked = true;
				else
				{
					foreach (RadioButton each in scp.themeColorsTop.Children)
						if (((SolidColorBrush)each.Background).Color == b.Color)
						{
							each.IsChecked = true;
							return;
						}

					foreach (RadioButton each in scp.themeColorsBody.Children)
						if (((SolidColorBrush)each.Background).Color == b.Color)
						{
							each.IsChecked = true;
							return;
						}

					foreach (RadioButton each in scp.recentColors.Children)
						if (((SolidColorBrush)each.Background).Color == b.Color)
						{
							each.IsChecked = true;
							return;
						}

					foreach (RadioButton each in scp.standardColors.Children)
						if (((SolidColorBrush)each.Background).Color == b.Color)
						{
							each.IsChecked = true;
							return;
						}
				}
			}
			else if (e.Property == ShowNoColorProperty)
			{
				scp.noColorButton.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
			}
			else if (e.Property == ShowAutomaticProperty)
			{
				scp.automaticButton.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		private void MainWindow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!PART_Popup.IsMouseOver && !PART_ToggleButton.IsMouseOver)
				IsChecked = false;
		}

		#endregion

		#region SplitColorPicker Events

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (!IsChecked)
			{
				PART_Overlay.BorderBrush = (SolidColorBrush)FindResource("RibbonMouseOver");
			}
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (!IsChecked)
			{
				PART_Overlay.BorderBrush = Background = Brushes.Transparent;
			}
		}

		private void userControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			Opacity = (bool)e.NewValue ? 1 : 0.5;
		}

		protected override void OnToolTipOpening(ToolTipEventArgs e)
		{
			base.OnToolTipOpening(e);

			// Prevent tooltip from opening if the drop down is open.
			if (IsChecked)
				e.Handled = true;
		}

		#endregion

		#region UI Events

		private void Color_Click(object sender, RoutedEventArgs e)
		{
			if (sender == noColorButton && ActiveColor != null)
			{
				SelectedColor = null;
				ActiveColor = SelectedColor;
				IsChecked = false;
				SelectedChangedEvent(e);
			}
			else if (ActiveColor == null ||
				((SolidColorBrush)ActiveColor).Color != ((SolidColorBrush)(sender as RadioButton).Background).Color)
			{
				SelectedColor = (sender as RadioButton).Background;
				ActiveColor = SelectedColor;
				IsChecked = false;
				SelectedChangedEvent(e);
			}
		}

		private void PART_Button_Click(object sender, RoutedEventArgs e)
		{
			SelectedChangedEvent(e);
		}

		private void moreColorsButton_Click(object sender, RoutedEventArgs e)
		{
			ColorPickerDialog dlg = new ColorPickerDialog();

			if (SelectedColor != null)
				dlg.SelectedColor = (SelectedColor as SolidColorBrush).Color;

			dlg.Owner = Window.GetWindow(this);

			if (dlg.ShowDialog() == true)
			{
				SolidColorBrush brush = new SolidColorBrush(dlg.SelectedColor);

				foreach (RadioButton each in recentColors.Children)
				{
					if ((each.Background as SolidColorBrush).Color == brush.Color)
					{
						each.IsChecked = true;

						int origCol = Grid.GetColumn(each);

						Grid.SetColumn(each, 0);

						foreach (UIElement u in recentColors.Children)
							if (u != each && Grid.GetColumn(u) < origCol)
								Grid.SetColumn(u, Grid.GetColumn(u) + 1);

						return;
					}
				}

				if (recentColors.Children.Count == 10)
					recentColors.Children.RemoveAt(0);

				foreach (UIElement each in recentColors.Children)
					Grid.SetColumn(each, Grid.GetColumn(each) + 1);

				RadioButton radio = new RadioButton();
				radio.Background = brush;
				recentColors.Children.Add(radio);
				radio.IsChecked = true;

				recentColors.Visibility = Visibility.Visible;

				if (ActiveColor == null || ((SolidColorBrush)ActiveColor).Color != brush.Color)
				{
					SelectedColor = brush;
					ActiveColor = SelectedColor;
					IsChecked = false;
					SelectedChangedEvent(e);
				}
			}
		}

		#endregion

		#region Events

		public delegate void OnSelectedChanged(object sender, EventArgs e);

		public event OnSelectedChanged OnSelectedChangedEvent;

		protected void SelectedChangedEvent(EventArgs e)
		{
			if (OnSelectedChangedEvent != null)
				OnSelectedChangedEvent(this, e);
		}

		#endregion
	}
}
