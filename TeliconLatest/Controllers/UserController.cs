using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        public UserController(TeliconDbContext db)
        {
            this.db = db;
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

            var user = db.Users.Include(p => p.Membership).Include(p => p.Profiles).FirstOrDefault(t => t.UserName == id);
            var role = db.UsersInRoles.Include(p => p.Roles).FirstOrDefault(t => t.UserId == user.UserId).Roles;
            var tuser = new TeliconUser();
            if (user != null)
            {
                tuser = new TeliconUser
                {
                    Profile = role.RoleName == "Technician" || role.RoleName == "Supervisor"
                        ? db.ADM03300.Where(x => x.Email == id).Select(x => new ProfileInfo
                        {
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            AltPhone = x.Phone2,
                            Phone = x.Phone1
                        }).FirstOrDefault()
                        : Utilities.GetProfileInfoValue(user.Profiles != null ? user.Profiles.PropertyValueStrings : string.Empty),
                    Role = role.RoleName,
                    OldRole = role.RoleName,
                    Email = user.Membership != null ? user.Membership.Email : string.Empty,
                    UserName = id,
                    IsNew = false,
                    IsApproved = user.Membership != null && user.Membership.IsApproved,
                    OldUserName = id
                };
            }
            return View("CreateOrUpdate", tuser);
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
                    byte[] encbuff = System.Text.Encoding.UTF8.GetBytes("Password123.");
                    var user = new Users
                    {
                        ApplicationId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Version).Value,
                        IsAnonymous = false,
                        UserName = model.UserName,
                        Membership = new Memberships
                        {
                            ApplicationId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Version).Value,
                            Comment = string.Empty,
                            CreateDate = DateTime.Now,
                            Email = model.Email,
                            IsApproved = model.IsApproved,
                            IsLockedOut = false,
                            LastLockoutDate = DateTime.Now,
                            LastLoginDate = DateTime.Now,
                            LastPasswordChangedDate = DateTime.Now,
                            Password = Convert.ToBase64String(encbuff),
                        },
                    };
                    db.Users.Add(user);
                    db.SaveChanges();

                    UsersInRoles usersInRoles = new UsersInRoles
                    {
                        RoleId = db.Roles.FirstOrDefault(x => x.RoleName == model.Role).RoleId,
                        UserId = user.UserId
                    };
                    db.UsersInRoles.Add(usersInRoles);

                    byte[] bytes = Array.Empty<byte>();
                    ProfileInfo profileInfo = new ProfileInfo
                    {
                        AltPhone = model.Profile.AltPhone,
                        FirstName = model.Profile.FirstName,
                        LastName = model.Profile.LastName,
                        Phone = model.Profile.Phone
                    };
                    Profiles profiles = new Profiles
                    {
                        UserId = user.UserId,
                        LastUpdatedDate = DateTime.Now,
                        PropertyNames = "ProfileInfo:0:233",
                        PropertyValueStrings = Utilities.GetProfileInfoValue(profileInfo),
                        PropertyValueBinary = bytes,
                    };
                    db.Profiles.Add(profiles);
                    db.SaveChanges();
                }
                else
                {
                    var user = db.Users.Include(t => t.Membership).FirstOrDefault(t => t.UserName == model.UserName);
                    if (user != null)
                    {
                        var user1 = db.Users.FirstOrDefault(t => t.UserName == model.OldUserName);
                        if (db.UsersInRoles.Include(t => t.Roles).Any(p => p.Roles.RoleName == model.OldRole && p.UserId == user1.UserId))
                        {
                            db.UsersInRoles.RemoveRange(db.UsersInRoles.Include(t => t.Roles).Where(p => p.Roles.RoleName == model.OldRole && p.UserId == user1.UserId).ToList());
                        }
                        var savedUser = db.Users.FirstOrDefault(x => x.UserName == model.OldUserName);
                        if (savedUser != null)
                            savedUser.UserName = model.UserName.ToLower();
                        db.SaveChanges();
                        user.Membership.Email = model.Email;
                        user.Membership.IsApproved = model.IsApproved;
                        db.SaveChanges();

                        UsersInRoles usersInRoles = new UsersInRoles
                        {
                            RoleId = db.Roles.FirstOrDefault(x => x.RoleName == model.Role).RoleId,
                            UserId = user.UserId
                        };
                        db.UsersInRoles.Add(usersInRoles);

                        Profiles profiles = db.Profiles.FirstOrDefault(t => t.UserId == user.UserId);
                        if (profiles != null)
                        {
                            ProfileInfo profileInfo = new ProfileInfo
                            {
                                AltPhone = model.Profile.AltPhone,
                                FirstName = model.Profile.FirstName,
                                LastName = model.Profile.LastName,
                                Phone = model.Profile.Phone
                            };
                            profiles.PropertyValueStrings = Utilities.GetProfileInfoValue(profileInfo);
                        }

                        db.SaveChanges();
                    }
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
                if (db.UsersInRoles.Include(t => t.Roles).Any(t => t.UserId == id && t.Roles.RoleName == "Technician"))
                {
                    int conId = 0;
                    if (!string.IsNullOrEmpty(id))
                        conId = db.ADM03300.FirstOrDefault(x => x.Email == id).ConID;
                    if (db.ADM03400.Any(x => x.ContractorID == conId))
                    {
                        return Json(new JsonReturnParams
                        {
                            Additional = -1,
                            Code = "100",
                            Msg = ""
                        });
                    }
                    else
                    {
                        ADM03300 user = db.ADM03300.Find(id);
                        db.ADM03300.Remove(user);
                        Memberships memberships = db.Memberships.FirstOrDefault(x => x.Email == user.Email);
                        if (memberships != null)
                        {
                            db.Memberships.Remove(memberships);
                            db.UsersInRoles.RemoveRange(db.UsersInRoles.Where(p => p.UserId == memberships.UserId).ToList());
                            db.Profiles.Remove(db.Profiles.FirstOrDefault(t => t.UserId == memberships.UserId));
                            db.Users.Remove(db.Users.FirstOrDefault(p => p.UserId == memberships.UserId));
                            db.SaveChanges();
                        }
                        return Json(new JsonReturnParams
                        {
                            Additional = db.SaveChanges(),
                            Code = "100",
                            Msg = ""
                        });
                    }
                }
                await Task.Run(() =>
                {
                    db.Memberships.Remove(db.Memberships.FirstOrDefault(p => p.UserId == id));
                    db.UsersInRoles.RemoveRange(db.UsersInRoles.Where(p => p.UserId == id).ToList());
                    db.Profiles.Remove(db.Profiles.FirstOrDefault(t => t.UserId == id));
                    db.Users.Remove(db.Users.FirstOrDefault(p => p.UserId == id));
                    db.SaveChanges();
                });
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
            Memberships user = db.Memberships.FirstOrDefault(x => x.UserId == id);
            if (user != null && !user.IsLockedOut)
            {
                byte[] encbuff = System.Text.Encoding.UTF8.GetBytes("Password123.");
                user.Password = Convert.ToBase64String(encbuff);
                db.SaveChanges();

                #region Password Sending To User 

                //string fromEmail = "portal@telicongroup.com";
                //string toEmail = user.Email;
                //Send email confirmation.
                //string emailBody = Utilities.GetEmailTemplateValue("ForgotPassword/Body");
                //string emailSubject = Utilities.GetEmailTemplateValue("ForgotPassword/Subject");

                //Replace data in mail body.
                //emailBody = emailBody.Replace("@@@USERNAME", user.Email);
                //emailBody = emailBody.Replace("@@@PASSWORD", "Password123.");

                //Utilities.SendMail(fromEmail, "vinit.jain@arkasoftwares.com", emailBody, emailSubject);
                #endregion

                return Json("Password123.");
            }
            return Json(new { msg = "User is locked out" });
        }
        [HttpPost]
        public async Task<JsonResult> UnlockAccount(string id)
        {
            var unlocked = false;
            await Task.Run(() =>
            {
                Memberships user = db.Memberships.FirstOrDefault(x => x.UserId == id);
                if (user != null)
                {
                    user.IsLockedOut = false;
                    db.SaveChanges();
                    unlocked = true;
                }
            });
            return Json(unlocked);
        }
        [HttpPost]
        public async Task<JsonResult> ActivateAccount(string id)
        {
            var ok = true;
            try
            {
                await Task.Run(() =>
                {
                    Memberships user = db.Memberships.FirstOrDefault(x => x.Email == id);
                    if (user != null)
                    {
                        user.IsApproved = !user.IsApproved;
                        db.SaveChanges();
                    }
                });
            }
            catch
            {
                ok = false;
            }
            return Json(ok);
        }
        public Dictionary<string, string> GetAllowedRoles()
        {
            return User.IsInRole("AppAdmin") ? DataDictionaries.AllRoles :
            DataDictionaries.AllRoles.Where(x => x.Key != "AppAdmin").ToDictionary(x => x.Key, x => x.Value);
        }
    }
}