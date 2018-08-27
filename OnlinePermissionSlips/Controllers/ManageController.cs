using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using OnlinePermissionSlips.Models;
using OnlinePermissionSlips.Models.DAL;
using System.Data.Entity;
using System.Net;

namespace OnlinePermissionSlips.Controllers
{
	[Authorize]
	public class ManageController : Controller
	{
		OnlinePermissionSlipEntities EntityDB = new OnlinePermissionSlipEntities();
		private ApplicationSignInManager _signInManager;
		private ApplicationUserManager _userManager;
		private ApplicationRoleManager _roleManager;

		public ManageController()
		{
		}

		public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
		{
			UserManager = userManager;
			SignInManager = signInManager;
		}

		public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
		{
			UserManager = userManager;
			SignInManager = signInManager;
			RoleManager = roleManager;
		}

		public ApplicationSignInManager SignInManager
		{
			get
			{
				return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
			}
			private set
			{
				_signInManager = value;
			}
		}

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

		//
		// GET: /Manage/Index
		public async Task<ActionResult> Index(ManageMessageId? message)
		{
			ApplicationUser user = null;
			List<IdentityRole> userRoles = null;
			List<School> UserSchools = null;
			List<Student> UserStudents = null;
			List<ClassRoom> UserClasses = null;
			IndexViewModel model = null;
			string userId = "";

			ViewBag.StatusMessage =
					message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
					: message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
					: message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
					: message == ManageMessageId.Error ? "An error has occurred."
					: message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
					: message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
					: message == ManageMessageId.AddStudentSuccess ? "Student was successfully added"
					: message == ManageMessageId.RemoveStudentSuccess ? "Student was successfully removed"
					: message == ManageMessageId.AddClassSuccess ? "Class was successfully added"
					: message == ManageMessageId.RemoveClassSuccess ? "Class was successfully removed"
					: message == ManageMessageId.RemoveClassSuccess ? "Class was successfully removed"
					: "";

			userId = User.Identity.GetUserId();
			user = UserManager.FindById(userId);

			userRoles = null;
			userRoles = RoleManager.Roles.Where(r => r.Users.Any(ru => ru.UserId == userId)).ToList();

			if (User.IsInRole("Guardian"))
			{
				UserStudents = EntityDB.Students.Where(s => s.Guardians.Any(g => g.Id == userId)).ToList();
				UserSchools = UserStudents.Select(us => us.School).Distinct().ToList();
				UserClasses = new List<ClassRoom>();
			}
			else if (User.IsInRole("Teacher"))
			{
				UserClasses = EntityDB.ClassRooms.Where(c => c.TeacherUserID == userId).ToList();
				UserStudents = new List<Student>();
				UserSchools = UserClasses.Select(uc => uc.School).Distinct().ToList();
			}
			else if (User.IsInRole("School Admin"))
			{
				AspNetUser aspNetUser = EntityDB.AspNetUsers.Where(a => a.Id == userId).FirstOrDefault();
				if(aspNetUser == null) { throw new Exception("Unable to locate user account by ID"); }
				UserSchools = aspNetUser.Schools.ToList();
				UserStudents = new List<Student>();
				UserClasses = new List<ClassRoom>();
			}
			else if(User.IsInRole("System Admin"))
			{
				UserSchools = new List<School>();
				UserStudents = new List<Student>();
				UserClasses = new List<ClassRoom>();
			}
			else
			{
				throw new Exception("User Security Misconfigured");
			}

			model = new IndexViewModel
			{
				HasPassword = HasPassword(),
				PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
				TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
				Logins = await UserManager.GetLoginsAsync(userId),
				BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
				Email = user.Email,
				FirstName = user.FirstName,
				LastName = user.LastName,
				MiddleName = user.MiddleName,
				Schools = UserSchools,
				Students = UserStudents,
				Classes = UserClasses,
				Roles = userRoles
			};

			return View(model);
		}
		
		[HttpGet]
		public ActionResult AddStudent(int? id)
		{
			ViewBag.SchoolID = Common.GetSchoolsForDropdown(EntityDB, User);
			return View(new AddStudentViewModel() { ID = id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult AddStudent([Bind(Include = "ID,StudentNumber,FullName,SchoolID")] AddStudentViewModel model)
		{
			Student existingStudent = null;
			Student student = null;
			if (ModelState.IsValid)
			{
				try
				{
					if(model.ID != null)
					{
						existingStudent = EntityDB.Students.Find(model.ID);
					}

					student = EntityDB.Students.Where(s => s.StudentNumber == model.StudentNumber).FirstOrDefault();
					if(existingStudent != null && student != null)
					{
						if(existingStudent.StudentNumber != student.StudentNumber || existingStudent.FullName != student.FullName || existingStudent.SchoolID != student.SchoolID)
						{
							throw new Exception("Student cannot be verified. Double check information and try again. Contact the school administrator if the issues persists");
						}
					}
					if ((student != null) && (student.FullName == model.FullName) && (student.SchoolID == model.SchoolID))
					{
						//TODO : Add Guardian/Student Record and Guardian/School if not already existing.
						AspNetUser user = EntityDB.AspNetUsers.Find(User.Identity.GetUserId());
						Student userStudent = user.Students.Where(s => s.StudentNumber == model.StudentNumber).FirstOrDefault();

						if ((user != null) && (userStudent == null) && ((student.Guardian1TempEmail == user.Email) || (student.Guardian2TempEmail == user.Email)))
						{
							user.Students.Add(student);

							if (student.Guardian1TempEmail == user.Email)
							{
								student.Guardian1TempEmail = null;
							}
							else
							{
								student.Guardian2TempEmail = null;
							}

							EntityDB.Entry(student).State = EntityState.Modified;
							EntityDB.Entry(user).State = EntityState.Modified;
							EntityDB.SaveChanges();
							return RedirectToAction("Index", new { Message = ManageMessageId.AddStudentSuccess });
						}
						else if (userStudent != null)
						{
							ModelState.AddModelError("", "Student Number already added");
						}
						else if ((student.Guardian1TempEmail != user.Email) && (student.Guardian2TempEmail != user.Email))
						{
							ModelState.AddModelError("", "Student cannot be verified. Double check information and try again. Contact the school administrator if the issues persists");
						}
						else
						{
							ModelState.AddModelError("", "Unable to manage user account. Log off/on and try adding the student again. Contact support if the issue continues");
						}
					}
					else
					{
						ModelState.AddModelError("", "Student cannot be verified. Double check information and try again. Contact the school administrator if the issues persists");
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", "Exception adding Student. Double check information and try again. Contact the school administrator if the issues persists. :: " + ex.ToString());
				}
			}

			ViewBag.SchoolID = new SelectList(EntityDB.Schools.ToList(), "SchoolID", "SchoolName");
			return View(model);
		}

		[HttpGet]
		public ActionResult RemoveStudent(int id) //id = student number
		{
			RemoveStudentViewModel model = null;
			Student student = null;
			try
			{
				student = EntityDB.Students.Where(s => s.StudentNumber == id).FirstOrDefault();
				if (student != null)
				{
					model = new RemoveStudentViewModel()
					{
						StudentNumber = id,
						FullName = student.FullName
					};
					return View(model);
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "Exception Confirming Student Removal. Double check information and try again. Contact the school administrator if the issues persists. :: " + ex.ToString());
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult RemoveStudent([Bind(Include = "StudentNumber")] RemoveStudentViewModel model)
		{
			Student student = null;
			AspNetUser user = null;
			if (ModelState.IsValid)
			{
				try
				{
					student = EntityDB.Students.Where(s => s.StudentNumber == model.StudentNumber).FirstOrDefault();
					if (student != null)
					{
						user = EntityDB.AspNetUsers.Find(User.Identity.GetUserId());
						student.Guardians.Remove(user);

						EntityDB.SaveChanges();
						return RedirectToAction("Index", new { Message = ManageMessageId.RemoveStudentSuccess });
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", "Exception removing Student. Try again and contact the school administrator if the issues persists. :: " + ex.ToString());
				}
			}
			return View(model);
		}

		// POST: /Manage/RemoveLogin
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
		{
			ManageMessageId? message;
			var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
			if (result.Succeeded)
			{
				var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
				if (user != null)
				{
					await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
				}
				message = ManageMessageId.RemoveLoginSuccess;
			}
			else
			{
				message = ManageMessageId.Error;
			}
			return RedirectToAction("ManageLogins", new { Message = message });
		}

		//
		// GET: /Manage/AddPhoneNumber
		public ActionResult AddPhoneNumber()
		{
			return View();
		}

		//
		// POST: /Manage/AddPhoneNumber
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			// Generate the token and send it
			var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
			if (UserManager.SmsService != null)
			{
				var message = new IdentityMessage
				{
					Destination = model.Number,
					Body = "Your security code is: " + code
				};
				await UserManager.SmsService.SendAsync(message);
			}
			return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
		}

		//
		// POST: /Manage/EnableTwoFactorAuthentication
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> EnableTwoFactorAuthentication()
		{
			await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
			var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
			if (user != null)
			{
				await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
			}
			return RedirectToAction("Index", "Manage");
		}

		//
		// POST: /Manage/DisableTwoFactorAuthentication
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DisableTwoFactorAuthentication()
		{
			await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
			var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
			if (user != null)
			{
				await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
			}
			return RedirectToAction("Index", "Manage");
		}

		//
		// GET: /Manage/VerifyPhoneNumber
		public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
		{
			var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
			// Send an SMS through the SMS provider to verify the phone number
			return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
		}

		//
		// POST: /Manage/VerifyPhoneNumber
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
			if (result.Succeeded)
			{
				var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
				if (user != null)
				{
					await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
				}
				return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
			}
			// If we got this far, something failed, redisplay form
			ModelState.AddModelError("", "Failed to verify phone");
			return View(model);
		}

		//
		// POST: /Manage/RemovePhoneNumber
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> RemovePhoneNumber()
		{
			var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
			if (!result.Succeeded)
			{
				return RedirectToAction("Index", new { Message = ManageMessageId.Error });
			}
			var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
			if (user != null)
			{
				await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
			}
			return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
		}

		//
		// GET: /Manage/ChangePassword
		public ActionResult ChangePassword()
		{
			return View();
		}

		//
		// POST: /Manage/ChangePassword
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
			if (result.Succeeded)
			{
				var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
				if (user != null)
				{
					await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
				}
				return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
			}
			AddErrors(result);
			return View(model);
		}

		//
		// GET: /Manage/SetPassword
		public ActionResult SetPassword()
		{
			return View();
		}

		//
		// POST: /Manage/SetPassword
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
				if (result.Succeeded)
				{
					var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
					if (user != null)
					{
						await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
					}
					return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
				}
				AddErrors(result);
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		//
		// GET: /Manage/ManageLogins
		public async Task<ActionResult> ManageLogins(ManageMessageId? message)
		{
			ViewBag.StatusMessage =
					message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
					: message == ManageMessageId.Error ? "An error has occurred."
					: "";
			var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
			if (user == null)
			{
				return View("Error");
			}
			var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
			var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
			ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
			return View(new ManageLoginsViewModel
			{
				CurrentLogins = userLogins,
				OtherLogins = otherLogins
			});
		}

		//
		// POST: /Manage/LinkLogin
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LinkLogin(string provider)
		{
			// Request a redirect to the external login provider to link a login for the current user
			return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage", null, protocol: Request.Url.Scheme), User.Identity.GetUserId());
		}

		//
		// GET: /Manage/LinkLoginCallback
		public async Task<ActionResult> LinkLoginCallback()
		{
			var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
			if (loginInfo == null)
			{
				return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
			}
			var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
			return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _userManager != null)
			{
				_userManager.Dispose();
				_userManager = null;
			}

			base.Dispose(disposing);
		}

		#region Helpers
		// Used for XSRF protection when adding external logins
		private const string XsrfKey = "XsrfId";

		private IAuthenticationManager AuthenticationManager
		{
			get
			{
				return HttpContext.GetOwinContext().Authentication;
			}
		}

		private void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError("", error);
			}
		}

		private bool HasPassword()
		{
			var user = UserManager.FindById(User.Identity.GetUserId());
			if (user != null)
			{
				return user.PasswordHash != null;
			}
			return false;
		}

		private bool HasPhoneNumber()
		{
			var user = UserManager.FindById(User.Identity.GetUserId());
			if (user != null)
			{
				return user.PhoneNumber != null;
			}
			return false;
		}

		public enum ManageMessageId
		{
			AddPhoneSuccess,
			ChangePasswordSuccess,
			SetTwoFactorSuccess,
			SetPasswordSuccess,
			RemoveLoginSuccess,
			RemovePhoneSuccess,
			AddStudentSuccess,
			RemoveStudentSuccess,
			AddClassSuccess,
			RemoveClassSuccess,
			Error
		}

		#endregion
	}
}