using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;
using TeliconLatest.Reusables;

namespace TeliconLatest.Controllers
{
    public class WorkOrderController : Controller
    {
        private readonly TeliconDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly string settingFilePath = string.Empty;

        public WorkOrderController(TeliconDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            _env = env;
            settingFilePath = Path.Combine(_env.WebRootPath, "settings.xml");
        }
        //
        // GET: /WorkOrders/
        #region Pages
        //[TeliconAuthorize(TaskId = 15)]
        public ActionResult Index()
        {
            var minYear = db.TRN23100.Min(x => x.Requestdt).Year;
            var maxYear = db.TRN23100.Max(x => x.Requestdt).Year;
            var yearsForSelection = new List<SelectListItem>();
            for (int year = minYear; year <= maxYear; year++)
            {
                yearsForSelection.Add(new SelectListItem { Text = year.ToString(), Value = year.ToString(), Selected = year == maxYear });
            }
            ViewBag.Years = yearsForSelection;
            var statuses = DataDictionaries.WordOrderStatuses.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key.ToString()
            }).ToList();
            ViewBag.Statuses = statuses;

            var zones = db.ADM26100.OrderBy(x => x.Name).Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.ZoneID.ToString()
            }).ToList();
            zones.Insert(0, new SelectListItem { Text = "All", Value = "" });
            ViewBag.Zones = zones;

            var clas = db.ADM03500.OrderBy(x => x.ClassName).Select(x => new SelectListItem
            {
                Text = x.ClassName,
                Value = x.ClassId.ToString()
            }).ToList();
            clas.Insert(0, new SelectListItem { Text = "All", Value = "" });
            ViewBag.Clas = clas;

            if (User.IsInRole("Technician") || User.IsInRole("Supervisor"))
                ViewBag.canEdit = false;
            else
                ViewBag.canEdit = true;
            return View();
        }

        //[Authorize(Roles = "AppAdmin, SuperAdmin, Admin, Supervisor")]
        //[TeliconAuthorize(TaskId = 15)]
        public ActionResult Create(string type)
        {
            ViewBag.Teams = new List<TeamMember>();
            SetupWorkOrder(null);
            var model = new TRN23100();
            model.Dispatchdt = DateTime.Now.Date;
            model.Requestdt = DateTime.Now.Date;
            TeliconUserFull tUser = GetLoggedUser();
            var id = tUser != null ? tUser.ProfileFullInfo != null ? tUser.ProfileFullInfo.ProfileID : 0 : 0;
            var member = db.ADM03300.FirstOrDefault(x => x.ConID == id);
            if (member != null)
                ViewBag.Teams = new List<TeamMember>() {
                    new TeamMember {
                    ID = member.ConID,
                    ImgUrl = CheckIfUserImgExists(member.EmployeeID),
                    Name = member.FirstName + " " + member.LastName,
                    IsLead = true
                    }
                };
            ViewBag.Type = type;
            return View("CreateOrUpdate", model);
        }

        //[TeliconAuthorize(TaskId = 15)]
        public ActionResult Edit(int id, string type)
        {
            ViewBag.Teams = db.ADM03400.Where(x => x.WorkOrderId == id).AsEnumerable().Select(x => new TeamMember
            {
                ID = x.ContractorID,
                ImgUrl = CheckIfUserImgExists(x.ADM03300.EmployeeID),
                Name = x.ADM03300.FirstName + " " + x.ADM03300.LastName,
                IsLead = x.CrewLead,
                Portion = x.ContractorRate
            }).ToList();
            ViewBag.Type = type;
            var model = db.TRN23100.Find(id);
            SetupWorkOrder(model.ClassId, model);
            return View("CreateOrUpdate", model);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public async Task<JsonResult> CreateOrUpdateAsync(TRN23100 model)
        {
            try
            {
                if (model.Dispatchdt > DateTime.Now)
                    return Json(new JsonReturnParams
                    {
                        Additional = "Cannot use future date as Dispatch Date.",
                        Code = "999",
                        Msg = "Cannot use future date as Dispatch Date."
                    });
                else if (model.Requestdt > DateTime.Now)
                    return Json(new JsonReturnParams
                    {
                        Additional = "Cannot use future date as Request Date.",
                        Code = "999",
                        Msg = "Cannot use future date as Request Date."
                    });
                else if (model.Requestdt > model.Dispatchdt)
                    return Json(new JsonReturnParams
                    {
                        Additional = "Request Date cannot be greater than Dispatch Date.",
                        Code = "999",
                        Msg = "Request Date cannot be greater than Dispatch Date."
                    });
                model.Wo_ref = model.Wo_ref.TrimStart().TrimEnd().ToUpper();
                model.Wo_title = model.Wo_title.TrimStart().TrimEnd().ToUpper();
                model.Requestby = model.Requestby.ToUpper().TrimStart().TrimEnd();

                if (model.Workid == 0)
                {
                    model.CreateBy = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.PrimarySid).Value;
                    model.Wo_title = model.Wo_title.ToUpper();
                    model.Wo_ref = model.Wo_ref.ToUpper();
                    db.TRN23100.Add(model);
                    if (User.IsInRole("Technician") || User.IsInRole("Supervisor"))
                    {
                        var contractor = db.ADM03300.FirstOrDefault(x => x.Email == User.Identity.Name);
                        if (contractor != null)
                        {
                            db.ADM03400.Add(new ADM03400
                            {
                                ContractorRate = 100,
                                WorkOrderId = model.Workid,
                                CrewLead = true,
                                ContractorID = contractor.ConID
                            });
                        }
                    }
                    db.SaveChanges();
                }
                else
                {
                    var order = db.TRN23100.Find(model.Workid);
                    order.Wo_title = model.Wo_title.ToUpper();
                    order.Wo_ref = model.Wo_ref.ToUpper();
                    order.Wo_split = model.Wo_split;
                    order.Wo_split2 = model.Wo_split2;
                    order.PONum = model.PONum;
                    bool canUpdate = true;
                    if (order != null && order.Submitted)
                        if (User.IsInRole("Technician") || User.IsInRole("Supervisor"))
                            return Json(new
                            {
                                Msg = "<p>Cannot <b>Update</b> Workorder. It has been <b class='default-col'>Submitted</b>.</p>"
                            });
                        else
                            canUpdate = true;
                    else
                        canUpdate = true;
                    if (canUpdate)
                        if (await TryUpdateModelAsync(order, "", x => x.Wo_ref, x => x.Requestby, x => x.Wo_title, x => x.DepartmentID, x => x.ClassId, x => x.SpliceDocs, x => x.Requestdt, x => x.Dispatchdt, x => x.CompletionDt, x => x.Wo_client, x => x.Wo_split, x => x.Wo_split2, x => x.PONum, x => x.AreaID))
                        {
                            if (order.Status.ToLower() == "i")
                            {
                                int invNo = order.TRN23110.FirstOrDefault().TRN09110.FirstOrDefault().InvoiceNum;
                                var inv = db.TRN09100.Find(invNo);
                                if (inv != null)
                                {
                                    inv.InvoiceTitle = order.Wo_title.ToUpper();
                                    if (await TryUpdateModelAsync(inv, "", x => x.InvoiceTitle))
                                    {
                                    }
                                }
                            }
                            db.SaveChanges();
                        }
                }
                return Json(new JsonReturnParams
                {
                    Additional = model.Workid,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (DbUpdateException e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.Workid == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }
        #endregion

        #region Merger
        public ActionResult Merger()
        {
            int? invID = null;
            var invNos = db.TRN09100.AsEnumerable().Where(y => y.Status.ToLower() == "r").Select(y => new SelectListItem
            {
                Text = Customs.MakeGenericInvoiceNo(y.InvoiceNum).ToString(),
                Value = y.InvoiceNum.ToString()
            }).ToList();
            ViewBag.InvNOs = invNos;
            ViewBag.InvoiceID = invID.HasValue ? invID.Value : 0;
            ViewBag.Invoiced = invID.HasValue;
            ViewBag.id = null;
            ViewBag.Title = "";
            return View(GetRefrencesForMerging(null, null));
        }
        public ActionResult GetMerger(int id)
        {
            int? invID = null;
            var merge = db.TRN13120.Find(id);
            if (merge != null)
            {
                ViewBag.RefCode = merge.MergedRefNum;
                var wOActs = db.TRN23110.Where(x => x.WorkOID == merge.WorkOrderId && x.TRN09110.Any());
                if (wOActs.Count() > 0)
                    invID = wOActs.Select(x => x.TRN09110.FirstOrDefault().InvoiceNum).FirstOrDefault();
            }
            var invNos = db.TRN09100.AsEnumerable().Where(y => y.Status.ToLower() == "r").Select(y => new SelectListItem
            {
                Text = Customs.MakeGenericInvoiceNo(y.InvoiceNum).ToString(),
                Value = y.InvoiceNum.ToString()
            }).ToList();
            ViewBag.InvNOs = invNos;
            ViewBag.InvoiceID = invID.HasValue ? invID.Value : 0;
            ViewBag.Invoiced = invID.HasValue;
            ViewBag.id = id;
            ViewBag.Title = merge != null ? merge.MergerdTitle.ToUpper() : "";
            return View("Merger", GetRefrencesForMerged(merge.MergedRefNum));
        }
        public ActionResult MergeCandiditates(string code)
        {
            var candidates = db.TRN23100.Where(x => x.Wo_ref == code && (x.Status != "i" && !x.TRN13110.Any() && !x.TRN13120.Any())).AsEnumerable().Select(x => new WorkOrderPartial
            {
                WOId = x.Workid,
                Title = x.Wo_title.ToUpper(),
                Status = DataDictionaries.WordOrderStatuses[x.Status],
                Total = x.TRN23110.Sum(y => Convert.ToDouble(y.OActQty) * y.ADM01100.GetRateAmountForDate(y.ActDate)),
                IsMerged = db.TRN13120.FirstOrDefault(m => m.WorkOrderId == x.Workid) != null || db.TRN13110.FirstOrDefault(m => m.WorkOrderId == x.Workid) != null
            }).ToList();
            return View(candidates);
        }
        public ActionResult MergeCandiditatesWithMerged(string code, int id)
        {
            var candidates = db.TRN23100.Where(x => x.Wo_ref == code && (x.TRN13110.Any(w => w.MergedOrderId == id) || x.TRN13120.Any(w => w.MergedOrderId == id) || (!x.TRN13110.Any() && !x.TRN13120.Any() && x.Status != "i"))).AsEnumerable().Select(x => new WorkOrderPartial
            {
                WOId = x.Workid,
                Title = x.Wo_title.ToUpper(),
                Status = DataDictionaries.WordOrderStatuses[x.Status],
                Total = x.TRN23110.Sum(y => Convert.ToDouble(y.OActQty) * y.ADM01100.GetRateAmountForDate(y.ActDate)),
                IsMerged = db.TRN13120.FirstOrDefault(m => m.WorkOrderId == x.Workid) != null || db.TRN13110.FirstOrDefault(m => m.WorkOrderId == x.Workid) != null
            }).ToList();
            return View("MergeCandiditates", candidates);
        }
        public ActionResult GetMergeReferences(string val)
        {
            return PartialView("ReferenceListPartial", GetRefrencesForMerging(val, null));
        }
        public ActionResult GetMergeReferencesForMerged(string val, int id)
        {
            return PartialView("ReferenceListPartial", GetRefrencesForMerging(val, id));
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public async Task<JsonResult> MergeWorkOrdersAsync(List<int> wOIds, int masterWOId, bool isUpdate, int? id, string title)
        {
            db.Database.OpenConnection();
            using TransactionScope ts = new TransactionScope();
            try
            {
                if (wOIds.Count > 0)
                {
                    if (db.TRN23100.Any(x => x.Workid == masterWOId && x.Status.ToLower() == "i") || db.TRN23100.Any(x => wOIds.Contains(x.Workid) && x.Status.ToLower() == "i"))
                        return Json(new JsonReturnParams
                        {
                            Additional = "Warning",
                            Code = "300",
                            Msg = "<p>Cannot <b class='default-col'>Merge</b> some Merged Work Order(s) already <b class='act-col'>Invoiced</b>. Please <b class='act-col'>Reverse</b> the invoice and try again.</p>"
                        });
                    TRN13120 master = db.TRN13120.Find(id ?? 0);
                    var masterWO = db.TRN23100.Find(masterWOId);
                    if (isUpdate)
                        return await PerformUpdateMergeAsync(wOIds, masterWOId, id.Value, ts, title);
                    if (master == null)
                    {
                        master = new TRN13120()
                        {
                            WorkOrderId = masterWOId,
                            MergedRefNum = masterWO.Wo_ref,
                            MergerdTitle = string.IsNullOrEmpty(title) ? masterWO.Wo_title.ToUpper() : title.ToUpper(),
                            MergerdDate = DateTime.Now.Date
                        };
                        db.TRN13120.Add(master);
                    }
                    else
                        db.TRN13110.RemoveRange(db.TRN13110.Where(x => x.MergedOrderId == master.MergedOrderId));
                    db.SaveChanges();
                    var list = new List<TRN13110>();
                    foreach (var wOId in wOIds)
                    {
                        list.Add(new TRN13110
                        {
                            WorkOrderId = wOId,
                            MergedOrderId = master.MergedOrderId
                        });
                    }
                    db.TRN13110.AddRange(list);
                    db.SaveChanges();
                    ts.Complete();
                    ts.Dispose();
                    return Json(new JsonReturnParams
                    {
                        Additional = master.MergedOrderId,
                        Code = "100",
                        Msg = ""
                    });
                }
                else
                    return Json(new JsonReturnParams
                    {
                        Additional = "Warning",
                        Code = "300",
                        Msg = "<p>Cannot <b class='default-col'>Merge</b> there are no selected Work Orders to <b class='act-col'>Merge</b>.</p>"
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
        public async Task<JsonResult> PerformUpdateMergeAsync(List<int> wOIds, int masterWOId, int id, TransactionScope ts, string title)
        {
            try
            {
                int masterMergeId = id;
                bool isRemoved = false;
                var masterWO = db.TRN23100.Find(masterWOId);
                TRN13120 master = db.TRN13120.Find(masterMergeId);

                master.WorkOrderId = masterWOId;
                master.MergedRefNum = masterWO.Wo_ref;
                master.MergerdTitle = string.IsNullOrEmpty(title) ? masterWO.Wo_title.ToUpper() : title.ToUpper();
                master.MergerdDate = DateTime.Now.Date;
                if (await TryUpdateModelAsync(master, "", x => x.WorkOrderId, x => x.MergedRefNum, x => x.MergerdTitle, x => x.MergerdDate))
                {
                    db.TRN13110.RemoveRange(db.TRN13110.Where(x => x.MergedOrderId == master.MergedOrderId));
                    isRemoved = true;
                }
                db.SaveChanges();
                if (isRemoved)
                {
                    var list = new List<TRN13110>();
                    foreach (var wOId in wOIds)
                    {
                        list.Add(new TRN13110
                        {
                            WorkOrderId = wOId,
                            MergedOrderId = master.MergedOrderId
                        });
                    }
                    db.TRN13110.AddRange(list);
                    db.SaveChanges();
                    ts.Complete();
                    ts.Dispose();
                    return Json(new JsonReturnParams
                    {
                        Additional = "",
                        Code = "100",
                        Msg = ""
                    });
                }
                else
                    return Json(new JsonReturnParams
                    {
                        Additional = "Warning",
                        Code = "300",
                        Msg = "<p>Cannot <b class='default-col'>Merge</b>, something went <b class='act-col'>Wrong</b>.</p>"
                    });
            }
            catch
            {
                throw;
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public JsonResult DeleteMerge(string id)
        {
            int mergeid = Convert.ToInt32(id);
            db.Database.OpenConnection();
            int saved = -1;
            using TransactionScope ts = new TransactionScope();
            try
            {
                if (db.TRN13110.Any(x => x.MergedOrderId == mergeid))
                {
                    db.TRN13110.RemoveRange(db.TRN13110.Where(x => x.MergedOrderId == mergeid));
                    saved = db.SaveChanges();
                }
                db.TRN13120.Remove(db.TRN13120.Find(mergeid));
                saved = db.SaveChanges();
                ts.Complete();
                return Json(new JsonReturnParams
                {
                    Additional = saved,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = saved,
                    Code = "400",
                    Msg = e.Message
                });
            }
        }
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public ActionResult ProcessMerger(string code, int? invID, int id, string title)
        {
            db.Database.OpenConnection();
            using TransactionScope ts = new TransactionScope();
            try
            {
                List<int> wOIds = new List<int>();
                var merge = db.TRN13120.Find(id);
                if (merge != null)
                {
                    title = string.IsNullOrEmpty(title) ? merge.MergerdTitle.ToUpper() : title.ToUpper();
                    wOIds.Add(merge.WorkOrderId);
                    wOIds.AddRange(merge.TRN13110.Select(i => i.WorkOrderId));
                }
                else
                    return Json(new JsonReturnParams
                    {
                        Additional = "",
                        Code = "300",
                        Msg = "<p>Cannot <b class='default-col'>Process</b> the Work Orders those have not yet been <b class='act-col'>Merged</b>. Please make sure that you have merged the Work Orders.</p>"
                    });
                if (wOIds != null && wOIds.Count > 0)
                {
                    var total = db.TRN23110.Where(x => wOIds.Contains(x.WorkOID)).AsEnumerable().Sum(x => (Convert.ToDouble(x.OActQty) * x.ADM01100.GetClientRateAmountForDate(x.ActDate)));
                    var invoice = new TRN09100();
                    if (invID.HasValue)
                    {
                        invoice = db.TRN09100.Find(invID.Value);
                        invoice.InvoiceTotal = Convert.ToDecimal(total);
                        invoice.Status = "o";
                        invoice.InvoiceTitle = title.ToUpper();
                    }
                    else
                    {
                        invoice = new TRN09100
                        {
                            InvoiceDate = DateTime.Now,
                            GeneratedDate = DateTime.Now,
                            InvoiceTitle = title.ToUpper(),
                            Status = "o",
                            InvoiceTotal = Convert.ToDecimal(total)
                        };
                        db.TRN09100.Add(invoice);
                    }
                    db.SaveChanges();
                    foreach (var wOId in wOIds)
                    {
                        foreach (var act in db.TRN23110.Where(x => x.WorkOID == wOId).ToList())
                        {
                            act.ActQty = 0;
                            db.TRN09110.Add(new TRN09110
                            {
                                InvoicedAmount = act.OActQty,
                                InvoiceNum = invoice.InvoiceNum,
                                WoActID = act.RecID
                            });
                        }
                        db.TRN23100.Find(wOId).Status = "i";
                    }
                    db.SaveChanges();
                    ts.Complete();
                    ts.Dispose();
                }
                return GetMerger(merge.MergedOrderId);
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
        public JsonResult MergerActivityCount(int id)
        {
            string msg = "";
            msg = db.TRN13120.Any(x => x.MergedOrderId == id) ? "" : "notMerged";
            bool hasActivities = db.TRN13120.Any(x => x.MergedOrderId == id && x.TRN23100.TRN23110.Any()) || db.TRN13120.Any(x => x.MergedOrderId == id && x.TRN13110.Any(a => a.TRN23100.TRN23110.Any()));
            bool verified = !(db.TRN13120.Any(x => x.MergedOrderId == id && x.TRN23100.Status.ToLower() != "v") || db.TRN13120.Any(x => x.MergedOrderId == id && x.TRN13110.Any(a => a.TRN23100.Status.ToLower() != "v")));
            var data = new
            {
                any = hasActivities,
                verified = verified,
                msg = msg
            };
            return Json(data);
        }
        #endregion

        #region Updaters
        [HttpPost]
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public JsonResult Delete(string id)
        {
            int wid = Convert.ToInt32(id.Split('_')[1]);
            db.Database.OpenConnection();
            int saved = -1;
            using TransactionScope ts = new TransactionScope();
            try
            {
                if (db.TRN23110.Where(x => x.WorkOID == wid).Any(x => x.TRN09110.Any()))
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
                    if (db.TRN23110.Any(x => x.WorkOID == wid))
                    {
                        db.TRN23110.RemoveRange(db.TRN23110.Where(x => x.WorkOID == wid));
                        saved = db.SaveChanges();
                    }
                    if (db.ADM03400.Any(x => x.WorkOrderId == wid))
                    {
                        db.ADM03400.RemoveRange(db.ADM03400.Where(x => x.WorkOrderId == wid));
                        saved = db.SaveChanges();
                    }
                    db.TRN23100.Remove(db.TRN23100.Find(wid));
                    saved = db.SaveChanges();
                    ts.Complete();
                    return Json(new JsonReturnParams
                    {
                        Additional = saved,
                        Code = "100",
                        Msg = ""
                    });
                }
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = saved,
                    Code = "400",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public JsonResult UpdateActivities(List<WorkOrderRecordSet> model)
        {
            db.Database.OpenConnection();
            model.ForEach(x => x.Location = x.Location.Trim());
            int saved = -1;
            using TransactionScope ts = new TransactionScope();
            try
            {
                foreach (var rec in model)
                {
                    var actIDs = rec.Activities.Where(x => x.RecID != 0 && x.Amount > 0).Select(x => x.RecID);
                    foreach (var activity in db.TRN23110.Where(x => actIDs.Contains(x.RecID)))
                    {
                        var act = rec.Activities.FirstOrDefault(x => x.RecID == activity.RecID);
                        activity.ActivityID = act.RID;
                        if (rec.InvID.HasValue)
                        {
                            var invAct = db.TRN09110.FirstOrDefault(x => x.InvoiceNum == rec.InvID.Value && x.WoActID == act.RecID);
                            if (activity.OActQty > 0)
                            {
                                var invQtyDiff = act.Amount - activity.OActQty;
                                activity.ActQty = invQtyDiff < 0 ? (invQtyDiff * -1) : 0;
                            }
                            invAct.InvoicedAmount = act.Amount;
                        }
                        else
                        {
                            activity.OActQty = act.Amount;
                            activity.ActQty = act.Amount;
                        }
                        if (act.HasMetarials)
                        {
                            var actMatIDs = act.Metarials.Where(x => x.RID != 0 && x.Amount > 0).Select(x => x.RID);
                            if (activity.TRN23120.Any())
                            {
                                foreach (var metarial in db.TRN23120.Where(x => actMatIDs.Contains(x.WoMatRecID)))
                                {
                                    var mat = act.Metarials.FirstOrDefault(x => x.RID == metarial.WoMatRecID);
                                    metarial.WoMatQty = Convert.ToInt32(mat.Amount);
                                }
                            }
                            if (act.Metarials.Any(x => x.RID == 0))
                            {
                                if (act.Metarials.Where(x => x.RID == 0 && x.Amount > 0).Count() > 0)
                                    foreach (var metarial in act.Metarials.Where(x => x.RID == 0 && x.Amount > 0))
                                    {
                                        var newMat = new TRN23120
                                        {
                                            WoActID = activity.RecID,
                                            WoMatID = metarial.MetarialId,
                                            WoMatQty = Convert.ToInt32(metarial.Amount)
                                        };
                                        db.TRN23120.Add(newMat);
                                    }
                            }
                        }
                    }
                    foreach (var activity in rec.Activities.Where(x => x.RecID == 0 && x.Amount > 0))
                    {
                        if (db.TRN23110.Where(x => x.WorkOID == rec.WID && x.ActDate == rec.Date
                            && x.Location == rec.Location && x.ActivityID == activity.RID).Any())
                        {
                            var record = db.TRN23110.FirstOrDefault(x => x.WorkOID == rec.WID && x.ActDate == rec.Date
                            && x.Location == rec.Location && x.AdtnlDetails == rec.AdtnlDetails && x.ActivityID == activity.RID);
                            record.OActQty = activity.Amount;
                            if (rec.InvID.HasValue)
                            {
                                var invAct = db.TRN09110.FirstOrDefault(x => x.InvoiceNum == rec.InvID.Value && x.WoActID == activity.RecID);
                                if (record.OActQty > 0)
                                {
                                    var invQtyDiff = activity.Amount - record.OActQty;
                                    record.ActQty = invQtyDiff < 0 ? (invQtyDiff * -1) : 0;
                                }
                                invAct.InvoicedAmount = activity.Amount;
                            }
                            else
                            {
                                record.OActQty = activity.Amount;
                                record.ActQty = activity.Amount;
                            }
                            if (activity.HasMetarials)
                            {
                                var actMatIDs = activity.Metarials.Where(x => x.RID != 0 && x.Amount > 0).Select(x => x.RID);
                                if (record.TRN23120.Any())
                                {
                                    foreach (var metarial in db.TRN23120.Where(x => actMatIDs.Contains(x.WoMatRecID)))
                                    {
                                        var mat = activity.Metarials.FirstOrDefault(x => x.RID == metarial.WoMatRecID);
                                        metarial.WoMatQty = Convert.ToInt32(mat.Amount);
                                    }
                                }
                                if (activity.Metarials.Any(x => x.RID == 0))
                                {
                                    if (activity.Metarials.Where(x => x.RID == 0 && x.Amount > 0).Count() > 0)
                                        foreach (var metarial in activity.Metarials.Where(x => x.RID == 0 && x.Amount > 0))
                                        {
                                            var newMat = new TRN23120
                                            {
                                                WoActID = record.RecID,
                                                WoMatID = metarial.MetarialId,
                                                WoMatQty = Convert.ToInt32(metarial.Amount)
                                            };
                                            db.TRN23120.Add(newMat);
                                        }
                                }
                            }
                        }
                        else
                        {
                            var newAct = new TRN23110
                            {
                                InvFlag = rec.InvID.HasValue,
                                OActQty = rec.InvID.HasValue ? 0 : activity.Amount,
                                ActQty = rec.InvID.HasValue ? 0 : activity.Amount,
                                ActDate = rec.Date,
                                Location = rec.Location,
                                AdtnlDetails = string.IsNullOrEmpty(rec.AdtnlDetails) ? string.Empty : rec.AdtnlDetails,
                                WorkOID = rec.WID,
                                ActivityID = activity.RID
                            };
                            db.TRN23110.Add(newAct);
                            db.SaveChanges();
                            if (activity.HasMetarials)
                            {
                                var metarials = activity.Metarials;
                                foreach (var metarial in metarials)
                                {
                                    var newMat = new TRN23120
                                    {
                                        WoActID = newAct.RecID,
                                        WoMatID = metarial.MetarialId,
                                        WoMatQty = Convert.ToInt32(metarial.Amount)
                                    };
                                    db.TRN23120.Add(newMat);
                                }
                            }
                            saved = db.SaveChanges();
                            if (rec.InvID.HasValue)
                            {
                                db.TRN09110.Add(new TRN09110
                                {
                                    InvoicedAmount = activity.Amount,
                                    InvoiceNum = rec.InvID.Value,
                                    WoActID = newAct.RecID
                                });
                            }
                        }
                    }
                    foreach (var activity in rec.Activities.Where(x => x.Amount <= 0))
                    {
                        var recAct = db.TRN23110.Find(activity.RecID);
                        if (db.TRN23110.Find(activity.RecID).TRN23120.Any())
                            db.TRN23120.RemoveRange(db.TRN23110.Find(activity.RecID).TRN23120);
                        if (rec.InvID.HasValue)
                        {
                            recAct.ActQty = recAct.OActQty;
                            recAct.TRN23100.Status = "v";
                            db.TRN09110.Remove(db.TRN09110.FirstOrDefault(x => x.InvoiceNum == rec.InvID.Value && x.WoActID == activity.RecID));
                        }
                        else
                            db.TRN23110.Remove(recAct);
                    }
                    if (rec.InvID.HasValue)
                    {
                        if (!db.TRN09110.Any(x => x.InvoiceNum == rec.InvID.Value))
                            db.TRN09100.Find(rec.InvID.Value).Status = "r";
                        else
                        {
                            db.TRN09100.Find(rec.InvID.Value).InvoiceTotal = Convert.ToDecimal(db.TRN09110.Where(x => x.InvoiceNum == rec.InvID.Value).AsEnumerable()
    .Sum(x => (Convert.ToDouble(x.TRN23110.OActQty) * x.TRN23110.ADM01100.GetPaymentRateAmountForDate(x.TRN23110.ActDate))));
                        }
                    }
                }
                saved = db.SaveChanges();
                ts.Complete();
                return Json(new JsonReturnParams
                {
                    Additional = saved,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = "200",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public JsonResult UpdateWorkOrderDate(int wid, DateTime oldDate, DateTime newDate)
        {
            try
            {
                bool invoiced = false;
                invoiced = db.TRN23100.Find(wid).Status.ToLower() == "i";
                List<TRN23110> list = new List<TRN23110>();
                if (!invoiced)
                    db.TRN23110.Where(x => x.WorkOID == wid && x.ActDate == oldDate).ToList().ForEach(x => x.ActDate = newDate);
                else
                {
                    int invID = 0;
                    invID = db.TRN23110.FirstOrDefault(x => x.WorkOID == wid).TRN09110.FirstOrDefault().InvoiceNum;
                    List<int> wOIds = new List<int>();
                    wOIds.AddRange(db.TRN09110.Where(x => x.InvoiceNum == invID).Select(x => x.TRN23110.WorkOID).Distinct());
                    db.TRN23110.Where(x => wOIds.Contains(x.WorkOID) && x.ActDate == oldDate).ToList().ForEach(x => x.ActDate = newDate);
                }
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
                    Code = "200",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public JsonResult UpdateWorkOrderLocation(int wid, DateTime date, string oldLocation, string newLocation, string aditinalDetail)
        {
            try
            {
                newLocation = newLocation.Trim();
                if (oldLocation.ToLower() == "newrec")
                {
                    TRN23110 data = new TRN23110();
                    data.ActDate = DateTime.Now.Date;
                    data.ActivityID = 0;
                    data.ActQty = Convert.ToDecimal(0);
                    data.OActQty = Convert.ToDecimal(0);
                    data.Location = newLocation;
                    data.InvFlag = false;
                    data.AddMaterial = false;
                    data.WorkOID = wid;
                    data.AdtnlDetails = aditinalDetail;
                }
                else
                {
                    bool invoiced = false;
                    invoiced = db.TRN23100.Find(wid).Status.ToLower() == "i";
                    List<TRN23110> list = new List<TRN23110>();
                    if (!invoiced)
                        list = db.TRN23110.Where(x => x.WorkOID == wid && x.ActDate == date && x.Location == oldLocation).ToList();
                    else
                    {
                        int invID = 0;
                        invID = db.TRN23110.FirstOrDefault(x => x.WorkOID == wid).TRN09110.FirstOrDefault().InvoiceNum;
                        List<int> wOIds = new List<int>();
                        wOIds.AddRange(db.TRN09110.Where(x => x.InvoiceNum == invID).Select(x => x.TRN23110.WorkOID).Distinct());
                        list = db.TRN23110.Where(x => wOIds.Contains(x.WorkOID) && x.ActDate == date && x.Location == oldLocation).ToList();
                    }
                    foreach (TRN23110 item in list)
                    {
                        item.Location = newLocation;
                        item.AdtnlDetails = aditinalDetail;
                        db.SaveChanges();
                    }
                }
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
                    Code = "200",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public JsonResult DeleteFromWorkOrder(string val, string type, int wid, int? invID)
        {
            db.Database.OpenConnection();
            using TransactionScope ts = new TransactionScope();
            try
            {
                switch (type)
                {
                    case "Date":
                        DateTime date = Convert.ToDateTime(val);
                        if (invID.HasValue)
                            db.TRN09110.RemoveRange(db.TRN09110.Where(x => x.InvoiceNum == invID.Value && x.TRN23110.ActDate == date));
                        else
                        {
                            if (db.TRN23120.Any(m => m.TRN23110.ActDate == date && m.TRN23110.WorkOID == wid))
                                db.TRN23120.RemoveRange(db.TRN23120.Where(m => m.TRN23110.ActDate == date && m.TRN23110.WorkOID == wid));
                            db.TRN23110.RemoveRange(db.TRN23110.Where(x => x.ActDate == date && x.WorkOID == wid));
                        }
                        break;
                    case "Location":
                        if (invID.HasValue)
                            db.TRN09110.RemoveRange(db.TRN09110.Where(x => x.InvoiceNum == invID.Value && x.TRN23110.Location == val));
                        else
                        {
                            if (db.TRN23120.Any(m => m.TRN23110.Location == val && m.TRN23110.WorkOID == wid))
                                db.TRN23120.RemoveRange(db.TRN23120.Where(m => m.TRN23110.Location == val && m.TRN23110.WorkOID == wid));
                            db.TRN23110.RemoveRange(db.TRN23110.Where(x => x.Location == val && x.WorkOID == wid));
                        }
                        break;
                    default:
                        int recId = Convert.ToInt32(val);
                        if (invID.HasValue)
                            db.TRN09110.RemoveRange(db.TRN09110.Where(x => x.InvoiceNum == invID.Value && x.WoActID == recId));
                        else
                        {
                            if (db.TRN23120.Any(m => m.TRN23110.RecID == recId))
                                db.TRN23120.RemoveRange(db.TRN23120.Where(m => m.TRN23110.RecID == recId));
                            db.TRN23110.Remove(db.TRN23110.Find(recId));
                        }
                        break;
                }
                int saved = db.SaveChanges();
                if (invID.HasValue)
                {
                    if (!db.TRN09110.Any(x => x.InvoiceNum == invID.Value))
                        saved = ChangeWorkOrderStatus(db.TRN09110.Where(x => x.InvoiceNum == invID.Value).FirstOrDefault().TRN23110.WorkOID, "r");
                }
                ts.Complete();
                return Json(new JsonReturnParams
                {
                    Additional = saved,
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
        [HttpPost]
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public JsonResult SaveTeam(TeamForUpdate model)
        {
            try
            {
                db.ADM03400.RemoveRange(db.ADM03400.Where(x => x.WorkOrderId == model.WID));
                db.SaveChanges();
                foreach (var mem in model.Team)
                {
                    db.ADM03400.Add(new ADM03400
                    {
                        ContractorID = mem.ID,
                        ContractorRate = mem.Share,
                        CrewLead = mem.IsLead,
                        WorkOrderId = model.WID
                    });
                }
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
                    Code = "200",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public JsonResult ChangeStatus(int wid, string type)
        {
            try
            {
                var workOrder = db.TRN23100.FirstOrDefault(x => x.Workid == wid);
                if ((type == "submit" || type == "verify") && !db.TRN23110.Any(x => x.WorkOID == wid))
                {
                    return Json(new
                    {
                        Msg = "<p>Cannot <b>" + type.NormalizeSentence(false) + "</b> an empty Workorder. Please add activities before changing the status.</p>"
                    });
                }
                else if (type == "verify" && !workOrder.Submitted && !workOrder.DateSubmitted.HasValue)
                {
                    return Json(new
                    {
                        Msg = "<p>Cannot <b>" + type.NormalizeSentence(false) + "</b> Workorder before it has been <b class='default-col'>Submitted</b>.</p>"
                    });
                }
                else if (type == "submit" && workOrder.Dispatchdt.AddHours(48) < DateTime.Now)
                {
                    return Json(new
                    {
                        Additional = ChangeWorkOrderStatus(wid, type),
                        Msg = "<p>Work Order Submitted <b>late - Penalties apply</b>.</p>",
                        Code = "1001"
                    });
                }
                else
                {
                    return Json(new JsonReturnParams
                    {
                        Additional = ChangeWorkOrderStatus(wid, type),
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
                    Code = "300",
                    Msg = e.Message
                });
            }
        }
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public ActionResult ProcessActivities(int id, string title, int? invID)
        {
            title = title.Trim().ToUpper();
            db.Database.OpenConnection();
            using TransactionScope ts = new TransactionScope();
            try
            {
                var total = db.TRN23110.Where(x => x.WorkOID == id).AsEnumerable().Sum(x => (Convert.ToDouble(x.OActQty) * x.ADM01100.GetClientRateAmountForDate(x.ActDate)));
                var invoice = new TRN09100();
                if (invID.HasValue)
                {
                    invoice = db.TRN09100.Find(invID.Value);
                    invoice.InvoiceTotal = Convert.ToDecimal(total);
                    invoice.Status = "o";
                    invoice.InvoiceTitle = string.IsNullOrEmpty(title) ? db.TRN23100.Find(id).Wo_title.ToUpper() : title;
                }
                else
                {
                    invoice = new TRN09100
                    {
                        InvoiceDate = DateTime.Now,
                        GeneratedDate = DateTime.Now,
                        InvoiceTitle = string.IsNullOrEmpty(title) ? db.TRN23100.Find(id).Wo_title.ToUpper() : title,
                        Status = "o",
                        InvoiceTotal = Convert.ToDecimal(total)
                    };
                    db.TRN09100.Add(invoice);
                }
                db.SaveChanges();
                foreach (var act in db.TRN23110.Where(x => x.WorkOID == id).ToList())
                {
                    act.ActQty = 0;
                    db.TRN09110.Add(new TRN09110
                    {
                        InvoicedAmount = act.OActQty,
                        InvoiceNum = invoice.InvoiceNum,
                        WoActID = act.RecID
                    });
                }
                db.TRN23100.Find(id).Status = "i";
                if (!string.IsNullOrEmpty(title))
                    db.TRN23100.Find(id).Wo_title = title.Trim().ToUpper();
                db.SaveChanges();
                ts.Complete();
                ts.Dispose();
                return GenerateConstructor(id, invoice.InvoiceNum, "");
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
        #endregion

        #region Metarials

        public ActionResult GenerateMetarialConstructor(int? wActid, DateTime date, string loc, int actId, int? invID, string type, int wid, int? qty)
        {
            type = type ?? string.Empty;
            ViewBag.InvoiceID = invID.HasValue ? invID.Value : 0;
            var act = db.ADM01100.Find(actId);
            var user = db.ADM03300.FirstOrDefault(x => x.Email == User.Identity.Name);
            var companyId = db.TRN23100.Find(wid).Wo_client;
            var actsDdl = new List<ConstructorActivityDisplay>();
            if (HttpContext.Session.Get<List<ConstructorActivityDisplay>>("mats") != null)
                actsDdl = HttpContext.Session.Get<List<ConstructorActivityDisplay>>("mats");
            else
            {
                List<int> actsToRemove = new List<int>();
                if (User.IsInRole("Technician") || User.IsInRole("Supervisor"))
                {
                    var deptID = user.DepartmentID;
                    actsToRemove = db.ADM13100.Where(x => x.ADM01110.Any(a => a.ADM01100.DepartmentId != deptID && a.ADM01100.Active)).Select(x => x.MaterialID).ToList();
                }
                actsDdl = db.ADM01110.Where(x => x.ActivID == actId)
                    .Select(x => new ConstructorActivityDisplay { Description = x.ADM13100.MaterialName, ActivityID = x.ADM13100.MaterialID, MaxQty = x.ActMatQty }).ToList();
            }
            ViewBag.ActsDdl = actsDdl;
            var wo = db.TRN23100.Find(wid);
            var mats = db.TRN23120.Where(x => x.WoActID == wActid)
                .Select(x => new ConstructorActivity
                {
                    RID = x.WoMatRecID,
                    ActivityAmount = x.WoMatQty,
                    ActivityID = x.WoMatID
                }).ToList();
            if (mats.Count == 0 && act.MaterialsRequired)
            {
                if (act.RateDescr.ToLower().Contains(("Splice Fiber").ToLower()))
                {
                    mats = db.ADM01110.Where(x => x.ActivID == actId)
                    .Select(x => new ConstructorActivity
                    {
                        ActivityAmount = qty ?? x.ActMatQty,
                        ActivityID = x.MaterID
                    }).ToList();
                }
                else
                {
                    mats = db.ADM01110.Where(x => x.ActivID == actId)
                    .Select(x => new ConstructorActivity
                    {
                        ActivityAmount = x.ActMatQty,
                        ActivityID = x.MaterID
                    }).ToList();
                }
            }

            var model = new MatConstructorPartial
            {
                WOActId = wActid ?? 0,
                ConstructorActivities = mats,
                Location = loc,
                Date = date,
                ActivityId = actId,
                UserRole = new string[1],//System.Web.Security.Roles.GetRolesForUser(),
                Status = wo.Submitted && wo.Status == "v" ? "v" : wo.Submitted && wo.Status != "v" ? "s" : wo.Status.ToLower(),
                Title = date.ToShortDateString() + " | " + loc + " | " + db.ADM01100.FirstOrDefault(a => a.RateID == actId).RateDescr
            };
            ViewBag.Invoiced = invID.HasValue;
            ViewBag.Type = type;
            ViewBag.MatReq = act.MaterialsRequired;
            return PartialView("MatConstructor", model);
        }
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public JsonResult UpdateMetarials(List<WorkOrderRecordSet> model)
        {
            db.Database.OpenConnection();
            int saved = -1;
            using TransactionScope ts = new TransactionScope();
            try
            {
                foreach (var rec in model)
                {
                    var actIDs = rec.Activities.Where(x => x.RecID != 0 && x.Amount > 0).Select(x => x.RecID);
                    foreach (var activity in db.TRN23120.Where(x => actIDs.Contains(x.WoMatRecID)))
                    {
                        var act = rec.Activities.FirstOrDefault(x => x.RecID == activity.WoMatRecID);
                        activity.WoMatID = act.RID;

                        activity.WoMatQty = Convert.ToInt32(act.Amount);
                    }
                    foreach (var activity in rec.Activities.Where(x => x.RecID == 0 && x.Amount > 0))
                    {
                        if (db.TRN23120.Where(x => x.WoActID == rec.WID && x.WoMatID == activity.RID).Any())
                        {
                            var record = db.TRN23120.FirstOrDefault(x => x.WoActID == rec.WID && x.WoMatID == activity.RID);
                            record.WoMatQty = Convert.ToInt32(activity.Amount);
                            if (rec.InvID.HasValue)
                            {

                            }
                            else
                            {
                                record.WoMatQty = Convert.ToInt32(activity.Amount);
                            }
                        }
                        else
                        {
                            var newMat = new TRN23120
                            {
                                WoMatQty = Convert.ToInt32(activity.Amount),
                                WoActID = rec.WID,
                                WoMatID = activity.RID
                            };
                            db.TRN23120.Add(newMat);
                            saved = db.SaveChanges();
                        }
                    }
                    foreach (var activity in rec.Activities.Where(x => x.Amount <= 0))
                    {
                        var recMat = db.TRN23120.Find(activity.RecID);
                        if (rec.InvID.HasValue)
                        {
                        }
                        else
                            db.TRN23120.Remove(recMat);
                    }
                }
                saved = db.SaveChanges();
                ts.Complete();
                return Json(new JsonReturnParams
                {
                    Additional = saved,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (Exception e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = "200",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 15, Mode = ActionMode.Write)]
        public JsonResult DeleteMetarialFromWorkOrder(string val, string type, int wid, int? invID)
        {
            db.Database.OpenConnection();
            using TransactionScope ts = new TransactionScope();
            try
            {
                switch (type)
                {
                    default:
                        int recId = Convert.ToInt32(val);
                        if (invID.HasValue)
                        {
                        }
                        else
                            db.TRN23120.Remove(db.TRN23120.Find(recId));
                        break;
                }
                int saved = db.SaveChanges();
                if (invID.HasValue)
                {
                    if (!db.TRN09110.Any(x => x.InvoiceNum == invID.Value))
                        saved = ChangeWorkOrderStatus(db.TRN09110.Where(x => x.InvoiceNum == invID.Value).FirstOrDefault().TRN23110.WorkOID, "r");
                }
                ts.Complete();
                return Json(new JsonReturnParams
                {
                    Additional = saved,
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
        [HttpPost]
        public JsonResult GetMetarials(int actId)
        {
            var acts = db.ADM01110.Where(x => x.ActivID == actId)
                    .Select(x => new
                    {
                        activityID = x.ADM13100.MaterialID,
                        description = x.ADM13100.MaterialName,
                        maxQty = x.ActMatQty
                    }).ToList();
            return Json(acts);
        }
        [HttpPost]
        public JsonResult GetMaxMetarialQty(int matId)
        {
            return Json(new
            {
                MaxQty = db.ADM13100.FirstOrDefault(x => x.MaterialID == matId).MaxQty ?? 0
            });
        }

        [HttpPost]
        public JsonResult GetMaterialMaxQty(int actId, int matId)
        {
            return Json(new
            {
                MaxQty = db.ADM01110.FirstOrDefault(x => x.ActivID == actId && x.MaterID == matId).ActMatQty
            });
        }

        #endregion

        #region Data Fetchers

        public ActionResult GenerateConstructor(int? wid, int? invID, string type)
        {
            List<int> wOIds = new List<int>();
            if (wid.HasValue)
                wOIds.Add(wid.Value);
            else
                wOIds.AddRange(db.TRN09110.Where(x => x.InvoiceNum == invID.Value).Select(x => x.TRN23110.WorkOID).Distinct());
            wid = !wid.HasValue ? db.TRN09110.FirstOrDefault(x => x.InvoiceNum == invID.Value).TRN23110.TRN23100.Workid : wid.Value;
            var invNos = db.TRN09100.AsEnumerable().Where(y => y.Status.ToLower() == "r" && !y.IsNewFormat).Select(y => new SelectListItem
            {
                Text = Customs.MakeGenericInvoiceNo(y.InvoiceNum).ToString(),
                Value = y.InvoiceNum.ToString()
            }).ToList();
            ViewBag.InvoiceID = invID.HasValue ? invID.Value : 0;
            var model = db.TRN23100.AsEnumerable().Where(x => wOIds.Contains(x.Workid)).Select(x => new ConstructorPartial
            {
                Dates = invID.HasValue ? x.TRN23110.Where(y => y.OActQty > y.ActQty || y.ActQty <= 0).Select(y => y.ActDate).Distinct().ToList() : x.TRN23110.Select(y => y.ActDate).Distinct().ToList(),
                InvoiceNos = invNos,
                WorkOrderId = x.Workid,
                InvoiceId = Customs.MakeGenericInvoiceNo(db.TRN09100.OrderByDescending(y => y.InvoiceNum).FirstOrDefault().InvoiceNum),
                UserRole = new string[1], //System.Web.Security.Roles.GetRolesForUser(),
                Status = x.Submitted && x.Status == "v" ? "v" : x.Submitted && x.Status != "v" ? "s" : x.Status.ToLower(),
                Title = invID.HasValue ? db.TRN09100.Find(invID.Value).InvoiceTitle.ToUpper() : x.Wo_title.ToUpper()
            }).FirstOrDefault();
            ViewBag.Invoiced = invID.HasValue;
            ViewBag.Type = type;
            ViewBag.SubTotal = GetWorkOrderSubTotal(wid ?? 0);
            return PartialView("Constructor", model);
        }
        [HttpPost]
        public ActionResult GetConstructorLocations(int wid, DateTime date, int? invID, string type)
        {
            ViewBag.Type = type;

            List<int> wOIds = new List<int>();
            if (!invID.HasValue)
                wOIds.Add(wid);
            else
                wOIds.AddRange(db.TRN09110.Where(x => x.InvoiceNum == invID.Value).Select(x => x.TRN23110.WorkOID).Distinct());

            var LocIds = invID.HasValue ?
               db.TRN09110.Where(x => x.InvoiceNum == invID.Value && wOIds.Contains(x.TRN23110.WorkOID) && x.TRN23110.ActDate == date)
               .AsEnumerable().Select(x => x.TRN23110.RecID.ToString().NormalizeSentence(true)).Distinct().ToList()
               : db.TRN23110.Where(x => wOIds.Contains(x.WorkOID) && x.ActDate == date).AsEnumerable()
               .Select(x => x.RecID.ToString()).Distinct().ToList();

            ViewBag.LocIds = LocIds;

            var locations = invID.HasValue ?
                db.TRN09110.Where(x => x.InvoiceNum == invID.Value && wOIds.Contains(x.TRN23110.WorkOID) && x.TRN23110.ActDate == date)
                .AsEnumerable().Select(x => x.TRN23110.Location.NormalizeSentence(true)).Distinct().ToList()
                : db.TRN23110.Where(x => wOIds.Contains(x.WorkOID) && x.ActDate == date).AsEnumerable()
                .Select(x => x.Location).Distinct().ToList();

            ViewBag.locations = locations;

            var adtnldetails = invID.HasValue ?
                db.TRN09110.Where(x => x.InvoiceNum == invID.Value && wOIds.Contains(x.TRN23110.WorkOID) && x.TRN23110.ActDate == date)
                .AsEnumerable().Select(x => x.TRN23110.AdtnlDetails.NormalizeSentence(true)).Distinct().ToList()
                : db.TRN23110.Where(x => wOIds.Contains(x.WorkOID) && x.ActDate == date).AsEnumerable()
                .Select(x => x.AdtnlDetails).Distinct().ToList();

            ViewBag.adtnldetails = adtnldetails;
            ViewBag.Invoiced = invID.HasValue;
            return View("ConstructorLocations");
        }
        [HttpPost]
        public ActionResult GetConstructorActivities(int wid, DateTime date, string location, int? invID, string type)
        {
            ViewBag.Type = type;
            return GenerateActivitiesForWorkOrder(wid, date, location, invID, type);
        }
        [HttpPost]
        public JsonResult GetActivities(int wid)
        {
            var wo = db.TRN23100.Find(wid);
            var companyId = wo != null ? wo.Wo_client : 0;
            var user = db.ADM03300.FirstOrDefault(x => x.Email == User.Identity.Name);
            List<int> allowedActIds = new List<int>();
            if (wo != null && wo.DepartmentID.HasValue)
                allowedActIds = db.ADM04210.Where(x => x.DepartmentID == wo.DepartmentID.Value).Select(x => x.ActivityID).ToList();
            List<int> actsToRemove = new List<int>();
            if (User.IsInRole("Technician") || User.IsInRole("Supervisor"))
            {
                var deptID = user.DepartmentID;
                actsToRemove = db.ADM01100.Where(x => x.DepartmentId != deptID && x.Active).Select(x => x.RateID).ToList();
            }

            var acts = new List<ActList>();
            //    db.ADM01100.Where(x => x.ClientID == companyId && x.Active).Select(x => new ActList
            //{
            //    activityID = x.RateID,
            //    description = x.RateDescr,
            //    maxQty = x.MaxQty
            //}).ToList();
            if (wo != null && wo.DepartmentID.HasValue)
            {
                if (User.IsInRole("SuperAdmin") || User.IsInRole("AppAdmin") || User.IsInRole("Admin"))
                    acts = db.ADM01100.Where(x => x.ClientID == companyId && allowedActIds.Contains(x.RateID) && x.Active)
                        .Select(x => new ActList { activityID = x.RateID, description = x.RateDescr, maxQty = x.MaxQty ?? 0 }).ToList();
                else
                    acts = db.ADM01100.Where(x => x.ClientID == companyId && allowedActIds.Contains(x.RateID) && x.Active)
                        .Select(x => new ActList { description = x.RateDescr, activityID = x.RateID, maxQty = x.MaxQty ?? 0 }).ToList();
            }
            else
                acts = db.ADM01100.Where(x => x.ClientID == companyId && x.Active)
                        .Select(x => new ActList { activityID = x.RateID, description = x.RateDescr, maxQty = x.MaxQty ?? 0 }).ToList();

            //if (actsToRemove.Any())
            //    acts.RemoveAll(x => actsToRemove.Contains(x.activityID));

            return Json(acts);
        }
        public ViewResult GenerateActivitiesForWorkOrder(int wid, DateTime date, string location, int? invID, string type)
        {
            List<int> wOIds = new List<int>();
            if (!invID.HasValue)
                wOIds.Add(wid);
            else
                wOIds.AddRange(db.TRN09110.Where(x => x.InvoiceNum == invID.Value).Select(x => x.TRN23110.WorkOID).Distinct());

            var user = db.ADM03300.FirstOrDefault(x => x.Email == User.Identity.Name);
            List<int> allowedActIds = new List<int>();
            var wo = db.TRN23100.Find(wid);
            if (wo != null && wo.DepartmentID.HasValue)
                allowedActIds = db.ADM04210.Where(x => x.DepartmentID == wo.DepartmentID.Value).Select(x => x.ActivityID).ToList();
            var companyId = db.TRN23100.Find(wid).Wo_client;
            var actsDdl = new List<ConstructorActivityDisplay>();
            if (HttpContext.Session.Get<List<ConstructorActivityDisplay>>("acts") != null)
                actsDdl = HttpContext.Session.Get<List<ConstructorActivityDisplay>>("acts");
            else
            {
                List<int> actsToRemove = new List<int>();
                if (User.IsInRole("Technician") || User.IsInRole("Supervisor"))
                {
                    var deptID = user.DepartmentID;
                    actsToRemove = db.ADM01100.Where(x => x.DepartmentId != deptID && x.Active).Select(x => x.RateID).ToList();
                }
                if (wo != null && wo.DepartmentID.HasValue)
                {
                    if (User.IsInRole("SuperAdmin") || User.IsInRole("AppAdmin") || User.IsInRole("Admin"))
                        actsDdl = db.ADM01100.Where(x => x.ClientID == companyId && allowedActIds.Contains(x.RateID) && x.Active)
                            .Select(x => new ConstructorActivityDisplay { Description = x.RateDescr, ActivityID = x.RateID, MaxQty = x.MaxQty ?? 0 }).ToList();
                    else
                        actsDdl = db.ADM01100.Where(x => x.ClientID == companyId && allowedActIds.Contains(x.RateID) && x.Active)
                            .Select(x => new ConstructorActivityDisplay { Description = x.RateDescr, ActivityID = x.RateID, MaxQty = x.MaxQty ?? 0 }).ToList();
                }
                else
                    actsDdl = db.ADM01100.Where(x => x.ClientID == companyId && x.Active)
                            .Select(x => new ConstructorActivityDisplay { Description = x.RateDescr, ActivityID = x.RateID, MaxQty = x.MaxQty ?? 0 }).ToList();
                //if (actsToRemove.Any())
                //    actsDdl.RemoveAll(x => actsToRemove.Contains(x.ActivityID));
            }

            ViewBag.ActsDdl = actsDdl;
            var acts = invID.HasValue ?
                db.TRN09110.Where(x => x.InvoiceNum == invID.Value && wOIds.Contains(x.TRN23110.WorkOID) && x.TRN23110.ActDate == date && x.TRN23110.Location == location)
                .Select(x => new ConstructorActivity
                {
                    RID = x.WoActID,
                    ActivityAmount = x.InvoicedAmount,
                    ActivityID = x.TRN23110.ActivityID,
                    HasMaterials = x.TRN23110.ADM01100.ADM01110.Any(),
                    MaxQty = x.TRN23110.ADM01100.MaxQty ?? 0,
                    CapturedMaterials = x.TRN23110.TRN23120.Any()
                }).ToList()
                : db.TRN23110.Where(x => x.WorkOID == wid && x.ActDate == date && x.Location == location)
                .Select(x => new ConstructorActivity
                {
                    RID = x.RecID,
                    ActivityAmount = string.IsNullOrEmpty(type) ? x.ActQty : x.OActQty,
                    ActivityID = x.ActivityID,
                    HasMaterials = x.ADM01100.ADM01110.Any(),
                    MaxQty = x.ADM01100.MaxQty ?? 0,
                    CapturedMaterials = x.TRN23120.Any()
                }).ToList();
            return View("ConstructorActivities", acts);
        }

        public JsonResult HasMaterial(int id)
        {
            var act = db.ADM01100.Find(id);
            var hasMaterials = act != null ? act.HasMaterials : false;
            var materialsReq = act != null ? act.MaterialsRequired : false;
            return Json(new { hasMats = hasMaterials, matsReq = materialsReq });
        }
        public JsonResult GetContractors(string query, List<int> ids = null)
        {
            var data = ids != null ? db.ADM03300.Where(x => !ids.Contains(x.ConID) && x.isActive == true) : db.ADM03300.Where(x => x.isActive == true);
            data = string.IsNullOrEmpty(query) ? data : data.Where(x => (x.LastName.Contains(query) || x.FirstName.Contains(query)));
            var cons = data.Where(x => (x.ConClass == 0 || x.ConClass == 1) && x.isActive == true).AsEnumerable().Select(x => new
            {
                term = x.FirstName + " " + x.LastName,
                id = x.ConID,
                imgUrl = CheckIfUserImgExists(x.EmployeeID),
                payScale = x.payScale != null ? x.payScale == 1 ? "Snr" : "Jr" : "Jr",
                payScaleId = x.payScale,
                PlateNo = x.PlateNo,
                Portion = 100,
                TechNo = x.TechNo
            });
            cons = cons.OrderBy(x => x.term);
            return Json(new { query = query, suggestions = cons });
        }
        public ActionResult GetContractorPartial()
        {
            return PartialView("TeamMemberPartial", new TeamMember());
        }
        public JsonResult WorkOrderActivityCount(int id)
        {
            var data = new
            {
                any = db.TRN23110.Any(x => x.WorkOID == id),
                verified = db.TRN23100.FirstOrDefault(x => x.Workid == id).Status.ToLower() == "v"
            };
            return Json(data);
        }
        [HttpPost]
        public JsonResult GetMaxQty(int actId)
        {
            var act = db.ADM01100.Find(actId);
            var hasMaterials = act != null ? act.HasMaterials : false;
            var materialsReq = act != null ? act.MaterialsRequired : false;
            return Json(new
            {
                MaxQty = act.MaxQty ?? 0,
                hasMats = hasMaterials,
                matsReq = materialsReq
            });
        }
        [HttpPost]
        public JsonResult CheckMaxQtyExceeded(int id)
        {
            string message = "a";
            var wo = db.TRN23100.Find(id);
            if (wo != null)
            {
                if (wo.TRN23110.Any(x => x.OActQty > x.ADM01100.MaxQty))
                    message = "a";
                else if (wo.TRN23110.Any(x => x.TRN23120.Any(m => m.WoMatQty > x.ADM01100.ADM01110.FirstOrDefault(am => am.MaterID == m.WoMatID).ActMatQty)))
                {
                    message = "a";
                }
                else
                    message = "";
            }
            return Json(new
            {
                msg = message
            });
        }
        [HttpPost]
        public JsonResult CheckAuthentication(string uName, string pWord)
        {
            string message = "a";
            //if (Membership.ValidateUser(uName, pWord))
            //    if (System.Web.Security.Roles.IsUserInRole(uName, "Technician") && System.Web.Security.Roles.IsUserInRole(uName, "Supervisor"))
            message = "";
            return Json(new
            {
                msg = message
            });
        }



        #endregion

        #region Customs
        public List<WorkOrderReferencePartial> GetRefrencesForMerging(string val, int? id)
        {
            if (id.HasValue)
            {
                var merger = db.TRN13120.Find(id);
                if (merger != null)
                {
                    return db.TRN23100.Where(x => x.Wo_ref == val && (x.Status != "i" || x.TRN13110.Any(m => m.MergedOrderId == id) || x.TRN13120.Any(m => m.MergedOrderId == id))).GroupBy(x => x.Wo_ref).Select(x => new WorkOrderReferencePartial
                    {
                        ReferenceCode = x.Key,
                        Count = x.Count()
                    }).Where(x => x.Count > 1).ToList();
                }
            }
            if (string.IsNullOrEmpty(val))
            {
                return db.TRN23100.Where(x => x.Status != "i" && !db.TRN13120.Select(m => m.MergedRefNum).Contains(x.Wo_ref)).GroupBy(x => x.Wo_ref).Select(x => new WorkOrderReferencePartial
                {
                    ReferenceCode = x.Key,
                    Count = x.Count()
                }).Where(x => x.Count > 1).ToList();
            }
            return db.TRN23100.Where(x => x.Wo_ref.Contains(val) && (x.Status != "i" || x.TRN13110.Any() || x.TRN13120.Any())).GroupBy(x => x.Wo_ref).Select(x => new WorkOrderReferencePartial
            {
                ReferenceCode = x.Key,
                Count = x.Count()
            }).Where(x => x.Count > 1).ToList();
        }
        public List<WorkOrderReferencePartial> GetRefrencesForMerged(string val)
        {
            return db.TRN23100.Where(x => x.Wo_ref.Contains(val) && (x.Status != "i" || x.TRN13110.Any() || x.TRN13120.Any())).GroupBy(x => x.Wo_ref).Select(x => new WorkOrderReferencePartial
            {
                ReferenceCode = x.Key,
                Count = x.Count()
            }).Where(x => x.Count > 1).ToList();
        }
        public int ChangeWorkOrderStatus(int wid, string type)
        {
            var anOrder = db.TRN23100.Find(wid);
            switch (type)
            {
                case "submit":
                    bool canContinue = false;
                    var today = DateTime.Now.Date;
                    if (anOrder.RollbackDate != null && today > anOrder.RollbackDate.Value.AddHours(48))
                        canContinue = true;
                    else
                        canContinue = true;
                    anOrder.Status = "s";
                    anOrder.Submitted = true;
                    if (canContinue)
                    {
                        anOrder.DateSubmitted = today;

                        #region Set Paid Date New

                        // Edited by D I Shyamal
                        var dateSubmitted = anOrder.DateSubmitted.Value.Date;
                        var applicablePeriods = db.ADM16100.Where(x => x.DueDate >= dateSubmitted).OrderBy(x => x.DueDate).Take(2).ToList();
                        if (applicablePeriods.Any())
                        {
                            if (applicablePeriods[0].PeriodStart <= anOrder.Dispatchdt && applicablePeriods[0].PeriodEnd >= anOrder.Dispatchdt)
                                anOrder.PaidDate = applicablePeriods[0].PayDate;
                            else if (applicablePeriods[0].PeriodStart > anOrder.Dispatchdt)
                                anOrder.PaidDate = applicablePeriods[0].PayDate;
                            else
                            {
                                if (applicablePeriods.Count > 1)
                                    anOrder.PaidDate = applicablePeriods[1].PayDate;
                            }
                        }

                        #endregion
                    }
                    break;
                case "verify":
                    anOrder.Status = "v";
                    anOrder.Submitted = true;
                    anOrder.dateVerified = DateTime.Now;
                    break;
                default:
                    anOrder.Status = db.TRN23110.Any(x => x.WorkOID == wid) ? "p" : "n";
                    anOrder.Submitted = false;
                    anOrder.DateSubmitted = null;
                    anOrder.PaidDate = null;
                    break;
            }
            try
            {
                return db.SaveChanges();
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public void SetupWorkOrder(int? classId, TRN23100 wo = null)
        {
            var nowDate = DateTime.Now;
            var classes = db.ADM03500.Where(x => x.StartDate <= nowDate && x.EndDate >= nowDate).ToList();
            if (classId.HasValue)
                if (!classes.Any(x => x.ClassId == classId))
                    classes.Add(db.ADM03500.Find(classId));

            ViewBag.Classes = classes.Select(x => new SelectListItem
            {
                Text = x.ClassName,
                Value = x.ClassId.ToString()
            }).ToList();
            var clnts = db.ADM03200.Where(x => x.StartDt <= DateTime.Now && x.EndDt >= DateTime.Now).Take(10).ToList();
            if (wo != null && !clnts.Any(x => x.CustID == wo.Wo_client))
                clnts.Add(db.ADM03200.Find(wo.Wo_client));
            ViewBag.Clients = clnts.Select(x => new SelectListItem
            {
                Text = x.CustName,
                Value = x.CustID.ToString()
            }).ToList();
            ViewBag.Locations = db.ADM12100.Select(x => new SelectListItem
            {
                Text = x.locationName,
                Value = x.locationID.ToString()
            }).ToList();
            ViewBag.Classifactions = db.ADM03500.Where(x => x.ClassId == 18).Select(x => new SelectListItem
            {
                Selected = (x.ClassName.ToUpper().Contains("HFC INSTALLATIONS")),
                Text = x.ClassName,
                Value = x.ClassId.ToString()
            }).ToList();
            List<SelectListItemExt> contracts = db.ADM03300.Select(x => new SelectListItemExt
            {
                Text = x.FirstName + " " + x.LastName,
                Value = x.ConID.ToString(),
                term = x.FirstName + " " + x.LastName,
                id = x.ConID,
                payScale = x.payScale != null ? x.payScale == 1 ? "Snr" : "Jr" : "Jr",
                PlateNo = x.PlateNo
            }).ToList();
            foreach (var contract in contracts)
            {
                contract.imgUrl = CheckIfUserImgExists(db.ADM03300.Where(x => x.ConID == contract.id).FirstOrDefault().EmployeeID);
            }
            ViewBag.Contractors = contracts.OrderBy(x => x.Text).ToList();
            ViewBag.TeamMax = Customs.GetSettingsFileValue("TeamMax", "");
            List<SelectListItem> vehicals = db.ADM22100.Select(x => new SelectListItem
            {
                Text = x.PlateNo + " : Owned Company",
                Value = x.PlateNo.ToString()
            }).ToList();
            ViewBag.Vehicles = vehicals;
            var dic = new Dictionary<string, string> { { "Remark1", "Remark1" }, { "Remark2", "Remark2" } };
            ViewBag.Remarks = dic.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Value
            }).ToList();

            ViewBag.Areas = db.ADM01400.OrderBy(x => x.ADM26100.Name).Select(x => new SelectListItem
            {
                Text = x.areaName + " (" + x.ADM26100.Name + ")",
                Value = x.areaID.ToString()
            }).ToList();
            var dbPOes = db.ADM16200.OrderBy(x => x.PONUM).ToList();
            var deps = new List<SelectListItem>();
            //if (!System.Web.Security.Roles.IsUserInRole("Technician") && !System.Web.Security.Roles.IsUserInRole("Supervisor"))
            //{
            //    ViewBag.POes = dbPOes.Where(x => !x.IsClosed).OrderBy(x => x.PONUM).Select(x => new SelectListItem
            //    {
            //        Text = x.PONUM + " - " + string.Format("{0:C}", x.BALANCE),
            //        Value = x.PONUM
            //    }).ToList();
            //    deps = db.ADM04200.OrderBy(x => x.Name).Select(x => new SelectListItem
            //    {
            //        Text = x.Name,
            //        Value = x.DepartmentID.ToString()
            //    }).ToList();
            //}
            //else
            //{
            ViewBag.POes = dbPOes.Where(x => !x.IsClosed).OrderBy(x => x.PONUM).Select(x => new SelectListItem
            {
                Text = x.PONUM,
                Value = x.PONUM
            }).ToList();
            var user = db.ADM03300.FirstOrDefault(x => x.Email == User.Identity.Name);
            var dbDeps = db.ADM04200.OrderBy(x => x.Name).ToList();
            deps = dbDeps.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.DepartmentID.ToString(),
                Selected = x.DepartmentID == (user != null ? user.DepartmentID : -1)
            }).ToList();
            //}

            ViewBag.Departments = deps;
        }
        public string CheckIfUserImgExists(string eid)
        {
            try
            {
                return System.IO.File.Exists(System.IO.Path.Combine(_env.WebRootPath, "images", "technicians/" + eid + ".jpg")) ?
                    "../images/technicians/" + eid + ".jpg" : "../images/icons/default-user.png";
            }
            catch { return ""; }
        }
        private TeliconUserFull GetLoggedUser()
        {
            var id = User.Identity.Name;
            var role = "";// System.Web.Security.Roles.GetRolesForUser(id)[0];
            var user = new { };// Membership.GetUser(id);
            var tuser = new TeliconUserFull();
            if (user != null)
            {
                tuser = new TeliconUserFull
                {
                    ProfileFullInfo = role == "Technician" || role == "Supervisor"
                        ? db.ADM03300.Where(x => x.Email == id).Select(x => new ProfileFull
                        {
                            ProfileID = x.ConID,
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            AltPhone = x.Phone2,
                            Phone = x.Phone1,
                            LocationID = x.LocationID ?? 0,
                            AreaID = x.AreaID ?? 0
                        }).FirstOrDefault()
                        : null, //AppProfile.GetProfile(id).ProfileInfo,
                    Role = role,
                    OldRole = role,
                    Email = "",// user.Email,
                    UserName = id,
                    IsNew = false,
                    IsApproved = false,// user.IsApproved,
                    OldUserName = id
                };
            }
            return tuser;
        }

        public double GetWorkOrderSubTotal(int wId)
        {
            try
            {
                if (db.TRN23110.Any(x => x.WorkOID == wId))
                {
                    var query = (from o in db.TRN23100
                                 join wi in db.TRN23110 on o.Workid equals wi.WorkOID
                                 into grouping
                                 where o.Workid == wId
                                 select new
                                 {
                                     WORef = o.Wo_ref,
                                     TotalAmount =
                                         (from g in grouping
                                          join a in db.ADM01100 on g.ActivityID equals a.RateID
                                          join r in db.ADM01150 on a.RateID equals r.RateID
                                          where r.StartDate <= g.ActDate && r.EndDate >= g.ActDate
                                          select new
                                          {
                                              Amount = g != null && r != null ? (((double)g.OActQty) * r.Amount) : 0
                                          }).Sum(x => x.Amount)
                                 }).FirstOrDefault();
                    return query.TotalAmount;
                }
                else
                    return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        [HttpPost]
        public JsonResult GetRequestedBy(int areaId)
        {
            var area = db.ADM01400.Find(areaId);
            string sup = "";
            if (area != null)
                sup = area.ADM26100 != null ? area.ADM26100.SupervisorName : "";
            return Json(new JsonReturnParams
            {
                Additional = sup,
                Code = "300",
                Msg = ""
            });
        }
        #endregion
    }
}
