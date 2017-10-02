using Daytimer.Dialogs;
using System;
using System.Printing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Threading;
using System.Windows.Xps;

namespace Daytimer.PrintHelpers
{
	/// <summary>
	/// Handles manual-duplex printing.
	/// </summary>
	public class ManualDuplex
	{
		public ManualDuplex(FixedDocument document, PrintQueue printer, bool collated, int copies)
		{
			Document = document;
			Printer = printer;
			Collated = collated;
			Copies = copies;

			Print();
		}

		public FixedDocument Document
		{
			get;
			private set;
		}

		public PrintQueue Printer
		{
			get;
			private set;
		}

		public bool Collated
		{
			get;
			private set;
		}

		public int Copies
		{
			get;
			private set;
		}

		private void Print()
		{
			// Collated: 1, 3, 5; 2, 4, 6, 1, 3, 5; 2, 4, 6.
			// Uncollated: 1, 1, 3, 3, 5, 5; 2, 2, 4, 4, 6, 6.

			ShowPrintingDialog();

			int[] first = firstBatch();
			int[] second = secondBatch();

			if (Collated)
			{
				for (int i = 0; i < Copies; i++)
				{
					print(1, first);

					if (second.Length > 0)
					{
						ShowFlipMessage();
						print(1, second);
					}
				}
			}
			else
			{
				print(Copies, first);

				if (second.Length > 0)
				{
					ShowFlipMessage();
					print(Copies, second);
				}
			}
		}

		private void print(int copies, params int[] pages)
		{
			FixedDocument doc = new FixedDocument();
			doc.DocumentPaginator.PageSize = Document.DocumentPaginator.PageSize;
			doc.Resources = Document.Resources;
			doc.PrintTicket = Document.PrintTicket;

			PrintTicket ticket = (PrintTicket)doc.PrintTicket;
			ticket.CopyCount = copies;
			//ticket.Collation = Collated ? Collation.Collated : Collation.Uncollated;

			foreach (int p in pages)
			{
				Border border = (Border)Document.Pages[p].Child.Children[0];

				FixedPage fp = new FixedPage();
				fp.Children.Add(new Border()
				{
					Width = border.Width,
					Height = border.Height,
					Margin = border.Margin,
					Background = border.Background
				});
				doc.Pages.Add(new PageContent() { Child = fp });
			}

			XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(Printer);
			HandleMessages(writer);
			writer.Write(doc);
		}

		/// <summary>
		/// Get the pages which should be printed before the user is asked to
		/// manually flip sheets around.
		/// </summary>
		/// <returns></returns>
		private int[] firstBatch()
		{
			int length = (int)Math.Ceiling((double)Document.Pages.Count / 2);
			int[] array = new int[length];

			int counter = 0;

			for (int i = 0; i < length; i++)
			{
				array[i] = counter;
				counter += 2;
			}

			return array;
		}

		/// <summary>
		/// Get the pages which should be printed after the user is asked to
		/// manually flip sheets around.
		/// </summary>
		/// <returns></returns>
		private int[] secondBatch()
		{
			int length = (int)Math.Floor((double)Document.Pages.Count / 2);
			int[] array = new int[length];

			int counter = 1;

			for (int i = 0; i < length; i++)
			{
				array[i] = counter;
				counter += 2;
			}

			return array;
		}

		/// <summary>
		/// Requests the user to manually flip sheets around.
		/// </summary>
		private void ShowFlipMessage()
		{
			TaskDialog td = new TaskDialog(
				dialog,
				"Double-Sided Printing",
				"Please remove printout of first side from tray and place it in the input bin. Then press OK to continue printing.",
				MessageType.Information);
			td.ShowDialog();
		}

		private PrintProgressDialog dialog = null;

		private void ShowPrintingDialog()
		{
			dialog = new PrintProgressDialog();
			dialog.Show();
		}

		private int _printedPagesCount = 0;

		private int TotalPages
		{
			get { return Document.Pages.Count * Copies; }
		}

		private void HandleMessages(XpsDocumentWriter writer)
		{
			writer.WritingProgressChanged += (sender, e) =>
			{
				if (e.WritingLevel == WritingProgressChangeLevel.FixedPageWritingProgress)
					IncrementPageDisplay();

				if (_printedPagesCount == TotalPages)
					dialog.CloseThreadSafe();
			};
		}

		private void IncrementPageDisplay()
		{
			_printedPagesCount++;
			dialog.UpdateProgressThreadSafe(
				"Page " + _printedPagesCount.ToString() + " of " + TotalPages.ToString(),
				(double)_printedPagesCount / (double)TotalPages);
		}
	}
}
