using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace OnlinePermissionSlips.Models.DAL
{

	#region Helper Interface/Classes

	public interface IDatedEntity
	{
		DateTime CreateDateTime { get; set; }
		DateTime ModifiedDateTime { get; set; }
		DateTime? DisabledDateTime { get; set; }

		void SetCreated();
		void SetDisabled();
	}

	public class DatedEntity
	{
		public void SetCreated(IDatedEntity DatedEntity)
		{
			DateTime currentDateTime = DateTime.Now;
			DatedEntity.CreateDateTime = currentDateTime;
			DatedEntity.ModifiedDateTime = currentDateTime;
			DatedEntity.DisabledDateTime = null;
		}

		//Potentially Unnecessary
		public void SetModified(IDatedEntity DatedEntity)
		{
			DateTime currentDateTime = DateTime.Now;
			DatedEntity.ModifiedDateTime = currentDateTime;
		}

		public void SetDisabled(IDatedEntity DatedEntity)
		{
			DateTime currentDateTime = DateTime.Now;
			DatedEntity.ModifiedDateTime = currentDateTime;
			DatedEntity.DisabledDateTime = currentDateTime;
		}
	}

	#endregion

	#region Metadata Interfaces for Entity Framework Classes

	public interface ISystemConfiguration
	{
		[Required]
		[Display(Name = "ID")]
		int Id { get; set; }

		[Required]
		[Display(Name = "MailGun Email Domain")]
		string EmailDomain { get; set; }

		[Required]
		[Display(Name = "MailGun Email API Key")]
		string EmailAPIKey { get; set; }

		[Required]
		[Display(Name = "Default Email 'From' Address")]
		[EmailAddress]
		string DefaultFromAddress { get; set; }
	}

	public interface ICreateTeacher
	{
		[Required]
		string Id { get; set; }
		[Required]
		[Display(Name = "User Name")]
		string UserName { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		string ConfirmPassword { get; set; }

		[Required]
		[Display(Name = "First Name")]
		string FirstName { get; set; }

		[Display(Name = "Middle Name")]
		string MiddleName { get; set; }

		[Required]
		[Display(Name = "Last Name")]
		string LastName { get; set; }

		[Required]
		[EmailAddress]
		string Email { get; set; }

		[Display(Name = "Phone Number")]
		string PhoneNumber { get; set; }
	}

	public interface IAspNetUser
	{
		[Required]
		string Id { get; set; }
		[Required]
		[Display(Name = "User Name")]
		string UserName { get; set; }

		[Required]
		[Display(Name = "First Name")]
		string FirstName { get; set; }

		[Display(Name = "Middle Name")]
		string MiddleName { get; set; }

		[Required]
		[Display(Name = "Last Name")]
		string LastName { get; set; }

		[Display(Name = "Full Name")]
		string FullName { get; }

		[Required]
		[EmailAddress]
		string Email { get; set; }

		[Display(Name = "Email Confirmed")]
		bool EmailConfirmed { get; set; }

		[Display(Name = "Phone Number")]
		string PhoneNumber { get; set; }

		[Display(Name = "Phone Confirmed")]
		bool PhoneNumberConfirmed { get; set; }

		[Display(Name = "Two Factor Auth Enabled")]
		bool TwoFactorEnabled { get; set; }

		[Display(Name = "Lockout End Date")]
		Nullable<DateTime> LockoutEndDateUtc { get; set; }

		[Display(Name = "Lockout Enabled")]
		bool LockoutEnabled { get; set; }

		[Display(Name = "Access Failed Count")]
		int AccessFailedCount { get; set; }
	}

	public interface ISchoolMetaData
	{
		[Required]
		[Display(Name = "School Name")]
		string SchoolName { get; set; }

		[Required]
		[Display(Name = "UserName")]
		string AdminUserName { get; set; }

		[Required]
		[Display(Name = "First Name")]
		string FirstName { get; set; }

		[Display(Name = "Middle Name")]
		string MiddleName { get; set; }

		[Required]
		[Display(Name = "Last Name")]
		string LastName { get; set; }

		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		string Email { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		string AdminPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("AdminPassword", ErrorMessage = "The password and confirmation password do not match.")]
		string ConfirmAdminPassword { get; set; }
	}

	public interface IClassRoomMetaData
	{
		[Required]
		[Display(Name = "Room Number")]
		string RoomNumber { get; set; }

		[Required]
		[Display(Name = "School")]
		int SchoolID { get; set; }

		[Required]
		[Display(Name = "Teacher")]
		string TeacherUserID { get; set; }

		[Display(Name = "Created")]
		DateTime CreateDateTime { get; set; }

		[Display(Name = "Disabled")]
		DateTime? DisabledDateTime { get; set; }
	}

	public interface IStudent
	{
		[Required]
		[Display(Name = "Student ID")]
		int StudentNumber { get; set; }

		[Required]
		[Display(Name = "Full Name")]
		string FullName { get; set; }

		[Required]
		[Display(Name = "School")]
		int SchoolID { get; set; }

		[Required]
		[Display(Name = "Class Room")]
		int ClassRoomID { get; set; }

		[Display(Name = "Guardian 1 Temp Email")]
		string Guardian1TempEmail { get; set; }

		[Display(Name = "Guardian 2 Temp Email")]
		string Guardian2TempEmail { get; set; }
	}

	public interface ICategory
	{
		[Required]
		[Display(Name = "Category Name")]
		string Name { get; set; }
	}

	public interface ITemplate
	{
		[Required]
		[Display(Name = "School")]
		int? SchoolID { get; set; }

		[Display(Name = "Class")]
		int? ClassRoomID { get; set; }

		[Required]
		[Display(Name = "Category")]
		int CategoryID { get; set; }

		[Required]
		[Display(Name = "Template Name")]
		string Name { get; set; }

		[Required]
		[Display(Name = "Location")]
		string Location { get; set; }

		[Required]
		[Display(Name = "Cost")]
		decimal? Cost { get; set; }

		[Required]
		[Display(Name = "Chaperone Required")]
		bool RequireChaperone { get; set; }

		[Required]
		[Display(Name = "Chaperone Requires Background Check")]
		bool RequireChaperoneBackgroundCheck { get; set; }

		[Display(Name = "Disable")]
		bool Disable { get; set; }

		[JsonIgnore]
		byte[] ConsistencyCheck { get; set; }

		[JsonIgnore]
		ICollection<PermissionSlip> PermissionSlips { get; set; }

		[JsonIgnore]
		Category Category { get; set; }

		[JsonIgnore]
		ClassRoom ClassRoom { get; set; }

		[JsonIgnore]
		School School { get; set; }
	}

	public interface IPermissionSlip
	{
		[Display(Name = "Template")]
		int? PermissionSlipTemplateID { get; set; }

		[Required(ErrorMessage = "Category must be selected")]
		[Display(Name = "Category")]
		int PermissionSlipCategoryID { get; set; }

		[Display(Name = "Class Room")]
		int ClassRoomID { get; set; }

		[Required(ErrorMessage = "Must provide a Name")]
		string Name { get; set; }

		[Required(ErrorMessage = "Must provide a Location")]
		string Location { get; set; }

		[Required(ErrorMessage = "Start Date/Time must be selected")]
		[Display(Name = "Start Date/Time")]
		DateTime StartDateTime { get; set; }

		[Required(ErrorMessage = "End Date/Time must be selected")]
		[Display(Name = "End Date/Time")]
		DateTime EndDateTime { get; set; }

		[Range(0, Double.MaxValue, ErrorMessage = "Must Provide a cost value. Use 0(zero) for no cost")]
		decimal Cost { get; set; }

		[Display(Name = "Need Chaperones")]
		bool RequireChaperone { get; set; }

		[Display(Name = "Chaperone Background Check Req'd")]
		bool RequireChaperoneBackgroundCheck { get; set; }
	}

	public interface ICreatePermissionSlip
	{
		[Display(Name = "School")]
		int SchoolID { get; set; }

		[Required(ErrorMessage = "Class Room must be selected")]
		[Display(Name = "Class Room")]
		int ClassRoomID { get; set; }

		[Display(Name = "Template")]
		int? PermissionSlipTemplateID { get; set; }

		[Required(ErrorMessage = "Category must be selected")]
		[Display(Name = "Category")]
		int PermissionSlipCategoryID { get; set; }

		[Required(ErrorMessage = "Must provide a Name")]
		string Name { get; set; }

		[Required(ErrorMessage = "Must provide a Location")]
		string Location { get; set; }

		[Required(ErrorMessage = "Start Date/Time must be selected")]
		[Display(Name = "Start Date/Time")]
		DateTime StartDateTime { get; set; }

		[Required(ErrorMessage = "End Date/Time must be selected")]
		[Display(Name = "End Date/Time")]
		DateTime EndDateTime { get; set; }

		[Range(0, Double.MaxValue, ErrorMessage = "Must Provide a cost value. Use 0(zero) for no cost")]
		decimal Cost { get; set; }

		[Display(Name = "Need Chaperones")]
		bool RequireChaperone { get; set; }

		[Display(Name = "Chaperone Background Check Req'd")]
		bool RequireChaperoneBackgroundCheck { get; set; }

		[Display(Name = "Email Guardians for Permission")]
		bool EmailGuardians { get; set; }
	}

	public interface IGuardianApproval
	{
		#region PermissionSlipData

		string Name { get; set; }
		string Location { get; set; }
		[Display(Name = "Start Date/Time")]
		DateTime StartDateTime { get; set; }
		[Display(Name = "End Date/Time")]
		DateTime EndDateTime { get; set; }
		decimal Cost { get; set; }
		bool RequireChaperone { get; set; }
		[Display(Name = "Chaperone Background Check Req'd")]
		bool RequireChaperoneBackgroundCheck { get; set; }

		#endregion

		[Required]
		[Display(Name = "Student Name")]
		string StudentFullName { get; set; }

		[Required]
		[Display(Name = "Guardian Name")]
		string GuardianName { get; set; }

		[Required]
		[Display(Name = "Confirm Guardian Email")]
		string GuardianEmail { get; set; }

		[Required]
		[Display(Name = "Approve?")]
		bool Approved { get; set; }

		[Display(Name = "Can Chaperone?")]
		bool CanChaperone { get; set; }

		[Display(Name = "Any Health/Dietary/Access accommodations?")]
		string SpecialHealthDietaryAccessConsiderations { get; set; }

		[Required]
		[Phone]
		[Display(Name = "Daytime Phone")]
		string DaytimePhone { get; set; }

		[Required]
		[Phone]
		[Display(Name = "Emergency Phone")]
		string EmergencyPhone { get; set; }

		[Required]
		[Display(Name = "Signature")]
		string SignatureData { get; set; }
	}

	public interface IPermissionSlipPrint
	{
		string Name { get; set; }

		string Location { get; set; }

		[Display(Name = "Start Date/Time")]
		DateTime StartDateTime { get; set; }

		[Display(Name = "End Date/Time")]
		DateTime EndDateTime { get; set; }

		decimal Cost { get; set; }

		bool RequireChaperone { get; set; }

		[Display(Name = "Chaperone Background Check Req'd")]
		bool RequireChaperoneBackgroundCheck { get; set; }

		[Required]
		[Display(Name = "Student Name")]
		string StudentFullName { get; set; }

		[Required]
		[Display(Name = "Guardian Name")]
		string GuardianName { get; set; }

		[Required]
		[Display(Name = "Approve?")]
		bool Approved { get; set; }

		[Display(Name = "Can Chaperone?")]
		bool CanChaperone { get; set; }

		[Display(Name = "Any Health/Dietary/Access accommodations?")]
		string SpecialHealthDietaryAccessConsiderations { get; set; }

		[Required]
		[Phone]
		[Display(Name = "Daytime Phone")]
		string DaytimePhone { get; set; }

		[Required]
		[Phone]
		[Display(Name = "Emergency Phone")]
		string EmergencyPhone { get; set; }

		[Required]
		[Display(Name = "Signature")]
		string SignatureData { get; set; }
	}

	public interface IGuardianPermissionSlipsView
	{
		int SchoolID { get; set; }
		[Display(Name = "School")]
		string SchoolName { get; set; }
		string UserID { get; set; }
		string FirstName { get; set; }
		string MiddleName { get; set; }
		string LastName { get; set; }
		string Email { get; set; }
		bool EmailConfirmed { get; set; }
		string PhoneNumber { get; set; }
		string RoleName { get; set; }
		int ClassID { get; set; }
		string RoomNumber { get; set; }
		string TeacherUserID { get; set; }
		int EventID { get; set; }
		[Display(Name = "Event")]
		string EventName { get; set; }
		[Display(Name = "Location")]
		string EventLocation { get; set; }
		[Display(Name = "Start")]
		DateTime StartDateTime { get; set; }
		[Display(Name = "End")]
		DateTime EndDateTime { get; set; }
		decimal Cost { get; set; }
		bool RequireChaperone { get; set; }
		bool RequireChaperoneBackgroundCheck { get; set; }
		[Display(Name = "Approved")]
		bool? Approved { get; set; }
		[Display(Name = "Approval By")]
		string GuardianName { get; set; }
		bool? CanChaperone { get; set; }
		string SpecialHealthDietaryAccessConsiderations { get; set; }
		string DaytimePhone { get; set; }
		string EmergencyPhone { get; set; }
		DateTime? CreatedDateTime { get; set; }
		byte[] GuardianApprovalSignature { get; set; }
		[Display(Name = "Student")]
		string StudentName { get; set; }
		[Display(Name = "Class")]
		string ClassName { get; set; }
	}

	#endregion

	#region "Implementations of all Metadata"

	[MetadataType(typeof(ISystemConfiguration))]
	public partial class SystemConfiguration: ISystemConfiguration { }

	[MetadataType(typeof(ISchoolMetaData))]
	public partial class School : ISchoolMetaData
	{
		public string AdminUserName { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string AdminPassword { get; set; }
		public string ConfirmAdminPassword { get; set; }
	}

	[MetadataType(typeof(IClassRoomMetaData))]
	public partial class ClassRoom : DatedEntity, IClassRoomMetaData, IDatedEntity
	{
		public void SetCreated()
		{
			SetCreated(this);
		}

		public void SetModified()
		{
			SetModified(this);
		}

		public void SetDisabled()
		{
			SetDisabled(this);
		}

		public string GetClassName()
		{
			AspNetUser Teacher = this.AspNetUser;
			string ClassName = "";
			try
			{
				if (Teacher == null)
				{
					Teacher = new OnlinePermissionSlipEntities().AspNetUsers.Where(u => u.Id == TeacherUserID).FirstOrDefault();
					if (Teacher == null) { throw new Exception("Null Teacher"); }
				}

				ClassName = Teacher.FullName + "(" + RoomNumber + ")";
			}
			catch (Exception)
			{
				throw;
			}
			return ClassName;
		}
	}

	[MetadataType(typeof(IStudent))]
	public partial class Student { }

	[MetadataType(typeof(ICategory))]
	public partial class Category { }

	[MetadataType(typeof(ITemplate))]
	public partial class Template : DatedEntity, ITemplate, IDatedEntity
	{
		public bool Disable { get; set; }

		public void SetCreated()
		{
			SetCreated(this);
		}

		public void SetDisabled()
		{
			SetDisabled(this);
		}
	}

	[MetadataType(typeof(ICreateTeacher))]
	public class CreateTeacher
	{
		public int Id { get; set; }
		public int SchoolID { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string ConfirmPassword { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
	}

	[MetadataType(typeof(IAspNetUser))]
	public partial class AspNetUser : IAspNetUser
	{
		public string FullName
		{
			get
			{
				string fullName = (FirstName ?? "").Trim();
				if ((MiddleName != null) && (MiddleName.Trim().Length > 0))
				{
					if (fullName.Length > 0)
					{
						fullName += " ";
					}
					fullName += MiddleName;
				}

				if ((LastName != null) && (LastName.Trim().Length > 0))
				{
					if (fullName.Length > 0)
					{
						fullName += " ";
					}
					fullName += LastName;
				}

				return fullName;
			}
		}
	}

	[MetadataType(typeof(IPermissionSlip))]
	public partial class PermissionSlip { }

	[MetadataType(typeof(ICreatePermissionSlip))]
	public class CreatePermissionSlip : PermissionSlip
	{
		public int SchoolID { get; set; }
		public bool EmailGuardians { get; set; }
	}

	[MetadataType(typeof(IGuardianApproval))]
	public partial class GuardianApproval
	{
		#region Permission Slip Data
		public string Name { get; set; }
		public string Location { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public decimal Cost { get; set; }
		public bool RequireChaperone { get; set; }
		public bool RequireChaperoneBackgroundCheck { get; set; }
		#endregion

		public string StudentFullName { get; set; }
		public string SignatureData { get; set; }
		public string ExistingSignatureData { get; set; }
		public byte[] ExistingSignature { get; set; }
	}

	[MetadataType(typeof(IPermissionSlipPrint))]
	public class PermissionSlipPrint
	{
		public string Name { get; set; }
		public string Location { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public decimal Cost { get; set; }
		public bool RequireChaperone { get; set; }
		public bool RequireChaperoneBackgroundCheck { get; set; }
		public string StudentFullName { get; set; }
		public string GuardianName { get; set; }
		public bool Approved { get; set; }
		public bool CanChaperone { get; set; }
		public string SpecialHealthDietaryAccessConsiderations { get; set; }
		public string DaytimePhone { get; set; }
		public string EmergencyPhone { get; set; }
		public string SignatureData { get; set; }
	}



	[MetadataType(typeof(IGuardianPermissionSlipsView))]
	public partial class GuardianPermissionSlipsView { }

	public class IndexPermissionSlip
	{
		#region Guardian Properties
		public AspNetUser guardian { get; set; }
		public Student student { get; set; }
		public GuardianApproval guardianApproval { get; set; }
		public PermissionSlip permissionSlip { get; set; }
		public bool? GuardianApproved { get; set; }
		#endregion

		#region Teacher Properties
		public int ApprovedCount { get; set; } //For Teachers who are showing their list of Permission Slips.
		public int NotApprovedCount { get; set; } //For Teachers who are showing their list of Permission Slips.
		public int NoApprovalCount { get; set; } //For Teachers who are showing their list of Permission Slips.
		public int TotalCount { get { return ApprovedCount + NotApprovedCount + NoApprovalCount; } }
		#endregion
	}

	public class HomePageViewModel
	{
		public List<string> UserMessages { get; set; }
		public List<IndexPermissionSlip> PermissionSlips { get; set; }

		public HomePageViewModel()
		{
			UserMessages = new List<string>();
			PermissionSlips = new List<IndexPermissionSlip>();
		}
	}

	public class StudentPermissionSlipStatus
	{
		public int StudentID { get; set; }
		public string StudentFullName { get; set; }
		public List<AspNetUser> Guardians { get; set; }
		public bool? Approval { get; set; } //True = Approved, False = Not Approved, Null = No Approval

		public StudentPermissionSlipStatus()
		{
			Guardians = new List<AspNetUser>();
		}
	}

	public class PermissionSlipStatus
	{
		public List<StudentPermissionSlipStatus> studentPermissionStatuses { get; set; } //Will include Guardian Approvals and Guardians
		public PermissionSlip permissionSlip { get; set; }
		//TODO : Add Chaperone Listings

		public PermissionSlipStatus()
		{
			studentPermissionStatuses = new List<StudentPermissionSlipStatus>();
		}
		public PermissionSlipStatus(PermissionSlip p)
		{
			studentPermissionStatuses = new List<StudentPermissionSlipStatus>();
			permissionSlip = p;
		}
	}

	#endregion
}