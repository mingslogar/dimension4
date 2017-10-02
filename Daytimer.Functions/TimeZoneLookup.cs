using System;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace Daytimer.Functions
{
	public class TimeZoneLookup
	{
		public async static Task<TimeZoneInfo> TimeZone(double latitude, double longitude)
		{
			return await Task.Factory.StartNew<TimeZoneInfo>(() =>
			{
				try
				{
					WebClient client = new WebClient();
					string response = client.DownloadString(GlobalData.AskGeoBaseURI + "?points=" + latitude + "%2C" + longitude + "&databases=TimeZone");

					XmlDocument parsed = new XmlDocument();
					parsed.InnerXml = response;

					string tz = parsed.GetElementsByTagName("TimeZone")[0].Attributes["WindowsStandardName"].Value;
					return TimeZoneInfo.FindSystemTimeZoneById(tz);
				}
				catch
				{
					return null;
				}
			});
		}
	}
}
