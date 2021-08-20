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
using System.Threading.Tasks;
using System.Transactions;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;
using TeliconLatest.Reusables;

namespace TeliconLatest.Controllers
{
    public class QuotationController : Controller
    {
        private readonly TeliconDbContext db;
        private readonly IWebHostEnvironment _env;
        public QuotationController(TeliconDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            _env = env;
        }
        // GET: Quotation
        //[TeliconAuthorize(TaskId = 20)]
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
        //[TeliconAuthorize(TaskId = 20)]
        public ActionResult Create(string type)
        {
            var model = new TRN17100();
            model.Requestdt = DateTime.Now.Date;

            return View("CreateOrUpdate", model);
        }
        //[TeliconAuthorize(TaskId = 20)]
        public ActionResult Edit(int id, string type)
        {
            return View("CreateOrUpdate", db.TRN17100.Find(id));
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[TeliconAuthorize(TaskId = 20, Mode = ActionMode.Write)]
        public async Task<JsonResult> CreateOrUpdateAsync(TRN17100 model)
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

                model.Quot_ref = model.Quot_ref.TrimStart().TrimEnd();
                model.Quot_title = model.Quot_title.TrimStart().TrimEnd().ToUpper();
                model.Requestby = model.Requestby.TrimStart().TrimEnd();

                if (model.QuotationId == 0)
                {
                    model.CreateBy = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.PrimarySid).Value;
                    model.Quot_title = model.Quot_title.ToUpper();
                    model.Quot_ref = model.Quot_ref.ToUpper();
                    db.TRN17100.Add(model);
                    db.SaveChanges();
                }
                else
                {
                    var order = db.TRN17100.Find(model.QuotationId);
                    order.Quot_title = model.Quot_title.ToUpper();
                    order.Quot_ref = model.Quot_ref.ToUpper();
                    order.Requestby = model.Requestby;
                    order.RequestStreet = model.RequestStreet;
                    order.RequestCity = model.RequestCity;
                    order.RequestCountry = model.RequestCountry;
                    order.Requestdt = model.Requestdt;

                    if (await TryUpdateModelAsync(order, "", s => s.Quot_ref, s => s.Requestby, s => s.Quot_title, s => s.Requestdt, s => s.RequestStreet, s => s.RequestCity, s => s.RequestCountry))
                    {
                        db.SaveChanges();
                    }
                }
                return Json(new JsonReturnParams
                {
                    Additional = model.QuotationId,
                    Code = "100",
                    Msg = ""
                });
            }
            catch (DbUpdateException e)
            {
                return Json(new JsonReturnParams
                {
                    Additional = e.StackTrace,
                    Code = model.QuotationId == 0 ? "200" : "300",
                    Msg = e.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 20, Mode = ActionMode.Write)]
        public JsonResult Delete(string id)
        {
            int qid = Convert.ToInt32(id);
            db.Database.OpenConnection();
            int saved = -1;
            using TransactionScope ts = new TransactionScope();
            try
            {
                if (db.TRN17110.Any(x => x.QuotationId == qid))
                {
                    db.TRN17110.RemoveRange(db.TRN17110.Where(x => x.QuotationId == qid));
                    saved = db.SaveChanges();
                }
                db.TRN17100.Remove(db.TRN17100.Find(qid));
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

        public ActionResult GenerateConstructor(int? qid)
        {
            int wid = qid.Value;

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
            var acts = db.TRN17110.Where(x => x.QuotationId == qid)
                .Select(x => new ConstructorQuotationActivity
                {
                    RecID = x.RecId,
                    ActivityAmount = x.ActQty,
                    ActivityDesc = x.ActivityDesc,
                    Rate = x.ActivityRate ?? 0
                }).ToList();
            foreach (var act in acts)
            {
                if (actsDdl.Find(x => x.Description == act.ActivityDesc) == null)
                    actsDdl.Add(new ConstructorActivityDisplay { Description = act.ActivityDesc, ActivityID = 0, MaxQty = 0 });
            }
            ViewBag.ActsDdl = actsDdl;
            string[] strArray = new string[1];
            strArray[0] = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value;
            var model = db.TRN17100.AsEnumerable().Where(x => x.QuotationId == qid).Select(x => new QConstructorPartial
            {
                QuotationId = qid ?? 0,
                QConstructorActivities = acts,
                UserRole = strArray,
                Title = x.Quot_title.ToUpper()
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
        //[TeliconAuthorize(TaskId = 20, Mode = ActionMode.Write)]
        public JsonResult UpdateActivities(List<QuotationRecordSet> model)
        {
            db.Database.OpenConnection();
            int saved = -1;
            using TransactionScope ts = new TransactionScope();
            try
            {
                foreach (var rec in model)
                {
                    var actIDs = rec.Activities.Where(x => x.RecID != 0 && x.Qty > 0).Select(x => x.RecID);
                    foreach (var activity in db.TRN17110.Where(x => actIDs.Contains(x.RecId)))
                    {
                        var act = rec.Activities.FirstOrDefault(x => x.RecID == activity.RecId);
                        activity.ActivityDesc = act.ActDescr;
                        activity.ActQty = act.Qty;
                        activity.ActivityRate = act.Rate;
                    }
                    foreach (var activity in rec.Activities.Where(x => x.RecID == 0 && x.Qty > 0))
                    {
                        if (db.TRN17110.Where(x => x.QuotationId == rec.QID && x.ActivityDesc == activity.ActDescr).Any())
                        {
                            var record = db.TRN17110.FirstOrDefault(x => x.QuotationId == rec.QID && x.ActivityDesc == activity.ActDescr);
                            record.ActQty = activity.Qty;
                            record.ActivityRate = activity.Rate;
                        }
                        else
                        {
                            var newAct = new TRN17110
                            {
                                ActQty = activity.Qty,
                                QuotationId = rec.QID,
                                ActivityDesc = activity.ActDescr,
                                ActivityRate = activity.Rate
                            };
                            db.TRN17110.Add(newAct);
                            saved = db.SaveChanges();
                        }
                    }
                    foreach (var activity in rec.Activities.Where(x => x.Qty <= 0))
                    {
                        var recAct = db.TRN17110.Find(activity.RecID);
                        db.TRN17110.Remove(recAct);
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
        //[TeliconAuthorize(TaskId = 20, Mode = ActionMode.Write)]
        public JsonResult DeleteActivityFromQuotation(string val)
        {
            db.Database.OpenConnection();
            using TransactionScope ts = new TransactionScope();
            try
            {
                int recId = Convert.ToInt32(val);
                db.TRN17110.Remove(db.TRN17110.Find(recId));
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
        public JsonResult GetRate(string act)
        {
            int actId = -1;
            int.TryParse(act, out actId);
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

        public ActionResult Quotation(int id)
        {
            ViewBag.Id = id;
            return View(QuotationData(id));
        }

        [HttpPost]
        public async Task<IActionResult> QuotationToExcel(QuotationFilter model)
        {
            await Task.Yield();
            var stream = new MemoryStream();
            ExcelPackage pck = new ExcelPackage(stream);
            string fileName = "Quotation";
            var quot = QuotationData(model.id);
            if (quot != null)
            {
                #region Work Sheet                
                var ws = pck.Workbook.Worksheets.Add(quot.Client.ReferenceNo);
                fileName += " - " + quot.Client.ReferenceNo;
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
                ws.Cells["B" + y].Value = quot.Client.ClientName;

                ws.Cells["D" + y].Value = "Date:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = string.Format("{0:dd/MM/yyyy}", quot.Client.Date);
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Address:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = quot.Client.Street;

                ws.Cells["D" + y].Value = "Ticket #:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = quot.Client.ReferenceNo;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = quot.Client.City;

                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = quot.Client.Country;

                ws.Cells["D" + y].Value = "";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = "";
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                #endregion
                y++;
                y++;


                #region Title
                ws.Cells[y, 1, y, 5].Merge = true;
                ws.Cells["A" + y].Value = quot.Title.ToUpper();
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                #endregion
                y++;

                #region Table Headers
                ws.Cells["A" + y].Value = "Item No.";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Description";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Qty";
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


                int count = 1;
                foreach (var act in quot.Activities)
                {
                    #region Rows
                    ws.Cells["A" + y].Value = count;
                    ws.Cells["B" + y].Value = act.ActivityDesc;
                    ws.Cells["C" + y].Value = act.ActivityQty;
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


                var total = quot.Activities.Sum(x => x.ActivityCost * Convert.ToDouble(x.ActivityQty));
                
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

                #endregion
            }
            if (pck.Workbook.Worksheets.Count() > 0)
            {
                pck.Save();
                //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //Response.AddHeader("content-disposition", "attachment;  filename=" + fileName + ".xlsx");
            }
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName + ".xlsx");
        }

        public Quotation QuotationData(int id)
        {
            var refNum = db.TRN17100.Find(id).Quot_ref;
            var activities = db.TRN17110.Where(x => x.QuotationId == id).AsEnumerable().Select(x => new QuotationActivities
            {
                ActivityCost = x.ActivityRate ?? 0,
                ActivityDesc = x.ActivityDesc,
                ActivityQty = x.ActQty
            }).ToList();
            var quotation = db.TRN17100.Where(x => x.QuotationId == id).AsEnumerable().Select(x => new Quotation
            {
                Title = x.Quot_title.ToUpper(),
                Client = new RequesterData
                {
                    ReferenceNo = x.Quot_ref,
                    Date = x.Requestdt,
                    ClientName = x.Requestby,
                    Street = x.RequestStreet,
                    City = x.RequestCity,
                    Country = x.RequestCountry
                }
            }).FirstOrDefault();
            quotation.Activities = activities;
            return quotation;
        }
    }
}