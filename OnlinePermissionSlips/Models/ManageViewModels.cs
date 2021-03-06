﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using OnlinePermissionSlips.Models.DAL;

namespace OnlinePermissionSlips.Models
{
	public class IndexViewModel
	{
		public bool EmailConfirmed { get; set; }
		public bool IsSubscribed { get; set; }
		public bool HasPassword { get; set; }
		public IList<UserLoginInfo> Logins { get; set; }
		public string PhoneNumber { get; set; }
		public bool TwoFactor { get; set; }
		public bool BrowserRemembered { get; set; }

		[Display(Name = "First Name")]
		public string FirstName { get; set; }
		[Display(Name = "Middle Name")]
		public string MiddleName { get; set; }
		[Display(Name = "Last Name")]
		public string LastName { get; set; }
		[Display(Name = "Email")]
		public string Email { get; set; }
		[Display(Name = "User Role(s)")]
		public List<IdentityRole> Roles { get; set; }
		[Display(Name = "School(s)")]
		public List<School> Schools { get; set; }
		[Display(Name = "Student(s)")]
		public List<Student> Students { get; set; }
		[Display(Name = "Class Room(s)")]
		public List<ClassRoom> Classes { get; set; }
	}

	public class EditViewModel
	{
		[Required]
		[Display(Name = "User Name")]
		public string UserName { get; set; }
		[Display(Name = "First Name")]
		public string FirstName { get; set; }
		[Display(Name = "Middle Name")]
		public string MiddleName { get; set; }
		[Required]
		[Display(Name = "Last Name")]
		public string LastName { get; set; }
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }
	}

	public class ManageLoginsViewModel
	{
		public IList<UserLoginInfo> CurrentLogins { get; set; }
		public IList<AuthenticationDescription> OtherLogins { get; set; }
	}

	public class FactorViewModel
	{
		public string Purpose { get; set; }
	}

	public class SetPasswordViewModel
	{
		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "New password")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
	}

	public class ChangePasswordViewModel
	{
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Current password")]
		public string OldPassword { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "New password")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
	}

	public class AddStudentViewModel
	{
		public int? ID { get; set; }

		[Required]
		[Display(Name="Student ID Number")]
		public int StudentNumber { get; set; }
		[Required]
		[Display(Name = "Student Full Name")]
		public string FullName { get; set; }
		[Required]
		[Display(Name = "Student School")]
		public int SchoolID { get; set; }
	}

	public class RemoveStudentViewModel
	{
		[Required]
		[Display(Name = "Student ID Number")]
		public int StudentNumber { get; set; }

		[Display(Name = "Student Full Name")]
		public string FullName { get; set; }
	}

	public class AddPhoneNumberViewModel
	{
		[Required]
		[Phone]
		[Display(Name = "Phone Number")]
		public string Number { get; set; }
	}

	public class VerifyPhoneNumberViewModel
	{
		[Required]
		[Display(Name = "Code")]
		public string Code { get; set; }

		[Required]
		[Phone]
		[Display(Name = "Phone Number")]
		public string PhoneNumber { get; set; }
	}

	public class ConfigureTwoFactorViewModel
	{
		public string SelectedProvider { get; set; }
		public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
	}
}