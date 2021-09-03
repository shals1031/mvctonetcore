using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;
using TeliconLatest.Reusables;

namespace TeliconLatest.Controllers
{
    public class RoleController : Controller
    {
        private readonly TeliconDbContext db;
        public RoleController(TeliconDbContext db)
        {
            this.db = db;
        }
        // GET: Role        
        //[TeliconAuthorize(TaskId = 1)]
        public ActionResult Index()
        {
            return View();
        }
        //[TeliconAuthorize(TaskId = 1)]
        public ActionResult Create()
        {
            return View("CreateOrUpdate", new Roles());
        }
        //[TeliconAuthorize(TaskId = 1)]
        public ActionResult Edit(string id)
        {
            List<Roles> roles = db.Roles.ToList();
            Roles data = roles.FirstOrDefault(r => r.RoleName == id);
            return View("CreateOrUpdate", data);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 1, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(Roles model)
        {
            try
            {
                bool isNew = false;
                if (string.IsNullOrEmpty(model.RoleId))
                {
                    if (db.Roles.Any(t => t.RoleName.ToLower().Trim() == model.RoleName.ToLower().Trim()))
                    {
                        return Json(new JsonReturnParams
                        {
                            Additional = string.Empty,
                            Code = string.IsNullOrEmpty(model.RoleId) ? "200" : "300",
                            Msg = model.RoleName + " Already Exists!"
                        });
                    }
                    else
                    {
                        model.ApplicationId = "08ef351a-8128-43c2-b257-4d20ef383169";
                        model.RoleId = Guid.NewGuid().ToString();
                        db.Roles.Add(model);
                    }
                }
                else
                    db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturnParams
                {
                    Additional = model.RoleId,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = string.IsNullOrEmpty(model.RoleId) ? "200" : "300",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 1, Mode = ActionMode.Write)]
        public JsonResult Delete(string id)
        {
            try
            {
                if (db.UsersInRoles.Where(x => x.RoleId == id).Count() > 0)
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
                    db.Roles.Remove(db.Roles.FirstOrDefault(x => x.RoleId == id));
                    db.SaveChanges();
                    return Json(new JsonReturnParams
                    {
                        Additional = 1,
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

        public JsonResult RoleList(DataTablesParam model)
        {
            string order = Customs.GetSortString(model.order, model.columns);
            var data = db.Roles.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
            return Json(new DataTableReturn
            {
                draw = model.draw,
                recordsFiltered = data.Count(),
                recordsTotal = db.Roles.Count(),
                data = data.OrderBy(x => x.RoleName).Skip(model.start).Take(model.length).ToList().Select(x => new
                {
                    DT_RowId = x.RoleName,
                    Name = x.RoleName
                }).AsQueryable().ToStringArray()
            });
        }

        public ActionResult MergeCandiditates(string code)
        {
            var candidates = db.TRN23100.Where(x => x.Wo_ref == code && (x.Status != "i" && !x.TRN13110.Any() && !x.TRN13120.Any())).AsEnumerable().Select(x => new WorkOrderPartial
            {
                WOId = x.Workid,
                Title = x.Wo_title,
                Status = DataDictionaries.WordOrderStatuses[x.Status],
                Total = x.TRN23110.Sum(y => Convert.ToDouble(y.OActQty) * y.ADM01100.GetRateAmountForDate(y.ActDate)),
                IsMerged = db.TRN13120.FirstOrDefault(m => m.WorkOrderId == x.Workid) != null || db.TRN13110.FirstOrDefault(m => m.WorkOrderId == x.Workid) != null
            }).ToList();
            return View(candidates);
        }

        public ActionResult RoleTasks(string id)
        {
            var role = db.Roles.FirstOrDefault(x => x.RoleName == id);
            string roleId = role != null ? role.RoleId : "";
            var tasks = new List<RoleTaskList>();
            if (!string.IsNullOrEmpty(roleId))
                tasks = DataDictionaries.Tasks.Select(x => new RoleTaskList
                {
                    TaskId = x.Key,
                    TaskName = x.Value,
                    CanRead = db.TasksInRoles.Any(r => r.RoleId == roleId && r.TaskId == x.Key) && db.TasksInRoles.FirstOrDefault(r => r.RoleId == roleId && r.TaskId == x.Key).CanRead,
                    CanWrite = db.TasksInRoles.Any(r => r.RoleId == roleId && r.TaskId == x.Key) && db.TasksInRoles.FirstOrDefault(r => r.RoleId == roleId && r.TaskId == x.Key).CanWrite,
                    IsSelected = db.TasksInRoles.Any(r => r.RoleId == roleId && r.TaskId == x.Key)
                }).ToList();
            ViewBag.roleId = roleId;
            return View(tasks);
        }

        [HttpPost]
        public JsonResult SaveRoleTasks(List<RoleTaskList> model, Guid roleId)
        {
            db.Database.OpenConnection();
            using TransactionScope ts = new TransactionScope();
            try
            {
                db.TasksInRoles.RemoveRange(db.TasksInRoles.Where(x => x.RoleId == roleId.ToString()));
                db.SaveChanges();
                var list = new List<TasksInRoles>();
                foreach (var item in model)
                {
                    list.Add(new TasksInRoles
                    {
                        RoleId = roleId.ToString(),
                        TaskId = item.TaskId,
                        CanRead = item.CanRead,
                        CanWrite = item.CanWrite
                    });
                }
                db.TasksInRoles.AddRange(list);
                db.SaveChanges();
                ts.Complete();
                ts.Dispose();
                return Json(new JsonReturnParams
                {
                    Additional = roleId,
                    Code = "100",
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
    }
}