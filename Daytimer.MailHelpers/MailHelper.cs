using Daytimer.GoogleCalendarHelpers;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace Daytimer.MailHelpers
{
	public class MailHelper
	{
		/// <summary>
		/// Send an HTML email.
		/// </summary>
		/// <param name="from">A string representation of the email address used to send the email.</param>
		/// <param name="sender">A string representation of the email address which the email appears to be sent from.</param>
		/// <param name="to">A comma-delimited array of string representations of email addresses which the email should be sent to.</param>
		/// <param name="cc">A comma-delimited array of string representations of email addresses which a carbon copy of the email should be sent to.</param>
		/// <param name="bcc">A comma-delimited array of string representations of email addresses which a blind carbon copy of the email should be sent to.</param>
		/// <param name="subject">The subject of the email.</param>
		/// <param name="message">The body of the email, in HTML.</param>
		/// <param name="attachments">An array of attachments.</param>
		/// <param name="priority">The priority of the email.</param>
		public static void SendHtmlEmail(string from, string sender, string to, string cc, string bcc, string subject, string message, AttachmentCollection attachments, MailPriority priority)
		{
			MailMessage mailMessage = new MailMessage();
			mailMessage.To.Add(to);
			mailMessage.CC.Add(cc);
			mailMessage.Bcc.Add(bcc);
			mailMessage.Subject = subject;
			mailMessage.From = new MailAddress(from);
			mailMessage.Sender = new MailAddress(sender);
			mailMessage.IsBodyHtml = true;
			mailMessage.Body = message;
			mailMessage.Priority = priority;

			foreach (Attachment each in attachments)
				mailMessage.Attachments.Add(each);

			SendEmail(mailMessage, from, GoogleAccounts.Account(from).Password);
		}

		/// <summary>
		/// Send an email.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="email">The address for the email account used to send the email.</param>
		/// <param name="password">The password for the email account used to send the email.</param>
		public static void SendEmail(MailMessage message, string email, string password)
		{
			SmtpClient smtp = new SmtpClient
			{
				Host = "smtp.gmail.com",
				Port = 587,
				UseDefaultCredentials = false,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				Credentials = new NetworkCredential(email, password),
				EnableSsl = true,
				Timeout = 60000
			};

			smtp.Send(message);

			message.Dispose();
			smtp.Dispose();
		}

		/// <summary>
		/// Gets an attachment for a specified file name.
		/// </summary>
		/// <param name="filename">The file to create the attachment for.</param>
		/// <returns></returns>
		public static Attachment CreateAttachment(string filename)
		{
			FileInfo info = new FileInfo(filename);

			Attachment data = new Attachment(filename, MediaType(info.Extension));

			ContentDisposition disposition = data.ContentDisposition;
			disposition.CreationDate = info.CreationTime;
			disposition.ModificationDate = info.LastWriteTime;
			disposition.ReadDate = info.LastAccessTime;
			disposition.Size = info.Length;

			return data;
		}

		private static string MediaType(string extension)
		{
			switch (extension.ToLower())
			{
				case ".txt":
					return MediaTypeNames.Text.Plain;					

				case ".htm":
				case ".html":
					return MediaTypeNames.Text.Html;
					
				case ".rtf":
					return MediaTypeNames.Text.RichText;
					
				case ".xml":
					return MediaTypeNames.Text.Xml;
					
				case ".jpg":
				case ".jpe":
				case ".jpeg":
				case ".jfif":
					return MediaTypeNames.Image.Jpeg;
					
				case ".gif":
					return MediaTypeNames.Image.Gif;
					
				case ".png":
					return "image/png";
					
				case ".tif":
				case ".tiff":
					return MediaTypeNames.Image.Tiff;
					
				case ".pdf":
					return MediaTypeNames.Application.Pdf;
					
				case ".zip":
					return MediaTypeNames.Application.Zip;
					
				default:
					return MediaTypeNames.Application.Octet;
			}
		}
	}
}
