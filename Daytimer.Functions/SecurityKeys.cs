using System;

namespace Daytimer.Functions
{
	public class SecurityKeys
	{
		public static byte[] GenerateKey(string password)
		{
			return GenerateBytes(password, 32, 0);
		}

		public static byte[] GenerateIV(string password)
		{
			return GenerateBytes(password, 16, (int)Math.Round((double)password.Length / 2d, MidpointRounding.AwayFromZero));
		}

		private static byte[] GenerateBytes(string password, int length, int offset)
		{
			int passLength = password.Length;

			byte[] bytes = new byte[length];

			for (int i = 0; i < length; i++)
				bytes[i] = (byte)password[(i + offset) % passLength];

			return bytes;
		}

		#region Helper Functions

		//public static byte[] StringArrayToByteArray(string[] data)
		//{
		//	if (data == null)
		//		return null;

		//	int length = data.Length;
		//	byte[] bHash = new byte[length];

		//	for (int i = 0; i < length; i++)
		//		bHash[i] = byte.Parse(data[i]);

		//	return bHash;
		//}

		//public static string[] ByteArrayToStringArray(byte[] data)
		//{
		//	int length = data.Length;
		//	string[] hash = new string[length];

		//	for (int i = 0; i < length; i++)
		//		hash[i] = data[i].ToString();

		//	return hash;
		//}

		/// <summary>
		/// Taken from http://stackoverflow.com/questions/623104/byte-to-hex-string/14645617#14645617.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] StringToByteArray(string data)
		{
			if (data == null)
				return null;

			if (data.Length == 0 || data.Length % 2 != 0)
				return new byte[0];

			byte[] buffer = new byte[data.Length / 2];
			char c;

			for (int bx = 0, sx = 0; bx < buffer.Length; ++bx, ++sx)
			{
				// Convert first half of byte
				c = data[sx];
				buffer[bx] = (byte)((c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0')) << 4);

				// Convert second half of byte
				c = data[++sx];
				buffer[bx] |= (byte)(c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0'));
			}

			return buffer;
		}

		/// <summary>
		/// Taken from http://stackoverflow.com/questions/623104/byte-to-hex-string/14645617#14645617.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string ByteArrayToString(byte[] data)
		{
			if (data == null)
				return null;

			char[] c = new char[data.Length * 2];

			byte b;

			for (int bx = 0, cx = 0; bx < data.Length; ++bx, ++cx)
			{
				b = ((byte)(data[bx] >> 4));
				c[cx] = (char)(b > 9 ? b - 10 + 'A' : b + '0');

				b = ((byte)(data[bx] & 0x0F));
				c[++cx] = (char)(b > 9 ? b - 10 + 'A' : b + '0');
			}

			return new string(c);
		}

		#endregion
	}
}
