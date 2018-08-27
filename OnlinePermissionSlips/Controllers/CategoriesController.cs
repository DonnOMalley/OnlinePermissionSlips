using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OnlinePermissionSlips.Models.DAL;

namespace OnlinePermissionSlips.Controllers
{
	public class CategoriesController : Controller
	{
		private OnlinePermissionSlipEntities db = new OnlinePermissionSlipEntities();

		// GET: Categories
		public ActionResult Index()
		{
			List<int> schoolIDs = Common.GetSchools(db, User).Select(s => s.SchoolID).ToList();
			var Categories = db.Categories.Where(c => schoolIDs.Any(t => t == c.SchoolID) || c.UserDefined == false); //Get Common Categories OR those assigned to the school's templates.
			return View(Categories.ToList());
		}

		// GET: Categories/Details/5
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Category category = db.Categories.Find(id);
			if (category == null)
			{
				return HttpNotFound();
			}
			return View(category);
		}

		// GET: Categories/Create
		public ActionResult Create()
		{
			return View();
		}

		// POST: Categories/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "ID,Name,UserDefined")] Category category)
		{
			if (ModelState.IsValid)
			{
				db.Categories.Add(category);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(category);
		}

		// GET: Categories/Edit/5
		public ActionResult Edit(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Category category = db.Categories.Find(id);
			if (category == null)
			{
				return HttpNotFound();
			}
			return View(category);
		}

		// POST: Categories/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "ID,Name,UserDefined")] Category category)
		{
			if (ModelState.IsValid)
			{
				db.Entry(category).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(category);
		}

		// GET: Categories/Delete/5
		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Category category = db.Categories.Find(id);
			if (category == null)
			{
				return HttpNotFound();
			}
			return View(category);
		}

		// POST: Categories/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			Category category = db.Categories.Find(id);
			db.Categories.Remove(category);
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
