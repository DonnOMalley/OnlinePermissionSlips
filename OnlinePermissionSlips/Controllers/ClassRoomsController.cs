using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using OnlinePermissionSlips.Models;
using OnlinePermissionSlips.Models.DAL;
using OnlinePermissionSlips.Models.ViewModels;

namespace OnlinePermissionSlips.Controllers
{
	[Authorize(Roles = "School Admin, System Admin, Teacher")]
	public class ClassRoomsController : Controller
	{
		private OnlinePermissionSlipEntities db = new OnlinePermissionSlipEntities();
		private ApplicationUserManager _userManager;
		private ApplicationRoleManager _roleManager;

		public ApplicationUserManager UserManager
		{
			get
			{
				return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
			}
			private set
			{
				_userManager = value;
			}
		}

		public ApplicationRoleManager RoleManager
		{
			get
			{
				return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
			}
			private set
			{
				_roleManager = value;
			}
		}

		// GET: ClassRooms
		public ActionResult Index()
		{
			List<int> schoolIDs = Common.GetSchools(db, User).Select(s => s.SchoolID).ToList();

			var classRooms = db.ClassRooms.Where(c => schoolIDs.Any(s => s == c.SchoolID)).Include(c => c.School).Include(c => c.AspNetUser);
			return View(classRooms.ToList());
		}

		// GET: ClassRooms/Details/5
		public ActionResult Details(int? id, ClassRoomMessageId? message)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			ViewBag.StatusMessage =
					message == ClassRoomMessageId.EmailsSent ? "Guardian emails have been sent."
					: message == ClassRoomMessageId.GuardianEmailSent ? "Guardian emailhas been sent."
					: message == ClassRoomMessageId.Error ? "An error has occurred."
					: "";
			ClassRoom classRoom = db.ClassRooms.Find(id);
			if (classRoom == null)
			{
				return HttpNotFound();
			}
			return View(classRoom);
		}

		// GET: ClassRooms/Create
		public ActionResult Create()
		{
			//Get All Teachers for a specific school (Can I update teacher list as the school choice changes for system Admin?)

			ApplicationUser user = UserManager.FindByIdAsync(User.Identity.GetUserId()).Result;
			List<SelectListItem> SchoolList = Common.GetSchoolsForDropdown(db, User);

			ViewBag.SchoolID = SchoolList;
			if (User.IsInRole("Teacher"))
			{
				ViewBag.TeacherUserID = Common.GetTeachersForDropdown(db, User, User.Identity.GetUserId());
			}
			else if (SchoolList.Count == 1) //Will get Picked based on School
			{
				ViewBag.TeacherUserID = Common.GetTeachersForDropdown(db, User, "", int.Parse(SchoolList[0].Value));
			}
			else
			{
				ViewBag.TeacherUserID = new SelectList(new List<AspNetUser>(), "Id", "FullName");
			}
			return View();
		}

		// POST: ClassRooms/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "ID,RoomNumber,TeacherUserID,SchoolID")] ClassRoom classRoom)
		{
			if (ModelState.IsValid)
			{
				classRoom.SetCreated();
				db.ClassRooms.Add(classRoom);
				db.SaveChanges();

				return RedirectToAction("Edit", new { id = classRoom.ID });
			}

			//Get All Teachers for a specific school (Can I update teacher list as the school choice changes for system Admin?)

			ApplicationUser user = UserManager.FindByIdAsync(User.Identity.GetUserId()).Result;
			List<SelectListItem> SchoolList = Common.GetSchoolsForDropdown(db, User);

			ViewBag.SchoolID = SchoolList;
			if (User.IsInRole("Teacher"))
			{
				ViewBag.TeacherUserID = Common.GetTeachersForDropdown(db, User, User.Identity.GetUserId());
			}
			else if (SchoolList.Count == 1) //Will get Picked based on School
			{
				ViewBag.TeacherUserID = Common.GetTeachersForDropdown(db, User, "", int.Parse(SchoolList[0].Value));
			}
			else
			{
				ViewBag.TeacherUserID = new SelectList(new List<AspNetUser>(), "Id", "FullName");
			}
			return View(classRoom);
		}

		// GET: ClassRooms/Edit/5
		public ActionResult Edit(int? id, ClassRoomMessageId? message)
		{

			ViewBag.StatusMessage =
					message == ClassRoomMessageId.EmailsSent ? "Guardian emails have been sent."
					: message == ClassRoomMessageId.GuardianEmailSent ? "Guardian emailhas been sent."
					: message == ClassRoomMessageId.Error ? "An error has occurred."
					: "";

			ClassRoom classRoom = null;
			List<SelectListItem> SchoolList = null;

			if (id == null) { return new HttpStatusCodeResult(HttpStatusCode.BadRequest); }

			classRoom = db.ClassRooms.Find(id);

			if (classRoom == null) { return HttpNotFound(); }

			//Get All Teachers for a specific school (Can I update teacher list as the school choice changes for system Admin?)

			SchoolList = Common.GetSchoolsForDropdown(db, User, classRoom.SchoolID);

			ViewBag.SchoolID = SchoolList;
			if (User.IsInRole("Teacher"))
			{
				ViewBag.TeacherUserID = Common.GetTeachersForDropdown(db, User, User.Identity.GetUserId());
			}
			else if(SchoolList.Count >= 1)
			{
				ViewBag.TeacherUserID = Common.GetTeachersForDropdown(db, User, classRoom.TeacherUserID, classRoom.SchoolID);
			}
			else
			{
				ViewBag.TeacherUserID = new SelectList(new List<AspNetUser>(), "Id", "FullName");
			}

			return View(classRoom);
		}

		// POST: ClassRooms/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "ID,RoomNumber,TeacherUserID,SchoolID,Students")]ClassRoom classRoom)
		{
			List<Student> currentStudents = null;
			List<Student> remainingStudents = null;
			List<SelectListItem> SchoolList = null;

			if (ModelState.IsValid)
			{
				try
				{
					classRoom.SetModified();
					db.Entry(classRoom).State = EntityState.Modified;

					//Delete Students that are no longer there
					remainingStudents = classRoom.Students.ToList();
					currentStudents = db.Students.Where(s => s.ClassRoomID == classRoom.ID).ToList();
					foreach (Student s in currentStudents.Where(o => !remainingStudents.Any(s => s.ID == o.ID)))
					{
						classRoom.Students.Remove(s);
						db.Students.Remove(s);
					}
					foreach (Student s in remainingStudents)
					{
						db.Entry(s).State = EntityState.Modified;
					}

					db.SaveChanges();
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					string Message = "Error Saving Changes";
					string InnerMessage = "";
					Exception inner = ex.InnerException;
					while (inner != null)
					{
						InnerMessage = " :: " + inner.Message;
						inner = inner.InnerException;
					}
					Message += InnerMessage;

					ModelState.AddModelError("", Message);
				}
			}

			SchoolList = Common.GetSchoolsForDropdown(db, User);

			ViewBag.SchoolID = SchoolList;
			if (User.IsInRole("Teacher"))
			{
				ViewBag.TeacherUserID = Common.GetTeachersForDropdown(db, User, User.Identity.GetUserId());
			}
			else if (SchoolList.Count == 1) //Will get Picked based on School
			{
				ViewBag.TeacherUserID = Common.GetTeachersForDropdown(db, User, "", int.Parse(SchoolList[0].Value));
			}
			else
			{
				ViewBag.TeacherUserID = new SelectList(new List<AspNetUser>(), "Id", "FullName");
			}
			return View(classRoom);
		}

		[HttpGet]
		public ActionResult SendRegistrationEmails(int? id)
		{
			if (id == null) { return new HttpStatusCodeResult(HttpStatusCode.BadRequest); }
			ClassRoom classRoom = db.ClassRooms.Find(id);
			School school = db.Schools.Where(s => s.SchoolID == classRoom.SchoolID).FirstOrDefault();
			if (school == null) { return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Data Integrity Error. Classroom not assigned to existing school"); }

			try
			{
				List<Student> classStudents = db.Students.Where(s => s.ClassRoomID == id).ToList();
				foreach (Student s in classStudents)
				{
					EmailMessage email = new EmailMessage();
					email.EmailSubject = school.SchoolName + " Online Permission Slip Registration";
					email.HtmlMessageText = "<a href=\"" + Url.Action("AddStudent", "Manage", new { id = s.ID }, protocol: Request.Url.Scheme) + "\">Click Here to add your student to your profile</a>" +
																	"<br>" +
																	"If you haven't registered, you can <a href=\"" + Url.Action("Register", "Account", null, protocol: Request.Url.Scheme) + "\">Click Here to Register</a> first" +
																	"<p>" +
																		"Online Permission Slips allows better communication and transparency regarding events for your students' class, " + classRoom.GetClassName() +
																		"<br />" +
																		"<br />" +
																		"<U>You will need to provide the following for verification:</U><br />" +
																		"<ul>" +
																		"<li><b>You students ID Number: " + s.StudentNumber.ToString() + "</b></li>" +
																		"<li><b>Your Student's Full Name: " + s.FullName + "</b></li>" +
																		"</ul>" +
																	"</p>" +
																	"<p>" +
																		"Online Permission Slips will always protect your student's information, as well as yours, ensuring privacy and limited access to the information." +
																		"<br>" +
																		"Only the school and you will be able to access your student's permission slips and related information." +
																	"</p>";
					email.MessageText = "Click the following link to add your student to your profile" + Environment.NewLine +
															"Click to Add: " + Url.Action("AddStudent", "Manage", new { id = s.ID }, protocol: Request.Url.Scheme) + Environment.NewLine + Environment.NewLine +
															"If you haven't registered, you can Click Here to Register first:" + Url.Action("Register", "Account", null, protocol: Request.Url.Scheme) + Environment.NewLine + Environment.NewLine +
															"Online Permission Slips allows better communication and transparency regarding events for your students' class, " + classRoom.GetClassName() + Environment.NewLine +
															Environment.NewLine +
															"You will need to provide the following for verification:" + Environment.NewLine +
															"You students ID Number: " + s.StudentNumber.ToString() + Environment.NewLine +
															"Your Student's Full Name: " + s.FullName + Environment.NewLine +
															Environment.NewLine +
															"Online Permission Slips will always protect your student's information, as well as yours, ensuring privacy and limited access to the information." + Environment.NewLine +
															"Only the school and you will be able to access your student's permission slips and related information.";

					if (s.Guardian1TempEmail != null)
					{
						email.ToAddress = s.Guardian1TempEmail;
						MailGunUtility.SendSimpleMessage(email);
					}
					if (s.Guardian2TempEmail != null)
					{
						email.ToAddress = s.Guardian2TempEmail;
						MailGunUtility.SendSimpleMessage(email);
					}
				}
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Exception Emailing Guardians :: " + ex.ToString());
			}
			//TODO :: Add Indication that messages were sent??
			return RedirectToAction("Edit", "ClassRooms", new { id, message = ClassRoomMessageId.EmailsSent });
		}

		[HttpGet]
		public ActionResult SendGuardianRegistrationEmail(int id, int StudentId, string EmailAddress)
		{
			Student student = null;
			ClassRoom classRoom = null;
			School school = null;

			try
			{
				classRoom = db.ClassRooms.Find(id);
				student = db.Students.Find(StudentId);
				school = db.Schools.Where(s => s.SchoolID == classRoom.SchoolID).FirstOrDefault();
				if (school == null) { return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Data Integrity Error. Classroom not assigned to existing school"); }

				EmailMessage email = new EmailMessage()
				{
					ToAddress = EmailAddress,
					EmailSubject = school.SchoolName + " Online Permission Slip Registration"
				};
				email.HtmlMessageText = "<a href=\"" + Url.Action("AddStudent", "Manage", new { id = student.ID }, protocol: Request.Url.Scheme) + "\">Click Here to add your student to your profile</a>" +
																"<br>" +
																"If you haven't registered, you can <a href=\"" + Url.Action("Register", "Account", null, protocol: Request.Url.Scheme) + "\">Click Here to Register</a> first" +
																"<p>" +
																	"Online Permission Slips allows better communication and transparency regarding events for your students' class, " + classRoom.GetClassName() +
																	"<br />" +
																	"<br />" +
																	"<U>You will need to provide the following for verification:</U><br />" +
																	"<ul>" +
																	"<li><b>You students ID Number: " + student.StudentNumber.ToString() + "</b></li>" +
																	"<li><b>Your Student's Full Name: " + student.FullName + "</b></li>" +
																	"</ul>" +
																"</p>" +
																"<p>" +
																	"Online Permission Slips will always protect your student's information, as well as yours, ensuring privacy and limited access to the information." +
																	"<br>" +
																	"Only the school and you will be able to access your student's permission slips and related information." +
																"</p>";
				email.MessageText = "Click the following link to add your student to your profile" + Environment.NewLine +
														"Click to Add: " + Url.Action("AddStudent", "Manage", new { id = student.ID }, protocol: Request.Url.Scheme) + Environment.NewLine + Environment.NewLine +
														"If you haven't registered, you can Click Here to Register first:" + Url.Action("Register", "Account", null, protocol: Request.Url.Scheme) + Environment.NewLine + Environment.NewLine +
														"Online Permission Slips allows better communication and transparency regarding events for your students' class, " + classRoom.GetClassName() + Environment.NewLine +
														Environment.NewLine +
														"You will need to provide the following for verification:" + Environment.NewLine +
														"You students ID Number: " + student.StudentNumber.ToString() + Environment.NewLine +
														"Your Student's Full Name: " + student.FullName + Environment.NewLine +
														Environment.NewLine +
														"Online Permission Slips will always protect your student's information, as well as yours, ensuring privacy and limited access to the information." + Environment.NewLine +
														"Only the school and you will be able to access your student's permission slips and related information.";

				MailGunUtility.SendSimpleMessage(email);
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Exception Emailing Guardians :: " + ex.ToString());
			}
			//TODO :: Add Indication that messages were sent??
			return RedirectToAction("Details", "ClassRooms", new { id, message = ClassRoomMessageId.EmailsSent });
		}

		[HttpGet]
		public ActionResult ImportStudents(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			ApplicationUser user = UserManager.FindByIdAsync(User.Identity.GetUserId()).Result;

			if (UserManager.IsInRole(user.Id, "Guardian"))
			{
				return RedirectToAction("Index", "Home");
			}

			ClassRoom classRoom = db.ClassRooms.Find(id);

			if (classRoom == null)
			{
				return HttpNotFound();
			}
			if (!UserManager.IsInRole(user.Id, "System Admin"))
			{
				//If User is not part of classes school - redirect home
				//if(user.SchoolID != classRoom.SchoolID)
				//{
				//	return RedirectToAction("Index", "Home");
				//}
			}

			ClassRoomImport classRoomImport = new ClassRoomImport((int)id, classRoom.SchoolID, classRoom.School.SchoolName, classRoom.AspNetUser.FullName, classRoom.RoomNumber);
			return View(classRoomImport);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult ImportStudents([Bind(Include = "ID,SchoolID,SchoolName,TeacherName,RoomNumber,Students")]ClassRoomImport classRoom)
		{
			ActionResult result = View(classRoom);
			DbContextTransaction xactn = null;

			try
			{

				if (ModelState.IsValid)
				{
					xactn = db.Database.BeginTransaction();

					foreach (StudentImport s in classRoom.Students)
					{
						db.Students.Add(new Student()
						{
							SchoolID = classRoom.SchoolID,
							ClassRoomID = classRoom.ID,
							FullName = s.FullName,
							StudentNumber = s.StudentNumber,
							Guardian1TempEmail = s.Guardian1TempEmail,
							Guardian2TempEmail = s.Guardian2TempEmail
						});
					}

					ClassRoom c = db.ClassRooms.Find(classRoom.ID);
					c.SetModified();
					if (db.Entry(c).State != EntityState.Modified)
					{
						db.Entry(c).State = EntityState.Modified;
					}

					db.SaveChanges();

					//////////////////////////////////////////////////////
					///	Send Emails to each of the registered guardians
					//////////////////////////////////////////////////////
					foreach (StudentImport s in classRoom.Students)
					{
						try
						{
							if (s.Guardian1TempEmail != null)
							{
								SendGuardianRegistrationEmail(classRoom.ID, s.ID, s.Guardian1TempEmail);
							}
							if (s.Guardian2TempEmail != null)
							{
								SendGuardianRegistrationEmail(classRoom.ID, s.ID, s.Guardian2TempEmail);
							}
						}
						catch(Exception innerEx) { throw new Exception("Unable to send Guardian Emails", innerEx); } 
					}

					xactn.Commit();
					xactn.Dispose();
					xactn = null;

					result = RedirectToAction("Edit", "ClassRooms", new { id = classRoom.ID });
				}

			}
			catch (Exception ex)
			{
				string Message = "Error Saving Changes";
				string InnerMessage = "";
				Exception inner = ex.InnerException;
				while (inner != null)
				{
					InnerMessage = " :: " + inner.Message;
					inner = inner.InnerException;
				}
				Message += InnerMessage;

				ModelState.AddModelError("", Message);
				if (xactn != null)
				{
					xactn.Rollback();
					xactn.Dispose();
					xactn = null;
				}
			}
			return result;
		}

		// GET: ClassRooms/Delete/5
		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ClassRoom classRoom = db.ClassRooms.Find(id);
			if (classRoom == null)
			{
				return HttpNotFound();
			}
			return View(classRoom);
		}

		// POST: ClassRooms/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			ClassRoom classRoom = db.ClassRooms.Find(id);
			db.ClassRooms.Remove(classRoom);
			db.SaveChanges();
			return RedirectToAction("Index");
		}

		public ActionResult AddStudentImport()
		{
			StudentImport student = new StudentImport();
			return PartialView("~/Views/Shared/EditorTemplates/StudentImport.cshtml", student);
		}

		//private SelectList GetTeacherSelectList(ApplicationUser user, string TeacherID = null)
		//{
		//	SelectList TeacherList = null;
		//	string TeacherRoleID;
		//	List<ApplicationUser> Teachers = null;

		//	if (UserManager.IsInRole(user.Id, "Teacher"))
		//	{
		//		TeacherList = new SelectList(UserManager.Users.Where(u => u.UserName == User.Identity.Name).ToList(), "Id", "FullName", user.Id);
		//	}
		//	else
		//	{
		//		TeacherRoleID = RoleManager.FindByName("Teacher").Id;
		//		Teachers = UserManager.Users.Where(u => u.Roles.Any(ru => ru.RoleId == TeacherRoleID)).ToList();
		//		if (UserManager.IsInRole(user.Id, "School Admin")) //Limit to school specifically
		//		{
		//			//Teachers = Teachers.Where(t => t.SchoolID == user.SchoolID).ToList();
		//		}
		//		if (TeacherID == null)
		//		{
		//			TeacherList = new SelectList(Teachers, "Id", "FullName");
		//		}
		//		else
		//		{
		//			TeacherList = new SelectList(Teachers, "Id", "FullName", TeacherID);
		//		}
		//	}

		//	return TeacherList;
		//}

		[HttpPost]
		public ActionResult GetSchoolTeachers(int? SchoolID)
		{
			List<SelectListItem> Teachers = null;
			try
			{
				if (SchoolID != null) { Teachers = Common.GetTeachersForDropdown(db, User, "", SchoolID); }
			}
			catch (Exception) { throw; }

			return Json(Teachers);
		}

		[HttpPost]
		public ActionResult GetSchoolClasses(int? SchoolID)
		{
			List<SelectListItem> classRooms;
			try
			{
				classRooms = Common.GetClassRoomsForDropdown(db, User, SchoolID);
			}
			catch (Exception)
			{
				throw;
			}

			return Json(classRooms);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				db.Dispose();
			}
			base.Dispose(disposing);
		}

		public enum ClassRoomMessageId
		{
			EmailsSent,
			GuardianEmailSent,
			Error
		}
	}
}
