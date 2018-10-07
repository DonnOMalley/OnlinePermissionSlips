using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
	[Authorize(Roles = "System Admin")]
	public class SchoolsController : Controller
	{
		private OnlinePermissionSlipEntities db = new OnlinePermissionSlipEntities();

		// GET: Schools
		public ActionResult Index()
		{
			return View(db.Schools.ToList());
		}

		// GET: Schools/Details/5
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			School school = db.Schools.Find(id);
			if (school == null)
			{
				return HttpNotFound();
			}
			return View(school);
		}

		// GET: Schools/Create
		public ActionResult Create()
		{
			return View();
		}

		// POST: Schools/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(School school)
		{
			int SaveResult = 0;
			AspNetUser aspNetUser = null;
			ApplicationUser user = null;
			ApplicationUserManager UserMgr = null;
			if (ModelState.IsValid)
			{

				db.Schools.Add(school);
				SaveResult = db.SaveChanges();
				if(SaveResult > 0)
				{
					//db.Entry(school).Reload(); //Get ID for school

					//Create AdminUser and assign to school
					user = new ApplicationUser
					{
						UserName = school.AdminUserName,
						Email = school.Email,
						FirstName = school.FirstName,
						MiddleName = school.MiddleName,
						LastName = school.LastName,
						PhoneNumber = null
					};

					UserMgr = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

					IdentityResult result = UserMgr.Create(user, school.AdminPassword);
					if (result.Succeeded)
					{
						result = UserMgr.AddToRole(user.Id, "School Admin");
						if (result.Succeeded)
						{

							aspNetUser = db.AspNetUsers.Where(a => a.Email == school.Email).FirstOrDefault();
							aspNetUser.Schools.Add(school);
							school.AspNetUsers.Add(aspNetUser);

							SaveResult = db.SaveChanges();

							if(SaveResult > 0)
							{
								// Send an email with this link
								string code = UserMgr.GenerateEmailConfirmationToken(user.Id);
								var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
								UserMgr.SendEmail(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a> and logging in with the credentials you were provided.");

								return RedirectToAction("Index");
							}
						}
					}

				}
				return RedirectToAction("Index");
			}

			return View(school);
		}

		// GET: Schools/Edit/5
		public ActionResult Edit(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			School school = db.Schools.Find(id);
			if (school == null)
			{
				return HttpNotFound();
			}
			return View(school);
		}

		// POST: Schools/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "SchoolID,SchoolName")] School school)
		{
			if (ModelState.IsValid)
			{
				db.Entry(school).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(school);
		}

		// GET: Schools/Delete/5
		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			School school = db.Schools.Find(id);
			if (school == null)
			{
				return HttpNotFound();
			}
			return View(school);
		}

		// POST: Schools/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			//Delete everything associated with school
			School school = db.Schools.Find(id);

			school.AspNetUsers.Clear();

			db.Templates.RemoveRange(school.Templates);
			school.Templates.Clear();

			foreach(Student s in school.Students)
			{
				//db.GuardianTempEmails.RemoveRange(s.GuardianTempEmails);
				//s.GuardianTempEmails.Clear();

				db.GuardianApprovals.RemoveRange(s.GuardianApprovals);
				s.GuardianApprovals.Clear();

				s.Guardians.Clear();
			}
			db.Students.RemoveRange(school.Students);
			school.Students.Clear();


			db.Categories.RemoveRange(school.Categories);
			school.Categories.Clear();

			foreach(ClassRoom c in school.ClassRooms)
			{
				db.PermissionSlips.RemoveRange(c.PermissionSlips);
				c.PermissionSlips.Clear();
			}

			db.ClassRooms.RemoveRange(school.ClassRooms);
			school.ClassRooms.Clear();


			db.Schools.Remove(school);
			db.SaveChanges();
			return RedirectToAction("Index");
		}

		[HttpGet]


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
