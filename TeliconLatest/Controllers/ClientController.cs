using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        private readonly TeliconDbContext db;
        public ClientController(TeliconDbContext db)
        {
            this.db = db;
        }
        //
        // GET: /Client/
        //[TeliconAuthorize(TaskId = 3)]
        public ActionResult Index()
        {
            var statuses = DataDictionaries.ClientClass.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key.ToString()
            }).ToList();
            statuses.Insert(0, new SelectListItem { Value = "a", Text = "All" });
            ViewBag.Statuses = statuses;
            return View();
        }
        //[TeliconAuthorize(TaskId = 3)]
        public ActionResult Create()
        {
            SetupClients();
            var model = new ADM03200Extended();            
            return View("CreateOrUpdate", model);
        }
        //[TeliconAuthorize(TaskId = 3)]
        public ActionResult Edit(int id)
        {
            SetupClients();
            var data = new ADM03200Extended(db.ADM03200.Find(id));
            return View("CreateOrUpdate", data);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 3, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(ADM03200Extended modelEx)
        {
            db.Database.OpenConnection();
            var model = modelEx.GetBase();
            using TransactionScope ts = new TransactionScope();
            try
            {
                bool isNew = false;
                if (model.CustID == 0)
                {
                    isNew = true;
                    db.ADM03200.Add(model);
                }
                else
                    db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                if (isNew && modelEx.CopyActClientId.HasValue)
                    CopyActivities(model.CustID, modelEx.CopyActClientId ?? 0);
                ts.Complete();
                return Json(new JsonReturnParams
                {
                    Additional = model.CustID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.CustID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 3, Mode = ActionMode.Write)]
        public JsonResult Delete(int id)
        {
            try
            {
                if (db.TRN23100.Any(x => x.Wo_client == id))
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
                    db.ADM03200.Remove(db.ADM03200.Find(id));
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
        public void SetupClients()
        {
            ViewBag.Classes = DataDictionaries.ClientClass.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key.ToString()
            }).ToList();
            ViewBag.Parishes = DataDictionaries.Parishes.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key
            }).ToList();
            ViewBag.Currencies = new List<SelectListItem>
            {
                new SelectListItem{ Text = "US Dollars", Value ="USD"},
                new SelectListItem{ Text = "Jamaican Dollars", Value ="JMD"}
            };
            ViewBag.Clients = db.ADM03200.Select(x => new SelectListItem
            {
                Text = x.ClientCode.ToString() + " - " + x.CustName.ToString(),
                Value = x.CustID.ToString()
            }).ToList();
        }
        [HttpPost]
        public JsonResult ClientIDExists(string ClientCode, string initialID)
        {
            if (ClientCode != initialID || string.IsNullOrEmpty(initialID))
            {
                var clientExists = db.ADM03200.Where(x => x.ClientCode.ToUpper() == ClientCode.ToUpper()).Any();
                return Json(!clientExists);
            }
            return Json(true);
        }

        private void CopyActivities(int clientId, int actClientId)
        {
            try
            {
                var date = DateTime.Now;
                var model = db.ADM01100.Where(x => x.ClientID == actClientId).ToList();
                foreach (var x in model)
                {
                    var act = new ADM01100
                    {
                        ClientID = clientId,
                        DepartmentId = x.DepartmentId,
                        RateClass = x.RateClass,
                        RateDescr = x.RateDescr,
                        RateUnit = x.RateUnit,
                        StartDate = date,
                        Active = x.Active,
                        MaxQty = x.MaxQty,
                        HasMaterials = x.HasMaterials,
                        MaterialsRequired = x.MaterialsRequired
                    };
                    db.ADM01100.Add(act);
                    db.SaveChanges();
                    //Assign materials
                    var mats = db.ADM01110.Where(m => m.ActivID == x.RateID);
                    foreach (var mat in mats)
                    {
                        db.ADM01110.Add(new ADM01110
                        {
                            ActivID = act.RateID,
                            MaterID = mat.MaterID,
                            ActMatQty = mat.ActMatQty
                        });
                    }
                    //Get payment history
                    var pHis = db.ADM01250.Where(r => r.RateID == x.RateID).OrderByDescending(r => r.RateHistoryID).FirstOrDefault();
                    //Copy rates
                    if (pHis != null)
                        db.ADM01250.Add(new ADM01250
                        {
                            RateID = act.RateID,
                            StartDate = new DateTime(2017, 1, 1),
                            EndDate = new DateTime(2025, 12, 31),
                            Amount = pHis.Amount
                        });
                    //Get client history
                    var cHis = db.ADM01150.Where(r => r.RateID == x.RateID).OrderByDescending(r => r.RateHistoryID).FirstOrDefault();
                    //Copy rates
                    if (cHis != null)
                        db.ADM01150.Add(new ADM01150
                        {
                            RateID = act.RateID,
                            StartDate = new DateTime(2017, 1, 1),
                            EndDate = new DateTime(2025, 12, 31),
                            Amount = cHis.Amount
                        });
                }
                db.SaveChanges();
            }
            catch (Exception e)
            {

            }
        }
    }
}