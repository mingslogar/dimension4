// 
// This file is part of WikiquoteScreensaverLib.
//
// WikiquoteScreensaverLib is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// WikiquoteScreensaverLib is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with WikiquoteScreensaverLib.  If not, see <http://www.gnu.org/licenses/>.
//
// Filename: QuotePage.cs
// Copyright: Christian Hanser 2008
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace WikiquoteScreensaverLib.Common
{
	/// <summary>
	/// A collection class containing all quotes that belong to a specific topic.
	/// </summary>
	[ComVisible(false)]
	[DataContract(Name = "QuotePage")]
	public sealed class QuotePage
		: SelectablesListBase<string, SelectableQuote, SelectableQuoteCollection>, IList<SelectableQuote>,
		IEquatable<QuotePage>, ISelectable
	{
		[DataMember(Name = "Topic")]
		string _topic;

		[DataMember(Name = "Uri")]
		Uri _uri;

		[DataMember(Name = "Selected")]
		bool _selected = true;

		[DataMember(Name = "Culture")]
		readonly CultureInfo _culture;

		[DataMember(Name = "TopicTranslations", EmitDefaultValue = false)]
		TopicTranslation[] _topicTranslations;

		/// <summary>
		/// Initializes a new QuotePage instance.
		/// </summary>
		/// <param name="topic">The topic string.</param>
		/// <param name="uri">The official wikiquote URI.</param>
		/// <param name="culture">The culture corresponding to the language of the quote strings.</param>
		/// <param name="quotes">The collection of quotes.</param>
		/// <param name="topicLanguageData">Contains topic translations for other languages.</param>
		internal QuotePage(string topic, Uri uri, CultureInfo culture, SelectableQuoteCollection quotes,
			TopicTranslation[] topicTranslations)
			: base(quotes)
		{
			_topic = topic;
			_uri = uri;
			_culture = culture;
			TopicTranslations = topicTranslations;
			LastUpdate = DateTime.Now;
		}

		/// <summary>
		/// Method used for firing a INotifyCollectionChanged.CollectionChanged event.
		/// </summary>
		/// <param name="e">The event's arguments.</param>
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnCollectionChanged(e);

			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				LastUpdate = DateTime.Now;
				OnPropertyChanged("LastUpdate");
			}
		}

		/// <summary>
		/// Returns the quote page's topic.
		/// </summary>
		/// <returns>The topic string.</returns>
		public override string ToString()
		{
			return Topic;
		}

		/// <summary>
		/// Gets or sets the quote page's topic.
		/// </summary>
		public string Topic
		{
			get
			{
				return _topic;
			}
			internal set
			{
				if (_topic != value)
				{
					_topic = value;
					OnPropertyChanged("Topic");
				}
			}
		}

		/// <summary>
		/// Gets or sets the time of the quote page's last update.
		/// </summary>
		[DataMember(Name = "LastUpdate")]
		public DateTime LastUpdate { get; private set; }

		/// <summary>
		/// Gets or sets the official wikiquote URI.
		/// </summary>
		public Uri Uri
		{
			get
			{
				return _uri;
			}
			internal set
			{
				if (_uri != value)
				{
					_uri = value;
					OnPropertyChanged("Uri");
				}
			}
		}

		/// <summary>
		/// Gets the culture corresponding to the quotes' language.
		/// </summary>
		public CultureInfo Culture
		{
			get { return _culture; }
		}

		/// <summary>
		/// Gets or sets the available translations of a topic into other available languages.
		/// </summary>
		public TopicTranslation[] TopicTranslations
		{
			get
			{
				return _topicTranslations;
			}
			internal set
			{
				_topicTranslations = value;
				OnPropertyChanged("TopicTranslations");
			}
		}

		public override SelectableQuote this[int index]
		{
			get { return base[index]; }
		}

		public override bool IsReadOnly
		{
			get { return true; }
		}

		#region ICollection<SelectableQuote> Members
		void ICollection<SelectableQuote>.Clear()
		{
			throw new NotSupportedException();
		}

		void ICollection<SelectableQuote>.Add(SelectableQuote item)
		{
			throw new NotSupportedException();
		}

		bool ICollection<SelectableQuote>.Remove(SelectableQuote item)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region IList<SelectableQuote> Members
		void IList<SelectableQuote>.Insert(int index, SelectableQuote item)
		{
			throw new NotSupportedException();
		}

		void IList<SelectableQuote>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		SelectableQuote IList<SelectableQuote>.this[int index]
		{
			get { return base[index]; }
			set { throw new NotSupportedException(); }
		}
		#endregion

		#region ISelectable Members

		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				if (_selected != value)
				{
					_selected = value;
					OnPropertyChanged("Selected");
				}
			}
		}

		#endregion

		#region IEquatable<QuotePage> Members

		public bool Equals(QuotePage other)
		{
			return (other != null && Uri == other.Uri);
		}

		#endregion
	}
}
