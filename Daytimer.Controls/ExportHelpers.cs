using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Note;
using Daytimer.DatabaseHelpers.Templates;
using Daytimer.Dialogs;
using Modern.FileBrowser;
using Daytimer.Functions;
using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Daytimer.Controls
{
	public class ExportHelpers
	{
		private static FileDialog saveDialog;

		public static void ExportScreenshot(Window owner, StatusStrip statusStrip, UIElement source, string filename, string rootFolder = null)
		{
			statusStrip.UpdateMainStatus("EXPORTING...");

			FileDialog save = new FileDialog(owner,
				rootFolder == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) : rootFolder,
				FileDialogType.Save, ListViewMode.LargeIcon);
			save.Title = "Save Screenshot";
			save.SelectedFile = filename;
			save.Filter = "PNG Files (*.png)|.png|JPEG Files (*.jpg;*.jpeg;*.jpe;*.jfif)|.jpg;.jpeg;.jpe;.jfif|Bitmap Files (*.bmp)|.bmp|GIF Files (*.gif)|.gif";
			//save.IconSize = IconSize.Large;
			save.FilterIndex = 0;
			//save.RootFolder = rootFolder == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) : rootFolder;
			saveDialog = save;

			if (save.ShowDialog() == true)
				SaveScreenshot(owner, statusStrip, source, save.SelectedFile);
			else
				statusStrip.UpdateMainStatus("READY");
		}

		private static void SaveScreenshot(Window owner, StatusStrip statusStrip, UIElement source, string filename)
		{
			try
			{
				RenderTargetBitmap img = ImageProc.GetImage(source, Brushes.White);
				BitmapFrame frame = BitmapFrame.Create(img);

				FileStream fStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);

				switch (Path.GetExtension(filename).ToLower())
				{
					case ".jpg":
					case ".jpeg":
					case ".jpe":
					case ".jfif":
						JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
						jpgEncoder.QualityLevel = 100;
						jpgEncoder.Frames.Add(frame);
						jpgEncoder.Save(fStream);
						break;

					case ".png":
						PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
						pngEncoder.Interlace = PngInterlaceOption.On;
						pngEncoder.Frames.Add(frame);
						pngEncoder.Save(fStream);
						break;

					case ".bmp":
						BmpBitmapEncoder bmpEncoder = new BmpBitmapEncoder();
						bmpEncoder.Frames.Add(frame);
						bmpEncoder.Save(fStream);
						break;

					case ".gif":
						GifBitmapEncoder gifEncoder = new GifBitmapEncoder();
						gifEncoder.Frames.Add(frame);
						gifEncoder.Save(fStream);
						break;

					default:
						TaskDialog dialog = new TaskDialog(owner, "Error Saving File",
							"The requested file type is not available.",
							MessageType.Error);
						dialog.ShowDialog();
						break;
				}

				fStream.Close();

				statusStrip.UpdateMainStatus("EXPORT SUCCESSFUL");
			}
			catch (Exception exc)
			{
				statusStrip.UpdateMainStatus("ERROR: " + exc.Message.ToUpper());

				TaskDialog dialog = new TaskDialog(owner, "Error Saving File", exc.Message, MessageType.Error);
				dialog.ShowDialog();

				ExportScreenshot(owner, statusStrip, source, filename, Path.GetDirectoryName(saveDialog.SelectedFile));
			}
		}

		public static void ExportAppointment(Window owner, StatusStrip statusStrip, string rootFolder = null, Appointment appointment = null)
		{
			statusStrip.UpdateMainStatus("EXPORTING...");

			FileDialog save = new FileDialog(owner,
				rootFolder == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : rootFolder,
				FileDialogType.Save, ListViewMode.Detail);
			save.Title = "Export Current Appointment";
			save.SelectedFile = GetAppointmentName(appointment);
			save.Filter = "Rich Text Files (*.rtf)|.rtf|Plain Text Files (*.txt)|.txt";
			save.FilterIndex = 0;
			//save.RootFolder = rootFolder == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : rootFolder;
			saveDialog = save;

			if (save.ShowDialog() == true)
				SaveAppointment(owner, statusStrip, save.SelectedFile, appointment);
			else
				statusStrip.UpdateMainStatus("READY");
		}

		private static void SaveAppointment(Window owner, StatusStrip statusStrip, string filename, Appointment appointment)
		{
			try
			{
				switch (Path.GetExtension(filename).ToLower())
				{
					case ".rtf":
						FlowDocument doc = new AppointmentTemplate(appointment).GetFlowDocument();
						TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
						FileStream fStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
						range.Save(fStream, DataFormats.Rtf);
						fStream.Close();

						statusStrip.UpdateMainStatus("EXPORT SUCCESSFUL");
						break;

					case ".txt":
						string[] filecontents = new AppointmentTemplate(appointment).GetTextDocument();
						File.WriteAllLines(filename, filecontents);

						statusStrip.UpdateMainStatus("EXPORT SUCCESSFUL");
						break;

					default:
						statusStrip.UpdateMainStatus("ERROR: REQUESTED FILE TYPE UNAVAILABLE");

						TaskDialog dialog = new TaskDialog(owner, "Error Saving File",
							"The requested file type is not available.",
							MessageType.Error);
						dialog.ShowDialog();
						break;
				}
			}
			catch (Exception exc)
			{
				statusStrip.UpdateMainStatus("ERROR: " + exc.Message.ToUpper());

				TaskDialog dialog = new TaskDialog(owner, "Error Saving File", exc.Message, MessageType.Error);
				dialog.ShowDialog();

				ExportAppointment(owner, statusStrip, Path.GetDirectoryName(saveDialog.SelectedFile), appointment);
			}
		}

		private static string GetAppointmentName(Appointment appointment)
		{
			if (!appointment.IsRepeating)
				return appointment.Subject + " (" + CalendarHelpers.Month(appointment.StartDate.Month) + " "
					+ appointment.StartDate.Day.ToString() + ", "
					+ appointment.StartDate.Year.ToString() + ")";
			else
				return appointment.Subject + " (" + CalendarHelpers.Month(appointment.RepresentingDate.Month) + " "
					+ appointment.RepresentingDate.Day.ToString() + ", "
					+ appointment.RepresentingDate.Year.ToString() + ")";
		}

		public static void ExportTask(Window owner, StatusStrip statusStrip, UserTask task, string rootFolder = null)
		{
			statusStrip.UpdateMainStatus("EXPORTING...");

			FileDialog save = new FileDialog(owner,
				rootFolder == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : rootFolder,
				FileDialogType.Save, ListViewMode.Detail);
			save.Title = "Export Current Task";
			save.SelectedFile = task.Subject;
			save.Filter = "Rich Text Files (*.rtf)|.rtf|Plain Text Files (*.txt)|.txt";
			save.FilterIndex = 0;
			//save.RootFolder = rootFolder == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : rootFolder;
			saveDialog = save;

			if (save.ShowDialog() == true)
				SaveTask(owner, statusStrip, task, save.SelectedFile);
			else
				statusStrip.UpdateMainStatus("READY");
		}

		private static void SaveTask(Window owner, StatusStrip statusStrip, UserTask task, string filename)
		{
			try
			{
				switch (Path.GetExtension(filename).ToLower())
				{
					case ".rtf":
						FlowDocument doc = new TaskTemplate(task).GetFlowDocument();
						TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
						FileStream fStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
						range.Save(fStream, DataFormats.Rtf);
						fStream.Close();

						statusStrip.UpdateMainStatus("EXPORT SUCCESSFUL");
						break;

					case ".txt":
						string[] filecontents = new TaskTemplate(task).GetTextDocument();
						File.WriteAllLines(filename, filecontents);

						statusStrip.UpdateMainStatus("EXPORT SUCCESSFUL");
						break;

					default:
						statusStrip.UpdateMainStatus("ERROR: REQUESTED FILE TYPE UNAVAILABLE");

						TaskDialog dialog = new TaskDialog(owner,
							"Error Saving File", "The requested file type is not available.",
							MessageType.Error);
						dialog.ShowDialog();
						break;
				}
			}
			catch (Exception exc)
			{
				statusStrip.UpdateMainStatus("ERROR: " + exc.Message.ToUpper());

				TaskDialog dialog = new TaskDialog(owner,
					"Error Saving File", exc.Message, MessageType.Error);
				dialog.ShowDialog();

				ExportTask(owner, statusStrip, task, Path.GetDirectoryName(saveDialog.SelectedFile));
			}
		}

		public static void ExportContact(Window owner, StatusStrip statusStrip, Contact contact, string rootFolder = null)
		{
			statusStrip.UpdateMainStatus("EXPORTING...");

			FileDialog save = new FileDialog(owner,
				 rootFolder == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : rootFolder,
				 FileDialogType.Save, ListViewMode.Detail);
			save.Title = "Export Current Contact";
			save.SelectedFile = contact.Name.ToString();
			save.Filter = "Rich Text Files (*.rtf)|.rtf|Plain Text Files (*.txt)|.txt";
			save.FilterIndex = 0;
			//save.RootFolder = rootFolder == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : rootFolder;
			saveDialog = save;

			if (save.ShowDialog() == true)
				SaveContact(owner, statusStrip, contact, save.SelectedFile);
			else
				statusStrip.UpdateMainStatus("READY");
		}

		private static void SaveContact(Window owner, StatusStrip statusStrip, Contact contact, string filename)
		{
			try
			{
				switch (Path.GetExtension(filename).ToLower())
				{
					case ".rtf":
						FlowDocument doc = new ContactTemplate(contact).GetFlowDocument();
						TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
						FileStream fStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
						range.Save(fStream, DataFormats.Rtf);
						fStream.Close();

						statusStrip.UpdateMainStatus("EXPORT SUCCESSFUL");
						break;

					case ".txt":
						string[] filecontents = new ContactTemplate(contact).GetTextDocument();
						File.WriteAllLines(filename, filecontents);

						statusStrip.UpdateMainStatus("EXPORT SUCCESSFUL");
						break;

					default:
						statusStrip.UpdateMainStatus("ERROR: REQUESTED FILE TYPE UNAVAILABLE");

						TaskDialog dialog = new TaskDialog(owner,
							"Error Saving File", "The requested file type is not available.",
							MessageType.Error);
						dialog.ShowDialog();
						break;
				}
			}
			catch (Exception exc)
			{
				statusStrip.UpdateMainStatus("ERROR: " + exc.Message.ToUpper());

				TaskDialog dialog = new TaskDialog(owner,
					"Error Saving File", exc.Message, MessageType.Error);
				dialog.ShowDialog();

				ExportContact(owner, statusStrip, contact, Path.GetDirectoryName(saveDialog.SelectedFile));
			}
		}

		public static void ExportNote(Window owner, StatusStrip statusStrip, NotebookPage page, string rootFolder = null)
		{
			statusStrip.UpdateMainStatus("EXPORTING...");

			FileDialog save = new FileDialog(owner,
				rootFolder == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : rootFolder,
				FileDialogType.Save, ListViewMode.Detail);
			save.Title = "Export Current Note";
			save.SelectedFile = page.Title;
			save.Filter = "Rich Text Files (*.rtf)|.rtf|Plain Text Files (*.txt)|.txt";
			save.FilterIndex = 0;
			//save.RootFolder = rootFolder == null ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : rootFolder;
			saveDialog = save;

			if (save.ShowDialog() == true)
				SaveNote(owner, statusStrip, page, save.SelectedFile);
			else
				statusStrip.UpdateMainStatus("READY");
		}

		private static void SaveNote(Window owner, StatusStrip statusStrip, NotebookPage page, string filename)
		{
			try
			{
				switch (Path.GetExtension(filename).ToLower())
				{
					case ".rtf":
						FlowDocument doc = new NoteTemplate(page).GetFlowDocument();
						TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
						FileStream fStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
						range.Save(fStream, DataFormats.Rtf);
						fStream.Close();

						statusStrip.UpdateMainStatus("EXPORT SUCCESSFUL");
						break;

					case ".txt":
						string[] filecontents = new NoteTemplate(page).GetTextDocument();
						File.WriteAllLines(filename, filecontents);

						statusStrip.UpdateMainStatus("EXPORT SUCCESSFUL");
						break;

					default:
						statusStrip.UpdateMainStatus("ERROR: REQUESTED FILE TYPE UNAVAILABLE");

						TaskDialog dialog = new TaskDialog(owner,
							"Error Saving File", "The requested file type is not available.",
							MessageType.Error);
						dialog.ShowDialog();
						break;
				}
			}
			catch (Exception exc)
			{
				statusStrip.UpdateMainStatus("ERROR: " + exc.Message.ToUpper());

				TaskDialog dialog = new TaskDialog(owner,
					"Error Saving File", exc.Message, MessageType.Error);
				dialog.ShowDialog();

				ExportNote(owner, statusStrip, page, Path.GetDirectoryName(saveDialog.SelectedFile));
			}
		}
	}
}
