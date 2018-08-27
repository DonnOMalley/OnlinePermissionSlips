using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using OnlinePermissionSlips.Models.DAL;
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
				BaseUrl = new Uri(Common.MailGunAPIUrl),
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

		public static bool IsSubscribed(string EmailAddress)
		{
			SystemConfiguration config = null;
			RestClient client = null;
			RestRequest request = null;
			HttpStatusCode UnsubscribeRequest;

			try
			{
				using (OnlinePermissionSlipEntities db = new OnlinePermissionSlipEntities())
				{
					config = db.SystemConfigurations.First();

					client = new RestClient
					{
						BaseUrl = new Uri(Common.MailGunAPIUrl),
						Authenticator = new HttpBasicAuthenticator("api", config.EmailAPIKey)
					};
					request = new RestRequest();
					request.AddParameter("domain", config.EmailDomain, ParameterType.UrlSegment);
					request.AddParameter("address", EmailAddress, ParameterType.UrlSegment);
					request.Resource = "{domain}/unsubscribes/{address}";

					request.Method = Method.GET;
				}

				UnsubscribeRequest = client.Execute(request).StatusCode;
				if ((UnsubscribeRequest != HttpStatusCode.NotFound) && (UnsubscribeRequest != HttpStatusCode.OK))
				{
					throw new Exception("Unexpected http status code returned from email API service for unsubscribes");
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to query email service for unsubscribe validation", ex);
			}

			return UnsubscribeRequest == HttpStatusCode.NotFound;
		}

		public static bool DeleteUnsubscribed(string EmailAddress)
		{
			SystemConfiguration config = null;
			RestClient client = null;
			RestRequest request = null;
			HttpStatusCode DeleteUnsubscribeRequest;

			try
			{
				using (OnlinePermissionSlipEntities db = new OnlinePermissionSlipEntities())
				{
					config = db.SystemConfigurations.First();

					client = new RestClient
					{
						BaseUrl = new Uri(Common.MailGunAPIUrl),
						Authenticator = new HttpBasicAuthenticator("api", config.EmailAPIKey)
					};

					request = new RestRequest();
					request.AddParameter("domain", config.EmailDomain, ParameterType.UrlSegment);
					request.AddParameter("address", EmailAddress, ParameterType.UrlSegment);
					request.Resource = "{domain}/unsubscribes/{address}";

					request.Method = Method.DELETE;
				}

				DeleteUnsubscribeRequest = client.Execute(request).StatusCode;
				if (DeleteUnsubscribeRequest != HttpStatusCode.OK)
				{
					throw new Exception("Unexpected http status code returned from email API service for unsubscribes");
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to query email service for unsubscribe validation", ex);
			}

			return true;
		}
	}
}