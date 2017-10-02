using Daytimer.Functions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for SplitBorderSelector.xaml
	/// </summary>
	public partial class SplitBorderSelector : Grid
	{
		public SplitBorderSelector()
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
			DependencyProperty.Register("IsChecked", typeof(bool), typeof(SplitBorderSelector),
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
			DependencyProperty.Register("Selected", typeof(object), typeof(SplitBorderSelector),
			new UIPropertyMetadata(null, UpdateControlCallBack));

		/// <summary>
		/// Gets or sets the selected border.
		/// </summary>
		public Thickness SelectedBorder
		{
			get { return (Thickness)GetValue(SelectedBorderProperty); }
			set { SetValue(SelectedBorderProperty, value); }
		}

		public static readonly DependencyProperty SelectedBorderProperty =
			DependencyProperty.Register("SelectedBorder", typeof(Thickness), typeof(SplitBorderSelector),
			new UIPropertyMetadata(new Thickness(0, 0, 0, 1), UpdateControlCallBack));

		/// <summary>
		/// Gets or sets the border of the currently active paragraph.
		/// </summary>
		public Thickness ActiveBorder
		{
			get { return (Thickness)GetValue(ActiveBorderProperty); }
			set { SetValue(ActiveBorderProperty, value); }
		}

		public static readonly DependencyProperty ActiveBorderProperty =
			DependencyProperty.Register("ActiveBorder", typeof(Thickness), typeof(SplitBorderSelector),
			new UIPropertyMetadata(new Thickness(0), UpdateControlCallBack));

		private static void UpdateControlCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SplitBorderSelector scp = (SplitBorderSelector)d;

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
			else if (e.Property == ActiveBorderProperty)
			{
				Thickness b = (Thickness)e.NewValue;

				if (b == (Thickness)e.OldValue)
					return;

				foreach (UIElement each in scp.body.Children)
					if (each is RadioButton && (each as RadioButton).BorderThickness == b)
					{
						(each as RadioButton).IsChecked = true;
						return;
					}
			}
		}

		private void MainWindow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!PART_Popup.IsMouseOver && !PART_ToggleButton.IsMouseOver)
				IsChecked = false;
		}

		#endregion

		#region SplitBorderSelector Events

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

		private void Margin_Click(object sender, RoutedEventArgs e)
		{
			if (ActiveBorder != (sender as RadioButton).BorderThickness)
			{
				SelectedBorder = (sender as RadioButton).BorderThickness;
				ActiveBorder = SelectedBorder;
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
