using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace TeliconLatest.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly TeliconDbContext db;
        public HomeController(TeliconDbContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            WorkOrderSummary woSummary = new WorkOrderSummary();
            await Task.Run(() =>
            {
                woSummary = new WorkOrderSummary
                {
                    Activities = db.TRN23110.Count(),
                    Locations = db.TRN23110.Select(x => x.Location).Distinct().Count(),
                    Contractors = db.ADM03400.Select(x => x.ContractorID).Distinct().Count(),
                    Invoiced = db.TRN23100.Count(x => x.Status.ToLower() == "i"),
                    New = db.TRN23100.Count(x => x.Status.ToLower() == "n"),
                    Processing = db.TRN23100.Count(x => x.Status.ToLower() == "p" || x.Status.ToLower() == "d"),
                    Submitted = db.TRN23100.Count(x => x.Status.ToLower() == "s" || x.Submitted && !new[] { "v", "i" }.Contains(x.Status)),
                    Verified = db.TRN23100.Count(x => x.Status.ToLower() == "v"),
                    RevenueIncome = GetRevenuesVsIncomes("m", DateTime.Now.Year)
                };

                List<DateTime> tRN23100s = db.TRN23100.Select(p => p.Requestdt).ToList();
                ViewBag.Years = tRN23100s.Select(x => x.Year).Distinct()
                        .OrderByDescending(x => x)
                        .Select(x => new SelectListItem
                        {
                            Text = x.ToString(),
                            Value = x.ToString()
                        }).ToList();
            });
            return View(woSummary);
        }

        public List<RevenueVsIncomeChartData> GetRevenuesVsIncomes(string type, int? year)
        {
            string query = string.Format("CALL WorkOrderSummary('" + type + "',{0})", year);
            List<RevenueVsIncomeChartData> data = db.Set<RevenueVsIncomeChartData>().FromSqlRaw(query).AsEnumerable().Select(x => new RevenueVsIncomeChartData
            {
                PeriodName = type != "m" ? Convert.ToString(x.Period) : new DateTimeFormatInfo().GetAbbreviatedMonthName(Convert.ToInt32(x.Period)),
                Payments = x.Payments,
                Revenue = x.Revenue
            }).ToList();
            return data;
        }
        public JsonResult GetRevenueDataForChart(string type, int? year)
        {
            return Json(GetRevenuesVsIncomes(type, year));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}