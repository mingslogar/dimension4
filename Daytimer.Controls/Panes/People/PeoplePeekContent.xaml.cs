using Daytimer.Controls.Friction;
using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Contacts;
using Daytimer.Functions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Daytimer.Controls.Panes.People
{
	/// <summary>
	/// Interaction logic for PeoplePeekContent.xaml
	/// </summary>
	public partial class PeoplePeekContent : Peek
	{
		#region Constructors

		static PeoplePeekContent()
		{
			Type ownerType = typeof(PeoplePeekContent);
			OpenContactCommand = new RoutedCommand("OpenContactCommand", ownerType);
		}

		public PeoplePeekContent()
		{
			InitializeComponent();
			Loaded += PeoplePeekContent_Loaded;
			Unloaded += PeoplePeekContent_Unloaded;
		}

		#endregion

		#region Public Methods

		public override void Load()
		{
			string[] favorites = Settings.PeopleFavorites;

			contactsListBox.Items.Clear();

			if (favorites == null || favorites.Length == 0)
			{
				message.Visibility = Visibility.Visible;
				return;
			}

			contactsListBox.Visibility = Visibility.Visible;
			message.Visibility = Visibility.Hidden;

			foreach (string each in favorites)
			{
				Contact c = ContactDatabase.GetContact(each);
				if (c != null)
					PeopleView.SmartInsert(contactsListBox, c);
			}

			//
			// BUG FIX: Since a contact is not placed in database until it is saved,
			// a contact marked as "Favorite" would be inaccessible.
			//
			if (!contactsListBox.HasItems)
			{
				contactsListBox.Visibility = Visibility.Hidden;
				message.Visibility = Visibility.Visible;
				FrictionScrollViewerControl scrollViewer = (FrictionScrollViewerControl)contactsListBox.Template.FindName("_tv_scrollviewer_", contactsListBox);
				scrollViewer.LayoutVerticalScrollBar();
				scrollViewer.LayoutHorizontalScrollBar();
			}
		}

		public void Update(Contact contact)
		{
			// Ignore this call if a search is currently running.
			if (!string.IsNullOrEmpty(searchBox.Text))
				return;

			Delete(contact.ID);

			string[] favorites = Settings.PeopleFavorites;

			if (favorites != null && Array.IndexOf(favorites, contact.ID) != -1)
				Add(contact);
		}

		public void Delete(string id)
		{
			// Ignore this call if a search is currently running.
			if (!string.IsNullOrEmpty(searchBox.Text))
				return;

			foreach (Contact each in contactsListBox.Items)
				if (each.ID == id)
				{
					contactsListBox.Items.Remove(each);
					break;
				}

			if (contactsListBox.Items.Count == 0)
			{
				message.Visibility = Visibility.Visible;
				contactsListBox.Visibility = Visibility.Hidden;
			}
		}

		public void Add(Contact contact)
		{
			// Ignore this call if a search is currently running.
			if (!string.IsNullOrEmpty(searchBox.Text))
				return;

			message.Visibility = Visibility.Hidden;
			contactsListBox.Visibility = Visibility.Visible;
			PeopleView.SmartInsert(contactsListBox, contact);
		}

		/// <summary>
		/// Update a contact in all loaded people peeks.
		/// </summary>
		/// <param name="contact">The contact to refresh.</param>
		public static void UpdateAll(Contact contact)
		{
			foreach (PeoplePeekContent each in LoadedPeoplePeekContents)
				each.Update(contact);
		}

		#endregion

		#region Private Methods

		private void PeoplePeekContent_Loaded(object sender, RoutedEventArgs e)
		{
			LoadedPeoplePeekContents.Add(this);
		}

		private void PeoplePeekContent_Unloaded(object sender, RoutedEventArgs e)
		{
			searchBox.Clear();
			contactsListBox.Items.Clear();
			contactsListBox.Visibility = Visibility.Hidden;
			message.Visibility = Visibility.Visible;
			_cancel = true;
			statusBar.Text = "FAVORITES";
			message.Text = "Right-click a person in the People pane to add them to your favorites.";
			LoadedPeoplePeekContents.Remove(this);
		}

		private void contactsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
				OpenContactCommand.Execute(contactsListBox.SelectedItem, this);
		}

		#endregion

		#region Public Properties

		public override string Source
		{
			get { return "/Daytimer.Controls;component/Panes/People/PeoplePeekContent.xaml"; }
		}

		public static List<PeoplePeekContent> LoadedPeoplePeekContents = new List<PeoplePeekContent>();

		#endregion

		#region Commands

		public static RoutedCommand OpenContactCommand;

		#endregion

		#region Search

		private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!IsLoaded)
				return;

			if (!string.IsNullOrEmpty(searchBox.Text))
				Search(searchBox.Text);
			else
				ClearSearch();
		}

		private void Search(string query)
		{
			_cancel = true;

			if (_searchThread != null && _searchThread.IsAlive)
				try { _searchThread.Join(); }
				catch { }

			_cancel = false;

			statusBar.Text = "Searching...";
			message.Text = "";
			message.Visibility = Visibility.Visible;

			_searchThread = new Thread(search);
			_searchThread.IsBackground = true;
			_searchThread.Start(searchBox.Text);
		}

		private void ClearSearch()
		{
			_cancel = true;

			if (_searchThread != null && _searchThread.IsAlive)
				try { _searchThread.Join(); }
				catch { }

			statusBar.Text = "FAVORITES";
			message.Text = "Right-click a person in the People pane to add them to your favorites.";
			contactsListBox.Visibility = Visibility.Hidden;
			Load();
		}

		private bool _cancel = false;
		private Thread _searchThread = null;

		private void search(object _query)
		{
			string[] query =
				Daytimer.Search.Search.StripWhitespace
				(
					Daytimer.Search.Search.RemovePunctuation
					(
						(
							(string)_query
						).ToLower()
					)
				)
				.Trim()
				.Split(' ');

			List<Contact> results = new List<Contact>();

			if (query.Length > 0 && !string.IsNullOrEmpty(query[0]))
			{
				foreach (Contact each in ContactDatabase.GetContacts())
				{
					if (contactMatchesQuery(query, each))
						results.Add(each);
				}
			}

			if (_cancel)
				return;

			if (results.Count > 0)
				Dispatcher.Invoke(() =>
				{
					if (_cancel)
						return;

					statusBar.Text = "RESULTS (" + results.Count.ToString() + ")";
					message.Text = "";
					contactsListBox.Visibility = Visibility.Visible;

					contactsListBox.Items.Clear();

					foreach (Contact each in results)
						contactsListBox.Items.Add(each);
				});
			else
				Dispatcher.Invoke(() =>
				{
					if (_cancel)
						return;

					statusBar.Text = "NO RESULTS";
					message.Text = "We couldn't find the person you were looking for. You can try using the more powerful main search (Ctrl+F).";
					contactsListBox.Items.Clear();
					contactsListBox.Visibility = Visibility.Hidden;
				});
		}

		private bool contactMatchesQuery(string[] query, Contact contact)
		{
			string name = contact.Name.ToString().ToLower();
			string work = contact.WorkDescription.ToLower();
			Email[] email = contact.Emails;
			Website[] website = contact.Websites;
			PhoneNumber[] phone = contact.PhoneNumbers;
			Address[] address = contact.Addresses;

			foreach (string q in query)
			{
				if (name.Contains(q))
					continue;

				if (work.Contains(q))
					continue;

				bool isValid = false;

				if (email != null)
				{
					foreach (Email em in email)
					{
						if (em.Address.ToLower().Contains(q))
						{
							isValid = true;
							break;
						}
					}

					if (isValid)
						continue;
				}

				if (website != null)
				{
					foreach (Website ws in website)
					{
						if (ws.Url.ToLower().Contains(q))
						{
							isValid = true;
							break;
						}
					}

					if (isValid)
						continue;
				}

				if (phone != null)
				{
					foreach (PhoneNumber pn in phone)
					{
						if (pn.Number.ToLower().Contains(q))
						{
							isValid = true;
							break;
						}
					}

					if (isValid)
						continue;
				}

				if (address != null)
				{
					foreach (Address ad in address)
					{
						if (ad.ToString().ToLower().Contains(q))
						{
							isValid = true;
							break;
						}
					}

					if (isValid)
						continue;
				}

				return false;
			}

			return true;
		}

		#endregion
	}
}
