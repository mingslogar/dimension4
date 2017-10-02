using System;
using System.IO;
using System.Windows.Media;
using System.Xml;

namespace Daytimer.DatabaseHelpers
{
	public class TaskDatabase
	{
		#region Tags/Attributes

		public const string TaskTag = "t";

		// Attributes
		public const string SubjectAttribute = "t";
		public const string StartDateAttribute = "d";
		public const string ReminderAttribute = "r";
		public const string IsReminderEnabledAttribute = "e";
		public const string StatusAttribute = "s";
		public const string PriorityAttribute = "p";
		public const string ProgressAttribute = "g";
		public const string CategoryAttribute = "c";
		public const string OwnerAttribute = "o";
		public const string ReadOnlyAttribute = "x";
		public const string PrivateAttribute = "v";
		public const string LastModifiedAttribute = "m";


		public const string CategoryTag = "c";

		public const string CategoryNameAttribute = "n";
		public const string CategoryColorAttribute = "c";
		public const string CategoryDescriptionAttribute = "d";
		public const string CategoryReadOnlyAttribute = "r";

		#endregion

		#region Fields

		public static string TasksAppData = Static.LocalAppData + "\\Tasks";
		private static string DatabaseLocation = TasksAppData + "\\TaskDatabase.xml";

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
			db = new XmlDatabase(DatabaseLocation, new string[] { TaskTag, CategoryTag }, InitializeNewDatabase);
		}

		public static void Save()
		{
			db.Save();
			SaveCompletedEvent(new EventArgs());
		}

		/// <summary>
		/// Add new task.
		/// </summary>
		/// <param name="task">The task to add.</param>
		public static void Add(UserTask task)
		{
			XmlElement existing = db.Doc.GetElementById(task.ID);

			if (existing == null)
			{
				//
				// <yyyymmdd></yyyymmdd>
				//
				string date = "nodate";

				if (task.DueDate != null)
					date = FormatHelpers.DateString((DateTime)task.DueDate);

				XmlNode existingDate = db.Doc.SelectSingleNode("/db/" + date);

				//
				// <t />
				//
				XmlElement tsk = db.Doc.CreateElement(TaskTag);

				tsk.SetAttribute(XmlDatabase.IdAttribute, task.ID);
				tsk.SetAttribute(SubjectAttribute, task.Subject);

				if (task.StartDate != null)
					tsk.SetAttribute(StartDateAttribute, FormatHelpers.DateTimeToShortString(task.StartDate.Value));
				else
					tsk.SetAttribute(StartDateAttribute, "");

				tsk.SetAttribute(ReminderAttribute, FormatHelpers.DateTimeToString(task.Reminder));
				tsk.SetAttribute(IsReminderEnabledAttribute, FormatHelpers.BoolToString(task.IsReminderEnabled));
				tsk.SetAttribute(StatusAttribute, ((byte)task.Status).ToString());
				tsk.SetAttribute(PriorityAttribute, ((byte)task.Priority).ToString());
				tsk.SetAttribute(ProgressAttribute, task.Progress.ToString());
				tsk.SetAttribute(CategoryAttribute, task.CategoryID);
				tsk.SetAttribute(OwnerAttribute, task.Owner);
				tsk.SetAttribute(ReadOnlyAttribute, FormatHelpers.BoolToString(task.ReadOnly));
				tsk.SetAttribute(PrivateAttribute, FormatHelpers.BoolToString(task.Private));
				tsk.SetAttribute(LastModifiedAttribute, FormatHelpers.DateTimeToString(task.LastModified));

				if (existingDate == null)
				{
					XmlElement elem = db.Doc.CreateElement(date);
					elem.AppendChild(tsk);

					if (task.DueDate != null)
						db.Doc.SmartInsert(elem, (DateTime)task.DueDate, date);
					else
						db.Doc.DocumentElement.PrependChild(elem);
				}
				else
					existingDate.AppendChild(tsk);
			}
		}

		/// <summary>
		/// Add new task.
		/// </summary>
		/// <param name="task">The task to add.</param>
		public static void Insert(int index, UserTask task)
		{
			XmlElement existing = db.Doc.GetElementById(task.ID);

			if (existing == null)
			{
				//
				// <yyyymmdd></yyyymmdd>
				//
				string date = "nodate";

				if (task.DueDate != null)
					date = FormatHelpers.DateString((DateTime)task.DueDate);

				XmlNode existingDate = db.Doc.SelectSingleNode("/db/" + date);

				//
				// <t />
				//
				XmlElement tsk = db.Doc.CreateElement(TaskTag);

				tsk.SetAttribute(XmlDatabase.IdAttribute, task.ID);
				tsk.SetAttribute(SubjectAttribute, task.Subject);

				if (task.StartDate != null)
					tsk.SetAttribute(StartDateAttribute, FormatHelpers.DateTimeToShortString(task.StartDate.Value));
				else
					tsk.SetAttribute(StartDateAttribute, "");

				tsk.SetAttribute(ReminderAttribute, FormatHelpers.DateTimeToString(task.Reminder));
				tsk.SetAttribute(IsReminderEnabledAttribute, FormatHelpers.BoolToString(task.IsReminderEnabled));
				tsk.SetAttribute(StatusAttribute, ((byte)task.Status).ToString());
				tsk.SetAttribute(PriorityAttribute, ((byte)task.Priority).ToString());
				tsk.SetAttribute(ProgressAttribute, task.Progress.ToString());
				tsk.SetAttribute(CategoryAttribute, task.CategoryID);
				tsk.SetAttribute(OwnerAttribute, task.Owner);
				tsk.SetAttribute(ReadOnlyAttribute, FormatHelpers.BoolToString(task.ReadOnly));
				tsk.SetAttribute(PrivateAttribute, FormatHelpers.BoolToString(task.Private));
				tsk.SetAttribute(LastModifiedAttribute, FormatHelpers.DateTimeToString(task.LastModified));

				if (existingDate == null)
				{
					XmlElement elem = db.Doc.CreateElement(date);
					elem.AppendChild(tsk);

					if (task.DueDate != null)
						db.Doc.SmartInsert(elem, (DateTime)task.DueDate, date);
					else
						db.Doc.DocumentElement.PrependChild(elem);
				}
				else
				{
					existingDate.InsertBefore(tsk, existingDate.ChildNodes[index]);
				}
			}
		}

		/// <summary>
		/// Delete existing task.
		/// </summary>
		/// <param name="task">The task to delete.</param>
		/// <param name="deleteDetails">If true, delete the details file.</param>
		public static void Delete(UserTask task, bool deleteDetails = true)
		{
			if (task != null)
			{
				XmlElement elem = db.Doc.GetElementById(task.ID);

				if (elem != null)
				{
					if (deleteDetails)
					{
						// Delete details file, if it exists.
						string file = TasksAppData + "\\" + task.ID;

						if (File.Exists(file))
							File.Delete(file);
					}

					XmlNode parent = elem.ParentNode;
					parent.RemoveChild(elem);

					if (!parent.HasChildNodes)
						parent.ParentNode.RemoveChild(parent);
				}
			}
		}

		/// <summary>
		/// Get all tasks tagged with the specified date.
		/// </summary>
		/// <param name="dueDate">the date tasks should be returned for</param>
		public static UserTask[] GetTasks(DateTime? dueDate)
		{
			//
			// <yyyymmdd></yyyymmdd>
			//
			string datestring = "nodate";

			if (dueDate != null)
				datestring = FormatHelpers.DateString((DateTime)dueDate);

			XmlNode existingDate = db.Doc.SelectSingleNode("/db/" + datestring);

			if (existingDate != null)
			{
				XmlNodeList tsks = existingDate.ChildNodes;

				int count = tsks.Count;

				if (count != 0)
				{
					UserTask[] tasks = new UserTask[count];

					for (int i = 0; i < count; i++)
					{
						//
						// <t />
						//
						XmlNode node = tsks.Item(i);
						XmlAttributeCollection attribs = node.Attributes;

						tasks[i] = new UserTask(false);
						tasks[i].ID = attribs[XmlDatabase.IdAttribute].Value;
						tasks[i].DueDate = dueDate;

						if (attribs[StartDateAttribute].Value == "")
							tasks[i].StartDate = null;
						else
							tasks[i].StartDate = FormatHelpers.ParseShortDateTime(attribs[StartDateAttribute].Value);

						tasks[i].Subject = attribs[SubjectAttribute].Value;
						tasks[i].Reminder = FormatHelpers.ParseDateTime(attribs[ReminderAttribute].Value);
						tasks[i].IsReminderEnabled = FormatHelpers.ParseBool(attribs[IsReminderEnabledAttribute].Value);
						tasks[i].Status = (UserTask.StatusPhase)byte.Parse(attribs[StatusAttribute].Value);
						tasks[i].Priority = (Priority)byte.Parse(attribs[PriorityAttribute].Value);
						tasks[i].Progress = double.Parse(attribs[ProgressAttribute].Value);
						tasks[i].CategoryID = attribs[CategoryAttribute].Value;
						tasks[i].Owner = attribs[OwnerAttribute].Value;
						tasks[i].ReadOnly = FormatHelpers.ParseBool(attribs[ReadOnlyAttribute].Value);
						tasks[i].Private = FormatHelpers.ParseBool(attribs[PrivateAttribute].Value);
						tasks[i].LastModified = FormatHelpers.ParseDateTime(attribs[LastModifiedAttribute].Value);
					}

					return tasks;
				}

				return null;
			}

			return null;
		}

		/// <summary>
		/// Get all tasks.
		/// </summary>
		/// <returns></returns>
		public static UserTask[] GetTasks()
		{
			XmlNodeList tsks = db.Doc.GetElementsByTagName(TaskTag);

			int count = tsks.Count;

			if (count != 0)
			{
				UserTask[] tasks = new UserTask[count];

				for (int i = 0; i < count; i++)
				{
					//
					// <t />
					//
					XmlNode node = tsks.Item(i);
					XmlAttributeCollection attribs = node.Attributes;

					tasks[i] = new UserTask(false);
					tasks[i].ID = attribs[XmlDatabase.IdAttribute].Value;

					if (node.ParentNode.Name != "nodate")
						tasks[i].DueDate = FormatHelpers.SplitDateString(node.ParentNode.Name);
					else
						tasks[i].DueDate = null;

					if (attribs[StartDateAttribute].Value == "")
						tasks[i].StartDate = null;
					else
						tasks[i].StartDate = FormatHelpers.ParseShortDateTime(attribs[StartDateAttribute].Value);

					tasks[i].Subject = attribs[SubjectAttribute].Value;
					tasks[i].Reminder = FormatHelpers.ParseDateTime(attribs[ReminderAttribute].Value);
					tasks[i].IsReminderEnabled = FormatHelpers.ParseBool(attribs[IsReminderEnabledAttribute].Value);
					tasks[i].Status = (UserTask.StatusPhase)byte.Parse(attribs[StatusAttribute].Value);
					tasks[i].Priority = (Priority)byte.Parse(attribs[PriorityAttribute].Value);
					tasks[i].Progress = double.Parse(attribs[ProgressAttribute].Value);
					tasks[i].CategoryID = attribs[CategoryAttribute].Value;
					tasks[i].Owner = attribs[OwnerAttribute].Value;
					tasks[i].ReadOnly = FormatHelpers.ParseBool(attribs[ReadOnlyAttribute].Value);
					tasks[i].Private = FormatHelpers.ParseBool(attribs[PrivateAttribute].Value);
					tasks[i].LastModified = FormatHelpers.ParseDateTime(attribs[LastModifiedAttribute].Value);
				}

				return tasks;
			}

			return null;
		}

		/// <summary>
		/// Change the IsReminderEnabled value to false.
		/// </summary>
		/// <param name="task"></param>
		public static void NullifyAlarm(UserTask task)
		{
			if (task != null)
			{
				XmlElement elem = db.Doc.GetElementById(task.ID);

				if (elem != null)
					elem.SetAttribute(IsReminderEnabledAttribute, "False");
			}
		}

		/// <summary>
		/// Change the IsReminderEnabled value to false.
		/// </summary>
		public static void NullifyAlarm(string id)
		{
			XmlElement elem = db.Doc.GetElementById(id);

			if (elem != null)
				elem.SetAttribute(IsReminderEnabledAttribute, "False");
		}

		/// <summary>
		/// Update the values on an existing task.
		/// </summary>
		/// <param name="task"></param>
		public static void UpdateTask(UserTask task)
		{
			if (task != null)
			{
				XmlElement tsk = db.Doc.GetElementById(task.ID);

				if (tsk != null)
				{
					XmlNode parent = tsk.ParentNode;

					DateTime? tskDate = null;

					if (parent.Name != "nodate")
						tskDate = FormatHelpers.SplitDateString(parent.Name);

					if (task.DueDate == tskDate)
					{
						tsk.SetAttribute(SubjectAttribute, task.Subject);

						if (task.StartDate == null)
							tsk.SetAttribute(StartDateAttribute, "");
						else
							tsk.SetAttribute(StartDateAttribute, FormatHelpers.DateTimeToShortString(task.StartDate.Value));

						tsk.SetAttribute(ReminderAttribute, FormatHelpers.DateTimeToString(task.Reminder));
						tsk.SetAttribute(IsReminderEnabledAttribute, FormatHelpers.BoolToString(task.IsReminderEnabled));
						tsk.SetAttribute(StatusAttribute, ((byte)task.Status).ToString());
						tsk.SetAttribute(PriorityAttribute, ((byte)task.Priority).ToString());
						tsk.SetAttribute(ProgressAttribute, task.Progress.ToString());
						tsk.SetAttribute(CategoryAttribute, task.CategoryID);
						tsk.SetAttribute(OwnerAttribute, task.Owner);
						tsk.SetAttribute(ReadOnlyAttribute, FormatHelpers.BoolToString(task.ReadOnly));
						tsk.SetAttribute(PrivateAttribute, FormatHelpers.BoolToString(task.Private));
						tsk.SetAttribute(LastModifiedAttribute, FormatHelpers.DateTimeToString(task.LastModified));
					}
					else
					{
						parent.RemoveChild(tsk);

						if (!parent.HasChildNodes)
							parent.ParentNode.RemoveChild(parent);

						Add(task);
					}
				}
				else
					Add(task);
			}
		}

		public static UserTask GetTask(string id)
		{
			XmlElement element = db.Doc.GetElementById(id);

			if (element == null)
				return null;

			XmlAttributeCollection attribs = element.Attributes;

			UserTask task = new UserTask(false);

			task.ID = id;

			if (element.ParentNode.Name != "nodate")
				task.DueDate = FormatHelpers.SplitDateString(element.ParentNode.Name);
			else
				task.DueDate = null;

			if (attribs[StartDateAttribute].Value == "")
				task.StartDate = null;
			else
				task.StartDate = FormatHelpers.ParseShortDateTime(attribs[StartDateAttribute].Value);

			task.Subject = attribs[SubjectAttribute].Value;
			task.Reminder = FormatHelpers.ParseDateTime(attribs[ReminderAttribute].Value);
			task.IsReminderEnabled = FormatHelpers.ParseBool(attribs[IsReminderEnabledAttribute].Value);
			task.Status = (UserTask.StatusPhase)byte.Parse(attribs[StatusAttribute].Value);
			task.Priority = (Priority)byte.Parse(attribs[PriorityAttribute].Value);
			task.Progress = double.Parse(attribs[ProgressAttribute].Value);
			task.CategoryID = attribs[CategoryAttribute].Value;
			task.Owner = attribs[OwnerAttribute].Value;
			task.ReadOnly = FormatHelpers.ParseBool(attribs[ReadOnlyAttribute].Value);
			task.Private = FormatHelpers.ParseBool(attribs[PrivateAttribute].Value);
			task.LastModified = FormatHelpers.ParseDateTime(attribs[LastModifiedAttribute].Value);

			return task;
		}

		/// <summary>
		/// <para>WARNING: THIS FUNCTION IS NOT YET HOOKED UP.</para>
		/// <para>Gets all tasks which match a specified query.</para>
		/// </summary>
		/// <param name="query">The query string.</param>
		/// <param name="type">The type of query to perform.</param>
		/// <returns></returns>
		public static UserTask[] Query(string query, QueryType type)
		{
			throw (new NotImplementedException());
		}

		#region Category

		public static bool AddCategory(Category category)
		{
			if (GetCategory(category.ID) != null)
				return false;

			XmlElement element = db.Doc.CreateElement(CategoryTag);

			element.SetAttribute(XmlDatabase.IdAttribute, category.ID);
			element.SetAttribute(CategoryColorAttribute, category.Color.ToString());
			element.SetAttribute(CategoryNameAttribute, category.Name);
			element.SetAttribute(CategoryDescriptionAttribute, category.Description);
			element.SetAttribute(CategoryReadOnlyAttribute, FormatHelpers.BoolToString(category.ReadOnly));

			db.Doc.DocumentElement.PrependChild(element);

			return true;
		}

		public static void DeleteCategory(Category category)
		{
			XmlElement element = db.Doc.GetElementById(category.ID);

			if (element == null)
				return;

			element.ParentNode.RemoveChild(element);
		}

		public static void UpdateCategory(Category category)
		{
			XmlElement element = db.Doc.GetElementById(category.ID);

			if (element == null)
				AddCategory(category);
			else
			{
				element.SetAttribute(CategoryColorAttribute, category.Color.ToString());
				element.SetAttribute(CategoryNameAttribute, category.Name);
				element.SetAttribute(CategoryDescriptionAttribute, category.Description);
				element.SetAttribute(CategoryReadOnlyAttribute, FormatHelpers.BoolToString(category.ReadOnly));
			}
		}

		public static Category GetCategory(string id)
		{
			XmlElement element = db.Doc.GetElementById(id);

			if (element == null)
				return null;

			XmlAttributeCollection attribs = element.Attributes;

			Category category = new Category(false);

			category.ID = id;
			category.Name = attribs[CategoryNameAttribute].Value;
			category.Color = (Color)ColorConverter.ConvertFromString(attribs[CategoryColorAttribute].Value);
			category.ReadOnly = FormatHelpers.ParseBool(attribs[CategoryReadOnlyAttribute].Value);

			return category;
		}

		public static Category[] GetCategories()
		{
			XmlNodeList elems = db.Doc.SelectNodes("/db/" + CategoryTag);

			Category[] categories = new Category[elems.Count];

			for (int i = 0; i < elems.Count; i++)
			{
				XmlAttributeCollection attribs = elems[i].Attributes;

				Category c = new Category(false);
				c.ID = attribs[XmlDatabase.IdAttribute].Value;
				c.Color = (Color)ColorConverter.ConvertFromString(attribs[CategoryColorAttribute].Value);
				c.Name = attribs[CategoryNameAttribute].Value;
				c.ReadOnly = FormatHelpers.ParseBool(attribs[CategoryReadOnlyAttribute].Value);

				categories[i] = c;
			}

			return categories;
		}

		#endregion

		#endregion

		#region Private Methods

		private static void InitializeNewDatabase()
		{
			db.Doc.DocumentElement.InnerXml +=
				"<c id=\"0\" c=\"#FFBDD7EE\" n=\"Blue Category\" r=\"0\" />" +
				"<c id=\"1\" c=\"#FFFFC000\" n=\"Orange Category\" r=\"0\" />" +
				"<c id=\"2\" c=\"#FFFFD0FC\" n=\"Purple Category\" r=\"0\" />" +
				"<c id=\"3\" c=\"#FFC5E0B3\" n=\"Green Category\" r=\"0\" />" +
				"<c id=\"4\" c=\"#FFFFA2A2\" n=\"Red Category\" r=\"0\" />" +
				"<c id=\"5\" c=\"#FFFFE666\" n=\"Yellow Category\" r=\"0\" />";
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
