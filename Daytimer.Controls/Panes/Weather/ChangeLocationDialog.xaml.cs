using Daytimer.Dialogs;
using Daytimer.Functions;
using System;
using System.Net;
using System.Net.Cache;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace Daytimer.Controls.Panes.Weather
{
	/// <summary>
	/// Interaction logic for ChangeLocationDialog.xaml
	/// </summary>
	public partial class ChangeLocationDialog : DialogBase
	{
		public ChangeLocationDialog(bool isHomeLocation)
		{
			InitializeComponent();
			_isHomeLocation = isHomeLocation;
			ContentRendered += ChangeLocationDialog_ContentRendered;
		}

		private bool _isHomeLocation = false;

		private bool _loadFirstTime = true;

		private void ChangeLocationDialog_ContentRendered(object sender, EventArgs e)
		{
			if (!_loadFirstTime)
				return;

			_loadFirstTime = false;

			textBox = ((TextBox)comboBox.Template.FindName("PART_EditableTextBox", comboBox));
			textBox.TextChanged += textBox_TextChanged;
			textBox.Focus();
		}

		private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			okButton.IsEnabled = comboBox.SelectedIndex != -1;
		}

		private TextBox textBox;

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if (!_isHomeLocation)
			{
				string[] favs = Settings.WeatherFavorites;

				if (favs != null)
				{
					string loc = comboBox.Text.ToLower().Trim();

					foreach (string each in favs)
						if (each.ToLower() == loc)
						{
							TaskDialog td = new TaskDialog(this, "Location is in use", "You already have " + textBox.Text + " in your favorites.", MessageType.Error);
							td.ShowDialog();
							textBox.SelectAll();
							return;
						}
				}
			}

			if (comboBox.SelectedIndex == -1)
			{
				TaskDialog td = new TaskDialog(this, "Location unavailable", textBox.Text + " is not a recognized city.", MessageType.Error);
				td.ShowDialog();
				textBox.SelectAll();
				return;
			}

			DialogResult = true;
		}

		public string LocationReference
		{
			get { return (comboBox.SelectedItem as ComboBoxItem).Tag.ToString(); }
		}

		#region Location Searching

		private void textBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (comboBox.SelectedIndex != -1)
				return;

			if (!string.IsNullOrWhiteSpace(textBox.Text))
				Autocomplete();
			else
			{
				comboBox.Items.Clear();
				comboBox.IsDropDownOpen = false;
				errorMsg.Visibility = Visibility.Hidden;
			}
		}

		private WebClient autocompleteClient;

		private void Autocomplete()
		{
			if (autocompleteClient != null)
			{
				autocompleteClient.DownloadStringCompleted -= autocompleteClient_DownloadStringCompleted;
				autocompleteClient.CancelAsync();
			}

			autocompleteClient = new WebClient();
			autocompleteClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);
			Uri uri = new Uri("https://maps.googleapis.com/maps/api/place/autocomplete/xml?input="
				+ textBox.Text + "&sensor=false&key=" + GlobalData.GoogleDataAPIKey + "&offset="
				+ textBox.CaretIndex.ToString()
				+ "&types=(cities)");
			autocompleteClient.DownloadStringCompleted += autocompleteClient_DownloadStringCompleted;
			autocompleteClient.DownloadStringAsync(uri);
		}

		private void autocompleteClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			if (e.Cancelled)
				return;

			if (e.Error == null)
			{
				errorMsg.Visibility = Visibility.Hidden;

				try
				{
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(e.Result);

					XmlNodeList predictions = xmlDoc.SelectNodes("/AutocompletionResponse/prediction/description");

					int caretIndex = textBox.CaretIndex;

					comboBox.Items.Clear();

					foreach (XmlNode each in predictions)
					{
						ComboBoxItem item = new ComboBoxItem();
						item.Content = each.InnerText;
						item.Tag = each.ParentNode.SelectSingleNode("reference").InnerText;
						comboBox.Items.Add(item);
					}

					comboBox.IsDropDownOpen = comboBox.HasItems;

					comboBox.UpdateLayout();
					textBox.CaretIndex = caretIndex;
				}
				catch
				{
					errorMsg.Visibility = Visibility.Visible;
				}
			}
			else
			{
				errorMsg.Visibility = Visibility.Visible;
			}
		}

		#endregion
	}
}
