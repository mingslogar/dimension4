using System;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// A wrapper class to allow easy access from the Options control
	/// to the application MainWindow.
	/// </summary>
	public class BackstageEvents
	{
		public BackstageEvents()
		{

		}

		public static BackstageEvents StaticUpdater;

		/// <summary>
		/// Call to indicate theme has changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void InvokeThemeChanged(object sender, ThemeChangedEventArgs e)
		{
			if (OnThemeChangedEvent != null)
				OnThemeChangedEvent(sender, e);
		}

		/// <summary>
		/// Call to indicate background has changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void InvokeBackgroundChanged(object sender, BackgroundChangedEventArgs e)
		{
			if (OnBackgroundChangedEvent != null)
				OnBackgroundChangedEvent(sender, e);
		}

		/// <summary>
		/// Call to indicate general settings update.
		/// </summary>
		public void InvokeForceUpdate(object sender, ForceUpdateEventArgs e)
		{
			if (OnForceUpdateEvent != null)
				OnForceUpdateEvent(sender, e);
		}

		/// <summary>
		/// Call to indicate backstage has opened.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void InvokeBackstageOpen(object sender, EventArgs e)
		{
			if (OnBackstageOpenEvent != null)
				OnBackstageOpenEvent(sender, e);
		}

		/// <summary>
		/// Call to indicate backstage has closed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void InvokeBackstageClose(object sender, EventArgs e)
		{
			if (OnBackstageCloseEvent != null)
				OnBackstageCloseEvent(sender, e);
		}

		/// <summary>
		/// Call to indicate backstage should be closed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void InvokeForceBackstageClose(object sender, EventArgs e)
		{
			if (OnForceBackstageCloseEvent != null)
				OnForceBackstageCloseEvent(sender, e);
		}

		/// <summary>
		/// Call to indicate help window should be shown.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void InvokeHelp(object sender, EventArgs e)
		{
			if (OnHelpEvent != null)
				OnHelpEvent(sender, e);
		}

		/// <summary>
		/// Call to indicate document request.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void InvokeDocumentRequest(object sender, DocumentRequestEventArgs e)
		{
			if (OnDocumentRequestEvent != null)
				OnDocumentRequestEvent(sender, e);
		}

		/// <summary>
		/// Call to indicate print job start.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void InvokePrintStarted(object sender, PrintEventArgs e)
		{
			if (OnPrintStartedEvent != null)
				OnPrintStartedEvent(sender, e);
		}

		/// <summary>
		/// Call to request file import.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void InvokeImport(object sender, ImportEventArgs e)
		{
			if (OnImportEvent != null)
				OnImportEvent(sender, e);
		}

		/// <summary>
		/// Call to request calendar quotes refresh.
		/// </summary>
		public void InvokeQuotesChanged()
		{
			if (OnQuotesChangedEvent != null)
				OnQuotesChangedEvent(null, EventArgs.Empty);
		}

		/// <summary>
		/// Allow only one CustomDictionaryEditor to be open at a time.
		/// </summary>
		public CustomDictionaryEditor DictionaryEditor;

		#region Editing

		public bool InAppointmentEditMode = false;
		public bool InTaskEditMode = false;
		public bool InContactEditMode = false;
		public bool InNoteEditMode = false;

		/// <summary>
		/// Call to indicate an item has been opened.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void BeginEdit(EditType type)
		{
			if (type == EditType.Appointment)
				InAppointmentEditMode = true;
			else if (type == EditType.Task)
				InTaskEditMode = true;
			else if (type == EditType.Contact)
				InContactEditMode = true;
			else if (type == EditType.Note)
				InNoteEditMode = true;
		}

		/// <summary>
		/// Call to indicate an item has been closed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void EndEdit(EditType type)
		{
			if (type == EditType.Appointment)
				InAppointmentEditMode = false;
			else if (type == EditType.Task)
				InTaskEditMode = false;
			else if (type == EditType.Contact)
				InContactEditMode = false;
			else if (type == EditType.Note)
				InNoteEditMode = false;
		}

		/// <summary>
		/// Call to indicate an export operation.
		/// </summary>
		public void InvokeExport(object sender, ExportEventArgs e)
		{
			if (OnExportEvent != null)
				OnExportEvent(sender, e);
		}

		#endregion

		#region Events

		public delegate void OnThemeChanged(object sender, ThemeChangedEventArgs e);

		public event OnThemeChanged OnThemeChangedEvent;

		public delegate void OnBackgroundChanged(object sender, BackgroundChangedEventArgs e);

		public event OnBackgroundChanged OnBackgroundChangedEvent;

		public delegate void OnForceUpdate(object sender, ForceUpdateEventArgs e);

		public event OnForceUpdate OnForceUpdateEvent;

		public delegate void OnExport(object sender, ExportEventArgs e);

		public event OnExport OnExportEvent;

		public delegate void OnBackstageOpen(object sender, EventArgs e);

		public event OnBackstageOpen OnBackstageOpenEvent;

		public delegate void OnBackstageClose(object sender, EventArgs e);

		public event OnBackstageClose OnBackstageCloseEvent;

		public delegate void OnForceBackstageClose(object sender, EventArgs e);

		public event OnForceBackstageClose OnForceBackstageCloseEvent;

		public delegate void OnHelp(object sender, EventArgs e);

		public event OnHelp OnHelpEvent;

		public delegate void OnDocumentRequest(object sender, DocumentRequestEventArgs e);

		public event OnDocumentRequest OnDocumentRequestEvent;

		public delegate void OnPrintStarted(object sender, PrintEventArgs e);

		public event OnPrintStarted OnPrintStartedEvent;

		public delegate void OnImport(object sender, ImportEventArgs e);

		public event OnImport OnImportEvent;

		public delegate void OnQuotesChanged(object sender, EventArgs e);

		public event OnQuotesChanged OnQuotesChangedEvent;

		#endregion
	}
}
