using System;
using System.Text;

namespace Daytimer.DatabaseHelpers
{
	public class IDGenerator
	{
		public static string GenerateID()
		{
			return RandString() + DateTime.Now.TimeOfDay.Ticks.ToString() + (_counter % 10000).ToString("0000");
		}

		private static long _counter = 0;

		private const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		private const byte charLength = 36;
		private const byte randLength = 12;

		private static Random rand = new Random((int)DateTime.Now.Ticks);

		private static string RandString()
		{
			StringBuilder value = new StringBuilder(randLength);

			for (byte i = 0; i < randLength; i++)
			{
				int x = rand.Next(charLength);
				value.Append(characters[x]);
			}

			return value.ToString();
		}
	}
}
