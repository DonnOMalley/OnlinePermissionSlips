using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using OnlinePermissionSlips.Models.DAL;
using System.Data.Entity;
using Microsoft.AspNet.Identity;

namespace OnlinePermissionSlips
{
	public static class Common
	{
		public enum ApprovalStatusTypes { Approved = 0, Pending = 1, Not_Approved = 2};
		public const string MailGunAPIUrl = "https://api.mailgun.net/v3";

		public static void EnsureUserIsAuthenticated(IPrincipal User)
		{
			if (!User.Identity.IsAuthenticated) { throw new Exception("User Must Be Authenticated"); }
		}

		public static List<School> GetSchools(OnlinePermissionSlipEntities db, IPrincipal User)
		{
			List<School> SchoolList = new List<School>();
			List<int> UserSchoolIDs = new List<int>();
			string userID = "";
			try
			{
				EnsureUserIsAuthenticated(User);
				if (User.IsInRole("System Admin"))
				{
					SchoolList = db.Schools.ToList();
				}
				else
				{
					userID = User.Identity.GetUserId();
					//Get School User is associated with.
					if (User.IsInRole("Teacher"))
					{
						UserSchoolIDs = db.AspNetUsers.Where(u => u.Id == userID).SelectMany(u => u.Schools).Select(s => s.SchoolID).ToList();
					}
					else if (User.IsInRole("School Admin") || User.IsInRole("Guardian"))
					{
						UserSchoolIDs = db.AspNetUsers.Find(userID).Schools.Select(s => s.SchoolID).ToList();
					}
					else if (User.IsInRole("System Admin"))
					{
						UserSchoolIDs = db.Schools.Select(s => s.SchoolID).ToList();
					}
					SchoolList = db.Schools.Where(s => UserSchoolIDs.Any(us => us == s.SchoolID)).ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to Obtain List of Schools for " + (User.Identity.IsAuthenticated ? User.Identity.Name : "User [Not Authenticated]"), ex);
			}
			return SchoolList;
		}

		public static List<AspNetUser> GetTeachers(OnlinePermissionSlipEntities db, IPrincipal User, int? SchoolID = null)
		{
			List<AspNetUser> Teachers = new List<AspNetUser>();
			string UserID = "";
			string TeacherRoleID = "";
			try
			{
				EnsureUserIsAuthenticated(User);
				UserID = User.Identity.GetUserId();

				if (TeacherRoleID == null) { throw new Exception("Invalid Role Configuration - Teacher Role is Missing"); }

				if(User.IsInRole("Teacher")) 
				{
					//return self!
					Teachers = db.AspNetUsers.Where(a => a.Id == UserID).ToList();
				}
				else if (User.IsInRole("School Admin"))
				{
					if(SchoolID == null) { throw new Exception("School must be included to query list of teachers"); }

					School school = db.AspNetUsers.Where(u => u.Id == UserID).FirstOrDefault().Schools.Where(s => s.SchoolID == SchoolID).FirstOrDefault();
					if(school != null)
					{
						Teachers = db.AspNetUsers.Where(u => u.AspNetRoles.Any(r => r.Name == "Teacher") && u.Schools.Any(s => s.SchoolID == SchoolID)).ToList();
					}
					
				}
				else if (User.IsInRole("System Admin"))
				{
					if(SchoolID == null) { throw new Exception("School must be included to query list of teachers"); }
					Teachers = db.AspNetUsers.Where(u => u.AspNetRoles.Any(r => r.Name == "Teacher") && u.Schools.Any(s => s.SchoolID == SchoolID)).ToList();
				}
				else if (User.IsInRole("Guardian"))
				{
					//TODO : Do Guardians need to make this call??
				}
				else
				{
					throw new Exception("Invalid Role assignment");
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to Obtain List of Teachers for " + (User.Identity.IsAuthenticated ? User.Identity.Name : "User [Not Authenticated]"), ex);
			}
			return Teachers;
		}

		public static List<ClassRoom> GetClassRooms(OnlinePermissionSlipEntities db, IPrincipal User, int? SchoolID = null)
		{
			List<ClassRoom> ClassList = new List<ClassRoom>();
			string UserID = "";
			try
			{
				EnsureUserIsAuthenticated(User);
				UserID = User.Identity.GetUserId();

				if (User.IsInRole("Teacher"))
				{
					ClassList = db.ClassRooms.Where(c => c.TeacherUserID == UserID && ((SchoolID == null) || (c.SchoolID == SchoolID))).ToList();
				}
				else if (User.IsInRole("School Admin"))
				{
					List<int> schoolIDs = (SchoolID != null) ? new List<int>() { (int)SchoolID } : GetSchools(db, User).Select(s => s.SchoolID).ToList();
					ClassList = db.ClassRooms.Where(c => schoolIDs.Any(s => s == c.SchoolID)).ToList();
				}
				else if (User.IsInRole("System Admin"))
				{
					if(SchoolID != null)
					{
						ClassList = db.ClassRooms.Where(c => c.SchoolID == SchoolID).ToList();
					}
					else
					{
						ClassList = db.ClassRooms.ToList();
					}
				}
				else if (User.IsInRole("Guardian"))
				{
					//TODO : Do Guardians need to make this call??
					ClassList = db.AspNetUsers.Find(UserID).Students.Select(s => s.ClassRoom).Distinct().ToList();
				}
				else
				{
					throw new Exception("Invalid Role assignment");
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to Obtain List of Class Rooms for " + (User.Identity.IsAuthenticated ? User.Identity.Name : "User [Not Authenticated]"), ex);
			}
			return ClassList;
		}

		public static List<Category> GetPermissionSlipCategories(OnlinePermissionSlipEntities db, IPrincipal User)
		{
			List<Category> PermissionSlipCategoryList = new List<Category>();
			try
			{
				EnsureUserIsAuthenticated(User);

				List<int> schoolIDs = Common.GetSchools(db, User).Select(s => s.SchoolID).ToList();
				var Categories = db.Categories.Where(c => schoolIDs.Any(t => t == c.SchoolID) || c.UserDefined == false); //Get Common Categories OR those assigned to the school's templates.

				PermissionSlipCategoryList = Categories.ToList();
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to Obtain List of PermissionSlipCategory Rooms for " + (User.Identity.IsAuthenticated ? User.Identity.Name : "User [Not Authenticated]"), ex);
			}
			return PermissionSlipCategoryList;
		}

		public static List<Template> GetPermissionSlipTemplates(OnlinePermissionSlipEntities db, IPrincipal User, int? SchoolID = null, int? ClassRoomID = null, bool includeDisabled = false)
		{
			List<Template> TemplateList = new List<Template>();
			string UserID = "";
			try
			{
				EnsureUserIsAuthenticated(User);
				UserID = User.Identity.GetUserId();
				List<int> SchoolIDList = null;
				if(SchoolID != null)
				{
					SchoolIDList = new List<int>() { (int)SchoolID };
				}
				else
				{
					SchoolIDList = db.AspNetUsers.Find(UserID).Schools.Select(s => s.SchoolID).ToList();
				}
				//Only look for Templates by ClassRoom ID if assigned - Different from schools which must be either provided or based on the user making the request.
				if (ClassRoomID != null)
				{
					if (includeDisabled)
					{
						TemplateList = db.Templates.Where(t => SchoolIDList.Any(s => t.SchoolID == s) && t.ClassRoomID == ClassRoomID).ToList();
					}
					else
					{
						TemplateList = db.Templates.Where(t => SchoolIDList.Any(s => t.SchoolID == s) && t.ClassRoomID == ClassRoomID && t.DisabledDateTime == null).ToList();
					}
				}
				else
				{
					if (includeDisabled)
					{
						TemplateList = db.Templates.Where(t => SchoolIDList.Any(s => t.SchoolID == s)).ToList();
					}
					else
					{
						TemplateList = db.Templates.Where(t => SchoolIDList.Any(s => t.SchoolID == s) && t.DisabledDateTime == null).ToList();
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to Obtain List of Templates for " + (User.Identity.IsAuthenticated ? User.Identity.Name : "User [Not Authenticated]"), ex);
			}
			return TemplateList;
		}

		public static List<Student> GetStudents(OnlinePermissionSlipEntities db, IPrincipal User, int? ClassID = null)
		{
			List<Student> Students = null;
			string UserID = "";

			try
			{
				EnsureUserIsAuthenticated(User);
				UserID = User.Identity.GetUserId();
				//if (ClassID != null)
				//{
				//	Students = db.ClassRooms.Find(ClassID).Students.ToList();
				//}
				//else 
				if (User.IsInRole("Guardian"))
				{
					Students = db.AspNetUsers.Find(UserID).Students.Where(s => s.ClassRoomID == (ClassID != null ? ClassID : s.ClassRoomID)).ToList();
				}
				else if (User.IsInRole("Teacher"))
				{
					Students = db.AspNetUsers.Find(UserID).ClassRooms.Where(c => c.ID ==(ClassID != null ? ClassID : c.ID)).SelectMany(c => c.Students).ToList();
				}
				else if (User.IsInRole("School Admin"))
				{
					Students = db.AspNetUsers.Find(UserID).Schools.SelectMany(s => s.Students).Where(c => c.ClassRoomID == (ClassID != null ? ClassID : c.ClassRoomID)).ToList();
				}
				else if (User.IsInRole("System Admin"))
				{
					Students = db.Students.ToList();
				}
				else
				{
					throw new Exception();
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to list students", ex);
			}

			return Students;
		}

		public static List<PermissionSlip> GetPermissionSlips(OnlinePermissionSlipEntities db, IPrincipal User, DateTime? StartDate = null, DateTime? EndDate = null)
		{
			List<PermissionSlip> PermissionSlips = new List<PermissionSlip>();
			string UserID = "";
			if(StartDate == null) { StartDate = new DateTime(0); }
			if(EndDate == null) { EndDate = DateTime.Now.AddYears(100); }
			try
			{
				UserID = User.Identity.GetUserId();
				if (User.IsInRole("System Admin"))
				{
					PermissionSlips = db.PermissionSlips.Where(p => ((p.StartDateTime <= StartDate) &&
																													(p.EndDateTime >= StartDate)) ||
																													((p.StartDateTime <= EndDate) &&
																													(p.EndDateTime >= EndDate))).ToList();
				}
				else if (User.IsInRole("School Admin"))
				{
					PermissionSlips = db.AspNetUsers.Find(UserID).Schools.SelectMany(s => s.ClassRooms).SelectMany(c => c.PermissionSlips.Where(p => ((p.StartDateTime <= StartDate) && 
																																																																						(p.EndDateTime >= StartDate)) || 
																																																																						((p.StartDateTime <= EndDate) &&
																																																																						(p.EndDateTime >= EndDate)))).ToList();
				}
				else if (User.IsInRole("Teacher"))
				{
					PermissionSlips = db.AspNetUsers.Find(UserID).ClassRooms.SelectMany(c => c.PermissionSlips.Where(p => ((p.StartDateTime <= StartDate) &&
																																																								(p.EndDateTime >= StartDate)) ||
																																																								((p.StartDateTime <= EndDate) &&
																																																								(p.EndDateTime >= EndDate)))).ToList();
				}
				else if (User.IsInRole("Guardian"))
				{
					PermissionSlips = db.AspNetUsers.Find(UserID).GuardianApprovals.Where(g => ((g.PermissionSlip.StartDateTime <= StartDate) &&
																																											(g.PermissionSlip.EndDateTime >= StartDate)) ||
																																											((g.PermissionSlip.StartDateTime <= EndDate) &&
																																											(g.PermissionSlip.EndDateTime >= EndDate)))
																																					.Select(g => g.PermissionSlip).ToList();
				}
			}
			catch (Exception ex)
			{
				throw;
			}
			return PermissionSlips;
		}

		public static List<SelectListItem> GetSchoolsForDropdown(OnlinePermissionSlipEntities db, IPrincipal User, int? SchoolID = null)
		{
			//Based on Roles and then assignments, get list of schools.
			List<SelectListItem> SchoolList = new List<SelectListItem>();
			List<School> DBSchoolList = null;
			int? schoolID = SchoolID;
			try
			{
				DBSchoolList = GetSchools(db, User);

				foreach (School s in DBSchoolList)
				{
					SelectListItem si = new SelectListItem()
					{
						Text = s.SchoolName,
						Value = s.SchoolID.ToString(),
						Selected = ((DBSchoolList.Count == 1) || ((schoolID != null) && (s.SchoolID == schoolID)))
					};

					SchoolList.Add(si);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to Create Dropdown List of Schools for " + (User.Identity.IsAuthenticated ? User.Identity.Name : "User [Not Authenticated]"), ex);
			}

			return SchoolList;
		}

		public static List<SelectListItem> GetTeachersForDropdown(OnlinePermissionSlipEntities db, IPrincipal User, string TeacherID = "", int? SchoolID = null)
		{
			//Based on Roles and then assignments, get list of schools.
			List<SelectListItem> TeacherList = new List<SelectListItem>();
			List<AspNetUser> DBTeacherList = null;
			int? schoolID = SchoolID;
			try
			{
				DBTeacherList = GetTeachers(db, User, SchoolID);

				foreach (AspNetUser u in DBTeacherList)
				{
					SelectListItem si = new SelectListItem()
					{
						Text = u.FullName,
						Value = u.Id,
						Selected = ((DBTeacherList.Count == 1) || ((TeacherID.Length > 0) && (u.Id == TeacherID)))
					};

					TeacherList.Add(si);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to Create Dropdown List of Teachers for " + (User.Identity.IsAuthenticated ? User.Identity.Name : "User [Not Authenticated]"), ex);
			}

			return TeacherList;
		}

		public static List<SelectListItem> GetClassRoomsForDropdown(OnlinePermissionSlipEntities db, IPrincipal User, int? SchoolID = null, int? ClassRoomID = null)
		{
			List<SelectListItem> ClassList = new List<SelectListItem>();
			List<ClassRoom> DBClassList = null;
			string UserID = "";
			try
			{
				EnsureUserIsAuthenticated(User);
				UserID = User.Identity.GetUserId();
				if (ClassRoomID != null)
				{
					DBClassList = db.ClassRooms.Where(c => c.ID == ClassRoomID).ToList();
				}
				else
				{
					DBClassList = GetClassRooms(db, User, SchoolID);
				}

				foreach (ClassRoom c in DBClassList)
				{
					//Only select the classroom if the classroom id has NOT been passed in or the teacher is assigned to that class AND only that class.
					SelectListItem si = new SelectListItem()
					{
						Text = c.GetClassName(),
						Value = c.ID.ToString(),
						Selected = (DBClassList.Count == 1) || ((DBClassList.Count > 1) && (c.ID == ClassRoomID))
					};

					ClassList.Add(si);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to Create Dropdown List of Schools for " + (User.Identity.IsAuthenticated ? User.Identity.Name : "User [Not Authenticated]"), ex);
			}
			return ClassList;
		}

		public static List<SelectListItem> GetPermissionSlipCategoriesForDropdown(OnlinePermissionSlipEntities db, IPrincipal User, int? CategoryID = null)
		{
			List<SelectListItem> PermissionSlipCategoryList = new List<SelectListItem>();
			List<Category> DBCategoryList = null;
			try
			{
				EnsureUserIsAuthenticated(User);
				DBCategoryList = GetPermissionSlipCategories(db, User);
				foreach (Category c in DBCategoryList)
				{
					SelectListItem si = new SelectListItem()
					{
						Text = c.Name,
						Value = c.ID.ToString(),
						Selected = ((CategoryID != null) && (c.ID == CategoryID))
					};

					PermissionSlipCategoryList.Add(si);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to Create Dropdown List of Categories", ex);
			}

			return PermissionSlipCategoryList;

		}

		public static List<SelectListItem> GetPermissionSlipTemplatesForDropdown(OnlinePermissionSlipEntities db, IPrincipal User, int? SchoolID = null, int? ClassRoomID = null, bool? includeDisabled = false, int? TemplateID = null)
		{
			List<SelectListItem> PermissionSlipTemplateList = new List<SelectListItem>();
			List<Template> DBPermissionSlipTemplates = null;
			try
			{
				DBPermissionSlipTemplates = GetPermissionSlipTemplates(db, User, SchoolID, ClassRoomID);
				foreach (Template t in DBPermissionSlipTemplates)
				{
					SelectListItem si = new SelectListItem()
					{
						Text = t.Name,
						Value = t.ID.ToString(),
						Selected = ((TemplateID != null) && (t.ID == TemplateID))
					};

					PermissionSlipTemplateList.Add(si);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to Create Dropdown List of Templates for " + (User.Identity.IsAuthenticated ? User.Identity.Name : "User [Not Authenticated]"), ex);
			}

			return PermissionSlipTemplateList;
		}

		public static List<SelectListItem> GetStudentsForDropdown(OnlinePermissionSlipEntities db, IPrincipal User, int? StudentID = null, int? ClassID = null)
		{
			List<SelectListItem> StudentList = new List<SelectListItem>();
			List<Student> students = null;
			try
			{
				students = GetStudents(db, User, ClassID);
				foreach(Student s in students)
				{
					StudentList.Add(new SelectListItem()
					{
						Text = s.FullName,
						Value = s.ID.ToString(),
						Selected = ((students.Count == 1) || (s.ID == StudentID))
					});
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to query students for drop down list", ex);
			}

			return StudentList;
		}

		public static List<SelectListItem> GetPermissionSlipNamesForDropdown(OnlinePermissionSlipEntities db, IPrincipal User, DateTime? StartDate, DateTime? EndDate)
		{
			List<SelectListItem> PermissionSlipList = new List<SelectListItem>();
			List<string> permissionSlipMames = null;
			try
			{
				permissionSlipMames = GetPermissionSlips(db, User, StartDate, EndDate).Select(p => p.Name).Distinct().ToList();
				foreach(string name in permissionSlipMames)
				{
					PermissionSlipList.Add(new SelectListItem()
					{
						Text = name,
						Value = name,
						Selected = (permissionSlipMames.Count == 1)
					});
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to query students for drop down list", ex);
			}

			return PermissionSlipList;
		}

		public static List<SelectListItem> GetPermissionSlipsForDropdown(OnlinePermissionSlipEntities db, IPrincipal User)
		{
			List<SelectListItem> PermissionSlipList = new List<SelectListItem>();
			List<PermissionSlip> permissionSlips = null;
			try
			{
				permissionSlips = GetPermissionSlips(db, User);
				foreach (PermissionSlip p in permissionSlips)
				{
					PermissionSlipList.Add(new SelectListItem()
					{
						Text = p.Name,
						Value = p.ID.ToString(),
						Selected = (permissionSlips.Count == 1)
					});
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to query students for drop down list", ex);
			}

			return PermissionSlipList;
		}
	}
}