using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Daytimer.DatabaseHelpers
{
	class FlowDocumentStorage
	{
		public FlowDocumentStorage(string filename)
		{
			_filename = filename;
		}

		private string _filename;

		public string StringValue
		{
			get
			{
				FlowDocument doc = DocumentValue;
				TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
				return range.Text;
			}
		}

		public FlowDocument DocumentValue
		{
			get
			{
				for (int i = 0; i < 5; i++)
				{
					try
					{
						FlowDocument doc = new FlowDocument();

						if (File.Exists(_filename))
						{
							TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);

							MemoryStream mStream = new MemoryStream(CachedData, false);
							range.Load(mStream, DataFormats.XamlPackage);
							mStream.Close();
						}

						return doc;
					}
					catch
					{
						if (i == 4)
							throw;

						Thread.Sleep(1000);
					}
				}

				return null;
			}
			set
			{
				for (int i = 0; i < 5; i++)
				{
					try
					{
						if (value.Blocks.Count > 0)
						{
							string folder = new FileInfo(_filename).DirectoryName;

							// Ensure directory exists
							if (!Directory.Exists(folder))
								Directory.CreateDirectory(folder);

							TextRange range = new TextRange(value.ContentStart, value.ContentEnd);
							FileStream fStream = new FileStream(_filename, FileMode.OpenOrCreate);
							range.Save(fStream, DataFormats.XamlPackage);
							fStream.Close();
						}
						else if (File.Exists(_filename))
							File.Delete(_filename);

						// Content has changed; delete object from cache.
						MemoryCache.Default.Remove(_filename);
					}
					catch
					{
						if (i == 4)
							throw;

						Thread.Sleep(1000);
					}
				}
			}
		}

		public async Task<FlowDocument> GetDocumentValueAsync()
		{
			FlowDocument doc = new FlowDocument();

			if (await Task.Factory.StartNew<bool>(() => { return File.Exists(_filename); }))
			{
				TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);

				MemoryStream mStream = await Task.Factory.StartNew<MemoryStream>(() =>
				{
					return new MemoryStream(CachedData, false);
				});

				range.Load(mStream, DataFormats.XamlPackage);
				mStream.Close();
			}

			return doc;
		}

		public async Task SetDocumentValueAsync(FlowDocument value)
		{
			if (value.Blocks.Count > 0)
			{
				TextRange range = new TextRange(value.ContentStart, value.ContentEnd);

				MemoryStream mStream = new MemoryStream();
				range.Save(mStream, DataFormats.XamlPackage);
				
				await Task.Factory.StartNew(() =>
				{
					string folder = new FileInfo(_filename).DirectoryName;

					// Ensure directory exists
					if (!Directory.Exists(folder))
						Directory.CreateDirectory(folder);

					File.WriteAllBytes(_filename, mStream.ToArray());
					mStream.Close();
				});
			}
			else if (await Task.Factory.StartNew<bool>(() => { return File.Exists(_filename); }).ConfigureAwait(false))
				await Task.Factory.StartNew(() => { File.Delete(_filename); });

			// Content has changed; delete object from cache.
			MemoryCache.Default.Remove(_filename);
		}

		private byte[] CachedData
		{
			get
			{
				ObjectCache cache = MemoryCache.Default;
				byte[] fileContents = cache[_filename] as byte[];

				if (fileContents == null)
				{
					CacheItemPolicy policy = new CacheItemPolicy();
					policy.SlidingExpiration = TimeSpan.FromMinutes(1);

					List<string> filePaths = new List<string>();
					filePaths.Add(_filename);

					policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));
					fileContents = File.ReadAllBytes(_filename);
					cache.Set(_filename, fileContents, policy);
				}

				return fileContents;
			}
		}
	}
}
