using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;
using TeliconLatest.Reusables;

namespace TeliconLatest.Controllers
{
    public class ContractorDeductionController : Controller
    {
        private readonly TeliconDbContext db;
        public ContractorDeductionController(TeliconDbContext db)
        {
            this.db = db;
        }
        // GET: ContractorDeduction
        //[TeliconAuthorize(TaskId = 4)]
        public IActionResult Index(int p_ConId)
        {
            HttpContext.Session.SetInt32("p_ConId", p_ConId);
            ADM03300 _ADM03300 = db.ADM03300.Where(t => t.ConID == p_ConId).FirstOrDefault();
            if (_ADM03300 != null)
                ViewBag.ContName = _ADM03300.FirstName + " " + _ADM03300.LastName;

            return View();
        }
        //[TeliconAuthorize(TaskId = 4)]
        public IActionResult Create()
        {
            var model = new TRN04100();
            int ConductorID = Convert.ToInt32(HttpContext.Session.GetInt32("p_ConId"));
            ViewBag.TechStats = Customs.GetTechStats(ConductorID, User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email).Value);

            var deductions = db.ADM04100.Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.DeductionID.ToString()
            }).ToList();
            deductions.Insert(0, new SelectListItem { Text = "", Value = "" });
            ViewBag.deductions = deductions;
            model.StartDate = DateTime.Now;
            return View("CreateOrUpdate", model);
        }
        //[TeliconAuthorize(TaskId = 4)]
        public IActionResult Edit(int id)
        {
            int ConductorID = Convert.ToInt32(HttpContext.Session.GetInt32("p_ConId"));
            ViewBag.TechStats = Reusables.Customs.GetTechStats(ConductorID, User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email).Value);
            TRN04100 _TRN04100 = db.TRN04100.Find(id);
            var deductions = db.ADM04100.Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.DeductionID.ToString(),
                Selected = _TRN04100.DeductionID == x.DeductionID ? true : false
            }).ToList();
            deductions.Insert(0, new SelectListItem { Text = "", Value = "" });
            ViewBag.deductions = deductions;
            return View("CreateOrUpdate", _TRN04100);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 4, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(TRN04100 model)
        {
            try
            {
                if (model.DeductionConductorID == 0)
                {
                    model.ConductorID = Convert.ToInt32(HttpContext.Session.GetInt32("p_ConId"));
                    DateTime dStart = DateTime.ParseExact(Request.Form["StartDate"].ToString(), "MM/dd/yyyy", new System.Globalization.CultureInfo("en-GB"));
                    DateTime dEnd = DateTime.ParseExact(Request.Form["EndDate"].ToString(), "MM/dd/yyyy", new System.Globalization.CultureInfo("en-GB"));
                    model.StartDate = dStart;
                    model.EndDate = dEnd;
                    db.TRN04100.Add(model);
                }
                else
                {
                    TRN04100 _TRN04100 = db.TRN04100.Find(model.DeductionConductorID);
                    _TRN04100.ConductorID = Convert.ToInt32(HttpContext.Session.GetInt32("p_ConId"));
                    DateTime dStart = DateTime.ParseExact(Request.Form["StartDate"].ToString(), "MM/dd/yyyy", new System.Globalization.CultureInfo("en-GB"));
                    DateTime dEnd = DateTime.ParseExact(Request.Form["EndDate"].ToString(), "MM/dd/yyyy", new System.Globalization.CultureInfo("en-GB"));
                    _TRN04100.StartDate = dStart;
                    _TRN04100.EndDate = dEnd;
                    _TRN04100.DeductionID = model.DeductionID;
                    _TRN04100.Amount = model.Amount;
                    _TRN04100.HoldIt = model.HoldIt;
                    //_TRN04100.Periods = model.Periods;
                    _TRN04100.Recurring = model.Recurring;
                    _TRN04100.YTDAmount = model.YTDAmount;
                    db.Entry(_TRN04100).State = EntityState.Modified;
                }
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
        //[TeliconAuthorize(TaskId = 4, Mode = ActionMode.Write)]
        public JsonResult Delete(int id)
        {
            try
            {
                db.TRN04100.Remove(db.TRN04100.Find(id));
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