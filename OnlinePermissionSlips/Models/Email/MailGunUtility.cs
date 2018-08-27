using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using RestSharp;
using RestSharp.Authenticators;

namespace OnlinePermissionSlips.Models
{

	public static class MailGunUtility
	{
		public static IRestResponse SendSimpleMessage(EmailMessage email)
		{
			RestClient client = new RestClient
			{
				BaseUrl = new Uri("https://api.mailgun.net/v3"),
				Authenticator = new HttpBasicAuthenticator("api", email.EmailAPIKey)
			};
			RestRequest request = new RestRequest();
			request.AddParameter("domain", email.EmailDomain, ParameterType.UrlSegment);
			request.Resource = "{domain}/messages";
			request.AddParameter("from", email.FromAddress);
			request.AddParameter("to", email.ToAddress.Replace("\r\n", ",").Replace(";", ",").Replace(" ", ","));
			if(email.CCAddresses != null)
			{
				request.AddParameter("cc", email.BCCAddresses.Replace("\r\n", ",").Replace(";", ",").Replace(" ", ","));
			}
			if(email.BCCAddresses != null)
			{
				request.AddParameter("bcc", email.BCCAddresses.Replace("\r\n", ",").Replace(";", ",").Replace(" ", ","));
			}
			request.AddParameter("subject", email.EmailSubject);
			request.AddParameter("text", email.MessageText);
			request.AddParameter("html", email.HtmlMessageText);

			foreach (EmailAttachment a in email.Attachments)
			{
				if ((a.Attachment != null) && (a.Attachment.FileName.Trim().Length > 0))
				{
					MemoryStream ms = new MemoryStream();
					a.Attachment.InputStream.CopyTo(ms);
					request.AddFile("attachment", ms.ToArray(), Path.GetFileName(a.Attachment.FileName));
				}
			}
			request.Method = Method.POST;
			return client.Execute(request);
		}
	}
}