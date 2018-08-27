using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlinePermissionSlips.Models.DAL;
using OnlinePermissionSlips.Models.ViewModels;

namespace OnlinePermissionSlips.Controllers
{
	public class ReportsController : Controller
	{
		OnlinePermissionSlipEntities db = new OnlinePermissionSlipEntities();

		// GET: Reports
		public ActionResult Index()
		{
			throw new NotImplementedException();
		}

		private void InitializeViewBagsForSearchCriteria(DateTime StartDate, DateTime EndDate)
		{
			//Define Variables for providing values to drop down to filter search results
			List<SelectListItem> Schools = null;
			int? SchoolID = null;
			List<SelectListItem> Classes = null;
			int? ClassID = null;
			List<SelectListItem> Students = null;
			List<SelectListItem> PermissionSlips = null;

			try
			{
				//SearchVM.ClassRoomID
				Schools = Common.GetSchoolsForDropdown(db, User);
				ViewBag.SchoolID = Schools;

				//SearchVM.ClassRoomID
				if ((Schools != null) && (Schools.Count == 1)) { SchoolID = int.Parse(Schools[0].Value); }
				Classes = Common.GetClassRoomsForDropdown(db, User, SchoolID);
				ViewBag.ClassRoomID = Classes;

				//SearchVM.StudentID
				if ((Classes != null) && (Classes.Count == 1)) { ClassID = int.Parse(Classes[0].Value); }
				Students = Common.GetStudentsForDropdown(db, User, null, ClassID);
				ViewBag.StudentID = Students;

				//SearchVM.PermissionSlipID
				PermissionSlips = Common.GetPermissionSlipNamesForDropdown(db, User, StartDate, EndDate);
				ViewBag.PermissionSlipName = PermissionSlips;

				List<SelectListItem> ApprovalTypes = new List<SelectListItem>();
				foreach (int approvalType in Enum.GetValues(typeof(Common.ApprovalStatusTypes)))
				{
					ApprovalTypes.Add(new SelectListItem() { Text = Enum.GetName(typeof(Common.ApprovalStatusTypes), approvalType).Replace("_", " "), Value = approvalType.ToString() });
				}
				ViewBag.ApprovalStatusID = ApprovalTypes;

			}
			catch (Exception ex)
			{
				throw;
			}
		}

		[HttpGet]
		public ActionResult Search()
		{
			string ViewName = "TeacherSearch";
			DateTime StartDT = new DateTime(0);
			DateTime EndDT = new DateTime(0);
			try
			{
				if (User.IsInRole("Guardian"))
				{
					ViewName = "GuardianSearch";
				}

				if(DateTime.Now.Month >= 8)
				{
					StartDT = new DateTime(DateTime.Now.Year, 8, 1);
					EndDT = new DateTime(DateTime.Now.Year + 1, 6, 30);
				}
				else
				{
					StartDT = new DateTime(DateTime.Now.Year - 1, 8, 1);
					EndDT = new DateTime(DateTime.Now.Year, 6, 30);
				}
				InitializeViewBagsForSearchCriteria(StartDT, EndDT);
			}
			catch (Exception ex)
			{
				throw;
			}
			return View(ViewName, new ReportingSearchViewModel() { EventStartDate = StartDT, EventEndDate = EndDT });
		}
	}
}