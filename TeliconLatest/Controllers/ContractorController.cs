using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Simple.ImageResizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;
using TeliconLatest.Reusables;

namespace TeliconLatest.Controllers
{
    [Authorize]
    public class ContractorController : Controller
    {
        private readonly TeliconDbContext db;
        private readonly IWebHostEnvironment _env;
        public ContractorController(TeliconDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            _env = env;
        }
        //
        // GET: /Contractor/
        //[TeliconAuthorize(TaskId = 4)]
        public ActionResult Index()
        {
            var minYear = db.TRN23100.Min(x => x.Requestdt).Year;
            var maxYear = db.TRN23100.Max(x => x.Requestdt).Year;
            var yearsForSelection = new List<SelectListItem>();
            for (int year = minYear; year <= maxYear; year++)
            {
                yearsForSelection.Add(new SelectListItem { Text = year.ToString(), Value = year.ToString(), Selected = year == maxYear });
            }
            ViewBag.Years = yearsForSelection;
            var statuses = DataDictionaries.UserTypes.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key.ToString()
            }).ToList();
            ViewBag.WoStatuses = DataDictionaries.WordOrderStatuses.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key
            }).ToList();
            statuses.Insert(0, new SelectListItem { Value = "a", Text = "All" });
            ViewBag.Statuses = statuses;
            return View();
        }
        //[TeliconAuthorize(TaskId = 4)]
        public ActionResult Create()
        {
            SetupContractorDddl(null);
            var model = new ADM03300();
            return View("CreateOrUpdate", model);
        }
        //[TeliconAuthorize(TaskId = 4)]
        public ActionResult Edit(int id)
        {
            var model = db.ADM03300.Find(id);
            SetupContractorDddl(model);
            return View("CreateOrUpdate", model);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 4, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(ADM03300 model, IFormCollection formCollection)
        {
            bool ok = false;
            bool uploaded = false;
            bool isNew = false;
            string url = "";
            var serverPath = Path.Combine(_env.WebRootPath, "images", "technicians");
            try
            {
                model.Email = model.Email.Trim();
                model.FirstName = model.FirstName != null ? model.FirstName.Trim() : model.FirstName;
                model.LastName = model.LastName != null ? model.LastName.Trim() : model.LastName;
                model.Phone1 = model.Phone1 != null ? model.Phone1.Trim() : model.Phone1;
                model.Phone2 = model.Phone2 != null ? model.Phone2.Trim() : model.Phone2;
                model.PlateNo = model.PlateNo != null ? model.PlateNo.Trim() : model.PlateNo;
                model.Street = model.Street != null ? model.Street.Trim() : model.Street;
                model.TechNo = model.TechNo != null ? model.TechNo.Trim() : model.TechNo;
                model.EmergencyCon = model.EmergencyCon != null ? model.EmergencyCon.Trim() : model.EmergencyCon;
                if (model.ConID == 0)
                {
                    isNew = true;
                    db.ADM03300.Add(model);
                    if (formCollection.Files.Count > 0 && !string.IsNullOrEmpty(formCollection.Files[0].FileName))
                    {
                        var file = formCollection.Files[0];
                        url = serverPath + model.EmployeeID + ".jpg";
                        long length = file.Length;
                        using var fileStream = file.OpenReadStream();
                        byte[] fileData = new byte[length];
                        fileStream.Read(fileData, 0, (int)file.Length);
                        ImageResizer resizer = new ImageResizer(fileData);
                        resizer.Resize(256, 256, ImageEncoding.Jpg100);
                        resizer.SaveToFile(url);
                    }
                    uploaded = true;
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();
                    var oldEmp = db.ADM03300.Find(model.ConID);
                    var oldID = oldEmp.EmployeeID;
                    if (oldID != model.EmployeeID)
                    {
                        var oldFile = serverPath + oldID + ".jpg";
                        if (System.IO.File.Exists(oldFile))
                            System.IO.File.Move(oldFile, serverPath + model.EmployeeID + ".jpg");
                    }
                    if (oldEmp.Email != model.Email)
                    {
                        Memberships users = db.Memberships.FirstOrDefault(p => p.Email == oldEmp.Email);
                        if (users != null)
                        {
                            users.Email = model.Email;
                            db.SaveChanges();
                        }
                    }
                }
                ok = true;
                return Json(new JsonReturnParams
                {
                    Additional = model.ConID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                ok = false;
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.ConID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
            finally
            {
                if (!ok && uploaded)
                    System.IO.File.Delete(url);

                Memberships memberships = db.Memberships.FirstOrDefault(p => p.Email == model.Email);
                if (ok && isNew && memberships == null)
                {
                    byte[] encbuff = System.Text.Encoding.UTF8.GetBytes("Password123.");
                    Users users = new Users
                    {
                        ApplicationId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Version).Value,
                        IsAnonymous = false,
                        LastActivityDate = DateTime.Now,
                        UserName = model.Email,
                    };
                    db.Users.Add(users);
                    db.SaveChanges();

                    Memberships memberships1 = new Memberships
                    {
                        ApplicationId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Version).Value,
                        UserId = users.UserId,
                        Email = model.Email,
                        Comment = "",
                        CreateDate = DateTime.Now,
                        LastPasswordChangedDate = DateTime.Now,
                        LastLoginDate = DateTime.Now,
                        LastLockoutDate = DateTime.Now,
                        IsLockedOut = false,
                        IsApproved = true,
                        Password = Convert.ToBase64String(encbuff)
                    };
                    db.Memberships.Add(memberships1);

                    UsersInRoles usersInRoles = new UsersInRoles
                    {
                        RoleId = db.Roles.FirstOrDefault(x => x.RoleName == "Technician").RoleId,
                        UserId = users.UserId
                    };
                    db.UsersInRoles.Add(usersInRoles);

                    byte[] bytes = Array.Empty<byte>();
                    ProfileInfo profileInfo = new ProfileInfo
                    {
                        AltPhone = model.EmerPhone1,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Phone = model.EmergencyCon
                    };
                    Profiles profiles = new Profiles
                    {
                        UserId = users.UserId,
                        LastUpdatedDate = DateTime.Now,
                        PropertyNames = "ProfileInfo:0:233:",
                        PropertyValueStrings = Utilities.GetProfileInfoValue(profileInfo),
                        PropertyValueBinary = bytes,
                    };
                    db.Profiles.Add(profiles);
                    db.SaveChanges();
                }
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 4, Mode = ActionMode.Write)]
        public JsonResult Delete(int id, string email)
        {
            try
            {
                if (!string.IsNullOrEmpty(email))
                    id = db.ADM03300.FirstOrDefault(x => x.Email == email).ConID;
                if (db.ADM03400.Any(x => x.ContractorID == id))
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
                    var user = db.ADM03300.Find(id);
                    db.ADM03300.Remove(user);
                    Memberships memberships = db.Memberships.FirstOrDefault(x => x.Email == user.Email);
                    if (memberships != null)
                    {
                        db.Memberships.Remove(memberships);
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
        public JsonResult GetBranchesForBank(string id)
        {
            var branches = db.ADM02200.Where(x => x.BankId == id).Select(x => new SelectListItem
            {
                Text = x.BranchName.ToUpper(),
                Value = x.BankId
            }).ToList();
            return Json(branches);
        }
        public void SetupContractorDddl(ADM03300 con)
        {
            ViewBag.UserTypes = DataDictionaries.UserTypes.Where(x => x.Key < 2).Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key.ToString()
            }).ToList();
            var parishes = DataDictionaries.Parishes.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key.ToString()
            }).ToList();
            var relatives = DataDictionaries.Relatives.Select(x => new SelectListItem
            {
                Text = x,
                Value = x
            }).ToList();
            var bnks = db.ADM02100.ToList();
            var fstBnk = bnks.FirstOrDefault(x => x.BankName.ToLower().Contains("global"));
            if (fstBnk != null)
            {
                bnks.Remove(fstBnk);
                bnks.Insert(0, fstBnk);
            }
            var banks = bnks.OrderBy(x => x.BankName).Select(x => new SelectListItem
            {
                Text = x.BankName.ToUpper(),
                Value = x.BankId,
                Selected = con != null && con.ADM02200 != null && con.ADM02200.ADM02100.BankId == x.BankId
            }).ToList();
            string bnkID = con != null && con.ADM02200 != null ? con.ADM02200.BankId : banks.FirstOrDefault().Value;
            var branches = db.ADM02200.Where(x => x.BankId == bnkID).OrderBy(x => x.BranchName).Select(x => new SelectListItem
            {
                Text = x.BranchName.ToUpper(),
                Value = x.RecID.ToString()
            }).ToList();
            var departments = db.ADM04200.OrderBy(x => x.Name).Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.DepartmentID.ToString()
            }).ToList();
            var payScales = new List<SelectListItem>();
            payScales.Insert(0, new SelectListItem { Text = "Snr", Value = "1" });
            payScales.Insert(1, new SelectListItem { Text = "Jr", Value = "2" });

            banks.Insert(0, new SelectListItem { Text = "", Value = "" });
            branches.Insert(0, new SelectListItem { Text = "", Value = "" });
            parishes.Insert(0, new SelectListItem { Text = "", Value = "" });
            relatives.Insert(0, new SelectListItem { Text = "", Value = "" });
            departments.Insert(0, new SelectListItem { Text = "", Value = "" });
            ViewBag.Branches = branches;
            ViewBag.Parishes = parishes;
            ViewBag.Relatives = relatives;
            ViewBag.Banks = banks;
            ViewBag.Departments = departments;
            ViewBag.PayScales = payScales;
        }
        [HttpPost]
        public JsonResult EmployeeIDExists(string employeeID, string initialID)
        {
            if (employeeID != initialID || string.IsNullOrEmpty(initialID))
            {
                var empExists = db.ADM03300.Where(x => x.EmployeeID.ToUpper() == employeeID.ToUpper()).Any();
                return Json(!empExists);
            }
            return Json(true);
        }
        public ActionResult GetContractorWorkOrders(int id)
        {
            var stats = Customs.GetTechStats(id, "");
            ViewBag.ContractorID = id;
            return View("ContractorWorkOrders", stats);
        }
        public ActionResult RefreshConStats(int id)
        {
            var stats = Customs.GetTechStats(id, "");
            ViewBag.ContractorID = id;
            return View("TechnicianStats", stats);
        }

        public ActionResult RefreshTechnicianState(int id)
        {
            var stats = Customs.GetTechStats(id, "");
            ViewBag.ContractorID = id;
            return PartialView("~/Views/Shared/TechnicianStats.cshtml", stats);
        }

        [HttpPost]
        public JsonResult GetBranches(string bankId)
        {
            var bank = db.ADM02100.Find(bankId);
            var branches = new List<SelectListItem>();
            if (bank != null)
            {
                string bnkID = bank.BankId;
                branches = db.ADM02200.Where(x => x.BankId == bnkID).OrderBy(x => x.BranchName).Select(x => new SelectListItem
                {
                    Text = x.BranchName.ToUpper(),
                    Value = x.RecID.ToString()
                }).ToList();
                branches.Insert(0, new SelectListItem { Text = "", Value = "" });
            }
            return Json(new
            {
                Branches = branches
            });
        }
    }
}