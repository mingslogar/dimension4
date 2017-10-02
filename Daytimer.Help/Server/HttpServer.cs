using Daytimer.Help;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

// offered to the public domain for any use with no restriction
// and also with no warranty of any kind, please enjoy. - David Jeske. 

// simple HTTP explanation
// http://www.jmarshall.com/easy/http/

namespace Bend.Util
{
	public abstract class HttpServer
	{
		TcpListener listener;
		bool is_active = true;

		public HttpServer()
		{

		}

		public void Listen()
		{
			listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();
			StaticData.Port = Port = ((IPEndPoint)listener.LocalEndpoint).Port;

			while (is_active)
			{
				TcpClient s = listener.AcceptTcpClient();
				HttpProcessor processor = new HttpProcessor(s, this);
				Task.Factory.StartNew(processor.Process);
			}
		}

		public void Shutdown()
		{
			if (listener != null)
				listener.Stop();
		}

		public int Port
		{
			get;
			private set;
		}

		public abstract void HandleGETRequest(HttpProcessor p);
		public abstract void HandlePOSTRequest(HttpProcessor p, StreamReader inputData);
	}
}