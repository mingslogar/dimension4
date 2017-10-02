using CharMap;
using Daytimer.Functions;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for SymbolPicker.xaml
	/// </summary>
	public partial class SymbolPicker : ComboBox
	{
		public SymbolPicker()
		{
			InitializationOptions.Initialize(this);
			DropDownOpened += SymbolPicker_DropDownOpened;
		}

		private void SymbolPicker_DropDownOpened(object sender, EventArgs e)
		{
			LoadSymbols();
		}

		private bool _isLoaded = false;

		private void LoadSymbols(bool force = false)
		{
			if (_isLoaded && !force)
				return;

			_isLoaded = true;

			Items.Clear();

			string symbols = Settings.RecentSymbols;

			int x = 0;
			int y = 0;

			foreach (char each in symbols)
			{
				ComboBoxItem item = new ComboBoxItem();
				item.Content = each;

				item.Margin = new Thickness(x, y, 0, 0);

				if (x == 0)
					y -= 30;

				if (x < 120)
					x += 30;
				else
				{
					x = 0;
					y = 0;
				}

				Items.Add(item);
			}
		}

		private char _selected;

		public char Selected
		{
			get { return _selected; }
		}

		private Window d = null;

		private void moreButton_Click(object sender, RoutedEventArgs e)
		{
			if (d == null)
			{
				CharMapDialog dlg = new CharMapDialog();
				dlg.Owner = Application.Current.MainWindow;
				dlg.Insert += d_Insert;
				dlg.Closed += d_Closed;
				dlg.Show();
				d = dlg;
			}
			else
				d.Activate();
		}

		private void d_Insert(object sender, RoutedEventArgs e)
		{
			CharMapDialog dlg = (CharMapDialog)d;
			_selected = dlg.Selected;

			string recent = Settings.RecentSymbols;

			if (!recent.Contains(_selected))
				Settings.RecentSymbols = _selected.ToString() + recent.Remove(recent.Length - 1);
			else
				Settings.RecentSymbols = _selected.ToString() + recent.Remove(recent.IndexOf(_selected), 1);

			LoadSymbols(true);
			RaiseInsertEvent();
		}

		private void d_Closed(object sender, EventArgs e)
		{
			CharMapDialog dlg = (CharMapDialog)d;
			dlg.Insert -= d_Insert;
			dlg.Closed -= d_Closed;
			d = null;

			Window.GetWindow(this).Activate();
		}

		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			base.OnSelectionChanged(e);

			if (e.AddedItems.Count > 0)
			{
				_selected = (char)((ContentControl)e.AddedItems[0]).Content;

				SelectedIndex = -1;

				string recent = Settings.RecentSymbols;
				Settings.RecentSymbols = _selected.ToString() + recent.Remove(recent.IndexOf(_selected), 1);

				LoadSymbols(true);
				RaiseInsertEvent();
			}
		}

		/// <summary>
		/// Closes the CharMapDialog, if applicable.
		/// </summary>
		public void Close()
		{
			if (d != null)
				d.Close();
		}

		private void ComboBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (d != null && !(bool)e.NewValue)
				d.Close();
		}

		#region RoutedEvents

		public static readonly RoutedEvent InsertEvent = EventManager.RegisterRoutedEvent(
			"Insert", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SymbolPicker));

		public event RoutedEventHandler Insert
		{
			add { AddHandler(InsertEvent, value); }
			remove { RemoveHandler(InsertEvent, value); }
		}

		private void RaiseInsertEvent()
		{
			RoutedEventArgs newEventArgs = new RoutedEventArgs(SymbolPicker.InsertEvent);
			RaiseEvent(newEventArgs);
		}

		#endregion
	}
}
