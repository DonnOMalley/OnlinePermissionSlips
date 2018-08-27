using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using OnlinePermissionSlips.Models.DAL;

namespace OnlinePermissionSlips.Models
{
	public class EmailMessage
	{

		[Display(Name = "Email Domain")]
		[Required]
		public string EmailDomain { get; set; }

		[Display(Name = "API Key")]
		[Required]
		public string EmailAPIKey { get; set; }

		[Display(Name = "From Email")]
		[AllowHtml]
		[Required]
		public string FromAddress { get; set; }

		[Display(Name = "\"To\" Email List")]
		[DataType(DataType.MultilineText)]
		[Required]
		public string ToAddress { get; set; }

		[Display(Name = "\"bcc\" Email List")]
		[DataType(DataType.MultilineText)]
		public string BCCAddresses { get; set; }

		[Display(Name = "\"bcc\" Email List")]
		[DataType(DataType.MultilineText)]
		public string CCAddresses { get; set; }

		[Display(Name = "Subject")]
		[Required]
		public string EmailSubject { get; set; }

		public string MessageText { get; set; }

		[Display(Name = "Email Message")]
		[DataType(DataType.MultilineText)]
		[Required]
		public string HtmlMessageText { get; set; }

		[Display(Name = "Attachment(s)")]
		public List<EmailAttachment> Attachments { get; set; }

		public EmailMessage()
		{
			try
			{
				using (OnlinePermissionSlipEntities db = new OnlinePermissionSlipEntities())
				{
					SystemConfiguration config = db.SystemConfigurations.First();

					EmailDomain = config.EmailDomain;
					EmailAPIKey = config.EmailAPIKey;
					FromAddress = config.DefaultFromAddress;
				}

				Attachments = new List<EmailAttachment>();
			}
			catch (Exception ex)
			{
				throw new Exception("Error loading system configuration configuration", ex);
			}
		}
	}
}