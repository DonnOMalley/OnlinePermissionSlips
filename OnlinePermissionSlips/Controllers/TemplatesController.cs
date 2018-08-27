using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using OnlinePermissionSlips.Models;
using OnlinePermissionSlips.Models.DAL;

namespace OnlinePermissionSlips.Controllers
{
	[Authorize]
	public class TemplatesController : Controller
	{
		private OnlinePermissionSlipEntities db = new OnlinePermissionSlipEntities();

		// GET: Templates
		public ActionResult Index()
		{
			List<Template> Templates = Common.GetPermissionSlipTemplates(db, User);
			return View(Templates);
		}

		// GET: Templates/Details/5
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Template template = db.Templates.Find(id);
			if (template == null)
			{
				return HttpNotFound();
			}
			return View(template);
		}

		private void InitializeViewBagsForCreate(ref Template template)
		{
			List<SelectListItem> SchoolList = null;
			List<SelectListItem> ClassList = null;
			int? SchoolID = null;
			int? ClassID = null;

			SchoolList = Common.GetSchoolsForDropdown(db, User);
			if (SchoolList.Count == 1)
			{
				SchoolID = Convert.ToInt32(SchoolList[0].Value);
				ClassList = Common.GetClassRoomsForDropdown(db, User, SchoolID, ClassID);
				if (ClassList.Count == 1) { ClassID = Convert.ToInt32(ClassList[0].Value); }
			}
			else
			{
				ClassList = new List<SelectListItem>(); //Must Show up Empty and update based on School Selection
			}

			//Common manages access to various lists based on current "User' that is authenticated
			ViewBag.SchoolID = SchoolList;
			ViewBag.ClassRoomID = ClassList;
			ViewBag.CategoryID = Common.GetPermissionSlipCategoriesForDropdown(db, User);

			if (SchoolID != null)
			{
				template.SchoolID = (int)SchoolID;
				template.School = db.Schools.Find(SchoolID);
			}
			if (ClassID != null)
			{
				template.ClassRoomID = (int)ClassID;
				template.ClassRoom = db.ClassRooms.Find(ClassID);
			}
		}

		[HttpPost]
		public ActionResult GetTemplateForPermissionSlip(int TemplateID)
		{
			Template t = null;
			ContentResult content = new ContentResult()
			{
				ContentType = "application/json"
			};
			try
			{
				content.ContentType = "application/json";
				t = db.Templates.Find(TemplateID);
				//if (t == null) { throw new Exception("Unable to find Template by ID(" + TemplateID.ToString() + ")"); }
				if(t == null) { Console.WriteLine("Unable to find Template by ID(" + TemplateID.ToString() + ")"); }
			}
			catch (Exception)
			{
				throw;
			}

			content.Content = JsonConvert.SerializeObject(t, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			return content;
		}

		// GET: Templates/Create
		public ActionResult Create()
		{
			Template template = new Template();
			InitializeViewBagsForCreate(ref template);

			return View(template);
		}

		// POST: Templates/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "ID,SchoolID,ClassRoomID,CategoryID,Name,Location,Cost,RequireChaperone,RequireChaperoneBackgroundCheck")] Template template)
		{
			if (ModelState.IsValid)
			{
				template.SetCreated();

				if (template.ClassRoomID == null) //OR 0??
				{
					//Allow Null - Assign to school, not classroom.
				}

				db.Templates.Add(template);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			InitializeViewBagsForCreate(ref template);
			return View(template);
		}

		// GET: Templates/Edit/5
		public ActionResult Edit(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Template template = db.Templates.Find(id);
			if (template == null)
			{
				return HttpNotFound();
			}
			return View(template);
		}

		// POST: Templates/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "ID,Name,Location,Cost,RequireChaperone,RequireChaperoneBackgroundCheck,ConsistencyCheck")] Template template)
		{
			if (ModelState.IsValid)
			{
				template.ModifiedDateTime = DateTime.Now;
				db.Entry(template).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(template);
		}

		// GET: Templates/Delete/5
		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Template template = db.Templates.Find(id);
			if (template == null)
			{
				return HttpNotFound();
			}
			return View(template);
		}

		// POST: Templates/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			Template template = db.Templates.Find(id);
			db.Templates.Remove(template);
			db.SaveChanges();
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult GetTemplateList(int? SchoolID, int? ClassRoomID)
		{
			List<SelectListItem> TemplateList = null;
			try
			{
				TemplateList = Common.GetPermissionSlipTemplatesForDropdown(db, User, SchoolID, ClassRoomID);
			}
			catch (Exception)
			{
				throw;
			}

			return Json(TemplateList);
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
