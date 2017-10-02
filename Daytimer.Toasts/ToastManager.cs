using Daytimer.Functions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace Daytimer.Toasts
{
	public class ToastManager
	{
		public static List<Toast> OpenToasts = new List<Toast>();
		public static Queue<Toast> QueuedToasts = new Queue<Toast>();
		public static bool IsAudioRunning = false;

		public static void AddToOpen(Toast toast)
		{
			ShowToast(toast);
		}

		private static void toast_Closed(object sender, EventArgs e)
		{
			RemoveFromOpen(sender as Toast);
		}

		private static void RemoveFromOpen(Toast toast)
		{
			OpenToasts.Remove(toast);
			toast.Closed -= toast_Closed;

			if (QueuedToasts.Count > 0)
				ShowToast(QueuedToasts.Dequeue() as Toast);
		}

		private static void AddToQueue(Toast toast)
		{
			QueuedToasts.Enqueue(toast);
		}

		private static void ShowToast(Toast toast)
		{
			if (OpenToasts.Count >= 3)
			{
				AddToQueue(toast);
				return;
			}

			Rect workingArea = SystemParameters.WorkArea;

			double top = workingArea.Top + 20.0;

			if (Environment.OSVersion.Version >= OSVersions.Win_8)
			{
				//
				// Windows 8
				//
				// We don't want to be hidden by the system toast messages,
				// so we'll have to go somewhere other than the top right.
				//

				// We want to completely skip the top portion of the screen,
				// to avoid running into the system toasts.
				top = 320;	// 20 + 90 * 3 + 10 + 10 + 10
			}

			double _baseTop = top;

			//
			// BUG FIX: In the event that a toast closed at the exact instant that
			//			this foreach loop is running, an exception would be thrown.
			//
			while (true)
			{
				try
				{
					foreach (Toast each in OpenToasts)
					{
						each.Dispatcher.Invoke(() =>
						{
							if (each.Top == top)
								top = each.Top + each.ActualHeight + 10.0;
						});
					}

					break;
				}
				catch
				{
					top = _baseTop;
				}
			}

			toast.Dispatcher.Invoke(() =>
			{
				if (top + toast.Height > workingArea.Bottom)
				{
					// There isn't enough room on the screen to show this toast.
					AddToQueue(toast);
					return;
				}

				OpenToasts.Add(toast);
				toast.Top = top;
				toast.Left = workingArea.Right - toast.Width;

				toast.Closed += toast_Closed;
				toast.Show();
				Dispatcher.Run();
			});
		}
	}
}
