using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Reusables
{
    public static class Customs
    {
        public static string GetSortString(List<OrderParam> orders, List<ColumnsParam> cols)
        {
            string sortString = "";
            int index = 0;
            foreach (OrderParam order in orders)
            {
                ColumnsParam col = cols.ToArray()[order.column];
                if (!string.IsNullOrEmpty(col.name))
                {
                    string sortDir = order.dir;
                    sortString += col.name + " " + sortDir;
                    if (index < orders.Count - 1)
                        sortString += ", ";
                }
            }
            return sortString;
        }
        public static TechnicianStat GetTechStats(int? conID, string email)
        {
            using TeliconDbContext db = new TeliconDbContext(GetDbContextOptions());
            var userWO = db.TRN23100.Where(x => !conID.HasValue ? x.ADM03400.Any(y => y.ADM03300.Email == email) : x.ADM03400.Any(y => y.ContractorID == conID)).Select(x => new
            {
                x.Status
            }).ToList();
            return db.ADM03300.Where(x => !conID.HasValue ? x.Email == email : x.ConID == conID.Value).AsEnumerable().Select(x => new TechnicianStat
            {
                ID = x.EmployeeID,
                Name = x.LastName + " " + x.FirstName.Substring(0, 1) + ".",
                Type = DataDictionaries.UserTypes[x.ConClass],
                Total = userWO.Count,
                Invoiced = userWO.Count(y => y.Status == "i"),
                Processing = userWO.Count(y => y.Status == "p"),
                Submitted = userWO.Count(y => y.Status == "s"),
                New = userWO.Count(y => y.Status == "n" || y.Status == "d"),
                HasSettings = !conID.HasValue
            }).FirstOrDefault();
        }
        public static string MakeRealAddress(string street, string city, string parish)
        {
            return street + " " + (string.IsNullOrEmpty(city) ? ", " : city + ", ") + parish;
        }
        public static int MakeGenericInvoiceNo(int invNum)
        {
            using TeliconDbContext db = new TeliconDbContext(GetDbContextOptions());
            var invoice = db.TRN09100.Find(invNum);
            int year = invoice == null ? DateTime.Now.Year : invoice.GeneratedDate.Value.Year;
            //int minInvoiceNo = year * 10000;
            string invNo = year + "" + invNum;
            return Convert.ToInt32(invNo);
            //return minInvoiceNo + invNum;
        }
        public static int MakeGenericSInvoiceNo(int sInvNum)
        {
            using TeliconDbContext db = new TeliconDbContext(GetDbContextOptions());
            var sInvoice = db.TRN19100.Find(sInvNum);
            int year = sInvoice == null ? DateTime.Now.Year : sInvoice.Requestdt.Year;
            int minInvoiceNo = year * 10000;
            return minInvoiceNo + sInvNum;
        }
        public static int RevertInvoiceNo(int invNum)
        {
            int yearLength = DateTime.Now.Year.ToString().Length;
            string invStr = invNum.ToString().Substring(yearLength, invNum.ToString().Length - yearLength);
            int.TryParse(invStr, out int newInvNum);
            return newInvNum;
        }
        public static string GetSettingsFileValue(string field, string filePath)
        {
            XDocument xdoc = XDocument.Load(filePath);
            string value = xdoc.Root.Element(field).Value;
            return value;
        }
        public static decimal GetTaxValue(string type, DateTime date)
        {
            using TeliconDbContext db = new TeliconDbContext(GetDbContextOptions());
            if (db.ADM07100.Any() && !string.IsNullOrEmpty(type) && date != null)
            {
                var periodTax = db.ADM07100.FirstOrDefault(x => x.StartDate >= date && (x.EndDate ?? DateTime.Now) <= date);
                return Convert.ToDecimal(periodTax.Percentage);
            }
            string filePath = "";// HttpContext.Current.Server.MapPath("~/settings.xml");
            return Convert.ToDecimal(GetSettingsFileValue("DefaultTaxRate", filePath));
        }
        public static List<Period> GetPeriods(int? min, int? max)
        {
            using TeliconDbContext db = new TeliconDbContext(GetDbContextOptions());
            List<TRN23110> tRN23110s = db.TRN23110.ToList();
            var minYear = min ?? tRN23110s.Min(x => x.ActDate.Year);
            var maxYear = max ?? tRN23110s.Max(x => x.ActDate.Year);
            var periods = db.ADM16100.Where(x => x.periodYear >= minYear && x.periodYear <= maxYear).Select(x => new Period
            {
                PayDate = x.PayDate,
                DateFrom = x.PeriodStart,
                DateTo = x.PeriodEnd,
                DueDate = x.DueDate,
                PeriodNo = x.Week,
                PeriodYear = x.periodYear
            }).ToList();
            return periods;
        }
        public static DbContextOptions<TeliconDbContext> GetDbContextOptions()
        {
            IConfiguration Configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

            return new DbContextOptionsBuilder<TeliconDbContext>()
                             .UseMySql(Configuration.GetConnectionString("DefaultConnection"), ServerVersion.AutoDetect(Configuration.GetConnectionString("DefaultConnection")))
                             .Options;
        }
    }
}