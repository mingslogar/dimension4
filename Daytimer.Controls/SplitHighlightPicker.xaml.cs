using Daytimer.Functions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for SplitHighlightPicker.xaml
	/// </summary>
	public partial class SplitHighlightPicker : Grid
	{
		public SplitHighlightPicker()
		{
			//InitializeComponent();
			InitializationOptions.Initialize(this);
		}

		#region DependencyProperties

		public bool IsChecked
		{
			get { return (bool)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}

		public static readonly DependencyProperty IsCheckedProperty =
			DependencyProperty.Register("IsChecked", typeof(bool), typeof(SplitHighlightPicker),
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
			DependencyProperty.Register("Selected", typeof(object), typeof(SplitHighlightPicker),
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
			DependencyProperty.Register("ButtonImage", typeof(ImageSource), typeof(SplitHighlightPicker),
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
			DependencyProperty.Register("SelectedColor", typeof(Brush), typeof(SplitHighlightPicker),
			new UIPropertyMetadata(Brushes.Yellow, UpdateControlCallBack));

		/// <summary>
		/// Gets or sets the foreground color of the currently active text.
		/// </summary>
		public Brush ActiveColor
		{
			get { return (Brush)GetValue(ActiveColorProperty); }
			set { SetValue(ActiveColorProperty, value); }
		}

		public static readonly DependencyProperty ActiveColorProperty =
			DependencyProperty.Register("ActiveColor", typeof(Brush), typeof(SplitHighlightPicker),
			new UIPropertyMetadata(Brushes.Transparent, UpdateControlCallBack));

		private static void UpdateControlCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SplitHighlightPicker scp = (SplitHighlightPicker)d;

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
				SolidColorBrush b = (SolidColorBrush)e.NewValue;

				if (e.NewValue == null)
					scp.noColorButton.IsChecked = true;
				else
				{
					foreach (RadioButton each in scp.themeColorsBody.Children)
						if (((SolidColorBrush)each.Background).Color == b.Color)
						{
							each.IsChecked = true;
							return;
						}
				}
			}
		}

		private void MainWindow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!PART_Popup.IsMouseOver && !PART_ToggleButton.IsMouseOver)
				IsChecked = false;
		}

		#endregion

		#region SplitHighlightPicker Events

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
