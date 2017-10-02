using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Note;
using Daytimer.DatabaseHelpers.Templates;
using Daytimer.Dialogs;
using Daytimer.Functions;
using Daytimer.PrintHelpers;
using System;
using System.Collections.Generic;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Xps;

namespace Daytimer.Controls.Ribbon
{
	/// <summary>
	/// Interaction logic for PrintControl.xaml
	/// </summary>
	public partial class PrintControl : Grid
	{
		public PrintControl(DocumentRequestEventArgs args)
		{
			InitializeComponent();

			_args = args;

			PopulatePaperSizes();
			PopulateMargins();
			orientationCombo.SelectedIndex = Settings.PrintOrientation;
			collationCombo.SelectedIndex = Settings.PrintCollation;
			pagesPerSheetCombo.SelectedIndex = Settings.PrintPagesPerSheet;

			Loaded += PrintControl_Loaded;
			Unloaded += PrintControl_Unloaded;
		}

		#region Fields

		private DocumentRequestEventArgs _args;

		private bool _isFirstTime = true;
		private DispatcherTimer updatePrintersTimer;

		#endregion

		#region Private Methods

		private void PrintControl_Loaded(object sender, RoutedEventArgs e)
		{
			Dispatcher.BeginInvoke(() =>
			{
				updatePrintersTimer = new DispatcherTimer(DispatcherPriority.Background);
				updatePrintersTimer.Interval = TimeSpan.FromSeconds(2);
				updatePrintersTimer.Tick += (timer, args) =>
				{
					// Don't refresh if the popup is open to prevent flickering.
					if (!printersCombo.IsDropDownOpen)
						PopulatePrinters();
				};
				updatePrintersTimer.Start();

				if (!_isFirstTime)
					return;

				_isFirstTime = false;

				ShowPreview();
			});
		}

		private void PrintControl_Unloaded(object sender, RoutedEventArgs e)
		{
			SavePrintSettings();

			updatePrintersTimer.Stop();
			updatePrintersTimer = null;
		}

		private void PopulatePrinters()
		{
			bool enablePrinting = false;

			try
			{
				PrintQueueCollection printers = Printers.GetPrinters();
				PrintQueue defaultPrintQueue = LocalPrintServer.GetDefaultPrintQueue();
				string defaultPrinter = defaultPrintQueue.HostingPrintServer.Name + "\\" + defaultPrintQueue.Description;

				List<BackstageComboBoxItem> items = new List<BackstageComboBoxItem>();

				foreach (PrintQueue each in printers)
				{
					BackstageComboBoxItem item = new BackstageComboBoxItem();
					item.Tag = each;
					item.Header = each.Name;
					item.Description = GetFriendlyQueueStatus(each.QueueStatus) +
						(each.NumberOfJobs > 0 ? " (" + each.NumberOfJobs.ToString() + " document" +
						(each.NumberOfJobs != 1 ? "s" : "") + " waiting)" : "");
					item.Image = new BitmapImage(new Uri(GetIconQueueStatus(each.QueueStatus, each.Description == defaultPrinter), UriKind.Absolute));

					items.Add(item);
				}

				enablePrinting = items.Count > 0;

				if (enablePrinting)
				{
					PrintQueue tag = selectedPrinter;

					printersCombo.Items.Clear();

					foreach (object each in items)
						printersCombo.Items.Add(each);

					if (tag != null)
					{
						bool found = false;

						foreach (FrameworkElement each in printersCombo.Items)
							if (((PrintQueue)each.Tag).Description == tag.Description)
							{
								found = true;
								printersCombo.SelectedItem = each;
								break;
							}

						if (!found)
							printersCombo.SelectedIndex = 0;
					}
					else
						printersCombo.SelectedIndex = 0;
				}
			}
			catch
			{
				enablePrinting = false;
			}

			printButton.IsEnabled = enablePrinting;

			if (!enablePrinting)
			{
				printersCombo.Items.Clear();
				noPrintersItem.Header = "No Printers Installed";
				printersCombo.Items.Add(noPrintersItem);
				printersCombo.SelectedIndex = 0;
			}
		}

		private string GetFriendlyQueueStatus(PrintQueueStatus status)
		{
			switch (status)
			{
				case PrintQueueStatus.Busy:
					return "Busy";

				case PrintQueueStatus.DoorOpen:
					return "Door Open";

				case PrintQueueStatus.Error:
					return "Error";

				case PrintQueueStatus.Initializing:
					return "Initializing";

				case PrintQueueStatus.IOActive:
					return "Exchanging data with server";

				case PrintQueueStatus.ManualFeed:
					return "Waiting for manual feed";

				case PrintQueueStatus.None:
					return "Ready";

				case PrintQueueStatus.NotAvailable:
					return "Status Unavailable";

				case PrintQueueStatus.NoToner:
					return "Out of toner";

				case PrintQueueStatus.Offline:
					return "Offline";

				case PrintQueueStatus.OutOfMemory:
					return "Out of memory";

				case PrintQueueStatus.OutputBinFull:
					return "Output bin full";

				case PrintQueueStatus.PagePunt:
					return "Unable to print current page";

				case PrintQueueStatus.PaperJam:
					return "Paper Jam";

				case PrintQueueStatus.PaperOut:
					return "Out of paper";

				case PrintQueueStatus.PaperProblem:
					return "Paper Problem";

				case PrintQueueStatus.Paused:
					return "Print queue paused";

				case PrintQueueStatus.PendingDeletion:
					return "Deleting print job";

				case PrintQueueStatus.PowerSave:
					return "Power saving mode";

				case PrintQueueStatus.Printing:
					return "Printing";

				case PrintQueueStatus.Processing:
					return "Not Responding";

				case PrintQueueStatus.ServerUnknown:
					return "Error";

				case PrintQueueStatus.TonerLow:
					return "Low toner";

				case PrintQueueStatus.UserIntervention:
					return "Waiting for user action";

				case PrintQueueStatus.Waiting:
					return "Waiting for print job";

				case PrintQueueStatus.WarmingUp:
					return "Warming Up";

				default:
					// Control should never reach here
					return null;
			}
		}

		private string GetIconQueueStatus(PrintQueueStatus status, bool isDefault)
		{
			string data = "";
			string normal = isDefault ? "printer_ready" : "printer_lg";

			switch (status)
			{
				case PrintQueueStatus.Busy:
					data = "printer_busy";
					break;

				case PrintQueueStatus.DoorOpen:
					data = "printer_error";
					break;

				case PrintQueueStatus.Error:
					data = "printer_error";
					break;

				case PrintQueueStatus.Initializing:
					data = "printer_busy";
					break;

				case PrintQueueStatus.IOActive:
					data = "printer_busy";
					break;

				case PrintQueueStatus.ManualFeed:
					data = "printer_waiting";
					break;

				case PrintQueueStatus.None:
					data = normal;
					break;

				case PrintQueueStatus.NotAvailable:
					data = "printer_error";
					break;

				case PrintQueueStatus.NoToner:
					data = "printer_error";
					break;

				case PrintQueueStatus.Offline:
					data = "printer_error";
					break;

				case PrintQueueStatus.OutOfMemory:
					data = "printer_error";
					break;

				case PrintQueueStatus.OutputBinFull:
					data = "printer_error";
					break;

				case PrintQueueStatus.PagePunt:
					data = "printer_error";
					break;

				case PrintQueueStatus.PaperJam:
					data = "printer_error";
					break;

				case PrintQueueStatus.PaperOut:
					data = "printer_error";
					break;

				case PrintQueueStatus.PaperProblem:
					data = "printer_error";
					break;

				case PrintQueueStatus.Paused:
					data = "printer_waiting";
					break;

				case PrintQueueStatus.PendingDeletion:
					data = "printer_busy";
					break;

				case PrintQueueStatus.PowerSave:
					data = "printer_waiting";
					break;

				case PrintQueueStatus.Printing:
					data = "printer_busy";
					break;

				case PrintQueueStatus.Processing:
					data = "printer_busy";
					break;

				case PrintQueueStatus.ServerUnknown:
					data = "printer_error";
					break;

				case PrintQueueStatus.TonerLow:
					data = normal;
					break;

				case PrintQueueStatus.UserIntervention:
					data = "printer_error";
					break;

				case PrintQueueStatus.Waiting:
					data = "printer_busy";
					break;

				case PrintQueueStatus.WarmingUp:
					data = "printer_busy";
					break;

				default:
					// Control should never reach here
					return null;
			}

			return "pack://application:,,,/Daytimer.Images;component/Images/" + data + ".png";
		}

		private void ShowPreview()
		{
			documentViewer.Visibility = Visibility.Collapsed;

			FlowDocument doc = null;

			switch (_args.DocumentType)
			{
				case EditType.Appointment:
					doc = new AppointmentTemplate((Appointment)_args.DatabaseObject).GetFlowDocument();
					break;

				case EditType.Contact:
					doc = new ContactTemplate((Contact)_args.DatabaseObject).GetFlowDocument();
					break;

				case EditType.Note:
					doc = new NoteTemplate((NotebookPage)_args.DatabaseObject).GetFlowDocument();
					break;

				case EditType.Task:
					doc = new TaskTemplate((UserTask)_args.DatabaseObject).GetFlowDocument();
					break;

				default:
					break;
			}

			FlowDocument copy = doc.Copy();

			Size size = ((PaperSize)((FrameworkElement)paperSizeCombo.SelectedItem).Tag).Size;

			if ((size.Width < size.Height && orientationCombo.SelectedIndex == 1)
				|| (size.Width > size.Height && orientationCombo.SelectedIndex == 0))
				size = new Size(size.Height, size.Width);

			documentViewer.Document = FlowPaginator.ConvertFrom(
				copy, size,
				((PageMargin)((FrameworkElement)marginCombo.SelectedItem).Tag).Margin
			);

			documentViewer.Visibility = Visibility.Visible;
			documentViewer.FadeIn(AnimationHelpers.AnimationDuration);

			documentViewer.FitToWidth();
		}

		private void PopulatePaperSizes()
		{
			PaperSize[] sizes = PrintData.PaperSizes;
			Size? custom = Settings.PrintPaperSizeCustom;

			if (custom.HasValue)
			{
				PaperSize[] resized = new PaperSize[sizes.Length + 1];
				sizes.CopyTo(resized, 1);
				resized[0] = new PaperSize("Custom", custom.Value);
				sizes = resized;
			}

			foreach (PaperSize each in sizes)
			{
				BackstageComboBoxItem item = new BackstageComboBoxItem();
				item.Header = each.Description;

				Size size = each.Size;
				item.Description = size.Width.ToString() + "\" x " + size.Height.ToString() + "\"";
				item.Image = GetPaperIcon(size);

				item.Tag = each;

				paperSizeCombo.Items.Add(item);
			}

			paperSizeCombo.SelectedIndex = Settings.PrintPaperSize;
		}

		private Pen PaperBorder = new Pen(new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)), 1);
		private Pen MarginLine = new Pen(new SolidColorBrush(Color.FromArgb(255, 122, 161, 202)), 1);

		private ImageSource GetPaperIcon(Size size)
		{
			// Largest paper: 22

			double width = Math.Round(size.Width / 23 * 32);
			double height = Math.Round(size.Height / 23 * 32);

			double left = Math.Round((32 - width) / 2) + 0.5;
			double top = Math.Round((32 - height) / 2) + 0.5;

			RenderTargetBitmap renderTarget = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);

			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext drawingContext = drawingVisual.RenderOpen();
			drawingContext.DrawRectangle(Brushes.White, PaperBorder, new Rect(left, top, width, height));
			drawingContext.Close();

			renderTarget.Render(drawingVisual);
			renderTarget.Freeze();

			return renderTarget;
		}

		private void PopulateMargins()
		{
			PageMargin[] margins = PrintData.PageMargins;
			Thickness? custom = Settings.PrintMarginCustom;

			if (custom.HasValue)
			{
				PageMargin[] resized = new PageMargin[margins.Length + 1];
				margins.CopyTo(resized, 1);
				resized[0] = new PageMargin("Custom", custom.Value);
				margins = resized;
			}

			foreach (PageMargin each in margins)
			{
				BackstageComboBoxItem item = new BackstageComboBoxItem();
				item.Header = each.Description;

				Thickness margin = each.Margin;
				item.Description = "Top:\t" + margin.Top.ToString()
					+ "\"\tBottom:\t" + margin.Bottom.ToString()
					+ "\"\nLeft:\t" + margin.Left.ToString()
					+ "\"\tRight:\t" + margin.Right.ToString() + "\"";
				item.Image = GetMarginIcon(margin);

				item.Tag = each;

				marginCombo.Items.Add(item);
			}

			marginCombo.SelectedIndex = Settings.PrintMargin;
		}

		private const double marginIconWidth = 24;
		private const double marginIconHeight = 30;
		private const double marginIconLeft = 4.5;
		private const double marginIconTop = 1.5;
		private const double marginIconMultiplier = 3;

		private ImageSource GetMarginIcon(Thickness thickness)
		{
			RenderTargetBitmap renderTarget = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);

			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext drawingContext = drawingVisual.RenderOpen();

			// Background
			drawingContext.DrawRectangle(Brushes.White, PaperBorder,
				new Rect(marginIconLeft, marginIconTop, marginIconWidth, marginIconHeight));

			// Left margin
			drawingContext.DrawLine(
				MarginLine,
				new Point(Math.Round(1 + marginIconLeft + thickness.Left * marginIconMultiplier) - 0.5,
					marginIconTop),
				new Point(Math.Round(1 + marginIconLeft + thickness.Left * marginIconMultiplier) - 0.5,
					marginIconTop + marginIconHeight)
			);

			// Top margin
			drawingContext.DrawLine(
				MarginLine,
				new Point(marginIconLeft,
					Math.Round(1 + marginIconTop + thickness.Top * marginIconMultiplier) - 0.5),
				new Point(marginIconLeft + marginIconWidth,
					Math.Round(1 + marginIconTop + thickness.Top * marginIconMultiplier) - 0.5)
			);

			// Right margin
			drawingContext.DrawLine(
				MarginLine,
				new Point(Math.Round(marginIconLeft + marginIconWidth - thickness.Right * marginIconMultiplier) - 0.5,
					marginIconTop),
				new Point(Math.Round(marginIconLeft + marginIconWidth - thickness.Right * marginIconMultiplier) - 0.5,
					marginIconTop + marginIconHeight)
			);

			// Bottom margin
			drawingContext.DrawLine(
				MarginLine,
				new Point(marginIconLeft,
					Math.Round(marginIconTop + marginIconHeight - thickness.Bottom * marginIconMultiplier) - 0.5),
				new Point(marginIconLeft + marginIconWidth,
					Math.Round(marginIconTop + marginIconHeight - thickness.Bottom * marginIconMultiplier) - 0.5)
			);

			drawingContext.Close();

			renderTarget.Render(drawingVisual);
			renderTarget.Freeze();

			return renderTarget;
		}

		private void SavePrintSettings()
		{
			Settings.PrintPaperSize = paperSizeCombo.SelectedIndex;
			Settings.PrintMargin = marginCombo.SelectedIndex;
			Settings.PrintOrientation = orientationCombo.SelectedIndex;
			Settings.PrintCollation = collationCombo.SelectedIndex;
			Settings.PrintPagesPerSheet = pagesPerSheetCombo.SelectedIndex;
		}

		private void UpdateDuplexUI(PrintQueue printer, PrintCapabilities capabilities)
		{
			longDuplex.Visibility = capabilities.DuplexingCapability.Contains(Duplexing.TwoSidedLongEdge) ? Visibility.Visible : Visibility.Collapsed;
			shortDuplex.Visibility = capabilities.DuplexingCapability.Contains(Duplexing.TwoSidedShortEdge) ? Visibility.Visible : Visibility.Collapsed;

			if (printer.DefaultPrintTicket.Duplexing.HasValue)
			{
				switch (printer.DefaultPrintTicket.Duplexing.Value)
				{
					case Duplexing.OneSided:
						duplexCombo.SelectedItem = oneSide;
						break;

					case Duplexing.TwoSidedLongEdge:
						duplexCombo.SelectedItem = longDuplex;
						break;

					case Duplexing.TwoSidedShortEdge:
						duplexCombo.SelectedItem = shortDuplex;
						break;

					default:
						if (duplexCombo.SelectedItem != manualDuplex)
							duplexCombo.SelectedItem = oneSide;
						break;
				}
			}
			else
			{
				if (duplexCombo.SelectedItem != manualDuplex)
					duplexCombo.SelectedItem = oneSide;
			}
		}

		#endregion

		#region UI

		private void printButton_Click(object sender, RoutedEventArgs e)
		{
			int copies;

			if (!int.TryParse(copiesTextBox.Text, out copies) || copies <= 0)
			{
				TaskDialog td = new TaskDialog(Window.GetWindow(this), "Invalid Input",
					"The value entered in the Copies field is not a valid. The value must be between 1 and " + int.MaxValue.ToString() + " inclusive.",
					MessageType.Error);
				td.ShowDialog();

				copiesTextBox.SelectAll();
				copiesTextBox.Focus();

				return;
			}

			printButton.IsEnabled = false;
			SavePrintSettings();

			PrintQueue printer = selectedPrinter;

			FixedDocument document = (FixedDocument)documentViewer.Document;
			PrintTicket ticket = printer.DefaultPrintTicket;

			if (duplexCombo.SelectedItem == oneSide)
				ticket.Duplexing = Duplexing.OneSided;
			else if (duplexCombo.SelectedItem == longDuplex)
				ticket.Duplexing = Duplexing.TwoSidedLongEdge;
			else if (duplexCombo.SelectedItem == shortDuplex)
				ticket.Duplexing = Duplexing.TwoSidedShortEdge;

			ticket.PagesPerSheet = int.Parse(((FrameworkElement)pagesPerSheetCombo.SelectedItem).Tag.ToString());

			document.PrintTicket = ticket;

			bool manDuplex = duplexCombo.SelectedItem == manualDuplex;
			bool collated = collationCombo.SelectedIndex == 0;

			BackstageEvents.StaticUpdater.InvokeForceBackstageClose(this, EventArgs.Empty);

			if (!manDuplex)
				Dispatcher.BeginInvoke(() =>
				{
					XpsDocumentWriter xps = PrintQueue.CreateXpsDocumentWriter(printer);
					BackstageEvents.StaticUpdater.InvokePrintStarted(this, new PrintEventArgs(xps));

					ticket.Collation = collated ? Collation.Collated : Collation.Uncollated;
					ticket.CopyCount = copies;
					xps.WriteAsync(document, XpsDocumentNotificationLevel.ReceiveNotificationEnabled);
				});
			else
				Dispatcher.BeginInvoke(() =>
				{
					new ManualDuplex(document, printer, collated, copies);
				});
		}

		private PrintQueue selectedPrinter;

		private void printersCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsLoaded)
				return;

			if (e.AddedItems.Count == 0)
				return;

			PrintQueue printer = (PrintQueue)((FrameworkElement)e.AddedItems[0]).Tag;

			if (printer == null)
				return;

			if (selectedPrinter != null)
				if (selectedPrinter.Description == printer.Description)
					return;

			selectedPrinter = printer;

			PrintCapabilities capabilities = printer.GetPrintCapabilities();
			UpdateDuplexUI(printer, capabilities);
		}

		private void paperSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsLoaded)
				return;

			ShowPreview();
		}

		private void marginCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsLoaded)
				return;

			ShowPreview();
		}

		private void orientationCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!IsLoaded)
				return;

			ShowPreview();
		}

		private void customSize_Click(object sender, RoutedEventArgs e)
		{
			paperSizeCombo.IsDropDownOpen = false;

			CustomPaperSizeDialog paperSize = new CustomPaperSizeDialog();
			paperSize.Owner = Window.GetWindow(this);
			if (paperSize.ShowDialog() != true)
				return;

			Size custom = paperSize.SelectedSize;
			Settings.PrintPaperSizeCustom = custom;

			// In case "Custom" is already selected, this value will be true.
			bool needsPreviewRefresh = false;

			BackstageComboBoxItem item = (BackstageComboBoxItem)paperSizeCombo.Items[0];

			if (item.Header != "Custom")
			{
				item = new BackstageComboBoxItem();
				item.Header = "Custom";

				paperSizeCombo.Items.Insert(0, item);
			}
			else if (item.IsSelected)
				needsPreviewRefresh = true;

			item.Description = custom.Width.ToString() + "\" x " + custom.Height.ToString() + "\"";
			item.Image = GetPaperIcon(custom);
			item.Tag = new PaperSize("Custom", custom);

			item.IsSelected = true;

			if (needsPreviewRefresh)
				ShowPreview();
		}

		private void customMargin_Click(object sender, RoutedEventArgs e)
		{
			marginCombo.IsDropDownOpen = false;

			CustomMarginDialog margin = new CustomMarginDialog();
			margin.Owner = Window.GetWindow(this);
			if (margin.ShowDialog() != true)
				return;

			Thickness custom = margin.SelectedMargin;
			Settings.PrintMarginCustom = custom;

			// In case "Custom" is already selected, this value will be true.
			bool needsPreviewRefresh = false;

			BackstageComboBoxItem item = (BackstageComboBoxItem)marginCombo.Items[0];

			if (item.Header != "Custom")
			{
				item = new BackstageComboBoxItem();
				item.Header = "Custom";

				marginCombo.Items.Insert(0, item);
			}
			else if (item.IsSelected)
				needsPreviewRefresh = true;

			item.Description = "Top:\t" + custom.Top.ToString()
					+ "\"\tBottom:\t" + custom.Bottom.ToString()
					+ "\"\nLeft:\t" + custom.Left.ToString()
					+ "\"\tRight:\t" + custom.Right.ToString() + "\"";
			item.Image = GetMarginIcon(custom);
			item.Tag = new PageMargin("Custom", custom);

			item.IsSelected = true;

			if (needsPreviewRefresh)
				ShowPreview();
		}

		#endregion
	}
}
