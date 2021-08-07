using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    public class AreaController : Controller
    {
        private readonly TeliconDbContext db;
        public AreaController(TeliconDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            ADM01400 model = new ADM01400
            {
                areaID = 0
            };
            ViewBag.Zones = db.ADM26100.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.ZoneID.ToString()
            }).ToList();
            return View("CreateOrUpdate", model);
        }
        public IActionResult Edit(int id)
        {
            ViewBag.Zones = db.ADM26100.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.ZoneID.ToString()
            }).ToList();
            return View("CreateOrUpdate", db.ADM01400.Find(id));
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 13, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(ADM01400 model)
        {
            try
            {
                if (model.areaID == 0)
                    db.ADM01400.Add(model);
                else
                    db.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturnParams
                {
                    Additional = model.areaID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.areaID == -1 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }

        [HttpPost]
        //[TeliconAuthorize(TaskId = 13, Mode = ActionMode.Write)]
        public JsonResult Delete(int id)
        {
            try
            {
                db.ADM01400.Remove(db.ADM01400.Find(id));
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