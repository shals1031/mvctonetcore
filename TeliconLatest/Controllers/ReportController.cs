using iTextSharp.text;
using iTextSharp.text.pdf;
using LinqKit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;
using TeliconLatest.Reusables;

namespace TeliconLatest.Controllers
{
    public class ReportController : Controller
    {
        private readonly TeliconDbContext db;
        private readonly IWebHostEnvironment _env;

        public ReportController(TeliconDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            _env = env;
        }

        #region Contractor Statements

        //[TeliconAuthorize(TaskId = 22)]
        public ActionResult ContractorStatements()
        {
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

        [HttpPost]
        public ActionResult GenerateContractorStatements(string sortBy, string direction, DateTime? dateFrom, DateTime? dateTo, int? periodStart, int? periodStartYear, int? periodEnd, int? periodEndYear)
        {
            return PartialView("Partials/ContractorStatementsPartial", ContractorStatementsData(sortBy, direction, dateFrom, dateTo, periodStart, periodStartYear, periodEnd, periodEndYear));
        }

        [HttpPost]
        public void ContractorStatementsToExcel(ContractorStatmentsFilter model)
        {
            string docName = "ContractorStatements";
            ExcelPackage pck = new ExcelPackage();

            var data = ContractorStatementsData(model.sortBy, model.direction, model.dateFrom, model.dateTo, model.periodStart, model.periodStartYear, model.periodEnd, model.periodEndYear);

            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("ContractorStatements");

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

                ws.Column(1).Width = 15;
                ws.Column(2).Width = 20;
                ws.Column(3).Width = 20;
                ws.Column(4).Width = 14;
                ws.Column(5).Width = 14;
                ws.Column(6).Width = 14;

                #region Title
                ws.Cells[1, 1, 1, 6].Merge = true;
                ws.Cells["A1"].Value = "Contractors Statement";
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A1"].Style.Font.Name = "Arial";
                ws.Cells["A1"].Style.Font.Size = 12;

                ws.Cells[2, 1, 2, 6].Merge = true;
                ws.Cells["A2"].Value = "Period Starting " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateFrom) + " to " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateTo);
                ws.Cells["A2"].Style.Font.Bold = true;
                ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A2"].Style.Font.Name = "Arial";
                ws.Cells["A2"].Style.Font.Size = 10;

                ws.Cells[3, 1, 3, 6].Merge = true;
                #endregion

                #region Table Headers
                ws.Cells["A4"].Value = "Employee No.";
                ws.Cells["A4"].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B4"].Value = "First Name";
                ws.Cells["B4"].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C4"].Value = "Last Name";
                ws.Cells["C4"].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D4"].Value = "Amount";
                ws.Cells["D4"].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E4"].Value = "Penalty";
                ws.Cells["E4"].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F4"].Value = "Total";
                ws.Cells["F4"].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[4, 1, 4, 6].Style.Font.Bold = true;
                ws.Cells[4, 1, 4, 6].Style.Font.Name = "Inherit";
                ws.Cells[4, 1, 4, 6].Style.Font.Size = 11;
                ws.Cells[4, 1, 4, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[4, 1, 4, 6].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[4, 1, 4, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[4, 1, 4, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[4, 1, 4, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[4, 1, 4, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(4).Height = 20;
                #endregion

                int y = 5;
                foreach (var row in data)
                {
                    #region Rows
                    ws.Cells["A" + y].Value = row.EmployeeID;
                    ws.Cells["B" + y].Value = row.FirstName;
                    ws.Cells["C" + y].Value = row.LastName;
                    ws.Cells["D" + y].Value = string.Format("{0:C}", row.Total);
                    ws.Cells["E" + y].Value = string.Format("{0:C}", (row.Total - row.PenalizeTotal));
                    ws.Cells["F" + y].Value = string.Format("{0:C}", row.PenalizeTotal);

                    #region Row Styles
                    ws.Cells[y, 4, y, 6].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[y, 4, y, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 6].Style.Font.Size = 11;
                    if (y % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    #endregion
                    y++;
                }

                var total = string.Format("{0:C}", data.Sum(x => x.PenalizeTotal));

                #region Total
                ws.Cells[y, 1, y, 5].Merge = true;
                ws.Cells["A" + y].Value = "Total:";
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[y, 1, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F" + y].Value = string.Format("{0:C}", total);
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 1, y, 6].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 6].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 6].Style.Font.Color.SetColor(Color.FromArgb(165, 0, 33));
                ws.Cells[y, 1, y, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                if (y % 2 != 0)
                {
                    ws.Cells[y, 1, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 1, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                y++;
                #endregion

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public List<ContractorStatementReport> ContractorStatementsData(string sortBy, string direction, DateTime? dateFrom, DateTime? dateTo, int? periodStart, int? periodStartYear, int? periodEnd, int? periodEndYear)
        {
            Period pEnd = null;
            Period pStart = null;
            DateTime? start = null;
            DateTime? end = null;
            if (!dateFrom.HasValue && !dateTo.HasValue)
            {
                var periods = Customs.GetPeriods(periodStartYear.Value, periodEndYear.Value);
                if (periods.Any())
                {
                    pEnd = periods.FirstOrDefault(x => x.PeriodNo == periodEnd.Value && x.PeriodYear == periodEndYear.Value);
                    pStart = periods.FirstOrDefault(x => x.PeriodNo == periodStart.Value && x.PeriodYear == periodStartYear.Value);
                    if (pEnd != null && pStart != null)
                    {
                        dateFrom = pStart.DateFrom;
                        start = pStart.PayDate;
                        end = pEnd.PayDate;
                        dateTo = pEnd.DateTo;
                    }
                }
            }
            else
            {
                start = dateFrom.Value;
                end = dateTo.Value;
            }
            var data = new List<ContractorStatementReport>();
            if (start.HasValue && end.HasValue)
            {
                ViewBag.DateFrom = dateFrom;
                ViewBag.DateTo = dateTo;
                var statements = db.Set<ContractorsStatement>().FromSqlRaw("CALL ContractorsStatement('" + start.Value.ToString("yyyy-MM-dd") + "','" + end.Value.ToString("yyyy-MM-dd") + "')").ToList();
                data = statements.Join(db.ADM03300.Include(z => z.ADM04200), x => x.ContractorID, y => y.ConID, (x, y) => new ContractorStatementReport
                {
                    EmployeeID = y.EmployeeID,
                    FirstName = y.FirstName,
                    LastName = y.LastName,
                    Department = y.ADM04200.Name,
                    Level = DataDictionaries.UserTypes[y.ConClass],
                    Total = x.Total.Value,
                    PenalizeTotal = x.PenalizeTotal.HasValue ? Convert.ToDouble(x.PenalizeTotal.Value) : 0,
                    IsEarning = true
                }).AsQueryable().OrderBy(sortBy + " " + direction).ToList();
            }
            return data;
        }

        #endregion

        #region YTD Payment report
        //[TeliconAuthorize(TaskId = 27)]
        public ActionResult YTDPayment()
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
        [HttpPost]
        public ActionResult GenerateYTDPayments(int? periodStartYear, int? periodEndYear)
        {
            var data = YTDPaymentsData(periodStartYear, periodEndYear);
            return PartialView("Partials/YTDPaymentsPartial", data);
        }
        [HttpPost]
        public void YTDPaymentsToExcel(YTDPaymentsFilter model)
        {
            string docName = "YTDPayments";
            ExcelPackage pck = new ExcelPackage();

            var data = YTDPaymentsData(model.periodStartYear, model.periodEndYear);
            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("YTDPayments");

                #region View and Print Seetings

                //ws.View.PageLayoutView = true;
                ws.PrinterSettings.ShowGridLines = false;

                ws.PrinterSettings.PaperSize = ePaperSize.A4;
                ws.PrinterSettings.Orientation = eOrientation.Landscape;
                ws.PrinterSettings.TopMargin = 0.75M;
                ws.PrinterSettings.RightMargin = 0.1M;
                ws.PrinterSettings.BottomMargin = 0.75M;
                ws.PrinterSettings.LeftMargin = 0.1M;
                ws.PrinterSettings.HeaderMargin = 0.0M;
                ws.PrinterSettings.FooterMargin = 0.0M;

                #endregion

                ws.Column(1).Width = 12;
                ws.Column(2).Width = 10.1;
                ws.Column(3).Width = 10.1;
                ws.Column(4).Width = 10.1;
                ws.Column(5).Width = 10.1;
                ws.Column(6).Width = 10.1;
                ws.Column(7).Width = 10.1;
                ws.Column(8).Width = 10.1;
                ws.Column(9).Width = 10.1;
                ws.Column(10).Width = 10.1;
                ws.Column(11).Width = 10.1;
                ws.Column(12).Width = 10.1;
                ws.Column(13).Width = 10.1;
                ws.Column(14).Width = 11;
                int y = 1;

                #region Title
                ws.Cells[y, 1, y, 14].Merge = true;
                ws.Cells["A" + y].Value = "Annual Contractor Earnings";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;

                ws.Cells[y, 1, y, 14].Merge = true;
                ws.Cells["A" + y].Value = "Year - " + string.Format("{0:yyyy}", ViewBag.DateTo);
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 10;
                y++;

                ws.Cells[y, 1, y, 14].Merge = true;
                y++;
                #endregion

                #region Table Headers
                ws.Cells["A" + y].Value = "Contractor";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Jan";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Feb";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Value = "Mar";
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = "Apr";
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F" + y].Value = "May";
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["G" + y].Value = "Jun";
                ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["H" + y].Value = "Jul";
                ws.Cells["H" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["I" + y].Value = "Aug";
                ws.Cells["I" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["J" + y].Value = "Sep";
                ws.Cells["J" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["K" + y].Value = "Oct";
                ws.Cells["K" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["L" + y].Value = "Nov";
                ws.Cells["L" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["M" + y].Value = "Dec";
                ws.Cells["M" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["N" + y].Value = "Total";
                ws.Cells["N" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 14].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 14].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 14].Style.Font.Size = 10;
                ws.Cells[y, 1, y, 14].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, 14].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, 14].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 14].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, 14].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[y, 1, y, 14].Style.WrapText = true;
                ws.Cells[y, 1, y, 14].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                ws.Row(y).Height = 33;
                y++;
                #endregion

                int i = 1;

                var conIds = data.Select(x => x.EmployeeID).Distinct();
                foreach (var conId in conIds)
                {
                    #region row

                    var rows = data.Where(x => x.EmployeeID == conId).ToList();
                    ws.Cells["A" + y].Value = rows[0].FirstName + " " + rows[0].LastName;
                    for (int m = 1; m <= 12; m++)
                    {
                        var mnthlySum = rows.Where(x => x.PaidDate.Value.Month == m).Sum(x => x.Total);
                        string c = "B";
                        switch (m)
                        {
                            case 1: c = "B"; break;
                            case 2: c = "C"; break;
                            case 3: c = "D"; break;
                            case 4: c = "E"; break;
                            case 5: c = "F"; break;
                            case 6: c = "G"; break;
                            case 7: c = "H"; break;
                            case 8: c = "I"; break;
                            case 9: c = "J"; break;
                            case 10: c = "K"; break;
                            case 11: c = "L"; break;
                            case 12: c = "M"; break;
                            default: break;
                        }
                        ws.Cells[c + y].Value = mnthlySum != 0 ? string.Format("{0:C}", mnthlySum) : "-";
                    }
                    ws.Cells["N" + y].Value = string.Format("{0:C}", rows.Sum(x => x.Total));

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[y, 2, y, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells[y, 1, y, 14].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                    ws.Cells[y, 1, y, 14].Style.WrapText = true;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["H" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["I" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["J" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["K" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["L" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["M" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["N" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 14].Style.Font.Size = 9;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 14].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 14].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    #endregion

                    y++;
                    i++;
                }

                #region Total row

                ws.Cells["A" + y].Value = "Monthly Total";
                for (int m = 1; m <= 12; m++)
                {
                    var mnthlySum = data.Where(x => x.PaidDate.Value.Month == m).Sum(x => x.Total);
                    string c = "B";
                    switch (m)
                    {
                        case 1: c = "B"; break;
                        case 2: c = "C"; break;
                        case 3: c = "D"; break;
                        case 4: c = "E"; break;
                        case 5: c = "F"; break;
                        case 6: c = "G"; break;
                        case 7: c = "H"; break;
                        case 8: c = "I"; break;
                        case 9: c = "J"; break;
                        case 10: c = "K"; break;
                        case 11: c = "L"; break;
                        case 12: c = "M"; break;
                        default: break;
                    }
                    ws.Cells[c + y].Value = mnthlySum != 0 ? string.Format("{0:C}", mnthlySum) : "-";
                }
                ws.Cells["N" + y].Value = string.Format("{0:C}", data.Sum(x => x.Total));

                #region Row Styles

                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 2, y, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws.Cells[y, 1, y, 14].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                ws.Cells[y, 1, y, 14].Style.WrapText = true;
                ws.Cells[y, 1, y, 14].Style.Font.Bold = true;

                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["H" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["I" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["J" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["K" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["L" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["M" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["N" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 14].Style.Font.Size = 9;
                if (i % 2 != 0)
                {
                    ws.Cells[y, 1, y, 14].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 1, y, 14].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                #endregion

                #endregion

                y++;
                i++;

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public List<YTDPaymentReport> YTDPaymentsData(int? periodStartYear, int? periodEndYear)
        {
            Period pEnd = null;
            Period pStart = null;
            DateTime? start = null;
            DateTime? end = null;
            DateTime? dateFrom = null;
            DateTime? dateTo = null;
            var periods = Customs.GetPeriods(periodStartYear.Value, periodEndYear.Value);
            if (periods.Any())
            {
                pEnd = periods.Where(x => x.PeriodYear == periodEndYear.Value).OrderByDescending(x => x.PeriodNo).FirstOrDefault();
                pStart = periods.FirstOrDefault(x => x.PeriodNo == 1 && x.PeriodYear == periodStartYear.Value);
                if (pEnd != null && pStart != null)
                {
                    dateFrom = pStart.DateFrom;
                    start = pStart.PayDate;
                    end = pEnd.PayDate;
                    dateTo = pEnd.DateTo;
                }
            }
            var data = new List<YTDPaymentReport>();
            if (start.HasValue && end.HasValue)
            {
                ViewBag.DateFrom = dateFrom;
                ViewBag.DateTo = dateTo;
                var statements = db.Set<ContractorsStatementYTD>().FromSqlRaw("CALL ContractorsStatementYTD('" + start.Value.ToString("yyyy-MM-dd") + "','" + end.Value.ToString("yyyy-MM-dd") + "')").ToList();
                data = statements.Join(db.ADM03300, x => x.ContractorID, y => y.ConID, (x, y) => new YTDPaymentReport
                {
                    EmployeeID = y.EmployeeID,
                    ConId = y.ConID,
                    FirstName = y.FirstName,
                    LastName = y.LastName,
                    PaidDate = x.PaidDate,
                    Total = x.PenalizeAmount
                }).ToList();
                data = data.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
            }
            return data;
        }

        [HttpPost]
        public ActionResult YTDPaymentsPeriodDetails(DateTime fromDate, DateTime toDate, int ConId)
        {
            var printData = YTDPaymentsPeriodDetailsData(fromDate, toDate, ConId);
            return View(printData);
        }
        [HttpPost]
        public void YTDPaymentsPeriodDetailsToExcel(YTDPaymentsPeriodDetailFilter model)
        {
            string docName = "YTDPaymentsPeriodDetails";
            ExcelPackage pck = new ExcelPackage();

            var data = YTDPaymentsPeriodDetailsData(model.fromDate, model.toDate, model.ConId);
            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("YTDPaymentsPeriodDetails");

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

                ws.Column(1).Width = 12;
                ws.Column(2).Width = 20;
                ws.Column(3).Width = 12;
                ws.Column(4).Width = 33;
                ws.Column(5).Width = 11;
                ws.Column(6).Width = 11;
                int y = 1;

                #region Title
                ws.Cells[y, 1, y, 6].Merge = true;
                ws.Cells["A" + y].Value = "Contractor Ernings Period Details";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;

                ws.Cells[y, 1, y, 6].Merge = true;
                ws.Cells["A" + y].Value = "Year - " + string.Format("{0:yyyy}", ViewBag.DateTo);
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 10;
                y++;

                ws.Cells[y, 1, y, 6].Merge = true;
                y++;
                #endregion

                #region Table Headers

                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Period";
                ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Referance";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Value = "Description";
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = "Amout";
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F" + y].Value = "Total";
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 6].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 6].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 6].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, 6].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[y, 1, y, 6].Style.WrapText = true;
                ws.Cells[y, 1, y, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                ws.Row(y).Height = 33;
                y++;
                #endregion

                int i = 1;

                var paidDates = data.Select(x => x.PaidDate).Distinct().ToList();
                foreach (var paidDate in paidDates)
                {
                    #region row
                    int r = 0;
                    var rows = data.Where(x => x.PaidDate == paidDate);
                    foreach (var row in rows)
                    {
                        ws.Cells["A" + y].Value = r == 0 ? row.PeriosName : "";
                        ws.Cells["B" + y].Value = r == 0 ? row.PeriodStart.ToShortDateString() + " - " + row.PeriodEnd.ToShortDateString() : "";
                        ws.Cells["C" + y].Value = row.Wo_ref;
                        ws.Cells["D" + y].Value = row.Wo_title;
                        ws.Cells["E" + y].Value = string.Format("{0:C}", row.Total);
                        ws.Cells["F" + y].Value = string.Format("{0:C}", row.PenalizeAmount);

                        #region Row Styles

                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[y, 5, y, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        ws.Cells[y, 1, y, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                        ws.Cells[y, 1, y, 6].Style.WrapText = true;

                        ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                        ws.Cells[y, 1, y, 6].Style.Font.Size = 11;
                        if (i % 2 != 0)
                        {
                            ws.Cells[y, 1, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[y, 1, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        }
                        #endregion

                        y++;
                        i++;
                    }

                    ws.Cells[y, 1, y, 4].Merge = true;
                    ws.Cells["A" + y].Value = "Period Total";

                    ws.Cells["E" + y].Value = string.Format("{0:C}", rows.Sum(x => x.Total));
                    ws.Cells["F" + y].Value = string.Format("{0:C}", rows.Sum(x => x.PenalizeAmount));

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[y, 5, y, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells[y, 1, y, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                    ws.Cells[y, 1, y, 6].Style.WrapText = true;
                    ws.Cells[y, 1, y, 6].Style.Font.Bold = true;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 6].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    #endregion

                    y++;
                    i++;
                }

                ws.Cells[y, 1, y, 4].Merge = true;
                ws.Cells["A" + y].Value = "Total";

                ws.Cells["E" + y].Value = string.Format("{0:C}", data.Sum(x => x.Total));
                ws.Cells["F" + y].Value = string.Format("{0:C}", data.Sum(x => x.PenalizeAmount));

                #region Row Styles

                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[y, 5, y, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws.Cells[y, 1, y, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                ws.Cells[y, 1, y, 6].Style.WrapText = true;
                ws.Cells[y, 1, y, 6].Style.Font.Bold = true;

                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 6].Style.Font.Size = 11;
                if (i % 2 != 0)
                {
                    ws.Cells[y, 1, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 1, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                #endregion

                y++;
                i++;

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public List<YTDPeriodDetails> YTDPaymentsPeriodDetailsData(DateTime fromDate, DateTime toDate, int ConId)
        {
            ViewBag.DateFrom = fromDate;
            ViewBag.DateTo = toDate;
            ViewBag.ConId = ConId;

            var data = db.Set<YTDPeriodDetails>().FromSqlRaw("CALL ContractorsStatementYTDPeriodDetails(" + toDate.Year + "," + ConId + ")").ToList();
            var printData = data.Select(x => new YTDPeriodDetails
            {
                ContractorID = x.ContractorID,
                Workid = x.Workid,
                Wo_ref = x.Wo_ref,
                Wo_title = x.Wo_title,
                PaidDate = x.PaidDate,
                PeriosName = x.PeriosName,
                PeriodStart = x.PeriodStart,
                PeriodEnd = x.PeriodEnd,
                Total = x.Total,
                PenalizeAmount = x.PenalizeAmount
            }).ToList();
            return printData;
        }

        #endregion

        #region Work Order Split

        //[TeliconAuthorize(TaskId = 23)]
        public ActionResult WorkOrderSplit()
        {
            ViewBag.Companies = db.ADM03200.Select(x => new SelectListItem
            {
                Text = x.CustName,
                Value = x.CustID.ToString()
            }).ToList();
            return View();
        }
        [HttpPost]
        public ActionResult GenerateWorkOrderSplit(WorkOrderSplitFilter model)
        {
            return PartialView("Partials/WorkOrderSplitPartial", WorkOrderSplitData(model));
        }
        [HttpPost]
        public void WorkOrderSplitToExcel(WorkOrderSplitFilter model)
        {
            string docName = "WorkOrderSplit";
            ExcelPackage pck = new ExcelPackage();

            var data = WorkOrderSplitData(model);

            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("ContractorStatements");
                int y = 1;
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

                ws.Column(1).Width = 12;
                ws.Column(2).Width = 45;
                ws.Column(3).Width = 7.5;
                ws.Column(4).Width = 8;
                ws.Column(5).Width = 17;
                ws.Column(6).Width = 8;

                #region Title
                ws.Cells[y, 1, y, 6].Merge = true;
                ws.Cells["A" + y].Value = "Work Order Split";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;

                ws.Cells[y, 1, y, 6].Merge = true;
                y++;

                #endregion

                #region Table Headers
                ws.Cells["A" + y].Value = "Ref. No.";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Title";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Mgmt.";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Value = "Contr.";
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 5, y, 6].Merge = true;
                ws.Cells["E" + y].Value = "Team";
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 6].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 6].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 6].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, 6].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(y).Height = 20;
                y++;

                #endregion
                int r = 0;
                foreach (var row in data)
                {
                    int count = row.team.Count;
                    if (count > 0)
                    {
                        #region Rows
                        ws.Cells[y, 1, y + count - 1, 1].Merge = true;
                        ws.Cells["A" + y].Value = row.referenceCode;
                        ws.Cells[y, 2, y + count - 1, 2].Merge = true;
                        ws.Cells["B" + y].Value = row.name;
                        ws.Cells[y, 3, y + count - 1, 3].Merge = true;
                        ws.Cells["C" + y].Value = row.managementSplit;
                        ws.Cells[y, 4, y + count - 1, 4].Merge = true;
                        ws.Cells["D" + y].Value = row.contractorSplit;
                    }

                    if (row.team.Count > 0)
                        foreach (var member in row.team)
                        {
                            ws.Cells["E" + y].Value = member.fullName + " :";
                            ws.Cells["F" + y].Value = string.Format("{0:C}", member.split);
                            y++;
                        }
                    else
                        y++;

                    #region Row Styles
                    //ws.Cells[y, 4, y, 6].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                    ws.Cells["A" + (y - count)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["A" + (y - count)].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    ws.Cells["B" + (y - count)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["B" + (y - count)].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    ws.Cells["B" + (y - count)].Style.WrapText = true;
                    ws.Cells["C" + (y - count)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["C" + (y - count)].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    ws.Cells["D" + (y - count)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["D" + (y - count)].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    for (int i = y - count; i < y; i++)
                    {
                        ws.Cells["E" + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells["F" + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }

                    if (count > 0)
                    {
                        ws.Cells[y - count, 1, y - 1, 1].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells[y - count, 2, y - 1, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells[y - count, 3, y - 1, 3].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells[y - count, 4, y - 1, 4].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells[y - count, 5, y - 1, 6].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                        ws.Cells[y - count, 1, y - 1, 6].Style.Font.Size = 11;
                        if (r % 2 != 0)
                        {
                            ws.Cells[y - count, 1, y - 1, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[y - count, 1, y - 1, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        }
                    }

                    #endregion

                    #endregion
                    //y++;
                    r++;
                }

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public List<WorkOrderSplitData> WorkOrderSplitData(WorkOrderSplitFilter model)
        {
            List<Expression<Func<TRN23100, bool>>> expressions = new List<Expression<Func<TRN23100, bool>>>();
            if (model.dateFrom.HasValue)
            {
                Expression<Func<TRN23100, bool>> dateFromExp = p => p.Requestdt >= model.dateFrom.Value;
                expressions.Add(dateFromExp);
            }
            if (model.dateTo.HasValue)
            {
                Expression<Func<TRN23100, bool>> dateToExp = p => p.Requestdt <= model.dateTo.Value;
                expressions.Add(dateToExp);
            }
            if (model.contractorSplitFrom.HasValue)
            {
                Expression<Func<TRN23100, bool>> conFromExp = p => p.Wo_split2 >= model.contractorSplitFrom.Value;
                expressions.Add(conFromExp);
            }
            if (model.contractorSplitTo.HasValue)
            {
                Expression<Func<TRN23100, bool>> conToExp = p => p.Wo_split2 <= model.contractorSplitTo;
                expressions.Add(conToExp);
            }
            if (model.managementSplitFrom.HasValue)
            {
                Expression<Func<TRN23100, bool>> manFromExp = p => p.Wo_split >= model.managementSplitFrom.Value;
                expressions.Add(manFromExp);
            }
            if (model.managementSplitTo.HasValue)
            {
                Expression<Func<TRN23100, bool>> manToExp = p => p.Wo_split <= model.managementSplitTo.Value;
                expressions.Add(manToExp);
            }
            var data = new List<WorkOrderSplitData>();
            if (expressions.Count > 0)
            {
                Expression<Func<TRN23100, bool>> mainExp = p => p.ADM03200.CustID == model.companyId;
                foreach (var exp in expressions)
                {
                    mainExp = mainExp.And(exp);
                }
                data = db.TRN23100.Include(x => x.ADM03400).ThenInclude(y => y.ADM03300).Where(mainExp.Expand()).Select(x => new WorkOrderSplitData
                {
                    contractorSplit = x.Wo_split2,
                    managementSplit = x.Wo_split,
                    name = x.Wo_title,
                    referenceCode = x.Wo_ref,
                    team = x.ADM03400.Select(y => new WorkOrderSplitTeamData
                    {
                        fullName = y.ADM03300.FirstName + " " + y.ADM03300.LastName,
                        split = y.ContractorRate
                    }).ToList()
                }).ToList();
            }
            else
            {
                data = db.TRN23100.Include(x => x.ADM03400).ThenInclude(y => y.ADM03300).Where(x => x.Wo_client == model.companyId).Select(x => new WorkOrderSplitData
                {
                    contractorSplit = x.Wo_split2,
                    managementSplit = x.Wo_split,
                    name = x.Wo_title,
                    referenceCode = x.Wo_ref,
                    team = x.ADM03400.Select(y => new WorkOrderSplitTeamData
                    {
                        fullName = y.ADM03300.FirstName + " " + y.ADM03300.LastName,
                        split = y.ContractorRate
                    }).ToList()
                }).ToList();
            }
            return data;
        }
        #endregion

        #region Payment Detail Report
        //[TeliconAuthorize(TaskId = 24)]
        public ActionResult PaymentDetails()
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

        [HttpPost]
        public ActionResult GeneratePaymentDetailStatements(string sortBy, string direction, DateTime? dateFrom, DateTime? dateTo)
        {
            var data = PaymentDetailStatementsData(sortBy, direction, dateFrom, dateTo);
            return PartialView("Partials/PaymentDetailStatementsPartial", data);
        }

        [HttpPost]
        public void PaymentDetailsToExcel(PaymentDetailFilter model)
        {
            string docName = "PaymentDetails";
            ExcelPackage pck = new ExcelPackage();

            var data = PaymentDetailStatementsData(model.sortBy, model.direction, model.dateFrom, model.dateTo);

            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("PaymentDetails");

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

                ws.Column(1).Width = 5.5;
                ws.Column(2).Width = 13.5;
                ws.Column(3).Width = 33.5;
                ws.Column(4).Width = 12;
                ws.Column(5).Width = 7;
                ws.Column(6).Width = 9.5;
                ws.Column(7).Width = 9.5;
                ws.Column(8).Width = 8.5;
                int y = 1;
                #region Title
                ws.Cells[y, 1, y, 8].Merge = true;
                ws.Cells["A" + y].Value = "Payment Detail";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;

                ws.Cells[y, 1, y, 8].Merge = true;
                ws.Cells["A" + y].Value = "Period Starting " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateFrom) + " to " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateTo);
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 10;
                y++;

                ws.Cells[y, 1, y, 8].Merge = true;
                y++;
                #endregion

                #region Table Headers
                ws.Cells["A" + y].Value = "S.No";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Ref #";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Title";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Value = "Dispatched";
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = "Status";
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F" + y].Value = "Submited";
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["G" + y].Value = "Paid Date";
                ws.Cells["G" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["H" + y].Value = "Leader";
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
                    ws.Cells["B" + y].Value = row.Wo_ref;
                    ws.Cells["C" + y].Value = row.Wo_title;
                    ws.Cells["D" + y].Value = row.Dispatchdt;
                    ws.Cells["E" + y].Value = row.Status;
                    ws.Cells["F" + y].Value = row.DateSubmitted;
                    ws.Cells["G" + y].Value = row.PaidDate;
                    ws.Cells["H" + y].Value = row.LastName;

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells[y, 4, y, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
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

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public List<PaymentDetailReport> PaymentDetailStatementsData(string sortBy, string direction, DateTime? dateFrom, DateTime? dateTo)
        {
            DateTime? start = null;
            DateTime? end = null;
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                start = dateFrom.Value;
                end = dateTo.Value;
            }
            var data = new List<PaymentDetailReport>();
            if (start.HasValue && end.HasValue)
            {
                ViewBag.DateFrom = dateFrom;
                ViewBag.DateTo = dateTo;
                var statements = db.Set<PaymentDetailStatement>().FromSqlRaw("CALL PaymentDetailStatement('" + start.Value.ToString("yyyy-MM-dd") + "','" + end.Value.ToString("yyyy-MM-dd") + "')").ToList();
                data = statements.Select(x => new PaymentDetailReport
                {
                    Num = !string.IsNullOrEmpty(Convert.ToString(x.RN)) ? Convert.ToString(x.RN) : "-",
                    WorkID = !string.IsNullOrEmpty(Convert.ToString(x.Workid)) ? Convert.ToString(x.Workid) : "-",
                    Wo_ref = !string.IsNullOrEmpty(Convert.ToString(x.Wo_ref)) ? Convert.ToString(x.Wo_ref) : "-",
                    Wo_title = !string.IsNullOrEmpty(Convert.ToString(x.Wo_title)) ? Convert.ToString(x.Wo_title) : "-",
                    Dispatchdt = !string.IsNullOrEmpty(Convert.ToString(x.Dispatchdt)) ? Convert.ToString(string.Format("{0:MMM dd}", x.Dispatchdt)) : "-",
                    Status = !string.IsNullOrEmpty(Convert.ToString(x.Status)) ? Convert.ToString(x.Status) : "-",
                    DateSubmitted = !string.IsNullOrEmpty(Convert.ToString(x.DateSubmitted)) ? Convert.ToString(string.Format("{0:MMM dd}", x.DateSubmitted)) : "-",
                    PaidDate = !string.IsNullOrEmpty(Convert.ToString(x.PaidDate)) ? Convert.ToString(string.Format("{0:MMM dd}", x.PaidDate)) : "-",
                    DateVerified = !string.IsNullOrEmpty(Convert.ToString(x.dateVerified)) ? Convert.ToString(string.Format("{0:MMM dd}", x.dateVerified)) : "-",
                    ConId = !string.IsNullOrEmpty(Convert.ToString(x.ConID)) ? Convert.ToString(x.ConID) : "-",
                    LastName = !string.IsNullOrEmpty(Convert.ToString(x.Name)) ? Convert.ToString(x.Name) : "-"
                }).ToList();
            }
            return data;
        }

        #endregion

        #region Invoice by Category Report
        //[TeliconAuthorize(TaskId = 25)]
        public ActionResult CategoryInvoice()
        {
            var CategorySelection = new List<SelectListItem>();
            var data = db.ADM03500.OrderBy(x => x.ClassId).ToList();
            CategorySelection.Add(new SelectListItem { Text = "All", Value = "0" });
            foreach (var item in data)
            {
                CategorySelection.Add(new SelectListItem { Text = item.ClassName, Value = item.ClassId.ToString() });
            }
            ViewBag.CategorySelection = CategorySelection;
            return View();
        }

        [HttpPost]
        public ActionResult GenerateCategoryInvoiceStatements(string sortBy, string direction, DateTime? dateFrom, DateTime? dateTo, string CategorySelection)
        {
            var data = CategoryInvoiceStatementsData(sortBy, direction, dateFrom, dateTo, CategorySelection);
            return PartialView("Partials/CategoryInvoiceStatementsPartial", data);
        }

        [HttpPost]
        public ActionResult CategoryInvoiceStatementInvoiceSummary(DateTime fromDate, DateTime toDate, int classId)
        {
            var printData = CategoryInvoiceStatementInvoiceSummaryData(fromDate, toDate, classId);
            var summary = new SummaryPrintOutWithClient
            {
                PrintData = printData
            };
            return View(summary);
        }

        [HttpPost]
        public void CategoryInvoiceStatementsToExcel(CategoryInvoiceStatmentFilter model)
        {
            string docName = "CategoryInvoiceStatements";
            ExcelPackage pck = new ExcelPackage();

            var data = CategoryInvoiceStatementsData(model.sortBy, model.direction, model.dateFrom, model.dateTo, model.CategorySelection);

            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("CategoryInvoiceStatements");

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

                ws.Column(1).Width = 19;
                ws.Column(2).Width = 52;
                ws.Column(3).Width = 27;
                int y = 1;

                #region Title
                ws.Cells[y, 1, y, 3].Merge = true;
                ws.Cells["A" + y].Value = "Invoice By Category";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;

                ws.Cells[y, 1, y, 3].Merge = true;
                ws.Cells["A" + y].Value = "Period Starting " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateFrom) + " to " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateTo);
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 10;
                y++;

                ws.Cells[y, 1, y, 3].Merge = true;
                y++;
                #endregion

                #region Table Headers
                ws.Cells["A" + y].Value = "S.No.";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "CATEGORY";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "TOTAL";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 3].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 3].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 3].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, 3].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(y).Height = 20;
                y++;
                #endregion

                int i = 1;
                foreach (var row in data)
                {
                    #region Rows
                    ws.Cells["A" + y].Value = i;
                    ws.Cells["B" + y].Value = row.Category;
                    ws.Cells["C" + y].Value = string.Format("{0:C}", row.Total);

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells[y, 1, y, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ws.Cells[y, 1, y, 3].Style.WrapText = true;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 3].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    #endregion
                    y++;
                    i++;
                }

                var total = string.Format("{0:C}", data.Sum(x => x.Total));

                #region Total
                ws.Cells[y, 1, y, 2].Merge = true;
                ws.Cells["A" + y].Value = "Grand Total:";
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[y, 1, y, 2].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = string.Format("{0:C}", total);
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 1, y, 3].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 3].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 3].Style.Font.Color.SetColor(Color.FromArgb(165, 0, 33));
                ws.Cells[y, 1, y, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                if (i % 2 != 0)
                {
                    ws.Cells[y, 1, y, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 1, y, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                y++;
                i++;
                #endregion

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        [HttpPost]
        public void CategoryInvoiceStatementInvoiceSummaryToExcel(CategorInvoiceStatmentInvoiceSummaryFilter model)
        {
            string docName = "CategoryInvoiceStatementInvoiceSummary";
            ExcelPackage pck = new ExcelPackage();

            var data = CategoryInvoiceStatementInvoiceSummaryData(model.fromDate, model.toDate, model.classId);
            string className = ViewBag.ClassName;
            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("CategoryInvoiceStatementInvoiceSummary");

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

                ws.Column(1).Width = 12;
                ws.Column(2).Width = 35;
                ws.Column(3).Width = 15;
                ws.Column(4).Width = 11;
                ws.Column(5).Width = 11;
                ws.Column(6).Width = 13;
                int y = 1;

                #region Title
                ws.Cells[y, 1, y, 6].Merge = true;
                ws.Cells["A" + y].Value = "Invoice Summary - " + className;
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;

                ws.Cells[y, 1, y, 6].Merge = true;
                ws.Cells["A" + y].Value = "Period Starting " + string.Format("{0:dddd MMMM dd, yyyy}", model.fromDate) + " to " + string.Format("{0:dddd MMMM dd, yyyy}", model.toDate);
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 10;
                y++;

                ws.Cells[y, 1, y, 6].Merge = true;
                y++;
                #endregion

                #region Table Headers
                ws.Cells["A" + y].Value = "Invoice #";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Description";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Technicians";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Value = "Subtotal";
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = "GCT";
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F" + y].Value = "Total";
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 6].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 6].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 6].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, 6].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(y).Height = 20;
                y++;
                #endregion

                int i = 1;
                foreach (var row in data)
                {
                    #region Rows
                    ws.Cells["A" + y].Value = row.InvoiceID;
                    ws.Cells["B" + y].Value = row.Title;
                    ws.Cells["C" + y].Value = row.Contractors;
                    ws.Cells["D" + y].Value = string.Format("{0:C}", row.Total);
                    ws.Cells["E" + y].Value = string.Format("{0:C}", row.GCT);
                    ws.Cells["F" + y].Value = string.Format("{0:C}", row.Total + row.GCT);

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[y, 4, y, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells[y, 1, y, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ws.Cells[y, 1, y, 6].Style.WrapText = true;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 6].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    #endregion
                    y++;
                    i++;
                }

                var total = string.Format("{0:C}", data.Sum(x => x.Total) + data.Sum(x => x.GCT));

                #region Total
                ws.Cells[y, 1, y, 5].Merge = true;
                ws.Cells["A" + y].Value = "Total:";
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[y, 1, y, 5].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F" + y].Value = string.Format("{0:C}", total);
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells[y, 1, y, 6].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 6].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 6].Style.Font.Color.SetColor(Color.FromArgb(165, 0, 33));
                ws.Cells[y, 1, y, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                if (i % 2 != 0)
                {
                    ws.Cells[y, 1, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[y, 1, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                }
                y++;
                i++;
                #endregion

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public List<CategoryInvoiceReport> CategoryInvoiceStatementsData(string sortBy, string direction, DateTime? dateFrom, DateTime? dateTo, string CategorySelection)
        {
            DateTime? start = null;
            DateTime? end = null;
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                start = dateFrom.Value;
                end = dateTo.Value;
            }
            var data = new List<CategoryInvoiceReport>();
            if (start.HasValue && end.HasValue)
            {
                ViewBag.DateFrom = dateFrom;
                ViewBag.DateTo = dateTo;

                var statements = db.Set<CategoryInvoiceStatement>().FromSqlRaw("CALL CategoryInvoiceStatement('" + start.Value.ToString("yyyy-MM-dd") + "','" + end.Value.ToString("yyyy-MM-dd") + "')").ToList();
                data = statements.Select(x => new CategoryInvoiceReport
                {
                    Total = !string.IsNullOrEmpty(Convert.ToString(x.TotalAmount.Value)) ? Convert.ToDecimal(x.TotalAmount.Value) : 0,
                    Category = !string.IsNullOrEmpty(Convert.ToString(x.Category)) ? Convert.ToString(x.Category) : "-",
                    CategoryID = x.ClassID
                }).ToList();
            }
            return data;
        }

        public List<SummaryPrintOut> CategoryInvoiceStatementInvoiceSummaryData(DateTime fromDate, DateTime toDate, int classId)
        {
            var classStr = db.ADM03500.FirstOrDefault(x => x.ClassId == classId).ClassName;
            ViewBag.ClassName = classStr;
            ViewBag.ClassId = classId;
            ViewBag.DateFrom = fromDate;
            ViewBag.DateTo = toDate;
            var data = db.Set<CategoryInvoiceStatementInvoiceSummary>().FromSqlRaw("CALL CategoryInvoiceStatementInvoiceSummary('" + fromDate.ToString("yyyy-MM-dd") + "','" + toDate.ToString("yyyy-MM-dd") + "'," + classId + ")").ToList();
            var printData = data.Select(x => new SummaryPrintOut
            {
                InvoiceID = Customs.MakeGenericInvoiceNo(x.InvoiceNum),
                GCT = Convert.ToDecimal(x.GCT.Value),
                RefNo = x.Wo_ref,
                Title = x.InvoiceTitle,
                IsInCategory = true,
                Total = x.Total.Value,
                Contractors = new List<string>() { x.Contractor }
            }).ToList();
            return printData;
        }

        #endregion

        #region Dispatched WO Report
        //[TeliconAuthorize(TaskId = 26)]
        public ActionResult DispatchedWO()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GenerateDispatchedWOStatements(string sortBy, string direction, DateTime? dateFrom, DateTime? dateTo)
        {
            var data = DispatchedWOStatementsData(sortBy, direction, dateFrom, dateTo);
            return PartialView("Partials/DispetchWOPartial", data);
        }
        [HttpPost]
        public void DispatchedWOStatementsToExcel(DispatchedWOStatmentFilter model)
        {
            string docName = "DispatchedWOStatements";
            ExcelPackage pck = new ExcelPackage();

            var data = DispatchedWOStatementsData(model.sortBy, model.direction, model.dateFrom, model.dateTo);
            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("DispatchedWOStatements");

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

                ws.Column(1).Width = 7;
                ws.Column(2).Width = 16;
                ws.Column(3).Width = 43;
                ws.Column(4).Width = 11;
                ws.Column(5).Width = 11;
                ws.Column(6).Width = 11;
                int y = 1;

                #region Title
                ws.Cells[y, 1, y, 6].Merge = true;
                ws.Cells["A" + y].Value = "Dispatched Work Orders";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;

                ws.Cells[y, 1, y, 6].Merge = true;
                ws.Cells["A" + y].Value = "Period Starting " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateFrom) + " to " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateTo);
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 10;
                y++;

                ws.Cells[y, 1, y, 6].Merge = true;
                y++;
                #endregion

                #region Table Headers
                ws.Cells["A" + y].Value = "S.No.";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Wo Reference";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Title";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Value = "Dispatch Date";
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = "Lead";
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["F" + y].Value = "Splice Docs";
                ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 6].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 6].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 6].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, 6].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells[y, 1, y, 6].Style.WrapText = true;
                ws.Cells[y, 1, y, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                ws.Row(y).Height = 33;
                y++;
                #endregion

                int i = 1;
                foreach (var row in data)
                {
                    #region Rows
                    ws.Cells["A" + y].Value = i;
                    ws.Cells["B" + y].Value = row.Wo_ref;
                    ws.Cells["C" + y].Value = row.Wo_title;
                    ws.Cells["D" + y].Value = row.Dispatchdt;
                    ws.Cells["E" + y].Value = row.LastName;
                    ws.Cells["F" + y].Value = row.SpliceDocs;

                    #region Row Styles

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells[y, 4, y, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[y, 1, y, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                    ws.Cells[y, 1, y, 6].Style.WrapText = true;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["F" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 6].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    #endregion
                    y++;
                    i++;
                }

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public List<DispatchWOModel> DispatchedWOStatementsData(string sortBy, string direction, DateTime? dateFrom, DateTime? dateTo)
        {
            DateTime? start = null;
            DateTime? end = null;
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                start = dateFrom.Value;
                end = dateTo.Value;
            }
            var data = new List<DispatchWOModel>();
            if (start.HasValue && end.HasValue)
            {
                ViewBag.DateFrom = dateFrom;
                ViewBag.DateTo = dateTo;
                var statements = db.Set<DispatchWOStatementReport>().FromSqlRaw("CALL DispatchWOStatementReport('" + start.Value.ToString("yyyy-MM-dd") + "','" + end.Value.ToString("yyyy-MM-dd") + "')").ToList();
                data = statements.Select(x => new DispatchWOModel
                {
                    Wo_ref = !string.IsNullOrEmpty(Convert.ToString(x.Wo_ref)) ? Convert.ToString(x.Wo_ref) : "-",
                    Wo_title = !string.IsNullOrEmpty(Convert.ToString(x.Wo_title)) ? Convert.ToString(x.Wo_title) : "-",
                    Dispatchdt = !string.IsNullOrEmpty(Convert.ToString(x.Dispatchdt)) ? Convert.ToString(string.Format("{0:MMM dd}", x.Dispatchdt)) : "-",
                    SpliceDocs = !string.IsNullOrEmpty(Convert.ToString(x.SpliceDocs)) ? Convert.ToString(x.SpliceDocs).ToLower() == "false" ? "" : Convert.ToString(x.SpliceDocs) : "-",
                    LastName = !string.IsNullOrEmpty(Convert.ToString(x.Name)) ? Convert.ToString(x.Name) : "-"
                }).ToList();
            }
            return data;
        }

        #endregion

        //[Authorize(Roles = "AppAdmin, SuperAdmin, Admin, Supervisor, Technician")]
        public ActionResult PeriodDetail(ActivitySummaryFilter model)
        {
            int year = model.year;
            int period = model.period ?? 0;
            var p = Customs.GetPeriods(year, year).FirstOrDefault(x => x.PeriodNo == period);
            ViewBag.Period = p;
            var gRecs = db.Set<TechnicianStatementDetail>().FromSqlRaw("CALL TechnicianStatementDetail(" + model.conId + ",'" + p.PayDate.ToString("yyyy-MM-dd") + ",'" + p.PayDate.ToString("yyyy-MM-dd") + "')").GroupBy(x => x.ActivityID).ToList();
            List<PeriodDetail> dbData = new List<PeriodDetail>();
            foreach (var rec in gRecs)
            {
                PeriodDetail m = new PeriodDetail();
                decimal Qty = 0;
                decimal Amt = 0;
                foreach (var recAct in rec)
                {
                    Qty = Qty + recAct.OActQty;
                    Amt = Amt + (recAct.OActQty * Convert.ToDecimal(recAct.Cost));
                    m.Description = recAct.RateDescr;
                    m.Unit = recAct.RateUnit;
                    m.Rate = recAct.Cost;
                }
                m.Quantity = Qty;
                m.Total = Convert.ToDouble(Amt);
                dbData.Add(m);
            }
            IEnumerable<PeriodDetail> data = dbData.OrderBy(x => x.Description).AsEnumerable();
            return View(data);
        }

        #region ContractorPaymentsBankFile

        //[TeliconAuthorize(TaskId = 28)]
        public ActionResult ContractorPaymentsBankFile()
        {
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

        [HttpPost]
        public void GenerateBankFileCSV(TempR model)
        {
            Period pStart = null;
            DateTime? start = null;
            DateTime? end = null;
            var periods = Customs.GetPeriods(model.periodStartYear, model.periodStartYear);
            if (periods.Any())
            {
                pStart = periods.FirstOrDefault(x => x.PeriodNo == model.periodStart && x.PeriodYear == model.periodStartYear);
                if (pStart != null)
                {
                    start = pStart.PayDate;
                    end = pStart.PayDate;
                }
            }

            var data = new List<ContractorBankPayments>();
            if (start.HasValue && end.HasValue)
            {
                var statements = db.Set<ContractorBankPaymentsReport>().FromSqlRaw("CALL ContractorBankPayments('" + start.Value.ToString("yyyy-MM-dd") + "','" + end.Value.ToString("yyyy-MM-dd") + "')").ToList();
                data = statements.Join(db.ADM03300.Include(z => z.ADM02200).ThenInclude(y => y.ADM02100), x => x.ContractorID, y => y.ConID, (x, y) => new ContractorBankPayments
                {
                    Amount = x.PenalizeTotal.Value,
                    Currency = "JMD",
                    BankId = y.ADM02200 != null ? y.ADM02200.BankId : "",
                    BankBranchCode = y.ADM02200 != null ? y.ADM02200.BranchId : "",
                    AccountNumber = y.Bankacc,
                    Name = y.FirstName + " " + y.LastName,
                    Address = y.Street,
                    City = y.City,
                    Country = "Jamaica",
                    BankName = y.ADM02200 != null ? y.ADM02200.ADM02100.BankName : ""
                }).ToList();
            }
            if (data != null && data.Count > 0)
            {
                StringWriter sw = new StringWriter();

                sw.WriteLine("\"H\",\"" + start.Value.ToString("yyyyMMdd") + "\",\"990751027606\",\"JMD\",\"" + data.Count + "\",\"" + string.Format("{0:0.00}", data.Sum(x => Math.Truncate(x.Amount * 100) / 100)) + "\", Telicon" + start.Value.ToString("yyyyMMdd") + "");

                Response.Clear();
                Response.Headers.Add("content-disposition", "attachment; filename=Telicon Payroll Global Access.csv");
                Response.ContentType = "text/csv";

                int n = 1;
                var FGBData = data.FindAll(x => x.BankId == "99");
                foreach (var line in FGBData)
                {
                    sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2:0.00}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\"",
                                               "D",
                                               n,
                                               Math.Truncate(line.Amount * 100) / 100,
                                               "JMD",
                                               line.BankBranchCode,
                                               line.AccountNumber,
                                               line.Name,
                                               line.Address,
                                               line.City,
                                               line.Country,
                                               "",
                                               "",
                                               "",
                                               "",
                                               "",
                                               "TELiCON",
                                               "",
                                               "",
                                               "Flow Contractor Payment",
                                               "",
                                               "ITG"));
                    n++;
                }
                var NonFGBData = data.FindAll(x => !x.BankName.ToUpper().Contains("FIRST GLOBAL"));
                foreach (var line in NonFGBData)
                {
                    sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2:0.00}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\"",
                                               "D",
                                               n,
                                               Math.Truncate(line.Amount * 100) / 100,
                                               "JMD",
                                               line.BankBranchCode,
                                               line.AccountNumber,
                                               line.Name,
                                               line.Address,
                                               line.City,
                                               line.Country,
                                               "",
                                               "",
                                               "",
                                               "",
                                               "",
                                               "TELiCON",
                                               "",
                                               "",
                                               "Flow Contractor Payment",
                                               "",
                                               "ACH"));
                    n++;
                }

                Response.StatusCode = StatusCodes.Status200OK;
                Response.WriteAsync(sw.ToString()).Wait();
            }

            else
            {
                StringWriter sw = new StringWriter();

                sw.WriteLine("\"H\",\"" + start.Value.ToString("yyyyMMdd") + "\",\"990751027606\",\"JMD\",\"" + data.Count + "\",\"" + string.Format("{0:0.00}", data.Sum(x => Math.Truncate(x.Amount * 100) / 100)) + "\", Telicon" + start.Value.ToString("yyyyMMdd") + "");

                Response.Clear();
                Response.Headers.Add("content-disposition", "attachment; filename=Telicon Payroll Global Access.csv");
                Response.ContentType = "text/csv";

                Response.StatusCode = StatusCodes.Status200OK;
                Response.WriteAsync(sw.ToString()).Wait();
            }
        }

        #endregion

        #region Contractor Pay Slip

        //[TeliconAuthorize(TaskId = 29)]
        public ActionResult ContractorPaySlip()
        {
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
        public FileResult GenerateConractorPaySlip(TempR model)
        {
            int year = model.periodStartYear;
            int period = model.periodStart;
            var p = Customs.GetPeriods(year, year).FirstOrDefault(x => x.PeriodNo == period);

            var statements = db.Set<ContractorsStatement>().FromSqlRaw("CALL ContractorsStatement('" + p.PayDate.ToString("yyyy-MM-dd") + "','" + p.PayDate.ToString("yyyy-MM-dd") + "')").ToList();
            var TRecords = statements.Join(db.ADM03300.Include(z => z.ADM04200), x => x.ContractorID, y => y.ConID, (x, y) => new ContractorPaySlipReport
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

            TRecords = TRecords.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("ContractorPaySlips_" + p.PayDate.ToString("yyyyMMdd") + ".pdf");

            Document doc = new Document();
            doc.SetPageSize(PageSize.A4);
            doc.SetMargins(0f, 0f, 0f, 0f);
            PdfWriter writer = PdfWriter.GetInstance(doc, workStream);
            writer.CloseStream = false;

            doc.Open();
            PdfContentByte cb = writer.DirectContent;
            if (TRecords != null && TRecords.Count > 0)
            {
                List<int> conIds = TRecords.Select(x => x.ConId).ToList();
                int c = 2;
                int i = 0;
                foreach (var conId in conIds)
                {
                    var rows = TRecords.FindAll(x => x.ConId == conId);
                    var deductions = db.TRN04100.Include(z => z.ADM04100).Where(x => x.ConductorID == conId && x.StartDate <= p.PayDate && x.EndDate >= p.PayDate).ToList();
                    var deductData = deductions.Select(x => new ContractorDeductionReport
                    {
                        DeductionName = x.ADM04100.Name,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate ?? new DateTime(),
                        Amount = x.Amount
                    }).ToList();

                    var path = Path.Combine(_env.WebRootPath, "Images", "print-logo.png");
                    var img = iTextSharp.text.Image.GetInstance(new Uri(path));
                    img.ScaleAbsolute(104, 56);
                    img.SetAbsolutePosition(11, ((c * (doc.PageSize.Height / 3)) + 5) + (doc.PageSize.Height / 3) - 70);
                    doc.Add(img);

                    //doc.Add(new Paragraph(new Chunk("PAY SLIP", FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    cb.Rectangle(10f, (c * (doc.PageSize.Height / 3)) + 5, doc.PageSize.Width - 20, (doc.PageSize.Height / 3) - 10);
                    cb.Stroke();
                    doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 8f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    //doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 2f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    PdfPTable HeaderTableLayout = new PdfPTable(6);
                    HeaderTableLayout.HorizontalAlignment = Element.ALIGN_RIGHT;
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
            //}
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

            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.HELVETICA, 8, 1, iTextSharp.text.BaseColor.Yellow))) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(128, 0, 0) });
        }

        // Method to add single cell to the body
        private static void AddCellToBody(PdfPTable tableLayout, string cellText)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.HELVETICA, 8, 1, iTextSharp.text.BaseColor.Black))) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255) });
        }

        #endregion


        #endregion

        #region PO Tracking

        //[TeliconAuthorize(TaskId = 30)]
        public ActionResult POTracking()
        {
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
        [HttpPost]
        public ActionResult GeneratePOTracking(POTrackingFilter model)
        {
            return PartialView("Partials/POTrackingPartial", POTrackingData(model));
        }
        [HttpPost]
        public void POTrackingToExcel(POTrackingFilter model)
        {
            string docName = "POTracking";
            ExcelPackage pck = new ExcelPackage();

            var data = POTrackingData(model);

            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("POTracking");

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

                ws.Column(1).Width = 13;
                ws.Column(2).Width = 38;
                ws.Column(3).Width = 13;
                ws.Column(4).Width = 20;
                ws.Column(5).Width = 13;
                int y = 1;

                #region Title
                ws.Cells[y, 1, y, 5].Merge = true;
                ws.Cells["A" + y].Value = "Purches Order Tracking Report";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;
                if (ViewBag.DateFrom != null && ViewBag.DateTo != null)
                {
                    ws.Cells[y, 1, y, 5].Merge = true;
                    ws.Cells["A" + y].Value = "Period Starting " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateFrom) + " to " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateTo);
                    ws.Cells["A" + y].Style.Font.Bold = true;
                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["A" + y].Style.Font.Name = "Arial";
                    ws.Cells["A" + y].Style.Font.Size = 10;
                    y++;
                }
                ws.Cells[y, 1, y, 5].Merge = true;
                y++;
                #endregion

                #region Table Headers
                ws.Cells["A" + y].Value = "Ref. No.";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Title";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "RequestDt";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Value = "Category";
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["E" + y].Value = "Amount";
                ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                ws.Cells[y, 1, y, 5].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                ws.Cells[y, 1, y, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, 5].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(y).Height = 20;
                #endregion
                y++;

                int i = 1;
                foreach (var po in data.Select(x => x.PONum).Distinct())
                {
                    ws.Cells[y, 1, y, 5].Merge = true;
                    ws.Cells["A" + y].Value = "PO Number : " + po;

                    #region Row Styles
                    ws.Cells[y, 1, y, 5].Style.Font.Color.SetColor(Color.FromArgb(21, 98, 175));
                    ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                    ws.Row(y).Height = 20;
                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells["A" + y].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion
                    i++;
                    y++;

                    foreach (var row in data.Where(x => x.PONum == po))
                    {
                        #region Rows
                        ws.Cells["A" + y].Value = row.WORef;
                        ws.Cells["B" + y].Value = row.WOTitle;
                        ws.Cells["C" + y].Value = string.Format("{0:MMM dd, yyyy}", row.Requestdt);
                        ws.Cells["D" + y].Value = row.Cetegory;
                        ws.Cells["E" + y].Value = string.Format("{0:C}", row.Amout);

                        #region Row Styles
                        ws.Cells[y, 5, y, 5].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells[y, 1, y, 5].Style.WrapText = true;
                        ws.Cells[y, 1, y, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                        if (i % 2 != 0)
                        {
                            ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        }
                        #endregion

                        #endregion
                        i++;
                        y++;
                    }

                    ws.Cells[y, 1, y, 4].Merge = true;
                    ws.Cells["A" + y].Value = "Total :";
                    ws.Cells["E" + y].Value = string.Format("{0:C}", data.Where(x => x.PONum == po).Sum(x => x.Amout));

                    #region Row Styles
                    ws.Cells[y, 1, y, 5].Style.Font.Color.SetColor(Color.FromArgb(175, 22, 22));
                    ws.Cells[y, 1, y, 5].Style.Font.Bold = true;
                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells["E" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["E" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                    ws.Cells[y, 1, y, 5].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion
                    i++;
                    y++;
                }

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public List<POTracking> POTrackingData(POTrackingFilter model)
        {
            ViewBag.DateFrom = model.dateFrom;
            ViewBag.DateTo = model.dateTo;
            var data = db.Set<POTracking>().FromSqlRaw("CALL POTracking('" + model.pONum + "','" + model.dateFrom.Value.ToString("yyyy-MM-dd") + "','" + model.dateTo.Value.ToString("yyyy-MM-dd") + "')").ToList();
            var po = db.ADM16200.Where(x => x.PONUM == model.pONum).FirstOrDefault();
            ViewBag.POAmount = po != null ? po.TOTAL : 0;
            ViewBag.Description = po != null ? po.Description : "";
            return data;
        }

        #endregion

        #region PO Summary

        //[TeliconAuthorize(TaskId = 31)]
        public ActionResult POSummary()
        {
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
        [HttpPost]
        public ActionResult GeneratePOSummary(POSummaryFilter model)
        {
            return PartialView("Partials/POSummaryPartial", POSummaryData(model));
        }
        [HttpPost]
        public void POSummaryToExcel(POSummaryFilter model)
        {
            string docName = "POSummary";
            ExcelPackage pck = new ExcelPackage();

            var data = POSummaryData(model);

            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("POSummary");

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

                ws.Column(1).Width = 25;
                ws.Column(2).Width = 25;
                ws.Column(3).Width = 25;
                int y = 1; int cc = 3;

                #region Title
                ws.Cells[y, 1, y, cc].Merge = true;
                ws.Cells["A" + y].Value = "Purches Order Summary Report";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;
                if (ViewBag.DateFrom != null && ViewBag.DateTo != null)
                {
                    ws.Cells[y, 1, y, cc].Merge = true;
                    ws.Cells["A" + y].Value = "Period Starting " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateFrom) + " to " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateTo);
                    ws.Cells["A" + y].Style.Font.Bold = true;
                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["A" + y].Style.Font.Name = "Arial";
                    ws.Cells["A" + y].Style.Font.Size = 10;
                    y++;
                }
                ws.Cells[y, 1, y, cc].Merge = true;
                y++;
                #endregion

                #region Table Headers
                ws.Cells["A" + y].Value = "PO No.";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Ref. No.";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Amount";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, cc].Style.Font.Bold = true;
                ws.Cells[y, 1, y, cc].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                ws.Cells[y, 1, y, cc].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, cc].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, cc].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(y).Height = 20;
                #endregion
                y++;

                int i = 1;
                foreach (var po in data.Select(x => x.PONum).Distinct())
                {
                    var rows = data.Where(x => x.PONum == po).ToList();

                    #region Rows
                    ws.Cells["A" + y].Value = po;
                    ws.Cells["B" + y].Value = rows[0].Wo_ref;
                    ws.Cells["C" + y].Value = string.Format("{0:C}", rows.Sum(x => x.InvoiceTotal));

                    #region Row Styles
                    ws.Cells[y, 3, y, cc].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells[y, 1, y, cc].Style.WrapText = true;
                    ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    #endregion
                    i++;
                    y++;
                }

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public List<POSummary> POSummaryData(POSummaryFilter model)
        {
            Period pEnd = null;
            Period pStart = null;
            DateTime? start = null;
            DateTime? end = null;
            var periods = Customs.GetPeriods(model.periodStartYear.Value, model.periodEndYear.Value);
            if (periods.Any())
            {
                pEnd = periods.FirstOrDefault(x => x.PeriodNo == model.periodEnd.Value && x.PeriodYear == model.periodEndYear.Value);
                pStart = periods.FirstOrDefault(x => x.PeriodNo == model.periodStart.Value && x.PeriodYear == model.periodStartYear.Value);
                if (pEnd != null && pStart != null)
                {
                    start = pStart.PayDate;
                    end = pEnd.PayDate;
                }
            }
            ViewBag.DateFrom = start;
            ViewBag.DateTo = end;
            ViewBag.pDateFrom = pStart.DateFrom;
            ViewBag.pDateTo = pEnd.DateTo;
            ViewBag.YearFrom = pStart.PeriodYear;
            ViewBag.YearTo = pStart.PeriodYear;
            var data = db.Set<POSummary>().FromSqlRaw("CALL POSummary(" + pStart.PeriodYear + ",'" + start.Value.ToString("yyyy-MM-dd") + "','" + end.Value.ToString("yyyy-MM-dd") + "')").ToList();
            data.ForEach(x => x.PONum = (x.PONum == null ? "TEMPPO" : x.PONum));
            return data;
        }

        [HttpPost]
        public ActionResult POSummaryDetails(int? yearFrom, int? yearTo, DateTime? dateFrom, DateTime? dateTo, string poNum)
        {
            var data = POSummaryDetailsData(yearFrom, yearTo, dateFrom, dateTo, poNum);
            return View(data);
        }
        [HttpPost]
        public void POSummaryDetailsToExcel(POSummaryDetaisFilter model)
        {
            string docName = "POSummaryDetails";
            ExcelPackage pck = new ExcelPackage();

            var data = POSummaryDetailsData(model.yearFrom, model.yearTo, model.dateFrom, model.dateTo, model.poNum);

            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("POSummaryDetails");

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

                ws.Column(1).Width = 12;
                ws.Column(2).Width = 12;
                ws.Column(3).Width = 50;
                ws.Column(4).Width = 13;
                int y = 1; int cc = 4;

                #region Title
                ws.Cells[y, 1, y, cc].Merge = true;
                ws.Cells["A" + y].Value = "Purches Order Invoice Detail Report";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;
                if (ViewBag.DateFrom != null && ViewBag.DateTo != null)
                {
                    ws.Cells[y, 1, y, cc].Merge = true;
                    ws.Cells["A" + y].Value = "Period Starting " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateFrom) + " to " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateTo);
                    ws.Cells["A" + y].Style.Font.Bold = true;
                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["A" + y].Style.Font.Name = "Arial";
                    ws.Cells["A" + y].Style.Font.Size = 10;
                    y++;
                }
                ws.Cells[y, 1, y, cc].Merge = true;
                y++;
                #endregion

                #region Table Headers
                ws.Cells["A" + y].Value = "Invoice No.";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Ref. No.";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Title";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["D" + y].Value = "Amount";
                ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, cc].Style.Font.Bold = true;
                ws.Cells[y, 1, y, cc].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                ws.Cells[y, 1, y, cc].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, cc].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, cc].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(y).Height = 20;
                #endregion
                y++;

                int i = 1;
                foreach (var po in data.Select(x => x.PONum).Distinct())
                {
                    ws.Cells[y, 1, y, cc].Merge = true;
                    ws.Cells["A" + y].Value = "PO Number: " + po;

                    #region Row Styles
                    ws.Cells[y, 1, y, cc].Style.Font.Color.SetColor(Color.FromArgb(21, 98, 175));

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells[y, 1, y, cc].Style.WrapText = true;
                    ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    ws.Cells[y, 1, y, cc].Style.Font.Bold = true;
                    ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion
                    i++;
                    y++;

                    var rows = data.Where(x => x.PONum == po).ToList();
                    foreach (var row in rows)
                    {
                        #region Rows
                        ws.Cells["A" + y].Value = row.InvoiceNum;
                        ws.Cells["B" + y].Value = row.Wo_ref;
                        ws.Cells["C" + y].Value = row.InvoiceTitle;
                        ws.Cells["D" + y].Value = string.Format("{0:C}", row.InvoiceTotal);

                        #region Row Styles
                        ws.Cells[y, 4, y, cc].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells[y, 1, y, cc].Style.WrapText = true;
                        ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                        if (i % 2 != 0)
                        {
                            ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        }
                        #endregion

                        #endregion
                        i++;
                        y++;
                    }

                    ws.Cells[y, 1, y, 3].Merge = true;
                    ws.Cells["A" + y].Value = "Total:";
                    ws.Cells["D" + y].Value = string.Format("{0:C}", rows.Sum(x => x.InvoiceTotal));

                    #region Row Styles
                    ws.Cells[y, 4, y, cc].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells[y, 1, y, cc].Style.WrapText = true;
                    ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    ws.Cells[y, 1, y, cc].Style.Font.Bold = true;
                    ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion
                    i++;
                    y++;

                }

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public List<POSummary> POSummaryDetailsData(int? yearFrom, int? yearTo, DateTime? dateFrom, DateTime? dateTo, string poNum)
        {
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;
            ViewBag.YearFrom = yearFrom;
            ViewBag.YearTo = yearTo;
            ViewBag.PONum = poNum;

            var data = db.Set<POSummary>().FromSqlRaw("CALL POSummary(" + yearFrom.Value + ",'" + dateFrom.Value.ToString("yyyy-MM-dd") + "','" + dateTo.Value.ToString("yyyy-MM-dd") + "')").ToList();
            data.ForEach(x => x.PONum = (string.IsNullOrEmpty(x.PONum) ? "TEMPPO" : x.PONum));
            var fData = data.Where(x => x.PONum.Trim() == poNum).ToList();
            fData = fData.Select(x => { x.InvoiceNum = Customs.MakeGenericInvoiceNo(x.InvoiceNum); return x; }).ToList();
            return fData;
        }

        #endregion

        #region Material Usage  

        //[TeliconAuthorize(TaskId = 32)]
        public ActionResult MaterialUsage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GenerateMaterialUsage(DateTime dateFrom, DateTime dateTo)
        {
            var printData = MaterialUsageData(dateFrom, dateTo);
            return PartialView("Partials/MaterialUsagePartial", printData);
        }

        [HttpPost]
        public void MaterialUsageToExcel(MaterialUsageFilter model)
        {
            string docName = "MaterialUsage";
            ExcelPackage pck = new ExcelPackage();

            var data = MaterialUsageData(model.dateFrom, model.dateTo);

            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("MaterialUsage");

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

                ws.Column(1).Width = 25;
                ws.Column(2).Width = 25;
                ws.Column(3).Width = 25;
                int y = 1; int cc = 3;

                #region Title
                ws.Cells[y, 1, y, cc].Merge = true;
                ws.Cells["A" + y].Value = "Material Usage Report";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;
                if (ViewBag.DateFrom != null && ViewBag.DateTo != null)
                {
                    ws.Cells[y, 1, y, cc].Merge = true;
                    ws.Cells["A" + y].Value = "Period Starting " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateFrom) + " to " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateTo);
                    ws.Cells["A" + y].Style.Font.Bold = true;
                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["A" + y].Style.Font.Name = "Arial";
                    ws.Cells["A" + y].Style.Font.Size = 10;
                    y++;
                }
                ws.Cells[y, 1, y, cc].Merge = true;
                y++;
                #endregion

                #region Table Headers
                ws.Cells["A" + y].Value = "Nos.";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Description";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Qty";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, cc].Style.Font.Bold = true;
                ws.Cells[y, 1, y, cc].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                ws.Cells[y, 1, y, cc].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, cc].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, cc].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(y).Height = 20;
                #endregion
                y++;

                int i = 1;
                foreach (var matId in data.Select(x => x.WoMatID).Distinct())
                {
                    var rows = data.Where(x => x.WoMatID == matId).ToList();

                    #region Rows
                    ws.Cells["A" + y].Value = i;
                    ws.Cells["B" + y].Value = rows[0].MaterialName;
                    ws.Cells["C" + y].Value = rows.Sum(x => x.WoMatQty);

                    #region Row Styles
                    ws.Cells[y, 3, y, cc].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells[y, 1, y, cc].Style.WrapText = true;
                    ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    #endregion
                    i++;
                    y++;
                }

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public List<MaterialUsage> MaterialUsageData(DateTime? dateFrom, DateTime? dateTo)
        {
            DateTime? start = null;
            DateTime? end = null;
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                start = dateFrom.Value;
                end = dateTo.Value;
            }
            var data = new List<MaterialUsage>();
            if (start.HasValue && end.HasValue)
            {
                ViewBag.DateFrom = dateFrom;
                ViewBag.DateTo = dateTo;

                data = db.Set<MaterialUsage>().FromSqlRaw("CALL MaterialUsage('" + start.Value.ToString("yyyy-MM-dd") + "','" + end.Value.ToString("yyyy-MM-dd") + "')").ToList();
            }
            return data;
        }

        [HttpPost]
        public ActionResult MaterialUsageDetail(DateTime? dateFrom, DateTime? dateTo, int matId)
        {
            var data = MaterialUsageDetailsData(dateFrom, dateTo, matId);
            return View(data);
        }

        [HttpPost]
        public void MaterialUsageDetailsToExcel(MaterialUsageDetailFilter model)
        {
            string docName = "MaterialUsageDetails";
            ExcelPackage pck = new ExcelPackage();

            var data = MaterialUsageDetailsData(model.dateFrom, model.dateTo, model.matId);

            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("MaterialUsageDetails");

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
                ws.Column(2).Width = 12;
                ws.Column(3).Width = 50;
                ws.Column(4).Width = 13;
                int y = 1; int cc = 3;

                #region Title
                ws.Cells[y, 1, y, cc].Merge = true;
                ws.Cells["A" + y].Value = "Material Usage Detail Report";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;
                if (ViewBag.DateFrom != null && ViewBag.DateTo != null)
                {
                    ws.Cells[y, 1, y, cc].Merge = true;
                    ws.Cells["A" + y].Value = "Period Starting " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateFrom) + " to " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateTo);
                    ws.Cells["A" + y].Style.Font.Bold = true;
                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["A" + y].Style.Font.Name = "Arial";
                    ws.Cells["A" + y].Style.Font.Size = 10;
                    y++;
                }
                ws.Cells[y, 1, y, cc].Merge = true;
                y++;
                #endregion

                #region Table Headers
                ws.Cells["A" + y].Value = "Nos.";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Activity";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Qty";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, cc].Style.Font.Bold = true;
                ws.Cells[y, 1, y, cc].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                ws.Cells[y, 1, y, cc].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, cc].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, cc].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(y).Height = 20;
                #endregion
                y++;

                int i = 1;
                foreach (var refNo in data.Select(x => x.Wo_ref).Distinct())
                {

                    var rows = data.Where(x => x.Wo_ref == refNo).ToList();

                    ws.Cells["A" + y].Value = refNo;
                    ws.Cells[y, 2, y, cc].Merge = true;
                    ws.Cells["B" + y].Value = rows[0].Wo_title;

                    #region Row Styles
                    ws.Cells[y, 1, y, cc].Style.Font.Color.SetColor(Color.FromArgb(21, 98, 175));

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells[y, 1, y, cc].Style.WrapText = true;
                    ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    ws.Cells[y, 1, y, cc].Style.Font.Bold = true;
                    ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion
                    i++;
                    y++;
                    int n = 1;

                    foreach (var row in rows)
                    {
                        #region Rows
                        ws.Cells["A" + y].Value = n;
                        ws.Cells["B" + y].Value = row.RateDescr;
                        ws.Cells["C" + y].Value = row.WoMatQty;

                        #region Row Styles
                        ws.Cells[y, 3, y, cc].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                        ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                        ws.Cells[y, 1, y, cc].Style.WrapText = true;
                        ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                        ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                        if (i % 2 != 0)
                        {
                            ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                        }
                        #endregion

                        #endregion
                        i++;
                        y++;
                        n++;
                    }

                    ws.Cells[y, 1, y, 2].Merge = true;
                    ws.Cells["A" + y].Value = "Total:";
                    ws.Cells["C" + y].Value = rows.Sum(x => x.WoMatQty);

                    #region Row Styles
                    ws.Cells[y, 3, y, cc].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["D" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["D" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells[y, 1, y, cc].Style.WrapText = true;
                    ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    ws.Cells[y, 1, y, cc].Style.Font.Bold = true;
                    ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion
                    i++;
                    y++;

                }

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public List<MaterialUsageDetail> MaterialUsageDetailsData(DateTime? dateFrom, DateTime? dateTo, int matId)
        {
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;
            ViewBag.MatId = matId;

            var data = db.Set<MaterialUsageDetail>().FromSqlRaw("CALL MaterialUsageDetail(" + matId + ",'" + dateFrom.Value.ToString("yyyy-MM-dd") + "','" + dateTo.Value.ToString("yyyy-MM-dd") + "')").ToList();
            return data;
        }

        #endregion

        #region Invoice Summary Report

        //[TeliconAuthorize(TaskId = 39)]
        public ActionResult InvoiceSummary()
        {
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

        [HttpPost]
        public ActionResult GenerateInvoiceSummary(InvoiceSummaryFilter model)
        {
            var printData = InvoiceSummaryData(model);
            return PartialView("Partials/InvoiceSummaryPartial", printData);
        }

        [HttpPost]
        public void InvoiceSummaryToExcel(InvoiceSummaryFilter model)
        {
            string docName = "MaterialUsage";
            ExcelPackage pck = new ExcelPackage();

            var data = MaterialUsageData(model.DateFrom, model.DateTo);

            if (data != null)
            {
                #region Work Sheet
                var ws = pck.Workbook.Worksheets.Add("MaterialUsage");

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

                ws.Column(1).Width = 25;
                ws.Column(2).Width = 25;
                ws.Column(3).Width = 25;
                int y = 1; int cc = 3;

                #region Title
                ws.Cells[y, 1, y, cc].Merge = true;
                ws.Cells["A" + y].Value = "Material Usage Report";
                ws.Cells["A" + y].Style.Font.Bold = true;
                ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["A" + y].Style.Font.Name = "Arial";
                ws.Cells["A" + y].Style.Font.Size = 12;
                y++;
                if (ViewBag.DateFrom != null && ViewBag.DateTo != null)
                {
                    ws.Cells[y, 1, y, cc].Merge = true;
                    ws.Cells["A" + y].Value = "Period Starting " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateFrom) + " to " + string.Format("{0:dddd MMMM dd, yyyy}", ViewBag.DateTo);
                    ws.Cells["A" + y].Style.Font.Bold = true;
                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["A" + y].Style.Font.Name = "Arial";
                    ws.Cells["A" + y].Style.Font.Size = 10;
                    y++;
                }
                ws.Cells[y, 1, y, cc].Merge = true;
                y++;
                #endregion

                #region Table Headers
                ws.Cells["A" + y].Value = "Nos.";
                ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["B" + y].Value = "Description";
                ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                ws.Cells["C" + y].Value = "Qty";
                ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));

                ws.Cells[y, 1, y, cc].Style.Font.Bold = true;
                ws.Cells[y, 1, y, cc].Style.Font.Name = "Inherit";
                ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                ws.Cells[y, 1, y, cc].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                ws.Cells[y, 1, y, cc].Style.Border.Bottom.Color.SetColor(Color.DodgerBlue);
                ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                ws.Cells[y, 1, y, cc].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(y).Height = 20;
                #endregion
                y++;

                int i = 1;
                foreach (var matId in data.Select(x => x.WoMatID).Distinct())
                {
                    var rows = data.Where(x => x.WoMatID == matId).ToList();

                    #region Rows
                    ws.Cells["A" + y].Value = i;
                    ws.Cells["B" + y].Value = rows[0].MaterialName;
                    ws.Cells["C" + y].Value = rows.Sum(x => x.WoMatQty);

                    #region Row Styles
                    ws.Cells[y, 3, y, cc].Style.Font.Color.SetColor(Color.FromArgb(0, 153, 0));

                    ws.Cells["A" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["B" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["C" + y].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells["A" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["B" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells["C" + y].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.FromArgb(230, 230, 230));
                    ws.Cells[y, 1, y, cc].Style.WrapText = true;
                    ws.Cells[y, 1, y, cc].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    ws.Cells[y, 1, y, cc].Style.Font.Size = 11;
                    if (i % 2 != 0)
                    {
                        ws.Cells[y, 1, y, cc].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[y, 1, y, cc].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(244, 244, 244));
                    }
                    #endregion

                    #endregion
                    i++;
                    y++;
                }

                #endregion
            }

            if (pck.Workbook.Worksheets.Count() > 0)
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.Headers.Add("content-disposition", "attachment;  filename=" + docName + ".xlsx");
                pck.SaveAs(Response.Body);
            }
        }

        public List<InvoiceSummaryData> InvoiceSummaryData(InvoiceSummaryFilter model)
        {
            var data = new List<InvoiceSummaryData>();

            var wos = db.TRN23100.Where(x => x.PaidDate >= model.DateFrom && x.PaidDate <= model.DateTo).OrderBy(x => x.Wo_ref).ToList();
            var dbActs = db.ADM01100.Where(x => x.Active).ToList();
            foreach (var wo in wos)
            {
                var header = new InvoiceSummaryData();
                header.WOID = wo.Workid;
                header.RefNo = wo.Wo_ref;
                header.Title = wo.Wo_title;
                header.PONum = wo.PONum;
                header.Date = wo.Requestdt;
                var items = dbActs.Select(x =>
                {
                    var woItem = wo.TRN23110.Where(i => i.ActivityID == x.RateID).ToList();
                    var loc = "";
                    var locDesc = "";
                    var qty = 0M;
                    if (woItem != null)
                    {
                        //    loc = woItem.Location;
                        //    locDesc = woItem.AdtnlDetails;
                        qty = woItem.Sum(i => i.OActQty);
                    }
                    return new InvoiceSummaryItem
                    {
                        ActCode = x.AltCode,
                        ActDescr = x.RateDescr,
                        ActId = x.RateID,
                        Loc = loc,
                        LocDescr = locDesc,
                        Qty = qty,
                        WOID = header.WOID
                    };
                }).ToList();
                header.Items = items;
                data.Add(header);
            }

            ViewBag.DateFrom = model.DateFrom;
            ViewBag.DateTo = model.DateTo;

            return data;
        }

        #endregion

        #region Annual Payroll

        //[TeliconAuthorize(TaskId = 40)]
        public ActionResult AnnualPayroll()
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

        #region Create PDF

        [HttpPost]
        public FileResult GenerateAnnualPayroll(TempR model)
        {
            int year = model.periodStartYear;
            int period = model.periodStart;
            var p = Customs.GetPeriods(year, year);

            var statements = db.Set<ContractorsStatement>().FromSqlRaw("CALL ContractorsStatement('" + p[0].DateFrom.ToString("yyyy-MM-dd") + "','" + p[p.Count - 1].DateTo.ToString("yyyy-MM-dd") + "')").ToList();
            var TRecords = statements.Join(db.ADM03300.Include(z => z.ADM04200), x => x.ContractorID, y => y.ConID, (x, y) => new ContractorPaySlipReport
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
                PayDate = DateTime.Now,
                EngagementDt = y.EngagementDt,
                TRN = y.TRN,
                NIS = y.NIS,
                ContNo = y.TechNo
            }).ToList();

            TRecords = TRecords.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            MemoryStream workStream = new MemoryStream();
            StringBuilder status = new StringBuilder("");
            DateTime dTime = DateTime.Now;
            //file name to be created 
            string strPDFFileName = string.Format("ContractorPaySlips_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");

            Document doc = new Document();
            doc.SetPageSize(PageSize.A4);
            doc.SetMargins(0f, 0f, 0f, 0f);
            //doc.SetMargins(0f, 0f, 0f, 0f);
            PdfWriter writer = PdfWriter.GetInstance(doc, workStream);
            writer.CloseStream = false;

            doc.Open();
            PdfContentByte cb = writer.DirectContent;
            if (TRecords != null && TRecords.Count > 0)
            {
                List<int> conIds = TRecords.Select(x => x.ConId).ToList();
                //Create Pay slip border
                //int c = 2;
                //int i = 0;
                foreach (var conId in conIds)
                {
                    var rows = TRecords.FindAll(x => x.ConId == conId);
                    //var deductions = db.TRN04100.Where(x => x.ConductorID == conId && x.StartDate <= p.PayDate && x.EndDate >= p.PayDate).ToList();
                    //var deductData = deductions.Select(x => new ContractorDeductionReport
                    //{
                    //    DeductionName = x.ADM04100.Name,
                    //    StartDate = x.StartDate,
                    //    EndDate = x.EndDate ?? new DateTime(),
                    //    Amount = x.Amount
                    //}).ToList();

                    //var img = iTextSharp.text.Image.GetInstance(new Uri(Server.MapPath("~/images/print-logo.png")));
                    //img.ScaleAbsolute(104, 56);
                    //img.SetAbsolutePosition(11, ((c * (doc.PageSize.Height / 3)) + 5) + (doc.PageSize.Height / 3) - 70);
                    //doc.Add(img);

                    doc.Add(new Paragraph(new Chunk("TELiCON", FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph(new Chunk("P.O. BOX 3069, KINGSTON 8", FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph(new Chunk("TEL: 618-3628", FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph(new Chunk("EMAIL: admin@telicongroup.com", FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph(new Chunk("CERTIFICATE OF PAY FOR THE YEAR " + year, FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    //cb.Rectangle(10f, (c * (doc.PageSize.Height / 3)) + 5, doc.PageSize.Width - 20, (doc.PageSize.Height / 3) - 10);
                    //cb.Stroke();
                    //doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 8f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    //doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 2f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    PdfPTable HeaderTableLayout = new PdfPTable(6);
                    HeaderTableLayout.HorizontalAlignment = Element.ALIGN_RIGHT;
                    doc.Add(Add_AnnuPayHeader_To_PDF(HeaderTableLayout, doc.PageSize.Width / 8, rows[0], p[0].DateFrom, p[p.Count - 1].DateTo));

                    doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 5f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    PdfPTable tableLayout = new PdfPTable(4);
                    tableLayout.HorizontalAlignment = Element.ALIGN_CENTER;
                    doc.Add(Add_AnnuPayContent_To_PDF(tableLayout, doc.PageSize.Width / 16, rows, null));

                    doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph(new Chunk("WE CERTIFY THAT THE PARTICULARS GIVEN ABOVE INCLUDE THE AMOUNT OF PAY PAID TO YOU BY US (LESS ANY REFUND) IN THAT YEAR." + year, FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_LEFT });


                    doc.Add(new Paragraph(new Chunk(" ", FontFactory.GetFont("Ebrima", 36f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
                    doc.Add(new Paragraph(new Chunk("................................................ EMPLOYER" + year, FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_RIGHT });
                }
            }
            else
                doc.Add(new Paragraph(new Chunk("No Records found", FontFactory.GetFont("Ebrima", 11f, iTextSharp.text.Font.BOLD))) { Alignment = Element.ALIGN_CENTER });
            doc.Close();
            //}
            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            return File(workStream, "application/pdf", strPDFFileName);

        }

        protected PdfPTable Add_AnnuPayHeader_To_PDF(PdfPTable tableLayout, float minColWidth, ContractorPaySlipReport detail, DateTime pStart, DateTime pEnd)
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

            tableLayout.AddCell(new PdfPCell(new Phrase("Cost :", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.BOLD))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase("AMBER/FIBER/INTALLATION", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.NORMAL))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase("NIS :", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.BOLD))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase(detail.NIS, FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.NORMAL))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase("TRN :", FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.BOLD))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });
            tableLayout.AddCell(new PdfPCell(new Phrase(detail.TRN, FontFactory.GetFont("Ebrima", 9, iTextSharp.text.Font.NORMAL))) { HorizontalAlignment = Element.ALIGN_LEFT, BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255), Border = 0 });

            return tableLayout;
        }

        protected PdfPTable Add_AnnuPayContent_To_PDF(PdfPTable tableLayout, float minColWidth, List<ContractorPaySlipReport> rows, List<ContractorDeductionReport> deductions)
        {

            float[] headers = {
                Convert.ToInt32(minColWidth * 4),
                Convert.ToInt32(minColWidth * 3) + Convert.ToInt32(minColWidth * 0.75),
                Convert.ToInt32(minColWidth * 3) + Convert.ToInt32(minColWidth * 0.75),
                Convert.ToInt32(minColWidth * 3) + Convert.ToInt32(minColWidth * 0.75)
            };  //Header Widths
            tableLayout.SetWidths(headers);        //Set the pdf headers
            tableLayout.WidthPercentage = 96.5f;       //Set the PDF File witdh percentage
            //tableLayout.HeaderRows = 1;
            //Add Title to the PDF file at the top

            var boldText = iTextSharp.text.Font.BOLD;
            var normalText = iTextSharp.text.Font.NORMAL;

            ////Add header
            AddCell(tableLayout, " ", boldText, 9, Element.ALIGN_LEFT, 0, 0, 0, 0, 2, 2);
            AddCell(tableLayout, "PREVIOUS EMPLOYMENT", boldText, 9, Element.ALIGN_RIGHT, 0, 0, 0, 5, 2, 2);
            AddCell(tableLayout, "THIS EMPLOYMENT", boldText, 9, Element.ALIGN_RIGHT, 0, 0, 0, 0, 0, 5, 2, 2);
            AddCell(tableLayout, "TOTAL", boldText, 9, Element.ALIGN_RIGHT, 0, 0, 0, 0, 0, 5, 2, 2); ;

            AddCell(tableLayout, "TAXABLE PAY", normalText, 9, Element.ALIGN_LEFT, 0, 0, 0, 0, 5, 0, 2, 2);
            AddCell(tableLayout, string.Format("{0:C}", 0), normalText, 9, Element.ALIGN_RIGHT, 0, 0, 0, 0, 0, 5, 2, 2);
            AddCell(tableLayout, string.Format("{0:C}", rows.Sum(x => x.PenalizeTotal)), normalText, 9, Element.ALIGN_RIGHT, 0, 0, 0, 0, 0, 5, 2, 2);
            AddCell(tableLayout, string.Format("{0:C}", rows.Sum(x => x.PenalizeTotal)), normalText, 9, Element.ALIGN_RIGHT, 0, 0, 0, 0, 0, 5, 2, 2);

            return tableLayout;
        }

        #endregion


        #endregion
    }
}
