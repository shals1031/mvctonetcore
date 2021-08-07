using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;
using TeliconLatest.Reusables;

namespace TeliconLatest.Controllers
{
    public class ListController : Controller
    {
        private readonly TeliconDbContext db;
        public ListController(TeliconDbContext db)
        {
            this.db = db;
        }
        //#region Technician Lists
        //[HttpPost]
        //public JsonResult TechnicianStatement(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var periods = Customs.GetPeriods(null, null);
        //    List<PeriodSummary> periodSummary = new List<PeriodSummary>();
        //    var pieces = model.additional.ToString().Split('|');
        //    var user = User.Identity.Name;
        //    int id = Convert.ToInt32(pieces[0]);
        //    string format = "dddd MMM dd, yyyy";
        //    System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;
        //    var minYear = db.Trn23100s.Min(x => x.Requestdt).Year;
        //    DateTime from = string.IsNullOrEmpty(pieces[1]) ? db.Adm16100s.Min(x => x.PayDate) : DateTime.ParseExact(pieces[1], format, provider);
        //    DateTime to = string.IsNullOrEmpty(pieces[2]) ? db.Adm16100s.Where(x => x.PeriodYear == DateTime.Now.Year).Max(x => x.PayDate) : DateTime.ParseExact(pieces[2], format, provider);

        //    //var actTotals = db.TechnicianStatementDetail(id, from, to).Select(x => new { paidDate = x.PaidDate, date = x.ActDate, total = x.PenalizeAmount }).ToList();
        //    if (!string.IsNullOrEmpty(pieces[1]) && !string.IsNullOrEmpty(pieces[2]))
        //    {
        //        var pastList = periods.Where(x => x.PayDate <= from);
        //        var nearestDateFrom = pastList != null && pastList.Any() ? pastList.OrderByDescending(x => x.PayDate).FirstOrDefault().PayDate : from;
        //        var futureList = periods.Where(x => x.PayDate >= to);
        //        var nearestDateTo = futureList == null || !futureList.Any() ? to : futureList.OrderBy(x => x.PayDate).FirstOrDefault().PayDate;
        //        periods = periods.Where(x => x.PayDate >= nearestDateFrom && x.PayDate <= nearestDateTo).ToList();
        //    }
        //    foreach (var p in periods)
        //    {
        //        //var total = actTotals.Where(x => x.paidDate == p.PayDate).Sum(x => x.total);
        //        periodSummary.Add(new PeriodSummary
        //        {
        //            PayDate = p.PayDate,
        //            DateFrom = p.DateFrom,
        //            DateTo = p.DateTo,
        //            PeriodNo = p.PeriodNo,
        //            DueDate = p.DueDate,
        //            Total = Convert.ToDecimal(0),
        //            PeriodYear = p.PeriodYear
        //        });
        //    }
        //    var data = Extensions.OrderByDynamic(periodSummary.AsQueryable(), order.Split(" ")[0], order.Split(" ")[1] != "asc").Skip(model.start).Take(model.length).Select(p => p).ToList();
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = periodSummary.Count,
        //        recordsTotal = 0, //db.Trn23110s.Count(x => x.TRN23100.ADM03400.Any(y => y.ContractorID == id)),
        //        data = data.ToList().Select(x => new
        //        {
        //            DT_RowId = x.PeriodYear + "-" + x.PeriodNo,
        //            PeriodNo = x.PeriodYear + "-" + x.PeriodNo,
        //            From = string.Format("{0:MMM. dd, yyyy}", x.DateFrom),
        //            To = string.Format("{0:MMM. dd, yyyy}", x.DateTo),
        //            DueDate = string.Format("{0:MMM. dd, yyyy}", x.DueDate),
        //            PayDate = string.Format("{0:MMM. dd, yyyy}", x.PayDate),
        //            Total = string.Format("{0:C}", x.Total)
        //        }).AsQueryable().ToStringArray()
        //    });
        //}
        ////[HttpPost]
        ////public JsonResult TechnicianOrders(DataTablesParam model)
        ////{
        ////    string order = Customs.GetSortString(model.order, model.columns);
        ////    string username = User.Identity.Name;
        ////    var data = db.Trn23100s.Where(x => x.Adm03400s.Any(y => y.Contractor.Email == username) || x.CreateByNavigation.UserName == username).Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        ////    if (!model.year.HasValue)
        ////        model.year = db.Trn23100s.Max(x => x.Requestdt).Year;
        ////    if (model.year != 0)
        ////        data = data.Where(x => x.Requestdt.Year == model.year);
        ////    if (model.additional != null && model.additional.ToString() != "a")
        ////        data = data.Where(x => x.Status == model.additional.ToString());
        ////    return Json(new DataTableReturn
        ////    {
        ////        draw = model.draw,
        ////        recordsFiltered = data.Count(),
        ////        recordsTotal = db.Trn23100s.Where(x => x.Adm03400s.Any(y => y.Contractor.Email == username) || x.CreateByNavigation.UserName == username).Count(),
        ////        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        ////        {
        ////            DT_RowId = x.Workid,
        ////            Title = x.WoTitle,
        ////            //Title = new HtmlString("<span" + (x.CompletionDt != null && x.CompletionDt.Value - x.Dispatchdt > new TimeSpan(4, 0, 0, 0,0) ? " style='color:red;'" : "") + ">" + x.Wo_title.ToUpper() + "</span>"),
        ////            RequestDate = string.Format("{0:dddd MMMM dd, yyyy}", x.Requestdt),
        ////            Status = DataDictionaries.WordOrderStatuses[x.Status],
        ////            Button = new HtmlString("<button class='manage-dets " + (x.Status == "i" || x.Status == "s" ? "view'><i class='fe-eye view'></i>" : "'><i class='fe-tools'></i>") +
        ////            "</button><button class='manage-dets tt-editw'><i class='fe-clipboard-1'></i></button>")
        ////        }).AsQueryable().ToStringArray()
        ////    });
        ////}
        ////[HttpPost]
        ////public JsonResult ContractorOrders(DataTablesParam model)
        ////{
        ////    string order = Customs.GetSortString(model.order, model.columns);
        ////    var pieces = model.additional.ToString().Split('|');
        ////    var status = pieces[0];
        ////    var conID = !string.IsNullOrEmpty(pieces[1]) ? Convert.ToInt32(pieces[1]) : db.Adm03300s.FirstOrDefault(x => x.Email == User.Identity.Name).ConId;
        ////    var data = db.Trn23100s.Where(x => x.Adm03400s.Any(y => y.Contractor.ConId == conID)).Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        ////    if (!model.year.HasValue)
        ////        model.year = db.Trn23100s.Max(x => x.Requestdt).Year;
        ////    if (model.year != 0)
        ////        data = data.Where(x => x.Requestdt.Year == model.year);
        ////    if (status != null && status != "a")
        ////        data = data.Where(x => x.Status == status);
        ////    return Json(new DataTableReturn
        ////    {
        ////        draw = model.draw,
        ////        recordsFiltered = data.Count(),
        ////        recordsTotal = db.Trn23100s.Where(x => x.Adm03400s.Any(y => y.Contractor.ConId == conID)).Count(),
        ////        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        ////        {
        ////            DT_RowId = DataDictionaries.WordOrderStatuses[x.Status] + "_" + x.Workid,
        ////            Title = x.WoTitle,
        ////            RequestDate = string.Format("{0:ddd. MMM. dd, yyyy}", x.Requestdt),
        ////            Status = new HtmlString("<i title='" + DataDictionaries.WordOrderStatuses[x.Status] + "' class='fe-flag-filled " + DataDictionaries.WordOrderStatuses[x.Status].ToLower() + "'></i>"),
        ////            IsLead = new HtmlString("<i class='fe-" + (x.Adm03400s.FirstOrDefault(y => y.ContractorId == conID).CrewLead ? "ok yes" : "cancel-1 no") + "'></i>"),
        ////            Button = new HtmlString("<button class='dt-details' title='Work Order'><i class='fe-news'></i></button>")
        ////        }).AsQueryable().ToStringArray()
        ////    });
        ////}
        //[HttpPost]
        //public JsonResult GetActivityAmount(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var p = (from a in db.Adm01150s
        //             join b in db.Adm01100s on a.RateId equals b.RateId
        //             select new { b.RateDescr, a.RateHistoryId, a.Amount, a.StartDate, a.EndDate, a.RateId }).ToList();
        //    string[] arr = model.additional.Split('|');
        //    int ID = Convert.ToInt32(arr[1]);
        //    var data = p.Where(x => x.RateId == ID).OrderByDescending(x => x.RateHistoryId).ToList();
        //    return Json(new DataTableReturn
        //    {
        //        data = data.Select(x => new
        //        {
        //            Name = x.RateDescr,
        //            Amount = x.Amount,
        //            StartDate = string.Format("{0:ddd. MMM. dd, yyyy}", x.StartDate),
        //            EndDate = string.Format("{0:ddd. MMM. dd, yyyy}", x.EndDate),
        //            Button = new HtmlString("<button class='AddClientRate' id='" + x.RateHistoryId.ToString() + "' title='Edit'><i class='fe-pencil-2'></i></button> <button class='dt-deleteClient' id='" + x.RateHistoryId.ToString() + "' title='Delete'><i class='fe-cancel'></i></button>")
        //        }).Skip(model.start).Take(model.length).AsQueryable().ToStringArray()
        //    });
        //}
        //[HttpPost]
        //public JsonResult GetActivityPaymnetAmount(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var p = (from a in db.Adm01250s
        //             join b in db.Adm01100s on a.RateId equals b.RateId
        //             select new { b.RateDescr, a.RateHistoryId, a.Amount, a.StartDate, a.EndDate, a.RateId }).ToList();
        //    string[] arr = model.additional.Split('|');
        //    int ID = Convert.ToInt32(arr[1]);//AddPaymentRate
        //    var data = p.Where(x => x.RateId == ID).OrderByDescending(x => x.RateHistoryId).ToList();
        //    return Json(new DataTableReturn
        //    {
        //        data = data.Select(x => new
        //        {
        //            Name = x.RateDescr,
        //            Amount = x.Amount,
        //            StartDate = string.Format("{0:ddd. MMM. dd, yyyy}", x.StartDate),
        //            EndDate = string.Format("{0:ddd. MMM. dd, yyyy}", x.EndDate),
        //            Button = new HtmlString("<button class='AddPaymentRate' id='" + x.RateHistoryId.ToString() + "' title='Edit'><i class='fe-pencil-2'></i></button> <button class='dt-deletePayment' id='" + x.RateHistoryId.ToString() + "' title='Delete'><i class='fe-cancel'></i></button>")
        //        }).Skip(model.start).Take(model.length).AsQueryable().ToStringArray()
        //    });
        //}
        //#endregion

        #region Administration Lists

        //[HttpPost]
        //public JsonResult Clients(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Adm03200s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //    {
        //        int statusID = Convert.ToInt32(model.additional);
        //        data = data.Where(x => x.CustClass == statusID);
        //    }
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Adm03200s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.CustId,
        //            Name = x.CustName,
        //            Phone = x.Phone1,
        //            Email = x.Email,
        //            Type = new HtmlString("<i title='" + DataDictionaries.ClientClass[x.CustClass] + "' class='fe-flag-filled " + DataDictionaries.ClientClass[x.CustClass].ToLower() + "'></i>"),
        //            Currency = x.Currency,
        //            Additional = new HtmlString("<div class='additional'><table>" +
        //                "<tr><td>Fax</td><td>" + EmptyDataFiller(x.Fax) + "</td></tr>" +
        //                "<tr><td>Alternate Phone</td><td>" + EmptyDataFiller(x.Phone2) + "</td></tr>" +
        //                "<tr><td>Address</td><td>" + EmptyDataFiller(Customs.MakeRealAddress(x.Street, x.City, x.Parish)) + "</td></tr>" +
        //            "</table></div>")
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult Contractors(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Adm03300s.Where(x => x.ConClass != 2 && x.ConClass != 3).Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //    {
        //        var conClass = Convert.ToInt32(model.additional);
        //        data = data.Where(x => x.ConClass == conClass);
        //    }
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Adm03300s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.ConId,
        //            FirstName = x.FirstName,
        //            LastName = x.LastName,
        //            Department = x.Department.Name.NormalizeSentence(true),
        //            PayScale = x.PayScale != null ? x.PayScale == 1 ? "Snr" : "Jr" : "Jr",
        //            Email = new HtmlString("<a href='mailto:" + x.Email + "'>" + x.Email + "</a>"),
        //            Level = new HtmlString("<span" + (x.ConClass == 3 ? " class='admin'" : x.ConClass == 1 ? " class='supv'" : x.ConClass == 2 ? " class='sadmin'" : "") + ">" + DataDictionaries.UserTypes[x.ConClass] + "</span>"),
        //            Additional = new HtmlString("<div class='additional'><table>" +
        //                "<tr><td>TRN</td><td>" + EmptyDataFiller(x.Trn) + "</td></tr>" +
        //                "<tr><td>Phone</td><td>" + EmptyDataFiller(x.Phone1) + " or " + EmptyDataFiller(x.Phone2) + "</td></tr>" +
        //                "<tr><td>Address</td><td>" + EmptyDataFiller(Customs.MakeRealAddress(x.Street, x.City, x.Parish)) + "</td></tr>" +
        //            "</table></div>")
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult WorkOrders(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Trn23100s.Where(x => x.ClassId != 18).Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (!model.year.HasValue)
        //        model.year = db.Trn23100s.Max(x => x.Requestdt).Year;
        //    if (model.year != 0)
        //        data = data.Where(x => x.Requestdt.Year == model.year);
        //    if (model.zone.HasValue)
        //        data = data.Where(x => x.Area.Zone.ZoneId == model.zone);
        //    if (model.clas.HasValue)
        //        data = data.Where(x => x.ClassId == model.clas);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //    {
        //        data = model.additional.ToString() == "s" ? data.Where(x => x.Status.ToLower() == "s" && x.DateSubmitted.HasValue && x.Submitted) :
        //            data.Where(x => x.Status.ToLower() == model.additional.ToString());
        //    }
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Trn23100s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = DataDictionaries.WordOrderStatuses[x.Status.ToLower()] + "_" + x.Workid,
        //            ReferenceNo = x.WoRef,
        //            Title = x.WoTitle.ToUpper(),
        //            //Title = new HtmlString("<span" + (x.CompletionDt != null && x.CompletionDt.Value - x.Dispatchdt > new TimeSpan(4, 0, 0, 0,0) ? " style='color:red;'" : "") + ">" + x.Wo_title.ToUpper() + "</span>"),
        //            Date = string.Format("{0:MMM dd, yyyy}", x.Requestdt),
        //            //Submitted = new HtmlString("<i class='fe-" + (x.Submitted ? "ok yes" : "cancel-1 no") + "'></i>"),
        //            Completiondt = x.CompletionDt != null ? string.Format("{0:MMM dd, yyyy}", x.CompletionDt.Value) : "",
        //            Status = new HtmlString("<i title='" + DataDictionaries.WordOrderStatuses[x.Status.ToLower()] + "' class='fe-flag-filled " + DataDictionaries.WordOrderStatuses[x.Status.ToLower()].ToLower() + "'></i>")
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult ServiceWorkOrders(DataTablesParam model)
        //{
        //    try
        //    {
        //        string order = Customs.GetSortString(model.order, model.columns);
        //        var data = db.Trn23100s.Where(x => x.ClassId == 18).Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //        if (model.additional != null && model.additional.ToString() != "a")
        //        {
        //            data = model.additional.ToString() == "s" ? data.Where(x => x.Status == "s" || x.Submitted && !new[] { "v", "i" }.Contains(x.Status)) :
        //                data.Where(x => x.Status == model.additional.ToString());
        //        }
        //        DataTableReturn dtr = new DataTableReturn
        //        {
        //            draw = model.draw,
        //            recordsFiltered = data.Count(),
        //            recordsTotal = db.Trn23100s.Where(x => x.ClassId == 18).Count(),
        //            data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //            {
        //                DT_RowId = DataDictionaries.WordOrderStatuses[x.Status] + "_" + x.Workid,
        //                ReferenceNo = x.WoRef,
        //                Title = x.WoTitle,
        //                Date = string.Format("{0:MMM dd, yyyy}", x.Requestdt),
        //                Submitted = new HtmlString("<i class='fe-" + (x.Submitted ? "ok yes" : "cancel-1 no") + "'></i>"),
        //                Status = new HtmlString("<i title='" + DataDictionaries.WordOrderStatuses[x.Status] + "' class='fe-flag-filled " + DataDictionaries.WordOrderStatuses[x.Status].ToLower() + "'></i>")
        //            }).AsQueryable().ToStringArray()
        //        };
        //        return Json(dtr);
        //    }
        //    catch { throw; }
        //}

        //public JsonResult MergedOrders(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Trn13120s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Trn13120s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.MergedOrderId,
        //            RefNum = x.MergedRefNum,
        //            Title = x.MergerdTitle,
        //            Date = string.Format("{0:MMM dd, yyyy}", x.MergerdDate),
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult Activities(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var clientCode = db.Adm03200s.ToDictionary(x => x.CustId, x => x.ClientCode);
        //    var data = db.Adm03200s.SelectMany(c => db.Adm01100s.Include(x => x.Department).Include(x => x.RateClassNavigation), (c, p) =>
        //    new
        //    {
        //        c = c,
        //        p = p
        //    }).Where(x => (x.c.CustId == x.p.ClientId)).Select(x => x.p);
        //    //var data = db.ADM01100.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    data = data.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.client.HasValue && model.client != 0)
        //        data = data.Where(x => x.ClientId == model.client);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //    {
        //        int depId = int.Parse(model.additional);
        //        data = data.Where(x => x.DepartmentId == depId);
        //    }
        //    int count = data.Count();
        //    if (order.Contains("ClientId"))
        //        data = data.OrderBy(x => x.ClientId).ThenBy(x => x.RateId).Skip(model.start).Take(model.length);
        //    else
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length);
        //    var asd = clientCode.ToList();
        //    var recs = data.ToList().Select(x => new
        //    {
        //        DT_RowId = x.RateId,
        //        Company = clientCode[x.ClientId],
        //        Department = x.Department.Name,
        //        Description = x.RateDescr,
        //        Unit = DataDictionaries.Units[x.RateUnit],
        //        //Amount = string.Format("{0:C}", x.RateAmount),
        //        Class = x.RateClassNavigation.ClassName,
        //        Active = new HtmlString("<i class='fe-" + (x.Active ? "ok yes" : "cancel-1 no") + "'></i>"),
        //        HasMaterials = new HtmlString("<i class='fe-" + (x.HasMaterials ? "ok yes" : "cancel-1 no") + "'></i>")
        //    }).AsQueryable().ToStringArray();
        //    var result = Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = count,
        //        recordsTotal = db.Adm01100s.Count(),
        //        data = recs
        //    });
        //    return result;
        //}

        ////[Authorize(Roles = "AppAdmin, SuperAdmin")]
        //[HttpPost]
        //public JsonResult Users(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var users = new List<ProfileFull>();
        //    List<string> usernames = System.Web.Security.Roles.GetUsersInRole("Admin").ToList();
        //    usernames.AddRange(System.Web.Security.Roles.GetUsersInRole("SuperAdmin").ToList());
        //    if (System.Web.Security.Roles.IsUserInRole(User.Identity.Name, "AppAdmin"))
        //        usernames.AddRange(System.Web.Security.Roles.GetUsersInRole("AppAdmin").ToList());
        //    usernames.AddRange(db.ADM03300.Select(x => x.Email).ToList());
        //    foreach (var username in usernames.Where(x => x != User.Identity.Name))
        //    {
        //        var user = Membership.GetUser(username);
        //        if (user == null) continue;
        //        if (System.Web.Security.Roles.IsUserInRole(username, "Technician") || System.Web.Security.Roles.IsUserInRole(username, "Supervisor"))
        //        {
        //            var u = db.ADM03300.FirstOrDefault(x => x.Email == username);
        //            if (u != null)
        //            {
        //                users.Add(new ProfileFull
        //                {
        //                    AltPhone = u.Phone2,
        //                    Email = user.Email,
        //                    UserName = username,
        //                    FirstName = u.FirstName,
        //                    LastName = u.LastName,
        //                    IsLocked = user.IsLockedOut,
        //                    IsApproved = user.IsApproved,
        //                    Phone = u.Phone1,
        //                    Role = System.Web.Security.Roles.GetRolesForUser(username)[0]
        //                });
        //            }
        //        }
        //        else
        //        {
        //            var profile = AppProfile.GetProfile(username).ProfileInfo;
        //            if (System.Web.Security.Roles.GetRolesForUser(username).Length > 0)
        //            {
        //                users.Add(new ProfileFull
        //                {
        //                    AltPhone = profile.AltPhone,
        //                    Email = user.Email,
        //                    UserName = username,
        //                    FirstName = profile.FirstName,
        //                    LastName = profile.LastName,
        //                    IsApproved = user.IsApproved,
        //                    IsLocked = user.IsLockedOut,
        //                    Phone = profile.Phone,
        //                    Role = System.Web.Security.Roles.GetRolesForUser(username)[0]
        //                });
        //            }
        //        }
        //    }
        //    var data = users.AsEnumerable().AsQueryable().Search(model.columns.Where(x => x.searchable).ToList(), model.search, false);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //        data = data.Where(x => x.Role == model.additional.ToString());
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = usernames.Count(x => x != User.Identity.Name),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.UserName,
        //            x.FirstName,
        //            x.LastName,
        //            Phone = EmptyDataFiller(x.Phone),
        //            Role = DataDictionaries.AllRoles[x.Role],
        //            IsApproved = new HtmlString("<button class='approver td-toggler" + (x.IsApproved ? " on" : "") + "'><span></span></button>"),
        //            IsLocked = new HtmlString("<button class='locker td-toggler" + (x.IsLocked ? " on" : "") + "'><span></span></button>"),
        //            Additional = new HtmlString("<div class='additional'><table>" +
        //                "<tr><td>Username</td><td>" + x.UserName + "</td></tr>" +
        //                "<tr><td>Email</td><td>" + EmptyDataFiller(x.Email) + "</td></tr>" +
        //                "<tr><td>Alternate Phone</td><td>" + EmptyDataFiller(x.AltPhone) + "</td></tr>" +
        //            "</table></div>")
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult Materials(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Adm13100s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //        data = data.Where(x => x.MaterialUnit == model.additional.ToString());
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Adm13100s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.MaterialId,
        //            Code = x.MaterialCode,
        //            Name = x.MaterialName,
        //            Unit = DataDictionaries.Units[x.MaterialUnit],
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult Classifications(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Adm03500s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Adm03500s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.ClassId,
        //            Name = x.ClassName,
        //            StartDate = string.Format("{0:dd/MM/yyyy}", x.StartDate),
        //            EndDate = string.Format("{0:dd/MM/yyyy}", x.EndDate)
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult Departments(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Adm04200s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.additional != null && model.additional != "a")
        //    {
        //        int statusID = Convert.ToInt32(model.additional);
        //        data = data.Where(x => x.DepartmentId == statusID);
        //    }
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Adm04200s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.DepartmentId,
        //            Name = x.Name
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult Deductions(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Adm04100s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //    {
        //        int statusID = Convert.ToInt32(model.additional);
        //        data = data.Where(x => x.DeductionId == statusID);
        //    }
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Adm04100s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.DeductionId,
        //            Name = x.Name
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult ContractorDeductions(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    int p_ConId = 0;// Convert.ToInt32(Session["p_ConId"]);
        //    var data = (from trn41 in db.Trn04100s
        //                join adm41 in db.Adm04100s on trn41.DeductionId equals adm41.DeductionId
        //                where trn41.ConductorId == p_ConId
        //                select new TRN04100Meta.TRN04100MetaData
        //                {
        //                    DeductionConductorID = trn41.DeductionConductorId,
        //                    ConductorID = trn41.ConductorId,
        //                    DeductionID = trn41.DeductionId,
        //                    Amount = trn41.Amount,
        //                    HoldIt = trn41.HoldIt,
        //                    //Periods = trn41.Periods,
        //                    Recurring = trn41.Recurring,
        //                    StartDate = trn41.StartDate,
        //                    EndDate = trn41.EndDate.Value,
        //                    YearToDateAmount = trn41.Ytdamount,
        //                    DeductionName = adm41.Name
        //                }).Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.additional != null && model.additional != "a")
        //    {
        //        int statusID = Convert.ToInt32(model.additional);
        //        data = data.Where(x => x.DeductionConductorID == statusID);
        //    }
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Trn04100s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.DeductionConductorID,
        //            DeductionName = x.DeductionName,
        //            Recurring = x.Recurring,
        //            HoldIt = x.HoldIt,
        //            Amount = x.Amount,
        //            StartDate = string.Format("{0:MMMM dd, yyyy}", x.StartDate),
        //            EndDate = string.Format("{0:MMMM dd, yyyy}", x.EndDate),
        //            YearToDateAmount = x.YearToDateAmount
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult Fixcodes(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.ADM06100.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //    {
        //        int statusID = Convert.ToInt32(model.additional);
        //        data = data.Where(x => x.FixID == statusID);
        //    }
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.ADM06100.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.FixID,
        //            FixName = x.FixName
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult Periods(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Adm16100s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //    {
        //        int statusID = Convert.ToInt32(model.additional);
        //        data = data.Where(x => x.PeriodYear == statusID);
        //    }
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Adm16100s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.PeriodId,
        //            week = x.PeriodYear + " - " + x.Week,
        //            periodStart = string.Format("{0:dddd MMMM dd, yyyy}", x.PeriodStart),
        //            periodEnd = string.Format("{0:dddd MMMM dd, yyyy}", x.PeriodEnd),
        //            dueDate = string.Format("{0:dddd MMMM dd, yyyy}", x.DueDate),
        //            payDate = string.Format("{0:dddd MMMM dd, yyyy}", x.PayDate)
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult vehicles(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Adm22100s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //    {
        //        int statusID = Convert.ToInt32(model.additional);
        //        data = data.Where(x => x.VehicleId == statusID);
        //    }
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Adm22100s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.VehicleId,
        //            PlateNo = x.PlateNo,
        //            FleetNo = x.FleetNo
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult Zones(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Adm26100s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //    {
        //        int statusID = Convert.ToInt32(model.additional);
        //        data = data.Where(x => x.ZoneId == statusID);
        //    }
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Adm04100s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.ZoneId,
        //            Name = x.Name,
        //            Supervisor = x.SupervisorName
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        [HttpPost]
        public JsonResult Areas(DataTablesParam model)
        {
            string order = Customs.GetSortString(model.order, model.columns);
            IQueryable<ADM01400> data = db.ADM01400.Include(p => p.ADM26100).Search(model.columns.Where(x => x.searchable).ToList(), model.search);
            if (model.additional != null && model.additional.ToString() != "a")
            {
                int statusID = Convert.ToInt32(model.additional);
                data = data.Where(x => x.areaID == statusID);
            }
            return Json(new DataTableReturn
            {
                draw = model.draw,
                recordsFiltered = data.Count(),
                recordsTotal = db.ADM04100.Count(),
                data = Extensions.OrderByDynamic(data, order.Split(" ")[0], order.Split(" ")[1] != "asc").Skip(model.start).Take(model.length).ToList().Select(x => new
                {
                    DT_RowId = x.areaID,
                    Zone = x.ADM26100.Name,
                    Name = x.areaName
                }).AsQueryable().ToStringArray()
            });
        }

        //[HttpPost]
        //public JsonResult POs(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Adm16200s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //    {
        //        int statusID = Convert.ToInt32(model.additional);
        //        data = data.Where(x => x.Poid == statusID);
        //    }
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Adm04100s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.Poid,
        //            PONUM = x.Ponum,
        //            Total = string.Format("{0:C}", x.Total),
        //            Balance = string.Format("{0:C}", x.Balance),
        //            Description = x.Description,
        //            IsClosed = new HtmlString("<i class='fe-" + (x.IsClosed ? "ok yes" : "cancel-1 no") + "'></i>")
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult Banks(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Adm02100s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //    {
        //        string statusID = model.additional;
        //        data = data.Where(x => x.BankId == statusID);
        //    }
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Adm02100s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.BankId,
        //            BankId = x.BankId,
        //            BankName = x.BankName
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult Branches(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.Adm02200s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //    {
        //        int statusID = Convert.ToInt32(model.additional);
        //        data = data.Where(x => x.RecId == statusID);
        //    }
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.Adm02200s.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.RecId,
        //            BankName = x.Bank.BankName,
        //            BranchId = x.BranchId,
        //            BranchName = x.BranchName
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        #endregion

        #region InvoiceList
        //[HttpPost]
        //public JsonResult Invoices(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.InvoiceList.Where(x => x.Status != "r" && !x.IsNewFormat).Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (!model.year.HasValue)
        //        model.year = db.TRN23100.Max(x => x.Requestdt).Year;
        //    if (model.year != 0)
        //        data = data.Where(x => x.InvoiceDate.Year == model.year);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //        data = model.additional.ToString() == "b" ? data.Where(x => x.BatchId.HasValue) : data.Where(x => x.Status == model.additional.ToString() && !x.BatchId.HasValue);
        //    var list = data.ToList();
        //    int tCount = list.Count();
        //    if (tCount < model.length)
        //        model.length = tCount;
        //    var dataList = data.ToList();
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = tCount,
        //        recordsTotal = db.InvoiceList.Count(),
        //        data = dataList.OrderBy(order).Skip(model.start).Take(model.length).AsEnumerable().Select(x => new
        //        {
        //            DT_RowId = x.InvoiceNum,
        //            InvoiceNo = Customs.MakeGenericInvoiceNo(x.InvoiceNum),
        //            Title = x.InvoiceTitle.ToUpper(),
        //            //Contractor = x.Contractor,//new HtmlString("<i title='" + DataDictionaries.InvoiceStatuses[x.BatchId.HasValue ? "b" : x.Status] + "' class='fe-flag-filled " + DataDictionaries.InvoiceStatuses[x.BatchId.HasValue ? "b" : x.Status].ToLower() + "'></i>"),
        //            Date = string.Format("{0:dd/MM/yyyy}", x.InvoiceDate),
        //            GCT = string.Format("{0:C}", x.GCT),
        //            SubTotal = string.Format("{0:C}", x.SubTotal),
        //            Total = string.Format("{0:C}", x.Total)
        //        }).AsQueryable().ToStringArray()
        //    });


        //}
        //[HttpPost]
        //public JsonResult NewInvoices(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.InvoiceList.Where(x => x.Status != "r" && x.IsNewFormat).Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (!model.year.HasValue)
        //        model.year = db.TRN23100.Max(x => x.Requestdt).Year;
        //    if (model.year != 0)
        //        data = data.Where(x => x.InvoiceDate.Year == model.year);
        //    if (model.additional != null && model.additional.ToString() != "a")
        //        data = model.additional.ToString() == "b" ? data.Where(x => x.BatchId.HasValue) : data.Where(x => x.Status == model.additional.ToString() && !x.BatchId.HasValue);
        //    var list = data.ToList();
        //    int tCount = list.Count();
        //    if (tCount < model.length)
        //        model.length = tCount;
        //    var dataList = data.ToList();
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = tCount,
        //        recordsTotal = db.InvoiceList.Count(),
        //        data = dataList.OrderBy(order).Skip(model.start).Take(model.length).AsEnumerable().Select(x => new
        //        {
        //            DT_RowId = x.InvoiceNum,
        //            InvoiceNo = Customs.MakeGenericInvoiceNo(x.InvoiceNum),
        //            Title = x.InvoiceTitle.ToUpper(),
        //            //Contractor = x.Contractor,//new HtmlString("<i title='" + DataDictionaries.InvoiceStatuses[x.BatchId.HasValue ? "b" : x.Status] + "' class='fe-flag-filled " + DataDictionaries.InvoiceStatuses[x.BatchId.HasValue ? "b" : x.Status].ToLower() + "'></i>"),
        //            Date = string.Format("{0:dd/MM/yyyy}", x.InvoiceDate),
        //            GCT = string.Format("{0:C}", x.GCT),
        //            SubTotal = string.Format("{0:C}", x.SubTotal),
        //            Total = string.Format("{0:C}", x.Total)
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        //[HttpPost]
        //public JsonResult Batches(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.BatchList.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (!model.year.HasValue)
        //        model.year = db.TRN23100.Max(x => x.Requestdt).Year;
        //    if (model.year != 0)
        //        data = data.Where(x => x.BatchDate.Year == model.year);
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.ADM02300.Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).ToList().Select(x => new
        //        {
        //            DT_RowId = x.BatchID,
        //            BatchNo = (x.BatchID * 100000).ToString("X"),
        //            BatchRange = db.TRN09100.Where(y => y.BatchId == x.BatchID).Min(y => y.InvoiceNum) + " - " + db.TRN09100.Where(y => y.BatchId == x.BatchID).Max(y => y.InvoiceNum),
        //            Date = string.Format("{0:dddd MMMM dd, yyyy}", x.BatchDate),
        //            InvoiceCount = x.Count,
        //            Total = string.Format("{0:C}", x.Total)
        //        }).AsQueryable().ToStringArray()
        //    });
        //}
        #endregion

        #region Report Lists
        //[HttpPost]
        //public JsonResult ContractorEarnings(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    var data = db.GetContractorEarnings().ToList().AsQueryable().Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = data.Count(),
        //        recordsTotal = db.GetContractorEarnings().Count(),
        //        data = data.OrderBy(order).Skip(model.start).Take(model.length).Select(x => new
        //        {
        //            DT_RowId = x.ConID,
        //            Name = x.Name,
        //            Amount = string.Format("{0:C}", x.Total)
        //        }).ToStringArray(),
        //        additional = new
        //        {
        //            tot = data.Sum(x => x.Total),
        //            subtot = data.OrderBy(order).Skip(model.start).Take(model.length).Sum(x => x.Total)
        //        }
        //    });
        //}
        #endregion

        #region List Functions
        public string EmptyDataFiller(string data)
        {
            return (string.IsNullOrEmpty(data) ? "<i class='fe-dot-3'></i>" : data);
        }
        #endregion

        #region Quotation

        //[HttpPost]
        //public JsonResult Quotations(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    IQueryable<Trn17100> data = db.Trn17100s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (!model.year.HasValue)
        //        model.year = DateTime.Now.Year;
        //    if (model.year != 0)
        //        data = data.Where(x => x.Requestdt.Year == model.year);
        //    var list = data.ToList();
        //    int tCount = list.Count;
        //    if (tCount < model.length)
        //        model.length = tCount;
        //    var dataList = data.ToList();
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = tCount,
        //        recordsTotal = db.Trn17100s.Count(),
        //        data = dataList.OrderBy(order).Skip(model.start).Take(model.length).AsEnumerable().Select(x => new
        //        {
        //            DT_RowId = x.QuotationId,
        //            ReferenceNo = x.QuotRef,
        //            Title = x.QuotTitle,
        //            Date = string.Format("{0:dd/MM/yyyy}", x.Requestdt),
        //            Requestby = x.Requestby
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        #endregion

        #region StandbyInvoice

        //[HttpPost]
        //public JsonResult StandbyInvoices(DataTablesParam model)
        //{
        //    string order = Customs.GetSortString(model.order, model.columns);
        //    IQueryable<Trn19100> data = db.Trn19100s.Search(model.columns.Where(x => x.searchable).ToList(), model.search);
        //    if (!model.year.HasValue)
        //        model.year = DateTime.Now.Year;
        //    if (model.year != 0)
        //        data = data.Where(x => x.Requestdt.Year == model.year);
        //    var list = data.ToList();
        //    int tCount = list.Count;
        //    if (tCount < model.length)
        //        model.length = tCount;
        //    var dataList = data.ToList();
        //    return Json(new DataTableReturn
        //    {
        //        draw = model.draw,
        //        recordsFiltered = tCount,
        //        recordsTotal = db.Trn19100s.Count(),
        //        data = dataList.OrderBy(order).Skip(model.start).Take(model.length).AsEnumerable().Select(x => new
        //        {
        //            DT_RowId = x.SinvoiceId,
        //            ReferenceNo = Customs.MakeGenericInvoiceNo(x.SinvoiceId),
        //            Title = x.SinvTitle,
        //            Date = string.Format("{0:dd/MM/yyyy}", x.Requestdt),
        //            Zone = x.Zone.Name
        //        }).AsQueryable().ToStringArray()
        //    });
        //}

        #endregion
    }
}