using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using OnlinePermissionSlips.Models;
using OnlinePermissionSlips.Models.DAL;

namespace OnlinePermissionSlips.Controllers
{
	[Authorize]
	public class PermissionSlipsController : Controller
	{
		private OnlinePermissionSlipEntities db = new OnlinePermissionSlipEntities();

		// GET: PermissionSlips
		public ActionResult Index()
		{
			List<PermissionSlip> permissionSlips = null;
			List<int> SchoolIDs = null;
			string userID = User.Identity.GetUserId();
			if (User.IsInRole("Teacher"))
			{
				permissionSlips = db.PermissionSlips.Where(p => p.ClassRoom.TeacherUserID == userID).Include(p => p.Category).Include(p => p.ClassRoom).Include(p => p.Template).ToList();
			}
			else if (User.IsInRole("School Admin"))
			{
				SchoolIDs = db.AspNetUsers.Find(userID).Schools.Select(s => s.SchoolID).ToList();
				permissionSlips = db.PermissionSlips.Where(p => SchoolIDs.Any(si => si == p.ClassRoom.SchoolID)).Include(p => p.Category).Include(p => p.ClassRoom).Include(p => p.Template).ToList();
			}
			else if (User.IsInRole("System Admin"))
			{
				permissionSlips = db.PermissionSlips.Include(p => p.Category).Include(p => p.ClassRoom).Include(p => p.Template).ToList();
			}
			return View(permissionSlips);
		}

		// GET: PermissionSlips/Details/5
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			PermissionSlip permissionSlip = db.PermissionSlips.Find(id);
			if (permissionSlip == null)
			{
				return HttpNotFound();
			}
			return View(permissionSlip);
		}

		private void InitializePermissionSlipForCreate(ref CreatePermissionSlip createPermissionSlip)
		{
			List<School> SchoolList = null;
			List<ClassRoom> ClassList = null;
			int SchoolID = 0;
			int? ClassID = null;

			SchoolList = Common.GetSchools(db, User);
			if (SchoolList.Count == 1)
			{
				SchoolID = SchoolList[0].SchoolID;
				createPermissionSlip.SchoolID = SchoolID;
				ClassList = Common.GetClassRooms(db, User, SchoolID);
				if (ClassList.Count == 1) { ClassID = ClassList[0].ID; }
			}

			createPermissionSlip.StartDateTime = DateTime.Now;
			createPermissionSlip.EndDateTime = DateTime.Now;
			if (ClassID != null)
			{
				createPermissionSlip.ClassRoomID = (int)ClassID;
				createPermissionSlip.ClassRoom = db.ClassRooms.Find(ClassID);
			}
		}

		private void InitializeViewBagsForCreate()
		{
			List<SelectListItem> SchoolList = null;
			List<SelectListItem> ClassList = null;
			List<SelectListItem> PermissionSlipCategoryList = null;
			List<SelectListItem> PermissionSlipTemplateList = null;

			int? SchoolID = null;
			int? ClassID = null;

			//Common manages access to various lists based on current "User' that is authenticated
			PermissionSlipCategoryList = Common.GetPermissionSlipCategoriesForDropdown(db, User);
			SchoolList = Common.GetSchoolsForDropdown(db, User);
			if (SchoolList.Count == 1)
			{
				SchoolID = Convert.ToInt32(SchoolList[0].Value);
				ClassList = Common.GetClassRoomsForDropdown(db, User, SchoolID, ClassID);
				//if (ClassList.Count == 0)
				//{
				//	PermissionSlipTemplateList = new List<SelectListItem>(); //Must Show up Empty and update based on School & Class Selection
				//}
				//else
				//{
				PermissionSlipTemplateList = Common.GetPermissionSlipTemplatesForDropdown(db, User, SchoolID);
				//}
			}
			else
			{
				PermissionSlipTemplateList = Common.GetPermissionSlipTemplatesForDropdown(db, User);
			}

			ViewBag.SchoolID = SchoolList;
			ViewBag.ClassRoomID = ClassList;
			ViewBag.PermissionSlipCategoryID = PermissionSlipCategoryList;
			ViewBag.PermissionSlipTemplateID = PermissionSlipTemplateList;
		}

		// GET: PermissionSlips/Create
		public ActionResult Create()
		{
			CreatePermissionSlip p = new CreatePermissionSlip();
			InitializePermissionSlipForCreate(ref p);
			InitializeViewBagsForCreate();
			return View(p);
		}

		// POST: PermissionSlips/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "ClassRoomID,PermissionSlipCategoryID,PermissionSlipTemplateID,Name,Location,StartDateTime,EndDateTime,Cost,RequireChaperone,RequireChaperoneBackgroundCheck,EmailGuardians")] CreatePermissionSlip createPermissionSlip)
		{
			if (ModelState.IsValid)
			{
				try
				{
					PermissionSlip p = new PermissionSlip()
					{
						PermissionSlipCategoryID = createPermissionSlip.PermissionSlipCategoryID,
						PermissionSlipTemplateID = createPermissionSlip.PermissionSlipTemplateID,
						ClassRoomID = createPermissionSlip.ClassRoomID,
						Name = createPermissionSlip.Name,
						Location = createPermissionSlip.Location,
						StartDateTime = createPermissionSlip.StartDateTime,
						EndDateTime = createPermissionSlip.EndDateTime,
						Cost = createPermissionSlip.Cost,
						RequireChaperone = createPermissionSlip.RequireChaperone,
						RequireChaperoneBackgroundCheck = createPermissionSlip.RequireChaperoneBackgroundCheck
					};

					db.PermissionSlips.Add(p);
					db.SaveChanges();
					db.Entry(p).Reload(); // Make sure the ID is populated
					if (createPermissionSlip.EmailGuardians)
					{
						List<Student> students = db.ClassRooms.Where(c => c.ID == createPermissionSlip.ClassRoomID).SelectMany(c => c.Students).ToList();
						foreach (Student s in students)
						{
							foreach (AspNetUser g in s.Guardians)
							{
								if (g.EmailConfirmed)
								{

									string EmailMessage = "Permission Slip Created for your student's class. " +
																				Url.Action("PermissionSlipApproval", "PermissionSlips", new RouteValueDictionary(new { PermissionSlipID = p.ID, StudentID = s.ID }), protocol: Request.Url.Scheme);
									//for each guardian of class, send email
									MailGunUtility.SendSimpleMessage(new EmailMessage()
									{
										EmailSubject = "Class Room Permission Slip",
										ToAddress = g.Email,
										MessageText = EmailMessage,
										HtmlMessageText = EmailMessage
									});
								}
							}
						}
						////for each guardian of class, send email
						//MailGunUtility.SendSimpleMessage(new EmailMessage()
						//{
						//	MessageText = "Permission Slip Created for your student's class"
						//});
					}

					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
				}
			}

			InitializeViewBagsForCreate();
			return View(createPermissionSlip);
		}

		[HttpGet]
		public ActionResult PermissionSlipApproval(int PermissionSlipID, int StudentID)
		{
			PermissionSlip permissionSlip = null;
			Student student = null;
			AspNetUser Guardian = null;
			GuardianApproval guardianApproval = null;
			string UserID = "";
			bool AllowEdit = true;

			try
			{
				if (!User.IsInRole("Guardian") && !User.IsInRole("Teacher")) { throw new Exception("Unable to provide approval for permission slip"); }
				UserID = User.Identity.GetUserId();
				permissionSlip = db.PermissionSlips.Find(PermissionSlipID);
				AllowEdit = permissionSlip.StartDateTime >= DateTime.Now.Date;
				student = db.Students.Find(StudentID);
				if (User.IsInRole("Guardian"))
				{
					Guardian = db.AspNetUsers.Where(u => u.Id == UserID).FirstOrDefault();
					if ((permissionSlip != null) && (student != null) && (Guardian != null))
					{
						//Check to make sure Guardian is one of the Student's Guardians
						if (student.Guardians.Where(g => g.Id == Guardian.Id).FirstOrDefault() == null) { throw new Exception("Unauthorized User"); }

						guardianApproval = db.GuardianApprovals.Where(a => a.PermissionSlipID == PermissionSlipID && a.StudentID == StudentID).FirstOrDefault();
						if (guardianApproval == null)
						{
							guardianApproval = new GuardianApproval()
							{
								Name = permissionSlip.Name,
								Location = permissionSlip.Location,
								StartDateTime = permissionSlip.StartDateTime,
								EndDateTime = permissionSlip.EndDateTime,
								Cost = permissionSlip.Cost,
								RequireChaperone = permissionSlip.RequireChaperone,
								RequireChaperoneBackgroundCheck = permissionSlip.RequireChaperoneBackgroundCheck,

								GuardianUserID = AllowEdit ? UserID : "",
								GuardianName = AllowEdit ? Guardian.FullName : "",
								StudentID = StudentID,
								StudentFullName = student.FullName,
								PermissionSlipID = PermissionSlipID,
								PermissionSlip = permissionSlip,
								Approved = false,
								CanChaperone = false,
								DaytimePhone = Guardian.PhoneNumberConfirmed ? Guardian.PhoneNumber : "",
								EmergencyPhone = "",
								GuardianEmail = "",
								SpecialHealthDietaryAccessConsiderations = ""
							};
						}
						else
						{
							guardianApproval.StudentFullName = student.FullName;
							guardianApproval.Name = permissionSlip.Name;
							guardianApproval.Location = permissionSlip.Location;
							guardianApproval.StartDateTime = permissionSlip.StartDateTime;
							guardianApproval.EndDateTime = permissionSlip.EndDateTime;
							guardianApproval.Cost = permissionSlip.Cost;
							guardianApproval.RequireChaperone = permissionSlip.RequireChaperone;
							guardianApproval.RequireChaperoneBackgroundCheck = permissionSlip.RequireChaperoneBackgroundCheck;
							guardianApproval.ExistingSignature = guardianApproval.Signature;
							guardianApproval.Signature = null;
						}
					}
				}
				else //Teacher
				{
					AspNetUser Teacher = db.AspNetUsers.Find(UserID);
					guardianApproval = db.GuardianApprovals.Where(a => a.PermissionSlipID == PermissionSlipID && a.StudentID == StudentID).FirstOrDefault();
					if (guardianApproval == null)
					{

						guardianApproval = new GuardianApproval()
						{
							Name = permissionSlip.Name,
							Location = permissionSlip.Location,
							StartDateTime = permissionSlip.StartDateTime,
							EndDateTime = permissionSlip.EndDateTime,
							Cost = permissionSlip.Cost,
							RequireChaperone = permissionSlip.RequireChaperone,
							RequireChaperoneBackgroundCheck = permissionSlip.RequireChaperoneBackgroundCheck,
							GuardianUserID = AllowEdit ? UserID : "",
							GuardianName = AllowEdit ? "Verbal Approval - " + Teacher.FullName : "",
							StudentID = StudentID,
							StudentFullName = student.FullName,
							PermissionSlipID = PermissionSlipID,
							PermissionSlip = permissionSlip,
							Approved = false,
							CanChaperone = false,
							DaytimePhone = "",
							EmergencyPhone = "",
							GuardianEmail = "",
							SpecialHealthDietaryAccessConsiderations = ""
						};
					}
					else
					{
						guardianApproval.StudentFullName = student.FullName;
						guardianApproval.Name = permissionSlip.Name;
						guardianApproval.Location = permissionSlip.Location;
						guardianApproval.StartDateTime = permissionSlip.StartDateTime;
						guardianApproval.EndDateTime = permissionSlip.EndDateTime;
						guardianApproval.Cost = permissionSlip.Cost;
						guardianApproval.RequireChaperone = permissionSlip.RequireChaperone;
						guardianApproval.RequireChaperoneBackgroundCheck = permissionSlip.RequireChaperoneBackgroundCheck;
						guardianApproval.ExistingSignature = guardianApproval.Signature;
						guardianApproval.Signature = null;
					}
				}
			}
			catch (Exception ex)
			{
				//TODO : Do something with Exception Action
				ModelState.AddModelError("", "Exception occurred processing approval request :: " + ex.ToString());
				return RedirectToAction("Index", "Home");
			}

			return View(guardianApproval);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult PermissionSlipApproval(GuardianApproval guardianApproval)
		{
			GuardianApproval existingGuardianApproval = null;
			try
			{
				if (ModelState.IsValid)
				{
					AspNetUser user = db.AspNetUsers.Find(guardianApproval.GuardianUserID);

					if (!User.IsInRole("Teacher") && (User.IsInRole("Guardian") && guardianApproval.GuardianEmail != user.Email)) { throw new Exception("Unable to verify guardian for approval"); }
					existingGuardianApproval = db.GuardianApprovals.Where(a => a.PermissionSlipID == guardianApproval.PermissionSlipID && a.StudentID == guardianApproval.StudentID).FirstOrDefault();
					if (existingGuardianApproval == null)
					{
						guardianApproval.CreatedDateTime = DateTime.Now;
						guardianApproval.Signature = Convert.FromBase64String(guardianApproval.SignatureData.Split(',')[1]);
						db.GuardianApprovals.Add(guardianApproval);
					}
					else
					{
						db.Entry(existingGuardianApproval).State = EntityState.Detached;
						guardianApproval.Signature = Convert.FromBase64String(guardianApproval.SignatureData.Split(',')[1]);
						db.Entry(guardianApproval).State = EntityState.Modified;
					}
					db.SaveChanges();

					return RedirectToAction("Index", "Home");
				}
			}
			catch (Exception ex) { ModelState.AddModelError("", ex.Message); }

			if (guardianApproval.ExistingSignatureData != null)
			{
				guardianApproval.ExistingSignature = Convert.FromBase64String(guardianApproval.ExistingSignatureData.Split(',')[1]);
			}
			return View(guardianApproval);
		}
		
		[HttpGet]
		public ActionResult PermissionSlipStatus(int PermissionSlipID)
		{
			//TODO
			/*
			 * Validate Teacher or School Admin 
			 * Return a view that shows a list of all students for that permission slip, their approval status
			 * Include buttons to:
			 *		Send emails to guardians of students with "No Approval" status to give approval
			 *		Send specific email to student guardians to give approval
			 *		Send Emails to guardians of all students as a reminder of the event
			 */

			PermissionSlipStatus permissionSlipStatus = new PermissionSlipStatus();
			PermissionSlip permissionSlip = null;
			GuardianApproval guardianApproval = null;
			StudentPermissionSlipStatus studentPermissionSlip = null;
			List<StudentPermissionSlipStatus> studentPermissionSlipStatuses = new List<StudentPermissionSlipStatus>();
			string UserID = "";

			try
			{
				if (User.IsInRole("Guardian")) { throw new Exception("Unable to provide permission slip status"); }
				UserID = User.Identity.GetUserId();
				permissionSlip = db.PermissionSlips.Find(PermissionSlipID);
				if ((permissionSlip.ClassRoom.TeacherUserID != UserID) && User.IsInRole("Teacher")) { throw new Exception("Unable to provide permission slip status"); }
				if (permissionSlip != null)
				{

					List<Student> students = permissionSlip.ClassRoom.Students.ToList();
					foreach (Student s in students)
					{
						studentPermissionSlip = new StudentPermissionSlipStatus()
						{
							StudentID = s.ID,
							StudentFullName = s.FullName,
							Guardians = s.Guardians.ToList()
						};

						guardianApproval = db.GuardianApprovals.Where(g => g.StudentID == s.ID && g.PermissionSlipID == permissionSlip.ID).FirstOrDefault();
						if (guardianApproval != null)
						{
							studentPermissionSlip.Approval = guardianApproval.Approved;
						}
						studentPermissionSlipStatuses.Add(studentPermissionSlip);
					}
				}

				permissionSlipStatus.permissionSlip = permissionSlip;
				permissionSlipStatus.studentPermissionStatuses = studentPermissionSlipStatuses;
			}
			catch (Exception ex)
			{
				//TODO : Do something with Exception Action
				ModelState.AddModelError("", "Exception occurred processing request for permission slip status:: " + ex.ToString());
				return RedirectToAction("Index", "Home");
			}

			return View(permissionSlipStatus);
		}

		// GET: PermissionSlips/Edit/5
		public ActionResult Edit(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			PermissionSlip permissionSlip = db.PermissionSlips.Find(id);
			if (permissionSlip == null)
			{
				return HttpNotFound();
			}
			ViewBag.PermissionSlipCategoryID = new SelectList(db.Categories, "ID", "Name", permissionSlip.PermissionSlipCategoryID);
			ViewBag.ClassRoomID = new SelectList(db.ClassRooms, "ID", "RoomNumber", permissionSlip.ClassRoomID);
			ViewBag.PermissionSlipTemplateID = new SelectList(db.Templates, "ID", "Name", permissionSlip.PermissionSlipTemplateID);
			return View(permissionSlip);
		}

		// POST: PermissionSlips/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "ID,PermissionSlipCategoryID,PermissionSlipTemplateID,ClassRoomID,Name,Location,StartDateTime,EndDateTime,Cost,RequireChaperone,RequireChaperoneBackgroundCheck")] PermissionSlip permissionSlip)
		{
			if (ModelState.IsValid)
			{
				db.Entry(permissionSlip).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			ViewBag.PermissionSlipCategory = new SelectList(db.Categories, "ID", "Name", permissionSlip.PermissionSlipCategoryID);
			ViewBag.ClassRoomID = new SelectList(db.ClassRooms, "ID", "RoomNumber", permissionSlip.ClassRoomID);
			ViewBag.PermissionSlipTemplateID = new SelectList(db.Templates, "ID", "Name", permissionSlip.PermissionSlipTemplateID);
			return View(permissionSlip);
		}

		// GET: PermissionSlips/Delete/5
		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			PermissionSlip permissionSlip = db.PermissionSlips.Find(id);
			if (permissionSlip == null)
			{
				return HttpNotFound();
			}
			return View(permissionSlip);
		}

		// POST: PermissionSlips/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			PermissionSlip permissionSlip = db.PermissionSlips.Find(id);
			db.PermissionSlips.Remove(permissionSlip);
			db.SaveChanges();
			return RedirectToAction("Index");
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				db.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
