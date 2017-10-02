using System.ComponentModel;

namespace Modern.FileBrowser
{
	class FilterComboBoxItem : INotifyPropertyChanged
	{
		#region Constructors

		public FilterComboBoxItem(string displayText, string filter)
		{
			DisplayText = displayText;
			Filter = filter;
		}

		#endregion

		#region Properties

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		private string _displayText;

		public string DisplayText
		{
			get { return _displayText; }
			set
			{
				_displayText = value;
				OnPropertyChanged("DisplayText");
			}
		}

		private string _filter;

		public string Filter
		{
			get { return _filter; }
			set
			{
				_filter = value;
				OnPropertyChanged("Filter");
			}
		}

		#endregion
	}
}
