using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    [Authorize]
    public class DepartmentController : Controller
    {
        private readonly TeliconDbContext db;
        public DepartmentController(TeliconDbContext db)
        {
            this.db = db;
        }
        // GET: Department
        //[TeliconAuthorize(TaskId = 8)]
        public ActionResult Index()
        {
            return View();
        }
        //[TeliconAuthorize(TaskId = 8)]
        public ActionResult Create()
        {
            return View("CreateOrUpdate", new ADM04200());
        }
        //[TeliconAuthorize(TaskId = 8)]
        public ActionResult Edit(int id)
        {
            return View("CreateOrUpdate", db.ADM04200.Find(id));
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 8, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(ADM04200 model)
        {
            try
            {
                if (model.DepartmentID == 0)
                    db.ADM04200.Add(model);
                else
                    db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturnParams
                {
                    Additional = model.DepartmentID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.DepartmentID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }

        [HttpPost]
        //[TeliconAuthorize(TaskId = 8, Mode = ActionMode.Write)]
        public JsonResult Delete(int id)
        {
            try
            {
                if (db.ADM03300.Any(x => x.DepartmentID == id))
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
                    db.ADM04200.Remove(db.ADM04200.Find(id));
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


        public ActionResult DepartmentActivities(int id)
        {
            var acts = new List<DepartmentActivityList>();
            if (id >= 0)
            {
                acts = db.ADM01100.Join(db.ADM03200, x => x.ClientID, y => y.CustID, (x, y) => new DepartmentActivityList
                {
                    ActId = x.RateID,
                    ActName = x.RateDescr,
                    Client = y.ClientCode
                }).ToList();
                foreach (var x in acts)
                {
                    var depAct = db.ADM04210.FirstOrDefault(r => r.DepartmentID == id && r.ActivityID == x.ActId);
                    x.DepActId = depAct != null ? depAct.DepActID : 0;
                    x.IsSelected = depAct != null;
                }
            }
            ViewBag.depId = id;
            acts = acts.OrderBy(x => x.Client).ThenBy(x => x.ActName).ToList();
            return View(acts);
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 8, Mode = ActionMode.Write)]
        public JsonResult SaveDepActivities(List<DepartmentActivityList> actList, int depId)
        {
            db.Database.OpenConnection();
            using TransactionScope ts = new TransactionScope();
            try
            {
                var actIds = actList.Select(x => x.ActId).ToList();
                db.ADM04210.RemoveRange(db.ADM04210.Where(x => x.DepartmentID == depId && !actIds.Contains(x.ActivityID)));
                db.SaveChanges();
                var existIds = actList.Where(x => x.DepActId > 0).Select(x => x.DepActId);
                foreach (var dbItem in db.ADM04210.Where(x => existIds.Contains(x.DepActID)))
                {
                    var item = actList.Find(x => x.DepActId == dbItem.DepActID);
                }
                var list = new List<ADM04210>();
                foreach (var item in actList.Where(x => x.DepActId <= 0))
                {
                    list.Add(new ADM04210
                    {
                        DepartmentID = depId,
                        ActivityID = item.ActId
                    });
                }
                db.ADM04210.AddRange(list);
                db.SaveChanges();
                ts.Complete();
                ts.Dispose();
                return Json(new JsonReturnParams
                {
                    Additional = depId,
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
        [HttpPost]
        //[TeliconAuthorize(TaskId = 8, Mode = ActionMode.Write)]
        public JsonResult DepartmentAllowedForActivities(int depId)
        {
            var hasMat = db.ADM04210.Any(x => x.DepartmentID == depId);
            return Json(new JsonReturnParams
            {
                Additional = hasMat,
                Code = "300",
                Msg = ""
            });
        }
    }
}