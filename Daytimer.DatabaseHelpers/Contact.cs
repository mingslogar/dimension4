using Daytimer.DatabaseHelpers.Contacts;
using Daytimer.DatabaseHelpers.Recovery;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Thought.vCards;

namespace Daytimer.DatabaseHelpers
{
	[Serializable]
	public class Contact : DatabaseObject, INotifyPropertyChanged
	{
		#region Constructors

		static Contact()
		{
			try
			{
				DefaultTile = new Uri("pack://application:,,,/Daytimer.Images;component/Images/defaulttile.png", UriKind.Absolute);
			}
			catch { }
		}

		public Contact()
			: base()
		{

		}

		public Contact(bool generateID)
			: base(generateID)
		{

		}

		public Contact(bool generateID, bool saveChangesToDisk)
			: base(generateID)
		{
			_saveChangesToDisk = saveChangesToDisk;
		}

		public Contact(Contact contact)
		{
			CopyFrom(contact);
		}

		public Contact(Contact contact, bool saveChangesToDisk)
		{
			CopyFrom(contact);
			_saveChangesToDisk = saveChangesToDisk;
		}

		#endregion

		#region Properties

		private string _name = "";
		private string _work = "";
		private string _email = "";
		private string _website = "";
		private string _im = "";
		private string _phone = "";
		private string _address = "";
		private string _tile = "";
		private BitmapSource _decodedTile = null;
		private string _specialDate = "";
		private Gender _gender = Gender.Unknown;
		private bool _readOnly = false;
		private bool _private = false;

		private bool _saveChangesToDisk = true;
		private FlowDocument _notesDocument = null;

		public Name Name
		{
			get
			{
				if (_name == "")
					return new Name();

				return Name.Deserialize(_name);
			}
			set
			{
				if (value == null)
				{
					_name = "";
					return;
				}

				_name = value.ToSerializedString();

				OnPropertyChanged("Name");
			}
		}

		public Work Work
		{
			get
			{
				if (_work == "")
					return null;

				return Work.Deserialize(_work);
			}
			set
			{
				_work = value.ToSerializedString();
				OnPropertyChanged("WorkDescription");
			}
		}

		public Email[] Emails
		{
			get
			{
				if (_email == "")
					return null;

				string[] split = _email.Split('\\');
				int length = split.Length;

				Email[] e = new Email[length];

				for (int i = 0; i < length; i++)
					e[i] = Email.Deserialize(split[i]);

				return e;
			}
			set
			{
				if (value.Length == 0)
				{
					_email = "";
					return;
				}

				string e = "";

				foreach (Email each in value)
					e += each.ToSerializedString() + '\\';

				_email = e.Remove(e.Length - 1);
			}
		}

		public Website[] Websites
		{
			get
			{
				if (_website == "")
					return null;

				string[] split = _website.Split('\\');
				int length = split.Length;

				Website[] w = new Website[length];

				for (int i = 0; i < length; i++)
					w[i] = Website.Deserialize(split[i]);

				return w;
			}
			set
			{
				if (value.Length == 0)
				{
					_website = "";
					return;
				}

				string w = "";

				foreach (Website each in value)
					w += each.ToSerializedString() + '\\';

				_website = w.Remove(w.Length - 1);
			}
		}

		public IM[] IM
		{
			get
			{
				if (_im == "")
					return null;

				string[] split = _im.Split('\\');
				int length = split.Length;

				IM[] im = new IM[length];

				for (int i = 0; i < length; i++)
					im[i] = Daytimer.DatabaseHelpers.Contacts.IM.Deserialize(split[i]);

				return im;
			}
			set
			{
				if (value.Length == 0)
				{
					_im = "";
					return;
				}

				string w = "";

				foreach (IM each in value)
					w += each.ToSerializedString() + '\\';

				_im = w.Remove(w.Length - 1);
			}
		}

		public PhoneNumber[] PhoneNumbers
		{
			get
			{
				if (_phone == "")
					return null;

				string[] split = _phone.Split('\\');
				int length = split.Length;

				PhoneNumber[] p = new PhoneNumber[length];

				for (int i = 0; i < length; i++)
					p[i] = PhoneNumber.Deserialize(split[i]);

				return p;
			}
			set
			{
				if (value.Length == 0)
				{
					_phone = "";
					return;
				}

				string p = "";

				foreach (PhoneNumber each in value)
					p += each.ToSerializedString() + '\\';

				_phone = p.Remove(p.Length - 1);
			}
		}

		public Address[] Addresses
		{
			get
			{
				if (_address == "")
					return null;

				string[] split = _address.Split('\\');
				int length = split.Length;

				Address[] a = new Address[length];

				for (int i = 0; i < length; i++)
					a[i] = Address.Deserialize(split[i]);

				return a;
			}
			set
			{
				if (value.Length == 0)
				{
					_address = "";
					return;
				}

				string a = "";

				foreach (Address each in value)
					a += each.ToSerializedString() + '\\';

				_address = a.Remove(a.Length - 1);
			}
		}

		public SpecialDate[] SpecialDates
		{
			get
			{
				if (_specialDate == "")
					return null;

				string[] split = _specialDate.Split('\\');
				int length = split.Length;

				SpecialDate[] d = new SpecialDate[length];

				for (int i = 0; i < length; i++)
					d[i] = SpecialDate.Deserialize(split[i]);

				return d;
			}
			set
			{
				if (value.Length == 0)
				{
					_specialDate = "";
					return;
				}

				string d = "";

				foreach (SpecialDate each in value)
					d += each.ToSerializedString() + '\\';

				_specialDate = d.Remove(d.Length - 1);
			}
		}

		public Gender Gender
		{
			get { return _gender; }
			set { _gender = value; }
		}

		public bool ReadOnly
		{
			get { return _readOnly; }
			set { _readOnly = value; }
		}

		public bool Private
		{
			get { return _private; }
			set { _private = value; }
		}

		// We don't want to run multiple threads for the same image.
		private bool _isDecoding = false;

		public BitmapSource Tile
		{
			get
			{
				if (_decodedTile != null)
					return _decodedTile;

				if (_tile != "")
				{
					if (!_isDecoding)
					{
						_isDecoding = true;
						Task.Factory.StartNew(decodeTile);
					}
				}

				return DefaultTileBitmap;
			}
			set
			{
				if (_decodedTile == value)
					return;

				_decodedTile = value;

				OnPropertyChanged("Tile");

				if (value == null || (value is BitmapImage && ((BitmapImage)value).UriSource == DefaultTile))
				{
					_tile = "";
					return;
				}

				//byte[] pixels = new byte[value.PixelWidth * value.PixelHeight * 4];
				//value.CopyPixels(pixels, value.PixelWidth * 4, 0);

				JpegBitmapEncoder encoder = new JpegBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(value));

				MemoryStream stream = new MemoryStream();
				encoder.Save(stream);

				Task.Factory.StartNew(() => { encodeTile(stream.GetBuffer()); });
			}
		}

		/// <summary>
		/// This function takes approximately 10 milliseconds to run.
		/// </summary>
		private void decodeTile()
		{
			byte[] pixels = Convert.FromBase64String(_tile);

			using (MemoryStream compressed = new MemoryStream(pixels))
			{
				using (MemoryStream uncompressed = new MemoryStream())
				{
					using (DeflateStream gz = new DeflateStream(compressed, CompressionMode.Decompress))
					{
						gz.CopyTo(uncompressed);
						gz.Close();
					}

					pixels = uncompressed.GetBuffer();
					uncompressed.Close();
				}

				compressed.Close();
			}

			BitmapImage tile = new BitmapImage();
			tile.BeginInit();
			tile.StreamSource = new MemoryStream(pixels);
			tile.EndInit();

			_decodedTile = tile;//BitmapSource.Create(96, 96, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256Transparent, pixels, 96 * 4);
			_decodedTile.Freeze();

			Application.Current.Dispatcher.BeginInvoke(() => { OnPropertyChanged("Tile"); });

			_isDecoding = false;
		}

		private void encodeTile(byte[] pixels)
		{
			using (MemoryStream uncompressed = new MemoryStream(pixels))
			{
				using (MemoryStream compressed = new MemoryStream())
				{
					using (DeflateStream gz = new DeflateStream(compressed, CompressionMode.Compress))
					{
						uncompressed.CopyTo(gz);
						gz.Close();
					}

					pixels = compressed.GetBuffer();
					compressed.Close();
				}

				uncompressed.Close();
			}

			_tile = Convert.ToBase64String(pixels);
		}

		/// <summary>
		/// Gets the text contents of the notes document.
		/// </summary>
		public string Notes
		{
			get
			{
				FlowDocument doc = NotesDocument;

				if (doc != null)
					return new TextRange(doc.ContentStart, doc.ContentEnd).Text;
				else
					return null;
			}
		}

		/// <summary>
		/// Gets the notes document.
		/// </summary>
		public FlowDocument NotesDocument
		{
			get
			{
				if (_saveChangesToDisk)
					return new FlowDocumentStorage(ContactDatabase.ContactsAppData + "\\" + _id).DocumentValue;
				else
					return (FlowDocument)_notesDocument;
			}
			set
			{
				if (_saveChangesToDisk)
					new FlowDocumentStorage(ContactDatabase.ContactsAppData + "\\" + _id).DocumentValue = value;
				else
					_notesDocument = value;
			}
		}

		/// <summary>
		/// Get the notes document asynchronously.
		/// </summary>
		/// <returns></returns>
		public async Task<FlowDocument> GetNotesDocumentAsync()
		{
			if (_saveChangesToDisk)
				return await new FlowDocumentStorage(ContactDatabase.ContactsAppData + "\\" + _id).GetDocumentValueAsync();
			else
				return _notesDocument;
		}

		/// <summary>
		/// Set the notes document asynchronously.
		/// </summary>
		/// <returns></returns>
		public async Task SetNotesDocumentAsync(FlowDocument value)
		{
			if (_saveChangesToDisk)
				await new FlowDocumentStorage(ContactDatabase.ContactsAppData + "\\" + _id).SetDocumentValueAsync(value);
			else
				_notesDocument = value;
		}

		#region Raw data

		public string RawName
		{
			get { return _name; }
			set { _name = value; }
		}

		public string RawEmail
		{
			get { return _email; }
			set { _email = value; }
		}

		public string RawWork
		{
			get { return _work; }
			set { _work = value; }
		}

		public string RawWebsite
		{
			get { return _website; }
			set { _website = value; }
		}

		public string RawIM
		{
			get { return _im; }
			set { _im = value; }
		}

		public string RawPhone
		{
			get { return _phone; }
			set { _phone = value; }
		}

		public string RawAddress
		{
			get { return _address; }
			set { _address = value; }
		}

		public string RawSpecialDate
		{
			get { return _specialDate; }
			set { _specialDate = value; }
		}

		public string RawTile
		{
			get { return _tile; }
			set { _tile = value; }
		}

		#endregion

		private static BitmapImage _cachedDefaultTileBitmap = null;

		private static BitmapImage DefaultTileBitmap
		{
			get
			{
				if (_cachedDefaultTileBitmap == null)
				{
					_cachedDefaultTileBitmap = new BitmapImage(DefaultTile);
					_cachedDefaultTileBitmap.Freeze();
				}

				return _cachedDefaultTileBitmap;
			}
		}

		public static Uri DefaultTile;

		public bool SaveChangesToDisk
		{
			get { return _saveChangesToDisk; }
			set { _saveChangesToDisk = value; }
		}

		#endregion

		#region Methods

		private void CopyFrom(Contact contact)
		{
			base.CopyFrom(contact);

			_name = contact._name;
			_work = contact._work;
			_email = contact._email;
			_website = contact._website;
			_im = contact._im;
			_phone = contact._phone;
			_address = contact._address;
			_tile = contact._tile;
			_specialDate = contact._specialDate;
			_private = contact._private;
			_readOnly = contact._readOnly;
		}

		public void CopyPropertiesFrom(Contact contact)
		{
			base.CopyFrom(contact);

			if (_name != contact._name)
			{
				_name = contact._name;
				OnPropertyChanged("Name");
			}

			if (_work != contact._work)
			{
				_work = contact._work;
				OnPropertyChanged("WorkDescription");
			}

			_email = contact._email;
			_website = contact._website;
			_im = contact._im;
			_phone = contact._phone;
			_address = contact._address;

			if (_tile != contact._tile)
			{
				_tile = contact._tile;
				OnPropertyChanged("Tile");
			}

			_specialDate = contact._specialDate;
			_private = contact._private;
			_readOnly = contact._readOnly;
		}

		/// <summary>
		/// Gets if this contact matches a specified query.
		/// </summary>
		/// <param name="query">The query string.</param>
		/// <param name="type">The type of query to perform.</param>
		/// <returns></returns>
		public bool MatchesQuery(string query, QueryType type)
		{
			switch (type)
			{
				case QueryType.AllWords:
					return MatchesQueryAllWords(query.Split(' '));

				case QueryType.AnyWord:
					return MatchesQueryAnyWord(query.Split(' '));

				case QueryType.ExactMatch:
					return MatchesQueryExactMatch(query);

				default:
					return false;
			}
		}

		/// <summary>
		/// Gets if this contact exactly matches a specified query.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public bool MatchesQueryExactMatch(string query)
		{
			string name = Name.ToString().ToLower();
			string work = WorkDescription.ToLower();
			string gender = _gender.ToString();
			string details = Notes.StripWhitespace();
			Email[] email = Emails;
			Website[] website = Websites;
			PhoneNumber[] phone = PhoneNumbers;
			Address[] address = Addresses;
			IM[] im = IM;
			SpecialDate[] date = SpecialDates;

			if (name.Contains(query))
				return true;

			if (work.Contains(query))
				return true;

			if (gender.Contains(query))
				return true;

			if (details.Contains(query))
				return true;

			if (email != null)
			{
				foreach (Email em in email)
				{
					if (em.Address.ToLower().Contains(query))
						return true;
				}
			}

			if (website != null)
			{
				foreach (Website ws in website)
				{
					if (ws.Url.ToLower().Contains(query))
						return true;
				}
			}

			if (phone != null)
			{
				foreach (PhoneNumber pn in phone)
				{
					if (pn.Number.ToLower().Contains(query))
						return true;
				}
			}

			if (address != null)
			{
				foreach (Address ad in address)
				{
					if (ad.ToString().ToLower().Contains(query))
						return true;
				}
			}

			if (im != null)
			{
				foreach (IM i in im)
				{
					if (i.Address.ToLower().Contains(query))
						return true;
				}
			}

			if (date != null)
			{
				foreach (SpecialDate sd in date)
				{
					if (sd.Date.ToString("MMMM d yyyy").Contains(query))
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets if this contact matches all words in a specified query.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public bool MatchesQueryAllWords(string[] query)
		{
			string name = Name.ToString().ToLower();
			string work = WorkDescription.ToLower();
			string gender = _gender.ToString();
			string details = Notes.StripWhitespace();
			Email[] email = Emails;
			Website[] website = Websites;
			PhoneNumber[] phone = PhoneNumbers;
			Address[] address = Addresses;
			IM[] im = IM;
			SpecialDate[] date = SpecialDates;

			foreach (string q in query)
			{
				if (name.Contains(q))
					continue;

				if (work.Contains(q))
					continue;

				if (gender.Contains(q))
					continue;

				if (details.Contains(q))
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

				if (im != null)
				{
					foreach (IM i in im)
					{
						if (i.Address.ToLower().Contains(q))
						{
							isValid = true;
							break;
						}
					}
				}

				if (date != null)
				{
					foreach (SpecialDate sd in date)
					{
						if (sd.Date.ToString("MMMM d yyyy").ToLower().Contains(q))
						{
							isValid = true;
							break;
						}
					}
				}

				return false;
			}

			return true;
		}

		/// <summary>
		/// Gets if this contact matches any word in a specified query.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public bool MatchesQueryAnyWord(string[] query)
		{
			string name = Name.ToString().ToLower();
			string work = WorkDescription.ToLower();
			string gender = _gender.ToString();
			string details = Notes.StripWhitespace();
			Email[] email = Emails;
			Website[] website = Websites;
			PhoneNumber[] phone = PhoneNumbers;
			Address[] address = Addresses;
			IM[] im = IM;
			SpecialDate[] date = SpecialDates;

			foreach (string q in query)
			{
				if (name.Contains(q))
					return true;

				if (work.Contains(q))
					return true;

				if (gender.Contains(q))
					return true;

				if (details.Contains(q))
					return true;

				if (email != null)
				{
					foreach (Email em in email)
					{
						if (em.Address.ToLower().Contains(q))
							return true;
					}
				}

				if (website != null)
				{
					foreach (Website ws in website)
					{
						if (ws.Url.ToLower().Contains(q))
							return true;
					}
				}

				if (phone != null)
				{
					foreach (PhoneNumber pn in phone)
					{
						if (pn.Number.ToLower().Contains(q))
							return true;
					}
				}

				if (address != null)
				{
					foreach (Address ad in address)
					{
						if (ad.ToString().ToLower().Contains(q))
							return true;
					}
				}

				if (im != null)
				{
					foreach (IM i in im)
					{
						if (i.Address.ToLower().Contains(q))
							return true;
					}
				}

				if (date != null)
				{
					foreach (SpecialDate sd in date)
					{
						if (sd.Date.ToString("MMMM d yyyy").ToLower().Contains(q))
							return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Parses a vCard contact.
		/// </summary>
		/// <param name="vCardFile"></param>
		/// <returns></returns>
		public static List<Contact> ParseVCard(string vCardFile)
		{
			using (StreamReader streamReader = new StreamReader(vCardFile))
			{
				return Parse(streamReader);
			}
		}

		/// <summary>
		/// Parses a vCard contact.
		/// </summary>
		/// <param name="vCardData"></param>
		/// <returns></returns>
		public static List<Contact> ParseVCard(TextReader vCardData)
		{
			return Parse(vCardData);
		}

		private static List<Contact> Parse(TextReader reader)
		{
			vCardReader vcReader = new vCardStandardReader();
			List<vCard> data = vcReader.ReadList(reader);

			List<Contact> contacts = new List<Contact>(data.Count);

			foreach (vCard each in data)
				contacts.Add(ParseVCard(each));

			return contacts;
		}

		/// <summary>
		/// Parses a vCard contact
		/// </summary>
		/// <param name="vCard"></param>
		/// <returns></returns>
		private static Contact ParseVCard(vCard vCard)
		{
			Contact contact = new Contact(false);
			contact.ID = vCard.UniqueId;

			if (contact.ID == string.Empty)
				contact.ID = IDGenerator.GenerateID();

			#region Name

			{
				Name name = Name.TryParse(vCard.FormattedName);

				if (name == null)
					name = new Name();

				if (vCard.GivenName != string.Empty)
					name.FirstName = vCard.GivenName;

				if (vCard.AdditionalNames != string.Empty)
					name.MiddleName = vCard.AdditionalNames;

				if (vCard.FamilyName != string.Empty)
					name.LastName = vCard.FamilyName;

				if (vCard.NamePrefix != string.Empty)
					name.Title = vCard.NamePrefix;

				if (vCard.NameSuffix != string.Empty)
					name.Suffix = vCard.NameSuffix;

				contact.Name = name;
			}

			#endregion

			#region Delivery Address

			{
				vCardDeliveryAddressCollection vAddresses = vCard.DeliveryAddresses;
				int count = vAddresses.Count;
				Address[] addresses = new Address[count];

				for (int i = 0; i < count; i++)
				{
					vCardDeliveryAddress vAddress = vAddresses[i];
					Address address = new Address();

					address.City = vAddress.City;
					address.Country = vAddress.Country;
					address.State = vAddress.Region;
					address.Street = vAddress.Street.TrimEnd(',');
					address.ZIP = vAddress.PostalCode;
					address.Type = vAddress.AddressType.ToString();

					addresses[i] = address;
				}

				contact.Addresses = addresses;
			}

			#endregion

			#region Email Address

			{
				vCardEmailAddressCollection vEmails = vCard.EmailAddresses;
				int count = vEmails.Count;
				Email[] emails = new Email[count];

				for (int i = 0; i < count; i++)
				{
					vCardEmailAddress vEmail = vEmails[i];
					Email email = new Email();

					email.Address = vEmail.Address;
					email.Type = vEmail.EmailType.ToString();

					emails[i] = email;
				}

				contact.Emails = emails;
			}

			#endregion

			#region Website

			{
				vCardWebsiteCollection vWebsites = vCard.Websites;
				int count = vWebsites.Count;
				Website[] websites = new Website[count];

				for (int i = 0; i < count; i++)
				{
					vCardWebsite vWebsite = vWebsites[i];
					Website website = new Website();

					website.Url = vWebsite.Url;
					website.Type = vWebsite.WebsiteType.ToString();

					websites[i] = website;
				}

				contact.Websites = websites;
			}

			#endregion

			#region Notes

			{
				FlowDocument notes = new FlowDocument();

				foreach (vCardNote each in vCard.Notes)
				{
					Paragraph para = new Paragraph(new Run(each.Text));

					if (each.Language != string.Empty)
						try { para.Language = XmlLanguage.GetLanguage(each.Language); }
						catch { }

					notes.Blocks.Add(para);
				}

				contact.NotesDocument = notes;
			}

			#endregion

			#region Phone

			{
				vCardPhoneCollection vPhones = vCard.Phones;
				int count = vPhones.Count;
				PhoneNumber[] phones = new PhoneNumber[count];

				for (int i = 0; i < count; i++)
				{
					vCardPhone vPhone = vPhones[i];
					PhoneNumber phone = new PhoneNumber();

					phone.Number = vPhone.FullNumber;
					phone.Type = InsertSpaces(vPhone.PhoneType.ToString());

					phones[i] = phone;
				}

				contact.PhoneNumbers = phones;
			}

			#endregion

			if (vCard.BirthDate.HasValue)
				contact.SpecialDates = new SpecialDate[] { new SpecialDate("Birthday", vCard.BirthDate.Value) };

			contact.Private = vCard.AccessClassification.HasFlag(vCardAccessClassification.Confidential)
				|| vCard.AccessClassification.HasFlag(vCardAccessClassification.Private);
			contact.Work = new Work()
			{
				Company = vCard.Organization,
				Department = vCard.Department,
				Office = vCard.Office,
				Title = vCard.Title
			};
			contact.Gender = (Gender)vCard.Gender;

			if (vCard.IMAddress != string.Empty)
				contact.IM = new IM[] { new IM("IM", vCard.IMAddress) };

			foreach (vCardPhoto photo in vCard.Photos)
			{
				if (photo.Url != null)
					try
					{
						photo.Fetch();
						contact.encodeTile(photo.GetBytes());
						//contact.Tile = Create96By96Tile(ConvertBytesToBitmapSource(photo.GetBytes()));
						break;
					}
					catch { }
				else
				{
					contact.encodeTile(photo.GetBytes());
					//contact.Tile = Create96By96Tile(ConvertBytesToBitmapSource(photo.GetBytes()));
					break;
				}
			}

			return contact;
		}

		/// <summary>
		/// Inserts spaces every time an uppercase letter is encountered. Used
		/// for converting enums to strings.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private static string InsertSpaces(string data)
		{
			int length = data.Length;

			for (int i = 1; i < length; i++)
			{
				if (char.IsUpper(data[i]) && data[i - 1] != ' ')
				{
					data = data.Insert(i, " ");
					i++;
					length++;
				}
			}

			return data;
		}

		//private static BitmapSource ConvertBytesToBitmapSource(byte[] data)
		//{
		//	BitmapImage bi = new BitmapImage();
		//	bi.BeginInit();
		//	bi.StreamSource = new MemoryStream(data);
		//	bi.EndInit();

		//	return bi;
		//}

		//private static BitmapSource Create96By96Tile(BitmapSource bitmap)
		//{
		//	RenderTargetBitmap renderTarget = new RenderTargetBitmap(96, 96, 96, 96, PixelFormats.Pbgra32);

		//	ImageBrush sourceBrush = new ImageBrush(bitmap);
		//	sourceBrush.Stretch = Stretch.Uniform;

		//	DrawingVisual drawingVisual = new DrawingVisual();
		//	DrawingContext drawingContext = drawingVisual.RenderOpen();
		//	//drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, 96, 96));
		//	drawingContext.DrawRectangle(sourceBrush, null, new Rect(0, 0, 96, 96));
		//	drawingContext.Close();

		//	renderTarget.Render(drawingVisual);
		//	return renderTarget;
		//}

		#endregion

		#region Serialization

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(ContactDatabase.NameAttribute, _name);
			info.AddValue(ContactDatabase.WorkAttribute, _work);
			info.AddValue(ContactDatabase.EmailAttribute, _email);
			info.AddValue(ContactDatabase.WebSiteAttribute, _website);
			info.AddValue(ContactDatabase.IMAttribute, _im);
			info.AddValue(ContactDatabase.PhoneAttribute, _phone);
			info.AddValue(ContactDatabase.AddressAttribute, _address);
			info.AddValue(ContactDatabase.TileAttribute, _tile);
			info.AddValue(ContactDatabase.DateAttribute, _specialDate);
			info.AddValue(ContactDatabase.ReadOnlyAttribute, _readOnly);
			info.AddValue(ContactDatabase.PrivateAttribute, _private);
			info.AddValue(ContactDatabase.GenderAttribute, ((byte)_gender).ToString());
			info.AddValue("dtl", Serializer.FlowDocumentSerialize(NotesDocument));
		}

		protected Contact(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_saveChangesToDisk = false;

			_name = info.GetString(ContactDatabase.NameAttribute);
			_work = info.GetString(ContactDatabase.WorkAttribute);
			_email = info.GetString(ContactDatabase.EmailAttribute);
			_website = info.GetString(ContactDatabase.WebSiteAttribute);
			_im = info.GetString(ContactDatabase.IMAttribute);
			_phone = info.GetString(ContactDatabase.PhoneAttribute);
			_address = info.GetString(ContactDatabase.AddressAttribute);
			_tile = info.GetString(ContactDatabase.TileAttribute);
			_specialDate = info.GetString(ContactDatabase.DateAttribute);
			_readOnly = info.GetBoolean(ContactDatabase.ReadOnlyAttribute);
			_private = info.GetBoolean(ContactDatabase.PrivateAttribute);
			_gender = (Gender)info.GetByte(ContactDatabase.GenderAttribute);
			NotesDocument = Serializer.FlowDocumentDeserialize(info.GetString("dtl"));
		}

		#endregion

		#region Graphics

		public string WorkDescription
		{
			get
			{
				Work work = Work;

				if (work != null)
				{
					string descrip = work.Title;

					string other = work.Department != "" ? work.Department : work.Company;
					descrip += (work.Title != "" && other != "" ? ", " : "") + other;

					return descrip;
				}

				return "";
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		#endregion
	}
}
