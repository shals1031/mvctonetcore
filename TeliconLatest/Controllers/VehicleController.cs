using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    public class VehicleController : Controller
    {
        private readonly TeliconDbContext db;
        public VehicleController(TeliconDbContext db)
        {
            this.db = db;
        }
        // GET: Vehicle
        //[TeliconAuthorize(TaskId = 11)]
        public ActionResult Index()
        {
            return View();
        }
        //[TeliconAuthorize(TaskId = 11)]
        public ActionResult Create()
        {
            var model = new ADM22100();
            var contractors = db.ADM03300.Where(x => x.ConClass != 2 && x.ConClass != 3).Select(x => new SelectListItem
            {
                Text = x.FirstName + " " + x.LastName,
                Value = x.ConID.ToString()
            }).ToList();
            string conID = contractors.FirstOrDefault().Value;
            ViewBag.Contractors = contractors;
            ViewBag.OwnerTypes = DataDictionaries.OwnerTypes.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key.ToString()
            }).ToList();

            return View("CreateOrUpdate", model);
        }
        //[TeliconAuthorize(TaskId = 11)]
        public ActionResult Edit(int id)
        {
            var contractors = db.ADM03300.Where(x => x.ConClass != 2 && x.ConClass != 3).Select(x => new SelectListItem
            {
                Text = x.FirstName + " " + x.LastName,
                Value = x.ConID.ToString()
            }).ToList();
            string conID = contractors.FirstOrDefault().Value;
            ViewBag.Contractors = contractors;
            ViewBag.OwnerTypes = DataDictionaries.OwnerTypes.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key.ToString()
            }).ToList();
            return View("CreateOrUpdate", db.ADM22100.Find(id));
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 11, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(ADM22100 model)
        {
            try
            {
                model.FleetNo = model.FleetNo.Trim();
                model.PlateNo = model.PlateNo.Trim();
                if (model.VehicleID == 0)
                    db.ADM22100.Add(model);
                else
                    db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new JsonReturnParams
                {
                    Additional = model.VehicleID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.VehicleID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }

        [HttpPost]
        //[TeliconAuthorize(TaskId = 11, Mode = ActionMode.Write)]
        public JsonResult Delete(int id)
        {
            try
            {
                db.ADM22100.Remove(db.ADM22100.Find(id));
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