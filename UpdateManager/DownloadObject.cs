namespace UpdateManager
{
	class DownloadObject
	{
		public DownloadObject(string id, string path, int size)
		{
			ID = id;
			Path = path;
			Size = size;
		}

		public string ID
		{
			get;
			internal set;
		}

		public string Path
		{
			get;
			internal set;
		}

		public int Size
		{
			get;
			internal set;
		}
	}
}
