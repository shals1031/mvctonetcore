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
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly TeliconDbContext db;
        private readonly IWebHostEnvironment _env;
        private readonly string settingFilePath = string.Empty;

        public InvoiceController(TeliconDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            _env = env;
            settingFilePath = Path.Combine(_env.WebRootPath, "settings.xml");
        }

        //[TeliconAuthorize(TaskId = 16)]
        public ActionResult Index()
        {
            List<DateTime> dateTimes = db.TRN23100.Select(x => x.Requestdt).ToList();
            var minYear = dateTimes.Min(x => x).Year;
            var maxYear = dateTimes.Max(x => x).Year;
            var yearsForSelection = new List<SelectListItem>();
            for (int year = minYear; year <= maxYear; year++)
            {
                yearsForSelection.Add(new SelectListItem { Text = year.ToString(), Value = year.ToString(), Selected = year == maxYear });
            }
            ViewBag.Years = yearsForSelection;
            var statuses = DataDictionaries.InvoiceStatuses.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key.ToString()
            }).ToList();
            statuses.Insert(0, new SelectListItem { Value = "a", Text = "All" });
            ViewBag.Statuses = statuses;
            return View();
        }
        //[TeliconAuthorize(TaskId = 17)]
        public ActionResult Summary()
        {
            ViewBag.Companies = db.ADM03200.Select(x => new SelectListItem
            {
                Text = x.ClientCode,
                Value = x.CustID.ToString()
            }).ToList();

            ViewBag.Years = db.TRN09100.Select(x => x.InvoiceDate.Year).Distinct().Select(x => new SelectListItem
            {
                Text = x.ToString(),
                Value = x.ToString()
            }).ToList();
            return View();
        }
        //[TeliconAuthorize(TaskId = 18)]
        public ActionResult Batch()
        {
            List<DateTime> dateTimes = db.TRN23100.Select(x => x.Requestdt).ToList();
            var minYear = dateTimes.Min(x => x).Year;
            var maxYear = dateTimes.Max(x => x).Year;
            var yearsForSelection = new List<SelectListItem>();
            for (int year = minYear; year <= maxYear; year++)
            {
                yearsForSelection.Add(new SelectListItem { Text = year.ToString(), Value = year.ToString(), Selected = year == maxYear });
            }
            ViewBag.Years = yearsForSelection;
            return View();
        }
        //[TeliconAuthorize(TaskId = 19)]
        public ActionResult MultipleInvoice()
        {
            ViewBag.Companies = db.ADM03200.Select(x => new SelectListItem
            {
                Text = x.ClientCode,
                Value = x.CustID.ToString()
            }).ToList();

            ViewBag.Years = db.TRN09100.Select(x => x.InvoiceDate.Year).Distinct().Select(x => new SelectListItem
            {
                Text = x.ToString(),
                Value = x.ToString()
            }).ToList();
            return View();
        }

        [HttpPost]
        //[TeliconAuthorize(TaskId = 18, Mode = ActionMode.Write)]
        public JsonResult BatchInvoices(List<int> ids)
        {
            db.Database.OpenConnection();
            int saved = -1;
            using TransactionScope ts = new TransactionScope();
            try
            {
                var batch = new ADM02300
                {
                    BatchDate = DateTime.Now
                };
                db.ADM02300.Add(batch);
                db.SaveChanges();
                foreach (var id in ids)
                {
                    var invoice = db.TRN09100.Find(id);
                    if (invoice != null)
                    {
                        invoice.BatchId = batch.BatchID;
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
                    Additional = saved,
                    Code = "400",
                    Msg = e.InnerException.InnerException.Message
                });
            }
        }
        [HttpPost]
        //[TeliconAuthorize(TaskId = 18, Mode = ActionMode.Write)]
        public JsonResult DeleteBatch(int id)
        {
            db.Database.OpenConnection();
            int saved = -1;
            using TransactionScope ts = new TransactionScope();
            try
            {
                foreach (var invoice in db.TRN09100.Where(x => x.BatchId.Value == id))
                {
                    invoice.BatchId = null;
                }
                var batch = db.ADM02300.Find(id);
                db.ADM02300.Remove(batch);
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
        [HttpPost]
        public JsonResult GetSummaryPartialByDay(SummaryModelForDay model)
        {
            string order = Customs.GetSortString(model.order, model.columns);
            var data = db.TRN09100.Where(x => !x.BatchId.HasValue && x.TRN09110.Any(y => y.TRN23110.TRN23100.ADM03200.CustID == model.clientID &&
                x.InvoiceDate == model.date));
            return Json(new DataTableReturn
            {
                draw = model.draw,
                recordsFiltered = data.Count(),
                recordsTotal = data.Count(),
                data = Extensions.OrderByDynamic(data, order.Split(" ")[0], order.Split(" ")[1] != "asc").Skip(model.start).Take(model.length).ToList().Select(x => new
                {
                    DT_RowId = "tr-" + x.InvoiceNum,
                    InvoiceNum = Customs.MakeGenericInvoiceNo(x.InvoiceNum),
                    InvoiceTitle = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_title.ToUpper(),
                    InvoiceTotal = string.Format("{0:C}", x.InvoiceTotal.Value),
                    InvoiceDate = string.Format("{0:dd/MM/yyyy}", x.InvoiceDate)
                }).AsQueryable().ToStringArray()
            });
        }
        [HttpPost]
        public JsonResult GetSummaryPartialByRange(SummaryModelForRange model)
        {
            string order = Customs.GetSortString(model.order, model.columns);
            string format = "ddd MMMM dd, yyyy";
            DateTime dFrom = DateTime.ParseExact(model.from, format, System.Globalization.CultureInfo.InvariantCulture);
            DateTime dTo = DateTime.ParseExact(model.to, format, System.Globalization.CultureInfo.InvariantCulture);
            var data = db.TRN09100.Where(x => !x.BatchId.HasValue &&
                x.TRN09110.Any(y => y.TRN23110.TRN23100.ADM03200.CustID == model.clientID &&
                x.InvoiceDate >= dFrom && x.InvoiceDate <= dTo));
            return Json(new DataTableReturn
            {
                draw = model.draw,
                recordsFiltered = data.Count(),
                recordsTotal = db.ADM02300.Count(),
                data = Extensions.OrderByDynamic(data, order.Split(" ")[0], order.Split(" ")[1] != "asc").Skip(model.start).Take(model.length).ToList().Select(x => new
                {
                    DT_RowId = "tr-" + x.InvoiceNum,
                    InvoiceNum = Customs.MakeGenericInvoiceNo(x.InvoiceNum),
                    InvoiceTitle = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_title.ToUpper(),
                    InvoiceTotal = string.Format("{0:C}", x.InvoiceTotal.Value),
                    InvoiceDate = string.Format("{0:dd/MM/yyyy}", x.InvoiceDate)
                }).AsQueryable().ToStringArray()
            });
        }
        [HttpPost]
        public ActionResult SummaryPrintOut(List<int> invIDs, int clientId)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var printData = db.TRN09100.Where(x => invIDs.Contains(x.InvoiceNum) && x.Status == "o").AsEnumerable().Select(x => new SummaryPrintOut
            {
                InvoiceID = Customs.MakeGenericInvoiceNo(x.InvoiceNum),
                GCT = Convert.ToDecimal(((db.ADM07100.FirstOrDefault(x1 => x1.StartDate >= x.InvoiceDate && x1.EndDate <= x.InvoiceDate) == null ? db.ADM07100.FirstOrDefault(x2 => x2.EndDate == null).Percentage : db.ADM07100.FirstOrDefault(x1 => x1.StartDate >= x.InvoiceDate && x1.EndDate <= x.InvoiceDate).Percentage) / 100) * Convert.ToDouble(x.InvoiceTotal.Value)),
                RefNo = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_ref,
                Title = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_title.ToUpper(),
                Total = x.InvoiceTotal.Value,
                Contractors = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM03400.Where(yx => yx.CrewLead == true).Select(y => y.ADM03300.FirstName.Substring(0, 1) + ". " + y.ADM03300.LastName).ToList()
            }).ToList();
            var summary = new SummaryPrintOutWithClient
            {
                Client = db.ADM03200.Where(x => x.CustID == clientId).ToList().Select(x => new ClientData
                {
                    Date = DateTime.Now,
                    Attention = "Morris Reid",
                    ClientName = x.CustName,
                    Currency = x.Currency,
                    Email = x.Email,
                    User = user,
                    Address = x.Street + ", " + x.Parish,
                    Phone = string.IsNullOrEmpty(x.Phone1) ? x.Phone2 : string.IsNullOrEmpty(x.Phone2) ? x.Phone1 : "Not Specified"
                }).FirstOrDefault(),
                PrintData = printData
            };
            return View(summary);
        }

        [HttpPost]
        public JsonResult GetMultiplePartialByDay(SummaryModelForDay model)
        {
            string order = Customs.GetSortString(model.order, model.columns);
            var data = db.TRN09100.Where(x => x.TRN09110.Any(y => y.TRN23110.TRN23100.ADM03200.CustID == model.clientID &&
                x.InvoiceDate == model.date));
            return Json(new DataTableReturn
            {
                draw = model.draw,
                recordsFiltered = data.Count(),
                recordsTotal = data.Count(),
                data = Extensions.OrderByDynamic(data, order.Split(" ")[0], order.Split(" ")[1] != "asc").Skip(model.start).Take(model.length).ToList().Select(x => new
                {
                    DT_RowId = "tr-" + x.InvoiceNum,
                    InvoiceNum = Customs.MakeGenericInvoiceNo(x.InvoiceNum),
                    InvoiceTitle = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_title.ToUpper(),
                    InvoiceTotal = string.Format("{0:C}", x.InvoiceTotal.Value),
                    InvoiceDate = string.Format("{0:dd/MM/yyyy}", x.InvoiceDate)
                }).AsQueryable().ToStringArray()
            });
        }
        [HttpPost]
        public JsonResult GetMultiplePartialByRange(SummaryModelForRange model)
        {
            string order = Customs.GetSortString(model.order, model.columns);
            string format = "ddd MMMM dd, yyyy";
            DateTime dFrom = DateTime.ParseExact(model.from, format, System.Globalization.CultureInfo.InvariantCulture);
            DateTime dTo = DateTime.ParseExact(model.to, format, System.Globalization.CultureInfo.InvariantCulture);
            var data = db.TRN09100.Include(z1 => z1.TRN09110).ThenInclude(z2 => z2.TRN23110).ThenInclude(z3 => z3.TRN23100).Where(x => x.TRN09110.Any(y => y.TRN23110.TRN23100.ADM03200.CustID == model.clientID && x.InvoiceDate >= dFrom && x.InvoiceDate <= dTo));
            return Json(new DataTableReturn
            {
                draw = model.draw,
                recordsFiltered = data.Count(),
                recordsTotal = db.ADM02300.Count(),
                data = Extensions.OrderByDynamic(data, order.Split(" ")[0], order.Split(" ")[1] != "asc").Skip(model.start).Take(model.length).ToList().Select(x => new
                {
                    DT_RowId = "tr-" + x.InvoiceNum,
                    InvoiceNum = Customs.MakeGenericInvoiceNo(x.InvoiceNum),
                    InvoiceTitle = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_title.ToUpper(),
                    InvoiceTotal = string.Format("{0:C}", x.InvoiceTotal.Value),
                    InvoiceDate = string.Format("{0:dd/MM/yyyy}", x.InvoiceDate)
                }).AsQueryable().ToStringArray()
            });
        }
        //[HttpPost]
        public ActionResult MultipleInvoicesPrintOut(string invIDs, int clientId)
        {
            List<int> ids = invIDs.Split(',').Select(x => int.Parse(x)).ToList();
            List<ClientInvoice> invoices = new List<Models.ClientInvoice>();
            var user = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            foreach (var id in ids)
            {
                var activities = db.TRN23110.Include(z1 => z1.TRN09110).Include(z2 => z2.ADM01100)
                    .Where(x => x.TRN09110.Any(y => y.InvoiceNum == id)).AsEnumerable().Select(x => new ClientInvoiceActivities
                    {
                        ActivityCost = x.ADM01100.GetClientRateAmountForDate(x.ActDate),
                        ActivityDate = x.ActDate,
                        ActivityDesc = x.ADM01100.RateDescr,
                        ActivityLocation = x.Location,
                        ActivityQty = x.TRN09110.FirstOrDefault(y => y.InvoiceNum == id).InvoicedAmount,
                        Comments = x.AdtnlDetails
                    }).ToList();
                var mats = db.TRN23120.Include(z1 => z1.TRN23110).ThenInclude(z2 => z2.TRN09110).ToList()
                    .Where(x => x.TRN23110.TRN09110.Any(y => y.InvoiceNum == id)).AsEnumerable().Select(x => new ClientInvoiceActivityMaterial
                    {
                        MatDesc = db.ADM13100.FirstOrDefault(m => m.MaterialID == x.WoMatID).MaterialName,
                        ActivityDate = x.TRN23110.ActDate,
                        ActivityLocation = x.TRN23110.Location,
                        Comments = x.TRN23110.AdtnlDetails,
                        MatCost = 0,
                        MatQty = x.WoMatQty
                    }).ToList();
                var invoice = db.TRN09100.Include(z1 => z1.TRN09110).ThenInclude(z2 => z2.TRN23110).ThenInclude(z3 => z3.TRN23100).ThenInclude(z4 => z4.ADM01400).ThenInclude(z5 => z5.ADM26100)
                    .Include(z6 => z6.TRN09110).ThenInclude(z7 => z7.TRN23110).ThenInclude(z8 => z8.TRN23100).ThenInclude(z9 => z9.ADM03200)
                    .Where(x => x.InvoiceNum == id).AsEnumerable().Select(x => new ClientInvoice
                    {
                        Title = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_title.ToUpper(),
                        Date = x.InvoiceDate,
                        Client = new ClientData
                        {
                            Date = x.InvoiceDate,
                            InvoiceNo = x.InvoiceNum,
                            ReferenceNo = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_ref,
                            Attention = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Requestby,
                            ClientName = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM03200.CustName,
                            Currency = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM03200.Currency,
                            Email = string.IsNullOrEmpty(x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Email) ?
                              "Not Specified" : x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Email,
                            Address = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Addr
                              + ", " + x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.areaName
                              + ", " + x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Name,
                            Phone = string.IsNullOrEmpty(x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Phone) ?
                              "Not Specified" : x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Phone,
                            PONo = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.PONum
                        }
                    }).FirstOrDefault();
                invoice.Client.User = user;
                invoice.Activities = activities;
                invoice.Materials = mats;
                var gct = db.ADM07100.FirstOrDefault(x => x.StartDate >= invoice.Date && x.EndDate <= invoice.Date);

                invoice.GCT = Convert.ToDecimal((gct == null ? db.ADM07100.FirstOrDefault(x => x.EndDate == null).Percentage : gct.Percentage) / 100);
                invoice.Client.GCTNo = Customs.GetSettingsFileValue("GctReg", settingFilePath);
                invoices.Add(invoice);
            }
            return View(invoices);
        }

        public ActionResult BatchPrintOut(int id)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var printData = db.TRN09100.Include(z1 => z1.TRN09110).ThenInclude(z2 => z2.TRN23110).ThenInclude(z3 => z3.TRN23100).ThenInclude(z4 => z4.ADM03400).ThenInclude(z5 => z5.ADM03300)
                .Where(x => x.BatchId == id && x.Status == "o").AsEnumerable().ToList().Select(x => new SummaryPrintOut
                {
                    InvoiceID = Customs.MakeGenericInvoiceNo(x.InvoiceNum),
                    RefNo = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_ref,
                    Title = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_title.ToUpper(),
                    Total = x.InvoiceTotal.Value,
                    GCT = Convert.ToDecimal(((db.ADM07100.FirstOrDefault(x1 => x1.StartDate >= x.InvoiceDate && x1.EndDate <= x.InvoiceDate) == null ? db.ADM07100.FirstOrDefault(x2 => x2.EndDate == null).Percentage : db.ADM07100.FirstOrDefault(x1 => x1.StartDate >= x.InvoiceDate && x1.EndDate <= x.InvoiceDate).Percentage) / 100) * Convert.ToDouble(x.InvoiceTotal.Value)),
                    GrandTotal = x.InvoiceTotal.Value,
                    Contractors = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM03400.Where(yx => yx.CrewLead == true).Select(y => y.ADM03300.FirstName.Substring(0, 1) + ". " + y.ADM03300.LastName).ToList()
                }).ToList();

            var clientId = db.TRN09110.Include(z1 => z1.TRN09100).Include(z2 => z2.TRN23110).ThenInclude(z3 => z3.TRN23100).ThenInclude(z4 => z4.ADM03200)
                .FirstOrDefault(x => x.TRN09100.BatchId == id).TRN23110.TRN23100.ADM03200.CustID;
            var summary = new SummaryPrintOutWithClient
            {
                Client = db.ADM03200.Where(x => x.CustID == clientId).ToList().Select(x => new ClientData
                {
                    Date = DateTime.Now,
                    Attention = "Morris Reid",
                    ClientName = x.CustName,
                    Currency = x.Currency,
                    Email = x.Email,
                    User = user,
                    Address = x.Street + ", " + x.Parish,
                    Phone = string.IsNullOrEmpty(x.Phone1) ? x.Phone2 : string.IsNullOrEmpty(x.Phone2) ? x.Phone1 : "Not Specified"
                }).FirstOrDefault(),
                PrintData = printData
            };
            return PartialView("SummaryPrintOut", summary);
        }

        #region Print Invoice
        public ActionResult ClientInvoice(int id)
        {
            var invoice = ClientInvoiceData(id);
            ViewBag.InvoiceNo = id;
            return View(invoice);
        }
        [HttpPost]
        public void ClientInvoiceToExcel(ClientInvoiceFilter model)
        {
            var stream = new MemoryStream();
            ExcelPackage pck = new ExcelPackage(stream);
            int invNo = 0;
            var invoice = GetClientInvoice(model.InvoiceNo);
            if (invoice != null)
            {
                #region Work Sheet

                if (invoice.Client.InvoiceNo > 0)
                    invNo = Customs.MakeGenericInvoiceNo(invoice.Client.InvoiceNo);
                var ws = pck.Workbook.Worksheets.Add(invNo.ToString());

                #region View and Print Seetings

                //ws.View.PageLayoutView = true;
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
                else
                    logoSearchPath = Path.Combine(_env.WebRootPath, "Images", "print-logo.png");
                FileInfo fi = new FileInfo(logoSearchPath);
                var image = ws.Drawings.AddPicture("print-logo.png", fi);
                image.SetPosition(0, 0);
                image.SetSize(205, 111);


                ws.Cells["C" + y].Value = "P.O. BOX 3069";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                if (invoice.Client.InvoiceNo > 0)
                {
                    ws.Cells["D" + y].Value = "Invoice #:";
                    ws.Cells["E" + y].Value = Customs.MakeGenericInvoiceNo(invoice.Client.InvoiceNo);
                    ws.Cells["D" + y].Style.Font.Bold = true;
                    ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["C" + y].Value = "Kingston 8";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                if (invoice.Client.InvoiceNo > 0)
                {
                    ws.Cells["D" + y].Value = "GCT #:";
                    ws.Cells["E" + y].Value = invoice.Client.GCTNo;
                    ws.Cells["D" + y].Style.Font.Bold = true;
                }
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["C" + y].Value = "Tel: (876) 630-5278";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";

                ws.Cells["D" + y].Value = "Vendor #";
                ws.Cells["E" + y].Value = "10038549";
                ws.Cells["D" + y].Style.Font.Bold = true;
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
                ws.Cells["B" + y].Value = invoice.Client.ClientName;

                ws.Cells["D" + y].Value = "Date:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = string.Format("{0:dd/MM/yyyy}", invoice.Client.Date);
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Attn.:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = invoice.Client.Attention;

                ws.Cells["D" + y].Value = "Prepared By:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = invoice.Client.User;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Address:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = invoice.Client.Address;

                ws.Cells["D" + y].Value = "Currency:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = invoice.Client.Currency;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Phone:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = invoice.Client.Phone;
                if (invoice.Client.InvoiceNo > 0)
                {
                    ws.Cells["D" + y].Value = "Ticket #:";
                    ws.Cells["D" + y].Style.Font.Bold = true;
                    ws.Cells["E" + y].Value = invoice.Client.ReferenceNo;
                }
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Email:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = invoice.Client.Email;

                ws.Cells["D" + y].Value = "PO #:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = invoice.Client.PONo;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                #endregion
                y++;
                y++;


                #region Title
                ws.Cells[y, 1, y, 5].Merge = true;
                ws.Cells["A" + y].Value = invoice.Title.ToUpper();
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                #endregion
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


                foreach (var loc in invoice.Activities.Select(x => new { x.ActivityLocation, x.Comments }).Distinct())
                {
                    #region Location
                    ws.Cells[y, 1, y, 5].Merge = true;
                    ws.Cells["A" + y].Value = loc.ActivityLocation;
                    ws.Cells["A" + y].Style.Font.Bold = true;
                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["A" + y].Style.Font.Name = "Inherit";
                    ws.Cells["A" + y].Style.Font.Size = 12;
                    ws.Cells[y, 1, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["A" + y].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells["A" + y].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    #endregion
                    y++;
                    foreach (var act in invoice.Activities.Where(x => x.ActivityLocation == loc.ActivityLocation))
                    {
                        #region Rows
                        ws.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", act.ActivityDate);
                        ws.Cells["B" + y].Value = act.ActivityQty;
                        ws.Cells["C" + y].Value = act.ActivityDesc;
                        ws.Cells["D" + y].Value = string.Format("{0:C}", act.ActivityCost);
                        ws.Cells["E" + y].Value = string.Format("{0:C}", act.ActivityCost * Convert.ToDouble(act.ActivityQty));

                        #region Row Styles
                        ws.Cells["D" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));
                        ws.Cells["E" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
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
                }

                var total = invoice.Activities.Sum(x => x.ActivityCost * Convert.ToDouble(x.ActivityQty));
                var gct = total * Convert.ToDouble(invoice.GCT);
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
                ws.Cells["A" + y].Value = "GCT(" + (invoice.GCT * 100).ToString("#.##") + "):";
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

                #region Material Usage
                ws.Cells[y, 1, y, 5].Merge = true;
                y++;
                ws.Cells[y, 1, y, 5].Merge = true;
                y++;

                #region Title
                ws.Cells[y, 1, y, 5].Merge = true;
                ws.Cells["A" + y].Value = "Material Usage:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                #endregion
                y++;

                #region Table Headers
                ws.Cells["A" + y].Value = "Nos.";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 2, y, 4].Merge = true;
                ws.Cells["B" + y].Value = "Description";
                ws.Cells[y, 2, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = "Qty";
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

                int matCount = 1;
                foreach (var mat in invoice.Materials)
                {
                    #region Rows
                    ws.Cells["A" + y].Value = matCount;
                    ws.Cells[y, 2, y, 4].Merge = true;
                    ws.Cells["B" + y].Value = mat.MatDesc;
                    ws.Cells["E" + y].Value = mat.MatQty;

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

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

                #endregion

                #region BreakDown
                if (model.HasBreakDown)
                {
                    var breakDown = BreakDownData(model.InvoiceNo);
                    if (breakDown != null)
                    {
                        ws.Cells[y, 1, y, 5].Merge = true;
                        y++;

                        #region BreakDown Header

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Requested By:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = breakDown.RequestedBy;
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Request Date:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "Week Ending" + string.Format("{0:MMMM dd, yyyy}", breakDown.DateRequested);
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Issue Date:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = string.Format("{0:MMMM dd, yyyy}", breakDown.DateDispatched);
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Completion Date:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = string.Format("{0:MMMM dd, yyyy}", DateTime.Now);
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Department:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Field Survey:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "No";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Strand:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "No";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Fiber:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "Yes";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Drafting:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "No";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Transportation:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "No";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Location:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "";
                        y++;

                        #endregion

                        ws.Cells[y, 1, y, 5].Merge = true;
                        y++;

                        #region BreakDown Title
                        ws.Cells[y, 1, y, 5].Merge = true;
                        ws.Cells["A" + y].Value = "Work Order Details";
                        ws.Cells["A" + y].Style.Font.Bold = true;
                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["A" + y].Style.Font.Name = "Arial";
                        ws.Cells["A" + y].Style.Font.Size = 12;
                        y++;
                        #endregion

                        ws.Cells[y, 1, y, 5].Merge = true;
                        y++;

                        #region BreakDown Table Headers
                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Date";
                        ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "Description";
                        ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                        ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                        ws.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                        ws.Cells[y, 1, y, 5].Style.Font.Size = 12;
                        ws.Cells[y, 1, y, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                        ws.Cells[y, 1, y, 5].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                        ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        #endregion
                        y++;

                        #region BreakDown rows

                        foreach (var l in breakDown.Activities.Select(x => x.Location).Distinct())
                        {
                            ws.Cells[y, 1, y, 2].Merge = true;
                            ws.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", breakDown.Activities.FirstOrDefault(x => x.Location == l).Date);
                            ws.Cells[y, 3, y, 5].Merge = true;
                            ws.Cells["C" + y].Value = l;

                            #region Row Styles

                            ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells["C" + y].Style.Font.Bold = true;

                            ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                            ws.Cells[y, 3, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                            ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                            if (y % 2 != 0)
                            {
                                ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                            }
                            #endregion
                            y++;

                            foreach (var act in breakDown.Activities.Where(x => x.Location == l))
                            {
                                ws.Cells[y, 1, y, 2].Merge = true;
                                ws.Cells["A" + y].Value = "-";
                                ws.Cells[y, 3, y, 4].Merge = true;
                                ws.Cells["C" + y].Value = act.Description;

                                ws.Cells["E" + y].Value = act.Qty;

                                #region Row Styles

                                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                                ws.Cells[y, 3, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                                ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                                if (y % 2 != 0)
                                {
                                    ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                                }
                                #endregion
                                y++;
                            }
                        }

                        #endregion
                    }
                }
                #endregion

                #endregion
            }
            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + invNo + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public ClientInvoice ClientInvoiceData(int id)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var refNum = db.TRN23110.FirstOrDefault(x => x.TRN09110.Any(y => y.InvoiceNum == id)).TRN23100.Wo_ref;
            var refWOIds = db.TRN23100.Where(x => x.Wo_ref == refNum).Select(x => x.Workid).ToArray();
            var activities = db.TRN23110.Where(x => x.TRN09110.Any(y => y.InvoiceNum == id)).AsEnumerable().Select(x => new ClientInvoiceActivities
            {
                ActivityCost = x.ADM01100.GetClientRateAmountForDate(x.ActDate),
                ActivityDate = x.ActDate,
                ActivityDesc = x.ADM01100.RateDescr,
                ActivityLocation = x.Location,
                ActivityQty = x.TRN09110.FirstOrDefault(y => y.InvoiceNum == id).InvoicedAmount,
                Comments = x.AdtnlDetails
            }).ToList();
            var mats = db.TRN23120.Where(x => x.TRN23110.TRN09110.Any(y => y.InvoiceNum == id)).AsEnumerable().Select(x => new ClientInvoiceActivityMaterial
            {
                MatDesc = db.ADM13100.FirstOrDefault(m => m.MaterialID == x.WoMatID).MaterialName,
                ActivityDate = x.TRN23110.ActDate,
                ActivityLocation = x.TRN23110.Location,
                Comments = x.TRN23110.AdtnlDetails,
                MatCost = 0,
                MatQty = x.WoMatQty
            }).ToList();
            var invoice = db.TRN09100.Where(x => x.InvoiceNum == id).AsEnumerable().Select(x => new ClientInvoice
            {
                Title = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_title.ToUpper(),
                Date = x.InvoiceDate,
                Client = new ClientData
                {
                    Date = x.InvoiceDate,
                    InvoiceNo = x.InvoiceNum,
                    ReferenceNo = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_ref,
                    Attention = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Requestby,
                    ClientName = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM03200.CustName,
                    Currency = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM03200.Currency,
                    Email = string.IsNullOrEmpty(x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Email) ?
                       "Not Specified" : x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Email,
                    Address = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Addr
                        + ", " + x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.areaName
                        + ", " + x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Name,
                    Phone = string.IsNullOrEmpty(x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Phone) ?
                       "Not Specified" : x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Phone,
                    PONo = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.PONum
                }
            }).FirstOrDefault();
            invoice.Client.User = user;
            invoice.Activities = activities;
            invoice.Materials = mats;
            var gct = db.ADM07100.FirstOrDefault(x => x.StartDate >= invoice.Date && x.EndDate <= invoice.Date);

            invoice.GCT = Convert.ToDecimal((gct == null ? db.ADM07100.FirstOrDefault(x => x.EndDate == null).Percentage : gct.Percentage) / 100);
            invoice.Client.GCTNo = Customs.GetSettingsFileValue("GctReg", settingFilePath);
            return invoice;
        }
        #endregion

        #region Print BreakDown
        public ActionResult BreakDown(int id, bool forInvoice = false)
        {
            var breakDown = BreakDownData(id);
            ViewBag.forInvoice = forInvoice;
            ViewBag.InvoiceNo = id;
            return View(breakDown);
        }
        [HttpPost]
        public async Task<IActionResult> BreakDownToExcel(ClientInvoiceFilter model)
        {
            await Task.Yield();
            var stream = new MemoryStream();
            string docName = "BreakDown";
            ExcelPackage pck = new ExcelPackage(stream);
            int invNo = 0;
            var breakDown = BreakDownData(model.InvoiceNo);
            if (breakDown != null)
            {
                #region Work Sheet
                if (model.InvoiceNo > 0)
                    invNo = Customs.MakeGenericInvoiceNo(model.InvoiceNo);
                var ws = pck.Workbook.Worksheets.Add(invNo.ToString());

                #region View and Print Seetings

                //ws.View.PageLayoutView = true;
                ws.PrinterSettings.ShowGridLines = false;

                ws.PrinterSettings.PaperSize = ePaperSize.A4;
                ws.PrinterSettings.TopMargin = 0.75M;
                ws.PrinterSettings.RightMargin = 0.25M;
                ws.PrinterSettings.BottomMargin = 0.75M;
                ws.PrinterSettings.LeftMargin = 0.25M;
                ws.PrinterSettings.HeaderMargin = 0.0M;
                ws.PrinterSettings.FooterMargin = 0.0M;

                #endregion

                ws.Column(1).Width = 11.71;
                ws.Column(2).Width = 8.43;
                ws.Column(3).Width = 48.14;
                ws.Column(4).Width = 12.29;
                ws.Column(5).Width = 15.57;
                int y = 1;
                #region BreakDown

                ws.Cells[y, 1, y, 5].Merge = true;
                y++;

                #region BreakDown Header

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Requested By:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = breakDown.RequestedBy;
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Request Date:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "Week Ending" + string.Format("{0:MMMM dd, yyyy}", breakDown.DateRequested);
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Issue Date:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = string.Format("{0:MMMM dd, yyyy}", breakDown.DateDispatched);
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "PO #:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = breakDown.RefNo;
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Ticket #:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = breakDown.RefNo;
                y++;

                //ws.Cells[y, 1, y, 2].Merge = true;
                //ws.Cells["A" + y].Value = "Completion Date:";
                //ws.Cells[y, 3, y, 5].Merge = true;
                //ws.Cells["C" + y].Value = string.Format("{0:MMMM dd, yyyy}", DateTime.Now);
                //y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Department:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "";
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Field Survey:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "No";
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Strand:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "No";
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Fiber:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "Yes";
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Drafting:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "No";
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Transportation:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "No";
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Location:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "";
                y++;

                #endregion

                ws.Cells[y, 1, y, 5].Merge = true;
                y++;

                #region BreakDown Title
                ws.Cells[y, 1, y, 5].Merge = true;
                ws.Cells["A" + y].Value = breakDown.Title.ToUpper();
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;
                #endregion

                ws.Cells[y, 1, y, 5].Merge = true;
                y++;

                #region BreakDown Table Headers
                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Date";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "Description";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 5].Style.Font.Size = 12;
                ws.Cells[y, 1, y, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, 5].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                #endregion
                y++;

                #region BreakDown rows

                foreach (var l in breakDown.Activities.Select(x => x.Location).Distinct())
                {
                    ws.Cells[y, 1, y, 2].Merge = true;
                    ws.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", breakDown.Activities.FirstOrDefault(x => x.Location == l).Date);
                    ws.Cells[y, 3, y, 5].Merge = true;
                    ws.Cells["C" + y].Value = l;

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells["C" + y].Style.Font.Bold = true;

                    ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells[y, 3, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                    if (y % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion
                    y++;

                    foreach (var act in breakDown.Activities.Where(x => x.Location == l))
                    {
                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "-";
                        ws.Cells[y, 3, y, 4].Merge = true;
                        ws.Cells["C" + y].Value = act.Description;

                        ws.Cells["E" + y].Value = act.Qty;

                        #region Row Styles

                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells[y, 3, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                        ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                        if (y % 2 != 0)
                        {
                            ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        }
                        #endregion
                        y++;
                    }
                }

                #endregion

                #endregion

                #endregion
            }
            if (pck.Workbook.Worksheets.Count() > 0)
            {
                pck.Save();
                //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //Response.AddHeader("content-disposition", "attachment;  filename=" + docName + ".xlsx");
            }
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", docName + ".xlsx");
        }

        public InvoiceBreakDown BreakDownData(int id)
        {
            var breakDown = new InvoiceBreakDown();
            var workOrder = db.TRN09110.FirstOrDefault(x => x.InvoiceNum == id).TRN23110.TRN23100;
            breakDown.DateDispatched = workOrder.Dispatchdt;
            breakDown.DateRequested = workOrder.Requestdt.Day > 14 ?
                new DateTime(workOrder.Requestdt.Year, workOrder.Requestdt.Month, DateTime.DaysInMonth(workOrder.Requestdt.Year, workOrder.Requestdt.Month)) :
                new DateTime(workOrder.Requestdt.Year, workOrder.Requestdt.Month, 14);
            breakDown.RequestedBy = workOrder.Requestby;
            breakDown.Title = workOrder.Wo_title.ToUpper();
            breakDown.RefNo = workOrder.Wo_ref;
            breakDown.PONo = workOrder.PONum;
            breakDown.Activities = db.TRN09110.Where(x => x.InvoiceNum == id).Select(x => new ShortActivity
            {
                Date = x.TRN23110.ActDate,
                Description = x.TRN23110.ADM01100.RateDescr,
                Location = x.TRN23110.Location,
                Qty = x.TRN23110.OActQty
            }).ToList();
            return breakDown;
        }

        #endregion

        [HttpPost]
        public JsonResult ChangeInvoiceStatus(int id, string type)
        {
            try
            {
                if (type == "b")
                {
                    db.TRN09100.Find(id).BatchId = null;
                }
                else
                {
                    var links = db.TRN09110.Where(x => x.InvoiceNum == id).ToList();
                    var wids = links.Select(x => x.TRN23110.WorkOID).Distinct().ToList();
                    foreach (var link in db.TRN09110.Where(x => x.InvoiceNum == id).ToList())
                    {
                        if (link.TRN23110.InvFlag && link.TRN23110.ActQty == 0 && link.TRN23110.OActQty == 0)
                            db.TRN23110.Remove(link.TRN23110);
                        else
                            link.TRN23110.ActQty = link.TRN23110.OActQty;
                        db.TRN09110.Remove(link);
                    }
                    db.TRN09100.Find(id).Status = type;
                    db.TRN23100.Where(x => wids.Contains(x.Workid)).ToList().ForEach(x => x.Status = "v");
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
                    Code = "300",
                    Msg = e.Message
                });
            }
        }

        //[TeliconAuthorize(TaskId = 21)]
        public ActionResult InvoiceExportToExcel()//(int? year)
        {
            return View();
        }
        [HttpPost]
        public void InvoiceExportToExcel(InvoiceExportToExcelModel model)
        {
            var stream = new MemoryStream();
            string docName = model.from + " - " + model.to;
            ExcelPackage pck = new ExcelPackage(stream);
            int from = 0;
            int to = 0;
            if (model.from != 0 && model.to != 0)
            {
                from = Convert.ToInt32(model.from.ToString().Substring(4, model.from.ToString().Length - 4));
                to = Convert.ToInt32(model.to.ToString().Substring(4, model.to.ToString().Length - 4));
            }
            for (int id = from; id <= to; id++)
            {
                var invoice = GetClientInvoice(id);
                if (invoice != null)
                {
                    #region Work Sheet
                    int invNo = 0;
                    if (invoice.Client.InvoiceNo > 0)
                        invNo = Customs.MakeGenericInvoiceNo(invoice.Client.InvoiceNo);
                    var ws = pck.Workbook.Worksheets.Add(invNo.ToString());

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
                    #region Header
                    string logoSearchPath = "/Images/print-logo.png";
                    if (Directory.Exists(Path.Combine(_env.WebRootPath, "work")))
                        logoSearchPath = "/work/images/print-logo.png";
                    else
                        logoSearchPath = Path.Combine(_env.WebRootPath, "Images", "print-logo.png");
                    FileInfo fi = new FileInfo(logoSearchPath);
                    var image = ws.Drawings.AddPicture("print-logo.png", fi);
                    image.SetPosition(0, 0);
                    image.SetSize(205, 111);

                    #region Telicon Address
                    ws.Cells["C1"].Value = "P.O. BOX 3069";
                    ws.Cells["C1"].Style.Font.Bold = true;
                    ws.Cells["C1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["C2"].Value = "Kingston 8";
                    ws.Cells["C2"].Style.Font.Bold = true;
                    ws.Cells["C2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["C3"].Value = "Tel: (876) 630-5278";
                    ws.Cells["C3"].Style.Font.Bold = true;
                    ws.Cells["C3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["C4"].Value = "Email: admin@telicongroup.com";
                    ws.Cells["C4"].Style.Font.Bold = true;
                    ws.Cells["C4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    #endregion

                    if (invoice.Client.InvoiceNo > 0)
                    {
                        ws.Cells["D1"].Value = "Invoice #:";
                        ws.Cells["E1"].Value = Customs.MakeGenericInvoiceNo(invoice.Client.InvoiceNo);
                        ws.Cells["D1"].Style.Font.Bold = true;
                        ws.Cells["E1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        ws.Cells["D2"].Value = "GCT #:";
                        ws.Cells["E2"].Value = invoice.Client.GCTNo;
                        ws.Cells["D2"].Style.Font.Bold = true;
                        ws.Cells[1, 3, 12, 4].Style.Font.Name = "Inherit";

                        ws.Cells["D3"].Value = "Vendor #";
                        ws.Cells["E3"].Value = "10038549";
                        ws.Cells["D3"].Style.Font.Bold = true;

                        ws.Cells[1, 3, 12, 4].Style.Font.Name = "Inherit";
                    }

                    #region Client Details
                    ws.Cells["A8"].Value = "Co. Name:";
                    ws.Cells["A8"].Style.Font.Bold = true;
                    ws.Cells[8, 2, 8, 3].Merge = true;
                    ws.Cells["B8"].Value = invoice.Client.ClientName;

                    ws.Cells["A9"].Value = "Attn.:";
                    ws.Cells["A9"].Style.Font.Bold = true;
                    ws.Cells[9, 2, 9, 3].Merge = true;
                    ws.Cells["B9"].Value = invoice.Client.Attention;

                    ws.Cells["A10"].Value = "Address:";
                    ws.Cells["A10"].Style.Font.Bold = true;
                    ws.Cells[10, 2, 10, 3].Merge = true;
                    ws.Cells["B10"].Value = invoice.Client.Address;

                    ws.Cells["A11"].Value = "Phone:";
                    ws.Cells["A11"].Style.Font.Bold = true;
                    ws.Cells[11, 2, 11, 3].Merge = true;
                    ws.Cells["B11"].Value = invoice.Client.Phone;

                    ws.Cells["A12"].Value = "Email:";
                    ws.Cells["A12"].Style.Font.Bold = true;
                    ws.Cells[12, 2, 12, 3].Merge = true;
                    ws.Cells["B12"].Value = invoice.Client.Email;

                    ws.Cells[8, 1, 12, 1].Style.Font.Name = "Inherit";
                    #endregion

                    ws.Cells["D8"].Value = "Date:";
                    ws.Cells["D8"].Style.Font.Bold = true;
                    ws.Cells["E8"].Value = string.Format("{0:dd/MM/yyyy}", invoice.Client.Date);

                    ws.Cells["D9"].Value = "Prepared By:";
                    ws.Cells["D9"].Style.Font.Bold = true;
                    ws.Cells["E9"].Value = invoice.Client.User;

                    ws.Cells["D10"].Value = "Currency:";
                    ws.Cells["D10"].Style.Font.Bold = true;
                    ws.Cells["E10"].Value = invoice.Client.Currency;

                    if (invoice.Client.InvoiceNo > 0)
                    {
                        ws.Cells["D11"].Value = "Ticket #:";
                        ws.Cells["D11"].Style.Font.Bold = true;
                        ws.Cells["E11"].Value = invoice.Client.ReferenceNo;
                    }

                    ws.Cells["D12"].Value = "PO #:";
                    ws.Cells["D12"].Style.Font.Bold = true;
                    ws.Cells["E12"].Value = invoice.Client.PONo;
                    #endregion

                    #region Title
                    ws.Cells[15, 1, 15, 5].Merge = true;
                    ws.Cells["A15"].Value = invoice.Title.ToUpper();
                    ws.Cells["A15"].Style.Font.Bold = true;
                    ws.Cells["A15"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["A15"].Style.Font.Name = "Arial";
                    ws.Cells["A15"].Style.Font.Size = 12;
                    #endregion

                    #region Table Headers
                    ws.Cells["A16"].Value = "Date";
                    ws.Cells["A16"].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B16"].Value = "Qty";
                    ws.Cells["B16"].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C16"].Value = "Description";
                    ws.Cells["C16"].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D16"].Value = "Rate";
                    ws.Cells["D16"].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["E16"].Value = "Amount";
                    ws.Cells["E16"].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[16, 1, 16, 5].Style.Font.Bold = true;
                    ws.Cells[16, 1, 16, 5].Style.Font.Name = "Inherit";
                    ws.Cells[16, 1, 16, 5].Style.Font.Size = 12;
                    ws.Cells[16, 1, 16, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                    ws.Cells[16, 1, 16, 5].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                    ws.Cells[16, 1, 16, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[16, 1, 16, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    #endregion

                    int y = 17;
                    foreach (var loc in invoice.Activities.Select(x => new { x.ActivityLocation, x.Comments }).Distinct())
                    {
                        #region Location
                        ws.Cells[y, 1, y, 5].Merge = true;
                        ws.Cells["A" + y].Value = loc.ActivityLocation;
                        ws.Cells["A" + y].Style.Font.Bold = true;
                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["A" + y].Style.Font.Name = "Inherit";
                        ws.Cells["A" + y].Style.Font.Size = 12;
                        ws.Cells[y, 1, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["A" + y].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells["A" + y].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        #endregion
                        y++;
                        foreach (var act in invoice.Activities.Where(x => x.ActivityLocation == loc.ActivityLocation))
                        {
                            #region Rows
                            ws.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", act.ActivityDate);
                            ws.Cells["B" + y].Value = act.ActivityQty;
                            ws.Cells["C" + y].Value = act.ActivityDesc;
                            ws.Cells["D" + y].Value = string.Format("{0:C}", act.ActivityCost);
                            ws.Cells["E" + y].Value = string.Format("{0:C}", act.ActivityCost * Convert.ToDouble(act.ActivityQty));

                            #region Row Styles
                            ws.Cells["D" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));
                            ws.Cells["E" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                            ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
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
                    }

                    var total = invoice.Activities.Sum(x => x.ActivityCost * Convert.ToDouble(x.ActivityQty));
                    var gct = total * Convert.ToDouble(invoice.GCT);
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
                    ws.Cells["A" + y].Value = "GCT(" + (invoice.GCT * 100).ToString("#.##") + "):";
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

                    #region Material Usage
                    ws.Cells[y, 1, y, 5].Merge = true;
                    y++;
                    ws.Cells[y, 1, y, 5].Merge = true;
                    y++;

                    #region Title
                    ws.Cells[y, 1, y, 5].Merge = true;
                    ws.Cells["A" + y].Value = "Material Usage:";
                    ws.Cells["A" + y].Style.Font.Bold = true;
                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["A" + y].Style.Font.Name = "Arial";
                    ws.Cells["A" + y].Style.Font.Size = 12;
                    #endregion
                    y++;

                    #region Table Headers
                    ws.Cells["A" + y].Value = "Nos.";
                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells[y, 2, y, 4].Merge = true;
                    ws.Cells["B" + y].Value = "Description";
                    ws.Cells[y, 2, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["E" + y].Value = "Qty";
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

                    int matCount = 1;
                    foreach (var mat in invoice.Materials)
                    {
                        #region Rows
                        ws.Cells["A" + y].Value = matCount;
                        ws.Cells[y, 2, y, 4].Merge = true;
                        ws.Cells["B" + y].Value = mat.MatDesc;
                        ws.Cells["E" + y].Value = mat.MatQty;

                        #region Row Styles

                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

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

                    #endregion
                    #endregion
                }
            }
            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);

            }
        }

        public ClientInvoice GetClientInvoice(int id)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var inv = db.TRN09100.Find(id);
            if (inv == null)
                return null;
            else if (inv.Status.ToLower() == "r")
                return null;
            var refNum = db.TRN23110.Include(z1 => z1.TRN09110).Include(z2 => z2.TRN23100)
                .FirstOrDefault(x => x.TRN09110.Any(y => y.InvoiceNum == id)).TRN23100.Wo_ref;
            var refWOIds = db.TRN23100.Where(x => x.Wo_ref == refNum).Select(x => x.Workid).ToArray();
            var activities = db.TRN23110.Include(z1 => z1.TRN09110).Include(z1 => z1.ADM01100).Where(x => x.TRN09110.Any(y => y.InvoiceNum == id)).AsEnumerable().Select(x => new ClientInvoiceActivities
            {
                ActivityCost = x.ADM01100.GetClientRateAmountForDate(x.ActDate),
                ActivityDate = x.ActDate,
                ActivityDesc = x.ADM01100.RateDescr,
                ActivityLocation = x.Location,
                ActivityQty = x.TRN09110.FirstOrDefault(y => y.InvoiceNum == id).InvoicedAmount,
                Comments = x.AdtnlDetails
            }).ToList();
            var mats = db.TRN23120.Include(z1 => z1.TRN23110).Where(x => x.TRN23110.TRN09110.Any(y => y.InvoiceNum == id)).AsEnumerable().Select(x => new ClientInvoiceActivityMaterial
            {
                MatDesc = db.ADM13100.FirstOrDefault(m => m.MaterialID == x.WoMatID).MaterialName,
                ActivityDate = x.TRN23110.ActDate,
                ActivityLocation = x.TRN23110.Location,
                Comments = x.TRN23110.AdtnlDetails,
                MatCost = 0,
                MatQty = x.WoMatQty
            }).ToList();
            var invoice = db.TRN09100.Include(z1 => z1.TRN09110).ThenInclude(z2 => z2.TRN23110).ThenInclude(z3 => z3.TRN23100).ThenInclude(z4 => z4.ADM03200)
                .Include(z5 => z5.TRN09110).ThenInclude(z6 => z6.TRN23110).ThenInclude(z7 => z7.TRN23100).ThenInclude(z8 => z8.ADM01400).ThenInclude(z9 => z9.ADM26100)
                .Where(x => x.InvoiceNum == id).AsEnumerable().Select(x => new ClientInvoice
                {
                    Title = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_title.ToUpper(),
                    Date = x.InvoiceDate,
                    Client = new ClientData
                    {
                        Date = x.InvoiceDate,
                        InvoiceNo = x.InvoiceNum,
                        ReferenceNo = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_ref,
                        Attention = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Requestby,
                        ClientName = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM03200.CustName,
                        Currency = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM03200.Currency,
                        Email = string.IsNullOrEmpty(x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Email) ?
                       "Not Specified" : x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Email,
                        Address = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Addr
                        + ", " + x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.areaName
                        + ", " + x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Name,
                        Phone = string.IsNullOrEmpty(x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Phone) ?
                       "Not Specified" : x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Phone,
                        PONo = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.PONum
                    }
                }).FirstOrDefault();
            invoice.Client.User = user;
            invoice.Activities = activities;
            invoice.Materials = mats;
            var gct = db.ADM07100.FirstOrDefault(x => x.StartDate >= invoice.Date && x.EndDate <= invoice.Date);

            invoice.GCT = Convert.ToDecimal((gct == null ? db.ADM07100.FirstOrDefault(x => x.EndDate == null).Percentage : gct.Percentage) / 100);
            invoice.Client.GCTNo = Customs.GetSettingsFileValue("GctReg", settingFilePath);
            return invoice;
        }

        #region New Invoice
        public ActionResult NewInv()
        {
            int? invID = null;
            var invNos = db.TRN09100.AsEnumerable().Where(y => y.Status.ToLower() == "r" && y.IsNewFormat).Select(y => new SelectListItem
            {
                Text = Reusables.Customs.MakeGenericInvoiceNo(y.InvoiceNum).ToString(),
                Value = y.InvoiceNum.ToString()
            }).ToList();
            var pos = db.ADM16200.Select(y => new SelectListItem
            {
                Text = y.PONUM,
                Value = y.PONUM
            }).ToList();
            var classifis = db.ADM03500.Select(y => new SelectListItem
            {
                Text = y.ClassName,
                Value = y.ClassId.ToString()
            }).ToList();
            var zones = db.ADM26100.Select(y => new SelectListItem
            {
                Text = y.Name,
                Value = y.ZoneID.ToString()
            }).ToList();
            var clients = db.ADM03200.Select(y => new SelectListItem
            {
                Text = y.CustName,
                Value = y.CustID.ToString(),
                Selected = y.CustID == 3
            }).ToList();
            ViewBag.InvNOs = invNos;
            ViewBag.Clients = clients;
            ViewBag.POs = pos;
            ViewBag.Classifis = classifis;
            ViewBag.Zones = zones;
            ViewBag.InvoiceID = invID.HasValue ? invID.Value : 0;
            ViewBag.Invoiced = invID.HasValue;
            ViewBag.id = null;
            ViewBag.Title = "";
            return View(GetRefrencesForMerging(null, null));
        }
        public ActionResult GetNewInvoiceCreator(int id)
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
                Text = Reusables.Customs.MakeGenericInvoiceNo(y.InvoiceNum).ToString(),
                Value = y.InvoiceNum.ToString()
            }).ToList();
            ViewBag.InvNOs = invNos;
            ViewBag.InvoiceID = invID.HasValue ? invID.Value : 0;
            ViewBag.Invoiced = invID.HasValue;
            ViewBag.id = id;
            ViewBag.Title = merge != null ? merge.MergerdTitle : "";
            return View("Merger", GetRefrencesForMerged(merge.MergedRefNum));
        }
        public ActionResult InvoiceCandiditates(NewInvoiceFilter model)
        {
            var data = db.TRN23100.Where(x => x.Status == "v" && x.Wo_client == model.client);
            if (!string.IsNullOrEmpty(model.po))
                data = data.Where(x => x.PONum == model.po);
            if (model.classifi.HasValue)
                data = data.Where(x => x.ClassId == model.classifi);
            if (model.region.HasValue)
            {
                var areasOfRegion = db.ADM26100.FirstOrDefault(x => x.ZoneID == model.region).ADM01400.Select(a => a.areaID).ToList();
                data = data.Where(x => areasOfRegion.Contains(x.AreaID));
            }
            if (model.dateFrom.HasValue)
                data = data.Where(x => x.DateSubmitted >= model.dateFrom);
            if (model.dateTo.HasValue)
                data = data.Where(x => x.DateSubmitted <= model.dateTo);
            var candidates = data.AsEnumerable().Select(x => new WorkOrderPartial
            {
                WOId = x.Workid,
                Title = x.Wo_title.ToUpper(),
                Status = DataDictionaries.WordOrderStatuses[x.Status],
                Total = x.TRN23110.Sum(y => Convert.ToDouble(y.OActQty) * y.ADM01100.GetRateAmountForDate(y.ActDate)),
                IsMerged = db.TRN13120.FirstOrDefault(m => m.WorkOrderId == x.Workid) != null || db.TRN13110.FirstOrDefault(m => m.WorkOrderId == x.Workid) != null
            }).ToList();
            return View(candidates);
        }
        public ActionResult InvoiceCandiditatesWithInvoiced(NewInvoiceFilter model, int id)
        {
            var candidates = db.TRN23100.Where(x => x.Wo_ref == model.po && (x.TRN13110.Any(w => w.MergedOrderId == id) || x.TRN13120.Any(w => w.MergedOrderId == id) || (!x.TRN13110.Any() && !x.TRN13120.Any() && x.Status != "i"))).AsEnumerable().Select(x => new WorkOrderPartial
            {
                WOId = x.Workid,
                Title = x.Wo_title.ToUpper(),
                Status = DataDictionaries.WordOrderStatuses[x.Status],
                Total = x.TRN23110.Sum(y => Convert.ToDouble(y.OActQty) * y.ADM01100.GetRateAmountForDate(y.ActDate)),
                IsMerged = db.TRN13120.FirstOrDefault(m => m.WorkOrderId == x.Workid) != null || db.TRN13110.FirstOrDefault(m => m.WorkOrderId == x.Workid) != null
            }).ToList();
            return View("InvoiceCandiditates", candidates);
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
        public ActionResult ProcessWorkOrders(List<int> wOIds, bool isUpdate, int? id, string title)
        {
            title = title.ToUpper();
            db.Database.OpenConnection();
            using TransactionScope ts = new TransactionScope();
            try
            {
                if (wOIds != null && wOIds.Count > 0)
                {
                    var total = db.TRN23110.Where(x => wOIds.Contains(x.WorkOID)).AsEnumerable().Sum(x => (Convert.ToDouble(x.OActQty) * x.ADM01100.GetClientRateAmountForDate(x.ActDate)));
                    var invoice = new TRN09100();
                    if (id.HasValue)
                    {
                        invoice = db.TRN09100.Find(id.Value);
                        invoice.InvoiceTotal = Convert.ToDecimal(total);
                        invoice.Status = "o";
                        invoice.InvoiceTitle = title.ToUpper();
                    }
                    else
                    {
                        var lastInv = db.TRN09100.Where(x => x.IsNewFormat).OrderByDescending(x => x.NewInvNo).FirstOrDefault();
                        int newNum = lastInv != null ? lastInv.InvNewNum + 1 : 1;
                        invoice = new TRN09100
                        {
                            InvoiceDate = DateTime.Now,
                            GeneratedDate = DateTime.Now,
                            InvoiceTitle = title.ToUpper(),
                            Status = "o",
                            IsNewFormat = true,
                            InvNewNum = newNum,
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
                            var invitem = new TRN09110
                            {
                                InvoicedAmount = act.OActQty,
                                InvoiceNum = invoice.InvoiceNum,
                                WoActID = act.RecID
                            };
                            db.TRN09110.Add(invitem);
                        }
                        db.TRN23100.Find(wOId).Status = "i";
                    }
                    db.SaveChanges();
                    ts.Complete();
                    ts.Dispose();
                    return Json(new JsonReturnParams
                    {
                        Additional = invoice.InvoiceNum,
                        Code = "100",
                        Msg = ""
                    });
                }
                else
                {
                    return Json(new JsonReturnParams
                    {
                        Additional = "",
                        Code = "300",
                        Msg = "Cannot process Work order(s), there are no selected Work order(s) to process."
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
        public JsonResult CheckForValidWOs(List<int> ids)
        {
            string msg = "";
            msg = db.TRN23100.Any(x => ids.Contains(x.Workid) && x.Status.ToLower() == "i") ? "invoiced" : "";
            bool hasActivities = db.TRN23100.Any(x => ids.Contains(x.Workid) && !x.TRN23110.Any()) ? false : true;
            bool verified = db.TRN23100.Any(x => ids.Contains(x.Workid) && x.Status.ToLower() != "v") ? false : true;
            var data = new
            {
                any = hasActivities,
                verified = verified,
                msg = msg
            };
            return Json(data);
        }


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
        #endregion

        #region New Print Invoice
        public ActionResult NewClientInvoice(int id)
        {
            var invoice = NewClientInvoiceData(id);
            ViewBag.InvoiceNo = id;
            return View(invoice);
        }
        [HttpPost]
        public void NewClientInvoiceToExcel(ClientInvoiceFilter model)
        {
            ExcelPackage pck = new ExcelPackage();
            int invNo = 0;
            var invoice = NewClientInvoiceData(model.InvoiceNo);
            if (invoice != null)
            {
                #region Work Sheet

                if (invoice.Client.InvoiceNo > 0)
                    invNo = Customs.MakeGenericInvoiceNo(invoice.Client.InvoiceNo);
                var ws = pck.Workbook.Worksheets.Add(invNo.ToString());

                #region View and Print Seetings

                //ws.View.PageLayoutView = true;
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
                else
                    logoSearchPath = Path.Combine(_env.WebRootPath, "Images", "print-logo.png");
                FileInfo fi = new FileInfo(logoSearchPath);
                var image = ws.Drawings.AddPicture("print-logo.png", fi);
                image.SetPosition(0, 0);
                image.SetSize(205, 111);



                ws.Cells["C" + y].Value = "P.O. BOX 3069";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                if (invoice.Client.InvoiceNo > 0)
                {
                    ws.Cells["D" + y].Value = "Invoice #:";
                    ws.Cells["E" + y].Value = Customs.MakeGenericInvoiceNo(invoice.Client.InvoiceNo);
                    ws.Cells["D" + y].Style.Font.Bold = true;
                    ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["C" + y].Value = "Kingston 8";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                if (invoice.Client.InvoiceNo > 0)
                {
                    ws.Cells["D" + y].Value = "GCT #:";
                    ws.Cells["E" + y].Value = invoice.Client.GCTNo;
                    ws.Cells["D" + y].Style.Font.Bold = true;
                }
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["C" + y].Value = "Tel: (876) 630-5278";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";

                ws.Cells["D" + y].Value = "GR #";
                ws.Cells["E" + y].Value = "5000433798";
                ws.Cells["D" + y].Style.Font.Bold = true;
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
                ws.Cells["B" + y].Value = invoice.Client.ClientName;

                ws.Cells["D" + y].Value = "Date:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = string.Format("{0:dd/MM/yyyy}", invoice.Client.Date);
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Attn.:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = invoice.Client.Attention;

                ws.Cells["D" + y].Value = "Prepared By:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = invoice.Client.User;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Address:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = invoice.Client.Address;

                ws.Cells["D" + y].Value = "Currency:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = invoice.Client.Currency;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Phone:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = invoice.Client.Phone;
                if (invoice.Client.InvoiceNo > 0)
                {
                    //ws.Cells["D" + y].Value = "Ticket #:";
                    //ws.Cells["D" + y].Style.Font.Bold = true;
                    //ws.Cells["E" + y].Value = invoice.Client.ReferenceNo;
                }
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Email:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = invoice.Client.Email;

                //ws.Cells["D" + y].Value = "PO #:";
                //ws.Cells["D" + y].Style.Font.Bold = true;
                //ws.Cells["E" + y].Value = invoice.Client.PONo;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                #endregion
                y++;
                y++;


                //#region Title
                //ws.Cells[y, 1, y, 5].Merge = true;
                //ws.Cells["A" + y].Value = invoice.Title;
                //ws.Cells["A" + y].Style.Font.Bold = true;
                //ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //ws.Cells["A" + y].Style.Font.Name = "Arial";
                //ws.Cells["A" + y].Style.Font.Size = 12;
                //#endregion
                //y++;

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


                #region Location
                ws.Cells[y, 2, y, 5].Merge = true;
                ws.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", invoice.Date);
                ws.Cells["B" + y].Value = invoice.Title.ToUpper();
                ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 5].Style.Font.Size = 12;
                ws.Cells[y, 1, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                #endregion
                y++;

                foreach (var desc in invoice.Activities.Select(x => x.ActivityDesc).Distinct())
                {
                    //#region Location
                    //ws.Cells[y, 1, y, 5].Merge = true;
                    //ws.Cells["A" + y].Value = loc.ActivityLocation;
                    //ws.Cells["A" + y].Style.Font.Bold = true;
                    //ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells["A" + y].Style.Font.Name = "Inherit";
                    //ws.Cells["A" + y].Style.Font.Size = 12;
                    //ws.Cells[y, 1, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    //ws.Cells["A" + y].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //ws.Cells["A" + y].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    //#endregion
                    //y++;

                    var act = invoice.Activities.Where(x => x.ActivityDesc == desc).ToList();

                    //foreach (var act in invoice.Activities.Where(x => x.ActivityDesc == desc))
                    //{
                    #region Rows
                    ws.Cells["A" + y].Value = "";
                    ws.Cells["B" + y].Value = act.Sum(x => x.ActivityQty);
                    ws.Cells["C" + y].Value = desc;
                    ws.Cells["D" + y].Value = string.Format("{0:C}", act[0].ActivityCost);
                    ws.Cells["E" + y].Value = string.Format("{0:C}", act[0].ActivityCost * Convert.ToDouble(act.Sum(x => x.ActivityQty)));

                    #region Row Styles
                    ws.Cells["D" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));
                    ws.Cells["E" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
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
                    //}
                }

                var total = invoice.Activities.Sum(x => x.ActivityCost * Convert.ToDouble(x.ActivityQty));
                var gct = total * Convert.ToDouble(invoice.GCT);
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
                ws.Cells["A" + y].Value = "GCT(" + (invoice.GCT * 100).ToString("#.##") + "):";
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

                #region Material Usage
                ws.Cells[y, 1, y, 5].Merge = true;
                y++;
                ws.Cells[y, 1, y, 5].Merge = true;
                y++;

                #region Title
                ws.Cells[y, 1, y, 5].Merge = true;
                ws.Cells["A" + y].Value = "Material Usage:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                #endregion
                y++;

                #region Table Headers
                ws.Cells["A" + y].Value = "Nos.";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 2, y, 4].Merge = true;
                ws.Cells["B" + y].Value = "Description";
                ws.Cells[y, 2, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = "Qty";
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

                int matCount = 1;
                foreach (var mat in invoice.Materials)
                {
                    #region Rows
                    ws.Cells["A" + y].Value = matCount;
                    ws.Cells[y, 2, y, 4].Merge = true;
                    ws.Cells["B" + y].Value = mat.MatDesc;
                    ws.Cells["E" + y].Value = mat.MatQty;

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

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

                #endregion

                #region BreakDown
                if (model.HasBreakDown)
                {
                    var breakDown = BreakDownData(model.InvoiceNo);
                    if (breakDown != null)
                    {
                        ws.Cells[y, 1, y, 5].Merge = true;
                        y++;

                        #region BreakDown Header

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Requested By:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = breakDown.RequestedBy;
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Request Date:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "Week Ending" + string.Format("{0:MMMM dd, yyyy}", breakDown.DateRequested);
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Issue Date:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = string.Format("{0:MMMM dd, yyyy}", breakDown.DateDispatched);
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Completion Date:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = string.Format("{0:MMMM dd, yyyy}", DateTime.Now);
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Department:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Field Survey:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "No";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Strand:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "No";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Fiber:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "Yes";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Drafting:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "No";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Transportation:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "No";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Location:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "";
                        y++;

                        #endregion

                        ws.Cells[y, 1, y, 5].Merge = true;
                        y++;

                        #region BreakDown Title
                        ws.Cells[y, 1, y, 5].Merge = true;
                        ws.Cells["A" + y].Value = "Work Order Details";
                        ws.Cells["A" + y].Style.Font.Bold = true;
                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["A" + y].Style.Font.Name = "Arial";
                        ws.Cells["A" + y].Style.Font.Size = 12;
                        y++;
                        #endregion

                        ws.Cells[y, 1, y, 5].Merge = true;
                        y++;

                        #region BreakDown Table Headers
                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Date";
                        ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "Description";
                        ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                        ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                        ws.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                        ws.Cells[y, 1, y, 5].Style.Font.Size = 12;
                        ws.Cells[y, 1, y, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                        ws.Cells[y, 1, y, 5].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                        ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        #endregion
                        y++;

                        #region BreakDown rows

                        foreach (var l in breakDown.Activities.Select(x => x.Location).Distinct())
                        {
                            ws.Cells[y, 1, y, 2].Merge = true;
                            ws.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", breakDown.Activities.FirstOrDefault(x => x.Location == l).Date);
                            ws.Cells[y, 3, y, 5].Merge = true;
                            ws.Cells["C" + y].Value = l;

                            #region Row Styles

                            ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells["C" + y].Style.Font.Bold = true;

                            ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                            ws.Cells[y, 3, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                            ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                            if (y % 2 != 0)
                            {
                                ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                            }
                            #endregion
                            y++;

                            foreach (var act in breakDown.Activities.Where(x => x.Location == l))
                            {
                                ws.Cells[y, 1, y, 2].Merge = true;
                                ws.Cells["A" + y].Value = "-";
                                ws.Cells[y, 3, y, 4].Merge = true;
                                ws.Cells["C" + y].Value = act.Description;

                                ws.Cells["E" + y].Value = act.Qty;

                                #region Row Styles

                                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                                ws.Cells[y, 3, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                                ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                                if (y % 2 != 0)
                                {
                                    ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                                }
                                #endregion
                                y++;
                            }
                        }

                        #endregion
                    }
                }
                #endregion

                #endregion
            }
            if (pck.Workbook.Worksheets.Count() > 0)
            {
                pck.Save();
                //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //Response.AddHeader("content-disposition", "attachment;  filename=" + invNo + ".xlsx");
            }
        }

        public ClientInvoice NewClientInvoiceData(int id)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var refNum = db.TRN23110.FirstOrDefault(x => x.TRN09110.Any(y => y.InvoiceNum == id)).TRN23100.Wo_ref;
            var refWOIds = db.TRN23100.Where(x => x.Wo_ref == refNum).Select(x => x.Workid).ToArray();
            var activities = db.TRN23110.Where(x => x.TRN09110.Any(y => y.InvoiceNum == id)).AsEnumerable().Select(x => new ClientInvoiceActivities
            //var activities = db.TRN23110.Where(x => refWOIds.Contains(x.WorkOID)).AsEnumerable().Select(x => new ClientInvoiceActivities
            {
                //ActivityCost = x.ADM01100.GetRateAmountForDate(x.ActDate),
                ActivityCost = x.ADM01100.GetClientRateAmountForDate(x.ActDate),
                ActivityDate = x.ActDate,
                ActivityDesc = x.ADM01100.RateDescr,
                ActivityLocation = x.Location,
                ActivityQty = x.TRN09110.FirstOrDefault(y => y.InvoiceNum == id).InvoicedAmount,
                Comments = x.AdtnlDetails
            }).ToList();
            var mats = db.TRN23120.Where(x => x.TRN23110.TRN09110.Any(y => y.InvoiceNum == id)).AsEnumerable().Select(x => new ClientInvoiceActivityMaterial
            {
                MatDesc = db.ADM13100.FirstOrDefault(m => m.MaterialID == x.WoMatID).MaterialName,
                ActivityDate = x.TRN23110.ActDate,
                ActivityLocation = x.TRN23110.Location,
                Comments = x.TRN23110.AdtnlDetails,
                MatCost = 0,
                MatQty = x.WoMatQty
            }).ToList();
            var invoice = db.TRN09100.Where(x => x.InvoiceNum == id).AsEnumerable().Select(x => new ClientInvoice
            {
                Title = x.InvoiceTitle.ToUpper(),
                Date = x.InvoiceDate,
                Client = new ClientData
                {
                    Date = x.InvoiceDate,
                    InvoiceNo = x.InvoiceNum,
                    ReferenceNo = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_ref,
                    Attention = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Requestby,
                    ClientName = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM03200.CustName,
                    Currency = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM03200.Currency,
                    Email = string.IsNullOrEmpty(x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Email) ?
                       "Not Specified" : x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Email,
                    Address = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Addr
                        + ", " + x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.areaName
                        + ", " + x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Name,
                    Phone = string.IsNullOrEmpty(x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Phone) ?
                       "Not Specified" : x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Phone,
                    PONo = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.PONum
                }
            }).FirstOrDefault();
            invoice.Client.User = user;
            invoice.Activities = activities;
            invoice.Materials = mats;
            var gct = db.ADM07100.FirstOrDefault(x => x.StartDate >= invoice.Date && x.EndDate <= invoice.Date);

            invoice.GCT = Convert.ToDecimal((gct == null ? db.ADM07100.FirstOrDefault(x => x.EndDate == null).Percentage : gct.Percentage) / 100);
            invoice.Client.GCTNo = Customs.GetSettingsFileValue("GctReg", settingFilePath);
            return invoice;
        }
        #endregion

        #region Invoice Summary
        public ActionResult NewSummary(int id)
        {
            var invoice = NewSummaryData(id);
            ViewBag.InvoiceNo = id;
            return View(invoice);
        }
        [HttpPost]
        public void NewSummaryToExcel(ClientInvoiceFilter model)
        {
            ExcelPackage pck = new ExcelPackage();
            int invNo = 0;
            var invoice = NewSummaryData(model.InvoiceNo);
            if (invoice != null)
            {
                #region Work Sheet

                if (invoice.Client.InvoiceNo > 0)
                    invNo = Customs.MakeGenericInvoiceNo(invoice.Client.InvoiceNo);
                var ws = pck.Workbook.Worksheets.Add(invNo.ToString());

                #region View and Print Seetings

                //ws.View.PageLayoutView = true;
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
                else
                    logoSearchPath = Path.Combine(_env.WebRootPath, "Images", "print-logo.png");
                FileInfo fi = new FileInfo(logoSearchPath);
                var image = ws.Drawings.AddPicture("print-logo.png", fi);
                image.SetPosition(0, 0);
                image.SetSize(205, 111);



                ws.Cells["C" + y].Value = "P.O. BOX 3069";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                if (invoice.Client.InvoiceNo > 0)
                {
                    ws.Cells["D" + y].Value = "Invoice #:";
                    ws.Cells["E" + y].Value = Customs.MakeGenericInvoiceNo(invoice.Client.InvoiceNo);
                    ws.Cells["D" + y].Style.Font.Bold = true;
                    ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["C" + y].Value = "Kingston 8";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                if (invoice.Client.InvoiceNo > 0)
                {
                    ws.Cells["D" + y].Value = "GCT #:";
                    ws.Cells["E" + y].Value = invoice.Client.GCTNo;
                    ws.Cells["D" + y].Style.Font.Bold = true;
                }
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["C" + y].Value = "Tel: (876) 630-5278";
                ws.Cells["C" + y].Style.Font.Bold = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";

                ws.Cells["D" + y].Value = "GR #";
                ws.Cells["E" + y].Value = "5000433798";
                ws.Cells["D" + y].Style.Font.Bold = true;
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
                ws.Cells["B" + y].Value = invoice.Client.ClientName;

                ws.Cells["D" + y].Value = "Date:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = string.Format("{0:dd/MM/yyyy}", invoice.Client.Date);
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Attn.:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = invoice.Client.Attention;

                ws.Cells["D" + y].Value = "Prepared By:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = invoice.Client.User;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Address:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = invoice.Client.Address;

                ws.Cells["D" + y].Value = "Currency:";
                ws.Cells["D" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Value = invoice.Client.Currency;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Phone:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = invoice.Client.Phone;
                if (invoice.Client.InvoiceNo > 0)
                {
                    //ws.Cells["D" + y].Value = "Ticket #:";
                    //ws.Cells["D" + y].Style.Font.Bold = true;
                    //ws.Cells["E" + y].Value = invoice.Client.ReferenceNo;
                }
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws.Cells["A" + y].Value = "Email:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells[y, 2, y, 3].Merge = true;
                ws.Cells["B" + y].Value = invoice.Client.Email;

                //ws.Cells["D" + y].Value = "PO #:";
                //ws.Cells["D" + y].Style.Font.Bold = true;
                //ws.Cells["E" + y].Value = invoice.Client.PONo;
                ws.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                #endregion
                y++;
                y++;


                //#region Title
                //ws.Cells[y, 1, y, 5].Merge = true;
                //ws.Cells["A" + y].Value = invoice.Title;
                //ws.Cells["A" + y].Style.Font.Bold = true;
                //ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //ws.Cells["A" + y].Style.Font.Name = "Arial";
                //ws.Cells["A" + y].Style.Font.Size = 12;
                //#endregion
                //y++;

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


                #region Location
                ws.Cells[y, 2, y, 5].Merge = true;
                ws.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", invoice.Date);
                ws.Cells["B" + y].Value = invoice.Title.ToUpper();
                ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 5].Style.Font.Size = 12;
                ws.Cells[y, 1, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                #endregion
                y++;

                foreach (var desc in invoice.Activities.Select(x => x.ActivityDesc).Distinct())
                {
                    //#region Location
                    //ws.Cells[y, 1, y, 5].Merge = true;
                    //ws.Cells["A" + y].Value = loc.ActivityLocation;
                    //ws.Cells["A" + y].Style.Font.Bold = true;
                    //ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //ws.Cells["A" + y].Style.Font.Name = "Inherit";
                    //ws.Cells["A" + y].Style.Font.Size = 12;
                    //ws.Cells[y, 1, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    //ws.Cells["A" + y].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //ws.Cells["A" + y].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    //#endregion
                    //y++;

                    var act = invoice.Activities.Where(x => x.ActivityDesc == desc).ToList();

                    //foreach (var act in invoice.Activities.Where(x => x.ActivityDesc == desc))
                    //{
                    #region Rows
                    ws.Cells["A" + y].Value = "";
                    ws.Cells["B" + y].Value = act.Sum(x => x.ActivityQty);
                    ws.Cells["C" + y].Value = desc;
                    ws.Cells["D" + y].Value = string.Format("{0:C}", act[0].ActivityCost);
                    ws.Cells["E" + y].Value = string.Format("{0:C}", act[0].ActivityCost * Convert.ToDouble(act.Sum(x => x.ActivityQty)));

                    #region Row Styles
                    ws.Cells["D" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));
                    ws.Cells["E" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
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
                    //}
                }

                var total = invoice.Activities.Sum(x => x.ActivityCost * Convert.ToDouble(x.ActivityQty));
                var gct = total * Convert.ToDouble(invoice.GCT);
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
                ws.Cells["A" + y].Value = "GCT(" + (invoice.GCT * 100).ToString("#.##") + "):";
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

                #region Material Usage
                ws.Cells[y, 1, y, 5].Merge = true;
                y++;
                ws.Cells[y, 1, y, 5].Merge = true;
                y++;

                #region Title
                ws.Cells[y, 1, y, 5].Merge = true;
                ws.Cells["A" + y].Value = "Material Usage:";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                #endregion
                y++;

                #region Table Headers
                ws.Cells["A" + y].Value = "Nos.";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 2, y, 4].Merge = true;
                ws.Cells["B" + y].Value = "Description";
                ws.Cells[y, 2, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = "Qty";
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

                int matCount = 1;
                foreach (var mat in invoice.Materials)
                {
                    #region Rows
                    ws.Cells["A" + y].Value = matCount;
                    ws.Cells[y, 2, y, 4].Merge = true;
                    ws.Cells["B" + y].Value = mat.MatDesc;
                    ws.Cells["E" + y].Value = mat.MatQty;

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

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

                #endregion

                #region BreakDown
                if (model.HasBreakDown)
                {
                    var breakDown = BreakDownData(model.InvoiceNo);
                    if (breakDown != null)
                    {
                        ws.Cells[y, 1, y, 5].Merge = true;
                        y++;

                        #region BreakDown Header

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Requested By:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = breakDown.RequestedBy;
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Request Date:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "Week Ending" + string.Format("{0:MMMM dd, yyyy}", breakDown.DateRequested);
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Issue Date:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = string.Format("{0:MMMM dd, yyyy}", breakDown.DateDispatched);
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Completion Date:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = string.Format("{0:MMMM dd, yyyy}", DateTime.Now);
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Department:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Field Survey:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "No";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Strand:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "No";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Fiber:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "Yes";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Drafting:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "No";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Transportation:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "No";
                        y++;

                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Location:";
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "";
                        y++;

                        #endregion

                        ws.Cells[y, 1, y, 5].Merge = true;
                        y++;

                        #region BreakDown Title
                        ws.Cells[y, 1, y, 5].Merge = true;
                        ws.Cells["A" + y].Value = "Work Order Details";
                        ws.Cells["A" + y].Style.Font.Bold = true;
                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["A" + y].Style.Font.Name = "Arial";
                        ws.Cells["A" + y].Style.Font.Size = 12;
                        y++;
                        #endregion

                        ws.Cells[y, 1, y, 5].Merge = true;
                        y++;

                        #region BreakDown Table Headers
                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "Date";
                        ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells[y, 3, y, 5].Merge = true;
                        ws.Cells["C" + y].Value = "Description";
                        ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                        ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                        ws.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                        ws.Cells[y, 1, y, 5].Style.Font.Size = 12;
                        ws.Cells[y, 1, y, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                        ws.Cells[y, 1, y, 5].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                        ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        #endregion
                        y++;

                        #region BreakDown rows

                        foreach (var l in breakDown.Activities.Select(x => x.Location).Distinct())
                        {
                            ws.Cells[y, 1, y, 2].Merge = true;
                            ws.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", breakDown.Activities.FirstOrDefault(x => x.Location == l).Date);
                            ws.Cells[y, 3, y, 5].Merge = true;
                            ws.Cells["C" + y].Value = l;

                            #region Row Styles

                            ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            ws.Cells["C" + y].Style.Font.Bold = true;

                            ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                            ws.Cells[y, 3, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                            ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                            if (y % 2 != 0)
                            {
                                ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                            }
                            #endregion
                            y++;

                            foreach (var act in breakDown.Activities.Where(x => x.Location == l))
                            {
                                ws.Cells[y, 1, y, 2].Merge = true;
                                ws.Cells["A" + y].Value = "-";
                                ws.Cells[y, 3, y, 4].Merge = true;
                                ws.Cells["C" + y].Value = act.Description;

                                ws.Cells["E" + y].Value = act.Qty;

                                #region Row Styles

                                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                                ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                                ws.Cells[y, 3, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                                ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                                if (y % 2 != 0)
                                {
                                    ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                                }
                                #endregion
                                y++;
                            }
                        }

                        #endregion
                    }
                }
                #endregion

                #endregion
            }
            if (pck.Workbook.Worksheets.Count() > 0)
            {
                pck.Save();
                //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //Response.AddHeader("content-disposition", "attachment;  filename=" + invNo + ".xlsx");
            }
        }

        public ClientInvoice NewSummaryData(int id)
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var refNum = db.TRN23110.FirstOrDefault(x => x.TRN09110.Any(y => y.InvoiceNum == id)).TRN23100.Wo_ref;
            var refWOIds = db.TRN23100.Where(x => x.Wo_ref == refNum).Select(x => x.Workid).ToArray();
            var activities = db.TRN23110.Where(x => x.TRN09110.Any(y => y.InvoiceNum == id)).AsEnumerable().Select(x => new ClientInvoiceActivities
            //var activities = db.TRN23110.Where(x => refWOIds.Contains(x.WorkOID)).AsEnumerable().Select(x => new ClientInvoiceActivities
            {
                //ActivityCost = x.ADM01100.GetRateAmountForDate(x.ActDate),
                ActivityCost = x.ADM01100.GetClientRateAmountForDate(x.ActDate),
                ActivityDate = x.ActDate,
                ActivityDesc = x.ADM01100.RateDescr,
                ActivityLocation = x.Location,
                ActivityQty = x.TRN09110.FirstOrDefault(y => y.InvoiceNum == id).InvoicedAmount,
                Comments = x.AdtnlDetails,
                AltCode = x.ADM01100.AltCode,
                RefNum = x.TRN23100.Wo_ref,
                ActId = x.ActivityID,
                WOSubmittedDate = x.TRN23100.DateSubmitted.Value,
                WOTitle = x.TRN23100.Wo_title.ToUpper(),
                WoId = x.TRN23100.Workid,
                TeamName = GetTeamMemberName(x.TRN23100.ADM03400.Select(p => p.ContractorID).ToList()), //string.Join(" , ", db.ADM03300.Where(t => x.TRN23100.ADM03400.Select(p => p.ContractorID).Contains(t.ConID)).Select(t => t.FirstName).ToList())
            }).OrderBy(p => p.ActivityDate).ThenBy(p => p.WoId).ToList();
            var mats = db.TRN23120.Where(x => x.TRN23110.TRN09110.Any(y => y.InvoiceNum == id)).AsEnumerable().Select(x => new ClientInvoiceActivityMaterial
            {
                MatDesc = db.ADM13100.FirstOrDefault(m => m.MaterialID == x.WoMatID).MaterialName,
                ActivityDate = x.TRN23110.ActDate,
                ActivityLocation = x.TRN23110.Location,
                Comments = x.TRN23110.AdtnlDetails,
                MatCost = 0,
                MatQty = x.WoMatQty
            }).ToList();
            var invoice = db.TRN09100.Where(x => x.InvoiceNum == id).AsEnumerable().Select(x => new ClientInvoice
            {
                Title = x.InvoiceTitle, //x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_title.ToUpper(),
                Date = x.InvoiceDate,
                Client = new ClientData
                {
                    Date = x.InvoiceDate,
                    InvoiceNo = x.InvoiceNum,
                    ReferenceNo = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Wo_ref,
                    Attention = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.Requestby,
                    ClientName = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM03200.CustName,
                    Currency = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM03200.Currency,
                    Email = string.IsNullOrEmpty(x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Email) ?
                       "Not Specified" : x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Email,
                    Address = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Addr,
                    //+ ", " + x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.areaName
                    //+ ", " + x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Name,
                    Phone = string.IsNullOrEmpty(x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Phone) ?
                       "Not Specified" : x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Phone,
                    PONo = x.TRN09110.Any(a => a.TRN23110.TRN23100.PONum != "") ? x.TRN09110.FirstOrDefault(a => a.TRN23110.TRN23100.PONum != "").TRN23110.TRN23100.PONum : "",
                    Region = x.TRN09110.FirstOrDefault().TRN23110.TRN23100.ADM01400.ADM26100.Name
                }
            }).FirstOrDefault();
            var dates = db.TRN09110.Where(x => x.InvoiceNum == id).Select(x => x.TRN23110.TRN23100.DateSubmitted).Distinct().OrderBy(x => x.Value).ToList();
            invoice.Client.FromDate = dates[0].Value;
            invoice.Client.ToDate = dates[dates.Count - 1].Value;
            invoice.Client.User = user;
            invoice.Activities = activities;
            invoice.Materials = mats;
            var gct = db.ADM07100.FirstOrDefault(x => x.StartDate >= invoice.Date && x.EndDate <= invoice.Date);

            invoice.GCT = Convert.ToDecimal((gct == null ? db.ADM07100.FirstOrDefault(x => x.EndDate == null).Percentage : gct.Percentage) / 100);
            invoice.Client.GCTNo = Customs.GetSettingsFileValue("GctReg", settingFilePath);
            return invoice;
        }

        public string GetTeamMemberName(List<int> ContractorIDs)
        {
            List<ProfileInfo> lstNames = db.ADM03300.Where(t => ContractorIDs.Contains(t.ConID)).Select(t => new ProfileInfo { FirstName = t.FirstName + " " + t.LastName }).ToList();
            return string.Join(" , ", lstNames.Select(p => p.FirstName).ToList());
        }


        #endregion

        #region Cabling Work Diary

        //[HttpPost]
        public void CablingWorkDiary(int id)
        {
            ExcelPackage pck = new ExcelPackage();
            int invNo = 0;
            var invoice = NewSummaryData(id);
            string comName = Customs.GetSettingsFileValue("Name", settingFilePath);
            string street = Customs.GetSettingsFileValue("Street", settingFilePath);
            string city = Customs.GetSettingsFileValue("City", settingFilePath);
            if (invoice != null)
            {
                #region Cabling Work Diary Work Sheet

                if (invoice.Client.InvoiceNo > 0)
                    invNo = Customs.MakeGenericInvoiceNo(invoice.Client.InvoiceNo);
                var ws = pck.Workbook.Worksheets.Add("Cabling Work Diary");

                #region View and Print Seetings

                //ws.View.PageLayoutView = true;
                ws.PrinterSettings.ShowGridLines = false;

                //ws.PrinterSettings.PaperSize = ePaperSize.A4;
                ws.PrinterSettings.TopMargin = 0.75M;
                ws.PrinterSettings.RightMargin = 0.25M;
                ws.PrinterSettings.BottomMargin = 0.75M;
                ws.PrinterSettings.LeftMargin = 0.25M;
                ws.PrinterSettings.HeaderMargin = 0.0M;
                ws.PrinterSettings.FooterMargin = 0.0M;

                #endregion

                //ws.Protection.IsProtected = true;
                //ws.Protection.SetPassword("Password123.");

                ws.Cells.Style.Font.Size = 11;
                ws.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Column(1).Width = 13;
                ws.Column(2).Width = 13;
                ws.Column(3).Width = 13;

                ws.Column(4).Width = 13;
                ws.Column(5).Width = 13;
                ws.Column(6).Width = 9;
                ws.Column(7).Width = 9;
                ws.Column(8).Width = 9;
                ws.Column(9).Width = 9;
                ws.Column(10).Width = 9;
                ws.Column(11).Width = 9;

                int y = 1;

                #region Logo
                string logoSearchPath = "/images/flow_logo.jpg";
                if (Directory.Exists(Path.Combine(_env.WebRootPath, "work")))
                    logoSearchPath = "/work/images/flow_logo.jpg";
                if (Directory.Exists(Path.Combine(_env.WebRootPath, "testing")))
                    logoSearchPath = "/testing/images/flow_logo.jpg";
                else
                    logoSearchPath = Path.Combine(_env.WebRootPath, "Images", "flow_logo.png");
                FileInfo fi = new FileInfo(logoSearchPath);
                var image = ws.Drawings.AddPicture("flow_logo.jpg", fi);
                image.SetPosition(25, 50);
                image.SetSize(129, 70);
                image.AdjustPositionAndSize();
                #endregion

                y++;

                #region Title

                ws.Cells[y, 10, y, 17].Merge = true;
                ws.Cells["J" + y].Value = "Cabling Work Diary - Jamaica".ToUpper();
                ws.Cells["J" + y].Style.Font.Name = "Bell MT";
                ws.Cells["J" + y].Style.Font.Size = 15;
                ws.Cells["J" + y].Style.Font.UnderLine = true;
                ws.Cells["J" + y].Style.Font.Bold = true;
                ws.Row(y).Height = 35;
                y++;

                #endregion
                y++;
                y++;

                ws.Cells[5, 1, 9, 43].Style.Font.Size = 12;
                ws.Cells[5, 1, 9, 43].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                #region Header row 01
                ws.Row(y).Height = 20;
                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Invoice No:";
                ws.Cells["A" + y].Style.Font.Name = "Bell MT";
                ws.Cells["A" + y].Style.Font.Size = 14;
                ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 3, y, 6].Merge = true;
                ws.Cells["C" + y].Value = invNo;
                ws.Cells["C" + y].Style.Font.Name = "Arial";
                ws.Cells[y, 3, y, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws.Cells[y, 3, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 3, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                ws.Cells[y, 8, y, 10].Merge = true;
                ws.Cells["H" + y].Value = "Contractor Name:";
                ws.Cells["H" + y].Style.Font.Name = "Bell MT";
                ws.Cells["H" + y].Style.Font.Size = 14;
                ws.Cells[y, 8, y, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 11, y, 22].Merge = true;
                ws.Cells["K" + y].Value = comName.ToUpper();
                ws.Cells["K" + y].Style.Font.Name = "Arial";
                ws.Cells[y, 11, y, 22].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws.Cells[y, 11, y, 22].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 11, y, 22].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                ws.Cells[y, 23, y, 24].Merge = true;
                ws.Cells["W" + y].Value = "Address:";
                ws.Cells["W" + y].Style.Font.Name = "Bell MT";
                ws.Cells["W" + y].Style.Font.Size = 14;
                ws.Cells[y, 23, y, 24].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 25, y, 43].Merge = true;
                ws.Cells["Y" + y].Value = (street + ", " + city).ToUpper();
                ws.Cells["Y" + y].Style.Font.Name = "Arial";
                ws.Cells[y, 25, y, 43].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws.Cells[y, 25, y, 43].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 25, y, 43].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));
                y++;

                #endregion

                #region Header row 02
                ws.Row(y).Height = 20;
                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "PO No:";
                ws.Cells["A" + y].Style.Font.Name = "Bell MT";
                ws.Cells["A" + y].Style.Font.Size = 14;
                ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 3, y, 6].Merge = true;
                ws.Cells["C" + y].Value = invoice.Client.PONo;
                ws.Cells["C" + y].Style.Font.Name = "Arial";
                ws.Cells[y, 3, y, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws.Cells[y, 3, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 3, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                ws.Cells[y, 8, y, 10].Merge = true;
                ws.Cells["H" + y].Value = "Vendor No:";
                ws.Cells["H" + y].Style.Font.Name = "Bell MT";
                ws.Cells["H" + y].Style.Font.Size = 14;
                ws.Cells[y, 8, y, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 11, y, 13].Merge = true;
                ws.Cells["K" + y].Value = "10038549";
                ws.Cells["K" + y].Style.Font.Name = "Arial";
                ws.Cells[y, 11, y, 13].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws.Cells[y, 11, y, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 11, y, 13].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                ws.Cells[y, 14, y, 16].Merge = true;
                ws.Cells["N" + y].Value = "Work Period:";
                ws.Cells["N" + y].Style.Font.Name = "Bell MT";
                ws.Cells["N" + y].Style.Font.Size = 14;
                ws.Cells[y, 14, y, 16].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 17, y, 22].Merge = true;
                ws.Cells["Q" + y].Value = string.Format("{0:MMM dd, yy}", invoice.Client.FromDate) + " - " + string.Format("{0:MMM dd, yy}", invoice.Client.ToDate);
                ws.Cells["Q" + y].Style.Font.Name = "Arial";
                ws.Cells[y, 17, y, 22].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws.Cells[y, 17, y, 22].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 17, y, 22].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                ws.Cells[y, 23, y, 25].Merge = true;
                ws.Cells["W" + y].Value = "Approved By:";
                ws.Cells["W" + y].Style.Font.Name = "Bell MT";
                ws.Cells["W" + y].Style.Font.Size = 14;
                ws.Cells[y, 23, y, 25].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 26, y, 34].Merge = true;
                ws.Cells["Z" + y].Style.Font.Name = "Arial";
                ws.Cells[y, 26, y, 34].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws.Cells[y, 26, y, 34].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 26, y, 34].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));
                y++;

                #endregion

                #region Header row 03
                ws.Row(y).Height = 20;
                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Region:";
                ws.Cells["A" + y].Style.Font.Name = "Bell MT";
                ws.Cells["A" + y].Style.Font.Size = 14;
                ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 3, y, 6].Merge = true;
                ws.Cells["C" + y].Value = invoice.Client.Region;
                ws.Cells["C" + y].Style.Font.Name = "Arial";
                ws.Cells[y, 3, y, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws.Cells[y, 3, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 3, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                ws.Cells[y, 8, y, 10].Merge = true;
                ws.Cells["H" + y].Value = "Submitted By:";
                ws.Cells["H" + y].Style.Font.Name = "Bell MT";
                ws.Cells["H" + y].Style.Font.Size = 14;
                ws.Cells[y, 8, y, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 11, y, 15].Merge = true;
                ws.Cells["K" + y].Value = "Dave Campbell";
                ws.Cells["K" + y].Style.Font.Name = "Arial";
                ws.Cells[y, 11, y, 15].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws.Cells[y, 11, y, 15].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 11, y, 15].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                ws.Cells[y, 16, y, 18].Merge = true;
                ws.Cells["P" + y].Value = "Submitted on:";
                ws.Cells["P" + y].Style.Font.Name = "Bell MT";
                ws.Cells["P" + y].Style.Font.Size = 14;
                ws.Cells[y, 16, y, 18].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 19, y, 24].Merge = true;
                ws.Cells["S" + y].Value = string.Format("{0:dd/MMM/yyyy}", DateTime.Now);
                ws.Cells["S" + y].Style.Font.Name = "Arial";
                ws.Cells[y, 19, y, 24].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws.Cells[y, 19, y, 24].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 19, y, 24].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                ws.Cells[y, 25, y, 27].Merge = true;
                ws.Cells["Y" + y].Value = "Approved on:";
                ws.Cells["Y" + y].Style.Font.Name = "Bell MT";
                ws.Cells["Y" + y].Style.Numberformat.Format = "dd/MMM/yyyy";
                ws.Cells["Y" + y].Style.Font.Size = 14;
                ws.Cells[y, 25, y, 27].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 28, y, 32].Merge = true;
                ws.Cells["AB" + y].Style.Font.Name = "Arial";
                ws.Cells[y, 28, y, 32].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws.Cells[y, 28, y, 32].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 28, y, 32].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));
                y++;

                #endregion

                y++;

                #region Table Titles
                ws.Row(y).Style.Font.Bold = true;
                ws.Row(y).Style.Font.Size = 12;
                ws.Cells[y, 6, y, 11].Merge = true;
                ws.Cells["F" + y].Value = "Work Description And Location".ToUpper();
                ws.Cells["F" + y].Style.Font.Name = "Bell MT";
                ws.Cells["F" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells[y, 16, y, 24].Merge = true;
                ws.Cells["P" + y].Value = "Item No. & Quantity of Work Completed".ToUpper();
                ws.Cells["P" + y].Style.Font.Name = "Bell MT";
                ws.Cells["P" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                y++;

                #endregion

                #region Table Headers
                ws.Row(y).Style.Font.Bold = true;
                ws.Cells[y, 1, y + 2, 1].Merge = true;
                ws.Cells["A" + y].Value = "Date".ToUpper();
                ws.Cells["A" + y].Style.Font.Name = "Bell MT";
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["A" + y].Style.WrapText = true;
                ws.Cells[y, 1, y + 2, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 2, y + 2, 2].Merge = true;
                ws.Cells["B" + y].Value = "Job Order No.".ToUpper();
                ws.Cells["B" + y].Style.Font.Name = "Bell MT";
                ws.Cells["B" + y].Style.Font.Size = 10;
                ws.Cells["B" + y].Style.WrapText = true;
                ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y + 2, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 3, y + 2, 3].Merge = true;
                ws.Cells["C" + y].Value = "Work Order/I.D. No.".ToUpper();
                ws.Cells["C" + y].Style.Font.Name = "Bell MT";
                ws.Cells["C" + y].Style.Font.Size = 10;
                ws.Cells["C" + y].Style.WrapText = true;
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y + 2, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 4, y + 2, 4].Merge = true;
                ws.Cells["D" + y].Value = "REF#".ToUpper();
                ws.Cells["D" + y].Style.Font.Name = "Bell MT";
                ws.Cells["D" + y].Style.Font.Size = 10;
                ws.Cells["D" + y].Style.WrapText = true;
                ws.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y + 2, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 5, y + 2, 12].Merge = true;
                ws.Cells["E" + y].Value = "Description and Location to Include Joint Box or M/H No., Pole Address and Street Address".ToUpper();
                ws.Cells["E" + y].Style.Font.Name = "Bell MT";
                ws.Cells["E" + y].Style.Font.Size = 10;
                ws.Cells["E" + y].Style.WrapText = true;
                ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y + 2, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                var acts = invoice.Activities.Select(x => new { x.ActId, x.AltCode, x.ActivityDesc }).OrderBy(x => x.AltCode).Distinct();
                int realChar = Convert.ToInt32('M');
                int X = 13;
                foreach (var act in acts)
                {
                    string colCode = GetColumnCode(X);
                    ws.Column(X).Width = 6;
                    ws.Cells[colCode + y].Value = "Item".ToUpper();
                    ws.Cells[colCode + y].Style.Font.Name = "Bell MT";
                    ws.Cells[colCode + y].Style.Font.Size = 8;
                    ws.Cells[colCode + y].Style.Font.Bold = false;

                    ws.Cells[colCode + (y + 1)].Value = string.IsNullOrEmpty(act.AltCode) ? "" : act.AltCode.ToUpper();
                    ws.Cells[colCode + (y + 1)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[colCode + (y + 1)].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));
                    ws.Cells[colCode + (y + 1)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[colCode + (y + 1)].Style.Font.Size = 8;

                    ws.Cells[colCode + (y + 2)].Value = "Qty".ToUpper();
                    ws.Cells[colCode + (y + 2)].Style.Font.Name = "Bell MT";
                    ws.Cells[y, X, y + 2, X].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    ws.Cells[colCode + (y + 2)].Style.Font.Size = 8;

                    X++;
                }

                if (acts.Count() < 17)
                {
                    for (; X < 13 + 17; X++)
                    {
                        ws.Column(X).Width = 6;
                        string colCode = GetColumnCode(X);

                        ws.Cells[colCode + y].Value = "Item".ToUpper();
                        ws.Cells[colCode + y].Style.Font.Name = "Bell MT";
                        ws.Cells[colCode + y].Style.Font.Size = 8;

                        ws.Cells[colCode + (y + 1)].Value = "".ToUpper();
                        ws.Cells[colCode + (y + 1)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[colCode + (y + 1)].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));
                        ws.Cells[colCode + (y + 1)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[colCode + (y + 1)].Style.Font.Size = 8;

                        ws.Cells[colCode + (y + 2)].Value = "Qty".ToUpper();
                        ws.Cells[colCode + (y + 2)].Style.Font.Name = "Bell MT";
                        ws.Cells[y, X, y + 2, X].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                        ws.Cells[colCode + (y + 2)].Style.Font.Size = 8;
                    }
                }
                y = y + 3;

                #endregion

                #region Rows
                foreach (var refNum in invoice.Activities.Select(x => x.RefNum).Distinct())
                {
                    var wo = invoice.Activities.Find(x => x.RefNum == refNum);
                    //foreach (var loc in invoice.Activities.Where(x => x.RefNum == refNum).Select(x => new { x.ActivityLocation, x.Comments, x.ActivityDate }).Distinct())
                    //{
                    ws.Row(y).Height = 30;
                    ws.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", wo.ActivityDate);
                    ws.Cells["A" + y].Style.Font.Name = "Arial";
                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    ws.Cells["A" + y].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells["A" + y].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                    ws.Cells["B" + y].Value = "".ToUpper();
                    ws.Cells["B" + y].Style.Font.Name = "Arial";
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    ws.Cells["B" + y].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells["B" + y].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                    ws.Cells["C" + y].Value = refNum.ToUpper();
                    ws.Cells["C" + y].Style.Font.Name = "Arial";
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    ws.Cells["C" + y].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells["C" + y].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                    ws.Cells["D" + y].Value = wo.WoId;
                    ws.Cells["D" + y].Style.Font.Name = "Arial";
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    ws.Cells["D" + y].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells["D" + y].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                    ws.Cells[y, 5, y, 12].Merge = true;
                    ws.Cells["E" + y].Value = wo.WOTitle.ToUpper();//(string.IsNullOrEmpty(loc.ActivityLocation) ? "" : loc.ActivityLocation.ToUpper()) + (string.IsNullOrEmpty(loc.Comments) ? "" : ", " + loc.Comments.ToUpper());
                    ws.Cells["E" + y].Style.Font.Name = "Arial";
                    ws.Cells[y, 5, y, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                    ws.Cells[y, 5, y, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 5, y, 12].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                    X = 13;
                    foreach (var ALTcODE in acts)
                    {
                        //var dtl = invoice.Activities.Find(x => x.RefNum == refNum && x.ActivityLocation == loc.ActivityLocation && x.ActivityDesc == ALTcODE.ActivityDesc);
                        var dtl = invoice.Activities.FindAll(x => x.RefNum == refNum && x.ActId == ALTcODE.ActId);
                        string colCode = GetColumnCode(X);

                        ws.Cells[colCode + y].Value = dtl != null && dtl.Count > 0 ? dtl.Sum(x => x.ActivityQty).ToString() : "";
                        ws.Cells[colCode + y].Style.Font.Name = "Arial";
                        ws.Cells[colCode + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        ws.Cells[colCode + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                        ws.Cells[colCode + y].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[colCode + y].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));

                        X++;
                    }

                    if (acts.Count() < 17)
                    {
                        for (; X < 13 + 17; X++)
                        {
                            string colCode = GetColumnCode(X);

                            ws.Cells[colCode + y].Value = "";
                            ws.Cells[colCode + y].Style.Font.Name = "Arial";
                            ws.Cells[colCode + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                            ws.Cells[colCode + y].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[colCode + y].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));
                        }
                    }
                    y++;
                    //}
                }

                #endregion

                #region Total
                ws.Row(y).Height = 30;
                ws.Cells["A" + y].Value = "";
                ws.Cells["A" + y].Style.Font.Name = "Bell MT";
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells["B" + y].Value = "";
                ws.Cells["B" + y].Style.Font.Name = "Bell MT";
                ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells["C" + y].Value = "";
                ws.Cells["C" + y].Style.Font.Name = "Bell MT";
                ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws.Cells[y, 5, y, 12].Merge = true;
                ws.Cells["E" + y].Value = "Total".ToUpper();
                ws.Cells["E" + y].Style.Font.Name = "Bell MT";
                ws.Cells["E" + y].Style.Font.Bold = true;
                ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 5, y, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                X = 13;
                foreach (var act in acts)
                {
                    string colCode = GetColumnCode(X);

                    ws.Cells[colCode + y].Value = invoice.Activities.Where(x => x.ActId == act.ActId).Sum(x => x.ActivityQty);
                    ws.Cells[colCode + y].Style.Font.Name = "Arial";

                    X++;
                }

                if (acts.Count() < 17)
                {
                    for (; X < 13 + 17; X++)
                    {
                        string colCode = GetColumnCode(X);

                        ws.Cells[colCode + y].Value = "";
                        ws.Cells[colCode + y].Style.Font.Name = "Bell MT";
                    }
                }
                y++;

                #endregion

                y++;

                #region Footer

                ws.Cells[y, 1, y, 10].Merge = true;
                ws.Cells["A" + y].Value = "Certified that the work has been duly perform and that the provision of the relevant specifications and measurement conditions have been met";
                ws.Cells["A" + y].Style.Font.Name = "Bell MT";
                ws.Cells["A" + y].Style.Font.Size = 8;
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Work Supevisor:";
                ws.Cells["A" + y].Style.Font.Name = "Bell MT";
                ws.Cells["A" + y].Style.Font.Size = 11;

                ws.Cells[y, 3, y, 4].Merge = true;
                ws.Cells[y, 3, y, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 3, y, 4].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(168, 226, 247));
                y++;
                #endregion

                #endregion

                #region Summary Price Sheet

                var ws2 = pck.Workbook.Worksheets.Add("Summary Price Sheet");

                #region View and Print Seetings

                //ws2.View.PageLayoutView = true;
                ws2.PrinterSettings.ShowGridLines = false;

                //ws.PrinterSettings.PaperSize = ePaperSize.A4;
                ws2.PrinterSettings.TopMargin = 0.75M;
                ws2.PrinterSettings.RightMargin = 0.25M;
                ws2.PrinterSettings.BottomMargin = 0.75M;
                ws2.PrinterSettings.LeftMargin = 0.25M;
                ws2.PrinterSettings.HeaderMargin = 0.0M;
                ws2.PrinterSettings.FooterMargin = 0.0M;



                //ws2.Protection.IsProtected = true;
                //ws2.Protection.SetPassword("Password123.");

                ws2.Cells[1, 1, 14, 5].Style.Font.Size = 10;

                ws2.Column(1).Width = 9;
                ws2.Column(2).Width = 10;
                ws2.Column(3).Width = 11;
                ws2.Column(4).Width = 11;
                ws2.Column(5).Width = 11;
                ws2.Column(6).Width = 25;
                ws2.Column(7).Width = 12;
                ws2.Column(8).Width = 12;
                ws2.Column(9).Width = 2;
                ws2.Column(10).Width = 15;
                ws2.Column(11).Width = 3;
                ws2.Column(12).Width = 21;

                #endregion
                ws2.Cells[1, 1, 9, 12].Style.Font.Bold = true;
                ws2.Cells[1, 1, 2600, 12].Style.Font.Name = "Arial";
                ws2.Cells[9, 1, 2600, 12].Style.Font.Size = 10;
                y = 1;

                #region Logo

                var image2 = ws2.Drawings.AddPicture("flow-logo.jpg", fi);
                image2.SetPosition(5, 10);
                image2.SetSize(129, 70);
                #endregion

                y++;

                #region Title

                ws2.Cells[y, 1, y, 12].Merge = true;
                ws2.Cells["A" + y].Value = "Cabling Work Diary Summary";
                ws2.Cells["A" + y].Style.Font.Name = "Arial";
                ws2.Cells["A" + y].Style.Font.Size = 11;
                ws2.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                y++;

                #endregion

                y++;
                y++;
                ws2.Cells[5, 1, 8, 12].Style.Font.Size = 9;
                #region Header Row 01

                ws2.Cells[y, 1, y, 2].Merge = true;
                ws2.Cells["A" + y].Value = "Contractor".ToUpper();
                ws2.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells[y, 3, y, 6].Merge = true;
                ws2.Cells["C" + y].Formula = "='Cabling Work Diary'!K5";
                ws2.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws2.Cells[y, 3, y, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells[y, 7, y, 8].Merge = true;
                ws2.Cells["G" + y].Value = "Invoice No:".ToUpper();
                ws2.Cells[y, 7, y, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells[y, 9, y, 12].Merge = true;
                ws2.Cells["I" + y].Formula = "='Cabling Work Diary'!C5";
                ws2.Cells["I" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws2.Cells[y, 9, y, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                y++;

                #endregion

                #region Header Row 02

                ws2.Cells[y, 1, y, 2].Merge = true;
                ws2.Cells["A" + y].Value = "VENDOR NO:".ToUpper();
                ws2.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells["C" + y].Formula = "='Cabling Work Diary'!K6";
                ws2.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                ws2.Cells["D" + y].Value = "Work Period:";
                ws2.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws2.Cells["E" + y].Formula = "='Cabling Work Diary'!Q6";
                ws2.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws2.Cells[y, 3, y, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells[y, 7, y, 8].Merge = true;
                ws2.Cells["G" + y].Value = "PO No:".ToUpper();
                ws2.Cells[y, 7, y, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells[y, 9, y, 12].Merge = true;
                ws2.Cells["I" + y].Formula = "='Cabling Work Diary'!C6";
                ws2.Cells["I" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws2.Cells[y, 9, y, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                y++;

                #endregion

                #region Header Row 03

                ws2.Cells[y, 1, y, 2].Merge = true;
                ws2.Cells["A" + y].Value = "SUMBITTED BY:".ToUpper();
                ws2.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells["C" + y].Formula = "='Cabling Work Diary'!K7";
                ws2.Cells[y, 3, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells["E" + y].Value = "DATE SUB:";
                ws2.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells["F" + y].Formula = "='Cabling Work Diary'!S7";
                ws2.Cells["F" + y].Style.Numberformat.Format = "dd/MMM/yyyy";
                ws2.Cells["F" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws2.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells[y, 7, y, 8].Merge = true;
                ws2.Cells["G" + y].Value = "Region:".ToUpper();
                ws2.Cells[y, 7, y, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells[y, 9, y, 12].Merge = true;
                ws2.Cells["I" + y].Formula = "='Cabling Work Diary'!C7";
                ws2.Cells[y, 9, y, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                y++;

                #endregion

                #region Header Row 04

                ws2.Cells[y, 1, y, 2].Merge = true;
                ws2.Cells["A" + y].Value = "APPROVED BY:".ToUpper();
                ws2.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells["C" + y].Formula = "='Cabling Work Diary'!Z6";
                ws2.Cells[y, 3, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells["E" + y].Value = "DATE APV:";
                ws2.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws2.Cells["F" + y].Formula = "='Cabling Work Diary'!AB7";
                ws2.Cells["F" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws2.Cells["F" + y].Style.Numberformat.Format = "dd/MMM/yyyy";
                ws2.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                //ws2.Cells[y, 7, y, 8].Merge = true;
                //ws2.Cells["G" + y].Value = "Region:".ToUpper();
                //ws2.Cells["G" + y].Style.Font.Name = "Inherit";
                //ws2.Cells[y, 7, y, 8].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);

                //ws2.Cells[y, 9, y, 12].Merge = true;
                //ws2.Cells["I" + y].Formula = "='Cabling Work Diary'!C7";
                //ws2.Cells["I" + y].Style.Font.Name = "Inherit";
                //ws2.Cells[y, 9, y, 12].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);
                y++;

                #endregion

                #region Table Headers

                ws2.Cells["A" + y].Value = "Item No.".ToUpper();
                ws2.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws2.Cells["A" + y].Style.Font.Color.SetColor(Color.Green);

                ws2.Cells[y, 2, y, 6].Merge = true;
                ws2.Cells["B" + y].Value = "DESCRIPTION".ToUpper();
                ws2.Cells[y, 2, y, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws2.Cells["B" + y].Style.Font.Color.SetColor(Color.Green);

                ws2.Cells["G" + y].Value = "Unit".ToUpper();
                ws2.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws2.Cells["G" + y].Style.Font.Color.SetColor(Color.Green);

                ws2.Cells["H" + y].Value = "Unit Rate".ToUpper();
                ws2.Cells["H" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws2.Cells["H" + y].Style.Font.Color.SetColor(Color.Green);

                ws2.Cells["J" + y].Value = "Quantity".ToUpper();
                ws2.Cells["J" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws2.Cells["J" + y].Style.Font.Color.SetColor(Color.Green);

                ws2.Cells["L" + y].Value = "Total".ToUpper();
                ws2.Cells["L" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                ws2.Cells["L" + y].Style.Font.Color.SetColor(Color.Green);
                y++;

                #endregion

                #region Rows

                var activities = db.ADM01100.Where(x => x.ClientID == 3 && x.Active).OrderBy(x => x.AltCode);

                if (activities != null && activities.Count() > 0)
                {
                    foreach (var ALTcODE in acts)
                    {
                        var dtl = invoice.Activities.FindAll(x => x.ActivityDesc == ALTcODE.ActivityDesc);
                        var activity = activities.FirstOrDefault(x => x.RateID == ALTcODE.ActId);
                        var rate = dtl[0].ActivityCost;
                        ws2.Cells["A" + y].Value = string.IsNullOrEmpty(activity.AltCode) ? "" : activity.AltCode.ToUpper();
                        ws2.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws2.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                        ws2.Cells[y, 2, y, 6].Merge = true;
                        ws2.Cells["B" + y].Value = activity.RateDescr.ToUpper();
                        ws2.Cells[y, 2, y, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                        ws2.Cells["G" + y].Value = activity.RateUnit.ToUpper();
                        ws2.Cells["G" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws2.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                        ws2.Cells["H" + y].Value = string.Format("{0:C}", rate);
                        ws2.Cells["H" + y].Style.Numberformat.Format = "$#,##0.00";
                        ws2.Cells["H" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        ws2.Cells["H" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                        ws2.Cells["J" + y].Formula = dtl != null ? dtl.Sum(x => x.ActivityQty).ToString() : "";
                        ws2.Cells["J" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        ws2.Cells["J" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                        ws2.Cells["L" + y].Formula = "=IF(J" + y + "<>\"\",H" + y + "*J" + y + ",\"\")";
                        ws2.Cells["L" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        ws2.Cells["L" + y].Style.Numberformat.Format = "$#,##0.00";
                        ws2.Cells["L" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                        y++;
                    }
                }

                #endregion

                y++;

                #region Total

                ws2.Cells["J" + y].Value = "Grand Total".ToUpper();
                ws2.Cells["J" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                var grandTotalCell = ws2.Cells["L" + y];
                ws2.Cells["L" + y].Formula = "=SUM(L10:L" + (y - 1) + ")";
                ws2.Cells["L" + y].Style.Numberformat.Format = "$#,##0.00";
                ws2.Cells["L" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws2.Cells["L" + y].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                y++;

                #endregion

                #endregion

                #region Invoice Sheet

                var ws3 = pck.Workbook.Worksheets.Add("Invoice Sheet");

                #region View and Print Seetings

                //ws.View.PageLayoutView = true;
                ws3.PrinterSettings.ShowGridLines = false;

                //ws.PrinterSettings.PaperSize = ePaperSize.A4;
                ws3.PrinterSettings.TopMargin = 0.75M;
                ws3.PrinterSettings.RightMargin = 0.25M;
                ws3.PrinterSettings.BottomMargin = 0.75M;
                ws3.PrinterSettings.LeftMargin = 0.25M;
                ws3.PrinterSettings.HeaderMargin = 0.0M;
                ws3.PrinterSettings.FooterMargin = 0.0M;

                #endregion

                //ws.Protection.IsProtected = true;
                //ws.Protection.SetPassword("Password123.");

                #region Column width

                ws3.Column(1).Width = 8;
                ws3.Column(2).Width = 15;
                ws3.Column(3).Width = 15;
                ws3.Column(4).Width = 12;
                ws3.Column(5).Width = 8;
                ws3.Column(6).Width = 8;
                ws3.Column(7).Width = 8;
                ws3.Column(8).Width = 8;
                ws3.Column(9).Width = 8;
                ws3.Column(10).Width = 8;
                ws3.Column(11).Width = 8;
                ws3.Column(12).Width = 8;
                ws3.Column(13).Width = 8;
                ws3.Column(14).Width = 8;
                ws3.Column(15).Width = 20;
                ws3.Column(16).Width = 8;
                ws3.Column(17).Width = 8;
                ws3.Column(18).Width = 8;
                ws3.Column(19).Width = 8;
                ws3.Column(20).Width = 8;
                ws3.Column(21).Width = 8;

                #endregion

                ws3.Cells[1, 1, 200, 21].Style.Font.Name = "Calibri";
                y = 1;

                #region Logo

                var image3 = ws3.Drawings.AddPicture("flow-logo.jpg", fi);
                image3.SetPosition(10, 70);
                image3.SetSize(129, 70);
                #endregion

                #region Title

                ws3.Cells[y, 2, y, 20].Merge = true;
                ws3.Cells["B" + y].Value = "External Plant Cabling and Jointing";
                ws3.Cells["B" + y].Style.Font.Size = 36;
                ws3.Cells["B" + y].Style.Font.Name = "Arial";
                ws3.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                y++;

                ws3.Cells[y, 2, y, 20].Merge = true;
                ws3.Cells["B" + y].Value = "Cabling Grand Total";
                ws3.Cells["B" + y].Style.Font.Size = 28;
                ws3.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws3.Cells[y - 1, 2, y, 20].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);
                y++;

                #endregion

                y++;

                ws3.Cells[y, 2, y + 4, 20].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);
                ws3.Cells[y, 2, y + 4, 20].Style.Font.Bold = true;

                #region Header row 01

                ws3.Cells[y, 2, y, 3].Merge = true;
                ws3.Cells["B" + y].Value = "Company or Contractor's Name:";
                ws3.Cells["B" + y].Style.Font.Size = 11;
                ws3.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws3.Cells[y, 4, y, 11].Merge = true;
                ws3.Cells["D" + y].Formula = "='Cabling Work Diary'!K5";
                ws3.Cells["D" + y].Style.Font.Size = 14;
                ws3.Cells[y, 4, y, 11].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);

                ws3.Cells[y, 12, y, 14].Merge = true;
                ws3.Cells["L" + y].Value = "Report Submitted on:";
                ws3.Cells["L" + y].Style.Font.Size = 11;
                ws3.Cells["L" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws3.Cells[y, 15, y, 19].Merge = true;
                ws3.Cells["O" + y].Formula = "='Cabling Work Diary'!S7";
                ws3.Cells["O" + y].Style.Numberformat.Format = "dd/MMM/yyyy";
                ws3.Cells["O" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws3.Cells["O" + y].Style.Font.Size = 14;
                ws3.Cells[y, 15, y, 19].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);
                y++;

                #endregion

                #region Header row 02

                ws3.Cells[y, 2, y, 3].Merge = true;
                ws3.Cells["B" + y].Value = "Address:";
                ws3.Cells["B" + y].Style.Font.Size = 11;
                ws3.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws3.Cells[y, 4, y, 11].Merge = true;
                ws3.Cells["D" + y].Formula = "='Cabling Work Diary'!Y5";
                ws3.Cells["D" + y].Style.Font.Size = 14;
                ws3.Cells[y, 4, y, 11].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);

                ws3.Cells[y, 12, y, 14].Merge = true;
                ws3.Cells["L" + y].Value = "Report Submitted by:";
                ws3.Cells["L" + y].Style.Font.Size = 11;
                ws3.Cells["L" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws3.Cells[y, 15, y, 19].Merge = true;
                ws3.Cells["O" + y].Formula = "='Cabling Work Diary'!K7";
                ws3.Cells["O" + y].Style.Font.Size = 14;
                ws3.Cells[y, 15, y, 19].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);
                y++;
                #endregion

                #region Header row 03

                ws3.Cells[y, 2, y, 3].Merge = true;
                ws3.Cells["B" + y].Value = "Vendor Number:";
                ws3.Cells["B" + y].Style.Font.Size = 11;
                ws3.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws3.Cells[y, 4, y, 5].Merge = true;
                ws3.Cells["D" + y].Formula = "='Cabling Work Diary'!K6";
                ws3.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws3.Cells["D" + y].Style.Font.Size = 14;
                ws3.Cells[y, 4, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);

                ws3.Cells[y, 6, y, 8].Merge = true;
                ws3.Cells["F" + y].Value = "PO Number:";
                ws3.Cells["F" + y].Style.Font.Size = 11;
                ws3.Cells["F" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws3.Cells[y, 9, y, 11].Merge = true;
                ws3.Cells["I" + y].Formula = "='Cabling Work Diary'!C6";
                ws3.Cells["I" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws3.Cells["I" + y].Style.Font.Size = 14;
                ws3.Cells[y, 9, y, 11].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);

                ws3.Cells[y, 12, y, 14].Merge = true;
                ws3.Cells["L" + y].Value = "Approved by:";
                ws3.Cells["L" + y].Style.Font.Size = 11;
                ws3.Cells["L" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws3.Cells[y, 15, y, 19].Merge = true;
                ws3.Cells["O" + y].Formula = "='Cabling Work Diary'!Z6";
                ws3.Cells["O" + y].Style.Font.Size = 14;
                ws3.Cells[y, 15, y, 19].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);
                y++;
                #endregion

                #region Header row 04

                ws3.Cells[y, 2, y, 3].Merge = true;
                ws3.Cells["B" + y].Value = "Invoice No:";
                ws3.Cells["B" + y].Style.Font.Size = 11;
                ws3.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws3.Cells[y, 4, y, 5].Merge = true;
                ws3.Cells["D" + y].Formula = "='Cabling Work Diary'!C5";
                ws3.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws3.Cells["D" + y].Style.Font.Size = 14;
                ws3.Cells[y, 4, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);

                ws3.Cells[y, 6, y, 8].Merge = true;
                ws3.Cells["F" + y].Value = "Region:";
                ws3.Cells["F" + y].Style.Font.Size = 11;
                ws3.Cells["F" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws3.Cells[y, 9, y, 12].Merge = true;
                ws3.Cells["I" + y].Formula = "='Cabling Work Diary'!C7";
                ws3.Cells["I" + y].Style.Font.Size = 14;
                ws3.Cells[y, 9, y, 12].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);

                ws3.Cells[y, 13, y, 14].Merge = true;
                ws3.Cells["M" + y].Value = "Approved on:";
                ws3.Cells["M" + y].Style.Font.Size = 11;
                ws3.Cells["M" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws3.Cells[y, 15, y, 19].Merge = true;
                ws3.Cells["O" + y].Formula = "='Cabling Work Diary'!AB7";
                ws3.Cells["O" + y].Style.Numberformat.Format = "dd/MMM/yyyy";
                ws3.Cells["O" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws3.Cells["O" + y].Style.Font.Size = 14;
                ws3.Cells[y, 15, y, 19].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);
                y++;
                #endregion

                #region Header row 05

                ws3.Cells[y, 2, y, 3].Merge = true;
                ws3.Cells["B" + y].Value = "RETENTION:";
                ws3.Cells["B" + y].Style.Font.Size = 11;
                ws3.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws3.Cells[y, 4, y, 6].Merge = true;
                ws3.Cells["D" + y].Value = "NO";
                ws3.Cells["D" + y].Style.Font.Size = 14;

                ws3.Cells[y, 12, y, 14].Merge = true;
                ws3.Cells["L" + y].Value = "Work Period:";
                ws3.Cells["L" + y].Style.Font.Size = 11;
                ws3.Cells["L" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws3.Cells[y, 15, y, 19].Merge = true;
                ws3.Cells["O" + y].Formula = "='Cabling Work Diary'!Q6";
                ws3.Cells["O" + y].Style.Font.Size = 14;
                y++;

                #endregion

                #region Header extra row

                ws3.Cells[y, 2, y, 3].Merge = true;
                ws3.Cells["B" + y].Value = "GOVERNMENT LEVY:";
                ws3.Cells["B" + y].Style.Font.Size = 11;
                ws3.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws3.Cells[y, 4, y, 6].Merge = true;
                ws3.Cells["D" + y].Value = "YES";
                ws3.Cells["D" + y].Style.Font.Size = 14;

                y++;
                #endregion

                ws3.Cells[10, 1, 18, 21].Style.Font.Name = "Arial";

                #region Sub total

                ws3.Cells[y, 2, y, 8].Merge = true;
                ws3.Cells["B" + y].Value = "";
                ws3.Cells["B" + y].Style.Font.Size = 14;
                ws3.Cells[y, 2, y, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws3.Cells[y, 9, y, 11].Merge = true;
                ws3.Cells["I" + y].Value = "Sub-Total";
                ws3.Cells["I" + y].Style.Font.Size = 24;
                ws3.Cells["I" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws3.Cells[y, 9, y, 11].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                y++;

                ws3.Cells[y, 2, y, 8].Merge = true;
                ws3.Cells["B" + y].Value = "Cabling Activities Reported";
                ws3.Cells["B" + y].Style.Font.Size = 16;
                ws3.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws3.Cells[y, 2, y, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws3.Cells[y, 9, y, 11].Merge = true;
                ws3.Cells["I" + y].Formula = "=" + grandTotalCell.FullAddress + "";
                ws3.Cells["I" + y].Style.Numberformat.Format = "$#,##0.00";
                ws3.Cells["I" + y].Style.Font.Size = 16;
                ws3.Cells["I" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws3.Cells[y, 9, y, 11].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                y++;

                #endregion

                y++;

                ws3.Cells[y, 5, y + 4, 5].Style.Font.Bold = true;

                #region Grand total

                ws3.Cells[y, 5, y, 8].Merge = true;
                ws3.Cells["E" + y].Value = "Grand Total";
                ws3.Cells["E" + y].Style.Font.Size = 18;
                ws3.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws3.Cells[y, 5, y, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws3.Cells[y, 9, y, 11].Merge = true;
                ws3.Cells["I" + y].Formula = "=I11";
                ws3.Cells["I" + y].Style.Numberformat.Format = "$#,##0.00";
                ws3.Cells["I" + y].Style.Font.Size = 16;
                ws3.Cells["I" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws3.Cells[y, 9, y, 11].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                y++;

                #endregion

                #region RETENTION

                ws3.Cells[y, 5, y, 8].Merge = true;
                ws3.Cells["E" + y].Value = "10% RETENTION";
                ws3.Cells["E" + y].Style.Font.Size = 18;
                ws3.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws3.Cells[y, 5, y, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws3.Cells[y, 9, y, 11].Merge = true;
                ws3.Cells["I" + y].Value = "";
                ws3.Cells["I" + y].Style.Numberformat.Format = "$#,##0.00";
                ws3.Cells["I" + y].Style.Font.Size = 16;
                ws3.Cells["I" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws3.Cells[y, 9, y, 11].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                y++;

                #endregion

                #region Sub total

                ws3.Cells[y, 5, y, 8].Merge = true;
                ws3.Cells["E" + y].Value = "Sub Total";
                ws3.Cells["E" + y].Style.Font.Size = 18;
                ws3.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws3.Cells[y, 5, y, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws3.Cells[y, 9, y, 11].Merge = true;
                ws3.Cells["I" + y].Formula = "=I11";
                ws3.Cells["I" + y].Style.Numberformat.Format = "$#,##0.00";
                ws3.Cells["I" + y].Style.Font.Size = 16;
                ws3.Cells["I" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws3.Cells[y, 9, y, 11].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                y++;

                #endregion

                #region GOVERNMET LEVY

                ws3.Cells[y, 5, y, 8].Merge = true;
                ws3.Cells["E" + y].Value = "2% GOVERNMET LEVY";
                ws3.Cells["E" + y].Style.Font.Size = 14;
                ws3.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws3.Cells[y, 5, y, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws3.Cells[y, 9, y, 11].Merge = true;
                ws3.Cells["I" + y].Formula = "=IF(I13<>\"\",0.02*I13,\"\")";
                ws3.Cells["I" + y].Style.Numberformat.Format = "$#,##0.00";
                ws3.Cells["I" + y].Style.Font.Size = 16;
                ws3.Cells["I" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws3.Cells[y, 9, y, 11].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                y++;

                #endregion

                #region Net Pay

                ws3.Cells[y, 5, y, 8].Merge = true;
                ws3.Cells["E" + y].Value = "NET PAY";
                ws3.Cells["E" + y].Style.Font.Size = 18;
                ws3.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws3.Cells[y, 5, y, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);

                ws3.Cells[y, 9, y, 11].Merge = true;
                ws3.Cells["I" + y].Formula = "=I13+IF(I14<>\"\",I14,0)+IF(I16<>\"\",I16,0)";
                ws3.Cells["I" + y].Style.Numberformat.Format = "$#,##0.00";
                ws3.Cells["I" + y].Style.Font.Size = 16;
                ws3.Cells["I" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws3.Cells[y, 9, y, 11].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                y++;

                #endregion

                #endregion

                #region Telicon INV

                var ws4 = pck.Workbook.Worksheets.Add("Telicon INV");


                #region View and Print Seetings

                //ws4.View.PageLayoutView = true;
                ws4.PrinterSettings.ShowGridLines = false;

                ws4.PrinterSettings.PaperSize = ePaperSize.A4;
                ws4.PrinterSettings.TopMargin = 0.75M;
                ws4.PrinterSettings.RightMargin = 0.25M;
                ws4.PrinterSettings.BottomMargin = 0.75M;
                ws4.PrinterSettings.LeftMargin = 0.25M;
                ws4.PrinterSettings.HeaderMargin = 0.0M;
                ws4.PrinterSettings.FooterMargin = 0.0M;

                #endregion

                ws4.Cells[1, 1, 14, 5].Style.Font.Size = 10;

                ws4.Column(1).Width = 11.71;
                ws4.Column(2).Width = 8.43;
                ws4.Column(3).Width = 48.14;
                ws4.Column(4).Width = 12.29;
                ws4.Column(5).Width = 15.57;

                y = 1;

                #region Header
                logoSearchPath = "/images/print-logo.png";
                if (Directory.Exists(Path.Combine(_env.WebRootPath, "work")))
                    logoSearchPath = "/work/images/print-logo.png";
                else
                    logoSearchPath = Path.Combine(_env.WebRootPath, "Images", "print-logo.png");
                FileInfo fi1 = new FileInfo(logoSearchPath);
                var image1 = ws4.Drawings.AddPicture("print-logo.png", fi1);
                image1.SetPosition(0, 0);
                image1.SetSize(205, 111);



                ws4.Cells["C" + y].Value = "P.O. BOX 3069";
                ws4.Cells["C" + y].Style.Font.Bold = true;
                ws4.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                if (invoice.Client.InvoiceNo > 0)
                {
                    ws4.Cells["D" + y].Value = "Invoice #:";
                    ws4.Cells["E" + y].Value = Customs.MakeGenericInvoiceNo(invoice.Client.InvoiceNo);
                    ws4.Cells["D" + y].Style.Font.Bold = true;
                    ws4.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                }
                ws4.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                ws4.Cells["C" + y].Value = "Kingston 8";
                ws4.Cells["C" + y].Style.Font.Bold = true;
                ws4.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                if (invoice.Client.InvoiceNo > 0)
                {
                    ws4.Cells["D" + y].Value = "GCT #:";
                    ws4.Cells["E" + y].Value = invoice.Client.GCTNo;
                    ws4.Cells["D" + y].Style.Font.Bold = true;
                }
                ws4.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                ws4.Cells["C" + y].Value = "Tel: (876) 630-5278";
                ws4.Cells["C" + y].Style.Font.Bold = true;
                ws4.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws4.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";

                ws4.Cells["D" + y].Value = "GR #";
                ws4.Cells["E" + y].Value = "";//5000433798
                ws4.Cells["D" + y].Style.Font.Bold = true;
                ws4.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                ws4.Cells["C" + y].Value = "Email: admin@telicongroup.com";
                ws4.Cells["C" + y].Style.Font.Bold = true;
                ws4.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws4.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                y++;

                y++;
                y++;
                y++;

                ws4.Cells["A" + y].Value = "Co. Name:";
                ws4.Cells["A" + y].Style.Font.Bold = true;
                ws4.Cells[y, 2, y, 3].Merge = true;
                ws4.Cells["B" + y].Value = invoice.Client.ClientName;

                ws4.Cells["D" + y].Value = "Date:";
                ws4.Cells["D" + y].Style.Font.Bold = true;
                ws4.Cells["E" + y].Value = string.Format("{0:dd/MM/yyyy}", invoice.Client.Date);
                ws4.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws4.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws4.Cells["A" + y].Value = "Attn.:";
                ws4.Cells["A" + y].Style.Font.Bold = true;
                ws4.Cells[y, 2, y, 3].Merge = true;
                ws4.Cells["B" + y].Value = invoice.Client.Attention;

                ws4.Cells["D" + y].Value = "Prepared By:";
                ws4.Cells["D" + y].Style.Font.Bold = true;
                ws4.Cells["E" + y].Value = invoice.Client.User;
                ws4.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws4.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws4.Cells["A" + y].Value = "Address:";
                ws4.Cells["A" + y].Style.Font.Bold = true;
                ws4.Cells[y, 2, y, 3].Merge = true;
                ws4.Cells["B" + y].Value = invoice.Client.Address;

                ws4.Cells["D" + y].Value = "Currency:";
                ws4.Cells["D" + y].Style.Font.Bold = true;
                ws4.Cells["E" + y].Value = invoice.Client.Currency;
                ws4.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws4.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws4.Cells["A" + y].Value = "Phone:";
                ws4.Cells["A" + y].Style.Font.Bold = true;
                ws4.Cells[y, 2, y, 3].Merge = true;
                ws4.Cells["B" + y].Value = invoice.Client.Phone;
                if (invoice.Client.InvoiceNo > 0)
                {
                    //ws4.Cells["D" + y].Value = "Ticket #:";
                    //ws4.Cells["D" + y].Style.Font.Bold = true;
                    //ws4.Cells["E" + y].Value = invoice.Client.ReferenceNo;
                }
                ws4.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws4.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                ws4.Cells["A" + y].Value = "Email:";
                ws4.Cells["A" + y].Style.Font.Bold = true;
                ws4.Cells[y, 2, y, 3].Merge = true;
                ws4.Cells["B" + y].Value = invoice.Client.Email;

                //ws4.Cells["D" + y].Value = "PO #:";
                //ws4.Cells["D" + y].Style.Font.Bold = true;
                //ws4.Cells["E" + y].Value = invoice.Client.PONo;
                ws4.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                ws4.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                y++;

                #endregion
                y++;
                y++;

                #region Location
                ws4.Cells[y, 1, y, 5].Merge = true;
                //ws4.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", invoice.Date);
                ws4.Cells["A" + y].Value = invoice.Title.ToUpper();
                ws4.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws4.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws4.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                ws4.Cells[y, 1, y, 5].Style.Font.Size = 12;
                //ws4.Cells[y, 1, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                //ws4.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                //ws4.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                #endregion
                y++;
                y++;

                #region Table Headers
                ws4.Cells["A" + y].Value = "Date";
                ws4.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws4.Cells["B" + y].Value = "Qty";
                ws4.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws4.Cells["C" + y].Value = "Description";
                ws4.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws4.Cells["D" + y].Value = "Rate";
                ws4.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws4.Cells["E" + y].Value = "Amount";
                ws4.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws4.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws4.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                ws4.Cells[y, 1, y, 5].Style.Font.Size = 12;
                ws4.Cells[y, 1, y, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws4.Cells[y, 1, y, 5].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws4.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws4.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                #endregion
                y++;


                #region Location
                ws4.Cells[y, 2, y, 5].Merge = true;
                ws4.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", invoice.Date);
                //ws4.Cells["B" + y].Value = invoice.Title.ToUpper();
                ws4.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws4.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws4.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                ws4.Cells[y, 1, y, 5].Style.Font.Size = 12;
                ws4.Cells[y, 1, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws4.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws4.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                #endregion
                y++;

                foreach (var desc in invoice.Activities.Select(x => x.ActivityDesc).Distinct())
                {
                    var act = invoice.Activities.Where(x => x.ActivityDesc == desc).ToList();

                    #region Rows
                    ws4.Cells["A" + y].Value = "";
                    ws4.Cells["B" + y].Value = act.Sum(x => x.ActivityQty);
                    ws4.Cells["C" + y].Value = desc;
                    ws4.Cells["D" + y].Value = string.Format("{0:C}", act[0].ActivityCost);
                    ws4.Cells["E" + y].Value = string.Format("{0:C}", act[0].ActivityCost * Convert.ToDouble(act.Sum(x => x.ActivityQty)));

                    #region Row Styles
                    ws4.Cells["D" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));
                    ws4.Cells["E" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                    ws4.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws4.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws4.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws4.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws4.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws4.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws4.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws4.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws4.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws4.Cells[y, 1, y, 5].Style.Font.Size = 11;
                    if (y % 2 != 0)
                    {
                        ws4.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws4.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    #endregion
                    y++;
                }

                var total = invoice.Activities.Sum(x => x.ActivityCost * Convert.ToDouble(x.ActivityQty));
                var gct = total * Convert.ToDouble(invoice.GCT);
                var grandTotal = total + gct;

                #region Total
                ws4.Cells[y, 1, y, 4].Merge = true;
                ws4.Cells["A" + y].Value = "Total:";
                ws4.Cells[y, 1, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws4.Cells["E" + y].Value = string.Format("{0:C}", total);
                ws4.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws4.Cells[y, 1, y, 5].Style.Font.Size = 11;
                ws4.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws4.Cells[y, 1, y, 5].Style.Font.Color.SetColor(Color.FromArgb(165, 0, 33));
                ws4.Cells[y, 1, y, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                if (y % 2 != 0)
                {
                    ws4.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws4.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                y++;
                #endregion

                #region GCT
                ws4.Cells[y, 1, y, 4].Merge = true;
                ws4.Cells["A" + y].Value = "GCT(" + (invoice.GCT * 100).ToString("#.##") + "):";
                ws4.Cells[y, 1, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws4.Cells["E" + y].Value = string.Format("{0:C}", gct);
                ws4.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws4.Cells[y, 1, y, 5].Style.Font.Size = 11;
                ws4.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws4.Cells[y, 1, y, 5].Style.Font.Color.SetColor(Color.FromArgb(165, 0, 33));
                ws4.Cells[y, 1, y, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                if (y % 2 != 0)
                {
                    ws4.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws4.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                y++;
                #endregion

                #region Grand Total
                ws4.Cells[y, 1, y, 4].Merge = true;
                ws4.Cells["A" + y].Value = "Grand Total:";
                ws4.Cells[y, 1, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws4.Cells["E" + y].Value = string.Format("{0:C}", grandTotal);
                ws4.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws4.Cells[y, 1, y, 5].Style.Font.Size = 11;
                ws4.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws4.Cells[y, 1, y, 5].Style.Font.Color.SetColor(Color.FromArgb(165, 0, 33));
                ws4.Cells[y, 1, y, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                if (y % 2 != 0)
                {
                    ws4.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws4.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                y++;
                #endregion

                #region Material Usage
                ws4.Cells[y, 1, y, 5].Merge = true;
                y++;
                ws4.Cells[y, 1, y, 5].Merge = true;
                y++;

                #region Title
                ws4.Cells[y, 1, y, 5].Merge = true;
                ws4.Cells["A" + y].Value = "Material Usage:";
                ws4.Cells["A" + y].Style.Font.Bold = true;
                ws4.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws4.Cells["A" + y].Style.Font.Name = "Arial";
                ws4.Cells["A" + y].Style.Font.Size = 12;
                #endregion
                y++;

                #region Table Headers
                ws4.Cells["A" + y].Value = "Nos.";
                ws4.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws4.Cells[y, 2, y, 4].Merge = true;
                ws4.Cells["B" + y].Value = "Description";
                ws4.Cells[y, 2, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws4.Cells["E" + y].Value = "Qty";
                ws4.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws4.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws4.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                ws4.Cells[y, 1, y, 5].Style.Font.Size = 12;
                ws4.Cells[y, 1, y, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws4.Cells[y, 1, y, 5].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws4.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws4.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                #endregion
                y++;

                int matCount = 1;
                foreach (var dmat in invoice.Materials.Select(x => x.MatDesc).Distinct())
                {
                    var matDtl = invoice.Materials.FindAll(x => x.MatDesc == dmat);
                    #region Rows
                    ws4.Cells["A" + y].Value = matCount;
                    ws4.Cells[y, 2, y, 4].Merge = true;
                    ws4.Cells["B" + y].Value = dmat;
                    ws4.Cells["E" + y].Value = matDtl.Sum(x => x.MatQty);

                    #region Row Styles

                    ws4.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws4.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws4.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws4.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws4.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws4.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws4.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws4.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws4.Cells[y, 1, y, 5].Style.Font.Size = 11;
                    if (y % 2 != 0)
                    {
                        ws4.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws4.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    #endregion
                    y++;
                }

                #endregion

                #endregion

                #region Individual INV

                foreach (var refNum in invoice.Activities.Select(x => x.RefNum).Distinct())
                {
                    var wo = invoice.Activities.Find(x => x.RefNum == refNum);
                    var ws5 = pck.Workbook.Worksheets.Add(wo.WoId.ToString());

                    #region View and Print Seetings

                    ws5.PrinterSettings.ShowGridLines = false;
                    ws5.PrinterSettings.PaperSize = ePaperSize.A4;
                    ws5.PrinterSettings.TopMargin = 0.75M;
                    ws5.PrinterSettings.RightMargin = 0.25M;
                    ws5.PrinterSettings.BottomMargin = 0.75M;
                    ws5.PrinterSettings.LeftMargin = 0.25M;
                    ws5.PrinterSettings.HeaderMargin = 0.0M;
                    ws5.PrinterSettings.FooterMargin = 0.0M;

                    #endregion

                    ws5.Cells[1, 1, 14, 5].Style.Font.Size = 10;
                    ws5.Column(1).Width = 11.71;
                    ws5.Column(2).Width = 8.43;
                    ws5.Column(3).Width = 48.14;
                    ws5.Column(4).Width = 12.29;
                    ws5.Column(5).Width = 15.57;

                    y = 1;

                    #region Header

                    logoSearchPath = "/images/print-logo.png";
                    if (Directory.Exists(Path.Combine(_env.WebRootPath, "work")))
                        logoSearchPath = "/work/images/print-logo.png";
                    else
                        logoSearchPath = Path.Combine(_env.WebRootPath, "Images", "print-logo.png");
                    fi1 = new FileInfo(logoSearchPath);
                    image1 = ws5.Drawings.AddPicture("print-logo.png", fi1);
                    image1.SetPosition(0, 0);
                    image1.SetSize(205, 111);

                    y++;

                    y++;

                    y++;

                    y++;
                    y++;
                    y++;

                    ws5.Cells["A" + y].Value = "Co. Name:";
                    ws5.Cells["A" + y].Style.Font.Bold = true;
                    ws5.Cells[y, 2, y, 3].Merge = true;
                    ws5.Cells["B" + y].Value = invoice.Client.ClientName;

                    ws5.Cells["D" + y].Value = "Date:";
                    ws5.Cells["D" + y].Style.Font.Bold = true;
                    ws5.Cells["E" + y].Value = string.Format("{0:dd/MM/yyyy}", wo.WOSubmittedDate);
                    ws5.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                    ws5.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                    y++;

                    ws5.Cells["A" + y].Value = "Address:";
                    ws5.Cells["A" + y].Style.Font.Bold = true;
                    ws5.Cells[y, 2, y, 3].Merge = true;
                    ws5.Cells["B" + y].Value = invoice.Client.Address;

                    ws5.Cells["D" + y].Value = "Currency:";
                    ws5.Cells["D" + y].Style.Font.Bold = true;
                    ws5.Cells["E" + y].Value = invoice.Client.Currency;
                    ws5.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                    ws5.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                    y++;

                    ws5.Cells["A" + y].Value = "Phone:";
                    ws5.Cells["A" + y].Style.Font.Bold = true;
                    ws5.Cells[y, 2, y, 3].Merge = true;
                    ws5.Cells["B" + y].Value = invoice.Client.Phone;
                    if (invoice.Client.InvoiceNo > 0)
                    {
                        ws5.Cells["D" + y].Value = "Ref #:";
                        ws5.Cells["D" + y].Style.Font.Bold = true;
                        ws5.Cells["E" + y].Value = Customs.MakeGenericInvoiceNo(invoice.Client.InvoiceNo) + "-" + wo.WoId;
                    }
                    ws5.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                    ws5.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                    y++;

                    ws5.Cells["A" + y].Value = "Email:";
                    ws5.Cells["A" + y].Style.Font.Bold = true;
                    ws5.Cells[y, 2, y, 3].Merge = true;
                    ws5.Cells["B" + y].Value = invoice.Client.Email;

                    if (invoice.Client.InvoiceNo > 0)
                    {
                        ws5.Cells["D" + y].Value = "Ticket #:";
                        ws5.Cells["D" + y].Style.Font.Bold = true;
                        ws5.Cells["E" + y].Value = wo.RefNum;
                    }
                    ws5.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                    ws5.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                    y++;

                    ws5.Cells["D" + y].Value = "PO #:";
                    ws5.Cells["D" + y].Style.Font.Bold = true;
                    ws5.Cells["E" + y].Value = invoice.Client.PONo;
                    ws5.Cells[y, 3, y, 4].Style.Font.Name = "Inherit";
                    ws5.Cells[y, 1, y, 1].Style.Font.Name = "Inherit";
                    y++;

                    #endregion
                    y++;
                    y++;

                    #region Location
                    ws5.Cells[y, 1, y, 5].Merge = true;
                    ws5.Cells["A" + y].Value = wo.WOTitle.ToUpper();
                    ws5.Cells[y, 1, y, 5].Style.Font.Bold = true;
                    ws5.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws5.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                    ws5.Cells[y, 1, y, 5].Style.Font.Size = 12;
                    #endregion
                    y++;

                    #region Table Headers
                    ws5.Cells["A" + y].Value = "Date";
                    ws5.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws5.Cells["B" + y].Value = "Qty";
                    ws5.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws5.Cells["C" + y].Value = "Description";
                    ws5.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws5.Cells["D" + y].Value = "Rate";
                    ws5.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws5.Cells["E" + y].Value = "Amount";
                    ws5.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws5.Cells[y, 1, y, 5].Style.Font.Bold = true;
                    ws5.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                    ws5.Cells[y, 1, y, 5].Style.Font.Size = 12;
                    ws5.Cells[y, 1, y, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                    ws5.Cells[y, 1, y, 5].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                    ws5.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws5.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    #endregion
                    y++;

                    //string ActivityLocation = string.Empty;
                    foreach (var loc in invoice.Activities.Where(t => t.WoId == wo.WoId).Select(x => new { x.ActivityDate, x.ActivityLocation, x.Comments }).Distinct().OrderBy(p => p.ActivityDate).ThenBy(p => p.ActivityLocation).ThenBy(p => p.Comments))
                    {
                        //var act = invoice.Activities.Where(x => x.ActivityDesc == desc && x.WoId == wo.WoId).ToList();

                        #region Rows

                        #region Location

                        ws5.Cells[y, 1, y, 5].Merge = true;
                        ws5.Cells["A" + y].Value = loc.ActivityLocation;
                        ws5.Cells[y, 1, y, 5].Style.Font.Bold = true;
                        ws5.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws5.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                        ws5.Cells[y, 1, y, 5].Style.Font.Size = 11;
                        y++;

                        if (!string.IsNullOrEmpty(loc.Comments) && loc.Comments != ".")
                        {
                            ws5.Cells[y, 1, y, 5].Merge = true;
                            ws5.Cells["A" + y].Value = "( " + loc.Comments + " )";
                            ws5.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws5.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                            ws5.Cells[y, 1, y, 5].Style.Font.Size = 10;
                            y++;
                        }

                        #endregion

                        foreach (var act in invoice.Activities.Where(x => x.ActivityLocation == loc.ActivityLocation && x.Comments == loc.Comments))
                        {
                            ws5.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", act.ActivityDate);
                            ws5.Cells["B" + y].Value = act.ActivityQty;
                            ws5.Cells["C" + y].Value = act.ActivityDesc;
                            ws5.Cells["D" + y].Value = string.Format("{0:C}", act.ActivityCost);
                            ws5.Cells["E" + y].Value = string.Format("{0:C}", act.ActivityCost * Convert.ToDouble(act.ActivityQty));

                            #region Row Styles
                            ws5.Cells["D" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));
                            ws5.Cells["E" + y].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                            ws5.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            ws5.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws5.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            ws5.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                            ws5.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                            ws5.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                            ws5.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                            ws5.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                            ws5.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                            ws5.Cells[y, 1, y, 5].Style.Font.Size = 11;
                            if (y % 2 != 0)
                            {
                                ws5.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws5.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                            }

                            //ActivityLocation = act[0].ActivityLocation;

                            #endregion

                            y++;
                        }

                        #endregion
                    }

                    total = invoice.Activities.Where(x => x.WoId == wo.WoId).Sum(x => x.ActivityCost * Convert.ToDouble(x.ActivityQty));
                    gct = total * Convert.ToDouble(invoice.GCT);
                    grandTotal = total + gct;

                    #region Total
                    ws5.Cells[y, 1, y, 4].Merge = true;
                    ws5.Cells["A" + y].Value = "Total:";
                    ws5.Cells[y, 1, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws5.Cells["E" + y].Value = string.Format("{0:C}", total);
                    ws5.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws5.Cells[y, 1, y, 5].Style.Font.Size = 11;
                    ws5.Cells[y, 1, y, 5].Style.Font.Bold = true;
                    ws5.Cells[y, 1, y, 5].Style.Font.Color.SetColor(Color.FromArgb(165, 0, 33));
                    ws5.Cells[y, 1, y, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    if (y % 2 != 0)
                    {
                        ws5.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws5.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    y++;
                    #endregion

                    #region GCT
                    ws5.Cells[y, 1, y, 4].Merge = true;
                    ws5.Cells["A" + y].Value = "GCT(" + (invoice.GCT * 100).ToString("#.##") + "):";
                    ws5.Cells[y, 1, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws5.Cells["E" + y].Value = string.Format("{0:C}", gct);
                    ws5.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws5.Cells[y, 1, y, 5].Style.Font.Size = 11;
                    ws5.Cells[y, 1, y, 5].Style.Font.Bold = true;
                    ws5.Cells[y, 1, y, 5].Style.Font.Color.SetColor(Color.FromArgb(165, 0, 33));
                    ws5.Cells[y, 1, y, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    if (y % 2 != 0)
                    {
                        ws5.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws5.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    y++;
                    #endregion

                    #region Grand Total
                    ws5.Cells[y, 1, y, 4].Merge = true;
                    ws5.Cells["A" + y].Value = "Grand Total:";
                    ws5.Cells[y, 1, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws5.Cells["E" + y].Value = string.Format("{0:C}", grandTotal);
                    ws5.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws5.Cells[y, 1, y, 5].Style.Font.Size = 11;
                    ws5.Cells[y, 1, y, 5].Style.Font.Bold = true;
                    ws5.Cells[y, 1, y, 5].Style.Font.Color.SetColor(Color.FromArgb(165, 0, 33));
                    ws5.Cells[y, 1, y, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    if (y % 2 != 0)
                    {
                        ws5.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws5.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    y++;
                    #endregion

                    #region Material Usage
                    ws5.Cells[y, 1, y, 5].Merge = true;
                    y++;
                    ws5.Cells[y, 1, y, 5].Merge = true;
                    y++;

                    ws5.Cells[y, 1, y, 5].Merge = true;
                    ws5.Cells["A" + y].Value = "Team: " + wo.TeamName;
                    //ws5.Cells["A" + y].Style.Font.Bold = true;
                    ws5.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws5.Cells["A" + y].Style.Font.Name = "Arial";
                    ws5.Cells["A" + y].Style.Font.Size = 12;
                    y++;

                    #region Title
                    ws5.Cells[y, 1, y, 5].Merge = true;
                    ws5.Cells["A" + y].Value = "Material Usage:";
                    ws5.Cells["A" + y].Style.Font.Bold = true;
                    ws5.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws5.Cells["A" + y].Style.Font.Name = "Arial";
                    ws5.Cells["A" + y].Style.Font.Size = 12;
                    #endregion
                    y++;

                    #region Table Headers
                    ws5.Cells["A" + y].Value = "Nos.";
                    ws5.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws5.Cells[y, 2, y, 4].Merge = true;
                    ws5.Cells["B" + y].Value = "Description";
                    ws5.Cells[y, 2, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws5.Cells["E" + y].Value = "Qty";
                    ws5.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws5.Cells[y, 1, y, 5].Style.Font.Bold = true;
                    ws5.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                    ws5.Cells[y, 1, y, 5].Style.Font.Size = 12;
                    ws5.Cells[y, 1, y, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                    ws5.Cells[y, 1, y, 5].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                    ws5.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws5.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    #endregion
                    y++;

                    matCount = 1;
                    foreach (var dmat in invoice.Materials.Select(x => x.MatDesc).Distinct())
                    {
                        var matDtl = invoice.Materials.FindAll(x => x.MatDesc == dmat);
                        #region Rows
                        ws5.Cells["A" + y].Value = matCount;
                        ws5.Cells[y, 2, y, 4].Merge = true;
                        ws5.Cells["B" + y].Value = dmat;
                        ws5.Cells["E" + y].Value = matDtl.Sum(x => x.MatQty);

                        #region Row Styles

                        ws5.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws5.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws5.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws5.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws5.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws5.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws5.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws5.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                        ws5.Cells[y, 1, y, 5].Style.Font.Size = 11;
                        if (y % 2 != 0)
                        {
                            ws5.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws5.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        }
                        #endregion

                        #endregion
                        y++;
                    }

                    #endregion
                }

                #endregion
            }
            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=Cabling Work Diary " + invNo + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        private string GetColumnCode(int X)
        {
            string colCode = "L";
            if (X > 26)
                if (X % 26 > 0)
                    colCode = Convert.ToChar(Convert.ToInt32(X / 26) + 64).ToString() + Convert.ToChar((X % 26) + 64).ToString();
                else
                    colCode = Convert.ToChar(Convert.ToInt32(X / 26) + 63).ToString() + Convert.ToChar(90).ToString();
            else
                colCode = Convert.ToChar(X + 64).ToString();
            return colCode;
        }
        #endregion

        #region New Invoice Print BreakDown
        public ActionResult WorkOrderBreakDown(int id, int invNo)
        {
            var breakDown = WorkOrderBreakDownData(id);
            ViewBag.forInvoice = false;
            ViewBag.InvoiceNo = invNo;
            return View(breakDown);
        }
        [HttpPost]
        public void WorkOrderBreakDownToExcel(ClientInvoiceFilter model)
        {
            string docName = "BreakDown";
            ExcelPackage pck = new ExcelPackage();
            int invNo = 0;
            var breakDown = BreakDownData(model.InvoiceNo);
            if (breakDown != null)
            {
                #region Work Sheet
                if (model.InvoiceNo > 0)
                    invNo = Customs.MakeGenericInvoiceNo(model.InvoiceNo);
                var ws = pck.Workbook.Worksheets.Add(invNo.ToString());

                #region View and Print Seetings

                //ws.View.PageLayoutView = true;
                ws.PrinterSettings.ShowGridLines = false;

                ws.PrinterSettings.PaperSize = ePaperSize.A4;
                ws.PrinterSettings.TopMargin = 0.75M;
                ws.PrinterSettings.RightMargin = 0.25M;
                ws.PrinterSettings.BottomMargin = 0.75M;
                ws.PrinterSettings.LeftMargin = 0.25M;
                ws.PrinterSettings.HeaderMargin = 0.0M;
                ws.PrinterSettings.FooterMargin = 0.0M;

                #endregion

                ws.Column(1).Width = 11.71;
                ws.Column(2).Width = 8.43;
                ws.Column(3).Width = 48.14;
                ws.Column(4).Width = 12.29;
                ws.Column(5).Width = 15.57;
                int y = 1;
                #region BreakDown

                ws.Cells[y, 1, y, 5].Merge = true;
                y++;

                #region BreakDown Header

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Requested By:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = breakDown.RequestedBy;
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Request Date:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "Week Ending" + string.Format("{0:MMMM dd, yyyy}", breakDown.DateRequested);
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Issue Date:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = string.Format("{0:MMMM dd, yyyy}", breakDown.DateDispatched);
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "PO #:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = breakDown.RefNo;
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Ticket #:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = breakDown.RefNo;
                y++;

                //ws.Cells[y, 1, y, 2].Merge = true;
                //ws.Cells["A" + y].Value = "Completion Date:";
                //ws.Cells[y, 3, y, 5].Merge = true;
                //ws.Cells["C" + y].Value = string.Format("{0:MMMM dd, yyyy}", DateTime.Now);
                //y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Department:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "";
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Field Survey:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "No";
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Strand:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "No";
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Fiber:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "Yes";
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Drafting:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "No";
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Transportation:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "No";
                y++;

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Location:";
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "";
                y++;

                #endregion

                ws.Cells[y, 1, y, 5].Merge = true;
                y++;

                #region BreakDown Title
                ws.Cells[y, 1, y, 5].Merge = true;
                ws.Cells["A" + y].Value = breakDown.Title.ToUpper();
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;
                #endregion

                ws.Cells[y, 1, y, 5].Merge = true;
                y++;

                #region BreakDown Table Headers
                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Date";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 3, y, 5].Merge = true;
                ws.Cells["C" + y].Value = "Description";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 5].Style.Font.Size = 12;
                ws.Cells[y, 1, y, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, 5].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                #endregion
                y++;

                #region BreakDown rows

                foreach (var l in breakDown.Activities.Select(x => x.Location).Distinct())
                {
                    ws.Cells[y, 1, y, 2].Merge = true;
                    ws.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", breakDown.Activities.FirstOrDefault(x => x.Location == l).Date);
                    ws.Cells[y, 3, y, 5].Merge = true;
                    ws.Cells["C" + y].Value = l;

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells["C" + y].Style.Font.Bold = true;

                    ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells[y, 3, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                    if (y % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion
                    y++;

                    foreach (var act in breakDown.Activities.Where(x => x.Location == l))
                    {
                        ws.Cells[y, 1, y, 2].Merge = true;
                        ws.Cells["A" + y].Value = "-";
                        ws.Cells[y, 3, y, 4].Merge = true;
                        ws.Cells["C" + y].Value = act.Description;

                        ws.Cells["E" + y].Value = act.Qty;

                        #region Row Styles

                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells[y, 3, y, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                        ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                        if (y % 2 != 0)
                        {
                            ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        }
                        #endregion
                        y++;
                    }
                }

                #endregion

                #endregion

                #endregion
            }
            if (pck.Workbook.Worksheets.Count() > 0)
            {
                pck.Save();
                //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //Response.AddHeader("content-disposition", "attachment;  filename=" + docName + ".xlsx");
            }
        }

        public InvoiceBreakDown WorkOrderBreakDownData(int id)
        {
            var breakDown = new InvoiceBreakDown();
            var workOrder = db.TRN23100.Find(id);
            breakDown.DateDispatched = workOrder.Dispatchdt;
            breakDown.DateRequested = workOrder.Requestdt.Day > 14 ?
                new DateTime(workOrder.Requestdt.Year, workOrder.Requestdt.Month, DateTime.DaysInMonth(workOrder.Requestdt.Year, workOrder.Requestdt.Month)) :
                new DateTime(workOrder.Requestdt.Year, workOrder.Requestdt.Month, 14);
            breakDown.RequestedBy = workOrder.Requestby;
            breakDown.Title = workOrder.Wo_title.ToUpper();
            breakDown.RefNo = workOrder.Wo_ref;
            breakDown.PONo = workOrder.PONum;
            var woActIds = workOrder.TRN23110.Select(i => i.WorkOID).ToList();
            breakDown.Activities = db.TRN09110.Where(x => woActIds.Contains(x.WoActID)).Select(x => new ShortActivity
            {
                Date = x.TRN23110.ActDate,
                Description = x.TRN23110.ADM01100.RateDescr,
                Location = x.TRN23110.Location,
                Qty = x.TRN23110.OActQty
            }).ToList();
            return breakDown;
        }

        #endregion

        #region Summary Price Sheet

        public ActionResult NewSummaryPriceSheet(int id)
        {
            var invoice = NewSummaryData(id);
            ViewBag.InvoiceNo = id;
            ViewBag.Activities = db.ADM01100.Where(x => x.ClientID == 3 && x.Active).OrderBy(x => x.AltCode).ToList();
            return View(invoice);
        }

        #endregion

        #region Invoice Sheet

        public ActionResult NewInvoiceSheet(int id)
        {
            var invoice = NewSummaryData(id);
            ViewBag.InvoiceNo = id;
            return View(invoice);
        }

        #endregion
    }
}
