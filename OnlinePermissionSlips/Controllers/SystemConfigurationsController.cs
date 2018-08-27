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
	[Authorize(Roles ="System Admin")]
	public class SystemConfigurationsController : Controller
	{
		private OnlinePermissionSlipEntities db = new OnlinePermissionSlipEntities();

		// GET: SystemConfigurations
		public ActionResult Index()
		{
			SystemConfiguration config = null;
			int ConfigID = -1;
			try
			{
				config = db.SystemConfigurations.First();
				ConfigID = config.Id;
			}
			catch (Exception)
			{
				return HttpNotFound();
			}

			return RedirectToAction("Details", new { id = ConfigID });
		}

		// GET: SystemConfigurations/Details/5
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			SystemConfiguration systemConfiguration = db.SystemConfigurations.Find(id);
			if (systemConfiguration == null)
			{
				return HttpNotFound();
			}
			return View(systemConfiguration);
		}

		// GET: SystemConfigurations/Create
		public ActionResult Create()
		{
			return HttpNotFound();
		}

		// GET: SystemConfigurations/Edit/5
		public ActionResult Edit(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			SystemConfiguration systemConfiguration = db.SystemConfigurations.Find(id);
			if (systemConfiguration == null)
			{
				return HttpNotFound();
			}
			return View(systemConfiguration);
		}

		// POST: SystemConfigurations/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "Id,EmailDomain,EmailAPIKey,DefaultFromAddress")] SystemConfiguration systemConfiguration)
		{
			if (ModelState.IsValid)
			{
				db.Entry(systemConfiguration).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(systemConfiguration);
		}

		// GET: SystemConfigurations/Delete/5
		public ActionResult Delete(int? id)
		{
			return HttpNotFound();
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
