using System;
using System.Collections.Generic;

namespace Daytimer.ICalendarHelpers
{
	class OlsonConverter
	{
		///// <summary>
		///// Converts an Olson time zone ID to a Windows time zone ID.
		///// </summary>
		///// <param name="olsonTimeZoneId">An Olson time zone ID. See http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/zone_tzid.html. </param>
		///// <returns>
		///// The TimeZoneInfo corresponding to the Olson time zone ID, 
		///// or null if you passed in an invalid Olson time zone ID.
		///// </returns>
		///// <remarks>
		///// See http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/zone_tzid.html
		///// </remarks>
		//public static TimeZoneInfo OlsonTimeZoneToTimeZoneInfo(string olsonTimeZoneId)
		//{
		//	Dictionary<string, string> olsonWindowsTimes = new Dictionary<string, string>()
		//	{
		//		{ "Africa/Bangui", "W. Central Africa Standard Time" },
		//		{ "Africa/Cairo", "Egypt Standard Time" },
		//		{ "Africa/Casablanca", "Morocco Standard Time" },
		//		{ "Africa/Harare", "South Africa Standard Time" },
		//		{ "Africa/Johannesburg", "South Africa Standard Time" },
		//		{ "Africa/Lagos", "W. Central Africa Standard Time" },
		//		{ "Africa/Monrovia", "Greenwich Standard Time" },
		//		{ "Africa/Nairobi", "E. Africa Standard Time" },
		//		{ "Africa/Windhoek", "Namibia Standard Time" },
		//		{ "America/Anchorage", "Alaskan Standard Time" },
		//		{ "America/Argentina/San_Juan", "Argentina Standard Time" },
		//		{ "America/Asuncion", "Paraguay Standard Time" },
		//		{ "America/Bahia", "Bahia Standard Time" },
		//		{ "America/Bogota", "SA Pacific Standard Time" },
		//		{ "America/Buenos_Aires", "Argentina Standard Time" },
		//		{ "America/Caracas", "Venezuela Standard Time" },
		//		{ "America/Cayenne", "SA Eastern Standard Time" },
		//		{ "America/Chicago", "Central Standard Time" },
		//		{ "America/Chihuahua", "Mountain Standard Time (Mexico)" },
		//		{ "America/Cuiaba", "Central Brazilian Standard Time" },
		//		{ "America/Denver", "Mountain Standard Time" },
		//		{ "America/Fortaleza", "SA Eastern Standard Time" },
		//		{ "America/Godthab", "Greenland Standard Time" },
		//		{ "America/Guatemala", "Central America Standard Time" },
		//		{ "America/Halifax", "Atlantic Standard Time" },
		//		{ "America/Indianapolis", "US Eastern Standard Time" },
		//		{ "America/La_Paz", "SA Western Standard Time" },
		//		{ "America/Los_Angeles", "Pacific Standard Time" },
		//		{ "America/Mexico_City", "Mexico Standard Time" },
		//		{ "America/Montevideo", "Montevideo Standard Time" },
		//		{ "America/New_York", "Eastern Standard Time" },
		//		{ "America/Noronha", "UTC-02" },
		//		{ "America/Phoenix", "US Mountain Standard Time" },
		//		{ "America/Regina", "Canada Central Standard Time" },
		//		{ "America/Santa_Isabel", "Pacific Standard Time (Mexico)" },
		//		{ "America/Santiago", "Pacific SA Standard Time" },
		//		{ "America/Sao_Paulo", "E. South America Standard Time" },
		//		{ "America/St_Johns", "Newfoundland Standard Time" },
		//		{ "America/Tijuana", "Pacific Standard Time" },
		//		{ "Antarctica/McMurdo", "New Zealand Standard Time" },
		//		{ "Atlantic/South_Georgia", "UTC-02" },
		//		{ "Asia/Almaty", "Central Asia Standard Time" },
		//		{ "Asia/Amman", "Jordan Standard Time" },
		//		{ "Asia/Baghdad", "Arabic Standard Time" },
		//		{ "Asia/Baku", "Azerbaijan Standard Time" },
		//		{ "Asia/Bangkok", "SE Asia Standard Time" },
		//		{ "Asia/Beirut", "Middle East Standard Time" },
		//		{ "Asia/Calcutta", "India Standard Time" },
		//		{ "Asia/Colombo", "Sri Lanka Standard Time" },
		//		{ "Asia/Damascus", "Syria Standard Time" },
		//		{ "Asia/Dhaka", "Bangladesh Standard Time" },
		//		{ "Asia/Dubai", "Arabian Standard Time" },
		//		{ "Asia/Irkutsk", "North Asia East Standard Time" },
		//		{ "Asia/Jerusalem", "Israel Standard Time" },
		//		{ "Asia/Kabul", "Afghanistan Standard Time" },
		//		{ "Asia/Kamchatka", "Kamchatka Standard Time" },
		//		{ "Asia/Karachi", "Pakistan Standard Time" },
		//		{ "Asia/Katmandu", "Nepal Standard Time" },
		//		{ "Asia/Kolkata", "India Standard Time" },
		//		{ "Asia/Krasnoyarsk", "North Asia Standard Time" },
		//		{ "Asia/Kuala_Lumpur", "Singapore Standard Time" },
		//		{ "Asia/Kuwait", "Arab Standard Time" },
		//		{ "Asia/Magadan", "Magadan Standard Time" },
		//		{ "Asia/Muscat", "Arabian Standard Time" },
		//		{ "Asia/Novosibirsk", "N. Central Asia Standard Time" },
		//		{ "Asia/Oral", "West Asia Standard Time" },
		//		{ "Asia/Rangoon", "Myanmar Standard Time" },
		//		{ "Asia/Riyadh", "Arab Standard Time" },
		//		{ "Asia/Seoul", "Korea Standard Time" },
		//		{ "Asia/Shanghai", "China Standard Time" },
		//		{ "Asia/Singapore", "Singapore Standard Time" },
		//		{ "Asia/Taipei", "Taipei Standard Time" },
		//		{ "Asia/Tashkent", "West Asia Standard Time" },
		//		{ "Asia/Tbilisi", "Georgian Standard Time" },
		//		{ "Asia/Tehran", "Iran Standard Time" },
		//		{ "Asia/Tokyo", "Tokyo Standard Time" },
		//		{ "Asia/Ulaanbaatar", "Ulaanbaatar Standard Time" },
		//		{ "Asia/Vladivostok", "Vladivostok Standard Time" },
		//		{ "Asia/Yakutsk", "Yakutsk Standard Time" },
		//		{ "Asia/Yekaterinburg", "Ekaterinburg Standard Time" },
		//		{ "Asia/Yerevan", "Armenian Standard Time" },
		//		{ "Atlantic/Azores", "Azores Standard Time" },
		//		{ "Atlantic/Cape_Verde", "Cape Verde Standard Time" },
		//		{ "Atlantic/Reykjavik", "Greenwich Standard Time" },
		//		{ "Australia/Adelaide", "Cen. Australia Standard Time" },
		//		{ "Australia/Brisbane", "E. Australia Standard Time" },
		//		{ "Australia/Darwin", "AUS Central Standard Time" },
		//		{ "Australia/Hobart", "Tasmania Standard Time" },
		//		{ "Australia/Perth", "W. Australia Standard Time" },
		//		{ "Australia/Sydney", "AUS Eastern Standard Time" },
		//		{ "Etc/GMT", "UTC" },
		//		{ "Etc/GMT+11", "UTC-11" },
		//		{ "Etc/GMT+12", "Dateline Standard Time" },
		//		{ "Etc/GMT+2", "UTC-02" },
		//		{ "Etc/GMT-12", "UTC+12" },
		//		{ "Europe/Amsterdam", "W. Europe Standard Time" },
		//		{ "Europe/Athens", "GTB Standard Time" },
		//		{ "Europe/Belgrade", "Central Europe Standard Time" },
		//		{ "Europe/Berlin", "W. Europe Standard Time" },
		//		{ "Europe/Brussels", "Romance Standard Time" },
		//		{ "Europe/Budapest", "Central Europe Standard Time" },
		//		{ "Europe/Dublin", "GMT Standard Time" },
		//		{ "Europe/Helsinki", "FLE Standard Time" },
		//		{ "Europe/Istanbul", "GTB Standard Time" },
		//		{ "Europe/Kiev", "FLE Standard Time" },
		//		{ "Europe/London", "GMT Standard Time" },
		//		{ "Europe/Minsk", "E. Europe Standard Time" },
		//		{ "Europe/Moscow", "Russian Standard Time" },
		//		{ "Europe/Paris", "Romance Standard Time" },
		//		{ "Europe/Sarajevo", "Central European Standard Time" },
		//		{ "Europe/Warsaw", "Central European Standard Time" },
		//		{ "Indian/Mauritius", "Mauritius Standard Time" },
		//		{ "Pacific/Apia", "Samoa Standard Time" },
		//		{ "Pacific/Auckland", "New Zealand Standard Time" },
		//		{ "Pacific/Fiji", "Fiji Standard Time" },
		//		{ "Pacific/Guadalcanal", "Central Pacific Standard Time" },
		//		{ "Pacific/Guam", "West Pacific Standard Time" },
		//		{ "Pacific/Honolulu", "Hawaiian Standard Time" },
		//		{ "Pacific/Pago_Pago", "UTC-11" },
		//		{ "Pacific/Port_Moresby", "West Pacific Standard Time" },
		//		{ "Pacific/Tongatapu", "Tonga Standard Time" }
		//	};

		//	string windowsTimeZoneId = default(string);
		//	TimeZoneInfo windowsTimeZone = default(TimeZoneInfo);

		//	if (olsonWindowsTimes.TryGetValue(olsonTimeZoneId, out windowsTimeZoneId))
		//	{
		//		try { windowsTimeZone = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId); }
		//		catch (TimeZoneNotFoundException) { }
		//		catch (InvalidTimeZoneException) { }
		//	}

		//	return windowsTimeZone;
		//}

		private static Dictionary<string, string> windowsOlsonTimes = new Dictionary<string, string>()
		{
			{ "W. Central Africa Standard Time", "Africa/Bangui" },
			{ "Egypt Standard Time", "Africa/Cairo" },
			{ "Morocco Standard Time", "Africa/Casablanca" },
			{ "South Africa Standard Time", "Africa/Harare" },
			//{ "South Africa Standard Time", "Africa/Johannesburg" },
			//{ "W. Central Africa Standard Time", "Africa/Lagos" },
			{ "Greenwich Standard Time", "Africa/Monrovia" },
			{ "E. Africa Standard Time", "Africa/Nairobi" },
			{ "Namibia Standard Time", "Africa/Windhoek" },
			{ "Alaskan Standard Time", "America/Anchorage" },
			{ "Argentina Standard Time", "America/Argentina/San_Juan" },
			{ "Paraguay Standard Time", "America/Asuncion" },
			{ "Bahia Standard Time", "America/Bahia" },
			{ "SA Pacific Standard Time", "America/Bogota" },
			//{ "Argentina Standard Time", "America/Buenos_Aires" },
			{ "Venezuela Standard Time", "America/Caracas" },
			{ "SA Eastern Standard Time", "America/Cayenne" },
			{ "Central Standard Time", "America/Chicago" },
			{ "Mountain Standard Time (Mexico)", "America/Chihuahua" },
			{ "Central Brazilian Standard Time", "America/Cuiaba" },
			{ "Mountain Standard Time", "America/Denver" },
			//{ "SA Eastern Standard Time", "America/Fortaleza" },
			{ "Greenland Standard Time", "America/Godthab" },
			{ "Central America Standard Time", "America/Guatemala" },
			{ "Atlantic Standard Time", "America/Halifax" },
			{ "US Eastern Standard Time", "America/Indianapolis" },
			{ "SA Western Standard Time", "America/La_Paz" },
			{ "Pacific Standard Time", "America/Los_Angeles" },
			{ "Mexico Standard Time", "America/Mexico_City" },
			{ "Montevideo Standard Time", "America/Montevideo" },
			{ "Eastern Standard Time", "America/New_York" },
			{ "UTC-02", "America/Noronha" },
			{ "US Mountain Standard Time", "America/Phoenix" },
			{ "Canada Central Standard Time", "America/Regina" },
			{ "Pacific Standard Time (Mexico)", "America/Santa_Isabel" },
			{ "Pacific SA Standard Time", "America/Santiago" },
			{ "E. South America Standard Time", "America/Sao_Paulo" },
			{ "Newfoundland Standard Time", "America/St_Johns" },
			//{ "Pacific Standard Time", "America/Tijuana" },
			//{ "New Zealand Standard Time", "Antarctica/McMurdo" },
			//{ "UTC-02", "Atlantic/South_Georgia" },
			{ "Central Asia Standard Time", "Asia/Almaty" },
			{ "Jordan Standard Time", "Asia/Amman" },
			{ "Arabic Standard Time", "Asia/Baghdad" },
			{ "Azerbaijan Standard Time", "Asia/Baku" },
			{ "SE Asia Standard Time", "Asia/Bangkok" },
			{ "Middle East Standard Time", "Asia/Beirut" },
			{ "India Standard Time", "Asia/Calcutta" },
			{ "Sri Lanka Standard Time", "Asia/Colombo" },
			{ "Syria Standard Time", "Asia/Damascus" },
			{ "Bangladesh Standard Time", "Asia/Dhaka" },
			{ "Arabian Standard Time", "Asia/Dubai" },
			{ "North Asia East Standard Time", "Asia/Irkutsk" },
			{ "Israel Standard Time", "Asia/Jerusalem" },
			{ "Afghanistan Standard Time", "Asia/Kabul" },
			{ "Kamchatka Standard Time", "Asia/Kamchatka" },
			{ "Pakistan Standard Time", "Asia/Karachi" },
			{ "Nepal Standard Time", "Asia/Katmandu" },
			//{ "India Standard Time", "Asia/Kolkata" },
			{ "North Asia Standard Time", "Asia/Krasnoyarsk" },
			//{ "Singapore Standard Time", "Asia/Kuala_Lumpur" },
			{ "Arab Standard Time", "Asia/Kuwait" },
			{ "Magadan Standard Time", "Asia/Magadan" },
			//{ "Arabian Standard Time", "Asia/Muscat" },
			{ "N. Central Asia Standard Time", "Asia/Novosibirsk" },
			{ "West Asia Standard Time", "Asia/Oral" },
			{ "Myanmar Standard Time", "Asia/Rangoon" },
			//{ "Arab Standard Time", "Asia/Riyadh" },
			{ "Korea Standard Time", "Asia/Seoul" },
			{ "China Standard Time", "Asia/Shanghai" },
			{ "Singapore Standard Time", "Asia/Singapore" },
			{ "Taipei Standard Time", "Asia/Taipei" },
			//{ "West Asia Standard Time", "Asia/Tashkent" },
			{ "Georgian Standard Time", "Asia/Tbilisi" },
			{ "Iran Standard Time", "Asia/Tehran" },
			{ "Tokyo Standard Time", "Asia/Tokyo" },
			{ "Ulaanbaatar Standard Time", "Asia/Ulaanbaatar" },
			{ "Vladivostok Standard Time", "Asia/Vladivostok" },
			{ "Yakutsk Standard Time", "Asia/Yakutsk" },
			{ "Ekaterinburg Standard Time", "Asia/Yekaterinburg" },
			{ "Armenian Standard Time", "Asia/Yerevan" },
			{ "Azores Standard Time", "Atlantic/Azores" },
			{ "Cape Verde Standard Time", "Atlantic/Cape_Verde" },
			//{ "Greenwich Standard Time", "Atlantic/Reykjavik" },
			{ "Cen. Australia Standard Time", "Australia/Adelaide" },
			{ "E. Australia Standard Time", "Australia/Brisbane" },
			{ "AUS Central Standard Time", "Australia/Darwin" },
			{ "Tasmania Standard Time", "Australia/Hobart" },
			{ "W. Australia Standard Time", "Australia/Perth" },
			{ "AUS Eastern Standard Time", "Australia/Sydney" },
			{ "UTC", "Etc/GMT" },
			{ "UTC-11", "Etc/GMT+11" },
			{ "Dateline Standard Time", "Etc/GMT+12" },
			//{ "UTC-02", "Etc/GMT+2" },
			{ "UTC+12", "Etc/GMT-12" },
			{ "W. Europe Standard Time", "Europe/Amsterdam" },
			{ "GTB Standard Time", "Europe/Athens" },
			//{ "Central Europe Standard Time", "Europe/Belgrade" },
			//{ "W. Europe Standard Time", "Europe/Berlin" },
			{ "Romance Standard Time", "Europe/Brussels" },
			{ "Central Europe Standard Time", "Europe/Budapest" },
			//{ "GMT Standard Time", "Europe/Dublin" },
			{ "FLE Standard Time", "Europe/Helsinki" },
			//{ "GTB Standard Time", "Europe/Istanbul" },
			//{ "FLE Standard Time", "Europe/Kiev" },
			{ "GMT Standard Time", "Europe/London" },
			{ "E. Europe Standard Time", "Europe/Minsk" },
			{ "Russian Standard Time", "Europe/Moscow" },
			//{ "Romance Standard Time", "Europe/Paris" },
			{ "Central European Standard Time", "Europe/Sarajevo" },
			//{ "Central European Standard Time", "Europe/Warsaw" },
			{ "Mauritius Standard Time", "Indian/Mauritius" },
			{ "Samoa Standard Time", "Pacific/Apia" },
			{ "New Zealand Standard Time", "Pacific/Auckland" },
			{ "Fiji Standard Time", "Pacific/Fiji" },
			{ "Central Pacific Standard Time", "Pacific/Guadalcanal" },
			{ "West Pacific Standard Time", "Pacific/Guam" },
			{ "Hawaiian Standard Time", "Pacific/Honolulu" },
			//{ "UTC-11", "Pacific/Pago_Pago" },
			//{ "West Pacific Standard Time", "Pacific/Port_Moresby" },
			{ "Tonga Standard Time", "Pacific/Tongatapu" }
		};

		/// <summary>
		/// Converts a Windows time zone ID to an Olson time zone ID.
		/// </summary>
		/// <param name="tzInfo">A Windows time zone ID.</param>
		/// <returns>
		/// The string corresponding to the Windows time zone ID, 
		/// or null if you passed in an invalid Windows time zone ID.
		/// </returns>
		/// <remarks>
		/// See http://unicode.org/repos/cldr-tmp/trunk/diff/supplemental/zone_tzid.html
		/// </remarks>
		public static string TimeZoneInfoToOlsonTimeZone(TimeZoneInfo tzInfo)
		{
			string olsonTimeZoneId = default(string);

			if (windowsOlsonTimes.TryGetValue(tzInfo.StandardName, out olsonTimeZoneId))
				return olsonTimeZoneId;

			return null;
		}
	}
}
