using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Documents;

namespace Daytimer.DatabaseHelpers.Recovery
{
	class Serializer
	{
		public static void SerializeItem(ISerializable item, string filename, IFormatter formatter)
		{
			FileStream fs = new FileStream(filename, FileMode.Create);
			formatter.Serialize(fs, item);
			fs.Close();
		}

		public static object DeserializeItem(string filename, IFormatter formatter)
		{
			FileStream fs = new FileStream(filename, FileMode.Open);

			if (fs.Length > 0)
			{
				object item = formatter.Deserialize(fs);
				fs.Close();
				return item;
			}
			else
				return null;
		}

		public static string FlowDocumentSerialize(FlowDocument flowDocument)
		{
			if (flowDocument != null)
			{
				TextRange range = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
				MemoryStream stream = new MemoryStream();
				range.Save(stream, DataFormats.XamlPackage);
				stream.Close();

				return StringSerialize(stream.ToArray());
			}
			else
				return null;
		}

		public static FlowDocument FlowDocumentDeserialize(string data)
		{
			if (data != null)
			{
				FlowDocument flowDoc = new FlowDocument();
				TextRange range = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
				MemoryStream stream = new MemoryStream(StringDeserialize(data));
				range.Load(stream, DataFormats.XamlPackage);
				stream.Close();

				return flowDoc;
			}
			else
				return null;
		}

		private static string StringSerialize(byte[] buffer)
		{
			if (buffer != null)
				return Convert.ToBase64String(buffer);
			else
				return null;
		}

		private static byte[] StringDeserialize(string buffer)
		{
			if (buffer != null)
				return Convert.FromBase64String(buffer);
			else
				return null;
		}
	}
}
