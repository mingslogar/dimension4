using Daytimer.Fundamentals;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.DockableDialogs
{
	[ComVisible(false)]
	[TemplatePart(Name = UndockedWindow.CaptionName, Type = typeof(Border))]
	public class UndockedWindow : OfficeWindow
	{
		#region Constructors

		static UndockedWindow()
		{
			Type ownerType = typeof(UndockedWindow);

			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));

			CommandBinding close = new CommandBinding(ApplicationCommands.Close, ExecutedClose);
			CommandManager.RegisterClassCommandBinding(ownerType, close);
		}

		public UndockedWindow()
		{

		}

		public UndockedWindow(Point dragOffset)
		{
			DragStart = dragOffset;
			_captureMouse = true;
		}

		#endregion

		#region Fields

		private const string CaptionName = "PART_Caption";

		#endregion

		#region Internal Properties

		internal Border PART_Caption;

		#endregion

		#region Public Properties

		public static DependencyProperty DockedWidthProperty = DependencyProperty.Register(
			"DockedWidth", typeof(double?), typeof(UndockedWindow), new PropertyMetadata(null));

		public double DockedWidth
		{
			get { return GetValue(DockedWidthProperty) == null ? ActualWidth : (double)GetValue(DockedWidthProperty); }
			set { SetValue(DockedWidthProperty, value); }
		}

		#endregion

		#region Public Methods

		private bool _captureMouse = false;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			PART_Caption = GetTemplateChild(CaptionName) as Border;

			if (_captureMouse)
				Mouse.Capture(PART_Caption);

			PART_Caption.MouseLeftButtonDown += PART_Caption_MouseLeftButtonDown;
			PART_Caption.MouseMove += PART_Caption_MouseMove;
			PART_Caption.MouseLeftButtonUp += PART_Caption_MouseLeftButtonUp;
		}

		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);

			if (newContent != null)
				DockTarget.SetDockContainer(newContent as DependencyObject, this);
		}

		#endregion

		#region Private Methods

		private static void ExecutedClose(object sender, ExecutedRoutedEventArgs e)
		{
			(sender as UndockedWindow).Close();
		}

		private Point DragStart;

		private void PART_Caption_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Mouse.Capture(PART_Caption);
			DragStart = e.GetPosition(PART_Caption);
		}

		private void PART_Caption_MouseMove(object sender, MouseEventArgs e)
		{
			if (Mouse.Captured == PART_Caption)
			{
				Point MousePos = e.GetPosition(PART_Caption);

				Left += MousePos.X - DragStart.X;
				Top += MousePos.Y - DragStart.Y;

				foreach (DockTarget each in DockTarget.LiveDockTargets)
				{
					Point dockPos = Mouse.GetPosition(each);

					if ((each.DockLocation == DockLocation.Right && Math.Abs(dockPos.X - each.ActualWidth) < 2 && dockPos.Y >= 0 && dockPos.Y <= each.ActualHeight)
						|| (each.DockLocation == DockLocation.Left && Math.Abs(dockPos.X) < 2 && dockPos.Y >= 0 && dockPos.Y <= each.ActualHeight))
					{
						Mouse.Capture(null);

						DockedWindow docked = new DockedWindow(DragStart);
						docked.Width = DockedWidth;
						docked.UndockedWidth = ActualWidth;
						docked.UndockedHeight = ActualHeight;

						DockContent content = Content as DockContent;
						content.SuppressCloseEvent();
						Content = null;
						docked.Content = content;

						if (each.DockLocation == DockLocation.Right)
							each.Items.Add(docked);
						else
							each.Items.Insert(0, docked);

						Close();

						break;
					}
				}
			}
		}

		private void PART_Caption_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Mouse.Capture(null);
		}

		#endregion
	}
}
