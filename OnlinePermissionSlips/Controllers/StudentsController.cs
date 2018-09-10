using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OnlinePermissionSlips.Models.DAL;

namespace OnlinePermissionSlips.Controllers
{
	[Authorize]
	public class StudentsController : Controller
	{
		private OnlinePermissionSlipEntities db = new OnlinePermissionSlipEntities();

		// GET: Students
		public ActionResult Index()
		{
			List<int> schoolIDs = Common.GetSchools(db, User).Select(s => s.SchoolID).ToList();
			List<int> classIDs = Common.GetClassRooms(db, User).Select(c => c.ID).ToList();
			var students = db.Students.Where(s => schoolIDs.Any(si => si == s.SchoolID) && classIDs.Any(ci => ci == s.ClassRoomID)).Include(s => s.ClassRoom).Include(s => s.School);
			return View(students.ToList());
		}

		// GET: Students/Details/5
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Student student = db.Students.Find(id);
			if (student == null)
			{
				return HttpNotFound();
			}
			return View(student);
		}

		// GET: Students/Create
		public ActionResult Create()
		{
			//Build School ID and then use Ajax to build Classrooms like w/ Teachers on ClassRoomsController
			int schoolID = -1;
			List<SelectListItem> SchoolIDs = Common.GetSchoolsForDropdown(db, User);
			ViewBag.SchoolID = SchoolIDs;
			if (SchoolIDs.Count == 1)
			{
				schoolID = int.Parse(SchoolIDs[0].Value);
			}
			ViewBag.ClassRoomID = Common.GetClassRoomsForDropdown(db, User, schoolID);
			return View();
		}

		// POST: Students/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "StudentNumber,FullName,ClassRoomID,SchoolID")] Student student)
		{
			int schoolID = -1;
			List<SelectListItem> SchoolIDs = null;

			if (ModelState.IsValid)
			{
				try
				{
					db.Students.Add(student);
					db.SaveChanges();
					return RedirectToAction("Index");
				}
				catch (DbEntityValidationException ex)
				{
					ModelState.AddModelError("", "Error creating student");
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
					ModelState.AddModelError("", "Error creating student");
					ModelState.AddModelError("", ex.Message);
					foreach (var key in ex.Data.Keys)
					{
						ModelState.AddModelError("", key + " :: " + ex.Data[key].ToString());
					}
				}
			}

			SchoolIDs = Common.GetSchoolsForDropdown(db, User);
			ViewBag.SchoolID = SchoolIDs;
			if (SchoolIDs.Count == 1)
			{
				schoolID = int.Parse(SchoolIDs[0].Value);
			}
			ViewBag.ClassRoomID = Common.GetClassRoomsForDropdown(db, User, schoolID);

			return View(student);
		}

		// GET: Students/Edit/5
		public ActionResult Edit(int? id)
		{
			int schoolID = -1;
			List<SelectListItem> SchoolIDs = null;

			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Student student = db.Students.Find(id);
			if (student == null)
			{
				return HttpNotFound();
			}

			SchoolIDs = Common.GetSchoolsForDropdown(db, User);
			ViewBag.SchoolID = SchoolIDs;
			if (SchoolIDs.Count == 1)
			{
				schoolID = int.Parse(SchoolIDs[0].Value);
			}
			ViewBag.ClassRoomID = Common.GetClassRoomsForDropdown(db, User, schoolID);

			return View(student);
		}

		// POST: Students/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "ID,StudentNumber,FullName,ClassRoomID,SchoolID")] Student student)
		{
			int schoolID = -1;
			List<SelectListItem> SchoolIDs = null;

			if (ModelState.IsValid)
			{
				try
				{
					db.Entry(student).State = EntityState.Modified;
					db.SaveChanges();
					return RedirectToAction("Index");
				}
				catch (DbEntityValidationException ex)
				{
					ModelState.AddModelError("", "Error updating student");
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
					ModelState.AddModelError("", "Error updating student");
					ModelState.AddModelError("", ex.Message);
					foreach (var key in ex.Data.Keys)
					{
						ModelState.AddModelError("", key + " :: " + ex.Data[key].ToString());
					}
				}
			}

			SchoolIDs = Common.GetSchoolsForDropdown(db, User);
			ViewBag.SchoolID = SchoolIDs;
			if (SchoolIDs.Count == 1)
			{
				schoolID = int.Parse(SchoolIDs[0].Value);
			}
			ViewBag.ClassRoomID = Common.GetClassRoomsForDropdown(db, User, schoolID);
			return View(student);
		}

		// GET: Students/Delete/5
		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Student student = db.Students.Find(id);
			if (student == null)
			{
				return HttpNotFound();
			}
			return View(student);
		}

		// POST: Students/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			Student student = null;
			try
			{
				student = db.Students.Find(id);
				db.Students.Remove(student);
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			catch (DbEntityValidationException ex)
			{
				ModelState.AddModelError("", "Error creating student");
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
				ModelState.AddModelError("", "Error creating student");
				ModelState.AddModelError("", ex.Message);
				foreach (var key in ex.Data.Keys)
				{
					ModelState.AddModelError("", key + " :: " + ex.Data[key].ToString());
				}
			}
			return View(student);
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
