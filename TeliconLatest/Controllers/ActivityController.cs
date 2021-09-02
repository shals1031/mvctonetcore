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
    public class ActivityController : Controller
    {
        private readonly TeliconDbContext db;
        public ActivityController(TeliconDbContext db)
        {
            this.db = db;
        }
        //
        // GET: /Activities/
        //[TeliconAuthorize(TaskId = 5)]
        public IActionResult Index()
        {
            var clnts = db.ADM03200.Where(x => x.StartDt <= DateTime.Now && x.EndDt >= DateTime.Now).Take(10).Select(x => new SelectListItem
            {
                Text = x.CustName,
                Value = x.CustID.ToString()
            }).ToList();
            clnts.Insert(0, new SelectListItem { Value = "0", Text = "All" });
            ViewBag.Clients = clnts;

            var deps = db.ADM04200.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.DepartmentID.ToString()
            }).ToList();
            deps.Insert(0, new SelectListItem { Value = "a", Text = "All" });
            ViewBag.Departments = deps;
            return View();
        }
        //[TeliconAuthorize(TaskId = 5)]
        public IActionResult Create()
        {
            SetupActivityList();
            var model = new CustomActivityModel
            {
                RateID = 0,
                Active = false,
                StartDate = DateTime.Now,
                PFStartDate = DateTime.Now,
                PFEndDate = DateTime.Now,
                CBStartDate = DateTime.Now,
                CBEndDate = DateTime.Now
            };
            return View("CreateOrUpdate", model);
        }
        //[TeliconAuthorize(TaskId = 5)]
        public ActionResult Edit(int id)
        {
            SetupActivityList();

            #region Get Data from ADM01100, tblClientBill, tblPaymentFrom & pass it to view.
            var ADN011_Data = db.ADM01100.Find(id);

            //Assign value to model variables & pass to view.
            CustomActivityModel model = new CustomActivityModel();
            model.Active = !string.IsNullOrEmpty(Convert.ToString(ADN011_Data.Active)) && ADN011_Data.Active;
            model.ClientID = !string.IsNullOrEmpty(Convert.ToString(ADN011_Data.ClientID)) ? ADN011_Data.ClientID : 0;
            model.DepartmentID = !string.IsNullOrEmpty(Convert.ToString(ADN011_Data.DepartmentId)) ? ADN011_Data.DepartmentId : 0;
            model.RateID = !string.IsNullOrEmpty(Convert.ToString(ADN011_Data.RateID)) ? ADN011_Data.RateID : 0;
            model.RateUnit = !string.IsNullOrEmpty(Convert.ToString(ADN011_Data.RateUnit)) ? ADN011_Data.RateUnit : "";
            model.RateDescr = !string.IsNullOrEmpty(Convert.ToString(ADN011_Data.RateDescr)) ? ADN011_Data.RateDescr : "";
            model.HasMaterials = !string.IsNullOrEmpty(Convert.ToString(ADN011_Data.HasMaterials)) && ADN011_Data.HasMaterials;
            model.MaterialsRequired = !string.IsNullOrEmpty(Convert.ToString(ADN011_Data.MaterialsRequired)) && ADN011_Data.MaterialsRequired;
            model.AltCode = ADN011_Data.AltCode;

            model.ClientBillAmount = 0;
            model.CBStartDate = DateTime.Now;
            model.CBEndDate = DateTime.Now;

            model.PayFromAmount = 0;
            model.PFStartDate = DateTime.Now;
            model.PFEndDate = DateTime.Now;

            #endregion

            return View("CreateOrUpdate", model);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 5, Mode = ActionMode.Write)]
        public JsonResult CreateOrUpdate(CustomActivityModel model)
        {
            try
            {
                if (model.RateID == 0)
                {
                    ADM01100 data = new ADM01100
                    {
                        ClientID = model.ClientID,
                        DepartmentId = model.DepartmentID,
                        RateClass = model.RateClass,
                        RateDescr = model.RateDescr,
                        RateUnit = model.RateUnit,
                        StartDate = DateTime.Now,
                        HasMaterials = model.HasMaterials,
                        MaterialsRequired = model.MaterialsRequired,
                        AltCode = model.AltCode,
                        Active = model.Active
                    };
                    db.ADM01100.Add(data);
                    db.SaveChanges();

                    #region save code in tblClientBill with checking the existance.

                    ADM01150 rec = new ADM01150
                    {
                        RateID = data.RateID,
                        Amount = model.ClientBillAmount,
                        StartDate = (model.CBStartDate == DateTime.MinValue) ? DateTime.Now : model.CBStartDate,
                        EndDate = (model.CBEndDate == DateTime.MinValue) ? DateTime.Now : model.CBEndDate
                    };
                    db.ADM01150.Add(rec);
                    db.SaveChanges();

                    #endregion

                    #region save code in tblPaymentFrom with checking the existance.

                    ADM01250 rec1 = new ADM01250
                    {
                        RateID = data.RateID,
                        Amount = model.ClientBillAmount,
                        StartDate = (model.CBStartDate == DateTime.MinValue) ? DateTime.Now : model.CBStartDate,
                        EndDate = (model.CBEndDate == DateTime.MinValue) ? DateTime.Now : model.CBEndDate
                    };
                    db.ADM01250.Add(rec1);
                    db.SaveChanges();

                    #endregion
                }
                else
                {
                    var oldRate = db.ADM01100.AsNoTracking().Where(x => x.RateID == model.RateID).Select(x => new
                    {
                        x.StartDate
                    }).FirstOrDefault();

                    var OldDta = db.ADM01100.Where(x => x.RateID == model.RateID).FirstOrDefault();
                    OldDta.ClientID = model.ClientID;
                    OldDta.DepartmentId = model.DepartmentID;
                    OldDta.RateClass = model.RateClass;
                    OldDta.RateDescr = model.RateDescr;
                    OldDta.RateUnit = model.RateUnit;
                    OldDta.Active = model.Active;
                    OldDta.HasMaterials = model.HasMaterials;
                    OldDta.MaterialsRequired = model.MaterialsRequired;
                    OldDta.AltCode = model.AltCode;
                    db.Entry(OldDta).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return Json(new JsonReturnParams
                {
                    Additional = model.RateID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.InnerException.Message,
                    Code = model.RateID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 5, Mode = ActionMode.Write)]
        public JsonResult Delete(int id)
        {
            try
            {
                if (db.TRN23110.Any(x => x.ActivityID == id) || db.ADM01110.Any(x => x.ActivID == id))
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
                    db.ADM01100.Remove(db.ADM01100.Find(id));
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
        public void SetupActivityList()
        {
            ViewBag.Clients = db.ADM03200.Select(x => new SelectListItem
            {
                Text = x.CustName,
                Value = x.CustID.ToString()
            }).ToList();
            ViewBag.Units = DataDictionaries.Units.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key
            }).ToList();
            ViewBag.Classes = db.ADM03500.Select(x => new SelectListItem
            {
                Text = x.ClassName,
                Value = x.ClassId.ToString()
            }).ToList();
            ViewBag.Departments = db.ADM04200.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.DepartmentID.ToString()
            }).ToList();
        }

        public ActionResult GetClientRateList(int id)
        {
            ViewBag.AID = id;
            return View("GetClientRateList");
        }

        public ActionResult CreateUpdateClientRate(int ID, int Activity)
        {
            ADM01150 model = new ADM01150();
            var act = db.ADM01100.Find(Activity);
            ViewBag.ActLable = act != null ? act.RateDescr : "";
            if (ID > 0)
            {
                var data = db.ADM01150.Find(ID);
                if (data != null)
                {
                    model.Amount = data.Amount;
                    model.StartDate = data.StartDate;
                    model.EndDate = data.EndDate;
                    model.RateHistoryID = data.RateHistoryID;
                }
            }
            model.RateID = Activity;
            if (model.StartDate == DateTime.MinValue)
                model.StartDate = DateTime.Now;
            if (model.EndDate == DateTime.MinValue)
                model.EndDate = DateTime.Now;
            return View(model);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 5, Mode = ActionMode.Write)]
        public JsonResult CreateUpdateClientRate(ADM01150 model)
        {
            try
            {
                if (model.Amount <= 0)
                    return Json(new JsonReturnParams
                    {
                        Additional = "Amount cannot be 0 or negetive value.",
                        Code = "999",
                        Msg = ""
                    });
                if (model.RateHistoryID == 0)
                {
                    db.ADM01150.Add(model);
                    db.SaveChanges();

                    if (db.ADM01150.Where(x => x.RateID == model.RateID).ToList().Count > 1)
                    {
                        var data = db.ADM01150.Where(x => x.RateID == model.RateID).OrderByDescending(x => x.RateHistoryID).ToList().Skip(1).Take(1).FirstOrDefault();
                        if (data != null)
                        {
                            data.EndDate = model.StartDate.AddDays(-1);
                            db.Entry(data).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    var OldData = db.ADM01150.Find(model.RateHistoryID);
                    OldData.Amount = model.Amount;
                    OldData.EndDate = model.EndDate;
                    OldData.StartDate = model.StartDate;
                    db.Entry(OldData).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return Json(new JsonReturnParams
                {
                    Additional = model.RateID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.InnerException.Message,
                    Code = model.RateID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 5, Mode = ActionMode.Write)]
        public JsonResult DeleteClientRate(int ID)
        {
            try
            {
                var ClientRec = db.ADM01150.Find(ID);
                var WorkRec = db.TRN23110.Where(x => x.ActivityID == ClientRec.RateID && (x.ActDate >= ClientRec.StartDate && x.ActDate <= ClientRec.EndDate)).ToList();
                if (WorkRec.Count <= 0)
                {
                    db.ADM01150.Remove(db.ADM01150.Find(ID));
                    db.SaveChanges();
                    return Json(new JsonReturnParams
                    {
                        Additional = ID,
                        Code = "100",
                        Msg = ""
                    });
                }
                else
                {
                    return Json(new JsonReturnParams
                    {
                        Additional = "Record cannot be deleted.",
                        Code = ID == 0 ? "200" : "300",
                        Msg = "Record cannot be deleted."
                    });
                }
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.InnerException.Message,
                    Code = ID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }

        public ActionResult GetPaymentRateList(int id)
        {
            ViewBag.AID = id;
            return View("PaymentRateList");
        }

        public ActionResult CreateUpdatePaymentRate(int ID, int Activity)
        {
            ADM01250 model = new ADM01250();
            var act = db.ADM01100.Find(Activity);
            ViewBag.ActLable = act != null ? act.RateDescr : "";
            if (ID > 0)
            {
                var data = db.ADM01250.Find(ID);
                if (data != null)
                {
                    model.Amount = data.Amount;
                    model.StartDate = data.StartDate;
                    model.EndDate = data.EndDate;
                    model.RateHistoryID = data.RateHistoryID;
                }
            }
            model.RateID = Activity;
            if (model.StartDate == DateTime.MinValue)
                model.StartDate = DateTime.Now;
            if (model.EndDate == DateTime.MinValue)
                model.EndDate = DateTime.Now;
            return View(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 5, Mode = ActionMode.Write)]
        public JsonResult CreateUpdatePaymentRate(ADM01250 model)
        {
            try
            {
                if (model.Amount <= 0)
                    return Json(new JsonReturnParams
                    {
                        Additional = "Amount cannot be 0 or negetive value.",
                        Code = "999",
                        Msg = ""
                    });
                if (model.RateHistoryID == 0)
                {
                    db.ADM01250.Add(model);
                    db.SaveChanges();

                    if (db.ADM01250.Where(x => x.RateID == model.RateID).ToList().Count > 1)
                    {
                        var data = db.ADM01250.Where(x => x.RateID == model.RateID).OrderByDescending(x => x.RateHistoryID).ToList().Skip(1).Take(1).FirstOrDefault();
                        if (data != null)
                        {
                            data.EndDate = model.StartDate.AddDays(-1);
                            db.Entry(data).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    var OldData = db.ADM01250.Find(model.RateHistoryID);
                    OldData.Amount = model.Amount;
                    OldData.EndDate = model.EndDate;
                    OldData.StartDate = model.StartDate;
                    db.Entry(OldData).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return Json(new JsonReturnParams
                {
                    Additional = model.RateID,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.InnerException.Message,
                    Code = model.RateID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }

        [HttpPost]
        //[TeliconAuthorize(TaskId = 5, Mode = ActionMode.Write)]
        public JsonResult DeletePaymentRate(int ID)
        {
            try
            {
                var ClientRec = db.ADM01250.Find(ID);
                var WorkRec = db.TRN23110.Where(x => x.ActivityID == ClientRec.RateID && (x.ActDate >= ClientRec.StartDate && x.ActDate <= ClientRec.EndDate)).ToList();
                if (WorkRec.Count <= 0)
                {
                    db.ADM01250.Remove(db.ADM01250.Find(ID));
                    db.SaveChanges();
                    return Json(new JsonReturnParams
                    {
                        Additional = ID,
                        Code = "100",
                        Msg = ""
                    });
                }
                else
                {
                    return Json(new JsonReturnParams
                    {
                        Additional = "Record cannot be deleted.",
                        Code = ID == 0 ? "200" : "300",
                        Msg = "Record cannot be deleted."
                    });
                }
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.InnerException.Message,
                    Code = ID == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }

        public ActionResult ActivityMaterials(int id)
        {
            var mats = new List<ActMaterialList>();
            if (id >= 0)
            {
                var dbMats = db.ADM13100.ToList();
                foreach (var x in dbMats)
                {
                    var actMat = db.ADM01110.FirstOrDefault(r => r.ActivID == id && r.MaterID == x.MaterialID);
                    mats.Add(new ActMaterialList
                    {
                        ActMatId = actMat != null ? actMat.ActMatID : 0,
                        MatId = x.MaterialID,
                        MatName = x.MaterialName,
                        Qty = actMat != null ? actMat.ActMatQty : Convert.ToInt32(x.MaxQty),
                        IsSelected = actMat != null
                    });
                }
            }
            ViewBag.actId = id;
            return View(mats);
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 5, Mode = ActionMode.Write)]
        public JsonResult SaveActMaterials(List<ActMaterialList> matList, int actId)
        {
            db.Database.OpenConnection();
            using TransactionScope ts = new TransactionScope();
            try
            {
                var matIds = matList.Select(x => x.MatId).ToList();
                db.ADM01110.RemoveRange(db.ADM01110.Where(x => x.ActivID == actId && !matIds.Contains(x.MaterID)));
                db.SaveChanges();
                var existIds = matList.Where(x => x.ActMatId > 0).Select(x => x.ActMatId);
                foreach (var dbItem in db.ADM01110.Where(x => existIds.Contains(x.ActMatID)))
                {
                    var item = matList.Find(x => x.ActMatId == dbItem.ActMatID);
                    dbItem.ActMatQty = item.Qty;
                }
                var list = new List<ADM01110>();
                foreach (var item in matList.Where(x => x.ActMatId <= 0))
                {
                    list.Add(new ADM01110
                    {
                        ActivID = actId,
                        MaterID = item.MatId,
                        ActMatQty = item.Qty
                    });
                }
                db.ADM01110.AddRange(list);
                db.SaveChanges();
                ts.Complete();
                ts.Dispose();
                return Json(new JsonReturnParams
                {
                    Additional = actId,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = "300",
                    Msg = e.Message
                });
            }
        }

        [HttpPost]
        //[TeliconAuthorize(TaskId = 5, Mode = ActionMode.Write)]
        public JsonResult ActivityAllowedForMaterials(int actId)
        {
            var hasMat = db.ADM01100.Find(actId).HasMaterials;
            return Json(new JsonReturnParams
            {
                Additional = hasMat,
                Code = "300",
                Msg = ""
            });
        }
    }
}