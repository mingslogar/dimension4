using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for SplitButton.xaml
	/// </summary>
	public partial class SplitButton : UserControl
	{
		public SplitButton()
		{
			InitializeComponent();
		}

		#region DependencyProperties

		public bool IsChecked
		{
			get { return (bool)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}

		public static readonly DependencyProperty IsCheckedProperty =
			DependencyProperty.Register("IsChecked", typeof(bool), typeof(SplitButton),
			new UIPropertyMetadata(false, UpdateControlCallBack));

		/// <summary>
		/// Gets or sets the main content of the SplitButton.
		/// </summary>
		public UIElement Body
		{
			get { return (UIElement)GetValue(BodyProperty); }
			set { SetValue(BodyProperty, value); }
		}

		public static readonly DependencyProperty BodyProperty =
			DependencyProperty.Register("Body", typeof(UIElement), typeof(SplitButton),
			new UIPropertyMetadata(null, UpdateControlCallBack));

		/// <summary>
		/// Gets or sets the currently selected item in the drop down.
		/// </summary>
		public object Selected
		{
			get { return GetValue(SelectedProperty); }
			set { SetValue(SelectedProperty, value); }
		}

		public static readonly DependencyProperty SelectedProperty =
			DependencyProperty.Register("Selected", typeof(object), typeof(SplitButton),
			new UIPropertyMetadata(null, UpdateControlCallBack));

		private static void UpdateControlCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SplitButton scp = (SplitButton)d;

			if (e.Property == IsCheckedProperty)
			{
				scp.PART_Overlay.BorderBrush = (bool)e.NewValue ? (SolidColorBrush)scp.FindResource("RibbonChecked")
					: (scp.IsMouseOver ? (SolidColorBrush)scp.FindResource("RibbonMouseOver")
					: Brushes.Transparent);
			}
			else if (e.Property == BodyProperty)
			{
				if (e.NewValue != null)
					scp.ItemsPresenter.Child = (UIElement)e.NewValue;
				else
					scp.ItemsPresenter.Child = null;
			}
		}

		#endregion

		#region SplitButton Events

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
				PART_Overlay.BorderBrush = Brushes.Transparent;
			}
		}

		#endregion
	}
}
