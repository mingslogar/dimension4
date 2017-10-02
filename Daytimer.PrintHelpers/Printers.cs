using System.Printing;

namespace Daytimer.PrintHelpers
{
	public class Printers
	{
		public static PrintQueueCollection GetPrinters()
		{
			return new LocalPrintServer().GetPrintQueues();
		}
	}
}
