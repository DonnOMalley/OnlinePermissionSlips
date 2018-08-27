﻿using System.Web;
using System.Web.Optimization;

namespace OnlinePermissionSlips
{
  public class BundleConfig
  {
    public static void RegisterBundles(BundleCollection bundles)
    {
      bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                  "~/Scripts/jquery-{version}.js"));

      bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                  "~/Scripts/jquery.validate*"));

      // Use the development version of Modernizr to develop with and learn from. Then, when you're
      // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
      bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                  "~/Scripts/modernizr-*"));

      bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
								"~/Scripts/moment.min.js",
								"~/Scripts/bootstrap-datetimepicker.min.js",
                "~/Scripts/respond.js"));

      bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
								"~/Content/bootstrap-datetimepicker.min.css",
								"~/Content/site.css"));

			bundles.Add(new ScriptBundle("~/bundles/AppJS").Include(
									"~/Scripts/app.js"));
		}
  }
}