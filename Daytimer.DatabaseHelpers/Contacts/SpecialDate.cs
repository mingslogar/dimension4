using System;

namespace Daytimer.DatabaseHelpers.Contacts
{
	public class SpecialDate : UnformattedData
	{
		public SpecialDate()
			: base()
		{
		}

		public SpecialDate(UnformattedData data)
			: base(data)
		{
		}

		public SpecialDate(string type, DateTime date)
			: base(type, FormatHelpers.DateTimeToShortString(date))
		{
		}

		public DateTime Date
		{
			get { return FormatHelpers.ParseShortDateTime(base.Data); }
			set { base.Data = FormatHelpers.DateTimeToShortString(value); }
		}

		new public static SpecialDate Deserialize(string raw)
		{
			return new SpecialDate(UnformattedData.Deserialize(raw));
		}
	}
}
