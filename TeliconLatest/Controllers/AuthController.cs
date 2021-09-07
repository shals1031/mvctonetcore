using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    public class AuthController : Controller
    {
        private readonly TeliconDbContext db;
        public IConfiguration _configuration;
        public AuthController(TeliconDbContext db, IConfiguration configuration)
        {
            this.db = db;
            _configuration = configuration;
        }
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                if (HttpContext.User.IsInRole("Admin") || HttpContext.User.IsInRole("SuperAdmin") || HttpContext.User.IsInRole("AppAdmin"))
                    return RedirectToLocal(returnUrl);
                else
                    return RedirectToAction("index", "technician");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(LoginModel model, string returnUrl)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(model.Password);
            string encPassword = Convert.ToBase64String(encbuff);
            if (ModelState.IsValid && db.Users.Any(x => x.UserName == model.UserName && x.Membership.Password == encPassword))
            {
                Users users = db.Users.Include(x => x.Profiles).Include(p => p.Membership).FirstOrDefault(x => x.UserName == model.UserName);
                ProfileInfo profileInfo = Utilities.GetProfileInfoValue(users.Profiles.PropertyValueStrings);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.UserName),
                    new Claim(ClaimTypes.Email, users.Membership.Email),
                    new Claim(ClaimTypes.GivenName, profileInfo.FirstName.Substring(0, 1) + ". " + profileInfo.LastName),
                    new Claim(ClaimTypes.Version, users.ApplicationId),
                    new Claim(ClaimTypes.NameIdentifier, profileInfo.FirstName + " " + profileInfo.LastName),
                    new Claim(ClaimTypes.PrimarySid, users.UserId),
                };

                string[] roles = db.UsersInRoles.Where(x => x.UserId == users.UserId).Select(p => p.Roles.RoleName).ToArray();

                foreach (string role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                var props = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props).Wait();

                users.Membership.LastLoginDate = DateTime.Now;
                db.SaveChanges();

                return roles.Contains("Technician") || roles.Contains("Supervisor")
                    ? RedirectToAction("index", "technician")
                    : RedirectToLocal(returnUrl);
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }
        [HttpPost]
        public IActionResult LogOff()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Users user = db.Users.Include(p => p.Membership).FirstOrDefault(x => x.UserName == User.Identity.Name);
            user.Membership.LastLockoutDate = DateTime.Now;
            db.SaveChanges();

            return RedirectToAction("login", "auth");
        }
        [AllowAnonymous]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ForgetPassword(ForgetPassword model)
        {
            if (ModelState.IsValid)
            {
                #region reset email & send email to user.

                Memberships user = db.Memberships.FirstOrDefault(x => x.Email == model.Email);
                if (user != null && !user.IsLockedOut)
                {
                    byte[] encbuff = System.Text.Encoding.UTF8.GetBytes("Password123.");
                    user.Password = Convert.ToBase64String(encbuff);
                    db.SaveChanges();

                    //#region Password Sending To User

                    //string fromEmail = "portal@telicongroup.com";
                    //string toEmail = user.Email;
                    ////Send email confirmation.
                    //string emailBody = Utilities.GetEmailTemplateValue("ForgotPassword/Body");
                    //string emailSubject = Utilities.GetEmailTemplateValue("ForgotPassword/Subject");

                    ////Replace data in mail body.
                    //emailBody = emailBody.Replace("@@@USERNAME", user.Email);
                    //emailBody = emailBody.Replace("@@@PASSWORD", "Password123.");
                    //Utilities utilities = new Utilities(_configuration);
                    //utilities.SendMail(fromEmail, toEmail, emailBody, emailSubject);

                    //#endregion

                    ViewBag.Success = true;
                    return View();
                }
                #endregion
                ViewBag.Error = true;
                return View();
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Changepassword()
        {
            return PartialView("ChangePassword");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Changepassword(LocalPasswordModel model)
        {
            try
            {
                bool changed = false;
                Users user = db.Users.Include(p => p.Membership).FirstOrDefault(x => x.UserName == User.Identity.Name);

                byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(model.OldPassword);
                if (user.Membership.Password == Convert.ToBase64String(encbuff))
                {
                    encbuff = System.Text.Encoding.UTF8.GetBytes(model.ConfirmPassword);
                    user.Membership.Password = Convert.ToBase64String(encbuff);
                    user.Membership.LastPasswordChangedDate = DateTime.Now;

                    db.SaveChanges();
                    changed = true;
                }
                return Json(new JsonReturnParams
                {
                    Additional = changed ? "" : "Current Password is incorrect please try again.",
                    Code = changed ? "100" : "999",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = "300",
                    Msg = e.Message
                });
            }
        }
        [Authorize]
        public JsonResult KeepAlive()
        {
            return Json("OK");
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}