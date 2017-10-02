using Daytimer.Controls.Ribbon;
using Daytimer.Dialogs;
using Daytimer.Functions;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for NewItemDialog.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class NewItemDialog : DialogBase
	{
		public NewItemDialog()
		{
			InitializeComponent();
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private int _slideLocation = 0;

		public EditType EditType = EditType.Appointment;

		private bool _allowTransition = true;

		private void prevButton_Click(object sender, RoutedEventArgs e)
		{
			if (!_allowTransition)
				return;

			nextButton.IsEnabled = true;

			if (_slideLocation > 0)
			{
				_allowTransition = false;

				screenshot.Source = ImageProc.GetImage(slideContainer.Children[_slideLocation]);
				slideContainer.Children[_slideLocation].Visibility = Visibility.Collapsed;
				AnimationHelpers.SlideDisplay slide = new AnimationHelpers.SlideDisplay((Grid)slideContainer.Children[_slideLocation - 1], screenshot);
				slide.OnAnimationCompletedEvent += slide_OnAnimationCompletedEvent;
				slide.SwitchViews(AnimationHelpers.SlideDirection.Left);
				_slideLocation--;

				if (_slideLocation == 0)
					prevButton.IsEnabled = false;
			}
		}

		private void nextButton_Click(object sender, RoutedEventArgs e)
		{
			if (!_allowTransition)
				return;

			prevButton.IsEnabled = true;

			if (_slideLocation < slideContainer.Children.Count - 2)
			{
				_allowTransition = false;

				screenshot.Source = ImageProc.GetImage(slideContainer.Children[_slideLocation]);
				slideContainer.Children[_slideLocation].Visibility = Visibility.Collapsed;
				AnimationHelpers.SlideDisplay slide = new AnimationHelpers.SlideDisplay((Grid)slideContainer.Children[_slideLocation + 1], screenshot);
				slide.OnAnimationCompletedEvent += slide_OnAnimationCompletedEvent;
				slide.SwitchViews(AnimationHelpers.SlideDirection.Right);
				_slideLocation++;

				if (_slideLocation == slideContainer.Children.Count - 2)
					nextButton.IsEnabled = false;
			}
		}

		private void slide_OnAnimationCompletedEvent(object sender, EventArgs e)
		{
			_allowTransition = true;

			screenshot.Source = null;
			createButton.Image = new BitmapImage(new Uri((slideContainer.Children[_slideLocation] as Grid).Tag.ToString(), UriKind.Absolute));
		}

		private void createButton_Click(object sender, RoutedEventArgs e)
		{
			EditType = (EditType)_slideLocation;
			DialogResult = true;
		}
	}
}
