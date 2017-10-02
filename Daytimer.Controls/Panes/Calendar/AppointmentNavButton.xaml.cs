using Daytimer.Functions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for NextAppointmentButton.xaml
	/// </summary>
	public partial class AppointmentNavButton : Button
	{
		public AppointmentNavButton()
		{
			InitializeComponent();
		}

		private void Button_Loaded(object sender, RoutedEventArgs e)
		{
			FrameworkElement parent = (FrameworkElement)Parent;

			parent.SizeChanged += AppointmentNavButton_SizeChanged;
			UpdateSize(parent.RenderSize.Height);
		}

		private bool _isSmall = false;

		private void AppointmentNavButton_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateSize(e.NewSize.Height);
		}

		private void UpdateSize(double height)
		{
			if (height < 300)
			{
				if (!_isSmall)
				{
					_isSmall = true;

					DoubleAnimation textAnim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration);
					textAnim.EasingFunction = AnimationHelpers.EasingFunction;
					textDisplay.BeginAnimation(OpacityProperty, textAnim);

					img.VerticalAlignment = VerticalAlignment.Center;
					img.Margin = new Thickness(0);

					DoubleAnimation heightAnim = new DoubleAnimation(32, AnimationHelpers.AnimationDuration);
					heightAnim.EasingFunction = AnimationHelpers.EasingFunction;
					BeginAnimation(HeightProperty, heightAnim);
				}
			}
			else
			{
				if (_isSmall)
				{
					_isSmall = false;

					DoubleAnimation textAnim = new DoubleAnimation(1, AnimationHelpers.AnimationDuration);
					textAnim.EasingFunction = AnimationHelpers.EasingFunction;
					textDisplay.BeginAnimation(OpacityProperty, textAnim);

					img.VerticalAlignment = VerticalAlignment.Top;
					img.Margin = new Thickness(0, 11, 0, 0);

					DoubleAnimation heightAnim = new DoubleAnimation(178, AnimationHelpers.AnimationDuration);
					heightAnim.EasingFunction = AnimationHelpers.EasingFunction;
					BeginAnimation(HeightProperty, heightAnim);
				}
			}
		}

		private bool _isPrev;

		public bool IsPrev
		{
			get { return _isPrev; }
			set
			{
				_isPrev = value;
				Layout();
			}
		}

		public void Layout()
		{
			if (_isPrev)
			{
				textDisplay.Text = "Previous Appointment";
				img.RenderTransform = new ScaleTransform(-1, 1);
			}
			else
			{
				textDisplay.Text = "Next Appointment";
				img.RenderTransform = new ScaleTransform(1, 1);
			}
		}
	}
}
