using Daytimer.Dialogs;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for EventRecurrenceSkip.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class EventRecurrenceSkip : DialogBase
	{
		public EventRecurrenceSkip(DateTime[] skip)
		{
			InitializeComponent();

			if (skip != null && skip.Length > 0)
			{
				statusText.Visibility = Visibility.Collapsed;

				foreach (object each in skip)
					listBox.Items.Add(each);
			}

			AccessKeyManager.Register(" ", okButton);
		}

		public DateTime[] Skip
		{
			get
			{
				int length = listBox.Items.Count;

				if (length > 0)
				{
					DateTime[] skip = new DateTime[length];

					for (int i = 0; i < length; i++)
						skip[i] = (DateTime)listBox.Items[i];

					return skip;
				}

				return null;
			}
		}

		private void deleteButton_Click(object sender, RoutedEventArgs e)
		{
			listBox.Items.Remove(listBox.ItemContainerGenerator.ItemFromContainer((sender as FrameworkElement).TemplatedParent));
			statusText.Visibility = listBox.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
