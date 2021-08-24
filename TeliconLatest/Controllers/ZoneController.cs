using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    public class ZoneController : Controller
    {
        private readonly TeliconDbContext db;
        private readonly IWebHostEnvironment _env;
        public ZoneController(TeliconDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            _env = env;
        }

        // GET: Zone
        //[TeliconAuthorize(TaskId = 12)]
        public ActionResult Index()
        {
            return View();
        }

        //[TeliconAuthorize(TaskId = 12)]
        public ActionResult Create()
        {
            var model = new ADM26100();
            model.ZoneID = -1;
            return View("CreateOrUpdate", model);
        }

        //[TeliconAuthorize(TaskId = 12)]
        public ActionResult Edit(int id)
        {
            return View("CreateOrUpdate", db.ADM26100.Find(id));
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 12, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(ADM26100 model)
        {
            try
            {
                if (model.ZoneID == -1)
                    db.ADM26100.Add(model);
                else
                    db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturnParams
                {
                    Additional = model.ZoneID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.ZoneID == -1 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }

        [HttpPost]
        //[TeliconAuthorize(TaskId = 12, Mode = ActionMode.Write)]
        public JsonResult Delete(int id)
        {
            try
            {
                db.ADM26100.Remove(db.ADM26100.Find(id));
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
