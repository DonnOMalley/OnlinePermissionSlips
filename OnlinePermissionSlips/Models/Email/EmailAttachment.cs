using System.ComponentModel.DataAnnotations;
using System.Web;

namespace OnlinePermissionSlips.Models
{
	public class EmailAttachment
	{
		[Display(Name = "Attachment")]
		public HttpPostedFileBase Attachment { get; set; }
	}
}