using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    public class POController : Controller
    {
        private readonly TeliconDbContext db;
        public POController(TeliconDbContext db)
        {
            this.db = db;
        }
        // GET: PO
        //[TeliconAuthorize(TaskId = 14)]
        public ActionResult Index()
        {
            return View();
        }
        //[TeliconAuthorize(TaskId = 14)]
        public ActionResult Create()
        {
            return View("CreateOrUpdate", new ADM16200());
        }
        //[TeliconAuthorize(TaskId = 14)]
        public ActionResult Edit(int id)
        {
            return View("CreateOrUpdate", db.ADM16200.Find(id));
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 14, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(ADM16200 model)
        {
            try
            {
                if (model.POID == 0)
                {
                    if (model.BALANCE == 0 || model.BALANCE == null)
                        model.BALANCE = model.TOTAL;
                    db.ADM16200.Add(model);
                }
                else
                {
                    model.BALANCE = model.TOTAL;
                    db.Entry(model).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Json(new JsonReturnParams
                {
                    Additional = model.POID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.POID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }

        [HttpPost]
        //[TeliconAuthorize(TaskId = 14, Mode = ActionMode.Write)]
        public JsonResult Delete(int id)
        {
            try
            {
                db.ADM16200.Remove(db.ADM16200.Find(id));
                return Json(new JsonReturnParams
                {
                    Additional = db.SaveChanges(),
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
    }
}