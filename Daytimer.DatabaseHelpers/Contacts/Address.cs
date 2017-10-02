using System;
using AddressParser;

namespace Daytimer.DatabaseHelpers.Contacts
{
	public class Address
	{
		public Address()
		{
		}

		public Address(Address address)
		{
			_type = address._type;
			_street = address._street;
			_city = address._city;
			_state = address._state;
			_zip = address._zip;
			_country = address._country;
		}

		private string _type = "";
		private string _street = "";
		private string _city = "";
		private string _state = "";
		private string _zip = "";
		private string _country = "";

		public string Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public string Street
		{
			get { return _street; }
			set { _street = value; }
		}

		public string City
		{
			get { return _city; }
			set { _city = value; }
		}

		public string State
		{
			get { return _state; }
			set { _state = value; }
		}

		public string ZIP
		{
			get { return _zip; }
			set { _zip = value; }
		}

		public string Country
		{
			get { return _country; }
			set { _country = value; }
		}

		public override string ToString()
		{
			return _street + '\n' + _city + ", " + _state + " " + _zip + (_country != "" ? '\n' + _country : "");
		}

		public string ToSerializedString()
		{
			return _type.Serialize()
				+ '|' + _street.Serialize()
				+ '|' + _city.Serialize()
				+ '|' + _state.Serialize()
				+ '|' + _zip.Serialize()
				+ '|' + _country.Serialize();
		}

		public static Address Deserialize(string address)
		{
			string[] split = address.Split('|');

			Address a = new Address();
			a._type = split[0].Deserialize();
			a._street = split[1].Deserialize();
			a._city = split[2].Deserialize();
			a._state = split[3].Deserialize();
			a._zip = split[4].Deserialize();
			a._country = split[5].Deserialize();
			return a;
		}

		//public static Address TryParse(string address)
		//{
		//	try
		//	{
		//		Address a = new Address();

		//		string[] split = address.Split('\n');

		//		a._street = split[0].TrimEnd();

		//		string split1 = split[1].TrimEnd();

		//		int index = split1.IndexOf(',');
		//		a._city = split1.Remove(index);
		//		a._state = split1.Substring(index + 2, split1.IndexOf(' ', index + 2) - (index + 2));
		//		index = split1.IndexOf(' ', index + 2) + 1;

		//		int space = split1.IndexOf(' ', index);
		//		if (space == -1)
		//			space = split1.Length;

		//		if (char.IsNumber(split1[index]))
		//		{
		//			a._zip = split1.Substring(index, space - index);

		//			index = space + 1;
		//			if (index < split1.Length)
		//			{
		//				if (char.IsLetter(split1[index]))
		//				{
		//					space = split1.IndexOf(' ', index);
		//					if (space == -1)
		//						space = split1.Length;

		//					a._country = split1.Substring(index, space - index);
		//				}
		//			}
		//		}
		//		else if (char.IsLetter(split1[index]))
		//		{
		//			a._country = split1.Substring(index, space - index);

		//			index = space + 1;
		//			if (index < split1.Length)
		//			{
		//				if (char.IsNumber(split1[index]))
		//				{
		//					space = split1.IndexOf(' ', index);
		//					if (space == -1)
		//						space = split1.Length;

		//					a._zip = split1.Substring(index, space - index);
		//				}
		//			}
		//		}

		//		if (split.Length >= 3)
		//		{
		//			string split2 = split[2].TrimEnd();

		//			bool lastUsed = false;

		//			if (a._zip == "")
		//			{
		//				if (RegexUtilities.IsValidZipCode(split2))
		//				{
		//					a._zip = split2;
		//					lastUsed = true;
		//				}
		//			}

		//			if (!lastUsed && a._country == "")
		//				a._country = split2;
		//		}

		//		return a;
		//	}
		//	catch { }

		//	return null;
		//}

		/// <summary>
		/// Tries to parse a string as a <see cref="Daytimer.DatabaseHelpers.Contacts.Address"/>.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public static Address TryParse(string address)
		{
			try
			{
				string country = "";

				int zipIndex = -1;

				for (int i = address.Length - 1; i >= 0; i--)
				{
					if (char.IsNumber(address[i]))
					{
						zipIndex = i + 1;
						break;
					}
				}

				if (zipIndex != -1 && zipIndex < address.Length)
				{
					country = address.Substring(zipIndex).Trim();
					address = address.Remove(zipIndex);
				}

				Address a = new Address();
				USAddressParseResult result = new USAddressParser().ParseAddress(address);

				if (result == null)
					return null;

				a._city = FormatHelpers.Capitalize(result.City);
				a._state = result.State;
				a._street = FormatHelpers.Capitalize(result.StreetLine) + ".";
				a._zip = result.Zip;
				a._country = country;

				return a;
			}
			catch { }

			return null;
		}
	}
}
