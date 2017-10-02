using Daytimer.Help.Server;
using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Resources;

// offered to the public domain for any use with no restriction
// and also with no warranty of any kind, please enjoy. - David Jeske. 

// simple HTTP explanation
// http://www.jmarshall.com/easy/http/

namespace Bend.Util
{
	public class HttpProcessor
	{
		public TcpClient socket;
		public HttpServer srv;

		private Stream inputStream;
		public BinaryWriter outputStream;

		public string http_method;
		public string http_url;
		public string http_protocol_versionstring;
		public Hashtable httpHeaders = new Hashtable();

		private const int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

		public HttpProcessor(TcpClient s, HttpServer srv)
		{
			this.socket = s;
			this.srv = srv;
		}

		private string streamReadLine(Stream inputStream)
		{
			int next_char;
			string data = "";

			while (true)
			{
				next_char = inputStream.ReadByte();
				if (next_char == '\n') { break; }
				if (next_char == '\r') { continue; }
				if (next_char == -1) { Thread.Sleep(1); continue; };
				data += Convert.ToChar(next_char);
			}

			return data;
		}

		public void Process()
		{
			// we can't use a StreamReader for input, because it buffers up extra data on us inside its
			// "processed" view of the world, and we want the data raw after the headers
			inputStream = new BufferedStream(socket.GetStream());

			// we probably shouldn't be using a streamwriter for all output from handlers either
			outputStream = new BinaryWriter(new BufferedStream(socket.GetStream()), Encoding.UTF8);

			try
			{
				ParseRequest();
				ReadHeaders();

				switch (http_method)
				{
					case "GET":
						HandleGETRequest();
						break;

					case "POST":
						HandlePOSTRequest();
						break;

					default:
						break;
				}
			}
			catch
			{
				WriteFailureHeaders(500);
			}

			try { outputStream.Flush(); }
			catch { }

			inputStream = null;
			outputStream = null;
			socket.Close();
		}

		public void ParseRequest()
		{
			string request = streamReadLine(inputStream);
			string[] tokens = request.Split(' ');

			if (tokens.Length != 3)
				throw new Exception("Invalid HTTP request line");

			http_method = tokens[0].ToUpper();
			http_url = tokens[1];
			http_protocol_versionstring = tokens[2];
		}

		public void ReadHeaders()
		{
			string line;

			while ((line = streamReadLine(inputStream)) != null)
			{
				if (line.Equals(""))
					return;

				int separator = line.IndexOf(':');

				if (separator == -1)
					throw new Exception("Invalid HTTP header line: " + line);

				string name = line.Substring(0, separator);
				int pos = separator + 1;

				while ((pos < line.Length) && (line[pos] == ' '))
					pos++; // strip any spaces

				string value = line.Substring(pos, line.Length - pos);
				httpHeaders[name] = value;
			}
		}

		public void HandleGETRequest()
		{
			srv.HandleGETRequest(this);
		}

		private const int BUF_SIZE = 4096;

		public void HandlePOSTRequest()
		{
			// this post data processing just reads everything into a memory stream.
			// this is fine for smallish things, but for large stuff we should really
			// hand an input stream to the request processor. However, the input stream 
			// we hand him needs to let him see the "end of the stream" at this content 
			// length, because otherwise he won't know when he's seen it all! 

			int content_len = 0;
			MemoryStream ms = new MemoryStream();

			if (httpHeaders.ContainsKey("Content-Length"))
			{
				content_len = Convert.ToInt32(httpHeaders["Content-Length"]);

				if (content_len > MAX_POST_SIZE)
				{
					throw new Exception(
						string.Format("POST Content-Length({0}) too big for this simple server",
						  content_len));
				}

				byte[] buf = new byte[BUF_SIZE];
				int to_read = content_len;

				while (to_read > 0)
				{
					int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));

					if (numread == 0)
					{
						if (to_read == 0)
						{
							break;
						}
						else
						{
							throw new Exception("Client disconnected during POST");
						}
					}

					to_read -= numread;
					ms.Write(buf, 0, numread);
				}

				ms.Seek(0, SeekOrigin.Begin);
			}

			srv.HandlePOSTRequest(this, new StreamReader(ms));
		}

		private static string _cachedAssemblyTitle = null;
		private static string _cachedAssemblyVersion = null;

		private string AssemblyTitle
		{
			get
			{
				if (_cachedAssemblyTitle != null)
					return _cachedAssemblyTitle;

				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);

				if (attributes.Length > 0)
				{
					AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];

					if (titleAttribute.Title != "")
						return _cachedAssemblyTitle = titleAttribute.Title;
				}

				return _cachedAssemblyTitle = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		private string AssemblyVersion
		{
			get
			{
				if (_cachedAssemblyVersion != null)
					return _cachedAssemblyVersion;
				return _cachedAssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		private const string DateTimeFormat = @"ddd, dd MMM yyyy HH:mm:ss \G\M\T";

		public void WriteSuccessHeaders(int statusCode, string contentType, string data)
		{
			WriteSuccessHeaders(statusCode, contentType, Encoding.UTF8.GetBytes(data));
		}

		public void WriteSuccessHeaders(int statusCode, string contentType, byte[] data)
		{
			string newline = Environment.NewLine;

			outputStream.Write(Encoding.UTF8.GetBytes("HTTP/1.1 " + statusCode.ToString() + " " + StatusCodes.Lookup(statusCode) + newline));
			outputStream.Write(Encoding.UTF8.GetBytes("Content-Type: " + contentType + newline));
			outputStream.Write(Encoding.UTF8.GetBytes("Server: " + AssemblyTitle.Replace(' ', '-') + '/' + AssemblyVersion + newline));
			outputStream.Write(Encoding.UTF8.GetBytes("Date: " + DateTime.UtcNow.ToString(DateTimeFormat) + newline));
			outputStream.Write(Encoding.UTF8.GetBytes("Connection: close" + newline));
			outputStream.Write(Encoding.UTF8.GetBytes("Content-Length: " + data.Length.ToString() + newline));
			outputStream.Write(Encoding.UTF8.GetBytes(newline));
			outputStream.Write(data);
		}

		public void WriteFailureHeaders(int errorCode)
		{
			string newline = Environment.NewLine;

			outputStream.Write(Encoding.UTF8.GetBytes("HTTP/1.1 " + errorCode.ToString() + " " + StatusCodes.Lookup(errorCode) + newline));
			outputStream.Write(Encoding.UTF8.GetBytes("Content-Type: text/html" + newline));
			outputStream.Write(Encoding.UTF8.GetBytes("Server: " + AssemblyTitle.Replace(' ', '-') + '/' + AssemblyVersion + newline));
			outputStream.Write(Encoding.UTF8.GetBytes("Date: " + DateTime.UtcNow.ToString(DateTimeFormat) + newline));
			outputStream.Write(Encoding.UTF8.GetBytes("Connection: close" + newline));

			string data = GetErrorDocument(errorCode);
			outputStream.Write(Encoding.UTF8.GetBytes("Content-Length: " + data.Length.ToString() + newline));
			outputStream.Write(Encoding.UTF8.GetBytes(newline));
			outputStream.Write(Encoding.UTF8.GetBytes(data));
		}

		private string GetErrorDocument(int errorCode)
		{
			StreamResourceInfo info = Application.GetResourceStream(new Uri("pack://application:,,,/Daytimer.Help;component/Server/errortemplate.html", UriKind.Absolute));
			StreamReader reader = new StreamReader(info.Stream);
			string contents = reader.ReadToEnd();
			reader.Dispose();
			return contents.Replace("$errorcode", errorCode.ToString());
		}
	}
}