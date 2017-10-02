using System;

namespace Daytimer.Functions
{
	public class GlobalData
	{
		/// <summary>
		/// Last mouse zoom action
		/// </summary>
		private static DateTime? LastMouseWheel;

		/// <summary>
		/// Gets if the UI should respond to a zoom gesture (Ctrl+Mouse Wheel).
		/// </summary>
		/// <returns></returns>
		public static bool ZoomOnMouseWheel
		{
			get
			{
				bool zoom = LastMouseWheel == null || DateTime.Compare(LastMouseWheel.Value.Add(AnimationHelpers.ZoomDelay), DateTime.Now) < 0;
				LastMouseWheel = DateTime.Now;
				return zoom;
			}
		}

		/// <summary>
		/// Daytimer website.
		/// </summary>
		public const string Website = "http://daytimer.hostzi.com";

		/// <summary>
		/// Text which has been typed while waiting for a new appointment to be created.
		/// </summary>
		public static string KeyboardBacklog = null;

		public const string GoogleDataAPIKey = "AIzaSyDLXoc7f7r_CgZWyFN3DNMJ9yHjhajtoBA";
		public const string GoogleDataAppName = "Dimension 4";//"t3ehgurbhfhwwet734"; // 1034817900387.apps.googleusercontent.com
		public const string GoogleDataClientID = "1034817900387-tnr7qdfbpt16iii9r6cjbh37ubuaqcm6.apps.googleusercontent.com";
		public const string GoogleDataClientSecret = "Kh0jfbmPuS7F8qSN-Xom2EA2";
		public const string OpenWeatherMapAppID = "f2a61264421d31705d4fcbcb27188101";

		public const string AskGeoAccountID = "1259";
		public const string AskGeoAPIKey = "3068ae01a81c19d09e0b60988b46073e76eb89acd2401acc65db6dc9034c8e57";
		public const string AskGeoBaseURI = "http://api.askgeo.com/v1/" + AskGeoAccountID + "/" + AskGeoAPIKey + "/query.xml";
	}
}
