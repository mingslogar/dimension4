using Daytimer.DatabaseHelpers;
using Modern.FileBrowser;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Daytimer.Controls
{
	public class ImportHelpers
	{
		private static FileDialog openDialog;

		public static async Task<List<Contact>> ImportContact(Window owner, StatusStrip statusStrip, string rootFolder = null)
		{
			statusStrip.UpdateMainStatus("IMPORTING...");

			FileDialog open = new FileDialog(owner,
				rootFolder == null ? Environment.GetFolderPath(Environment.SpecialFolder.Personal) : rootFolder,
				FileDialogType.Open, ListViewMode.Detail);
			open.Title = "Import Contact";
			open.Filter = "VCard Contact Files (*.vcf)|.vcf";
			//open.IconSize = IconSize.Small;
			open.FilterIndex = 0;
			//open.RootFolder = rootFolder == null ? Environment.GetFolderPath(Environment.SpecialFolder.Personal) : rootFolder;
			openDialog = open;

			List<Contact> list = null;

			if (open.ShowDialog() == true)
			{
				try
				{
					string file = open.SelectedFile;

					await Task.Factory.StartNew(() =>
					{
						list = Contact.ParseVCard(file);

						foreach (Contact each in list)
							ContactDatabase.Add(each);
					});

					statusStrip.UpdateMainStatus("READY");
				}
				catch (Exception exc)
				{
					list = null;
					statusStrip.UpdateMainStatus("ERROR: " + exc.Message.ToUpper());
				}
			}
			else
				statusStrip.UpdateMainStatus("READY");

			return list;
		}
	}
}
