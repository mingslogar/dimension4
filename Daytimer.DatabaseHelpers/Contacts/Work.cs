namespace Daytimer.DatabaseHelpers.Contacts
{
	public class Work
	{
		public Work()
		{
		}

		public Work(Work work)
		{
			_title = work._title;
			_department = work._department;
			_company = work._company;
			_office = work._office;
		}

		private string _title = "";
		private string _department = "";
		private string _company = "";
		private string _office = "";

		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		public string Department
		{
			get { return _department; }
			set { _department = value; }
		}

		public string Company
		{
			get { return _company; }
			set { _company = value; }
		}

		public string Office
		{
			get { return _office; }
			set { _office = value; }
		}

		public string ToSerializedString()
		{
			return _title.Serialize()
				+ '|' + _department.Serialize()
				+ '|' + _company.Serialize()
				+ '|' + _office.Serialize();
		}

		public static Work Deserialize(string work)
		{
			string[] split = work.Split('|');

			Work w = new Work();
			w._title = split[0].Deserialize();
			w._department = split[1].Deserialize();
			w._company = split[2].Deserialize();
			w._office = split[3].Deserialize();

			return w;
		}
	}
}
