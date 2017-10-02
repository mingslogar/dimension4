using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Daytimer.PrintHelpers
{
	public class FlowPaginator
	{
		public static FixedDocument ConvertFrom(FlowDocument doc, Size pageSize, Thickness pageMargin)
		{
			FixedDocument fixedDoc = new FixedDocument();
			fixedDoc.DocumentPaginator.PageSize = InchesToDpi(pageSize);

			pageSize = InchesToDpi(GetCoreSize(pageSize, pageMargin));
			pageMargin = InchesToDpi(pageMargin);

			DocumentPaginator paginator = (doc as IDocumentPaginatorSource).DocumentPaginator;
			paginator.PageSize = pageSize;

			if (!paginator.IsPageCountValid)
				paginator.ComputePageCount();

			int pages = paginator.PageCount;

			for (int i = 0; i < pages; i++)
			{
				DocumentPage page = paginator.GetPage(i);

				FixedPage fp = new FixedPage();
				fp.Children.Add(new Border()
				{
					Width = pageSize.Width,
					Height = pageSize.Height,
					Margin = pageMargin,
					Background = new VisualBrush(page.Visual)
				});
				fixedDoc.Pages.Add(new PageContent() { Child = fp });
			}

			return fixedDoc;
		}

		private const double DPI = 96;

		private static Size GetCoreSize(Size paperSize, Thickness margin)
		{
			return new Size(paperSize.Width - margin.Left - margin.Right, paperSize.Height - margin.Top - margin.Bottom);
		}

		private static Size InchesToDpi(Size size)
		{
			return new Size(
				size.Width * DPI,
				size.Height * DPI
			);
		}

		private static Thickness InchesToDpi(Thickness thickness)
		{
			return new Thickness(
				thickness.Left * DPI,
				thickness.Top * DPI,
				thickness.Right * DPI,
				thickness.Bottom * DPI
			);
		}
	}
}
