using Daytimer.DatabaseHelpers.Note;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Xml;

namespace Daytimer.DatabaseHelpers
{
	public class NoteDatabase
	{
		#region Tags/Attributes

		public const string NotebookTag = "n";
		public const string SectionTag = "s";
		public const string PageTag = "p";

		// Attributes
		public const string TitleAttribute = "t";
		public const string CreatedAttribute = "c";
		public const string LastModifiedAttribute = "m";
		public const string ReadOnlyAttribute = "x";
		public const string PrivateAttribute = "h";
		public const string ColorAttribute = "b";
		public const string LastSelectedAttribute = "s";

		#endregion

		#region Fields

		public static string NotesAppData = Static.LocalAppData + "\\Notes";
		private static string DatabaseLocation = NotesAppData + "\\NotesDatabase.xml";

		private static XmlDatabase db;

		public static XmlDatabase Database
		{
			get { return db; }
			set { db = value; }
		}

		#endregion

		#region Public Methods

		public static void Load()
		{
			db = new XmlDatabase(DatabaseLocation, new string[] { NotebookTag, SectionTag, PageTag }, InitializeNewDatabase);
		}

		public static void Save()
		{
			db.Save();
			SaveCompletedEvent(EventArgs.Empty);
		}

		/// <summary>
		/// Add a new notebook.
		/// </summary>
		/// <param name="notebook"></param>
		/// <returns></returns>
		public static XmlElement Add(Notebook notebook)
		{
			if (!NotebookExists(notebook))
			{
				//
				// <n />
				//
				XmlElement nt = db.Doc.CreateElement(NotebookTag);

				nt.SetAttribute(XmlDatabase.IdAttribute, notebook.ID);
				nt.SetAttribute(TitleAttribute, notebook.Title);
				nt.SetAttribute(ColorAttribute, notebook.Color.ToString());
				nt.SetAttribute(ReadOnlyAttribute, FormatHelpers.BoolToString(notebook.ReadOnly));
				nt.SetAttribute(PrivateAttribute, FormatHelpers.BoolToString(notebook.Private));
				nt.SetAttribute(LastModifiedAttribute, FormatHelpers.DateTimeToString(notebook.LastModified));
				nt.SetAttribute(LastSelectedAttribute, notebook.LastSelectedSectionID);

				db.Doc.DocumentElement.AppendChild(nt);

				return nt;
			}

			return null;
		}

		/// <summary>
		/// Add a new notebook.
		/// </summary>
		/// <param name="section"></param>
		/// <returns></returns>
		public static XmlElement Add(NotebookSection section)
		{
			if (!SectionExists(section))
			{
				//
				// <n />
				//
				XmlElement nt = db.Doc.CreateElement(SectionTag);

				nt.SetAttribute(XmlDatabase.IdAttribute, section.ID);
				nt.SetAttribute(TitleAttribute, section.Title);
				nt.SetAttribute(ColorAttribute, section.Color.ToString());
				nt.SetAttribute(ReadOnlyAttribute, FormatHelpers.BoolToString(section.ReadOnly));
				nt.SetAttribute(PrivateAttribute, FormatHelpers.BoolToString(section.Private));
				nt.SetAttribute(LastModifiedAttribute, FormatHelpers.DateTimeToString(section.LastModified));
				nt.SetAttribute(LastSelectedAttribute, section.LastSelectedPageID);

				db.Doc.GetElementById(section.NotebookID).AppendChild(nt);

				return nt;
			}

			return null;
		}

		/// <summary>
		/// Add a new page.
		/// </summary>
		/// <param name="page">the page to add</param>
		public static XmlElement Add(NotebookPage page)
		{
			if (!PageExists(page))
			{
				//
				// <p />
				//
				XmlElement nt = db.Doc.CreateElement(PageTag);

				nt.SetAttribute(XmlDatabase.IdAttribute, page.ID);
				nt.SetAttribute(TitleAttribute, page.Title);
				nt.SetAttribute(ReadOnlyAttribute, FormatHelpers.BoolToString(page.ReadOnly));
				nt.SetAttribute(PrivateAttribute, FormatHelpers.BoolToString(page.Private));
				nt.SetAttribute(CreatedAttribute, FormatHelpers.DateTimeToString(page.Created));
				nt.SetAttribute(LastModifiedAttribute, FormatHelpers.DateTimeToString(page.LastModified));

				// If section has been deleted, ignore requests to add page.
				XmlElement section = db.Doc.GetElementById(page.SectionID);

				if (section != null)
					section.AppendChild(nt);

				return nt;
			}

			return null;
		}

		/// <summary>
		/// Delete existing notebook.
		/// </summary>
		/// <param name="notebook">The notebook to delete.</param>
		public static void Delete(Notebook notebook)
		{
			if (notebook != null)
			{
				List<NotebookSection> sections = new List<NotebookSection>(notebook.Sections);

				foreach (NotebookSection each in sections)
					Delete(each);

				Delete(notebook.ID);
			}
		}

		/// <summary>
		/// Delete existing section
		/// </summary>
		/// <param name="section">The section to delete.</param>
		public static void Delete(NotebookSection section)
		{
			if (section != null)
			{
				List<NotebookPage> pages = new List<NotebookPage>(section.Pages);

				foreach (NotebookPage each in pages)
					Delete(each);

				Delete(section.ID);
			}
		}

		/// <summary>
		/// Delete existing page
		/// </summary>
		/// <param name="page">The page to delete.</param>
		public static void Delete(NotebookPage page)
		{
			if (page != null)
			{
				// Delete details file, if it exists.
				string file = NotesAppData + "\\" + page.ID;

				if (File.Exists(file))
					File.Delete(file);

				Delete(page.ID);
			}
		}

		/// <summary>
		/// Get all notebookss.
		/// </summary>
		public static IEnumerable<Notebook> GetNotebooks()
		{
			XmlNodeList notes = db.Doc.SelectNodes("/db/" + NotebookTag);

			foreach (XmlNode each in notes)
				yield return GetNotebook(each);
		}

		/// <summary>
		/// Update the values on an existing notebook.
		/// </summary>
		/// <param name="notebook"></param>
		public static void UpdateNotebook(Notebook notebook)
		{
			if (notebook != null)
			{
				XmlElement elem = db.Doc.GetElementById(notebook.ID);

				if (elem != null)
				{
					elem.SetAttribute(TitleAttribute, notebook.Title);
					elem.SetAttribute(ColorAttribute, notebook.Color.ToString());
					elem.SetAttribute(ReadOnlyAttribute, FormatHelpers.BoolToString(notebook.ReadOnly));
					elem.SetAttribute(PrivateAttribute, FormatHelpers.BoolToString(notebook.Private));
					elem.SetAttribute(LastModifiedAttribute, FormatHelpers.DateTimeToString(notebook.LastModified));
					elem.SetAttribute(LastSelectedAttribute, notebook.LastSelectedSectionID);
				}
				else
					Add(notebook);
			}
		}

		/// <summary>
		/// Update the values on an existing section.
		/// </summary>
		/// <param name="section"></param>
		public static void UpdateSection(NotebookSection section)
		{
			if (section != null)
			{
				XmlElement elem = db.Doc.GetElementById(section.ID);

				if (elem != null)
				{
					elem.SetAttribute(TitleAttribute, section.Title);
					elem.SetAttribute(ColorAttribute, section.Color.ToString());
					elem.SetAttribute(ReadOnlyAttribute, FormatHelpers.BoolToString(section.ReadOnly));
					elem.SetAttribute(PrivateAttribute, FormatHelpers.BoolToString(section.Private));
					elem.SetAttribute(LastModifiedAttribute, FormatHelpers.DateTimeToString(section.LastModified));
					elem.SetAttribute(LastSelectedAttribute, section.LastSelectedPageID);
				}
				else
					Add(section);
			}
		}

		/// <summary>
		/// Update the values on an existing page.
		/// </summary>
		/// <param name="page"></param>
		public static void UpdatePage(NotebookPage page)
		{
			if (page != null)
			{
				XmlElement elem = db.Doc.GetElementById(page.ID);

				if (elem != null)
				{
					elem.SetAttribute(TitleAttribute, page.Title);
					elem.SetAttribute(ReadOnlyAttribute, FormatHelpers.BoolToString(page.ReadOnly));
					elem.SetAttribute(PrivateAttribute, FormatHelpers.BoolToString(page.Private));
					elem.SetAttribute(CreatedAttribute, FormatHelpers.DateTimeToString(page.Created));
					elem.SetAttribute(LastModifiedAttribute, FormatHelpers.DateTimeToString(page.LastModified));
				}
				else
					Add(page);
			}
		}

		/// <summary>
		/// Gets if a specified notebook exists in the database.
		/// </summary>
		/// <param name="notebook"></param>
		/// <returns></returns>
		public static bool NotebookExists(Notebook notebook)
		{
			return db.Doc.GetElementById(notebook.ID) != null;
		}

		/// <summary>
		/// Gets if a specified section exists in the database.
		/// </summary>
		/// <param name="section"></param>
		/// <returns></returns>
		public static bool SectionExists(NotebookSection section)
		{
			return db.Doc.GetElementById(section.ID) != null;
		}

		/// <summary>
		/// Gets if a specified page exists in the database.
		/// </summary>
		/// <param name="page"></param>
		/// <returns></returns>
		public static bool PageExists(NotebookPage page)
		{
			return db.Doc.GetElementById(page.ID) != null;
		}

		public static Notebook GetNotebook(string id)
		{
			return GetNotebook(db.Doc.GetElementById(id));
		}

		public static Notebook GetNotebook(XmlNode node)
		{
			if (node == null)
				return null;

			XmlAttributeCollection attribs = node.Attributes;

			Notebook notebook = new Notebook(false);

			notebook.ID = attribs[XmlDatabase.IdAttribute].Value;
			notebook.Title = attribs[TitleAttribute].Value;
			notebook.Color = (Color)ColorConverter.ConvertFromString(attribs[ColorAttribute].Value);
			notebook.ReadOnly = FormatHelpers.ParseBool(attribs[ReadOnlyAttribute].Value);
			notebook.Private = FormatHelpers.ParseBool(attribs[PrivateAttribute].Value);
			notebook.LastModified = FormatHelpers.ParseDateTime(attribs[LastModifiedAttribute].Value);
			notebook.LastSelectedSectionID = attribs[LastSelectedAttribute].Value;

			return notebook;
		}

		public static NotebookSection GetSection(string id)
		{
			return GetSection(db.Doc.GetElementById(id));
		}

		public static NotebookSection GetSection(XmlNode node)
		{
			if (node == null)
				return null;

			XmlAttributeCollection attribs = node.Attributes;

			NotebookSection section = new NotebookSection(false);

			section.ID = attribs[XmlDatabase.IdAttribute].Value;
			section.NotebookID = node.ParentNode.Attributes[XmlDatabase.IdAttribute].Value;
			section.Title = attribs[TitleAttribute].Value;
			section.Color = (Color)ColorConverter.ConvertFromString(attribs[ColorAttribute].Value);
			section.ReadOnly = FormatHelpers.ParseBool(attribs[ReadOnlyAttribute].Value);
			section.Private = FormatHelpers.ParseBool(attribs[PrivateAttribute].Value);
			section.LastModified = FormatHelpers.ParseDateTime(attribs[LastModifiedAttribute].Value);
			section.LastSelectedPageID = attribs[LastSelectedAttribute].Value;

			return section;
		}

		public static NotebookPage GetPage(string id)
		{
			return GetPage(db.Doc.GetElementById(id));
		}

		public static NotebookPage GetPage(XmlNode node)
		{
			if (node == null)
				return null;

			XmlAttributeCollection attribs = node.Attributes;

			NotebookPage page = new NotebookPage(false);

			page.ID = attribs[XmlDatabase.IdAttribute].Value;
			page.SectionID = node.ParentNode.Attributes[XmlDatabase.IdAttribute].Value;
			page.Title = attribs[TitleAttribute].Value;
			page.ReadOnly = FormatHelpers.ParseBool(attribs[ReadOnlyAttribute].Value);
			page.Private = FormatHelpers.ParseBool(attribs[PrivateAttribute].Value);
			page.Created = FormatHelpers.ParseDateTime(attribs[CreatedAttribute].Value);
			page.LastModified = FormatHelpers.ParseDateTime(attribs[LastModifiedAttribute].Value);

			return page;
		}

		public static Notebook FirstNotebook()
		{
			XmlNode node = db.Doc.SelectSingleNode("/db/" + NotebookTag);

			if (node == null)
				return null;

			return GetNotebook(node);
		}

		/// <summary>
		/// <para>WARNING: THIS FUNCTION IS NOT YET HOOKED UP.</para>
		/// <para>Gets all notes which match a specified query.</para>
		/// </summary>
		/// <param name="query">The query string.</param>
		/// <param name="type">The type of query to perform.</param>
		/// <returns></returns>
		public static NotebookPage[] Query(string query, QueryType type)
		{
			throw (new NotImplementedException());
		}

		#endregion

		#region Private Methods

		private static void InitializeNewDatabase()
		{
			string now = FormatHelpers.DateTimeToString(DateTime.UtcNow);

			db.Doc.DocumentElement.InnerXml +=
				"<n id=\"0\" t=\"My Notebook\" m=\"" + now + "\" x=\"0\" h=\"0\" b=\"#FF7ACC93\" s=\"1\">" +
				"<s id=\"1\" t=\"Quick Notes\" m=\"" + now + "\" x=\"0\" h=\"0\" b=\"#FFF3D275\" s=\"2\">" +
				"<p id=\"2\" t=\"Dimension 4: The Notes Pane\" c=\"" + now + "\" m=\"" + now + "\" x=\"0\" h=\"0\" />" +
				"</s></n>";

			if (!Directory.Exists(NotesAppData))
				Directory.CreateDirectory(NotesAppData);

			string procFileName = Process.GetCurrentProcess().MainModule.FileName;
			File.Copy(procFileName.Remove(procFileName.LastIndexOf('\\')) + "\\Resources\\Files\\DefaultNote", NotesAppData + "\\2", true);
		}

		private static void Delete(string id)
		{
			XmlElement elem = db.Doc.GetElementById(id);

			if (elem != null)
			{
				XmlNode parent = elem.ParentNode;
				parent.RemoveChild(elem);
			}
		}

		#endregion

		#region Events

		public delegate void OnSaveCompleted(object sender, EventArgs e);

		public static event OnSaveCompleted OnSaveCompletedEvent;

		protected static void SaveCompletedEvent(EventArgs e)
		{
			if (OnSaveCompletedEvent != null)
				OnSaveCompletedEvent(null, e);
		}

		#endregion
	}
}
