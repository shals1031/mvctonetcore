using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    public class MaterialController : Controller
    {
        private readonly TeliconDbContext db;
        public MaterialController(TeliconDbContext db)
        {
            this.db = db;
        }
        //
        // GET: /Material/
        //[TeliconAuthorize(TaskId = 6)]
        public ActionResult Index()
        {
            var statuses = DataDictionaries.Units.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key.ToString()
            }).ToList();
            statuses.Insert(0, new SelectListItem { Value = "a", Text = "All" });
            ViewBag.Status = statuses;
            return View();
        }
        //[TeliconAuthorize(TaskId = 6)]
        public ActionResult Create()
        {
            ViewBag.Units = DataDictionaries.Units.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key
            }).ToList();
            return View("CreateOrUpdate", new ADM13100());
        }
        //[TeliconAuthorize(TaskId = 6)]
        public ActionResult Edit(int id)
        {
            ViewBag.Units = DataDictionaries.Units.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key
            }).ToList();
            return View("CreateOrUpdate", db.ADM13100.Find(id));
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 6, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(ADM13100 model)
        {
            try
            {
                if (model.MaterialID == 0)
                    db.ADM13100.Add(model);
                else
                    db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturnParams
                {
                    Additional = model.MaterialID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.MaterialID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 6, Mode = ActionMode.Write)]
        public JsonResult Delete(int id)
        {
            try
            {
                db.ADM13100.Remove(db.ADM13100.Find(id));
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