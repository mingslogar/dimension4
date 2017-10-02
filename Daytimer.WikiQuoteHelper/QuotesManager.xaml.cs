using Daytimer.Dialogs;
using Daytimer.Fundamentals;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WikiquoteScreensaver.Common.MultithreadBinding;
using WikiquoteScreensaverLib.Common;
using WikiquoteScreensaverLib.Common.ErrorHandling;
using WikiquoteScreensaverLib.IO.FileIO;
using WikiquoteScreensaverLib.IO.WebIO;
using WikiquoteScreensaverLib.IO.WebIO.CollectorRules;

namespace Daytimer.WikiQuoteHelper
{
	/// <summary>
	/// Interaction logic for QuotesManager.xaml
	/// </summary>
	public partial class QuotesManager : OfficeWindow
	{
		#region Constructors

		public QuotesManager()
		{
			InitializeComponent();

			playlist = new Playlist();
			cultureMapper = new CultureMapper { new EnglishQuoteCollectorRules() };
			playlistSerializer = new XmlPlaylistSerializer(playlist);

			if (File.Exists(QuotesDatabaseFile))
				try { playlistSerializer.LoadAsync(QuotesDatabaseFile); }
				catch (FileNotFoundException) { }

			lookupManager = new LookupManager(playlist, cultureMapper);

			InitializeEventHandlers();

			Loaded += QuotesManager_Loaded;
		}

		#endregion

		#region Public Fields

		/// <summary>
		/// Gets if changes were made to the database.
		/// </summary>
		public bool ChangesMade
		{
			get { return changesMade; }
		}

		public readonly static string QuotesDatabaseFile =
			Daytimer.DatabaseHelpers.Quotes.QuoteDatabase.QuotesAppData + "\\QuotesTopicalDatabase.xml";

		#endregion

		#region Private Fields

		/// <summary>
		/// Stores if the user made any changes to the selected items.
		/// </summary>
		private bool changesMade = false;

		private List<QuotePage> addedPages = new List<QuotePage>();
		private List<QuotePage> removedPages = new List<QuotePage>();

		private readonly LookupManager lookupManager;
		private readonly Playlist playlist;
		private readonly CultureMapper cultureMapper;
		private readonly XmlPlaylistSerializer playlistSerializer;

		#endregion

		#region Private Methods

		private void InitializeEventHandlers()
		{
			lookupManager.TopicNotFound += LookupManager_TopicNotFound;
			lookupManager.TopicAlreadyExists += LookupManager_TopicAlreadyExists;

			lookupManager.QuoteCollector.QuoteCollectingCompleted += QuoteCollector_QuoteCollectingCompleted;
			lookupManager.QuoteCollector.ErrorCollectingQuotes += QuoteCollector_ErrorCollectingQuotes;
			lookupManager.QuoteCollector.NoQuotesCollected += QuoteCollector_NoQuotesCollected;
			lookupManager.QuoteCollector.TopicAmbiguous += QuoteCollector_TopicAmbiguous;
		}

		private void QuotesManager_Loaded(object sender, RoutedEventArgs e)
		{
			listBox.ItemsSource = new DispatchingList<Playlist, QuotePage>(playlist, Dispatcher);
		}

		private void QuoteCollector_QuoteCollectingCompleted(object sender, QuotesCollectingCompletedEventArgs e)
		{
			changesMade = true;
			addedPages.Add(e.QuotePage);
		}

		private void QuoteCollector_ErrorCollectingQuotes(object sender, ErrorCollectingQuotesEventArgs e)
		{
			if (!IsLoaded)
				return;

			TaskDialog td = new TaskDialog(this,
				"Error Collecting Quotes",
				e.Error.Message,
				MessageType.Error);
			td.ShowDialog();
		}

		private void QuoteCollector_NoQuotesCollected(object sender, NoQuotesCollectedEventArgs e)
		{
			if (!IsLoaded)
				return;

			TaskDialog td = new TaskDialog(this,
				"Empty Topic",
				"We found \"" + e.Topic + ",\" but there weren't any quotes listed under that. We'll still add it to your list, but you " +
				(playlist.QuoteCount == 0 ? "might want to add another topic so quotes appear" : "won't see any quotes from this topic") +
				" in your feed.",
				MessageType.Information);
			td.ShowDialog();
		}

		private void QuoteCollector_TopicAmbiguous(object sender, TopicAmbiguousEventArgs e)
		{
			if (!IsLoaded)
				return;

			DisambiguationWindow disambiguation = new DisambiguationWindow(e.TopicChoices, e.Topic);
			disambiguation.Owner = this;

			if (disambiguation.ShowDialog() == true)
			{
				AddTopic(disambiguation.SelectedTopic.TopicName, e.Culture);
			}
		}

		private void LookupManager_TopicNotFound(object sender, TopicNotFoundEventArgs e)
		{
			if (!IsLoaded)
				return;

			TaskDialog td = new TaskDialog(this,
				"Topic Not Found",
				"We couldn't find \"" + e.Topic + "\" in the topic database.",
				MessageType.Error);
			td.ShowDialog();
		}

		private void LookupManager_TopicAlreadyExists(object sender, TopicAlreadyExistsEventArgs e)
		{
			if (!IsLoaded)
				return;

			TaskDialog td = new TaskDialog(this,
				"Duplicate Topic",
				"You already added \"" + e.Topic + "\" to your topics list.",
				MessageType.Error);
			td.ShowDialog();
		}

		private void AddTopic(string topic, CultureInfo culture)
		{
			try
			{
				lookupManager.AddLookup(topic, culture);
			}
			catch (QuotePageNotFoundException e)
			{
				TaskDialog td = new TaskDialog(this,
					"Topic Not Found",
					"We couldn't find \"" + e.Topic + "\" in the topic database.",
					MessageType.Error);
				td.ShowDialog();
			}
		}

		private void topicBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;

				string text = topicBox.Text.Trim();

				if (text.Length > 0)
				{
					AddTopic(text, CultureInfo.GetCultureInfo("en"));
					topicBox.Clear();
				}
			}
		}

		private void delete_Click(object sender, RoutedEventArgs e)
		{
			changesMade = true;

			QuotePage page = (QuotePage)((ContentPresenter)(((FrameworkElement)sender).TemplatedParent)).Content;
			playlist.Remove(page);

			if (!addedPages.Remove(page))
				removedPages.Add(page);
		}

		private async void okButton_Click(object sender, RoutedEventArgs e)
		{
			okButton.IsEnabled = false;
			okButton.Content = "Saving...";

			if (changesMade)
			{
				await Task.Factory.StartNew(() =>
				{
					string dir = Path.GetDirectoryName(QuotesDatabaseFile);

					if (!Directory.Exists(dir))
						Directory.CreateDirectory(dir);
				});

				playlistSerializer.Save(QuotesDatabaseFile);

				// Delete any pages which the user requested deleted.
				foreach (QuotePage each in removedPages)
					await Database.RemoveFromDatabase(each);

				// Add any new pages.
				await Database.AddToDatabase(addedPages);
			}

			DialogResult = true;
		}

		#endregion
	}
}
