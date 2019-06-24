﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Raf.FileMan.Models;
using Raf.FileMan.Context;

using System.Data.Entity;
using Raf.FileMan.Classes;

namespace Raf.FileMan.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
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

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message, string id = "")
        {
            string retTo = "folder";
            long? retId = -1;

            string retFun = "";
            SessionState ss;

            if (Session["SessionState"] != null)
            {
                ss = (SessionState)Session["SessionState"];
                retTo = ss.ReturnTo;
                retId = ss.ReturnId;
            }
            else
            {
                ItemService _is = new ItemService();

                retId = _is.GetRoot().Id;
                ss = new SessionState("manage", -1, (long)retId, null);
            }

            switch (retTo)
            {
                case "folder":
                    retFun = string.Format("goToFolder({0})", ss.ReturnId);
                    break;
                case "file":
                    retFun = string.Format("goToFile({0},{1})", ss.ReturnId, ss.CatId);
                    break;
                case "edit":
                    retFun = string.Format("goToEditFile({0},{1})", ss.ReturnId, ss.CatId);
                    break;
                case "search":
                    retFun = string.Format("goToSearch({0},'{1}',{2})", ss.CatId, ss.Search, ss.Scope);
                    break;
                case "admin":
                    retFun = string.Format("goToAdmin({0})", retId);
                    break;
                default:
                    retFun = string.Format("goToFolder({0})", retId);
                    break;
            }

            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = string.IsNullOrEmpty(id) ? User.Identity.GetUserId() : id;
            var user = await UserManager.FindByIdAsync(userId);

            var model = new IndexViewModel
            {
                Id = userId,
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                ShowOnRoot = user.UserSetting.ShowUncategorisedRoot,
                UncatVisible = user.UserSetting.UncategorisedVisible,
                ShowChangelog = user.UserSetting.ShowChangelog,
                SettingsId = user.UserSetting.Id,
                JoinDate = user.JoinDate,
                FirstName = user.FirstName,
                Surname = user.Surname,
                ReturnFunction = retFun,
                Settings = user.UserSetting
            };
            Session["SessionState"] = new SessionState("manage", ss.CatId, ss.DocId, ss.Search, ss.Scope, "manage", retId);
            return View(model);
        }

        public async Task<ActionResult> PartialIndex(ManageMessageId? message, string id = "")
        {
            string retTo = "folder";
            long? retId = -1;

            string retFun = "";
            SessionState ss;

            if (Session["SessionState"] != null)
            {
                ss = (SessionState)Session["SessionState"];
                retTo = ss.ReturnTo;
                retId = ss.ReturnId;
            }
            else
            {
                ItemService _is = new ItemService();

                retId = _is.GetRoot().Id;
                ss = new SessionState("manage", -1, (long)retId, null);
            }

            switch (retTo)
            {
                case "folder":
                    retFun = string.Format("goToFolder({0})", ss.ReturnId);
                    break;
                case "file":
                    retFun = string.Format("goToFile({0},{1})", ss.ReturnId, ss.CatId);
                    break;
                case "edit":
                    retFun = string.Format("goToEditFile({0},{1})", ss.ReturnId, ss.CatId);
                    break;
                case "search":
                    retFun = string.Format("goToSearch({0},'{1}',{2})", ss.CatId, ss.Search, ss.Scope);
                    break;
                case "admin":
                    retFun = string.Format("goToAdmin({0})", retId);
                    break;
                default:
                    retFun = string.Format("goToFolder({0})", retId);
                    break;
            }

            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = string.IsNullOrEmpty(id) ? User.Identity.GetUserId() : id;
            var user = await UserManager.FindByIdAsync(userId);

            var model = new IndexViewModel
            {
                Id = userId,
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                ShowOnRoot = user.UserSetting.ShowUncategorisedRoot,
                UncatVisible = user.UserSetting.UncategorisedVisible,
                ShowChangelog = user.UserSetting.ShowChangelog,
                SettingsId = user.UserSetting.Id,
                JoinDate = user.JoinDate,
                FirstName = user.FirstName,
                Surname = user.Surname,
                ReturnFunction = retFun,
                Settings = user.UserSetting

            };
            Session["SessionState"] = new SessionState("manage", ss.CatId, ss.DocId, ss.Search, ss.Scope, "manage", retId);
            return PartialView(model);
        }

        //
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSettings(long Id, UserSetting Settings)
        {
            AppDbContext _db = new AppDbContext();
            UserSetting settings = _db.UserSetting.Find(Id);
            bool rel = false;
            if (settings!=null)
            {
                settings.Theme = Settings.Theme;
                settings.ShowChangelog = Settings.ShowChangelog;
                settings.ShowUncategorisedRoot = Settings.ShowUncategorisedRoot;
                settings.UncategorisedVisible = Settings.UncategorisedVisible;
                settings.TreeSearch = Settings.TreeSearch;
                settings.TreeContext = Settings.TreeContext;
                settings.TreeDnD = Settings.TreeDnD;
                settings.TreeSort = Settings.TreeSort;
                settings.ForceDelete = Settings.ForceDelete;

                _db.SaveChanges();
                rel = !Settings.Theme.Equals((string)Session["theme"]);
                Session["theme"] = Settings.Theme;
                return Json(new { success = true, responseText = "Settings saved", reload = rel, id = Id }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, responseText = "User settings not found", reload = true, id = Id }, JsonRequestBehavior.AllowGet);
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
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, responseText = "Invalid model parameters" }, JsonRequestBehavior.AllowGet);
            }
            var result = await UserManager.ChangePasswordAsync(model.Id, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                if (model.Id.Equals(User.Identity.GetUserId()))
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                }
                return Json(new { success = true, responseText = "Password updated" }, JsonRequestBehavior.AllowGet);
            }
            AddErrors(result);
            return Json(new { success = false, responseText = "Error occured" }, JsonRequestBehavior.AllowGet);
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(model.Id, model.NewPassword);
                if (result.Succeeded)
                {
                    if (model.Id.Equals(User.Identity.GetUserId()))
                    {
                        var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                        if (user != null)
                        {
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        }
                    }
                    return Json(new { success = true, responseText = "Password set" }, JsonRequestBehavior.AllowGet);
                }
                AddErrors(result);
                return Json(new { success = false, responseText = "Error occured" }, JsonRequestBehavior.AllowGet);
            }

            // If we got this far, something failed, redisplay form
            return Json(new { success = false, responseText = "Invalid model parameters" }, JsonRequestBehavior.AllowGet);
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
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
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
            Error
        }

        #endregion
    }
}