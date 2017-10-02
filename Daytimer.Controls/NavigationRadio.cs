using Daytimer.Controls.Panes;
using Daytimer.Controls.Panes.Tasks;
using Daytimer.Fundamentals;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Controls
{
	[ComVisible(false)]
	public abstract class NavigationRadio : RadioButton
	{
		#region Constructors

		static NavigationRadio()
		{
			Type ownerType = typeof(NavigationRadio);

			DockPeekCommand = new RoutedCommand("DockPeekCommand", ownerType);
			ShowPeekCommand = new RoutedCommand("ShowPeekCommand", ownerType);

			CommandManager.RegisterClassCommandBinding(ownerType,
				new CommandBinding(DockPeekCommand, ExecutedDockPeekCommand));
			CommandManager.RegisterClassCommandBinding(ownerType,
				new CommandBinding(ShowPeekCommand, ExecutedShowPeekCommand));
		}

		protected NavigationRadio(double PreviewPaneOffset, PositionOrder PreviewPanePositionOrder)
		{
			this.PreviewPaneOffset = PreviewPaneOffset;
			this.PreviewPanePositionOrder = PreviewPanePositionOrder;
		}

		#endregion

		#region Protected Methods

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (AssociatedControlType != null)
				InitializePreviewPane();
		}

		protected override void OnClick()
		{
			base.OnClick();

			if (PreviewPane != null)
				PreviewPane.FastClose();
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (PreviewPane != null)
				PreviewPane.Close();
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonDown(e);

			if (PreviewPane != null)// && !PreviewPane.IsOpen)
				PreviewPane.FastClose();
		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty IsContextMenuEnabledProperty = DependencyProperty.Register(
			"IsContextMenuEnabled", typeof(bool), typeof(NavigationRadio),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

		public bool IsContextMenuEnabled
		{
			get { return (bool)GetValue(IsContextMenuEnabledProperty); }
			set { SetValue(IsContextMenuEnabledProperty, value); }
		}

		#endregion

		#region Private Methods

		protected double PreviewPaneOffset;
		protected PositionOrder PreviewPanePositionOrder;

		private BalloonTip PreviewPane = null;
		private Type _associatedControlType = null;

		public Type AssociatedControlType
		{
			get { return _associatedControlType; }
			set
			{
				_associatedControlType = value;

				IsContextMenuEnabled = value != null;
			}
		}

		private object AssociatedControl = null;

		private static BalloonTip OpenPreviewPane = null;

		private void InitializePreviewPane()
		{
			if (PreviewPane == null || !PreviewPane.IsOpen)
			{
				if (OpenPreviewPane != null && OpenPreviewPane.IsOpen)
					OpenPreviewPane.FastClose();

				if (PreviewPane != null)
					PreviewPane.Content = null;

				PreviewPane = new BalloonTip(this);

				PreviewPane.Offset = PreviewPaneOffset;
				PreviewPane.Closed += PreviewPane_Closed;
				PreviewPane.PositionOrder = PreviewPanePositionOrder;
				PreviewPane.ContentWidth = 250;
				PreviewPane.ContentHeight = 330;
				PreviewPane.Owner = Window.GetWindow(this);

				if (AssociatedControl == null)
					AssociatedControl = AssociatedControlType.GetConstructor(new Type[0]).Invoke(null);

				PreviewPane.Content = AssociatedControl;
				PreviewPane.Show();

				OpenPreviewPane = PreviewPane;
			}
			else
			{
				PreviewPane.ResetTimer();
			}
		}

		private void PreviewPane_Closed(object sender, EventArgs e)
		{
			if (PreviewPane.Content is TasksPeek)
				((TasksPeekContent)((ContentControl)PreviewPane.Content).Content).CancelDrag();
		}

		private static void ExecutedDockPeekCommand(object sender, ExecutedRoutedEventArgs e)
		{
			((UndockedPeek)((NavigationRadio)sender).AssociatedControlType.GetConstructor(new Type[0]).Invoke(null)).Dock();
		}

		private static void ExecutedShowPeekCommand(object sender, ExecutedRoutedEventArgs e)
		{
			((NavigationRadio)sender).InitializePreviewPane();
		}

		#endregion

		#region Public Methods

		public void UpdateTheme()
		{
			// Re-apply template
			ControlTemplate template = Template;
			Template = null;
			Template = template;
		}

		public void ClosePreview()
		{
			if (PreviewPane != null)
				PreviewPane.FastClose();
		}

		#endregion

		#region Commands

		public static RoutedCommand DockPeekCommand;
		public static RoutedCommand ShowPeekCommand;

		#endregion
	}
}
