using Daytimer.Dialogs;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Daytimer.DatabaseHelpers
{
	/// <summary>
	/// Base class for all databases used in this project.
	/// </summary>
	public class XmlDatabase
	{
		#region Attributes

		public const string IdAttribute = "id";

		#endregion

		/// <summary>
		/// Create an XML-based database with a specified file name.
		/// </summary>
		/// <param name="file">The name of the file where the database will be stored.</param>
		/// <param name="tags">The tags that will be used in this database.</param>
		/// <param name="createNewAction">The action which will be taken if the database is being initialized.</param>
		public XmlDatabase(string file, string[] tags, Action createNewAction)
		{
			InitializeComponent(file, tags, createNewAction);
		}

		/// <summary>
		/// Initialize the database with a specified file name.
		/// </summary>
		/// <param name="file">The name of the file where the database will be stored.</param>
		/// <param name="tags">The tags that will be used in this database.</param>
		/// <param name="createNewAction">The action which will be taken if the database is being initialized.</param>
		private void InitializeComponent(string file, string[] tags, Action createNewAction)
		{
			// Initialize variables
			_file = file;
			_tags = tags;
			_createNewAction = createNewAction;
			_database = new XmlDocument();

			_readSettings = new XmlReaderSettings();
			_readSettings.IgnoreComments = true;
			_readSettings.DtdProcessing = DtdProcessing.Parse;

			_writeSettings = new XmlWriterSettings();
			_writeSettings.Encoding = Encoding.Unicode;
			_writeSettings.Indent = false;
			_writeSettings.NewLineHandling = NewLineHandling.Entitize;
			_writeSettings.NewLineOnAttributes = false;
			_writeSettings.OmitXmlDeclaration = false;

			// Load the file if it exists
			Load();
		}

		private XmlDocument _database;
		private XmlWriterSettings _writeSettings;
		private XmlReaderSettings _readSettings;
		private string _file;
		private string[] _tags;
		private Action _createNewAction;
		private bool _isSaving = false;

		/// <summary>
		/// Gets the entire XmlDocument which represents the database.
		/// </summary>
		public XmlDocument Doc
		{
			get { return _database; }
			set { _database = value; }
		}

		/// <summary>
		/// Save the database
		/// </summary>
		public void Save()
		{
			if (_isSaving)
				return;

			_isSaving = true;
			Exception ioe = null;

			for (int i = 0; i < 10; i++)
			{
				try
				{
					string folder = Path.GetDirectoryName(_file);

					if (!Directory.Exists(folder))
						Directory.CreateDirectory(folder);
					else
						if (ioe is IOException)
						{
							try { File.Delete(_file); }
							catch { }
						}

					XmlWriter writer = XmlWriter.Create(_file, _writeSettings);
					_database.Save(writer);
					writer.Close();

					ioe = null;
					break;
				}
				catch (Exception exc) { ioe = exc; }
			}

			if (ioe != null)
			{
				TaskDialog dialog = new TaskDialog(null, "Error Saving Database",
					ioe.Message, MessageType.Error);
				dialog.Show();
			}

			_isSaving = false;
		}

		/// <summary>
		/// Load the database
		/// </summary>
		private void Load()
		{
			if (File.Exists(_file))
			{
				XmlReader reader = XmlReader.Create(_file, _readSettings);

				try
				{
					_database.Load(reader);

					if (_database.DocumentType == null)
						InitializeDatabase();
				}
				catch
				{
					InitializeDatabase();
				}

				reader.Close();
			}
			else
			{
				InitializeDatabase();
			}
		}

		/// <summary>
		/// Add the DTD to the document and the root element.
		/// </summary>
		private void InitializeDatabase()
		{
			string elemList = "";
			string attList = "";

			foreach (string each in _tags)
			{
				elemList += "<!ELEMENT " + each + " ANY>";
				attList += "<!ATTLIST " + each + " " + IdAttribute + " ID #REQUIRED>";
			}

			XmlDocumentType dtd = _database.CreateDocumentType("db", null, null, elemList + attList);

			_database.InsertAfter(dtd, _database.FirstChild);

			if (_database.GetElementsByTagName("db").Count == 0)
			{
				XmlElement elem = _database.CreateElement("db");
				_database.AppendChild(elem);
			}

			if (_createNewAction != null)
				_createNewAction.BeginInvoke(null, null);
		}

		/// <summary>
		/// Sort the document by date
		/// </summary>
		private void Sort()
		{
			int count = _database.DocumentElement.ChildNodes.Count - 1;

			while (true)
			{
				bool _swap = false;

				XmlNodeList nodes = _database.DocumentElement.ChildNodes;

				for (int i = 0; i < count; i++)
				{
					XmlNode node1 = nodes[i];
					XmlNode node2 = nodes[i + 1];

					if (string.CompareOrdinal(node1.Name, node2.Name) > 0)
					{
						_database.DocumentElement.RemoveChild(node1);
						_database.DocumentElement.InsertAfter(node1, node2);
						nodes = _database.DocumentElement.ChildNodes;
						_swap = true;
					}
				}

				if (!_swap)
					break;
			}
		}
	}
}