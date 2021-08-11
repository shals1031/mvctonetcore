using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    public class BankController : Controller
    {
        private readonly TeliconDbContext db;
        public BankController(TeliconDbContext db)
        {
            this.db = db;
        }
        // GET: Bank
        //[TeliconAuthorize(TaskId = 37)]
        public IActionResult Index()
        {
            return View();
        }
        //[TeliconAuthorize(TaskId = 37)]
        public IActionResult Create()
        {
            var model = new ADM02100Extended();
            return View("CreateOrUpdate", model);
        }
        //[TeliconAuthorize(TaskId = 37)]
        public ActionResult Edit(string id)
        {
            return View("CreateOrUpdate", new ADM02100Extended(db.ADM02100.Find(id)));
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 37, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(ADM02100Extended modelEx)
        {
            try
            {
                if (!string.IsNullOrEmpty(modelEx.OldBankId) && modelEx.OldBankId != modelEx.BankId)
                    return Json(new JsonReturnParams
                    {
                        Additional = "Create/Update failed. You cannot change existing Bank Code.",
                        Code = "999",
                        Msg = ""
                    });
                var model = modelEx.GetBase();
                if (!db.ADM02100.Any(x => x.BankId == model.BankId))
                    db.ADM02100.Add(model);
                else
                    db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturnParams
                {
                    Additional = model.BankId,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = modelEx.OldBankId == null ? "200" : "300",
                    Msg = e.Message
                });
            }
        }

        [HttpPost]
        //[TeliconAuthorize(TaskId = 37, Mode = ActionMode.Write)]
        public JsonResult Delete(string id)
        {
            try
            {
                db.ADM02100.Remove(db.ADM02100.Find(id));
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