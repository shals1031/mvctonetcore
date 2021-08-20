using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Transactions;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;
using TeliconLatest.Reusables;

namespace TeliconLatest.Controllers
{
    public class StandbyInvoiceController : Controller
    {
        private readonly TeliconDbContext db;
        private readonly IWebHostEnvironment _env;
        public StandbyInvoiceController(TeliconDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            _env = env;
        }
        // GET: StandbyInvoice
        //[TeliconAuthorize(TaskId = 41)]
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
            return View();
        }
        //[TeliconAuthorize(TaskId = 41)]
        public ActionResult Create(string type)
        {
            var model = new TRN19100();
            model.Requestdt = DateTime.Now;
            ViewBag.Zones = db.ADM26100.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.ZoneID.ToString()
            }).ToList();
            return View("CreateOrUpdate", model);
        }
        //[TeliconAuthorize(TaskId = 41)]
        public ActionResult Edit(int id, string type)
        {
            ViewBag.Zones = db.ADM26100.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.ZoneID.ToString()
            }).ToList();
            return View("CreateOrUpdate", db.TRN19100.Find(id));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 41, Mode = ActionMode.Write)]
        public async System.Threading.Tasks.Task<JsonResult> CreateOrUpdateAsync(TRN19100 model)
        {
            try
            {
                if (model.Requestdt > DateTime.Now)
                    return Json(new JsonReturnParams
                    {
                        Additional = "Cannot use future date as Request Date.",
                        Code = "999",
                        Msg = "Cannot use future date as Request Date."
                    });

                model.SInv_title = model.SInv_title.TrimStart().TrimEnd().ToUpper();

                if (model.SInvoiceId == 0)
                {
                    model.CreateBy = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.PrimarySid).Value;
                    model.SInv_title = model.SInv_title.ToUpper();
                    db.TRN19100.Add(model);
                    db.SaveChanges();
                }
                else
                {
                    var order = db.TRN19100.Find(model.SInvoiceId);
                    order.SInv_title = model.SInv_title.ToUpper();
                    order.Requestdt = model.Requestdt;

                    if (await TryUpdateModelAsync(order, "", x => x.SInv_ref, x => x.ZoneId, x => x.SInv_title, x => x.Requestdt))
                    {
                        db.SaveChanges();
                    }
                }
                return Json(new JsonReturnParams
                {
                    Additional = model.SInvoiceId,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (DbUpdateException e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.SInvoiceId == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 41, Mode = ActionMode.Write)]
        public JsonResult Delete(string id)
        {
            int sIid = Convert.ToInt32(id);
            db.Database.OpenConnection();
            int saved = -1;
            using TransactionScope ts = new TransactionScope();
            try
            {
                if (db.TRN19110.Any(x => x.SInvoiceId == sIid))
                {
                    db.TRN19110.RemoveRange(db.TRN19110.Where(x => x.SInvoiceId == sIid));
                    saved = db.SaveChanges();
                }
                db.TRN19100.Remove(db.TRN19100.Find(sIid));
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

        public ActionResult GenerateConstructor(int? sIid)
        {
            int wid = sIid.Value;

            var user = db.ADM03300.FirstOrDefault(x => x.Email == User.Identity.Name);
            var actsDdl = new List<ConstructorActivityDisplay>();
            if (HttpContext.Session.Get<List<ConstructorActivityDisplay>>("acts") != null)
                actsDdl = HttpContext.Session.Get<List<ConstructorActivityDisplay>>("acts");
            else
            {
                List<int> actsToRemove = new List<int>();
                if (User.IsInRole("Technician") || User.IsInRole("Supervisor"))
                {
                    var deptID = user.DepartmentID;
                    actsToRemove = db.ADM01100.Where(x => x.ADM01110.Any(a => a.ADM01100.DepartmentId != deptID && a.ADM01100.Active)).Select(x => x.RateID).ToList();
                }
                actsDdl = db.ADM01100.Select(x => new ConstructorActivityDisplay { Description = x.RateDescr, ActivityID = x.RateID, MaxQty = x.MaxQty ?? 0 }).ToList();
            }
            var acts = db.TRN19110.Where(x => x.SInvoiceId == sIid)
                .Select(x => new ConstructorSInvoiceActivity
                {
                    RecID = x.RecId,
                    ActivityAmount = x.ActQty,
                    ActivityId = x.ActivityId,
                    Rate = x.ActivityRate ?? 0
                }).ToList();

            ViewBag.ActsDdl = actsDdl;
            string[] strArray = new string[1];
            strArray[0] = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value;
            var model = db.TRN19100.AsEnumerable().Where(x => x.SInvoiceId == sIid).Select(x => new SIConstructorPartial
            {
                SInvoiceId = sIid ?? 0,
                SIConstructorActivities = acts,
                UserRole = strArray,
                Title = x.SInv_title.ToUpper()
            }).FirstOrDefault();
            return PartialView("Constructor", model);
        }
        [HttpPost]
        public JsonResult GetActivities()
        {
            var acts = db.ADM01100.Select(x => new
            {
                activityID = x.RateID,
                description = x.RateDescr,
                maxQty = x.MaxQty
            }).ToList();
            return Json(acts);
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 41, Mode = ActionMode.Write)]
        public JsonResult UpdateActivities(List<SInvoiceRecordSet> model)
        {
            db.Database.OpenConnection();
            int saved = -1;
            using TransactionScope ts = new TransactionScope();
            try
            {
                foreach (var rec in model)
                {
                    var actIDs = rec.Activities.Where(x => x.RecID != 0 && x.Qty > 0).Select(x => x.RecID);
                    foreach (var activity in db.TRN19110.Where(x => actIDs.Contains(x.RecId)))
                    {
                        var act = rec.Activities.FirstOrDefault(x => x.RecID == activity.RecId);
                        activity.ActivityId = act.ActId;
                        activity.ActQty = act.Qty;
                        activity.ActivityRate = act.Rate;
                    }
                    foreach (var activity in rec.Activities.Where(x => x.RecID == 0 && x.Qty > 0))
                    {
                        if (db.TRN19110.Where(x => x.SInvoiceId == rec.SIID && x.ActivityId == activity.ActId).Any())
                        {
                            var record = db.TRN19110.FirstOrDefault(x => x.SInvoiceId == rec.SIID && x.ActivityId == activity.ActId);
                            record.ActQty = activity.Qty;
                            record.ActivityRate = activity.Rate;
                        }
                        else
                        {
                            var newAct = new TRN19110
                            {
                                ActQty = activity.Qty,
                                SInvoiceId = rec.SIID,
                                ActivityId = activity.ActId,
                                ActivityRate = activity.Rate
                            };
                            db.TRN19110.Add(newAct);
                            saved = db.SaveChanges();
                        }
                    }
                    foreach (var activity in rec.Activities.Where(x => x.Qty <= 0))
                    {
                        var recAct = db.TRN19110.Find(activity.RecID);
                        db.TRN19110.Remove(recAct);
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
        //[TeliconAuthorize(TaskId = 41, Mode = ActionMode.Write)]
        public JsonResult DeleteActivityFromStandbyInvoice(string val)
        {
            db.Database.OpenConnection();
            using TransactionScope ts = new TransactionScope();
            try
            {
                int recId = Convert.ToInt32(val);
                db.TRN19110.Remove(db.TRN19110.Find(recId));
                int saved = db.SaveChanges();
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
        public JsonResult GetRate(int act)
        {
            int actId = act;
            var a = db.ADM01100.FirstOrDefault(x => x.RateID == actId && x.ADM01150.Any());
            if (a != null)
                return Json(new
                {
                    MaxQty = a.MaxQty ?? 0,
                    Rate = a.ADM01250.Any() ? a.ADM01250.OrderByDescending(x => x.RateHistoryID).FirstOrDefault().Amount : 0
                });
            else
                return Json(new
                {
                    MaxQty = 10000,
                    Rate = 0
                });
        }

        public ActionResult StandbyInvoice(int id)
        {
            ViewBag.Id = id;
            return View(SInvoiceData(id));
        }

        [HttpPost]
        public void StandbyInvoiceToExcel(SInvoiceFilter model)
        {
            ExcelPackage pck = new ExcelPackage();
            string fileName = "Standby Invoice";
            var sInv = SInvoiceData(model.id);
            if (sInv != null)
            {
                #region Work Sheet                
                var ws = pck.Workbook.Worksheets.Add(sInv.Client.ReferenceNo);
                fileName += " - " + sInv.Client.ReferenceNo;
                #region View and Print Seetings

                ws.PrinterSettings.ShowGridLines = false;

                ws.PrinterSettings.PaperSize = ePaperSize.A4;
                ws.PrinterSettings.TopMargin = 0.75M;
                ws.PrinterSettings.RightMargin = 0.25M;
                ws.PrinterSettings.BottomMargin = 0.75M;
                ws.PrinterSettings.LeftMargin = 0.25M;
                ws.PrinterSettings.HeaderMargin = 0.0M;
                ws.PrinterSettings.FooterMargin = 0.0M;

                #endregion

                ws.Cells[1, 1, 14, 5].Style.Font.Size = 10;

                ws.Column(1).Width = 11.71;
                ws.Column(2).Width = 8.43;
                ws.Column(3).Width = 48.14;
                ws.Column(4).Width = 12.29;
                ws.Column(5).Width = 15.57;

                int y = 1;

                #region Header
                string logoSearchPath = "/images/print-logo.png";
                if (Directory.Exists(Path.Combine(_env.WebRootPath, "work")))
                    logoSearchPath = "/work/images/print-logo.png";
                string logoPath = Path.Combine(_env.WebRootPath, logoSearchPath);
                FileInfo fi = new FileInfo(logoPath);
                var image = ws.Drawings.AddPicture("print-logo.png", fi);
                image.SetPosition(0, 0);
                image.SetSize(205, 111);

                ws.Cells["C" + y].Value = "P.O. BOX 3069";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["C" + y].Value = "Kingston 8";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["C" + y].Value = "Tel: (876) 630-5278";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["C" + y].Value = "Email: admin@telicongroup.com";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                y++;
                y++;
                y++;

                ws.Cells["A" + y].Value = "Co. Name:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = sInv.Client.ClientName;

                ws.Cells["D" + y].Value = "Date:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = string.Format("{0:dd/MM/yyyy}", sInv.Client.Date);
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Attn:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = sInv.Client.Attn;

                ws.Cells["D" + y].Value = "Prepared by:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = sInv.Client.PreparedBy;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Address:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = sInv.Client.Addr;

                ws.Cells["D" + y].Value = "Invoice #:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = sInv.Client.ReferenceNo;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Phone:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = sInv.Client.Phone;

                ws.Cells["D" + y].Value = "GCT #:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = "001-829-874";
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Email:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = sInv.Client.Email;

                ws.Cells["D" + y].Value = "Currency:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = "JMD";
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                #endregion
                y++;
                y++;

                #region Table Headers
                ws.Cells["A" + y].Value = "Date";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Qty";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Description";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Value = "Rate";
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = "Amount";
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 5].Style.Font.Size = 12;
                ws.Cells[y, 1, y, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, 5].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                #endregion
                y++;

                #region Rows
                ws.Cells["A" + y].Value = "";
                ws.Cells["B" + y].Value = "";
                ws.Cells["C" + y].Value = "Standby";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["D" + y].Value = "";
                ws.Cells["E" + y].Value = "";

                #region Row Styles
                ws.Cells["D" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));
                ws.Cells["E" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                if (y % 2 != 0)
                {
                    ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                #endregion

                #endregion
                y++;
                foreach (var act in sInv.Activities)
                {
                    #region Rows
                    ws.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", sInv.Client.Date);
                    ws.Cells["B" + y].Value = act.ActivityQty;
                    ws.Cells["C" + y].Value = act.ActivityDesc;
                    ws.Cells["D" + y].Value = string.Format("{0:C}", act.ActivityCost);
                    ws.Cells["E" + y].Value = string.Format("{0:C}", act.ActivityCost * Convert.ToDouble(act.ActivityQty));

                    #region Row Styles
                    ws.Cells["D" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));
                    ws.Cells["E" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                    if (y % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    #endregion
                    y++;
                }


                var total = sInv.Activities.Sum(x => x.ActivityCost * Convert.ToDouble(x.ActivityQty));
                var gct = total / 100 * Convert.ToDouble(sInv.GCTRate);
                var grandTotal = total + gct;

                #region Total
                ws.Cells[y, 1, y, 4].Merge = true;
                ws.Cells["A" + y].Value = "Total:";
                ws.Cells[y, 1, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = string.Format("{0:C}", total);
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 5].Style.Font.Color.SetColor(Color.FromArgb(165, 0, 33));
                ws.Cells[y, 1, y, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                if (y % 2 != 0)
                {
                    ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                y++;
                #endregion

                #region GCT
                ws.Cells[y, 1, y, 4].Merge = true;
                ws.Cells["A" + y].Value = "GCT:";
                ws.Cells[y, 1, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = string.Format("{0:C}", gct);
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 5].Style.Font.Color.SetColor(Color.FromArgb(165, 0, 33));
                ws.Cells[y, 1, y, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                if (y % 2 != 0)
                {
                    ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                y++;
                #endregion

                #region Grand Total
                ws.Cells[y, 1, y, 4].Merge = true;
                ws.Cells["A" + y].Value = "Grand Total:";
                ws.Cells[y, 1, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = string.Format("{0:C}", grandTotal);
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 5].Style.Font.Color.SetColor(Color.FromArgb(165, 0, 33));
                ws.Cells[y, 1, y, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                if (y % 2 != 0)
                {
                    ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                y++;
                #endregion

                #endregion
            }
            if (pck.Workbook.Worksheets.Count() > 0)
            {
                pck.Save();
                //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //Response.AddHeader("content-disposition", "attachment;  filename=" + fileName + ".xlsx");
            }
        }

        public SInvoice SInvoiceData(int id)
        {
            string user = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var refNum = db.TRN19100.Find(id).SInv_ref;
            var activities = db.TRN19110.Where(x => x.SInvoiceId == id).AsEnumerable().Select(x => new SInvoiceActivities
            {
                ActivityCost = x.ActivityRate ?? 0,
                ActivityId = x.ActivityId,
                ActivityDesc = x.ADM01100.RateDescr,
                ActivityQty = x.ActQty
            }).ToList();
            var sInvoice = db.TRN19100.Where(x => x.SInvoiceId == id).AsEnumerable().Select(x => new SInvoice
            {
                Title = x.SInv_title.ToUpper(),
                Client = new SInvoiceRequesterData
                {
                    ReferenceNo = Customs.MakeGenericInvoiceNo(x.SInvoiceId).ToString(),
                    Date = x.Requestdt,
                    Addr = x.ADM26100.Addr,
                    Attn = x.ADM26100.SupervisorName,
                    ClientName = db.ADM03200.OrderByDescending(c => c.CustID).FirstOrDefault().CustName,
                    Email = x.ADM26100.Email,
                    Phone = x.ADM26100.Phone
                }
            }).FirstOrDefault();
            sInvoice.Client.PreparedBy = user;
            sInvoice.GCTRate = db.ADM07100.FirstOrDefault(x => x.StartDate <= sInvoice.Client.Date && (x.EndDate != null ? x.EndDate : DateTime.Now) >= sInvoice.Client.Date).Percentage;
            sInvoice.Activities = activities;
            return sInvoice;
        }
    }
}