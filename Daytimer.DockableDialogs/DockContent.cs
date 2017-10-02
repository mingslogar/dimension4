using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.DockableDialogs
{
	[ComVisible(false)]
	public class DockContent : ContentControl
	{
		public DockContent()
		{
			Loaded += DockContent_Loaded;
		}

		private void DockContent_Loaded(object sender, RoutedEventArgs e)
		{
			object container = DockTarget.GetDockContainer(this);

			if (container is Window)
			{
				Window window = container as Window;
				window.Closed -= DockContent_Closed;
				window.Closed += DockContent_Closed;
			}
			else if (container is DockedWindow)
			{
				DockedWindow docked = container as DockedWindow;
				docked.Closed -= DockContent_Closed;
				docked.Closed += DockContent_Closed;
			}
		}

		private void DockContent_Closed(object sender, EventArgs e)
		{
			RaiseClosedEvent();
		}

		public void SuppressCloseEvent()
		{
			object container = DockTarget.GetDockContainer(this);

			if (container is Window)
				(container as Window).Closed -= DockContent_Closed;
			else if (container is DockedWindow)
				(container as DockedWindow).Closed -= DockContent_Closed;
		}

		public void Close()
		{
			object dockContainer = DockTarget.GetDockContainer(this);

			if (dockContainer is UndockedWindow)
				((UndockedWindow)dockContainer).Close();
			else if (dockContainer is DockedWindow)
				((DockedWindow)dockContainer).Close();
		}

		public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
			"Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DockContent));

		public event RoutedEventHandler Closed
		{
			add { AddHandler(ClosedEvent, value); }
			remove { RemoveHandler(ClosedEvent, value); }
		}

		private void RaiseClosedEvent()
		{
			RaiseEvent(new RoutedEventArgs(ClosedEvent));
		}
	}
}
