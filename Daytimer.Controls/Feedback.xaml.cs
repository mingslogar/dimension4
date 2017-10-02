using Daytimer.Dialogs;
using Daytimer.Functions;
using Daytimer.Fundamentals.MetroProgress;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for ErrorReport.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class Feedback : DialogBase
	{
		public Feedback(FeedbackType type)
			: this(type, "")
		{

		}

		/// <summary>
		/// Create a new Feedback window with extra text added to the message.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="addend">the text to append to the end of the message</param>
		public Feedback(FeedbackType type, string addend)
		{
			InitializeComponent();

			_type = type;
			_addend = addend;

			if (type == FeedbackType.Smile)
			{
				header.Text = "We appreciate your feedback. What did you like?";
				detailsTip.Description = "Tell us what you liked about Daytimer.";
			}
			else if (type == FeedbackType.Error)
			{
				header.Text = "We're extremely sorry this happened. Can you give us more information about the problem?";
				detailsTip.Description = "Tell us what you were doing when the error occurred.";
			}

			SpellChecking.HandleSpellChecking(details);
		}

		private FeedbackType _type;
		private string _addend = "";

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);
			email.Focus();
		}

		private async void sendButton_Click(object sender, RoutedEventArgs e)
		{
			string _email = email.Text;

			if (!string.IsNullOrEmpty(_email))
			{
				if (!await Task.Factory.StartNew<bool>(() => { return (new RegexUtilities()).IsValidEmail(_email); }))
				{
					TaskDialog td = new TaskDialog(this, "Invalid Email", "The email address you entered is not valid.", MessageType.Error);
					td.ShowDialog();

					email.Focus();

					return;
				}
			}

			string trimmedDetails = details.Text.Trim();

			switch (_type)
			{
				case FeedbackType.Error:
					if (string.IsNullOrEmpty(trimmedDetails))
					{
						TaskDialog td = new TaskDialog(this, "Error Details",
							"Please tell us what you were doing when you encountered the problem. It will make debugging much easier on our side.",
							MessageType.Information, "_OK", "_No Thanks");

						if (td.ShowDialog() == true)
						{
							details.Focus();
							return;
						}
					}
					break;

				case FeedbackType.Frown:
					if (trimmedDetails.Split(new char[] { ' ' }, 10, StringSplitOptions.RemoveEmptyEntries).Length < 10)
					{
						TaskDialog td = new TaskDialog(this, "Insufficient Data",
							"Please give a detailed (10 words or more) description of the problem.", MessageType.Error);
						td.ShowDialog();

						details.Focus();

						return;
					}
					break;

				case FeedbackType.Smile:
					if (string.IsNullOrEmpty(trimmedDetails))
					{
						TaskDialog td = new TaskDialog(this, "Data Missing",
							"You forgot to tell us what you like about " + GlobalAssemblyInfo.AssemblyName + ".", MessageType.Error);
						td.ShowDialog();

						details.Focus();

						return;
					}
					break;

				default:
					break;
			}

			sendButton.Visibility = Visibility.Hidden;
			cancelButton.IsCancel = false;
			email.IsEnabled = false;
			details.IsEnabled = false;

			IndeterminateProgressBar animation = new IndeterminateProgressBar();
			animation.Margin = new Thickness(0, 0, 100, 0);
			footer.Children.Add(animation);

			string typeString = _type.ToString().ToLower();

			trimmedDetails = "This message is a" + (IsVowel(typeString[0]) ? "n" : "") + " " + typeString
				+ ".<br/><br/>" + ConvertToHtml(trimmedDetails)
				+ "<br/><br/>Additional information:<br/>"
				+ ConvertToHtml(_addend)
				+ "<br/><br/>" + ConvertToHtml(GetStats());

			sendThread = new Thread(() => { SendReport(_email, trimmedDetails); });
			sendThread.Priority = ThreadPriority.BelowNormal;
			sendThread.IsBackground = true;
			sendThread.Start();
		}

		private bool IsVowel(char c)
		{
			return c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u';
		}

		private string ConvertToHtml(string data)
		{
			return data.Replace("  ", "&nbsp;&nbsp;").Replace("\r\n",
				"<br/>").Replace("\n", "<br/>").Replace("\t",
				"&nbsp;&nbsp;&nbsp;&nbsp;");
		}

		Thread sendThread;

		private void SendReport(string email, string details)
		{
			try
			{
				WebClient myClient = new WebClient();

				NameValueCollection data = new NameValueCollection();
				data.Add("email", email);
				data.Add("message", details);

				myClient.UploadValues(GlobalData.Website + "/bug", data);

				Dispatcher.BeginInvoke(ShowSuccess);
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				Dispatcher.BeginInvoke(() => { ShowException(null); });
			}
			catch (Exception exc)
			{
				Dispatcher.BeginInvoke(() => { ShowException(exc.Message); });
			}
		}

		private void ShowException(string str)
		{
			sendButton.Visibility = Visibility.Visible;
			cancelButton.IsCancel = true;
			email.IsEnabled = true;
			details.IsEnabled = true;

			footer.Children.RemoveAt(footer.Children.Count - 1);

			if (str != null && IsLoaded)
			{
				TaskDialog td = new TaskDialog(this, "Failed to send feedback", str, MessageType.Error);
				td.ShowDialog();
			}
		}

		private void ShowSuccess()
		{
			footer.Children.RemoveAt(footer.Children.Count - 1);

			TaskDialog td = new TaskDialog(this, "Success", "Thanks for your input! We may not have the time to respond to each piece of feedback, but we will work hard to make sure it is reviewed.", MessageType.Information);
			td.ShowDialog();

			Close();
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (sendThread != null)
					sendThread.Abort();
			}
			catch { }
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			try
			{
				if (sendThread != null)
					sendThread.Abort();
			}
			catch { }
		}

		/// <summary>
		/// Get information about currently loaded assemblies and operating system.
		/// </summary>
		private string GetStats()
		{
			StringBuilder data = new StringBuilder();

			data.AppendLine("Operating System:   " + Environment.OSVersion.VersionString);
			data.AppendLine("Product Version:    " + Assembly.GetEntryAssembly().GetName().Version.ToString());

			data.AppendLine("\r\n\r\nModules:\r\n");

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly each in assemblies)
				data.AppendLine(each.FullName + " " + each.GetName().Version.ToString());

			return data.ToString();
		}
	}

	public enum FeedbackType : byte { Smile, Frown, Error };
}
