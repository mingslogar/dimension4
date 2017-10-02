using System;
using System.IO;
using System.Security.Cryptography;

namespace Daytimer.Functions
{
	/// <summary>
	/// Provides basic methods for string encryption.
	/// </summary>
	public class Encryption
	{
		public static byte[] EncryptStringToBytes(string PlainText, byte[] Key, byte[] IV)
		{
			try
			{
				// Check arguments.
				if (PlainText == null || PlainText.Length <= 0)
					throw new ArgumentNullException("PlainText");
				if (Key == null || Key.Length <= 0)
					throw new ArgumentNullException("Key");
				if (IV == null || IV.Length <= 0)
					throw new ArgumentNullException("IV");

				// Declare the stream used to encrypt to an in memory
				// array of bytes.
				MemoryStream msEncrypt = null;

				// Declare the RijndaelManaged object
				// used to encrypt the data.
				RijndaelManaged aesAlg = null;

				try
				{
					// Create a RijndaelManaged object
					// with the specified key and IV.
					aesAlg = new RijndaelManaged();
					aesAlg.Key = Key;
					aesAlg.IV = IV;

					// Create a decrytor to perform the stream transform.
					ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

					// Create the streams used for encryption.
					msEncrypt = new MemoryStream();

					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
						{
							//Write all data to the stream.
							swEncrypt.Write(PlainText);
						}
					}
				}
				finally
				{
					// Clear the RijndaelManaged object.
					if (aesAlg != null)
						aesAlg.Clear();
				}

				// Return the encrypted bytes from the memory stream.
				return msEncrypt.ToArray();
			}
			catch { }

			return null;
		}

		public static string DecryptStringFromBytes(byte[] CipherText, byte[] Key, byte[] IV)
		{
			try
			{
				// Check arguments.

				if (CipherText == null || CipherText.Length <= 0)
					throw new ArgumentNullException("CipherText");
				if (Key == null || Key.Length <= 0)
					throw new ArgumentNullException("Key");
				if (IV == null || IV.Length <= 0)
					throw new ArgumentNullException("Key");

				// Declare the RijndaelManaged object
				// used to decrypt the data.

				RijndaelManaged aesAlg = null;

				// Declare the string used to hold
				// the decrypted text.

				string plaintext = null;

				try
				{
					// Create a RijndaelManaged object
					// with the specified key and IV.
					aesAlg = new RijndaelManaged();
					aesAlg.Key = Key;
					aesAlg.IV = IV;

					// Create a decrytor to perform the stream transform.
					ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

					// Create the streams used for decryption.
					using (MemoryStream msDecrypt = new MemoryStream(CipherText))
					{
						using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
						{
							using (StreamReader srDecrypt = new StreamReader(csDecrypt))
							{
								// Read the decrypted bytes from the decrypting stream
								// and place them in a string.
								plaintext = srDecrypt.ReadToEnd();
							}
						}
					}
				}
				finally
				{
					// Clear the RijndaelManaged object.
					if (aesAlg != null)
						aesAlg.Clear();
				}

				return plaintext;
			}
			catch { }

			return null;
		}
	}
}
