using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Contacts;
using Daytimer.Dialogs;
using Daytimer.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Daytimer.Controls.Panes.People
{
	/// <summary>
	/// Interaction logic for PeopleView.xaml
	/// </summary>
	public partial class PeopleView : Grid
	{
		public PeopleView()
		{
			InitializeComponent();

			SpellChecking.HandleSpellChecking(notesRTB);

			ColumnDefinitions[0].Width = Settings.PeopleColumn0Width;
			ColumnDefinitions[1].Width = Settings.PeopleColumn1Width;

			_currentView = Settings.PeopleShowFavorites ? ViewMode.Favorites : ViewMode.All;
			favoriteContactsRadio.IsChecked = _currentView == ViewMode.Favorites;

			Loaded += PeopleView_Loaded;

			notesRTB.Document.Blocks.Clear();
		}

		#region Initializers

		/// <summary>
		/// If true, content has already been loaded.
		/// </summary>
		public bool HasLoaded = false;

		private Contact _activeContact = null;

		public Contact ActiveContact
		{
			get { return _activeContact; }
		}

		public RichTextBox NotesText
		{
			get { return notesRTB; }
		}

		public bool InEditMode
		{
			get { return _inEditMode; }
		}

		public Contact LiveContact
		{
			get
			{
				if (!_inEditMode)
				{
					return (Contact)contactsListBox.SelectedItem;
				}

				Contact contact = new Contact(false, false);

				Name newName = DatabaseHelpers.Contacts.Name.TryParse(contactNameEdit.Detail);

				if (newName == null)
					newName = new Name();

				contact.Name = newName;

				contact.Tile = (BitmapSource)contactTileEdit.Source;

				Email[] emailArray = new Email[0];
				PhoneNumber[] phoneArray = new PhoneNumber[0];
				Work work = new Work();
				Address[] addressArray = new Address[0];
				Website[] websiteArray = new Website[0];
				IM[] imArray = new IM[0];
				SpecialDate[] specialDateArray = new SpecialDate[0];

				Panel panel1 = (Panel)footerGridEdit.Children[0];

				foreach (UIElement each in panel1.Children)
				{
					if (each is ContactDetailBlock)
					{
						ContactDetailBlock block = (ContactDetailBlock)each;

						if (!string.IsNullOrWhiteSpace(block.Detail))
						{
							switch (block.Tag.ToString())
							{
								case "Email":
									if (new RegexUtilities().IsValidEmail(block.Detail))
									{
										Array.Resize(ref emailArray, emailArray.Length + 1);
										emailArray[emailArray.Length - 1] = new Email(block.Title, block.Detail);
									}
									break;

								case "Phone":
									Array.Resize(ref phoneArray, phoneArray.Length + 1);
									phoneArray[phoneArray.Length - 1] = new PhoneNumber(block.Title, block.Detail);
									break;

								case "IM":
									Array.Resize(ref imArray, imArray.Length + 1);
									imArray[imArray.Length - 1] = new IM(block.Title, block.Detail);
									break;

								default:
									// Should never get here
									break;
							}
						}
					}
				}

				Panel panel2 = (Panel)footerGridEdit.Children[1];

				foreach (UIElement each in panel2.Children)
				{
					if (each is ContactDetailBlock)
					{
						ContactDetailBlock block = (ContactDetailBlock)each;

						if (!string.IsNullOrWhiteSpace(block.Detail))
						{
							switch (block.Tag.ToString())
							{
								case "Work":
									switch (block.Title)
									{
										case "Title":
											work.Title = block.Detail;
											break;

										case "Department":
											work.Department = block.Detail;
											break;

										case "Company":
											work.Company = block.Detail;
											break;

										case "Office":
											work.Office = block.Detail;
											break;

										default:
											// Should never get here
											break;
									}
									break;

								case "Address":
									Address a = Address.TryParse(block.Detail);

									if (a != null)
									{
										Array.Resize(ref addressArray, addressArray.Length + 1);
										a.Type = block.Title;
										addressArray[addressArray.Length - 1] = a;
									}
									break;

								case "Website":
									try
									{
										new Uri(block.Detail);
										Array.Resize(ref websiteArray, websiteArray.Length + 1);
										websiteArray[websiteArray.Length - 1] = new Website(block.Title, block.Detail);
									}
									catch
									{
										try
										{
											new Uri("http://" + block.Detail);
											Array.Resize(ref websiteArray, websiteArray.Length + 1);
											websiteArray[websiteArray.Length - 1] = new Website(block.Title, "http://" + block.Detail);
										}
										catch { }
									}
									break;

								case "Special Date":
									DateTime date;
									if (DateTime.TryParse(block.Detail, out date))
									{
										Array.Resize(ref specialDateArray, specialDateArray.Length + 1);
										specialDateArray[specialDateArray.Length - 1] = new SpecialDate(block.Title, date);
									}
									break;

								default:
									// Should never get here
									break;
							}
						}
					}
				}

				contact.IM = imArray;
				contact.Emails = emailArray;
				contact.PhoneNumbers = phoneArray;
				contact.Work = work;
				contact.Addresses = addressArray;
				contact.Websites = websiteArray;
				contact.SpecialDates = specialDateArray;

				contact.NotesDocument = notesRTB.Document;

				return contact;
			}
		}

		public enum ViewMode : byte { All, Favorites };

		private ViewMode _currentView = ViewMode.All;

		public ViewMode CurrentView
		{
			get { return _currentView; }
		}

		#endregion

		#region Functions

		public void SaveLayout()
		{
			Settings.PeopleColumn0Width = ColumnDefinitions[0].Width;
			Settings.PeopleColumn1Width = ColumnDefinitions[1].Width;
			Settings.PeopleShowFavorites = _currentView == ViewMode.Favorites;
		}

		public void ToggleShowFavorites(bool animate = true)
		{
			//Flip();

			_currentView = _currentView == ViewMode.All ? ViewMode.Favorites : ViewMode.All;

			if (_currentView == ViewMode.Favorites)
			{
				favoriteContactsRadio.IsChecked = true;
				//Settings.PeopleShowFavoritesOnly = true;

				//
				// Load favorites
				//
				string[] favorites = Settings.PeopleFavorites;

				if (favorites != null)
				{
					bool anyLeft = false;

					foreach (Contact each in contactsListBox.Items)
					{
						if (Array.IndexOf(favorites, each.ID) == -1)
						{
							ListBoxItem item = (ListBoxItem)contactsListBox.ItemContainerGenerator.ContainerFromItem(each);

							if (item != null)
							{
								if (animate && Settings.AnimationsEnabled)
									(new AnimationHelpers.DeleteAnimation(item)).Animate();
								else
								{
									item.IsHitTestVisible = false;
									item.IsEnabled = false;
									item.Opacity = 0;
									item.Height = 0;
								}
							}
						}
						else
							anyLeft = true;
					}

					if (!anyLeft)
					{
						statusText.Text = "We didn't find anything to show here.";

						if (animate && Settings.AnimationsEnabled)
							new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
						else
						{
							statusText.Visibility = Visibility.Visible;
							statusText.Opacity = 1;
						}
					}
				}
				else
				{
					foreach (Contact each in contactsListBox.Items)
					{
						ListBoxItem item = (ListBoxItem)contactsListBox.ItemContainerGenerator.ContainerFromItem(each);

						if (item != null)
						{
							if (animate && Settings.AnimationsEnabled)
								(new AnimationHelpers.DeleteAnimation(item)).Animate();
							else
							{
								item.IsHitTestVisible = false;
								item.IsEnabled = false;
								item.Opacity = 0;
								item.Height = 0;
							}
						}
					}

					statusText.Text = "We didn't find anything to show here.";

					if (animate && Settings.AnimationsEnabled)
						new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
					else
					{
						statusText.Visibility = Visibility.Visible;
						statusText.Opacity = 1;
					}
				}
			}
			else
			{
				allContactsRadio.IsChecked = true;
				//Settings.PeopleShowFavoritesOnly = false;

				//
				// Show all
				//
				if (contactsListBox.HasItems)
				{
					foreach (Contact each in contactsListBox.Items)
					{
						ListBoxItem item = (ListBoxItem)contactsListBox.ItemContainerGenerator.ContainerFromItem(each);

						if (item != null && item.Height == 0)
						{
							if (animate && Settings.AnimationsEnabled)
								(new AnimationHelpers.LoadAnimation(item)).Animate(50);
							else
							{
								item.IsHitTestVisible = true;
								item.IsEnabled = true;
								item.Opacity = 1;
								item.Height = 50;
							}
						}
					}

					if (animate && Settings.AnimationsEnabled)
						new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.Out, false, 0, 1, true);
					else
					{
						statusText.Visibility = Visibility.Hidden;
						statusText.Opacity = 0;
					}
				}
				else
				{
					statusText.Text = "We didn't find anything to show here.";

					if (animate && Settings.AnimationsEnabled)
						new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
					else
					{
						statusText.Visibility = Visibility.Visible;
						statusText.Opacity = 1;
					}
				}
			}

			ChangeViewEvent(EventArgs.Empty);
		}

		/// <summary>
		/// Load all contacts.
		/// </summary>
		public async Task Load()
		{
			statusText.Text = "Loading...";
			statusText.Visibility = Visibility.Visible;
			statusText.Opacity = 1;

			HasLoaded = true;

			contactsListBox.Items.Clear();

			Contact[] contacts = await Task.Factory.StartNew<Contact[]>(ContactDatabase.GetContacts().ToArray<Contact>);
			contactsListBox.Items.Clear();
			additems(contacts, _currentView == ViewMode.All);

			if (_currentView == ViewMode.Favorites)
			{
				contactsListBox.UpdateLayout();

				_currentView = ViewMode.All;
				ToggleShowFavorites(false);
			}
		}

		private void additems(Contact[] contacts, bool animate)
		{
			if (contacts != null)
			{
				foreach (Contact each in contacts)
					contactsListBox.Items.Add(each);

				if (!contactsListBox.HasItems)
				{
					statusText.Text = "We didn't find anything to show here.";

					if (animate && Settings.AnimationsEnabled)
						new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
					else
					{
						statusText.Opacity = 1;
						statusText.Visibility = Visibility.Visible;
					}
				}
				else
				{
					if (animate && Settings.AnimationsEnabled)
						new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.Out, true);
					else
						statusText.Visibility = Visibility.Hidden;

					if (_scrollToContactId != null)
						ScrollToContact(_scrollToContactId);
				}
			}
			else
			{
				statusText.Text = "We didn't find anything to show here.";

				if (animate && Settings.AnimationsEnabled)
					new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
				else
				{
					statusText.Opacity = 1;
					statusText.Visibility = Visibility.Visible;
				}
			}

			if (_hasQueue)
			{
				AddContact(true, _queued);
			}
			//else
			//{
			//	if (Settings.PeopleShowFavoritesOnly)
			//		ToggleShowFavorites();
			//}
		}

		/// <summary>
		/// Save the active contact.
		/// </summary>
		/// <param name="enableInteraction">True if this function should show dialogs. False otherwise.</param>
		/// <returns></returns>
		public async Task Save(bool enableInteraction)
		{
			if (_activeContact == null || !_inEditMode)
				return;

			Contact contact = _activeContact;

			bool rePositionContact = false;

			if (contactNameEdit.Detail != ((Name)contactNameEdit.Tag).ToString())
			{
				Name newName = DatabaseHelpers.Contacts.Name.TryParse(contactNameEdit.Detail);

				if (newName == null)
				{
					if (enableInteraction)
					{
						ClarifyName cn = new ClarifyName();
						cn.Owner = Window.GetWindow(this);

						if (cn.ShowDialog() == true)
						{
							newName = cn.Name;

							if (CheckNameDuplicate(newName))
								return;
						}
						else
							newName = new Name();
					}
					else
						newName = new Name();
				}
				else if (enableInteraction)
					if (CheckNameDuplicate(newName))
						return;

				if (contact.Name == null
					|| newName.ToSerializedString() != contact.RawName)
				{
					contact.Name = newName;
					rePositionContact = true;
				}
			}
			else
				contact.Name = (Name)contactNameEdit.Tag;

			if (_currentView == ViewMode.Favorites && Array.IndexOf(Settings.PeopleFavorites, contact.ID) == -1)
				ToggleShowFavorites();

			contact.Tile = (BitmapSource)contactTileEdit.Source;
			contact.RawSpecialDate = "";

			Email[] emailArray = new Email[0];
			PhoneNumber[] phoneArray = new PhoneNumber[0];
			Work work = new Work();
			Address[] addressArray = new Address[0];
			Website[] websiteArray = new Website[0];
			IM[] imArray = new IM[0];
			SpecialDate[] specialDateArray = new SpecialDate[0];

			foreach (Appointment each in GetContactSpecialDates(ContactDatabase.GetContact(contact.ID)))
			{
				AppointmentDatabase.Delete(each);
				DateModifiedEvent(new DateModifiedEventArgs(each.StartDate, null));
			}

			Panel panel1 = (Panel)footerGridEdit.Children[0];

			foreach (UIElement each in panel1.Children)
			{
				if (each is ContactDetailBlock)
				{
					ContactDetailBlock block = (ContactDetailBlock)each;

					if (!string.IsNullOrWhiteSpace(block.Detail))
					{
						switch (block.Tag.ToString())
						{
							case "Email":
								if (new RegexUtilities().IsValidEmail(block.Detail))
								{
									Array.Resize(ref emailArray, emailArray.Length + 1);
									emailArray[emailArray.Length - 1] = new Email(block.Title, block.Detail);
								}
								break;

							case "Phone":
								Array.Resize(ref phoneArray, phoneArray.Length + 1);
								phoneArray[phoneArray.Length - 1] = new PhoneNumber(block.Title, block.Detail);
								break;

							case "IM":
								Array.Resize(ref imArray, imArray.Length + 1);
								imArray[imArray.Length - 1] = new IM(block.Title, block.Detail);
								break;

							default:
								// Should never get here
								break;
						}
					}
				}
			}

			Panel panel2 = (Panel)footerGridEdit.Children[1];

			foreach (UIElement each in panel2.Children)
			{
				if (each is ContactDetailBlock)
				{
					ContactDetailBlock block = (ContactDetailBlock)each;

					if (!string.IsNullOrWhiteSpace(block.Detail))
					{
						switch (block.Tag.ToString())
						{
							case "Work":
								switch (block.Title)
								{
									case "Title":
										work.Title = block.Detail;
										break;

									case "Department":
										work.Department = block.Detail;
										break;

									case "Company":
										work.Company = block.Detail;
										break;

									case "Office":
										work.Office = block.Detail;
										break;

									default:
										// Should never get here
										break;
								}
								break;

							case "Address":
								Address a = block.OriginalData == null || block.Detail != ((Address)block.OriginalData).ToString() ? Address.TryParse(block.Detail) : (Address)block.OriginalData;

								if (a != null)
								{
									Array.Resize(ref addressArray, addressArray.Length + 1);
									a.Type = block.Title;
									addressArray[addressArray.Length - 1] = a;
								}
								break;

							case "Website":
								try
								{
									new Uri(block.Detail);
									Array.Resize(ref websiteArray, websiteArray.Length + 1);
									websiteArray[websiteArray.Length - 1] = new Website(block.Title, block.Detail);
								}
								catch
								{
									try
									{
										new Uri("http://" + block.Detail);
										Array.Resize(ref websiteArray, websiteArray.Length + 1);
										websiteArray[websiteArray.Length - 1] = new Website(block.Title, "http://" + block.Detail);
									}
									catch { }
								}
								break;

							default:
								// Should never get here
								break;
						}
					}
				}
				else if (each is ContactDetailBlockDate)
				{
					ContactDetailBlockDate block = (ContactDetailBlockDate)each;

					switch (block.Tag.ToString())
					{
						case "Special Date":
							if (block.SelectedDate.HasValue)
							{
								SpecialDate specialDate = new SpecialDate(block.Title, block.SelectedDate.Value);

								Array.Resize(ref specialDateArray, specialDateArray.Length + 1);
								specialDateArray[specialDateArray.Length - 1] = specialDate;

								if (block.ShowOnCalendarButtonChecked == true)
								{
									AppointmentDatabase.Add(GetContactSpecialDate(contact, specialDate, contact.Name.ToString()));
									DateModifiedEvent(new DateModifiedEventArgs(block.SelectedDate.Value.Date, null));
								}
							}
							break;

						default:
							break;
					}
				}
			}

			//DateModifiedEvent(new DateModifiedEventArgs(block.SelectedDate.Value.Date, oldBirthday));

			contact.IM = imArray;
			contact.Emails = emailArray;
			contact.PhoneNumbers = phoneArray;
			contact.Work = work;
			contact.Addresses = addressArray;
			contact.Websites = websiteArray;
			contact.SpecialDates = specialDateArray;

			if (notesRTB.HasContentChanged)
				await contact.SetNotesDocumentAsync(notesRTB.Document);

			ContactDatabase.UpdateContact(contact);

			if (rePositionContact)
			{
				bool refocus = enableInteraction && contactsListBox.SelectedIndex == contactsListBox.Items.IndexOf(contact);

				contactsListBox.Items.Remove(contact);
				SmartInsert(contactsListBox, contact);

				if (refocus)
				{
					int index = contactsListBox.Items.IndexOf(contact);
					_activeContact = null;
					contactsListBox.SelectedIndex = index;
				}
			}
		}

		public async Task HideEdit(bool deleteIfEmpty = true)
		{
			_inEditMode = false;

			headerGrid.Visibility = Visibility.Visible;
			footerGrid.Visibility = Visibility.Visible;
			headerGridEdit.Visibility = Visibility.Collapsed;
			footerGridEdit.Visibility = Visibility.Collapsed;

			notesRTB.IsReadOnly = true;
			notesRTB.BorderThickness = new Thickness(0);
			notesRTB.Margin = new Thickness(4, 14, 4, 5);

			detailsScroller.ScrollToHome();

			((Panel)footerGridEdit.Children[0]).Children.Clear();
			((Panel)footerGridEdit.Children[1]).Children.Clear();

			if (_activeContact != null)
			{
				FlowDocument notes = await _activeContact.GetNotesDocumentAsync();
				notesRTB.Document = notes;

				if (notes.Blocks.Count != 0)
				{
					notesRTB.Visibility = Visibility.Visible;
					noNoteMsg.Visibility = Visibility.Collapsed;
				}
				else
				{
					notesRTB.Visibility = Visibility.Collapsed;
					noNoteMsg.Visibility = Visibility.Visible;
				}

				if (deleteIfEmpty && !ContactDatabase.ContactExists(_activeContact))
				{
					if (Settings.AnimationsEnabled)
					{
						AnimationHelpers.DeleteAnimation deleteAnim = new AnimationHelpers.DeleteAnimation((ListBoxItem)contactsListBox.ItemContainerGenerator.ContainerFromItem(_activeContact));
						deleteAnim.OnAnimationCompletedEvent += deleteAnim_OnAnimationCompletedEvent;
						deleteAnim.Animate();

						if (contactsListBox.Items.Count <= 1)
						{
							statusText.Text = "We didn't find anything to show here.";
							new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
						}
					}
					else
					{
						contactsListBox.Items.Remove(_activeContact);

						if (contactsListBox.Items.Count == 0)
						{
							statusText.Text = "We didn't find anything to show here.";
							statusText.Visibility = Visibility.Visible;
							statusText.Opacity = 1;
						}
					}

					HideRightPanel();
				}
			}

			EndEditEvent(EventArgs.Empty);
		}

		private void BeginEdit()
		{
			Contact contact = (Contact)contactsListBox.SelectedItem;

			if (_inEditMode && _activeContact == contact)
			{
				contactNameEdit.Tag = new Name(_activeContact.Name);
				return;
			}

			_inEditMode = true;

			detailsScroller.ScrollToHome();

			headerGridEdit.Visibility = Visibility.Visible;
			footerGridEdit.Visibility = Visibility.Visible;
			headerGrid.Visibility = Visibility.Collapsed;
			footerGrid.Visibility = Visibility.Collapsed;

			contactNameEdit.Detail = contact.Name != null ? contact.Name.ToString() : string.Empty;
			contactNameEdit.Tag = new Name(contact.Name);
			contactNameEdit.FocusTextBox();

			contactTileEdit.Source = contact.Tile;

			Work work = contact.Work;

			if (work != null)
			{
				contactJobTitle.Text = work.Title;

				string other = work.Department != string.Empty ? work.Department : work.Company;
				contactJobTitle.Text += (work.Title != string.Empty && other != string.Empty ? ", " : string.Empty) + other;
			}
			else
				contactJobTitle.Clear();

			Panel panel1 = (Panel)footerGridEdit.Children[0];
			panel1.Children.Clear();

			//
			// Email
			//
			Email[] emails = contact.Emails;

			ContactDetailHeader emailHeader = new ContactDetailHeader("Email", panel1);
			List<string> availableEmails = new List<string>(new string[] { "Personal", "Personal 2", "Work", "Work 2", "Other" });

			if (emails != null)
				foreach (Email each in emails)
				{
					new ContactDetailBlock(each.Type, each.Address, panel1, "Email");

					if (each.Type != "Other")
						availableEmails.Remove(each.Type);
				}

			//if (availableEmails.Count > 0)
			emailHeader.MenuItems = availableEmails;
			//else
			//	emailHeader.IsAddEnabled = false;

			//
			// Phone
			//
			PhoneNumber[] phones = contact.PhoneNumbers;

			ContactDetailHeader phoneHeader = new ContactDetailHeader("Phone", panel1);
			List<string> availablePhones = new List<string>(new string[] { "Work", "Work 2", "Work Fax", "Mobile", "Home", "Home 2", "Other" });

			if (phones != null)
				foreach (PhoneNumber each in phones)
				{
					new ContactDetailBlock(each.Type, each.Number, panel1, "Phone");

					if (each.Type != "Other")
						availablePhones.Remove(each.Type);
				}

			//if (availablePhones.Count > 0)
			phoneHeader.MenuItems = availablePhones;
			//else
			//	phoneHeader.IsAddEnabled = false;

			//
			// IM
			//
			IM[] ims = contact.IM;

			ContactDetailHeader imHeader = new ContactDetailHeader("IM", panel1);
			List<string> availableIMs = new List<string>(new string[] { "Google Talk", "AIM", "Yahoo", "Skype", "QQ", "MSN", "ICQ", "Jabber", "Other" });

			if (ims != null)
				foreach (IM each in ims)
				{
					new ContactDetailBlock(each.Type, each.Address, panel1, "IM");

					if (each.Type != "Other")
						availableIMs.Remove(each.Type);
				}

			//if (availableIMs.Count > 0)
			imHeader.MenuItems = availableIMs;
			//else
			//	imHeader.IsAddEnabled = false;

			Panel panel2 = (Panel)footerGridEdit.Children[1];
			panel2.Children.Clear();

			//
			// Work
			//
			ContactDetailHeader workheader = new ContactDetailHeader("Work", panel2);
			List<string> availableWorkItems = new List<string>(new string[] { "Title", "Department", "Company", "Office" });

			if (work != null)
			{
				if (work.Title != string.Empty)
				{
					new ContactDetailBlock("Title", work.Title, panel2, "Work") { TitleReadOnly = true };
					availableWorkItems.Remove("Title");
				}

				if (work.Department != string.Empty)
				{
					new ContactDetailBlock("Department", work.Department, panel2, "Work") { TitleReadOnly = true };
					availableWorkItems.Remove("Department");
				}

				if (work.Company != string.Empty)
				{
					new ContactDetailBlock("Company", work.Company, panel2, "Work") { TitleReadOnly = true };
					availableWorkItems.Remove("Company");
				}

				if (work.Office != string.Empty)
				{
					new ContactDetailBlock("Office", work.Office, panel2, "Work") { TitleReadOnly = true };
					availableWorkItems.Remove("Office");
				}
			}

			if (availableWorkItems.Count > 0)
				workheader.MenuItems = availableWorkItems;
			else
				workheader.IsAddEnabled = false;

			//
			// Address
			//
			ContactDetailHeader addressHeader = new ContactDetailHeader("Address", panel2);

			Address[] addresses = contact.Addresses;
			List<string> availableAddresses = new List<string>(new string[] { "Home", "Work", "Other" });

			if (addresses != null)
				foreach (Address each in addresses)
				{
					ContactDetailBlock address = new ContactDetailBlock(each.Type, each.ToString(), panel2, "Address");
					address.CanClarify = true;
					address.IsMultiLine = true;
					address.OriginalData = new Address(each);
					address.Clarify += address_Clarify;

					if (each.Type != "Other")
						availableAddresses.Remove(each.Type);
				}

			//if (availableAddresses.Count > 0)
			addressHeader.MenuItems = availableAddresses;
			//else
			//	addressHeader.IsAddEnabled = false;

			//
			// Website
			//
			ContactDetailHeader websiteHeader = new ContactDetailHeader("Website", panel2);

			Website[] websites = contact.Websites;
			List<string> availableWebsites = new List<string>(new string[] { "Profile", "Blog", "Home Page", "Work", "Other" });

			if (websites != null)
				foreach (Website each in websites)
				{
					new ContactDetailBlock(each.Type, each.Url, panel2, "Website");

					if (each.Type != "Other")
						availableWebsites.Remove(each.Type);
				}

			//if (availableWebsites.Count > 0)
			websiteHeader.MenuItems = availableWebsites;
			//else
			//	websiteHeader.IsAddEnabled = false;

			//
			// Special Dates
			//
			ContactDetailHeader specialDateHeader = new ContactDetailHeader("Special Date", panel2);

			SpecialDate[] specialDates = contact.SpecialDates;
			List<string> availableSpecialDates = new List<string>(new string[] { "Birthday", "Anniversary", "Other" });

			if (specialDates != null)
			{
				foreach (SpecialDate each in specialDates)
				{
					new ContactDetailBlockDate(each.Type, each.Date, panel2, each.Type)
					{
						ShowOnCalendarButtonVisibility = Visibility.Visible,
						ShowOnCalendarButtonChecked = AppointmentDatabase.GetAppointment(ContactSpecialDateId(contact, each.Type)) != null
					};

					if (each.Type != "Other")
						availableSpecialDates.Remove(each.Type);
				}
			}

			specialDateHeader.MenuItems = availableSpecialDates;

			noNoteMsg.Visibility = Visibility.Collapsed;

			notesRTB.Visibility = Visibility.Visible;
			notesRTB.IsReadOnly = false;
			notesRTB.BorderThickness = new Thickness(1);
			notesRTB.Margin = new Thickness(3, 13, 3, 4);

			BeginEditEvent(EventArgs.Empty);
		}

		/// <summary>
		/// Save open item.
		/// </summary>
		public async void SaveAndClose()
		{
			await Save(true);
			await HideEdit();
			await OpenSelectedItem(true);

			string[] favs = Settings.PeopleFavorites;
			Contact contact = (Contact)contactsListBox.SelectedItem;

			if (favs != null && Array.IndexOf(favs, contact.ID) != -1)
			{
				foreach (PeoplePeekContent each in PeoplePeekContent.LoadedPeoplePeekContents)
					each.Update(contact);
			}
		}

		private async Task OpenSelectedItem(bool forceLoad = false)
		{
			Contact contact = (Contact)contactsListBox.SelectedItem;

			if (contact == null)
				return;

			if (_activeContact != contact)
			{
				await Save(false);
				_activeContact = contact;
			}
			else if (!forceLoad)
				return;

			detailsScroller.ScrollToHome();
			notesRTB.ScrollToHome();

			contactName.Text = contact.Name != null ? contact.Name.ToString() : "";
			contactTile.Source = contact.Tile;
			contactJobTitle.Text = contact.WorkDescription;

			Panel panel1 = (Panel)footerGrid.Children[0];
			panel1.Children.Clear();

			Email[] emails = contact.Emails;

			if (emails != null)
				foreach (Email each in emails)
					new ContactDetailBlockHyperlink(each.Type + " Email", each.Address, "mailto:" + each.Address, panel1);

			PhoneNumber[] phones = contact.PhoneNumbers;

			if (phones != null)
				foreach (PhoneNumber each in phones)
					new ContactDetailBlock(each.Type + " Phone", each.Number, panel1);

			IM[] ims = contact.IM;

			if (ims != null)
				foreach (IM each in ims)
					new ContactDetailBlock(each.Type, each.Address, panel1);

			Panel panel2 = (Panel)footerGrid.Children[1];
			panel2.Children.Clear();

			Work work = contact.Work;

			if (work != null)
			{
				if (work.Office != "")
					new ContactDetailBlock("Office", work.Office, panel2);

				if (work.Company != "")
					new ContactDetailBlock("Company", work.Company, panel2);
			}

			Address[] addresses = contact.Addresses;

			if (addresses != null)
				foreach (Address each in addresses)
				{
					ContactDetailBlock address = new ContactDetailBlock(each.Type + " Address", each.ToString(), panel2);
					address.IsMultiLine = true;
				}

			Website[] websites = contact.Websites;

			if (websites != null)
				foreach (Website each in websites)
					new ContactDetailBlockHyperlink(each.Type, each.Url, panel2);

			SpecialDate[] specialDates = contact.SpecialDates;

			if (specialDates != null)
				foreach (SpecialDate each in specialDates)
					new ContactDetailBlock(each.Type, each.Date.ToShortDateString(), panel2);

			FlowDocument notes = await contact.GetNotesDocumentAsync();
			notesRTB.Document = notes;

			if (notes.Blocks.Count != 0)
			{
				notesRTB.Visibility = Visibility.Visible;
				noNoteMsg.Visibility = Visibility.Collapsed;
			}
			else
			{
				notesRTB.Visibility = Visibility.Collapsed;
				noNoteMsg.Visibility = Visibility.Visible;
			}

			ShowRightPanel();

			OpenContactEvent(EventArgs.Empty);
		}

		private void ShowRightPanel()
		{
			if (detailsGrid.Visibility == Visibility.Hidden)
			{
				if (Settings.AnimationsEnabled)
				{
					detailsGrid.UpdateLayout();
					AnimationHelpers.SingleZoomDisplay zoom = new AnimationHelpers.SingleZoomDisplay(detailsGrid);
					zoom.SwitchViews(AnimationHelpers.ZoomDirection.In);
				}
				else
					detailsGrid.Visibility = Visibility.Visible;
			}
		}

		private void HideRightPanel()
		{
			if (detailsGrid.Visibility == Visibility.Visible)
			{
				if (Settings.AnimationsEnabled)
				{
					AnimationHelpers.SingleZoomDisplay zoom = new AnimationHelpers.SingleZoomDisplay(detailsGrid);
					zoom.SwitchViews(AnimationHelpers.ZoomDirection.Out);
				}
				else
					detailsGrid.Visibility = Visibility.Hidden;
			}
		}

		/// <summary>
		/// Insert a contact into the listbox the correct alphabetical posistion.
		/// </summary>
		public static void SmartInsert(ListBox box, Contact contact)
		{
			ItemCollection allItems = box.Items;
			int count = allItems.Count;

			int lowerbound = 0;
			int upperbound = count;

			Name name = contact.Name;

			if (name == null)
			{
				box.Items.Insert(0, contact);
				return;
			}

			if (count > 0)
			{
				while (true)
				{
					if (upperbound - lowerbound == 1)
					{
						int index = lowerbound;

						Name itemName = ((Contact)allItems[index]).Name;

						if (name.CompareTo(itemName) < 0)
							box.Items.Insert(index, contact);
						else
							box.Items.Insert(index + 1, contact);

						break;
					}
					else
					{
						Name middle = ((Contact)allItems[lowerbound + (upperbound - lowerbound) / 2]).Name;

						if (name.CompareTo(middle) < 0)
							upperbound -= (upperbound - lowerbound) / 2;
						else
							lowerbound += (upperbound - lowerbound) / 2;
					}
				}
			}
			else
				box.Items.Add(contact);
		}

		private string _scrollToContactId = null;

		public async void ScrollToContact(string id)
		{
			if (IsLoaded)
			{
				_scrollToContactId = null;

				int counter = 0;

				foreach (Contact each in contactsListBox.Items)
				{
					if (each.ID == id)
					{
						if (contactsListBox.SelectedIndex != counter)
							contactsListBox.SelectedIndex = counter;
						else
							await OpenSelectedItem();

						return;
					}

					counter++;
				}
			}
			else
				_scrollToContactId = id;
		}

		private string _queued;
		private bool _hasQueue = false;

		/// <summary>
		/// Queue a contact to show as soon as pane has loaded.
		/// </summary>
		/// <param name="subject"></param>
		public void Queue(string name)
		{
			_hasQueue = true;
			_queued = name;
		}

		/// <summary>
		/// Create a new blank contact.
		/// </summary>
		public void AddContact(bool openEdit = false, string name = null)
		{
			Contact c = new Contact();

			if (name != null)
			{
				Name n = DatabaseHelpers.Contacts.Name.TryParse(name);

				if (n == null)
				{
					ClarifyName cn = new ClarifyName();
					cn.Owner = Window.GetWindow(this);

					if (cn.ShowDialog() == true)
						n = cn.Name;
					else
						return;
				}

				c.Name = n;

				if (CheckNameDuplicate(n))
					return;
			}

			ContactDatabase.Add(c);
			AddContact(c, openEdit);
		}

		public void AddContact(Contact contact, bool openEdit = false)
		{
			SmartInsert(contactsListBox, contact);

			contactsListBox.UpdateLayout();
			ListBoxItem item = (ListBoxItem)contactsListBox.ItemContainerGenerator.ContainerFromItem(contact);
			new AnimationHelpers.LoadAnimation(item).Animate(50);

			if (openEdit)
			{
				contactsListBox.SelectedIndex = contactsListBox.Items.IndexOf(contact);
				BeginEdit();
			}

			if (_currentView == ViewMode.Favorites)
				ToggleShowFavorites();

			if (Settings.AnimationsEnabled)
				new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.Out, true);
			else
				statusText.Visibility = Visibility.Hidden;
		}

		/// <summary>
		/// Updates an existing contact. If it does not exist in the display, it is added.
		/// </summary>
		/// <param name="contact">The contact to update.</param>
		public void UpdateContact(Contact contact)
		{
			Contact existing = ContactExistsInDisplay(contact);

			if (existing != null)
				existing.CopyPropertiesFrom(contact);
			else
				AddContact(contact, false);
		}

		/// <summary>
		/// Delete the active contact.
		/// </summary>
		public void Delete()
		{
			DeleteContact(_activeContact);
			//_inEditMode = false;
			//ContactDatabase.Delete(_activeContact);

			//contactsListBox.Items.Remove(_activeContact);

			//if (contactsListBox.Items.Count == 0)
			//{
			//	statusText.Text = "We didn't find anything to show here.";

			//	if (AnimationHelpers.AnimationsEnabled)
			//		new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
			//	else
			//	{
			//		statusText.Visibility = Visibility.Visible;
			//		statusText.Opacity = 1;
			//	}
			//}

			//Settings.PeopleFavorites = Settings.PeopleFavorites.RemoveEntry(_activeContact.ID);
			//HideEdit();
			//HideRightPanel();
			//EndEditEvent(EventArgs.Empty);
		}

		#region Flip Animation

		///// <summary>
		///// Flip from showing all contacts to only favorites, or vice versa.
		///// </summary>
		//private void Flip()
		//{
		//	_currentView = _currentView == ViewMode.All ? ViewMode.Favorites : ViewMode.All;

		//	string n = (contactsListBox.ActualWidth / (100 * contactsListBox.ActualWidth / 165)).ToString();
		//	string n2 = (contactsListBox.ActualHeight / (100 * contactsListBox.ActualWidth / 165)).ToString();
		//	mesh3dFront.Positions = Point3DCollection.Parse("-" + n + ",-" + n2 + ",0 " + n + ",-" + n2 + ",0 " + n + "," + n2 + ",0 " + n + "," + n2 + ",0 -" + n + "," + n2 + ",0 -" + n + ",-" + n2 + ",0");
		//	mesh3dBack.Positions = Point3DCollection.Parse("-" + n + ",-" + n2 + ",0 -" + n + "," + n2 + ",0 " + n + "," + n2 + ",0 " + n + "," + n2 + ",0 " + n + ",-" + n2 + ",0 -" + n + ",-" + n2 + ",0");

		//	viewBox.Visibility = Visibility.Visible;
		//	viewBox.UpdateLayout();
		//	DoubleAnimation flipAnim = new DoubleAnimation(0, 90, AnimationHelpers.AnimationDuration, FillBehavior.Stop);
		//	QuarticEase ease = new QuarticEase();
		//	ease.EasingMode = EasingMode.EaseIn;
		//	flipAnim.EasingFunction = ease;
		//	flipAnim.Completed += flipAnim_Completed;
		//	yRotate.BeginAnimation(AxisAngleRotation3D.AngleProperty, flipAnim);
		//}

		//private void flipAnim_Completed(object sender, EventArgs e)
		//{
		//	if (((sender as AnimationClock).Timeline as DoubleAnimation).To == 90)
		//	{
		//		if (_currentView == ViewMode.Favorites)
		//		{
		//			//
		//			// Load favorites
		//			//
		//			string[] favorites = Settings.PeopleFavorites;

		//			if (favorites != null)
		//			{
		//				bool anyLeft = false;

		//				foreach (Contact each in contactsListBox.Items)
		//				{
		//					if (Array.IndexOf(favorites, each.ID) == -1)
		//						((ListBoxItem)contactsListBox.ItemContainerGenerator.ContainerFromItem(each)).Visibility = Visibility.Collapsed;
		//					else
		//						anyLeft = true;
		//				}

		//				if (!anyLeft)
		//				{
		//					statusText.Text = "We didn't find anything to show here.";
		//					new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
		//				}
		//			}
		//			else
		//			{
		//				foreach (Contact each in contactsListBox.Items)
		//					((ListBoxItem)contactsListBox.ItemContainerGenerator.ContainerFromItem(each)).Visibility = Visibility.Collapsed;

		//				statusText.Text = "We didn't find anything to show here.";
		//				new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
		//			}
		//		}
		//		else
		//		{
		//			//
		//			// Show all
		//			//
		//			if (contactsListBox.HasItems)
		//			{
		//				foreach (Contact each in contactsListBox.Items)
		//					((ListBoxItem)contactsListBox.ItemContainerGenerator.ContainerFromItem(each)).Visibility = Visibility.Visible;

		//				new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.Out);
		//			}
		//			else
		//			{
		//				statusText.Text = "We didn't find anything to show here.";
		//				new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
		//			}
		//		}

		//		DoubleAnimation flipAnim = new DoubleAnimation(90, 180, AnimationHelpers.AnimationDuration, FillBehavior.Stop);
		//		QuarticEase ease = new QuarticEase();
		//		ease.EasingMode = EasingMode.EaseOut;
		//		flipAnim.EasingFunction = ease;
		//		flipAnim.Completed += flipAnim_Completed;
		//		yRotate.BeginAnimation(AxisAngleRotation3D.AngleProperty, flipAnim);
		//	}
		//	else
		//		viewBox.Visibility = Visibility.Collapsed;
		//}

		#endregion

		private async void DeleteContact(Contact item)
		{
			// Close the contact if it is being edited
			if (item == contactsListBox.SelectedItem)
			{
				if (_inEditMode)
				{
					await HideEdit(false);
					EndEditEvent(EventArgs.Empty);
				}

				HideRightPanel();
				_activeContact = null;

				CloseContactEvent(EventArgs.Empty);
			}

			if (Settings.AnimationsEnabled)
			{
				AnimationHelpers.DeleteAnimation deleteAnim = new AnimationHelpers.DeleteAnimation((ListBoxItem)contactsListBox.ItemContainerGenerator.ContainerFromItem(item));
				deleteAnim.OnAnimationCompletedEvent += deleteAnim_OnAnimationCompletedEvent;
				deleteAnim.Animate();

				if (contactsListBox.Items.Count <= 1)
				{
					statusText.Text = "We didn't find anything to show here.";
					new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
				}
			}
			else
			{
				contactsListBox.Items.Remove(item);

				if (contactsListBox.Items.Count == 0)
				{
					statusText.Text = "We didn't find anything to show here.";
					statusText.Visibility = Visibility.Visible;
					statusText.Opacity = 1;
				}
			}

			if (item.SpecialDates != null)
				foreach (SpecialDate each in item.SpecialDates)
					AppointmentDatabase.Delete(ContactSpecialDateId(item, each.Type), false);

			ContactDatabase.Delete(item);

			string[] favs = Settings.PeopleFavorites;

			if (favs != null && Array.IndexOf(favs, item.ID) != -1)
			{
				foreach (PeoplePeekContent each in PeoplePeekContent.LoadedPeoplePeekContents)
					each.Delete(item.ID);

				Settings.PeopleFavorites = favs.RemoveEntry(item.ID);
			}
		}

		private void deleteAnim_OnAnimationCompletedEvent(object sender, EventArgs e)
		{
			AnimationHelpers.DeleteAnimation del = (AnimationHelpers.DeleteAnimation)sender;
			del.OnAnimationCompletedEvent -= deleteAnim_OnAnimationCompletedEvent;

			ListBoxItem item = (ListBoxItem)del.Control;

			// Delete the contact from the tree
			if (item != null)
				contactsListBox.Items.Remove(contactsListBox.ItemContainerGenerator.ItemFromContainer(item));
		}

		//private void LayoutQuickNav()
		//{
		//	double height = leftPanelGrid.ActualHeight;
		//	double buttonHeight = 23;	// Height + Margin;
		//	int maxButtons = (int)Math.Floor(height / buttonHeight) - 1;
		//	int lettersPerButton = (int)Math.Ceiling(26d / (double)maxButtons);

		//	quickNav.Children.RemoveRange(1, quickNav.Children.Count - 1);

		//	// a-z = 97-122
		//	for (int i = 0; i < maxButtons; i++)
		//	{
		//		Button button = new Button();

		//		int firstLetter = i * lettersPerButton + 97;
		//		int lastLetter = firstLetter + lettersPerButton - 1;

		//		lastLetter = lastLetter > 122 ? 122 : lastLetter;

		//		string text = ((char)firstLetter).ToString();

		//		if (lastLetter - firstLetter > 1)
		//			text += "-";

		//		if (lastLetter != firstLetter)
		//			text += ((char)lastLetter).ToString();

		//		button.Content = text;

		//		quickNav.Children.Add(button);

		//		if (lastLetter == 122)
		//			break;
		//	}
		//}

		private void LayoutQuickNav()
		{
			double height = leftPanelGrid.ActualHeight;
			double buttonHeight = 23;	// Height + Margin;
			string letters = "abcdefghijklmnopqrstuvwxyz";

			quickNav.Children.RemoveRange(1, quickNav.Children.Count - 1);

			while (letters.Length > 0)
			{
				int maxButtons = (int)Math.Floor(height / buttonHeight) - 1;
				int lettersPerButton = (int)Math.Ceiling(letters.Length / (double)maxButtons);

				if (lettersPerButton > letters.Length)
					lettersPerButton = letters.Length;

				height -= buttonHeight;

				Button button = new Button();

				if (lettersPerButton > 2)
					button.Content = letters[0].ToString() + "-" + letters[lettersPerButton - 1].ToString();
				else if (lettersPerButton == 2)
					button.Content = letters[0].ToString() + letters[1].ToString();
				else
					button.Content = letters[0];

				letters = letters.Substring(lettersPerButton);

				quickNav.Children.Add(button);
			}
		}

		/// <summary>
		/// Change the visibility of the currently active contact.
		/// </summary>
		/// <param name="_private"></param>
		public void ChangeActivePrivate(bool _private)
		{
			_activeContact.Private = _private;
		}

		public void ChangeContactPicture()
		{
			TilePicker picker = new TilePicker(_activeContact.Name);
			picker.ImageSource = (BitmapSource)contactTileEdit.Source;
			picker.Owner = Window.GetWindow(this);

			if (picker.ShowDialog() == true)
			{
				contactTileEdit.Source = picker.ImageSource != null ? picker.ImageSource : new BitmapImage(Contact.DefaultTile);
			}

			picker = null;

			GC.Collect();
		}

		private string ContactSpecialDateId(Contact contact, string type)
		{
			return type.ToUpper() + contact.ID;
		}

		private Appointment[] GetContactSpecialDates(Contact contact)
		{
			if (contact == null)
				return new Appointment[0];

			string name = contact.Name.ToString();

			SpecialDate[] dates = contact.SpecialDates;

			if (dates == null)
				return new Appointment[0];

			Appointment[] appts = new Appointment[dates.Length];
			int counter = 0;

			foreach (SpecialDate each in contact.SpecialDates)
				appts[counter++] = GetContactSpecialDate(contact, each, name);

			return appts;
		}

		private Appointment GetContactSpecialDate(Contact contact, SpecialDate specialDate, string name)
		{
			DateTime date = specialDate.Date;

			Appointment appt = new Appointment(false);
			appt.ID = ContactSpecialDateId(contact, specialDate.Type);
			appt.Subject = name + "'" + (!name.EndsWith("s", StringComparison.InvariantCultureIgnoreCase) ? "s" : "")
				+ " %s " + specialDate.Type;
			appt.StartDate = date;
			appt.EndDate = date.AddDays(1);

			Recurrence recur = appt.Recurrence;

			recur.End = RepeatEnd.None;
			recur.Year = 1;
			recur.Month = date.Month - 1;
			recur.Day = date.Day.ToString();
			recur.Type = RepeatType.Yearly;

			appt.IsRepeating = true;
			appt.ShowAs = ShowAs.Free;
			appt.AllDay = true;
			appt.ReadOnly = true;
			appt.CategoryID = AppointmentDatabase.SpecialDateCategoryId;
			appt.Sync = false;
			appt.LastModified = DateTime.UtcNow;

			return appt;
		}

		private Contact ContactExistsInDisplay(Contact contact)
		{
			string id = contact.ID;

			foreach (Contact each in contactsListBox.Items)
				if (each.ID == id)
					return each;

			return null;
		}

		private bool CheckNameDuplicate(Name name)
		{
			if (Settings.PeopleCheckDuplicates)
				if (ContactDatabase.ContactExists(name))
					return !new TaskDialog(Window.GetWindow(this), "Duplicate Contact",
						"You already added someone with this name. Do you want to create a duplicate contact?",
						MessageType.Question, "_Yes", "_No").ShowDialog().Value;

			return false;
		}

		#endregion

		#region UI

		private void PeopleView_Loaded(object sender, RoutedEventArgs e)
		{
			LayoutQuickNav();
			new ContextTextFormatter(notesRTB);
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			if (IsLoaded && sizeInfo.HeightChanged)
				LayoutQuickNav();
		}

		private void quickNav_Click(object sender, RoutedEventArgs e)
		{
			string content = ((ContentControl)sender).Content.ToString();

			if (content.Contains("-"))
			{
				char first = content[0];
				char last = content[2];

				string concat = "";

				for (int i = first; i < last; i++)
					concat += ((char)i).ToString();

				content = concat;
			}

			foreach (Contact each in contactsListBox.Items)
			{
				ListBoxItem lbi = (ListBoxItem)contactsListBox.ItemContainerGenerator.ContainerFromItem(each);
				if (lbi.Height != 0)
				{
					string lastName = each.Name.LastName;

					if (lastName.Length > 0)
					{
						char c = char.ToLower(lastName[0]);

						foreach (char x in content)
						{
							if (x == c)
							{
								lbi.IsSelected = true;
								lbi.BringIntoView();
								return;
							}
						}
					}
				}
			}
		}

		//private void newContactTextBox_TextChanged(object sender, TextChangedEventArgs e)
		//{
		//	newContactWatermark.Visibility = string.IsNullOrEmpty(newContactTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
		//}

		private void newContactTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				AddContact(true, newContactTextBox.Text);
				newContactTextBox.Clear();
			}
		}

		private void detailsGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				if (e.Delta < 0)
				{
					e.Handled = true;

					if (GlobalData.ZoomOnMouseWheel)
						SaveAndClose();
				}
			}
		}

		private bool _inEditMode = false;

		private void edit_Click(object sender, RoutedEventArgs e)
		{
			BeginEdit();
		}

		private async void cancelEdit_Click(object sender, RoutedEventArgs e)
		{
			await HideEdit();
		}

		private async void contactsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				if (_inEditMode)
				{
					await Save(false);
					await HideEdit();

					_activeContact = null;
				}

				await OpenSelectedItem();
			}
			else
				CloseContactEvent(e);
		}

		private void ContactDetailHeader_Add(object sender, RoutedEventArgs e)
		{
			ContactDetailHeader _sender = (ContactDetailHeader)sender;
			Panel _parent = (Panel)_sender.Parent;

			UIElement _block = null;

			switch (_sender.Title)
			{
				case "Special Date":
					{
						ContactDetailBlockDate block = new ContactDetailBlockDate(((AddEventArgs)e).Type, null);
						block.Loaded += block_Loaded;
						block.Tag = _sender.Title;
						block.ShowOnCalendarButtonVisibility = Visibility.Visible;
						block.ShowOnCalendarButtonChecked = true;

						_block = block;
					}
					break;

				default:
					{
						ContactDetailBlock block = new ContactDetailBlock(((AddEventArgs)e).Type, "");
						block.Loaded += block_Loaded;
						block.Tag = _sender.Title;

						switch (_sender.Title)
						{
							case "Address":
								block.IsMultiLine = true;
								block.CanClarify = true;
								block.Clarify += address_Clarify;
								break;

							case "Work":
								block.TitleReadOnly = true;
								break;

							default:
								break;
						}

						_block = block;
					}
					break;
			}

			int startIndex = _parent.Children.IndexOf(_sender) + 1;

			while (startIndex < _parent.Children.Count && !(_parent.Children[startIndex] is ContactDetailHeader))
				startIndex++;

			_parent.Children.Insert(startIndex, _block);
		}

		private void block_Loaded(object sender, RoutedEventArgs e)
		{
			if (sender is ContactDetailBlock)
				((ContactDetailBlock)sender).FocusTextBox();
			else if (sender is ContactDetailBlockDate)
				((ContactDetailBlockDate)sender).FocusTextBox();
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			TextEditing.Hyperlink_Click(sender, e);
		}

		private void contactTileEditButton_Click(object sender, RoutedEventArgs e)
		{
			ChangeContactPicture();
		}

		private void Contact_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			FrameworkElement grid = (FrameworkElement)sender;
			Contact contact = (Contact)((ContentControl)((FrameworkElement)grid.TemplatedParent).TemplatedParent).Content;

			string[] favorites = Settings.PeopleFavorites;
			bool found = false;
			if (favorites != null)
			{
				string id = contact.ID;

				foreach (string each in favorites)
				{
					if (each == id)
					{
						found = true;

						MenuItem mi = (MenuItem)grid.ContextMenu.Items[2];
						mi.Header = "Remove from _Favorites";
						mi.Tag = "0" + id;

						break;
					}
				}
			}

			if (!found)
			{
				MenuItem mi = (MenuItem)grid.ContextMenu.Items[2];
				mi.Header = "Add to _Favorites";
				mi.Tag = "1" + contact.ID;
			}
		}

		private void addToFavorites_Click(object sender, RoutedEventArgs e)
		{
			MenuItem _sender = (MenuItem)sender;

			string tag = _sender.Tag.ToString();
			string id = tag.Substring(1);

			if (tag[0] == '1')
			{
				string[] favorites = Settings.PeopleFavorites;

				if (favorites != null)
				{
					Array.Resize(ref favorites, favorites.Length + 1);
					favorites[favorites.Length - 1] = id;
				}
				else
					favorites = new string[] { id };

				foreach (PeoplePeekContent each in PeoplePeekContent.LoadedPeoplePeekContents)
					each.Add(ContactDatabase.GetContact(id));

				if (_currentView == ViewMode.Favorites)
				{
					if (Settings.AnimationsEnabled)
						new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.Out, true);
					else
						statusText.Visibility = Visibility.Hidden;

					ListBoxItem item = (ListBoxItem)contactsListBox.ItemContainerGenerator.ContainerFromItem(_activeContact);

					if (item != null)
					{
						if (Settings.AnimationsEnabled)
							(new AnimationHelpers.LoadAnimation(item)).Animate(50);
						else
						{
							item.IsHitTestVisible = true;
							item.IsEnabled = true;
							item.Opacity = 1;
							item.Height = 50;
						}
					}
				}

				Settings.PeopleFavorites = favorites;
			}
			else
			{
				Settings.PeopleFavorites = Settings.PeopleFavorites.RemoveEntry(id);

				if (_currentView == ViewMode.Favorites)
					foreach (Contact each in contactsListBox.Items)
					{
						if (each.ID == id)
						{
							ListBoxItem item = (ListBoxItem)contactsListBox.ItemContainerGenerator.ContainerFromItem(each);

							if (item != null)
								(new AnimationHelpers.DeleteAnimation(item)).Animate();

							if (Settings.PeopleFavorites.Length == 0)
							{
								statusText.Text = "We didn't find anything to show here.";
								new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In);
							}

							break;
						}
					}

				foreach (PeoplePeekContent each in PeoplePeekContent.LoadedPeoplePeekContents)
					each.Delete(id);

				if (_currentView == ViewMode.Favorites)
				{
					if (Settings.PeopleFavorites.Length == 0)
						if (Settings.AnimationsEnabled)
							new AnimationHelpers.Fade(statusText, AnimationHelpers.FadeDirection.In, true);
						else
							statusText.Visibility = Visibility.Visible;

					ListBoxItem item = (ListBoxItem)contactsListBox.ItemContainerGenerator.ContainerFromItem(_activeContact);

					if (item != null)
					{
						if (Settings.AnimationsEnabled)
							(new AnimationHelpers.DeleteAnimation(item)).Animate();
						else
						{
							item.IsHitTestVisible = false;
							item.IsEnabled = false;
							item.Opacity = 0;
							item.Height = 0;
						}
					}
				}
			}
		}

		private void delete_Click(object sender, RoutedEventArgs e)
		{
			Delete();
		}

		private void clarifyName_Click(object sender, RoutedEventArgs e)
		{
			Name name;

			if (contactNameEdit.Detail != _activeContact.Name.ToString()
				&& contactNameEdit.Detail != ((Name)contactNameEdit.Tag).ToString())
			{
				name = DatabaseHelpers.Contacts.Name.TryParse(contactNameEdit.Detail);
			}
			else
			{
				name = new Name((Name)contactNameEdit.Tag);
			}

			ClarifyName c = new ClarifyName(name);
			c.Owner = Window.GetWindow(this);

			if (c.ShowDialog() == true)
			{
				contactNameEdit.Detail = c.Name.ToString();
				contactNameEdit.Tag = c.Name;
			}
		}

		private void address_Clarify(object sender, RoutedEventArgs e)
		{
			ContactDetailBlock _sender = (ContactDetailBlock)sender;
			Address address = Address.TryParse(_sender.Detail);

			ClarifyAddress clarifyDialog = new ClarifyAddress(address);
			clarifyDialog.Owner = Window.GetWindow(this);

			if (clarifyDialog.ShowDialog() == true)
			{
				_sender.Detail = clarifyDialog.Address.ToString();
				_sender.OriginalData = clarifyDialog.Address;
			}
		}

		private void contactsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (!(e.OriginalSource is ScrollViewer))
				return;

			AddContact(true);
		}

		private void allContactsRadio_Checked(object sender, RoutedEventArgs e)
		{
			if (IsLoaded && _currentView != ViewMode.All)
				ToggleShowFavorites();
		}

		private void favoriteContactsRadio_Checked(object sender, RoutedEventArgs e)
		{
			if (IsLoaded && _currentView != ViewMode.Favorites)
				ToggleShowFavorites();
		}

		private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//
			// BUG FIX: For some reason, SpellChecking.FocusedRTB would be null when tab control
			//			switched back to notesRTB. Might have something to do with how the TabControl
			//			operates.
			//
			if (((TabControl)sender).SelectedIndex == 1)
				SpellChecking.FocusedRTB = notesRTB;
		}

		private void optionsButton_Click(object sender, RoutedEventArgs e)
		{
			ContextMenu menu = new ContextMenu();

			Contact contact = _activeContact;

			MenuItem favs = new MenuItem();
			favs.Click += addToFavorites_Click;

			string[] favorites = Settings.PeopleFavorites;
			bool found = false;
			if (favorites != null)
			{
				string id = contact.ID;

				foreach (string each in favorites)
				{
					if (each == id)
					{
						found = true;
						favs.Header = "Remove from _Favorites";
						favs.Tag = "0" + contact.ID;
						break;
					}
				}
			}

			if (!found)
			{
				favs.Header = "Add to _Favorites";
				favs.Tag = "1" + contact.ID;
			}

			menu.Items.Add(favs);

			menu.IsOpen = true;
		}

		#endregion

		#region Events

		public delegate void OnBeginEdit(object sender, EventArgs e);

		public event OnBeginEdit OnBeginEditEvent;

		protected void BeginEditEvent(EventArgs e)
		{
			if (OnBeginEditEvent != null)
				OnBeginEditEvent(this, e);
		}

		public delegate void OnEndEdit(object sender, EventArgs e);

		public event OnEndEdit OnEndEditEvent;

		protected void EndEditEvent(EventArgs e)
		{
			if (OnEndEditEvent != null)
				OnEndEditEvent(this, e);
		}

		public delegate void OnOpenContact(object sender, EventArgs e);

		public event OnOpenContact OnOpenContactEvent;

		protected void OpenContactEvent(EventArgs e)
		{
			if (OnOpenContactEvent != null)
				OnOpenContactEvent(this, e);
		}

		public delegate void OnCloseContact(object sender, EventArgs e);

		public event OnCloseContact OnCloseContactEvent;

		protected void CloseContactEvent(EventArgs e)
		{
			if (OnCloseContactEvent != null)
				OnCloseContactEvent(this, e);
		}

		public delegate void OnChangeView(object sender, EventArgs e);

		public event OnChangeView OnChangeViewEvent;

		protected void ChangeViewEvent(EventArgs e)
		{
			if (OnChangeViewEvent != null)
				OnChangeViewEvent(this, e);
		}

		public delegate void OnDateModified(object sender, DateModifiedEventArgs e);

		public event OnDateModified OnDateModifiedEvent;

		protected void DateModifiedEvent(DateModifiedEventArgs e)
		{
			if (OnDateModifiedEvent != null)
				OnDateModifiedEvent(this, e);
		}

		#endregion
	}

	public class DateModifiedEventArgs : EventArgs
	{
		public DateModifiedEventArgs(DateTime newDate, DateTime? oldDate)
		{
			NewDate = newDate;
			OldDate = oldDate;
		}

		public DateTime NewDate
		{
			get;
			private set;
		}

		public DateTime? OldDate
		{
			get;
			private set;
		}
	}
}
