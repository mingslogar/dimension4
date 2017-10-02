using Daytimer.Dialogs;
using Daytimer.Functions;
using Daytimer.Fundamentals;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for CustomDictionaryEditor.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class CustomDictionaryEditor : OfficeWindow
	{
		public CustomDictionaryEditor()
		{
			InitializeComponent();
			Load();
		}

		#region UI

		private void addNewText_GotKeyboardFocus(object sender, RoutedEventArgs e)
		{
			addNewItem.IsSelected = true;
		}

		private void addNewItem_Selected(object sender, RoutedEventArgs e)
		{
			addNewText.Focus();
		}

		private void addNewItem_Unselected(object sender, RoutedEventArgs e)
		{
			if (addNewText.IsKeyboardFocused)
				Keyboard.ClearFocus();
		}

		private void listBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			listBox.SelectedIndex = -1;
		}

		private void addNewText_TextChanged(object sender, TextChangedEventArgs e)
		{
			addNewTextWatermark.Visibility = string.IsNullOrEmpty(addNewText.Text) ? Visibility.Visible : Visibility.Hidden;
		}

		private void addNewText_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				AddItem(addNewText.Text);
			}
		}

		private void searchText_TextChanged(object sender, TextChangedEventArgs e)
		{
			Search();
		}

		private void cancelSearchButton_Click(object sender, RoutedEventArgs e)
		{
			statusText.Visibility = Visibility.Collapsed;
			searchText.Clear();
		}

		private void addButton_Click(object sender, RoutedEventArgs e)
		{
			AddItem(searchText.Text);
		}

		private void deleteButton_Click(object sender, RoutedEventArgs e)
		{
			ListBoxItem item = (ListBoxItem)((FrameworkElement)sender).TemplatedParent;
			string text = (string)item.Content;

			SpellChecking.DeleteWordFromDictionary(text);

			if (Settings.AnimationsEnabled)
			{
				AnimationHelpers.DeleteAnimation delete = new AnimationHelpers.DeleteAnimation(item);
				delete.OnAnimationCompletedEvent += delete_OnAnimationCompletedEvent;
				delete.Animate();
			}
			else
				listBox.Items.Remove(item);
		}

		private void delete_OnAnimationCompletedEvent(object sender, EventArgs e)
		{
			listBox.Items.Remove(((AnimationHelpers.DeleteAnimation)sender).Control);
			Search();
		}

		#endregion

		#region Functions

		private void Load()
		{
			if (File.Exists(SpellChecking.CustomDictionaryLocation))
			{
				string[] entries = File.ReadAllLines(SpellChecking.CustomDictionaryLocation);

				foreach (string each in entries)
					AddItem(each, false);

				entries = null;
			}
		}

		/// <summary>
		/// Add a word to the dictionary. A message is shown if the word is
		/// not in the correct format.
		/// </summary>
		/// <param name="text"></param>
		private void AddItem(string text)
		{
			text = text.Trim();

			//
			// Check that the word is not blank
			//
			if (!string.IsNullOrEmpty(text))
			{
				//
				// Check that the word is a single word
				//
				if (text.Split(' ').Length == 1)
				{
					//
					// Check that the word is not already in the dictionary
					//
					string[] entries = new string[0];

					if (File.Exists(SpellChecking.CustomDictionaryLocation))
						entries = File.ReadAllLines(SpellChecking.CustomDictionaryLocation);

					if (Array.IndexOf(entries, text) == -1)
					{
						AddItem(text, true, true);
						SpellChecking.AddWordToDictionary(text);
						addNewText.Clear();
					}
					else
					{
						TaskDialog dlg = new TaskDialog(this, "Duplicate Entry", "The word you entered already exists in the dictionary.", MessageType.Error);
						dlg.ShowDialog();
					}

					entries = null;
				}
				else
				{
					TaskDialog dlg = new TaskDialog(this, "Invalid Entry", "Entry cannot be more than one word long.", MessageType.Error);
					dlg.ShowDialog();
				}
			}
		}

		/// <summary>
		/// Add a word to the list box. The item can optionally be animated.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="animate"></param>
		private void AddItem(string text, bool animate, bool refreshSearch = false)
		{
			ListBoxItem i = new ListBoxItem();
			i.Content = text;
			i.Style = listBox.FindResource("DictionaryEntry") as Style;

			if (animate)
			{
				if (Settings.AnimationsEnabled)
				{
					AnimationHelpers.LoadAnimation load = new AnimationHelpers.LoadAnimation(i);

					if (refreshSearch)
						load.OnAnimationCompletedEvent += load_OnAnimationCompletedEvent;

					load.Animate(26);	// Hard-coded value of height
				}
				else if (refreshSearch)
				{
					//
					// Refresh search results, if applicable
					//
					Search();
				}
			}

			listBox.Items.Insert(listBox.Items.Count - 1, i);
		}

		private void load_OnAnimationCompletedEvent(object sender, EventArgs e)
		{
			//
			// Refresh search results, if applicable
			//
			Search();
		}

		private void Search()
		{
			searchTextWatermark.Visibility = string.IsNullOrEmpty(searchText.Text) ? Visibility.Visible : Visibility.Hidden;

			string searchstring = searchText.Text.Trim().ToLower();

			if (!string.IsNullOrEmpty(searchstring))
			{
				cancelSearchButton.Visibility = Visibility.Visible;
				bool hasResults = false;

				foreach (ListBoxItem each in listBox.Items)
				{
					if (each != addNewItem)
						if (((string)each.Content).ToLower().Contains(searchstring))
						{
							each.Visibility = Visibility.Visible;
							hasResults = true;
						}
						else
							each.Visibility = Visibility.Collapsed;
				}

				if (!hasResults)
					statusText.Visibility = Visibility.Visible;
				else
					statusText.Visibility = Visibility.Collapsed;
			}
			else
			{
				cancelSearchButton.Visibility = Visibility.Hidden;

				foreach (ListBoxItem each in listBox.Items)
					each.Visibility = Visibility.Visible;

				statusText.Visibility = Visibility.Collapsed;
			}
		}

		#endregion
	}
}
