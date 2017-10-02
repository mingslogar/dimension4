using Daytimer.DatabaseHelpers.Contacts;
using Daytimer.Functions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Daytimer.DatabaseHelpers.Templates
{
	public class ContactTemplate
	{
		public ContactTemplate(Contact contact)
		{
			_contact = contact;
		}

		private Contact _contact;

		public FlowDocument GetFlowDocument()
		{
			FlowDocument doc = (FlowDocument)Application.LoadComponent(
				new Uri("/Daytimer.DatabaseHelpers;component/Templates/ContactTemplate.xaml",
					UriKind.Relative));

			((Image)doc.FindName("Tile")).Source = _contact.Tile;
			((Paragraph)doc.FindName("Name")).Inlines.Add(_contact.Name.ToString());
			((Paragraph)doc.FindName("JobDescription")).Inlines.Add(_contact.WorkDescription);

			Email[] email = _contact.Emails;

			if (email != null)
			{
				Paragraph emailBlock = (Paragraph)doc.FindName("Email");

				foreach (Email each in email)
				{
					Hyperlink h = new Hyperlink(new Run(each.Address));
					h.NavigateUri = new Uri("mailto:" + each.Address);

					emailBlock.Inlines.Add(h);
					emailBlock.Inlines.Add(" (" + each.Type + ")");
					emailBlock.Inlines.Add(new LineBreak());
				}
			}

			PhoneNumber[] phone = _contact.PhoneNumbers;

			if (phone != null)
			{
				Paragraph phoneBlock = (Paragraph)doc.FindName("Phone");

				foreach (PhoneNumber each in phone)
				{
					phoneBlock.Inlines.Add(each.Number + " (" + each.Type + ")");
					phoneBlock.Inlines.Add(new LineBreak());
				}
			}

			IM[] im = _contact.IM;

			if (im != null)
			{
				Paragraph imBlock = (Paragraph)doc.FindName("IM");

				foreach (IM each in im)
				{
					imBlock.Inlines.Add(each.Address + " (" + each.Type + ")");
					imBlock.Inlines.Add(new LineBreak());
				}
			}

			Work work = _contact.Work;

			if (work != null && (!string.IsNullOrEmpty(work.Office) || !string.IsNullOrEmpty(work.Company)))
			{
				Paragraph workBlock = (Paragraph)doc.FindName("Work");

				if (!string.IsNullOrEmpty(work.Office))
				{
					workBlock.Inlines.Add("Office: " + work.Office);
					workBlock.Inlines.Add(new LineBreak());
				}

				if (!string.IsNullOrEmpty(work.Company))
				{
					workBlock.Inlines.Add("Company: " + work.Company);
					workBlock.Inlines.Add(new LineBreak());
				}
			}

			Address[] address = _contact.Addresses;

			if (address != null)
			{
				Paragraph addressBlock = (Paragraph)doc.FindName("Address");

				int length = address.Length;
				int count = 0;

				foreach (Address each in address)
				{
					string[] split = each.ToString().Split('\n');

					foreach (string s in split)
					{
						addressBlock.Inlines.Add(s);
						addressBlock.Inlines.Add(new LineBreak());
					}

					addressBlock.Inlines.Add("(" + each.Type + ")");
					addressBlock.Inlines.Add(new LineBreak());

					if (++count < length)
						addressBlock.Inlines.Add(new LineBreak());
				}
			}

			Website[] website = _contact.Websites;

			if (website != null)
			{
				Paragraph websiteBlock = (Paragraph)doc.FindName("Website");

				foreach (Website each in website)
				{
					Hyperlink h = new Hyperlink(new Run(each.Url));
					h.NavigateUri = new Uri("mailto:" + each.Url);

					websiteBlock.Inlines.Add(h);
					websiteBlock.Inlines.Add(" (" + each.Type + ")");
					websiteBlock.Inlines.Add(new LineBreak());
				}
			}

			SpecialDate[] specialDate = _contact.SpecialDates;

			if (specialDate != null)
			{
				Paragraph specialDateBlock = (Paragraph)doc.FindName("SpecialDate");

				foreach (SpecialDate each in specialDate)
				{
					specialDateBlock.Inlines.Add(each.Date.ToString("MMMM d, yyyy"));
					specialDateBlock.Inlines.Add(" (" + each.Type + ")");
					specialDateBlock.Inlines.Add(new LineBreak());
				}
			}

			Section notes = (Section)doc.FindName("Notes");

			FlowDocument notesDoc = _contact.NotesDocument;

			if (notesDoc != null)
			{
				BlockCollection blocks = _contact.NotesDocument.Copy().Blocks;

				while (blocks.Count > 0)
					notes.Blocks.Add(blocks.FirstBlock);
			}

			return doc;
		}

		public string[] GetTextDocument()
		{
			string[] data = new string[] { "Name:\t" + _contact.Name.ToString() };

			Email[] emails = _contact.Emails;

			if (emails != null)
				foreach (Email each in emails)
				{
					Array.Resize(ref data, data.Length + 1);
					data[data.Length - 1] = "Email:\t" + each.Address + " (" + each.Type + ")";
				}

			PhoneNumber[] phones = _contact.PhoneNumbers;

			if (phones != null)
				foreach (PhoneNumber each in phones)
				{
					Array.Resize(ref data, data.Length + 1);
					data[data.Length - 1] = each.Type + ":\t" + each.Number;
				}

			IM[] ims = _contact.IM;

			if (ims != null)
			{
				foreach (IM each in ims)
				{
					Array.Resize(ref data, data.Length + 1);
					data[data.Length - 1] = each.Type + ":\t" + each.Address;
				}
			}

			Work work = _contact.Work;

			if (work != null)
			{
				if (work.Title != "")
				{
					Array.Resize(ref data, data.Length + 1);
					data[data.Length - 1] = "Title:\t" + work.Title;
				}

				if (work.Department != "")
				{
					Array.Resize(ref data, data.Length + 1);
					data[data.Length - 1] = "Department:\t" + work.Department;
				}

				if (work.Company != "")
				{
					Array.Resize(ref data, data.Length + 1);
					data[data.Length - 1] = "Company:\t" + work.Company;
				}

				if (work.Office != "")
				{
					Array.Resize(ref data, data.Length + 1);
					data[data.Length - 1] = "Office:\t" + work.Office;
				}
			}

			Address[] addresses = _contact.Addresses;

			if (addresses != null)
				foreach (Address each in addresses)
				{
					Array.Resize(ref data, data.Length + 1);
					data[data.Length - 1] = each.Type + " Address:\t" + each.ToString();
				}

			Website[] websites = _contact.Websites;

			if (websites != null)
				foreach (Website each in websites)
				{
					Array.Resize(ref data, data.Length + 1);
					data[data.Length - 1] = each.Type + ":\t" + each.Url;
				}

			SpecialDate[] specialDates = _contact.SpecialDates;

			if (specialDates != null)
				foreach (SpecialDate each in specialDates)
				{
					Array.Resize(ref data, data.Length + 1);
					data[data.Length - 1] = each.Type + ":\t" + each.Date.ToShortDateString();
				}

			return data;
		}
	}
}
