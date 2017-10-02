namespace Daytimer.DatabaseHelpers.Contacts
{
	public class Website : UnformattedData
	{
		public Website()
			: base()
		{
		}

		public Website(UnformattedData data)
			: base(data)
		{
		}

		public Website(string type, string url)
			: base(type, url)
		{
		}

		public string Url
		{
			get { return base.Data; }
			set { base.Data = value; }
		}

		new public static Website Deserialize(string raw)
		{
			return new Website(UnformattedData.Deserialize(raw));
		}
	}
}
