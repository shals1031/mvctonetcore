using iTextSharp.text;
using iTextSharp.text.pdf;
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
using System.Text;
using System.Threading.Tasks;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;
using TeliconLatest.Reusables;

namespace TeliconLatest.Controllers
{
    public class TechnicianController : Controller
    {
        private readonly TeliconDbContext db;
        private readonly IWebHostEnvironment _env;
        public TechnicianController(TeliconDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            _env = env;
        }
        // GET: /Technician/
        //[Authorize(Roles = "Supervisor, Technician")]
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
            ViewBag.Statuses = DataDictionaries.WordOrderStatuses.Select(x => new SelectListItem
            {
                Text = x.Value,
                Value = x.Key
            }).ToList();
            return View();
        }

        //[Authorize(Roles = "Supervisor, Technician")]
        public ActionResult Statements()
        {
            ViewBag.ContractorID = db.ADM03300.FirstOrDefault(x => x.Email == User.Identity.Name).ConID;
            return View();
        }

        //[Authorize(Roles = "AppAdmin, SuperAdmin, Admin, Supervisor, Technician")]
        public ActionResult StatementDetail(string pdate, int id)
        {
            var data = StatementDetailData(pdate, id);
            return View(data);
        }
        [HttpPost]
        public async Task<IActionResult> StatementDetailToExcel(StatementDetailFilter model)
        {
            string docName = "StatementDetail";
            await Task.Yield();
            var stream = new MemoryStream();
            ExcelPackage pck = new ExcelPackage(stream);

            var data = StatementDetailData(model.pdate, model.id);

            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("StatementDetail");

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

                ws.Column(1).Width = 12;
                ws.Column(2).Width = 8;
                ws.Column(3).Width = 33;
                ws.Column(4).Width = 12;
                ws.Column(5).Width = 11;
                ws.Column(6).Width = 11;
                ws.Column(7).Width = 11;

                int y = 1;
                #region Title
                ws.Cells[y, 1, y, 7].Merge = true;
                ws.Cells["A" + y].Value = "Contractor Statement";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;

                ws.Cells[y, 1, y, 7].Merge = true;
                ws.Cells["A" + y].Value = "Period Starting " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateFrom) + " to " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateTo);
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 10;
                y++;

                ws.Cells[y, 1, y, 7].Merge = true;
                ws.Cells["A" + y].Value = "Contractor : " + ViewBag.ConName;
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 10;
                y++;

                ws.Cells[y, 1, y, 7].Merge = true;
                y++;
                #endregion

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
                ws.Cells["F" + y].Value = "Penalty";
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["G" + y].Value = "Total";
                ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 7].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 7].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 7].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, 7].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(y).Height = 20;
                y++;
                #endregion

                int i = 1;
                foreach (var t in data.Select(x => x.Wo_title).Distinct())
                {
                    ws.Cells[y, 1, y, 7].Merge = true;
                    ws.Cells["A" + y].Value = t;

                    ws.Cells["A" + y].Style.Font.Bold = true;
                    ws.Cells["A" + y].Style.Font.Size = 11;

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[y, 1, y, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[y, 1, y, 7].Style.WrapText = true;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 7].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    y++;
                    i++;

                    foreach (var l in data.Where(x => x.Wo_title == t).Select(x => x.Location).Distinct())
                    {

                        ws.Cells["A" + y].Value = string.Format("{0:dd/MM/yyyy}", data.FirstOrDefault(x => x.Location == l && x.Wo_title == t).ActDate);
                        ws.Cells["B" + y].Value = "";
                        ws.Cells[y, 3, y, 7].Merge = true;
                        ws.Cells["C" + y].Value = l;

                        ws.Cells[y, 1, y, 7].Style.Font.Bold = true;
                        ws.Cells[y, 1, y, 7].Style.Font.Size = 11;

                        #region Row Styles

                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                        ws.Cells[y, 1, y, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ws.Cells[y, 1, y, 7].Style.WrapText = true;

                        ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                        ws.Cells[y, 1, y, 7].Style.Font.Size = 11;
                        if (i % 2 != 0)
                        {
                            ws.Cells[y, 1, y, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[y, 1, y, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        }
                        #endregion

                        y++;
                        i++;

                        foreach (var act in data.Where(x => x.Location == l && x.Wo_title == t))
                        {
                            ws.Cells["A" + y].Value = "";
                            ws.Cells["B" + y].Value = act.OActQty;
                            ws.Cells["C" + y].Value = act.RateDescr;
                            ws.Cells["D" + y].Value = string.Format("{0:C}", act.Cost);
                            ws.Cells["E" + y].Value = string.Format("{0:C}", act.Cost * Convert.ToDouble(act.OActQty));
                            ws.Cells["F" + y].Value = string.Format("{0:C}", (act.Cost * Convert.ToDouble(act.OActQty)) - act.PenalizeAmount);
                            ws.Cells["G" + y].Value = string.Format("{0:C}", act.PenalizeAmount);

                            #region Row Styles

                            ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            ws.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            ws.Cells["F" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            ws.Cells["G" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                            ws.Cells[y, 1, y, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            ws.Cells[y, 1, y, 7].Style.WrapText = true;

                            ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                            ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                            ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                            ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                            ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                            ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                            ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                            ws.Cells[y, 1, y, 7].Style.Font.Size = 11;
                            if (i % 2 != 0)
                            {
                                ws.Cells[y, 1, y, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws.Cells[y, 1, y, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                            }
                            #endregion

                            y++;
                            i++;
                        }
                    }
                }

                var total = data.Sum(x => x.Cost * Convert.ToDouble(x.OActQty));
                var penalizeTotal = data.Sum(x => x.PenalizeAmount);


                ws.Cells[y, 1, y, 4].Merge = true;
                ws.Cells["A" + y].Value = "Total :";

                ws.Cells["E" + y].Value = string.Format("{0:C}", total);
                ws.Cells["F" + y].Value = string.Format("{0:C}", total - penalizeTotal);
                ws.Cells["G" + y].Value = string.Format("{0:C}", penalizeTotal);

                #region Row Styles

                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells["F" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells["G" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws.Cells[y, 1, y, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[y, 1, y, 7].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 7].Style.WrapText = true;

                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 7].Style.Font.Size = 11;
                if (i % 2 != 0)
                {
                    ws.Cells[y, 1, y, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 1, y, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                #endregion

                y++;
                i++;
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

        public List<TechnicianStatementDetail> StatementDetailData(string pdate, int id)
        {
            ViewBag.pdate = pdate;
            ViewBag.id = id;
            var pieces = pdate.Split('-');
            int year = Convert.ToInt32(pieces[0]);
            int periodNo = Convert.ToInt32(pieces[1]);
            var period = Customs.GetPeriods(null, null).FirstOrDefault(x => x.DateFrom.Year == year && x.PeriodNo == periodNo);
            string query = string.Format("CALL TechnicianStatementDetail({0},{1},{2})", id, period.PayDate.ToString("dd/MM/yyyy"), period.PayDate.ToString("dd/MM/yyyy"));
            var data = db.Set<TechnicianStatementDetail>().FromSqlRaw(query).AsEnumerable().Select(x => new TechnicianStatementDetail
            {
                PaidDate = x.PaidDate,
                ActDate = x.ActDate,
                PenalizeAmount = x.PenalizeAmount,
                ActivityID = x.ActivityID,
                AddMaterial = x.AddMaterial,
                Cost = x.Cost,
                Location = x.Location,
                OActQty = x.OActQty,
                PenalizeCost = x.PenalizeCost,
                Penalty = x.Penalty,
                RateDescr = x.RateDescr,
                RateUnit = x.RateUnit,
                Wo_title = x.Wo_title
            }).ToList();
            ViewBag.DateFrom = period.DateFrom;
            ViewBag.DateTo = period.DateTo;
            var con = db.ADM03300.FirstOrDefault(x => x.ConID == id);
            ViewBag.ConName = con != null ? con.FirstName + " " + con.LastName : "";

            var deductions = db.TRN04100.Where(x => x.ConductorID == id && x.StartDate <= period.PayDate && x.EndDate >= period.PayDate).ToList();
            var deductData = deductions.Select(x => new ContractorDeductionReport
            {
                DeductionName = x.ADM04100.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate ?? new DateTime(),
                Amount = x.Amount
            }).ToList();
            if (deductData == null)
                deductData = new List<ContractorDeductionReport>();
            ViewBag.DeductionData = deductData;
            return data;
        }


        //[Authorize(Roles = "AppAdmin, SuperAdmin, Admin, Supervisor, Technician")]
        public ActionResult PeriodDetail(int id, string pdate)
        {
            var data = PeriodDetailData(id, pdate);
            return View(data);
        }
        [HttpPost]
        public async Task<IActionResult> PeriodDetailToExcel(PeriodDetailFilter model)
        {
            string docName = "PeriodDetail";
            await Task.Yield();
            var stream = new MemoryStream();
            ExcelPackage pck = new ExcelPackage(stream);

            var data = PeriodDetailData(model.id, model.pdate).ToList();
            var deductions = (List<ContractorDeductionReport>)ViewBag.DeductionData;

            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("PeriodDetail");

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

                ws.Column(1).Width = 5.5;
                ws.Column(2).Width = 30;
                ws.Column(3).Width = 7;
                ws.Column(4).Width = 12;
                ws.Column(5).Width = 7;
                ws.Column(6).Width = 11;
                ws.Column(7).Width = 11;
                ws.Column(8).Width = 12;

                int y = 1;
                #region Title

                var period = (Period)ViewBag.Period;

                ws.Cells[y, 1, y, 8].Merge = true;
                ws.Cells["A" + y].Value = "Contractor Invoice # " + period.PeriodNo;
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;

                ws.Cells[y, 1, y, 8].Merge = true;
                ws.Cells["A" + y].Value = "Pay Period: " + string.Format("{0:MMMM dd, yyyy} - {1:MMMM dd, yyyy}", period.DateFrom, period.DateTo);
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 10;
                y++;

                ws.Cells[y, 1, y, 8].Merge = true;
                y++;
                #endregion

                #region Payments

                #region Table Headers
                ws.Cells["A" + y].Value = "Item No";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Description";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Unit";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Value = "Rate";
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = "Qty";
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F" + y].Value = "Amount";
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["G" + y].Value = "Penalty";
                ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["H" + y].Value = "Total";
                ws.Cells["H" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 8].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 8].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 8].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, 8].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(y).Height = 20;
                y++;
                #endregion

                int i = 1;
                foreach (var row in data)
                {
                    #region Rows
                    ws.Cells["A" + y].Value = i;
                    ws.Cells["B" + y].Value = row.Description;
                    ws.Cells["C" + y].Value = row.Unit;
                    ws.Cells["D" + y].Value = string.Format("{0:C}", row.Rate);
                    ws.Cells["E" + y].Value = row.Quantity;
                    ws.Cells["F" + y].Value = string.Format("{0:C}", row.Amount);
                    ws.Cells["G" + y].Value = string.Format("{0:C}", row.Penalty);
                    ws.Cells["H" + y].Value = string.Format("{0:C}", row.Total);

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[y, 4, y, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["H" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[y, 1, y, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ws.Cells[y, 1, y, 8].Style.WrapText = true;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["H" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 8].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    #endregion
                    y++;
                    i++;
                }

                var amount = data.Sum(x => x.Amount);
                var penalizeTotal = data.Sum(x => x.Total);

                ws.Cells[y, 1, y, 5].Merge = true;
                ws.Cells["A" + y].Value = "Total:";

                ws.Cells["F" + y].Value = string.Format("{0:C}", amount);
                ws.Cells["G" + y].Value = string.Format("{0:C}", amount - penalizeTotal);
                ws.Cells["H" + y].Value = string.Format("{0:C}", penalizeTotal);

                #region Row Styles

                ws.Cells[y, 1, y, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws.Cells[y, 1, y, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[y, 1, y, 8].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 8].Style.WrapText = true;

                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["H" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 8].Style.Font.Size = 11;
                if (i % 2 != 0)
                {
                    ws.Cells[y, 1, y, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 1, y, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                #endregion
                y++;
                i++;


                ws.Cells[y, 1, y, 5].Merge = true;
                ws.Cells["A" + y].Value = "Net Amount:";

                ws.Cells["H" + y].Value = string.Format("{0:C}", (penalizeTotal - deductions.Sum(x => x.Amount)));

                #region Row Styles


                ws.Cells[y, 4, y, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws.Cells[y, 1, y, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[y, 1, y, 8].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 8].Style.WrapText = true;

                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["H" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 8].Style.Font.Size = 11;
                if (i % 2 != 0)
                {
                    ws.Cells[y, 1, y, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 1, y, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                #endregion
                y++;
                i++;

                #endregion

                ws.Cells[y, 1, y, 8].Merge = true;
                y++;
                i++;
                #region Deductions

                if (deductions.Count() > 0)
                {
                    int d = 1;
                    ws.Cells[y, 1, y, 8].Merge = true;
                    ws.Cells["A" + y].Value = "Deductions";
                    ws.Cells["A" + y].Style.Font.Bold = true;
                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["A" + y].Style.Font.Name = "Arial";
                    ws.Cells["A" + y].Style.Font.Size = 10;
                    y++;
                    i++;
                    ws.Cells[y, 1, y, 8].Merge = true;
                    y++;
                    i++;
                    #region Table Headers
                    ws.Cells["A" + y].Value = "Item No";
                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Value = "Description";
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells[y, 3, y, 4].Merge = true;
                    ws.Cells["C" + y].Value = "Start Date";
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells[y, 5, y, 6].Merge = true;
                    ws.Cells["E" + y].Value = "End Date";
                    ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells[y, 7, y, 8].Merge = true;
                    ws.Cells["G" + y].Value = "Amount";
                    ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["H" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 8].Style.Font.Bold = true;
                    ws.Cells[y, 1, y, 8].Style.Font.Name = "Inherit";
                    ws.Cells[y, 1, y, 8].Style.Font.Size = 11;
                    ws.Cells[y, 1, y, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                    ws.Cells[y, 1, y, 8].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                    ws.Cells[y, 1, y, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 1, y, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    ws.Cells[y, 1, y, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[y, 1, y, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Row(y).Height = 20;
                    y++;
                    i++;
                    #endregion

                    foreach (var deduction in deductions)
                    {
                        #region Rows
                        ws.Cells["A" + y].Value = d;
                        ws.Cells["B" + y].Value = deduction.DeductionName;
                        ws.Cells[y, 3, y, 4].Merge = true;
                        ws.Cells["C" + y].Value = string.Format("{0:dd/MM/yyyy}", deduction.StartDate);
                        ws.Cells[y, 5, y, 6].Merge = true;
                        ws.Cells["E" + y].Value = string.Format("{0:dd/MM/yyyy}", deduction.EndDate);
                        ws.Cells[y, 7, y, 8].Merge = true;
                        ws.Cells["G" + y].Value = string.Format("{0:C}", deduction.Amount);

                        #region Row Styles

                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[y, 3, y, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        ws.Cells[y, 1, y, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        ws.Cells[y, 1, y, 8].Style.WrapText = true;

                        ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["H" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                        ws.Cells[y, 1, y, 8].Style.Font.Size = 11;
                        if (i % 2 != 0)
                        {
                            ws.Cells[y, 1, y, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[y, 1, y, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        }
                        #endregion

                        #endregion
                        y++;
                        i++;
                        d++;
                    }

                    ws.Cells[y, 1, y, 6].Merge = true;
                    ws.Cells["A" + y].Value = "Total:";
                    ws.Cells[y, 7, y, 8].Merge = true;
                    ws.Cells["G" + y].Value = string.Format("{0:C}", deductions.Sum(x => x.Amount));

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells["G" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[y, 1, y, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[y, 1, y, 8].Style.Font.Bold = true;
                    ws.Cells[y, 1, y, 8].Style.WrapText = true;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["H" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 8].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion
                    y++;
                    i++;
                }

                #endregion


                ws.Cells[y, 1, y, 8].Merge = true;
                ws.Cells["A" + y].Value = "Name:";
                ws.Cells[y, 2, y, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                ws.Cells[y, 2, y, 8].Style.Border.Bottom.Color.SetColor(Color.FromArgb(230, 230, 230));
                y++;
                i++;
                ws.Cells[y, 1, y, 8].Merge = true;
                ws.Cells["A" + y].Value = "Signature:";
                ws.Cells[y, 2, y, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                ws.Cells[y, 2, y, 8].Style.Border.Bottom.Color.SetColor(Color.FromArgb(230, 230, 230));
                y++;
                i++;
                ws.Cells[y, 1, y, 8].Merge = true;
                ws.Cells["A" + y].Value = "Date:";
                ws.Cells[y, 2, y, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                ws.Cells[y, 2, y, 8].Style.Border.Bottom.Color.SetColor(Color.FromArgb(230, 230, 230));
                y++;
                i++;

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

        public IEnumerable<PeriodDetail> PeriodDetailData(int id, string pdate)
        {
            ViewBag.pdate = pdate;
            ViewBag.id = id;
            var pieces = pdate.Split('-');
            int year = Convert.ToInt32(pieces[0]);
            int period = Convert.ToInt32(pieces[1]);
            var p = Customs.GetPeriods(year, year).FirstOrDefault(x => x.PeriodNo == period);
            ViewBag.Period = p;

            string query = string.Format("CALL TechnicianStatementDetail({0},{1},{2})", id, p.PayDate.ToString("dd/MM/yyyy"), p.PayDate.ToString("dd/MM/yyyy"));
            var TRecords = db.Set<TechnicianStatementDetail>().FromSqlRaw(query).AsEnumerable().Select(x => new TechnicianStatementDetail
            {
                PaidDate = x.PaidDate,
                ActDate = x.ActDate,
                PenalizeAmount = x.PenalizeAmount,
                ActivityID = x.ActivityID
            }).GroupBy(x => x.ActivityID).ToList();
            List<PeriodDetail> model = new List<PeriodDetail>();
            foreach (var item in TRecords)
            {
                PeriodDetail m = new PeriodDetail();
                decimal Qty = 0;
                decimal Amt = 0;
                double pnlty = 0;
                double ttl = 0;
                foreach (var item1 in item)
                {
                    Qty = Qty + item1.OActQty;
                    Amt = Amt + (item1.OActQty * Convert.ToDecimal(item1.Cost));
                    pnlty = pnlty + ((Convert.ToDouble(item1.OActQty) * item1.Cost) - item1.PenalizeAmount.Value);
                    ttl = ttl + item1.PenalizeAmount.Value;
                    m.Description = item1.RateDescr;
                    m.Unit = item1.RateUnit;
                    m.Rate = item1.Cost;
                }
                m.Quantity = Qty;
                m.Amount = Convert.ToDouble(Amt);
                m.Penalty = pnlty;
                m.Total = ttl;
                model.Add(m);
            }

            IEnumerable<PeriodDetail> data = model.OrderBy(x => x.Description).AsEnumerable();

            var deductions = db.TRN04100.Where(x => x.ConductorID == id && x.StartDate <= p.PayDate && x.EndDate >= p.PayDate).ToList();
            var deductData = deductions.Select(x => new ContractorDeductionReport
            {
                DeductionName = x.ADM04100.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate ?? new DateTime(),
                Amount = x.Amount
            }).ToList();
            if (deductData == null)
                deductData = new List<ContractorDeductionReport>();
            ViewBag.DeductionData = deductData;

            return data;
        }

        #region Paysilp
        //[Authorize(Roles = "AppAdmin, SuperAdmin, Admin, Supervisor, Technician")]
        public ActionResult TechPaySlip(int? p_ConId = null)
        {
            ViewBag.ContractorID = p_ConId.HasValue ? int.Parse(p_ConId.ToString()) : db.ADM03300.FirstOrDefault(x => x.Email == User.Identity.Name).ConID;
            ViewBag.IsAdmin = p_ConId.HasValue;
            if (p_ConId.HasValue)
            {
                ADM03300 _ADM03300 = db.ADM03300.Where(t => t.ConID == p_ConId).FirstOrDefault();
                if (_ADM03300 != null)
                    ViewBag.ContName = _ADM03300.FirstName + " " + _ADM03300.LastName;
            }
            var minYear = db.TRN23100.Min(x => x.Requestdt).Year;
            var maxYear = db.TRN23100.Max(x => x.Requestdt).Year;
            var yearsForSelection = new List<SelectListItem>();
            for (int year = minYear; year <= maxYear; year++)
            {
                yearsForSelection.Add(new SelectListItem { Text = year.ToString(), Value = year.ToString(), Selected = year == maxYear });
            }
            ViewBag.Years = yearsForSelection;
            var periodList = new List<SelectListItem>();
            var periods = Customs.GetPeriods(DateTime.Now.Year - 1, DateTime.Now.Year + 1);
            int currentWeek = 1;
            Period period = null;
            if (periods.Count > 0)
                period = periods.Find(x => x.DateFrom <= DateTime.Now && x.DateTo >= DateTime.Now);
            if (period != null)
                currentWeek = period.PeriodNo;
            for (int x = 1; x <= 27; x++)
            {
                periodList.Add(new SelectListItem { Value = x.ToString(), Text = "Period " + x, Selected = x == currentWeek });
            }
            ViewBag.PeriodList = periodList;
            return View();
        }

        #region Create PDF

        [HttpPost]
        public FileResult GenerateTechPaySlip(TempR model)
        {
            int year = model.periodStartYear;
            int period = model.periodStart;
            var p = Customs.GetPeriods(year, year).FirstOrDefault(x => x.PeriodNo == period);

            var statements = new List<ContractorsStatement>();// db.ContractorsStatement(p.PayDate, p.PayDate).ToList();
            statements = statements.FindAll(x => x.ContractorID == model.id);
            var TRecords = statements.Join(db.ADM03300, x => x.ContractorID, y => y.ConID, (x, y) => new ContractorPaySlipReport
            {
                EmployeeID = y.EmployeeID,
                FirstName = y.FirstName,
                LastName = y.LastName,
                Department = y.ADM04200.Name,
                Level = DataDictionaries.UserTypes[y.ConClass],
                Total = x.Total.Value,
                PenalizeTotal = x.PenalizeTotal.HasValue ? Convert.ToDouble(x.PenalizeTotal.Value) : 0,
                IsEarning = true,
                ConId = y.ConID,
                BankAcc = y.Bankacc,
                PayDate = p.PayDate,
                EngagementDt = y.EngagementDt,
                TRN = y.TRN,
                NIS = y.NIS,
                ContNo = y.EmployeeID
            }).ToList();

            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("ContractorPaySlips_" + p.PayDate.ToString("yyyyMMdd") + ".pdf");
            //file will created in this path
            string strAttachment = Path.Combine(_env.WebRootPath, "Downloadss", strPDFFileName);

            Document doc = new Document();
            doc.SetPageSize(PageSize.A4);
            doc.SetMargins(0f, 0f, 0f, 0f);
            doc.SetMargins(0f, 0f, 0f, 0f);
            PdfWriter writer = PdfWriter.GetInstance(doc, workStream);
            writer.CloseStream = false;

            doc.Open();
            PdfContentByte cb = writer.DirectContent;
            if (TRecords != null && TRecords.Any())
            {
                List<int> conIds = TRecords.Select(x => x.ConId).ToList();
                //Create Pay slip border
                int c = 2;
                int i = 0;
                foreach (var conId in conIds)
                {
                    var rows = TRecords.FindAll(x => x.ConId == conId);
                    var deductions = db.TRN04100.Where(x => x.ConductorID == conId && x.StartDate <= p.PayDate && x.EndDate >= p.PayDate).ToList();
                    var deductData = deductions.Select(x => new ContractorDeductionReport
                    {
                        DeductionName = x.ADM04100.Name,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate ?? new DateTime(),
                        Amount = x.Amount
                    }).ToList();

                    var img = iTextSharp.text.Image.GetInstance(new Uri(Path.Combine(_env.WebRootPath, "images", "print-logo.png")));
                    img.ScaleAbsolute(104, 56);
                    img.SetAbsolutePosition(11, ((c * (doc.PageSize.Height / 3)) + 5) + (doc.PageSize.Height / 3) - 70);
                    doc.Add(img);

                    cb.Rectangle(10f, (c * (doc.PageSize.Height / 3)) + 5, doc.PageSize.Width - 20, (doc.PageSize.Height / 3) - 10);
                    cb.Stroke();
                    doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 8f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    PdfPTable HeaderTableLayout = new PdfPTable(6)
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    };
                    doc.Add(Add_PaySlipHeader_To_PDF(HeaderTableLayout, doc.PageSize.Width / 8, rows[0], p.DateFrom, p.DateTo));
                    doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 5f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    PdfPTable tableLayout = new PdfPTable(7)
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    doc.Add(Add_Content_To_PDF(tableLayout, doc.PageSize.Width / 16, rows, deductData));
                    if ((i + 1) % 3 != 0)
                        doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 9f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    else
                        doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 4f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    if (i % 3 == 2)
                        c = 2;
                    else
                        c--;
                    i++;
                }
            }
            else
                doc.Add(new Paragraph(new Chunk("No Records found", FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return File(workStream, "application/pdf", strPDFFileName);
        }

        protected PdfPTable Add_PaySlipHeader_To_PDF(PdfPTable tableLayout, float minColWidth, ContractorPaySlipReport detail, DateTime pStart, DateTime pEnd)
        {
            float[] headers = { 2, 6, 3, 4, 2, 3 };  //Header Width
            tableLayout.SetWidths(headers);        //Set the pdf headers
            tableLayout.WidthPercentage = 78;       //Set the PDF File witdh percentage
                                                    //tableLayout.HeaderRows = 1;
                                                    //Add Title to the PDF file at the top
                                                    //tableLayout.

            tableLayout.AddCell(new PdfPCell(new Phrase("Cont :", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.BOLD))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase(detail.FirstName + " " + detail.LastName, FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.NORMAL))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase("Cont. No :", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.BOLD))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase(detail.ContNo, FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.NORMAL))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase("DOE :", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.BOLD))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase(detail.EngagementDt.HasValue ? detail.EngagementDt.Value.ToString("dd MMM yyyy") : "", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.NORMAL))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });

            tableLayout.AddCell(new PdfPCell(new Phrase("Period :", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.BOLD))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase(pStart.ToString("dd MMM yyyy") + " - " + pEnd.ToString("dd MMM yyyy"), FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.NORMAL))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase("Method :", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.BOLD))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase("LDG", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.NORMAL))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase("Date :", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.BOLD))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase(detail.PayDate.ToString("dd MMM yyyy"), FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.NORMAL))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });

            tableLayout.AddCell(new PdfPCell(new Phrase("Cost :", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.BOLD))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase(detail.Department, FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.NORMAL))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase("NIS :", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.BOLD))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase(detail.NIS, FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.NORMAL))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase("TRN :", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.BOLD))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase(detail.TRN, FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.NORMAL))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });

            return tableLayout;
        }

        protected PdfPTable Add_Content_To_PDF(PdfPTable tableLayout, float minColWidth, List<ContractorPaySlipReport> rows, List<ContractorDeductionReport> deductions)
        {

            float[] headers = {
                Convert.ToInt32(minColWidth * 3.5),
                Convert.ToInt32(minColWidth * 0),
                Convert.ToInt32(minColWidth * 1.5) + Convert.ToInt32(minColWidth * 0.75),
                Convert.ToInt32(minColWidth * 1.5) + Convert.ToInt32(minColWidth * 0.75),
                Convert.ToInt32(minColWidth * 3.5),
                Convert.ToInt32(minColWidth * 1.5) + Convert.ToInt32(minColWidth * 0.75),
                Convert.ToInt32(minColWidth * 1.5) + Convert.ToInt32(minColWidth * 0.75)
            };  //Header Widths
            tableLayout.SetWidths(headers);        //Set the pdf headers
            tableLayout.WidthPercentage = 96.5f;       //Set the PDF File witdh percentage
            //tableLayout.HeaderRows = 1;
            //Add Title to the PDF file at the top

            var boldText = iTextSharp.text.Font.BOLD;
            var normalText = iTextSharp.text.Font.NORMAL;

            tableLayout.AddCell(new PdfPCell(new Phrase("EARNINGS", FontFactory.GetFont("Ebrima", 9, boldText))) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_TOP });
            tableLayout.AddCell(new PdfPCell(new Phrase("DEDUCTIONS", FontFactory.GetFont("Ebrima", 9, boldText))) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });


            ////Add header
            AddCell(tableLayout, "Description", boldText, 9, Element.ALIGN_LEFT, 0, 0, 1, 0, 5, 0, 2, 2);
            AddCell(tableLayout, " ", boldText, 9, Element.ALIGN_RIGHT, 0, 0, 1, 0, 2, 2);
            //AddCell(tableLayout, "Rate", Font.BOLD, 9, Element.ALIGN_RIGHT, 0, 0, 1, 0, 2, 2);
            AddCell(tableLayout, "Amt", boldText, 9, Element.ALIGN_RIGHT, 0, 0, 1, 0, 2, 2);
            AddCell(tableLayout, "Y-T-D", boldText, 9, Element.ALIGN_RIGHT, 0, 1, 1, 0, 0, 5, 2, 2);
            AddCell(tableLayout, "Description", boldText, 9, Element.ALIGN_LEFT, 0, 0, 1, 0, 5, 0, 2, 2);
            AddCell(tableLayout, "Amt", boldText, 9, Element.ALIGN_RIGHT, 0, 0, 1, 0, 2, 2);
            AddCell(tableLayout, "Y-T-D", boldText, 9, Element.ALIGN_RIGHT, 0, 0, 1, 0, 0, 5, 2, 2);

            ////Add body
            int d = 0;
            double totalDeductions = 0;
            for (int i = 0; i <= 11; i++)
            {
                ContractorPaySlipReport row = null;
                if (rows.Count > i)
                    row = rows[i];
                int botBrdr = 0;
                if (i == 11)
                    botBrdr = 1;

                AddCell(tableLayout, row != null ? "Earnings" : " ", boldText, 9, Element.ALIGN_LEFT, 0, 0, botBrdr, 0, 5, 0, 2, 2);
                AddCell(tableLayout, " ", boldText, 9, Element.ALIGN_RIGHT, 0, 0, botBrdr, 0, 2, 2);
                //AddCell(tableLayout, "aaaa ", Font.BOLD, 9, Element.ALIGN_RIGHT, 0, 0, botBrdr, 0, 2, 2);
                AddCell(tableLayout, row != null ? string.Format("{0:C}", row.Total) : " ", normalText, 9, Element.ALIGN_RIGHT, 0, 0, botBrdr, 0, 2, 2);
                AddCell(tableLayout, row != null ? string.Format("{0:C}", row.Total) : " ", normalText, 9, Element.ALIGN_RIGHT, 0, 1, botBrdr, 0, 0, 5, 2, 2);
                if (row != null && row.Total - row.PenalizeTotal > 0)
                {
                    AddCell(tableLayout, "Late submit panalty", boldText, 9, Element.ALIGN_LEFT, 0, 0, botBrdr, 0, 5, 0, 2, 2);
                    AddCell(tableLayout, string.Format("{0:C}", row.Total - row.PenalizeTotal), normalText, 9, Element.ALIGN_RIGHT, 0, 0, botBrdr, 0, 2, 2);
                    AddCell(tableLayout, string.Format("{0:C}", row.Total - row.PenalizeTotal), boldText, 9, Element.ALIGN_RIGHT, 0, 0, botBrdr, 0, 0, 5, 2, 2);
                    totalDeductions = row.Total - row.PenalizeTotal;
                }
                else
                {
                    ContractorDeductionReport deduction = null;
                    if (deductions.Count > d)
                        deduction = deductions[d];
                    AddCell(tableLayout, deduction != null ? deduction.DeductionName : " ", boldText, 9, Element.ALIGN_LEFT, 0, 0, botBrdr, 0, 5, 0, 2, 2);
                    AddCell(tableLayout, deduction != null ? string.Format("{0:C}", deduction.Amount) : " ", normalText, 9, Element.ALIGN_RIGHT, 0, 0, botBrdr, 0, 2, 2);
                    AddCell(tableLayout, deduction != null ? string.Format("{0:C}", deduction.Amount) : " ", boldText, 9, Element.ALIGN_RIGHT, 0, 0, botBrdr, 0, 0, 5, 2, 2);
                    if (deduction != null)
                        totalDeductions += deduction.Amount;
                    d++;
                }
            }

            AddCell(tableLayout, "Earnings :", boldText, 9, Element.ALIGN_LEFT, 0, 0, 1, 0, 5, 0, 2, 2);
            AddCell(tableLayout, " ", boldText, 9, Element.ALIGN_RIGHT, 0, 0, 1, 0, 2, 2);
            //AddCell(tableLayout, " ", Font.BOLD, 9, Element.ALIGN_RIGHT, 0, 0, 1, 0, 2, 2);
            AddCell(tableLayout, string.Format("{0:C}", rows.Sum(x => x.Total)), normalText, 9, Element.ALIGN_RIGHT, 0, 0, 1, 0, 2, 2);
            AddCell(tableLayout, string.Format("{0:C}", rows.Sum(x => x.Total)), normalText, 9, Element.ALIGN_RIGHT, 0, 1, 1, 0, 0, 5, 2, 2);
            AddCell(tableLayout, "Deductions :", boldText, 9, Element.ALIGN_LEFT, 0, 0, 1, 0, 5, 0, 2, 2);
            AddCell(tableLayout, string.Format("{0:C}", totalDeductions), normalText, 9, Element.ALIGN_RIGHT, 0, 0, 1, 0, 2, 2);
            AddCell(tableLayout, string.Format("{0:C}", totalDeductions), normalText, 9, Element.ALIGN_RIGHT, 0, 0, 1, 0, 0, 5, 2, 2);

            AddCell(tableLayout, " ", boldText, 9, Element.ALIGN_LEFT, 0, 0, 0, 0, 2, 2);
            AddCell(tableLayout, " ", boldText, 9, Element.ALIGN_RIGHT, 0, 0, 0, 0, 2, 2);
            //AddCell(tableLayout, " ", Font.BOLD, 9, Element.ALIGN_RIGHT, 0, 0, 0, 0, 2, 2);
            AddCell(tableLayout, " ", boldText, 9, Element.ALIGN_RIGHT, 0, 0, 0, 0, 2, 2);
            AddCell(tableLayout, " ", boldText, 9, Element.ALIGN_LEFT, 0, 1, 0, 0, 2, 2);
            AddCell(tableLayout, "Net Pay :", boldText, 9, Element.ALIGN_LEFT, 0, 0, 1, 0, 5, 0, 2, 2);
            AddCell(tableLayout, string.Format("{0:C}", rows.Sum(x => x.Total) - totalDeductions), normalText, 9, Element.ALIGN_RIGHT, 0, 0, 1, 0, 2, 2);
            AddCell(tableLayout, string.Format("{0:C}", rows.Sum(x => x.Total) - totalDeductions), normalText, 9, Element.ALIGN_RIGHT, 0, 0, 1, 0, 0, 5, 2, 2);

            return tableLayout;
        }

        private static void AddCell(PdfPTable tableLayout, string cellText, int style, int size, int aling)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, FontFactory.GetFont("Ebrima", size, style))) { HorizontalAlignment = aling });
        }
        private static void AddCell(PdfPTable tableLayout, string cellText, int style, int size, int aling, int borberTop, int borderRight, int borderBottom, int borderLeft)
        {
            var cell = new PdfPCell(new Phrase(cellText, FontFactory.GetFont("Ebrima", size, style))) { HorizontalAlignment = aling };
            if (borberTop == 0)
                cell.BorderWidthTop = 0;
            if (borderRight == 0)
                cell.BorderWidthRight = 0;
            if (borderBottom == 0)
                cell.BorderWidthBottom = 0;
            if (borderLeft == 0)
                cell.BorderWidthLeft = 0;
            tableLayout.AddCell(cell);
        }
        private static void AddCell(PdfPTable tableLayout, string cellText, int style, int size, int aling, int borberTop, int borderRight, int borderBottom, int borderLeft, int paddingLeft, int paddingRight)
        {
            var cell = new PdfPCell(new Phrase(cellText, FontFactory.GetFont("Ebrima", size, style))) { HorizontalAlignment = aling, PaddingLeft = paddingLeft, PaddingRight = paddingRight };
            if (borberTop == 0)
                cell.BorderWidthTop = 0;
            if (borderRight == 0)
                cell.BorderWidthRight = 0;
            if (borderBottom == 0)
                cell.BorderWidthBottom = 0;
            if (borderLeft == 0)
                cell.BorderWidthLeft = 0;
            tableLayout.AddCell(cell);
        }
        private static void AddCell(PdfPTable tableLayout, string cellText, int style, int size, int aling, int borberTop, int borderRight, int borderBottom, int borderLeft, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom)
        {
            var cell = new PdfPCell(new Phrase(cellText, FontFactory.GetFont("Ebrima", size, style))) { HorizontalAlignment = aling, PaddingLeft = paddingLeft, PaddingRight = paddingRight, PaddingTop = paddingTop, PaddingBottom = paddingBottom };
            if (borberTop == 0)
                cell.BorderWidthTop = 0;
            if (borderRight == 0)
                cell.BorderWidthRight = 0;
            if (borderBottom == 0)
                cell.BorderWidthBottom = 0;
            if (borderLeft == 0)
                cell.BorderWidthLeft = 0;
            tableLayout.AddCell(cell);
        }

        // Method to add single cell to the Header
        private static void AddCellToHeader(PdfPTable tableLayout, string cellText)
        {

            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.YELLOW))) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(128, 0, 0) });
        }

        // Method to add single cell to the body
        private static void AddCellToBody(PdfPTable tableLayout, string cellText)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.BLACK))) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255) });
        }

        #endregion


        [HttpPost]
        public FileResult GenerateTechPaySlipForPeriod(string pdate, int id)
        {
            var pieces = pdate.Split('-');
            int year = Convert.ToInt32(pieces[0]);
            int periodNo = Convert.ToInt32(pieces[1]);
            var p = Customs.GetPeriods(null, null).FirstOrDefault(x => x.DateFrom.Year == year && x.PeriodNo == periodNo);

            string query = string.Format("CALL ContractorsStatement({0},{1})", p.PayDate.ToString("dd/MM/yyyy"), p.PayDate.ToString("dd/MM/yyyy"));
            var statements = db.Set<ContractorsStatement>().FromSqlRaw(query).AsEnumerable().Select(x => new
            {
                x.ContractorID,
                x.PenalizeTotal,
                x.Total
            }).ToList();

            statements = statements.FindAll(x => x.ContractorID == id);
            var TRecords = statements.Join(db.ADM03300, x => x.ContractorID, y => y.ConID, (x, y) => new ContractorPaySlipReport
            {
                EmployeeID = y.EmployeeID,
                FirstName = y.FirstName,
                LastName = y.LastName,
                Department = y.ADM04200.Name,
                Level = DataDictionaries.UserTypes[y.ConClass],
                Total = x.Total.Value,
                PenalizeTotal = x.PenalizeTotal.HasValue ? Convert.ToDouble(x.PenalizeTotal.Value) : 0,
                IsEarning = true,
                ConId = y.ConID,
                BankAcc = y.Bankacc,
                PayDate = p.PayDate,
                EngagementDt = y.EngagementDt,
                TRN = y.TRN,
                NIS = y.NIS,
                ContNo = y.TechNo
            }).ToList();

            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("ContractorPaySlips_" + p.PayDate.ToString("yyyyMMdd") + ".pdf");
            //file will created in this path
            string strAttachment = Path.Combine(_env.WebRootPath, "Downloadss", strPDFFileName);

            Document doc = new Document();
            doc.SetPageSize(PageSize.A4);
            doc.SetMargins(0f, 0f, 0f, 0f);
            doc.SetMargins(0f, 0f, 0f, 0f);
            PdfWriter writer = PdfWriter.GetInstance(doc, workStream);
            writer.CloseStream = false;

            doc.Open();
            PdfContentByte cb = writer.DirectContent;
            if (TRecords != null && TRecords.Count > 0)
            {
                List<int> conIds = TRecords.Select(x => x.ConId).ToList();
                //Create Pay slip border
                int c = 2;
                int i = 0;
                foreach (var conId in conIds)
                {
                    var rows = TRecords.FindAll(x => x.ConId == conId);
                    var deductions = db.TRN04100.Where(x => x.ConductorID == conId && x.StartDate <= p.PayDate && x.EndDate >= p.PayDate).ToList();
                    var deductData = deductions.Select(x => new ContractorDeductionReport
                    {
                        DeductionName = x.ADM04100.Name,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate ?? new DateTime(),
                        Amount = x.Amount
                    }).ToList();

                    var img = iTextSharp.text.Image.GetInstance(new Uri(Path.Combine(_env.WebRootPath, "images", "print-logo.png")));
                    img.ScaleAbsolute(104, 56);
                    img.SetAbsolutePosition(11, ((c * (doc.PageSize.Height / 3)) + 5) + (doc.PageSize.Height / 3) - 70);
                    doc.Add(img);

                    cb.Rectangle(10f, (c * (doc.PageSize.Height / 3)) + 5, doc.PageSize.Width - 20, (doc.PageSize.Height / 3) - 10);
                    cb.Stroke();
                    doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 8f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    PdfPTable HeaderTableLayout = new PdfPTable(6)
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    };
                    doc.Add(Add_PaySlipHeader_To_PDF(HeaderTableLayout, doc.PageSize.Width / 8, rows[0], p.DateFrom, p.DateTo));
                    doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 5f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    PdfPTable tableLayout = new PdfPTable(7);
                    tableLayout.HorizontalAlignment = Element.ALIGN_CENTER;
                    doc.Add(Add_Content_To_PDF(tableLayout, doc.PageSize.Width / 16, rows, deductData));
                    if ((i + 1) % 3 != 0)
                        doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 9f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    else
                        doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 4f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    if (i % 3 == 2)
                        c = 2;
                    else
                        c--;
                    i++;
                }
            }
            else
                doc.Add(new Paragraph(new Chunk("No Records found", FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return File(workStream, "application/pdf", strPDFFileName);
        }

        #endregion
    }
}