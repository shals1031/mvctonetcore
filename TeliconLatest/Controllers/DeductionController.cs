using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    [Authorize]
    public class DeductionController : Controller
    {
        private readonly TeliconDbContext db;
        public DeductionController(TeliconDbContext db)
        {
            this.db = db;
        }
        // GET: Deduction
        //[TeliconAuthorize(TaskId = 9)]
        public IActionResult Index()
        {
            return View();
        }
        //[TeliconAuthorize(TaskId = 9)]
        public IActionResult Create()
        {
            return View("CreateOrUpdate", new ADM04100());
        }
        //[TeliconAuthorize(TaskId = 9)]
        public ActionResult Edit(int id)
        {
            return View("CreateOrUpdate", db.ADM04100.Find(id));
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 9, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(ADM04100 model)
        {
            try
            {
                if (model.DeductionID == 0)
                    db.ADM04100.Add(model);
                else
                    db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturnParams
                {
                    Additional = model.DeductionID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.DeductionID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }

        [HttpPost]
        //[TeliconAuthorize(TaskId = 9, Mode = ActionMode.Write)]
        public JsonResult Delete(int id)
        {
            try
            {
                if (db.TRN04100.Any(x => x.DeductionID == id))
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
                    db.ADM04100.Remove(db.ADM04100.Find(id));
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