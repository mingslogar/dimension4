using Microsoft.Windows.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Daytimer
{
	/// <summary>
	/// Interaction logic for StartupWindow.xaml
	/// </summary>
	public partial class StartupWindow : Window
	{
		public StartupWindow()
		{
			InitializeComponent();
			Loaded += StartupWindow_Loaded;
		}

		public void LoadComplete()
		{
			Dispatcher.BeginInvoke(() =>
			{
				_softClose = true;
				Close();
			});
		}

		public void UpdateStatus(string text)
		{
			UpdateText.Enqueue(text);

			if (!IsStatusAnimRunning)
			{
				IsStatusAnimRunning = true;
				Dispatcher.BeginInvoke(updateStatusWorker);
			}
		}

		private Queue<string> UpdateText = new Queue<string>();
		private bool IsStatusAnimRunning = false;

		private void updateStatusWorker()
		{
			if (UpdateText.Count > 0)
			{
				IsStatusAnimRunning = true;

				copyStatusText.Text = statusText.Text;
				statusText.Text = UpdateText.Dequeue();

				Duration animDuration = new Duration(new TimeSpan(0, 0, 0, 0, 400));

				ThicknessAnimation anim1 = new ThicknessAnimation(new Thickness(13, 0, 0, 0), new Thickness(13, 0, 0, 13), animDuration);
				DoubleAnimation opacityAnim1 = new DoubleAnimation(0, 1, animDuration);
				statusText.BeginAnimation(MarginProperty, anim1);
				statusText.BeginAnimation(OpacityProperty, opacityAnim1);

				ThicknessAnimation anim2 = new ThicknessAnimation(new Thickness(13, 0, 0, 13), new Thickness(13, 0, 0, 26), animDuration);
				DoubleAnimation opacityAnim2 = new DoubleAnimation(1, 0, animDuration);
				opacityAnim2.Completed += opacityAnim2_Completed;
				copyStatusText.BeginAnimation(MarginProperty, anim2);
				copyStatusText.BeginAnimation(OpacityProperty, opacityAnim2);
			}
		}

		private void opacityAnim2_Completed(object sender, EventArgs e)
		{
			IsStatusAnimRunning = false;
			updateStatusWorker();
		}

		private void StartupWindow_Loaded(object sender, RoutedEventArgs e)
		{
			if (!Activate())
				ShowInTaskbar = true;
		}

		private bool _softClose = false;

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!_softClose)
				//Environment.Exit(Environment.ExitCode);
				Application.Current.Shutdown(Environment.ExitCode);
			else
				Dispatcher.InvokeShutdown();
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			DragMove();
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void minimizeButton_Click(object sender, RoutedEventArgs e)
		{
			ShowInTaskbar = true;
			SystemCommands.MinimizeWindow(this);
		}

		protected override void OnStateChanged(EventArgs e)
		{
			base.OnStateChanged(e);

			if (WindowState == WindowState.Minimized)
			{
				Topmost = true;
				ShowInTaskbar = true;
			}
			else
				Topmost = false;
		}

		protected override void OnDeactivated(EventArgs e)
		{
			base.OnDeactivated(e);
			ShowInTaskbar = true;
		}

		//private void Spin_Completed(object sender, EventArgs e)
		//{
		//	icon.Visibility = Visibility.Visible;
		//	iconViewport.Visibility = Visibility.Collapsed;
		//}

		//<Viewport3D x:Name="iconViewport" HorizontalAlignment="Left" VerticalAlignment="Top" Margin="13,14" Width="16"
		//		Height="16" ClipToBounds="False" IsHitTestVisible="False" UseLayoutRounding="True" SnapsToDevicePixels="True">
		//	<Viewport3D.Camera>
		//		<PerspectiveCamera Position="0, 0, 2.5" />
		//	</Viewport3D.Camera>
		//	<!-- project the icon in 3D -->
		//	<Viewport2DVisual3D>
		//		<Viewport2DVisual3D.Transform>
		//			<RotateTransform3D>
		//				<RotateTransform3D.Rotation>
		//					<AxisAngleRotation3D x:Name="angle" Angle="-90" Axis="0, 1, 0" />
		//				</RotateTransform3D.Rotation>
		//			</RotateTransform3D>
		//		</Viewport2DVisual3D.Transform>
		//		<!-- The Geometry, Material, and Visual for the Viewport2DVisual3D -->
		//		<Viewport2DVisual3D.Geometry>
		//			<MeshGeometry3D Positions="-1,1,0 -1,-1,0 1,-1,0 1,1,0" TextureCoordinates="0,0 0,1 1,1 1,0"
		//					TriangleIndices="0 1 2 0 2 3" />
		//		</Viewport2DVisual3D.Geometry>
		//		<Viewport2DVisual3D.Material>
		//			<DiffuseMaterial Viewport2DVisual3D.IsVisualHostMaterial="True" />
		//		</Viewport2DVisual3D.Material>
		//		<Image Source="splashimg.png" RenderOptions.BitmapScalingMode="NearestNeighbor" />
		//	</Viewport2DVisual3D>
		//	<!-- Lights -->
		//	<ModelVisual3D>
		//		<ModelVisual3D.Content>
		//			<AmbientLight />
		//		</ModelVisual3D.Content>
		//	</ModelVisual3D>
		//	<Viewport3D.Triggers>
		//		<EventTrigger RoutedEvent="Loaded">
		//			<BeginStoryboard>
		//				<Storyboard>
		//					<DoubleAnimation Storyboard.TargetProperty="Angle" Storyboard.TargetName="angle" From="-90"
		//							To="0" Duration="0:0:0.7" BeginTime="0:0:0.4" Completed="Spin_Completed">
		//						<DoubleAnimation.EasingFunction>
		//							<QuarticEase />
		//						</DoubleAnimation.EasingFunction>
		//					</DoubleAnimation>
		//				</Storyboard>
		//			</BeginStoryboard>
		//		</EventTrigger>
		//	</Viewport3D.Triggers>
		//</Viewport3D>
		//<Image x:Name="icon" Source="splashimg.png" RenderOptions.BitmapScalingMode="NearestNeighbor"
		//		HorizontalAlignment="Left" VerticalAlignment="Top" Margin="13,14" Width="16" Height="16" Visibility="Hidden"
		//		IsHitTestVisible="False" />
	}
}