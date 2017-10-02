namespace Daytimer.DatabaseHelpers.Contacts
{
	public class Name
	{
		public Name()
		{

		}

		public Name(Name name)
		{
			_firstName = name._firstName;
			_middleName = name._middleName;
			_lastName = name._lastName;
			_title = name._title;
			_suffix = name._suffix;
		}

		private string _firstName = "";
		private string _middleName = "";
		private string _lastName = "";
		private string _title = "";
		private string _suffix = "";

		public string FirstName
		{
			get { return _firstName; }
			set { _firstName = value; }
		}

		public string MiddleName
		{
			get { return _middleName; }
			set { _middleName = value; }
		}

		public string LastName
		{
			get { return _lastName; }
			set { _lastName = value; }
		}

		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		public string Suffix
		{
			get { return _suffix; }
			set { _suffix = value; }
		}

		public override string ToString()
		{
			return ((_title != "" ? _title + ' ' : "") + _firstName + (_middleName != "" ? ' ' + _middleName : "") + ' ' + _lastName + (_suffix != "" ? ' ' + _suffix : "")).Trim();
		}

		public string ToSerializedString()
		{
			return _lastName + '|' + _firstName + '|' + _middleName + '|' + _title + '|' + _suffix;
		}

		public static Name Deserialize(string name)
		{
			string[] split = name.Split('|');

			Name n = new Name();

			if (split.Length >= 5)
			{
				n._lastName = split[0].Deserialize();
				n._firstName = split[1].Deserialize();
				n._middleName = split[2].Deserialize();
				n._title = split[3].Deserialize();
				n._suffix = split[4].Deserialize();
			}

			return n;
		}

		public static Name TryParse(string name)
		{
			try
			{
				string[] split = name.Split(' ');

				if (split.Length >= 1)
				{
					Name n = new Name();

					if (split[0].EndsWith(","))
					{
						int index = 0;

						n._lastName = split[index++].Remove(split[0].Length - 1);

						string title = split[index].ToLower().TrimEnd('.');

						if (title == "dr" || title == "miss" || title == "mr"
							|| title == "mrs" || title == "ms" || title == "prof")
						{
							n._title = split[index++];
							n._firstName = split[index++];
						}
						else
							n._firstName = split[index++];

						string suffix = split[split.Length - 1].ToLower().TrimEnd('.');

						if (suffix == "i" || suffix == "ii" || suffix == "iii" || suffix == "jr" || suffix == "sr")
						{
							n._suffix = split[split.Length - 1];

							if (split.Length >= index + 2)
								n._middleName = split[index++];
						}
						else if (split.Length >= index + 1)
							n._middleName = split[index++];
					}
					else
					{
						int index = 0;

						string title = split[index].ToLower().TrimEnd('.');

						if (title == "dr" || title == "miss" || title == "mr"
							|| title == "mrs" || title == "ms" || title == "prof")
						{
							n._title = split[index++];
							n._firstName = split[index++];
						}
						else
							n._firstName = split[index++];

						string suffix = split[split.Length - 1].ToLower().TrimEnd('.');

						if (suffix == "i" || suffix == "ii" || suffix == "iii" || suffix == "jr" || suffix == "sr")
						{
							n._suffix = split[split.Length - 1];

							if (split.Length >= index + 3)
							{
								n._middleName = split[index++];
								n._lastName = split[index++];
							}
							else
								n._lastName = split[index++];
						}
						else if (split.Length >= index + 2)
						{
							n._middleName = split[index++];
							n._lastName = split[index++];
						}
						else
							n._lastName = split[index++];
					}

					return n;
				}
			}
			catch
			{
			}

			return null;
		}

		public int CompareTo(Name name)
		{
			if (name == null)
				return 1;

			int comparison = _lastName.CompareTo(name._lastName);

			if (comparison != 0)
				return comparison;

			comparison = _firstName.CompareTo(name._firstName);

			if (comparison != 0)
				return comparison;

			comparison = _middleName.CompareTo(name._middleName);

			if (comparison != 0)
				return comparison;

			comparison = _suffix.CompareTo(name._suffix);

			if (comparison != 0)
				return comparison;

			return 0;
		}
	}
}
