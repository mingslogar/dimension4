using Daytimer.Functions;
using Daytimer.Fundamentals;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Daytimer.Help
{
	/// <summary>
	/// Interaction logic for HelpViewer.xaml
	/// </summary>
	[ComVisible(false)]
	partial class HelpViewer : OfficeWindow
	{
		public HelpViewer()
		{
			InitializeComponent();

			Size size = Settings.HelpViewerSize;
			Width = size.Width;
			Height = size.Height;

			WindowState = Settings.HelpViewerIsMaximized ? WindowState.Maximized : WindowState.Normal;
			Topmost = Settings.HelpViewerTopmost;
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			if (Topmost)
				((RotateTransform)pinButton.Template.FindName("PathTransform", pinButton)).Angle = -90;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			Settings.HelpViewerSize = new Size(Width, Height);
			Settings.HelpViewerIsMaximized = WindowState == WindowState.Maximized;
			Settings.HelpViewerTopmost = Topmost;
		}

		private void pinButton_Click(object sender, RoutedEventArgs e)
		{
			Topmost = !Topmost;

			DoubleAnimation animation = new DoubleAnimation(Topmost ? -90 : 0, AnimationHelpers.AnimationDuration);
			animation.EasingFunction = AnimationHelpers.EasingFunction;
			animation.Completed += animation_Completed;
			((RotateTransform)pinButton.Template.FindName("PathTransform", pinButton)).BeginAnimation(RotateTransform.AngleProperty, animation);
			pinButton.ToolTip = Topmost ? "Don't keep help on top" : "Keep help on top";

			Path path = (Path)pinButton.Template.FindName("Path", pinButton);
			RenderOptions.SetEdgeMode(path, EdgeMode.Unspecified);
			path.Margin = new Thickness(-0.25, 0, 0, 0);
		}

		private void animation_Completed(object sender, EventArgs e)
		{
			Path path = (Path)pinButton.Template.FindName("Path", pinButton);
			RenderOptions.SetEdgeMode(path, EdgeMode.Aliased);
			path.Margin = new Thickness(0);
		}
	}
}
