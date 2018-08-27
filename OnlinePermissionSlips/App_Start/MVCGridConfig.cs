[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(OnlinePermissionSlips.MVCGridConfig), "RegisterGrids")]

namespace OnlinePermissionSlips
{
	using System;
	using System.Web;
	using System.Web.Mvc;
	using System.Linq;
	using System.Collections.Generic;
	using MVCGrid.Models;
	using MVCGrid.Web;
	using OnlinePermissionSlips.Models.DAL;
	using OnlinePermissionSlips.Models.ViewModels;
	using Microsoft.AspNet.Identity;
	using System.Security.Principal;

	public static class MVCGridConfig
	{
		public static void RegisterGrids()
		{
			MVCGridDefinitionTable.Add("GuardianSearchResults", new MVCGridBuilder<ReportingSearchResultViewModel>()
		.WithAuthorizationType(AuthorizationType.AllowAnonymous)
		.AddColumns(cols =>
		{
			// Add your columns here
			cols.Add("SchoolName")
				.WithHeaderText("School")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(false)
				.WithFiltering(true)
				.WithValueExpression(c => c.SchoolName); // use the Value Expression to return the cell text for this column
			cols.Add("ClassName")
				.WithHeaderText("Class")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.ClassName);
			cols.Add("StudentName")
				.WithHeaderText("Student")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.StudentName); // use the Value Expression to return the cell text for this column
			cols.Add("EventName")
				.WithHeaderText("Event")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression((i, c) => c.UrlHelper.Action("PermissionSlipApproval", "PermissionSlips", new { i.PermissionSlipID, i.StudentID }))
				.WithValueTemplate("<a href='{Value}'>{Model.EventName}</a>", false);
				//.WithValueExpression(c => c.EventName); // use the Value Expression to return the cell text for this column
			cols.Add("EventStartDate")
				.WithHeaderText("Start")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.EventStartDate.ToShortDateString()); // use the Value Expression to return the cell text for this column
			cols.Add("EventEndDate")
				.WithHeaderText("End")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.EventEndDate.ToShortDateString()); // use the Value Expression to return the cell text for this column
			cols.Add("Approved")
				.WithHeaderText("Approved")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.Approved != null ? c.Approved.ToString() : "Pending"); // use the Value Expression to return the cell text for this column
			cols.Add("ApprovedBy")
				.WithHeaderText("Approved By")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.ApprovedBy); // use the Value Expression to return the cell text for this column

			//.WithValueExpression((i, c) => c.UrlHelper.Action("detail", "demo", new { id = i.YourProperty }));
		})
		.WithAdditionalQueryOptionNames("SchoolID", "ClassID", "StudentID", "PermissionSlipName", "StartDate", "EndDate", "ApprovalStatusID", "ShowResults")
		.WithAdditionalSetting("RenderLoadingDiv", true)
		.WithSorting(true, "EventStartDate", SortDirection.Dsc)
		.WithFiltering(true)
		.WithPaging(true, 5, true, 100)
		.WithQueryOnPageLoad(false)
		.WithPreloadData(false)
		.WithRetrieveDataMethod((context) =>
					{
						string UserID = context.CurrentHttpContext.User.Identity.GetUserId();
						var options = context.QueryOptions;

						string globalSearch = options.GetAdditionalQueryOptionString("Search"); //Text typed into search box
						string SchoolIDParam = options.GetAdditionalQueryOptionString("SchoolID");
						string ApprovalStatusIDParam = options.GetAdditionalQueryOptionString("ApprovalStatusID");
						int? SchoolID = null;
						int? ClassID = null;
						int? StudentID = null;
						int? ApprovalStatusID = null;
						if (SchoolIDParam != null && SchoolIDParam.Length > 0)
						{
							SchoolID = int.Parse(SchoolIDParam);
						}
						if (ApprovalStatusIDParam != null && ApprovalStatusIDParam.Length > 0)
						{
							ApprovalStatusID = int.Parse(ApprovalStatusIDParam);
						}
						int? ApprovedStatusTypeID = (int)Common.ApprovalStatusTypes.Approved;
						int? PendingStatusTypeID = (int)Common.ApprovalStatusTypes.Pending;
						bool? ApprovalStatus = null;
						if (ApprovalStatusID != null) { ApprovalStatus = (ApprovalStatusID == ApprovedStatusTypeID); }

						string StartDateParam = options.GetAdditionalQueryOptionString("StartDate");
						string EndDateParam = options.GetAdditionalQueryOptionString("EndDate");
						DateTime StartDT = DateTime.Parse(StartDateParam);
						DateTime EndDT = DateTime.Parse(EndDateParam);

						string ClassIDParam = options.GetAdditionalQueryOptionString("ClassID");
						if (ClassIDParam != null && ClassIDParam.Length > 0)
						{
							ClassID = int.Parse(ClassIDParam);
						}
						string StudentIDParam = options.GetAdditionalQueryOptionString("StudentID");
						if (StudentIDParam != null && StudentIDParam.Length > 0)
						{
							StudentID = int.Parse(StudentIDParam);
						}
						string PermissionSlipNameParam = options.GetAdditionalQueryOptionString("PermissionSlipName");

						var result = new QueryResult<ReportingSearchResultViewModel>();
						using (var db = new OnlinePermissionSlipEntities())
						{
							var query = db.GuardianPermissionSlipsViews.Where(q => q.SchoolID == (SchoolID ?? q.SchoolID))
													.Where(q => q.ClassID == (ClassID ?? q.ClassID))
													.Where(q => q.StudentID == (StudentID ?? q.StudentID))
													.Where(q => q.EventName == (PermissionSlipNameParam != null && PermissionSlipNameParam.Length > 0 ? PermissionSlipNameParam : q.EventName))
													.Where(q => q.UserID == UserID)
													.Where(q =>
														(q.StartDateTime <= StartDT && q.EndDateTime >= StartDT) ||
														(q.StartDateTime >= StartDT && q.StartDateTime <= EndDT)
													)
													.Where(q => q.Approved == (ApprovalStatus == null ? q.Approved : (ApprovalStatusID != PendingStatusTypeID ? ApprovalStatus : null)))
													.OrderByDescending(q => q.StartDateTime).AsQueryable();

							if (!String.IsNullOrWhiteSpace(options.SortColumnName))
							{
								switch (options.SortColumnName.ToLower())
								{
									case "schoolname":
										if (options.SortDirection == SortDirection.Asc)
										{
											query = query.OrderBy(q => q.SchoolName);
										}
										else
										{
											query = query.OrderByDescending(q => q.SchoolName);
										}
										break;
									case "classname":
										if (options.SortDirection == SortDirection.Asc)
										{
											query = query.OrderBy(q => q.ClassName);
										}
										else
										{
											query = query.OrderByDescending(q => q.ClassName);
										}
										break;
									case "studentname":
										if (options.SortDirection == SortDirection.Asc)
										{
											query = query.OrderBy(q => q.StudentName);
										}
										else
										{
											query = query.OrderByDescending(q => q.StudentName);
										}
										break;
									case "eventname":
										if (options.SortDirection == SortDirection.Asc)
										{
											query = query.OrderBy(q => q.EventName);
										}
										else
										{
											query = query.OrderByDescending(q => q.EventName);
										}
										break;
									case "eventstartdate":
										if (options.SortDirection == SortDirection.Asc)
										{
											query = query.OrderBy(q => q.StartDateTime);
										}
										else
										{
											query = query.OrderByDescending(q => q.StartDateTime);
										}
										break;
									case "eventenddate":
										if (options.SortDirection == SortDirection.Asc)
										{
											query = query.OrderBy(q => q.EndDateTime);
										}
										else
										{
											query = query.OrderByDescending(q => q.EndDateTime);
										}
										break;
									case "approved":
										if (options.SortDirection == SortDirection.Asc)
										{
											query = query.OrderBy(q => q.Approved);
										}
										else
										{
											query = query.OrderByDescending(q => q.Approved);
										}
										break;
									case "approvedby":
										if (options.SortDirection == SortDirection.Asc)
										{
											query = query.OrderBy(q => q.GuardianName);
										}
										else
										{
											query = query.OrderByDescending(q => q.GuardianName);
										}
										break;
								}
							}

							result.TotalRecords = query.Count();

							if (options.GetLimitOffset().HasValue)
							{
								query = query.Skip(options.GetLimitOffset().Value).Take(options.GetLimitRowcount().Value);
							}

							List<ReportingSearchResultViewModel> resultList = new List<ReportingSearchResultViewModel>();

							foreach (GuardianPermissionSlipsView g in query.ToList())
							{
								resultList.Add(new ReportingSearchResultViewModel()
								{
									SchoolName = g.SchoolName,
									ClassName = g.ClassName,
									StudentID = g.StudentID,
									StudentName = g.StudentName,
									PermissionSlipID = g.EventID,
									EventName = g.EventName,
									EventStartDate = g.StartDateTime,
									EventEndDate = g.EndDateTime,
									Approved = g.Approved,
									ApprovedBy = g.GuardianName
								});
							}
							result.Items = resultList;
						}

						return result;
					}));

			MVCGridDefinitionTable.Add("GuardianSearchResultsMobile", new MVCGridBuilder<ReportingSearchResultViewModel>()
		.WithAuthorizationType(AuthorizationType.AllowAnonymous)
		.AddColumns(cols =>
		{
			// Add your columns here
			cols.Add("StudentName")
				.WithHeaderText("Student")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.StudentName); // use the Value Expression to return the cell text for this column
			cols.Add("EventName")
				.WithHeaderText("Event")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression((i, c) => c.UrlHelper.Action("PermissionSlipApproval", "PermissionSlips", new { i.PermissionSlipID, i.StudentID }))
				.WithValueTemplate("<a href='{Value}'>{Model.EventName}</a>", false);
			//.WithValueExpression(c => c.EventName); // use the Value Expression to return the cell text for this column
			cols.Add("EventStartDate")
				.WithHeaderText("Start")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.EventStartDate.ToShortDateString()); // use the Value Expression to return the cell text for this column
			cols.Add("Approved")
				.WithHeaderText("Approved")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.Approved != null ? c.Approved.ToString() : "Pending"); // use the Value Expression to return the cell text for this column

			//.WithValueExpression((i, c) => c.UrlHelper.Action("detail", "demo", new { id = i.YourProperty }));
		})
		.WithAdditionalQueryOptionNames("SchoolID", "ClassID", "StudentID", "PermissionSlipName", "StartDate", "EndDate", "ApprovalStatusID", "ShowResults")
		.WithAdditionalSetting("RenderLoadingDiv", true)
		.WithSorting(true, "EventStartDate", SortDirection.Dsc)
		.WithFiltering(true)
		.WithPaging(true, 5, true, 100)
		.WithQueryOnPageLoad(false)
		.WithPreloadData(false)
		.WithRetrieveDataMethod((context) =>
		{
			string UserID = context.CurrentHttpContext.User.Identity.GetUserId();
			var options = context.QueryOptions;

			string globalSearch = options.GetAdditionalQueryOptionString("Search"); //Text typed into search box
			string SchoolIDParam = options.GetAdditionalQueryOptionString("SchoolID");
			string ApprovalStatusIDParam = options.GetAdditionalQueryOptionString("ApprovalStatusID");
			int? SchoolID = null;
			int? ClassID = null;
			int? StudentID = null;
			int? ApprovalStatusID = null;
			if (SchoolIDParam != null && SchoolIDParam.Length > 0)
			{
				SchoolID = int.Parse(SchoolIDParam);
			}
			if (ApprovalStatusIDParam != null && ApprovalStatusIDParam.Length > 0)
			{
				ApprovalStatusID = int.Parse(ApprovalStatusIDParam);
			}
			int? ApprovedStatusTypeID = (int)Common.ApprovalStatusTypes.Approved;
			int? PendingStatusTypeID = (int)Common.ApprovalStatusTypes.Pending;
			bool? ApprovalStatus = null;
			if (ApprovalStatusID != null) { ApprovalStatus = (ApprovalStatusID == ApprovedStatusTypeID); }

			string StartDateParam = options.GetAdditionalQueryOptionString("StartDate");
			string EndDateParam = options.GetAdditionalQueryOptionString("EndDate");
			DateTime StartDT = DateTime.Parse(StartDateParam);
			DateTime EndDT = DateTime.Parse(EndDateParam);

			string ClassIDParam = options.GetAdditionalQueryOptionString("ClassID");
			if (ClassIDParam != null && ClassIDParam.Length > 0)
			{
				ClassID = int.Parse(ClassIDParam);
			}
			string StudentIDParam = options.GetAdditionalQueryOptionString("StudentID");
			if (StudentIDParam != null && StudentIDParam.Length > 0)
			{
				StudentID = int.Parse(StudentIDParam);
			}
			string PermissionSlipNameParam = options.GetAdditionalQueryOptionString("PermissionSlipName");

			var result = new QueryResult<ReportingSearchResultViewModel>();
			using (var db = new OnlinePermissionSlipEntities())
			{
				var query = db.GuardianPermissionSlipsViews.Where(q => q.SchoolID == (SchoolID ?? q.SchoolID))
										.Where(q => q.ClassID == (ClassID ?? q.ClassID))
										.Where(q => q.StudentID == (StudentID ?? q.StudentID))
										.Where(q => q.EventName == (PermissionSlipNameParam != null && PermissionSlipNameParam.Length > 0 ? PermissionSlipNameParam : q.EventName))
										.Where(q => q.UserID == UserID)
										.Where(q =>
											(q.StartDateTime <= StartDT && q.EndDateTime >= StartDT) ||
											(q.StartDateTime >= StartDT && q.StartDateTime <= EndDT)
										)
										.Where(q => q.Approved == (ApprovalStatus == null ? q.Approved : (ApprovalStatusID != PendingStatusTypeID ? ApprovalStatus : null)))
										.OrderByDescending(q => q.StartDateTime).AsQueryable();

				if (!String.IsNullOrWhiteSpace(options.SortColumnName))
				{
					switch (options.SortColumnName.ToLower())
					{
						case "studentname":
							if (options.SortDirection == SortDirection.Asc)
							{
								query = query.OrderBy(q => q.StudentName);
							}
							else
							{
								query = query.OrderByDescending(q => q.StudentName);
							}
							break;
						case "eventname":
							if (options.SortDirection == SortDirection.Asc)
							{
								query = query.OrderBy(q => q.EventName);
							}
							else
							{
								query = query.OrderByDescending(q => q.EventName);
							}
							break;
						case "eventstartdate":
							if (options.SortDirection == SortDirection.Asc)
							{
								query = query.OrderBy(q => q.StartDateTime);
							}
							else
							{
								query = query.OrderByDescending(q => q.StartDateTime);
							}
							break;
						case "approved":
							if (options.SortDirection == SortDirection.Asc)
							{
								query = query.OrderBy(q => q.Approved);
							}
							else
							{
								query = query.OrderByDescending(q => q.Approved);
							}
							break;
					}
				}

				result.TotalRecords = query.Count();

				if (options.GetLimitOffset().HasValue)
				{
					query = query.Skip(options.GetLimitOffset().Value).Take(options.GetLimitRowcount().Value);
				}

				List<ReportingSearchResultViewModel> resultList = new List<ReportingSearchResultViewModel>();

				foreach (GuardianPermissionSlipsView g in query.ToList())
				{
					resultList.Add(new ReportingSearchResultViewModel()
					{
						SchoolName = g.SchoolName,
						ClassName = g.ClassName,
						StudentID = g.StudentID,
						StudentName = g.StudentName,
						PermissionSlipID = g.EventID,
						EventName = g.EventName,
						EventStartDate = g.StartDateTime,
						EventEndDate = g.EndDateTime,
						Approved = g.Approved,
						ApprovedBy = g.GuardianName
					});
				}
				result.Items = resultList;
			}

			return result;
		}));


			MVCGridDefinitionTable.Add("TeacherSearchResults", new MVCGridBuilder<ReportingSearchResultViewModel>()
		.WithAuthorizationType(AuthorizationType.AllowAnonymous)
		.AddColumns(cols =>
		{
			// Add your columns here
			cols.Add("SchoolName")
				.WithHeaderText("School")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(false)
				.WithFiltering(true)
				.WithValueExpression(c => c.SchoolName); // use the Value Expression to return the cell text for this column
			cols.Add("ClassName")
				.WithHeaderText("Class")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.ClassName);
			cols.Add("StudentName")
				.WithHeaderText("Student")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.StudentName); // use the Value Expression to return the cell text for this column
			cols.Add("EventName")
				.WithHeaderText("Event")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.EventName); // use the Value Expression to return the cell text for this column
			cols.Add("EventStartDate")
				.WithHeaderText("Start")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.EventStartDate.ToShortDateString()); // use the Value Expression to return the cell text for this column
			cols.Add("EventEndDate")
				.WithHeaderText("End")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.EventEndDate.ToShortDateString()); // use the Value Expression to return the cell text for this column
			cols.Add("Approved")
				.WithHeaderText("Approved")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.Approved != null ? c.Approved.ToString() : "Pending"); // use the Value Expression to return the cell text for this column
			cols.Add("ApprovedBy")
				.WithHeaderText("Approved By")
				.WithAllowChangeVisibility(true)
				.WithSorting(true)
				.WithVisibility(true)
				.WithFiltering(true)
				.WithValueExpression(c => c.ApprovedBy); // use the Value Expression to return the cell text for this column

			//.WithValueExpression((i, c) => c.UrlHelper.Action("detail", "demo", new { id = i.YourProperty }));
		})
		.WithAdditionalQueryOptionNames("SchoolID", "ClassID", "StudentID", "PermissionSlipName", "StartDate", "EndDate", "ShowResults")
		.WithAdditionalSetting("RenderLoadingDiv", true)
		.WithSorting(true, "EventStartDate", SortDirection.Dsc)
		.WithFiltering(true)
		.WithPaging(true, 5, true, 100)
		.WithQueryOnPageLoad(false)
		.WithPreloadData(false)
		.WithRetrieveDataMethod((context) =>
		{
			IPrincipal User = context.CurrentHttpContext.User;
			string UserID = User.Identity.GetUserId();
			var options = context.QueryOptions;

			string globalSearch = options.GetAdditionalQueryOptionString("Search"); //Text typed into search box
			string SchoolIDParam = options.GetAdditionalQueryOptionString("SchoolID");
			int? SchoolID = null;
			int? ClassID = null;
			int? StudentID = null;
			if (SchoolIDParam != null && SchoolIDParam.Length > 0)
			{
				SchoolID = int.Parse(SchoolIDParam);
			}

			string StartDateParam = options.GetAdditionalQueryOptionString("StartDate");
			string EndDateParam = options.GetAdditionalQueryOptionString("EndDate");
			DateTime StartDT = DateTime.Parse(StartDateParam);
			DateTime EndDT = DateTime.Parse(EndDateParam);

			string ClassIDParam = options.GetAdditionalQueryOptionString("ClassID");
			if (ClassIDParam != null && ClassIDParam.Length > 0)
			{
				ClassID = int.Parse(ClassIDParam);
			}
			string StudentIDParam = options.GetAdditionalQueryOptionString("StudentID");
			if (StudentIDParam != null && StudentIDParam.Length > 0)
			{
				StudentID = int.Parse(StudentIDParam);
			}
			string PermissionSlipNameParam = options.GetAdditionalQueryOptionString("PermissionSlipName");

			var result = new QueryResult<ReportingSearchResultViewModel>();
			using (var db = new OnlinePermissionSlipEntities())
			{
				List<int> SchoolIDs = null;
				if (User.IsInRole("Teacher") || User.IsInRole("School Admin"))
				{
					SchoolIDs = db.AspNetUsers.Find(UserID).Schools.Where(s => s.SchoolID == (SchoolID ?? s.SchoolID)).Select(s => s.SchoolID).ToList();
				}
				else if (User.IsInRole("System Admin"))
				{
					SchoolIDs = db.Schools.Where(s => s.SchoolID == (SchoolID ?? s.SchoolID)).Select(s => s.SchoolID).ToList();
				}
				else
				{
					SchoolIDs = new List<int>();
				}

				if (!User.IsInRole("Teacher"))
				{
					UserID = null;
				}

				var query = db.TeacherPermissionSlipsViews.Where(q => SchoolIDs.Any(s => s == q.SchoolID))
										.Where(q => q.ClassID == (ClassID ?? q.ClassID))
										.Where(q => q.StudentID == (StudentID ?? q.StudentID))
										.Where(q => q.EventName == (PermissionSlipNameParam != null && PermissionSlipNameParam.Length > 0 ? PermissionSlipNameParam : q.EventName))
										.Where(q => q.TeacherUserID == (UserID ?? q.TeacherUserID))
										.Where(q =>
												(q.StartDateTime <= StartDT && q.EndDateTime >= StartDT) ||
												(q.StartDateTime >= StartDT && q.StartDateTime <= EndDT)
										)
										.OrderByDescending(q => q.StartDateTime).AsQueryable();

				if (!String.IsNullOrWhiteSpace(options.SortColumnName))
				{
					switch (options.SortColumnName.ToLower())
					{
						case "schoolname":
							if (options.SortDirection == SortDirection.Asc)
							{
								query = query.OrderBy(q => q.SchoolName);
							}
							else
							{
								query = query.OrderByDescending(q => q.SchoolName);
							}
							break;
						case "classname":
							if (options.SortDirection == SortDirection.Asc)
							{
								query = query.OrderBy(q => q.ClassName);
							}
							else
							{
								query = query.OrderByDescending(q => q.ClassName);
							}
							break;
						case "studentname":
							if (options.SortDirection == SortDirection.Asc)
							{
								query = query.OrderBy(q => q.StudentName);
							}
							else
							{
								query = query.OrderByDescending(q => q.StudentName);
							}
							break;
						case "eventname":
							if (options.SortDirection == SortDirection.Asc)
							{
								query = query.OrderBy(q => q.EventName);
							}
							else
							{
								query = query.OrderByDescending(q => q.EventName);
							}
							break;
						case "eventstartdate":
							if (options.SortDirection == SortDirection.Asc)
							{
								query = query.OrderBy(q => q.StartDateTime);
							}
							else
							{
								query = query.OrderByDescending(q => q.StartDateTime);
							}
							break;
						case "eventenddate":
							if (options.SortDirection == SortDirection.Asc)
							{
								query = query.OrderBy(q => q.EndDateTime);
							}
							else
							{
								query = query.OrderByDescending(q => q.EndDateTime);
							}
							break;
						case "approved":
							if (options.SortDirection == SortDirection.Asc)
							{
								query = query.OrderBy(q => q.Approved);
							}
							else
							{
								query = query.OrderByDescending(q => q.Approved);
							}
							break;
						case "approvedby":
							if (options.SortDirection == SortDirection.Asc)
							{
								query = query.OrderBy(q => q.GuardianName);
							}
							else
							{
								query = query.OrderByDescending(q => q.GuardianName);
							}
							break;
					}
				}

				result.TotalRecords = query.Count();

				if (options.GetLimitOffset().HasValue)
				{
					query = query.Skip(options.GetLimitOffset().Value).Take(options.GetLimitRowcount().Value);
				}

				List<ReportingSearchResultViewModel> resultList = new List<ReportingSearchResultViewModel>();

				foreach (TeacherPermissionSlipsView g in query.ToList())
				{
					resultList.Add(new ReportingSearchResultViewModel()
					{
						SchoolName = g.SchoolName,
						ClassName = g.ClassName,
						StudentName = g.StudentName,
						EventName = g.EventName,
						EventStartDate = g.StartDateTime,
						EventEndDate = g.EndDateTime,
						Approved = g.Approved,
						ApprovedBy = g.GuardianName
					});
				}
				result.Items = resultList;
			}

			return result;
		}));
		}
	}
}