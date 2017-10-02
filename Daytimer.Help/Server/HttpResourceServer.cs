using Bend.Util;
using Daytimer.Help.SearchEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Resources;

namespace Daytimer.Help.Server
{
	public class HttpResourceServer : HttpServer
	{
		public HttpResourceServer()
		{

		}

		public override void HandleGETRequest(HttpProcessor p)
		{
			try
			{
				string url = p.http_url;

				// Search
				if (url.StartsWith("/search?s=", StringComparison.InvariantCultureIgnoreCase))
					ServeSearch(p, url);

				// Index
				else if (url.Equals("/all", StringComparison.InvariantCultureIgnoreCase))
					ServeIndex(p, url);

				// File
				else
					ServeFile(p, url);
			}
			catch
			{
				p.WriteFailureHeaders(500);
			}
		}

		private void ServeSearch(HttpProcessor p, string url)
		{
			string query = DeEntitizeIn(url.Substring(10));

			Search search = new Search(query);
			List<SearchResult> results = search.Results;

			HtmlGenerator html = new HtmlGenerator(results, query);
			p.WriteSuccessHeaders(200, "text/html", html.HTML);
		}

		private void ServeIndex(HttpProcessor p, string url)
		{
			List<SearchResult> results = new List<SearchResult>();
			string[] index = SearchHelpers.SearchIndex;

			foreach (string each in index)
			{
				string contents = SearchHelpers.GetResourceContents(each);

				Dictionary<string, string> meta = SearchHelpers.GetPageAttributes(contents, out contents);
				SearchResult result = new SearchResult(each, meta[SearchHelpers.TitleAttribute]);
				results.Add(result);
			}

			HtmlGenerator html = new HtmlGenerator(results, "");
			p.WriteSuccessHeaders(200, "text/html", html.HTML);
		}

		private void ServeFile(HttpProcessor p, string url)
		{
			string contentType = ContentType(url);

			if (contentType.StartsWith("image/"))
				p.WriteSuccessHeaders(200, contentType, GetResourceImage(url));
			else
				p.WriteSuccessHeaders(200, contentType, Serve(url, contentType));
		}

		public override void HandlePOSTRequest(HttpProcessor p, StreamReader inputData)
		{
			throw new NotImplementedException();
		}

		private string Serve(string resource, string contentType)
		{
			if (resource == "/")
			{
				resource = "/default.html";
				contentType = "text/html";
			}

			string header = "";
			string content = "";
			string footer = "";

			content = GetResourceContents(resource);

			if (contentType == "text/html")
			{
				header = GetResourceContents("/include/header.html");

				Dictionary<string, string> pageData = SearchHelpers.GetPageAttributes(content, out content);

				foreach (KeyValuePair<string, string> each in pageData)
				{
					header = header.Replace(SearchHelpers.VariableDelimiter + each.Key, each.Value);
					content = content.Replace(SearchHelpers.VariableDelimiter + each.Key, each.Value);
				}

				footer = GetResourceContents("/include/footer.html");
			}

			return header + content + footer;
		}

		private string ContentType(string resource)
		{
			if (!resource.Contains("."))
				return "text/html";

			switch (resource.Substring(resource.LastIndexOf('.') + 1))
			{
				case "htm":
				case "html":
				default:
					return "text/html";

				case "css":
					return "text/css";

				case "js":
					return "application/javascript";

				case "png":
					return "image/png";

				case "jpg":
				case "jpeg":
					return "image/jpeg";
			}
		}

		private string GetResourceContents(string resource)
		{
			StreamResourceInfo info = Application.GetResourceStream(new Uri("pack://application:,,,/Daytimer.Help;component/Documentation" + resource, UriKind.Absolute));
			StreamReader reader = new StreamReader(info.Stream);
			string contents = reader.ReadToEnd();
			reader.Dispose();
			return contents;
		}

		private byte[] GetResourceImage(string resource)
		{
			StreamResourceInfo info = Application.GetResourceStream(new Uri("pack://application:,,,/Daytimer.Images;component/Images" + resource, UriKind.Absolute));

			byte[] buffer = new byte[info.Stream.Length];
			info.Stream.Read(buffer, 0, (int)info.Stream.Length);

			return buffer;
		}

		private string DeEntitizeIn(string query)
		{
			return query.Replace("%20", " ").Replace('+', ' ').Replace("%26", "&");
		}
	}
}
