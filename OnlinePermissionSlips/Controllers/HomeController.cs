using Microsoft.AspNet.Identity;
using OnlinePermissionSlips.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlinePermissionSlips.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			string UserID = "";
			HomePageViewModel vm = new HomePageViewModel();
			OnlinePermissionSlipEntities db = new OnlinePermissionSlipEntities();
			List<GuardianApproval> guardianApprovals = null;
			List<ClassRoom> classRooms = null;
			List<PermissionSlip> ClassPermissionSlips = null;
			int ApprovedCount = 0;
			int NotApprovedCount = 0;
			int NoApprovalCount = 0;
			Dictionary<int, List<int>> PermissionSlipStudents = new Dictionary<int, List<int>>();
			AspNetUser guardian = null;

			if (User.Identity.IsAuthenticated)
			{
				UserID = User.Identity.GetUserId();
				if (User.IsInRole("Guardian"))
				{
					guardian = db.AspNetUsers.Where(u => u.Id == UserID).FirstOrDefault();
					guardianApprovals = db.GuardianApprovals.Where(a => a.GuardianUserID == UserID).ToList();
					foreach (GuardianApproval g in guardianApprovals)
					{
						vm.PermissionSlips.Add(new IndexPermissionSlip()
						{
							guardian = g.AspNetUser, //Should guardian be shown if not the current user even though they are connected to the same student?
							guardianApproval = g,
							student = g.Student,
							permissionSlip = g.PermissionSlip,
							GuardianApproved = g.Approved
						});

						if(PermissionSlipStudents.ContainsKey(g.PermissionSlipID))
						{
							PermissionSlipStudents[g.PermissionSlipID].Add(g.Student.ID);
						}
						else
						{
							PermissionSlipStudents.Add(g.PermissionSlipID, new List<int>() { g.Student.ID });
						}
					}

					//Get Each Guardian's Student, Then ClassRoom, then permission Slip
					foreach(Student s in db.Students.Where(s => s.Guardians.Any(g => g.Id == UserID)).ToList())
					{
						foreach(PermissionSlip p in s.ClassRoom.PermissionSlips)
						{
							if(!PermissionSlipStudents.ContainsKey(p.ID) || !PermissionSlipStudents[p.ID].Contains(s.ID))
							{
								vm.PermissionSlips.Add(new IndexPermissionSlip()
								{
									guardian = guardian,
									guardianApproval = null,
									student = s,
									permissionSlip = p,
									GuardianApproved = null
								});

								if (PermissionSlipStudents.ContainsKey(p.ID))
								{
									PermissionSlipStudents[p.ID].Add(s.ID);
								}
								else
								{
									PermissionSlipStudents.Add(p.ID, new List<int>() { s.ID });
								}
							}
						}
					}

				}
				else if (User.IsInRole("Teacher"))
				{
					classRooms = db.ClassRooms.Where(c => c.TeacherUserID == UserID).ToList();
					DateTime StartDate = DateTime.Now.Date;
					DateTime EndDate = DateTime.Now.Date;

					if (DateTime.Now.Month >= 8)
					{
						StartDate = new DateTime(DateTime.Now.Year, 8, 1);
						EndDate = new DateTime(DateTime.Now.Year + 1, 6, 30);
					}
					else
					{
						StartDate = new DateTime(DateTime.Now.Year - 1, 8, 1);
						EndDate = new DateTime(DateTime.Now.Year, 6, 30);
					}

					foreach (ClassRoom c in classRooms)
					{

						ClassPermissionSlips = c.PermissionSlips.Where(p => (p.StartDateTime >= StartDate && p.StartDateTime <= EndDate) || 
																																(p.EndDateTime >= EndDate && p.EndDateTime >= StartDate)
																													).ToList();

						foreach (PermissionSlip p in ClassPermissionSlips)
						{
							ApprovedCount = db.GuardianApprovals.Where(a => a.PermissionSlipID == p.ID && a.Approved == true).Count();
							NotApprovedCount = db.GuardianApprovals.Where(a => a.PermissionSlipID == p.ID && a.Approved == false).Count();
							NoApprovalCount = p.ClassRoom.Students.Count - ApprovedCount - NotApprovedCount;

							vm.PermissionSlips.Add(new IndexPermissionSlip()
							{
								permissionSlip = p,
								ApprovedCount = ApprovedCount,
								NotApprovedCount = NotApprovedCount,
								NoApprovalCount = NoApprovalCount
							});
						}
					}
				}
			}

			vm.PermissionSlips = vm.PermissionSlips.OrderByDescending(p => p.permissionSlip.StartDateTime.Date).ThenBy(p => p.GuardianApproved).ToList();
			return View(vm);
		}

		public ActionResult About()
		{
			ViewBag.Message = "Online Permission Slips by Trash Panda Solutions";

			return View();
		}

		public ActionResult Contact()
		{
			ViewBag.Message = "Contact Trash Panda Solutions";

			return View();
		}
	}
}