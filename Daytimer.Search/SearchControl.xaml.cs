using Daytimer.DatabaseHelpers;
using Daytimer.DockableDialogs;
using Daytimer.Functions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Daytimer.Search
{
	/// <summary>
	/// Interaction logic for SearchControl.xaml
	/// </summary>
	public partial class SearchControl : DockContent
	{
		public SearchControl()
		{
			InitializeComponent();
		}

		#region Search

		private string SearchString;

		/// <summary>
		/// The maximum number of search results to display on each pass.
		/// </summary>
		private int MaxResults
		{
			get { return Settings.MaxSearchResults; }
		}

		private SearchFilter Filter = SearchFilter.All;

		/// <summary>
		/// Store a copy of the search results if we are showing a "More" button.
		/// </summary>
		private List<SearchResult> SearchResults;

		/// <summary>
		/// The number of results that are currently being shown.
		/// </summary>
		private int CurrentResultsNumber = 0;

		private Search search;

		private void GetResults()
		{
			if (search != null)
			{
				search.OnSearchCompletedEvent -= search_OnSearchCompletedEvent;
				search.Stop();
			}

			if (!string.IsNullOrWhiteSpace(SearchString))
			{
				statusText.Text = "Searching...";

				search = new Search(SearchString, Filter);
				search.OnSearchCompletedEvent += search_OnSearchCompletedEvent;
				search.GetSearchResults();
			}
			else
			{
				statusText.Text = "No search results.";

				DoubleAnimation fadeOutBoxAnim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration);
				fadeOutBoxAnim.EasingFunction = AnimationHelpers.EasingFunction;
				fadeOutBoxAnim.Completed += fadeOutBoxAnim_Completed;
				searchResults.BeginAnimation(OpacityProperty, fadeOutBoxAnim);
			}
		}

		private void search_OnSearchCompletedEvent(object sender, SearchEventArgs e)
		{
			SearchResults = e.Data;
			Dispatcher.BeginInvoke(ShowResults);
		}

		private void ShowResults()
		{
			try
			{
				searchResults.Items.Clear();
				int count = SearchResults.Count;

				CurrentResultsNumber = 0;
				int numAdded = 0;

				if (count > 0)
				{
					for (int i = 0; i < count; i++)
					{
						SearchResult each = SearchResults[i];

						if (numAdded < MaxResults)
						{
							searchResults.Items.Add(searchResultItem(each));
							numAdded++;
						}
						else
						{
							int next = Math.Min(count - numAdded, MaxResults);
							searchResults.Items.Add(moreLink(next));
							break;
						}
					}

					CurrentResultsNumber += numAdded;

					searchResults.ScrollIntoView(searchResults.Items[0]);

					DoubleAnimation boxAnim = new DoubleAnimation(1, AnimationHelpers.AnimationDuration);
					boxAnim.EasingFunction = AnimationHelpers.EasingFunction;
					searchResults.BeginAnimation(OpacityProperty, boxAnim);
				}
				else
				{
					statusText.Text = "No search results.";

					DoubleAnimation boxAnim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration);
					boxAnim.EasingFunction = AnimationHelpers.EasingFunction;
					searchResults.BeginAnimation(OpacityProperty, boxAnim);
				}
			}
			catch (ArgumentOutOfRangeException)
			{
				// The list was cleared while we were iterating through it.
			}
		}

		/// <summary>
		/// Gets if the specified query will be fast enough to perform an instant search.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		private bool InstantRecommended(string query)
		{
			if (!Settings.InstantSearchCap)
				return true;

			double systemRating = SystemStatistics.Rating;
			int maxLength = 0;

			if (systemRating < 2)
				maxLength = 5;
			else if (systemRating < 3)
				maxLength = 10;
			else if (systemRating < 4)
				maxLength = 15;
			else if (systemRating < 10)
				maxLength = 20;
			else
				maxLength = 30;

			return query.Length < maxLength;
		}

		private void fadeOutBoxAnim_Completed(object sender, EventArgs e)
		{
			searchResults.Items.Clear();
		}

		private ListBoxItem searchResultItem(SearchResult item)
		{
			SearchResultUI result = new SearchResultUI();
			result.QueryString = SearchString;
			result.Result = item;
			result.Selected += item_Selected;
			result.MouseDoubleClick += item_MouseDoubleClick;

			return result;
		}

		private void ShowSelection(SearchResultUI item)
		{
			SearchResult result = item.Result;

			if (result.RepresentingObject == RepresentingObject.Appointment)
			{
				//Appointment appointment = AppointmentDatabase.GetAppointment(result.ID);

				//if (!appointment.IsRepeating)
				if (!result.Recurring)
					NavigateAppointmentEvent(new NavigateAppointmentEventArgs(result.Date.Value, result.ID));
				else
				{
					Appointment appointment = AppointmentDatabase.GetAppointment(result.ID);

					DateTime now = DateTime.Now.Date;

					DateTime? date = appointment.GetNextRecurrence(now.AddDays(-1));

					if (date == null)
						date = appointment.GetPreviousRecurrence(now.AddDays(1));

					NavigateAppointmentEvent(new NavigateAppointmentEventArgs(date.Value, result.ID));
				}
			}
			else if (result.RepresentingObject == RepresentingObject.Contact)
				NavigateContactEvent(new NavigateContactEventArgs(result.ID));
			else if (result.RepresentingObject == RepresentingObject.Task)
				NavigateTaskEvent(new NavigateTaskEventArgs(result.ID));
			else if (result.RepresentingObject == RepresentingObject.Note)
				NavigateNoteEvent(new NavigateNoteEventArgs(result.ID));
		}

		private ListBoxItem moreLink(int count)
		{
			ListBoxItem item = new ListBoxItem();
			item.Content = "Show " + count.ToString() + " more " + (count == 1 ? "result" : "results");

			item.Selected += moreResults_Selected;

			return item;
		}

		#endregion

		#region UI

		private void item_Selected(object sender, RoutedEventArgs e)
		{
			ShowSelection(sender as SearchResultUI);
		}

		private void item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			ShowSelection(sender as SearchResultUI);
		}

		private void moreResults_Selected(object sender, RoutedEventArgs e)
		{
			// Remove the "More results" button
			searchResults.Items.Remove(sender);

			int count = SearchResults.Count;
			int end = Math.Min(count, MaxResults + CurrentResultsNumber);

			for (int i = CurrentResultsNumber; i < end; i++)
				searchResults.Items.Add(searchResultItem(SearchResults[i]));

			CurrentResultsNumber = end;

			if (CurrentResultsNumber < count)
			{
				int next = Math.Min(count - CurrentResultsNumber, MaxResults);
				searchResults.Items.Add(moreLink(next));
			}
		}

		private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			searchBoxWatermark.Visibility = string.IsNullOrEmpty(searchBox.Text) ? Visibility.Visible : Visibility.Hidden;

			if (InstantRecommended(searchBox.Text))
			{
				SearchString = searchBox.Text;
				GetResults();
			}
			else
			{
				if (search != null)
				{
					search.OnSearchCompletedEvent -= search_OnSearchCompletedEvent;
					search.Stop();
				}

				searchResults.Items.Clear();
				statusText.Text = "Press 'Enter' to search.";

				DoubleAnimation boxAnim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration);
				boxAnim.EasingFunction = AnimationHelpers.EasingFunction;
				searchResults.BeginAnimation(OpacityProperty, boxAnim);
			}
		}

		private void searchBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (SearchString != searchBox.Text)
				{
					SearchString = searchBox.Text;
					GetResults();
				}
			}
		}

		private SearchFilterWindow filterWindow = null;

		private void filterButton_Click(object sender, RoutedEventArgs e)
		{
			if (filterWindow != null && filterWindow.IsOpen)
				filterWindow.FastClose();
			else
			{
				filterWindow = new SearchFilterWindow(filterButton);
				filterWindow.SearchFilter = Filter;
				filterWindow.Owner = Window.GetWindow(this);
				filterWindow.Closed += filterWindow_Closed;
				filterWindow.Show();
			}
		}

		private void filterWindow_Closed(object sender, EventArgs e)
		{
			SearchFilter _new = (sender as SearchFilterWindow).SearchFilter;

			if (_new == Filter)
				return;

			searchBoxWatermark.Text = _new != SearchFilter.All ? "Search " + _new.ToString() : "Search";
			Filter = _new;
			GetResults();
		}

		#endregion

		#region Events

		public delegate void OnNavigateAppointment(object sender, NavigateAppointmentEventArgs e);

		public event OnNavigateAppointment OnNavigateAppointmentEvent;

		protected void NavigateAppointmentEvent(NavigateAppointmentEventArgs e)
		{
			if (OnNavigateAppointmentEvent != null)
				OnNavigateAppointmentEvent(this, e);
		}

		public delegate void OnNavigateContact(object sender, NavigateContactEventArgs e);

		public event OnNavigateContact OnNavigateContactEvent;

		protected void NavigateContactEvent(NavigateContactEventArgs e)
		{
			if (OnNavigateContactEvent != null)
				OnNavigateContactEvent(this, e);
		}

		public delegate void OnNavigateTask(object sender, NavigateTaskEventArgs e);

		public event OnNavigateTask OnNavigateTaskEvent;

		protected void NavigateTaskEvent(NavigateTaskEventArgs e)
		{
			if (OnNavigateTaskEvent != null)
				OnNavigateTaskEvent(this, e);
		}

		public delegate void OnNavigateNote(object sender, NavigateNoteEventArgs e);

		public event OnNavigateNote OnNavigateNoteEvent;

		protected void NavigateNoteEvent(NavigateNoteEventArgs e)
		{
			if (OnNavigateNoteEvent != null)
				OnNavigateNoteEvent(this, e);
		}

		#endregion
	}

	public class NavigateAppointmentEventArgs : EventArgs
	{
		public NavigateAppointmentEventArgs(DateTime date, string id)
		{
			_date = date;
			_id = id;
		}

		private DateTime _date;
		private string _id;

		public DateTime Date
		{
			get { return _date; }
		}

		public string ID
		{
			get { return _id; }
		}
	}

	public class NavigateContactEventArgs : EventArgs
	{
		public NavigateContactEventArgs(string id)
		{
			_id = id;
		}

		private string _id;

		public string ID
		{
			get { return _id; }
		}
	}

	public class NavigateTaskEventArgs : EventArgs
	{
		public NavigateTaskEventArgs(string id)
		{
			_id = id;
		}

		private string _id;

		public string ID
		{
			get { return _id; }
		}
	}

	public class NavigateNoteEventArgs : EventArgs
	{
		public NavigateNoteEventArgs(string id)
		{
			_id = id;
		}

		private string _id;

		public string ID
		{
			get { return _id; }
		}
	}
}
