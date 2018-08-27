using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OnlinePermissionSlips.Models;
using OnlinePermissionSlips.Models.DAL;

namespace OnlinePermissionSlips.Controllers
{
	[Authorize(Roles = "System Admin, School Admin")]
	public class TeachersController : Controller
	{
		private OnlinePermissionSlipEntities db = new OnlinePermissionSlipEntities();

		// GET: Teachers
		public ActionResult Index()
		{
			string UserID = "";
			List<AspNetUser> TeacherList = new List<AspNetUser>();
			List<School> SchoolList = null;
			List<SelectListItem> Schools = new List<SelectListItem>();
			List<int> SchoolIDList = null;
			int SchoolID = -1;
			try
			{
				UserID = User.Identity.GetUserId();
				//Schools = Common.GetSchoolsForDropdown(db, User);
				SchoolList = Common.GetSchools(db, User);
				Schools.AddRange(SchoolList.Select(s => new SelectListItem() { Text = s.SchoolName, Value = s.SchoolID.ToString() }));
				ViewBag.SchoolID = Schools;
				if (SchoolList.Count == 1)
				{
					SchoolID = SchoolList[0].SchoolID;
					TeacherList = db.Schools.Where(s => s.SchoolID == SchoolID).SelectMany(s => s.AspNetUsers).Where(u => u.AspNetRoles.Any(r => r.Name == "Teacher")).ToList();
				}
				else if (SchoolList.Count > 1)
				{
					SchoolIDList = SchoolList.Select(s => s.SchoolID).ToList();
					TeacherList = db.Schools.Where(s => SchoolIDList.Any(si => si == s.SchoolID)).SelectMany(s => s.AspNetUsers).Where(u => u.AspNetRoles.Any(r => r.Name == "Teacher")).ToList();
				}
				else if(User.IsInRole("System Admin"))
				{
					TeacherList = db.AspNetUsers.Where(u => u.AspNetRoles.Any(r => r.Name == "Teacher")).ToList();
				}

			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "Exception Obtaining Teacher List :: " + ex.ToString());
			}

			return View(TeacherList);
		}

		// GET: Teachers/Details/5
		public ActionResult Details(string id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			AspNetUser aspNetUser = db.AspNetUsers.Find(id);
			if (aspNetUser == null)
			{
				return HttpNotFound();
			}
			return View(aspNetUser);
		}

		// GET: Teachers/Create
		public ActionResult Create()
		{
			string UserID = "";
			CreateTeacher createTeacher = new CreateTeacher();
			List<SelectListItem> Schools = null;
			try
			{
				UserID = User.Identity.GetUserId();
				Schools = Common.GetSchoolsForDropdown(db, User);
				ViewBag.SchoolID = Schools;
				if (Schools.Count == 1)
				{
					createTeacher.SchoolID = int.Parse(Schools[0].Value);
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "Exception Initializing Teacher Creation :: " + ex.ToString());
			}

			return View(createTeacher);
		}

		// POST: Teachers/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "SchoolID,UserName,Password,ConfirmPassword,FirstName,MiddleName,LastName,Email,Email,PhoneNumber")] CreateTeacher createTeacher)
		{
			ApplicationUser user = null;
			ApplicationUserManager UserMgr = null;
			AspNetUser aspNetUser = null;
			AspNetRole TeacherRole = null;
			School school = null;
			IdentityResult result = null;

			if (ModelState.IsValid)
			{
				//TOOD : Create User
				user = new ApplicationUser
				{
					UserName = createTeacher.UserName,
					Email = createTeacher.Email,
					FirstName = createTeacher.FirstName,
					MiddleName = createTeacher.MiddleName,
					LastName = createTeacher.LastName,
					PhoneNumber = createTeacher.PhoneNumber
				};
				try
				{
					UserMgr = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

					aspNetUser = db.AspNetUsers.Where(a => a.UserName == createTeacher.UserName).FirstOrDefault();

					if (aspNetUser != null && aspNetUser.Id != "") { throw new Exception("Username is not available"); }
					if (aspNetUser == null)
					{
						//Attempt to create the AspNetUser Identity with credentials.
						//Emails must be unique - May want to remove this requirement
						result = UserMgr.Create(user, createTeacher.Password);
						if (!result.Succeeded)
						{
							ModelState.AddModelError("", "Unable to create user " + createTeacher.UserName);
							foreach (string err in result.Errors)
							{
								ModelState.AddModelError("", err);
							}
							throw new Exception();
						}
						//Reload aspNetUser variable
						aspNetUser = db.AspNetUsers.Where(a => a.UserName == createTeacher.UserName).FirstOrDefault();
					}

					TeacherRole = aspNetUser.AspNetRoles.Where(r => r.Name == "Teacher").FirstOrDefault();
					//If the User is new OR the user was not successfully added to the teacher role previously
					//Attempt to add the User to the Teacher Role
					if (TeacherRole == null)
					{
						result = UserMgr.AddToRole(aspNetUser.Id, "Teacher");
						if (!result.Succeeded)
						{
							ModelState.AddModelError("", "Unable to assign " + aspNetUser.FullName + " the role of 'Teacher'");
							foreach (string err in result.Errors)
							{
								ModelState.AddModelError("", err);
							}
							throw new Exception();
						}
					}

					//Get School to assign to the User and the user to the school
					school = db.Schools.Where(s => s.SchoolID == createTeacher.SchoolID).FirstOrDefault();
					aspNetUser.Schools.Add(school);
					school.AspNetUsers.Add(aspNetUser);
					if (db.SaveChanges() > 0)
					{
						// Send an email with this link
						string code = UserMgr.GenerateEmailConfirmationToken(user.Id);
						var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
						UserMgr.SendEmail(user.Id, "Confirm your account", "<h2>Welcome to Online Permission Slips by Trash Panda Solutions.</h2>" + Environment.NewLine + Environment.NewLine +
																																"You have been added to " + school.SchoolName + " with a Teacher Account" + Environment.NewLine + Environment.NewLine +
																																"Your user name is: " + createTeacher.UserName + Environment.NewLine +
																																"Your password will be provided by your school admin" + Environment.NewLine + Environment.NewLine +
																																"You can confirm your account using the following link: <a href=\"" + callbackUrl + "\">Click Here to confirm your account</a>");

						return RedirectToAction("Index");
					}
					else
					{
						ModelState.AddModelError("", "Login Created but an error occurred trying to assign Teacher to School");
					}

					return RedirectToAction("Index");
				}
				catch (DbEntityValidationException ex)
				{
					//ModelState.AddModelError("", "Login Created but an error occurred trying to assign Teacher to School");
					foreach (DbEntityValidationResult ValidationErrors in ex.EntityValidationErrors)
					{
						if (!ValidationErrors.IsValid)
						{
							foreach (DbValidationError ValidationError in ValidationErrors.ValidationErrors)
							{
								ModelState.AddModelError(ValidationError.PropertyName, ValidationError.ErrorMessage);
							}
						}
					}
				}
				catch (Exception ex)
				{
					//ModelState.AddModelError("", "Login Created but an error occurred trying to assign Teacher to School");
					ModelState.AddModelError("", ex.Message);
					foreach (var key in ex.Data.Keys)
					{
						ModelState.AddModelError("", key + " :: " + ex.Data[key].ToString());
					}
				}
			}

			
			ViewBag.SchoolID = Common.GetSchoolsForDropdown(db, User);
			return View(createTeacher);
		}

		// GET: Teachers/Edit/5
		public ActionResult Edit(string id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			AspNetUser aspNetUser = db.AspNetUsers.Find(id);
			if (aspNetUser == null)
			{
				return HttpNotFound();
			}
			return View(aspNetUser);
		}

		// POST: Teachers/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,FirstName,MiddleName,LastName")] AspNetUser aspNetUser)
		{
			if (ModelState.IsValid)
			{
				db.Entry(aspNetUser).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(aspNetUser);
		}

		// GET: Teachers/Delete/5
		public ActionResult Delete(string id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			AspNetUser aspNetUser = db.AspNetUsers.Find(id);
			if (aspNetUser == null)
			{
				return HttpNotFound();
			}
			return View(aspNetUser);
		}

		// POST: Teachers/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(string id)
		{
			AspNetUser aspNetUser = db.AspNetUsers.Find(id);
			foreach(ClassRoom c in aspNetUser.ClassRooms)
			{
				c.Students.Clear();
			}
			aspNetUser.ClassRooms.Clear();
			aspNetUser.AspNetRoles.Clear();
			aspNetUser.Schools.Clear();
			db.AspNetUsers.Remove(aspNetUser);
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
