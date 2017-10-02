using System;
using System.Windows.Controls;

namespace Daytimer.Controls.Tasks
{
	public static class Extensions
	{
		/// <summary>
		/// Returns the TreeViewItem if the requested header exists, or null.
		/// </summary>
		/// <param name="treeView"></param>
		/// <param name="header"></param>
		/// <returns></returns>
		public static TreeViewItem ContainsHeader(this TreeView treeView, string header)
		{
			foreach (TreeViewItem each in treeView.Items)
				if (each.Header.ToString() == header)
					return each;

			return null;
		}

		/// <summary>
		/// Insert a TreeViewItem into a TreeView in its chronological position.
		/// </summary>
		/// <param name="item"></param>
		public static void SmartInsert(this TreeView treeView, TreeViewItem item)
		{
			// If all spaces are filled, this is the index where
			// the item should be inserted at.
			int position = SmartInsertPosition(item.Header as string);

			bool inserted = false;

			foreach (TreeViewItem each in treeView.Items)
			{
				int eachPos = SmartInsertPosition(each.Header as string);

				if (eachPos > position)
				{
					treeView.Items.Insert(treeView.Items.IndexOf(each), item);
					inserted = true;
					break;
				}
			}

			if (!inserted)
				treeView.Items.Add(item);
		}

		private static int SmartInsertPosition(string header)
		{
			return Array.IndexOf(PossiblePositions, header);
		}

		private static string[] PossiblePositions = new string[] {
			"No Date",
			"Today", 
			"Tomorrow",
			"This Week",
			"Next Week",
			"This Month",
			"Next Month",
			"This Year",
			"Next Year", 
			"Later"
		};
	}
}
