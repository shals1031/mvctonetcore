using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    public class BranchController : Controller
    {
        private readonly TeliconDbContext db;
        public BranchController(TeliconDbContext db)
        {
            this.db = db;
        }
        // GET: Branch
        //[TeliconAuthorize(TaskId = 38)]
        public ActionResult Index()
        {
            return View();
        }

        //[TeliconAuthorize(TaskId = 38)]
        public ActionResult Create()
        {
            var model = new ADM02200();
            var bnks = db.ADM02100.ToList();
            var fstBnk = db.ADM02100.Find("99");
            bnks.Remove(bnks.Find(x => x.BankId == "99"));
            bnks.Insert(0, fstBnk);
            ViewBag.Banks = bnks.Select(x => new SelectListItem
            {
                Text = x.BankName,
                Value = x.BankId
            }).ToList();
            return View("CreateOrUpdate", model);
        }
        //[TeliconAuthorize(TaskId = 38)]
        public ActionResult Edit(int id)
        {
            ViewBag.Banks = db.ADM02100.Select(x => new SelectListItem
            {
                Text = x.BankName,
                Value = x.BankId
            }).ToList();
            return View("CreateOrUpdate", db.ADM02200.Find(id));
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 38, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(ADM02200 model)
        {
            try
            {
                if (model.RecID == 0)
                    db.ADM02200.Add(model);
                else
                    db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturnParams
                {
                    Additional = model.RecID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.RecID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }

        [HttpPost]
        //[TeliconAuthorize(TaskId = 38, Mode = ActionMode.Write)]
        public JsonResult Delete(int id)
        {
            try
            {
                db.ADM02200.Remove(db.ADM02200.Find(id));
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