namespace Daytimer.Functions
{
	public static class StringHelpers
	{
		/// <summary>
		/// Creates a string from the uppercase first letter of each word in the string (delimited by space).
		/// </summary>
		/// <param name="phrase"></param>
		/// <returns></returns>
		public static string Acronym(this string phrase)
		{
			if (phrase == null)
				return null;

			if (phrase.isAcronym())
				return phrase;

			string[] split = phrase.Split(' ');
			string result = "";

			foreach (string each in split)
			{
				foreach (char c in each)
				{
					if (char.IsLetter(c))
					{
						result += char.ToUpper(c);
						break;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Checks if the string is an acronym (it is all uppercase and without spaces).
		/// </summary>
		/// <param name="phrase"></param>
		/// <returns></returns>
		private static bool isAcronym(this string phrase)
		{
			foreach (char each in phrase)
			{
				if (!char.IsUpper(each))
					return false;

				if (each == ' ')
					return false;
			}

			return true;
		}
	}
}
