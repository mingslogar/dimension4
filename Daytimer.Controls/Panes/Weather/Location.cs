using Daytimer.Functions;
using System;
using System.Net;
using System.Net.Cache;
using System.Xml;

namespace Daytimer.Controls.Panes.Weather
{
	public class Location
	{
		public Location(string reference)
		{
			_reference = reference;
			Download();
		}

		private void Download()
		{
			WebClient wClient = new WebClient();
			wClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);
			Uri uri = new Uri("https://maps.googleapis.com/maps/api/place/details/xml?reference="
				+ _reference + "&sensor=false&key=" + GlobalData.GoogleDataAPIKey);
			string data = wClient.DownloadString(uri);

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(data);

			_locality = xmlDoc.SelectSingleNode("/PlaceDetailsResponse/result/name").InnerText;

			XmlNodeList address_components = xmlDoc.SelectNodes("/PlaceDetailsResponse/result/address_component");

			foreach (XmlNode each in address_components)
			{
				string type = each.SelectSingleNode("type").InnerText;

				if (type == "administrative_area_level_1")
					_administrative_area_level_1 = each.SelectSingleNode("long_name").InnerText;
				else if (type == "country")
					_country = each.SelectSingleNode("long_name").InnerText;
			}
		}

		private string _reference;
		private string _locality;
		private string _administrative_area_level_1;
		private string _country;

		public string Reference
		{
			get { return _reference; }
		}

		/// <summary>
		/// City
		/// </summary>
		public string Locality
		{
			get { return _locality; }
		}

		/// <summary>
		/// State
		/// </summary>
		public string AdministrativeAreaLevel1
		{
			get { return _administrative_area_level_1; }
		}

		public string Country
		{
			get { return _country; }
		}
	}
}
