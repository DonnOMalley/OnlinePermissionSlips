using OnlinePermissionSlips.Models.DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlinePermissionSlips.Models.ViewModels
{

	public interface IReportingSearchResultViewModel
	{
		[Display(Name = "School ID")]
		int SchoolID { get; set; }
		[Display(Name = "School Name")]
		string SchoolName { get; set; }

		[Display(Name = "Class ID")]
		int ClassID { get; set; }
		[Display(Name = "Class Name")]
		string ClassName { get; set; }

		[Display(Name = "Teacher")]
		int TeacherID { get; set; }
		[Display(Name = "Teacher Name")]
		string TeacherName { get; set; }

		[Display(Name = "Student")]
		int StudentID { get; set; }
		[Display(Name = "Student Name")]
		string StudentName { get; set; }

		[Display(Name = "Event Name")]
		string PermissionSlipName { get; set; }

		[Display(Name = "Start Date")]
		DateTime EventStartDate { get; set; }
		[Display(Name = "End Date")]
		DateTime EventEndDate { get; set; }
	}

	public interface IReportingSearchViewModel
	{
		[Display(Name = "School")]
		int SchoolID { get; set; }
		[Display(Name = "Class")]
		int ClassRoomID { get; set; }
		[Display(Name = "Teacher")]
		int TeacherID { get; set; }
		[Display(Name = "Student")]
		int StudentID { get; set; }
		[Display(Name = "Event")]
		string PermissionSlipName { get; set; }
		[Display(Name = "Start Date")]
		DateTime EventStartDate { get; set; }
		[Display(Name = "End Date")]
		DateTime EventEndDate { get; set; }
		[Display(Name = "Approval Status")]
		int ApprovalStatusID { get; set; }
	}

	[MetadataType(typeof(IReportingSearchResultViewModel))]
	public class ReportingSearchResultViewModel
	{
		public string SchoolName { get; set; }
		public string ClassName { get; set; }
		public int StudentID { get; set; }
		public string StudentName { get; set; }
		public int PermissionSlipID { get; set; }
		public string EventName { get; set; }
		public DateTime EventStartDate { get; set; }
		public DateTime EventEndDate { get; set; }
		public bool? Approved { get; set; }
		public string ApprovedBy { get; set; }
	}

	[MetadataType(typeof(IReportingSearchViewModel))]
	public class ReportingSearchViewModel
	{
		public bool ShowResults { get; set; } = false;
		public int SchoolID { get; set; }
		public int ClassRoomID { get; set; }
		public int TeacherID { get; set; }
		public int StudentID { get; set; }
		public string PermissionSlipName { get; set; }
		public DateTime EventStartDate { get; set; }
		public DateTime EventEndDate { get; set; }
		public int ApprovalStatusID { get; set; }

		public List<ReportingSearchResultViewModel> SearchResults;

		public ReportingSearchViewModel()
		{
			SearchResults = new List<ReportingSearchResultViewModel>();
		}

	}
}