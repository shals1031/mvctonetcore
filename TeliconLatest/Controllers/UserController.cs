using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    public class UserController : Controller
    {
        private readonly TeliconDbContext db;
        private readonly IWebHostEnvironment _env;
        public UserController(TeliconDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            _env = env;
        }
        //
        // GET: /User/
        //[TeliconAuthorize(TaskId = 2)]
        public ActionResult Index()
        {
            var roles = db.Roles.Select(x => new SelectListItem
            {
                Text = x.RoleName,
                Value = x.RoleName
            }).ToList();
            roles.Insert(0, new SelectListItem { Value = "a", Text = "All" });
            ViewBag.Roles = roles;
            return View();
        }
        //[TeliconAuthorize(TaskId = 2)]
        public ActionResult Create()
        {
            ViewBag.Roles = db.Roles.Select(x => new SelectListItem
            {
                Text = x.RoleName,
                Value = x.RoleName
            }).ToList();
            var model = new TeliconUser { IsNew = true };
            return View("CreateOrUpdate", model);
        }
        //[TeliconAuthorize(TaskId = 2)]
        public ActionResult Edit(string id)
        {
            ViewBag.Roles = db.Roles.Select(x => new SelectListItem
            {
                Text = x.RoleName,
                Value = x.RoleName
            }).ToList();
            //var role = System.Web.Security.Roles.GetRolesForUser(id)[0];
            //var user = Membership.GetUser(id);
            //var tuser = new TeliconUser();
            //if (user != null)
            //{
            //    tuser = new TeliconUser
            //    {
            //        Profile = role == "Technician" || role == "Supervisor"
            //            ? db.ADM03300.Where(x => x.Email == id).Select(x => new ProfileInfo
            //            {
            //                FirstName = x.FirstName,
            //                LastName = x.LastName,
            //                AltPhone = x.Phone2,
            //                Phone = x.Phone1
            //            }).FirstOrDefault()
            //            : AppProfile.GetProfile(id).ProfileInfo,
            //        Role = role,
            //        OldRole = role,
            //        Email = user.Email,
            //        UserName = id,
            //        IsNew = false,
            //        IsApproved = user.IsApproved,
            //        OldUserName = id
            //    };
            //}
            return View("CreateOrUpdate");
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 2, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(TeliconUser model)
        {
            try
            {
                if (model.IsNew)
                {
                    //var user = Membership.CreateUser(model.UserName, "Password123.", model.Email);
                    //user.IsApproved = model.IsApproved;
                    //Membership.UpdateUser(user);
                    //System.Web.Security.Roles.AddUserToRole(model.UserName, model.Role);
                    //AppProfile profile = AppProfile.GetProfile(model.UserName);
                    //profile.ProfileInfo.FirstName = model.Profile.FirstName;
                    //profile.ProfileInfo.LastName = model.Profile.LastName;
                    //profile.ProfileInfo.Phone = model.Profile.Phone;
                    //profile.ProfileInfo.AltPhone = model.Profile.AltPhone;
                    //profile.Save();
                }
                else
                {
                    //var user = Membership.GetUser(model.UserName);
                    //if (user != null)
                    //{
                    //    System.Web.Security.Roles.RemoveUserFromRole(model.OldUserName, model.OldRole);
                    //    var savedUser = db.Users.FirstOrDefault(x => x.UserName == model.OldUserName);
                    //    if (savedUser != null)
                    //        savedUser.UserName = model.UserName.ToLower();
                    //    db.SaveChanges();
                    //    user.Email = model.Email;
                    //    user.IsApproved = model.IsApproved;
                    //    Membership.UpdateUser(user);
                    //    System.Web.Security.Roles.AddUserToRole(model.UserName, model.Role);
                    //    AppProfile profile = AppProfile.GetProfile(model.UserName);
                    //    profile.ProfileInfo.FirstName = model.Profile.FirstName;
                    //    profile.ProfileInfo.LastName = model.Profile.LastName;
                    //    profile.ProfileInfo.Phone = model.Profile.Phone;
                    //    profile.ProfileInfo.AltPhone = model.Profile.AltPhone;
                    //    profile.Save();
                    //}
                }
                return Json(new JsonReturnParams
                {
                    Additional = new
                    {
                        OldUserName = model.UserName,
                        OldRole = model.Role,
                        IsNew = false
                    },
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.IsNew ? "200" : "300",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 2, Mode = ActionMode.Write)]
        public async Task<JsonResult> Delete(string id)
        {
            try
            {
                //if (System.Web.Security.Roles.IsUserInRole(id, DataDictionaries.AllRoles["Technician"]))
                //{
                //    return new ContractorController().Delete(-1, id);
                //}
                //await Task.Run(() => {
                //    Membership.DeleteUser(id);
                //});
                return Json(new JsonReturnParams
                {
                    Additional = 1,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = "400",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        public JsonResult ResetPassword(string id)
        {
            //var user = Membership.GetUser(id);
            //if (user != null && !user.IsLockedOut)
            //{
            //    var pswd = user.ResetPassword();
            //    user.ChangePassword(pswd, "Password123.");
            //    Membership.UpdateUser(user);

            //    #region Password Sending To User 

            //    string fromEmail = "portal@telicongroup.com";
            //    string toEmail = user.Email;
            //    //Send email confirmation.
            //    string emailBody = Utilities.GetEmailTemplateValue("ForgotPassword/Body");
            //    string emailSubject = Utilities.GetEmailTemplateValue("ForgotPassword/Subject");

            //    //Replace data in mail body.
            //    emailBody = emailBody.Replace("@@@USERNAME", user.Email);
            //    emailBody = emailBody.Replace("@@@PASSWORD", "Password123.");

            //    Utilities.SendMail(fromEmail, "vinit.jain@arkasoftwares.com", emailBody, emailSubject);
            //    #endregion

            //    return Json(pswd, JsonRequestBehavior.AllowGet);
            //}
            return Json(new { msg = "User is locked out" });
        }
        [HttpPost]
        public async Task<JsonResult> UnlockAccount(string id)
        {
            var unlocked = false;
            //await Task.Run(() =>
            //{
            //    var user = Membership.GetUser(id);
            //    if (user != null)
            //    {
            //        unlocked = user.UnlockUser();
            //    }
            //});
            return Json(unlocked);
        }
        [HttpPost]
        public async Task<JsonResult> ActivateAccount(string id)
        {
            var ok = true;
            try
            {
                //await Task.Run(() =>
                //{
                //    var user = Membership.GetUser(id);
                //    if (user != null)
                //    {
                //        user.IsApproved = !user.IsApproved;
                //        Membership.UpdateUser(user);
                //    }
                //});
            }
            catch
            {
                ok = false;
            }
            return Json(ok);
        }
        public Dictionary<string, string> GetAllowedRoles()
        {
            return DataDictionaries.AllRoles;
                //System.Web.Security.Roles.IsUserInRole("AppAdmin") ? DataDictionaries.AllRoles :
                //DataDictionaries.AllRoles.Where(x => x.Key != "AppAdmin").ToDictionary(x => x.Key, x => x.Value);
        }
    }
}