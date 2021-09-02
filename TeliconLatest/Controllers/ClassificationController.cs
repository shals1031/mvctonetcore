using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    [Authorize]
    public class ClassificationController : Controller
    {
        private readonly TeliconDbContext db;
        public ClassificationController(TeliconDbContext db)
        {
            this.db = db;
        }
        //
        // GET: /Classification/
        //[TeliconAuthorize(TaskId = 7)]
        public ActionResult Index()
        {
            return View();
        }
        //[TeliconAuthorize(TaskId = 7)]
        public ActionResult Create()
        {
            ADM03500 model = new ADM03500
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now
            };
            return View("CreateOrUpdate", model);
        }
        //[TeliconAuthorize(TaskId = 7)]
        public ActionResult Edit(int id)
        {
            return View("CreateOrUpdate", db.ADM03500.Find(id));
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 7, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(ADM03500 model)
        {
            try
            {
                if (model.ClassId == 0)
                    db.ADM03500.Add(model);
                else
                    db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturnParams
                {
                    Additional = model.ClassId,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.ClassId == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 7, Mode = ActionMode.Write)]
        public JsonResult Delete(int id)
        {
            try
            {
                if (db.TRN23100.Any(x => x.ClassId == id) || db.ADM01100.Any(x => x.RateClass == id))
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
                    db.ADM03500.Remove(db.ADM03500.Find(id));
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
    }
}