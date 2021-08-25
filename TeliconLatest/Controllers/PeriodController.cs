using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    public class PeriodController : Controller
    {
        private readonly TeliconDbContext db;
        public PeriodController(TeliconDbContext db)
        {
            this.db = db;
        }

        //[TeliconAuthorize(TaskId = 10)]
        public ActionResult Index()
        {
            var periodyear = db.ADM16100.Select(x => new SelectListItem
            {
                Text = x.periodYear.ToString(),
                Value = x.periodYear.ToString()
            }).Distinct().ToList();
            periodyear.Insert(0, new SelectListItem { Value = "", Text = "All" });
            ViewBag.PeriodYear = periodyear;
            return View();
        }

        //[TeliconAuthorize(TaskId = 10)]
        public ActionResult Create()
        {
            return View("CreateOrUpdate", new ADM16100());
        }

        //[TeliconAuthorize(TaskId = 10)]
        public ActionResult Edit(int id)
        {
            return View("CreateOrUpdate", db.ADM16100.Find(id));
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 10, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(ADM16100 model)
        {
            try
            {
                if (model.PeriodID == 0)
                    db.ADM16100.Add(model);
                else
                    db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturnParams
                {
                    Additional = model.PeriodID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.PeriodID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }

        [HttpPost]
        //[TeliconAuthorize(TaskId = 10, Mode = ActionMode.Write)]
        public JsonResult Delete(int id)
        {
            try
            {
                db.ADM16100.Remove(db.ADM16100.Find(id));
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