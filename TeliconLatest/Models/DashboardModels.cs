using System;
using System.Collections.Generic;

namespace TeliconLatest.Models
{
    public class WorkOrderSummary
    {
        public int Invoiced { get; set; }
        public int New { get; set; }
        public int Submitted { get; set; }
        public int Verified { get; set; }
        public int Processing { get; set; }
        public int Contractors { get; set; }
        public int Locations { get; set; }
        public int Activities { get; set; }
        public List<RevenueVsIncomeChartData> RevenueIncome { get; set; }
    }
    public class RevenueVsIncomeForGroup : RevenueVsIncome
    {
        public DateTime Date { get; set; }
    }
    public class RevenueVsIncome
    {
        public double? Payments { get; set; }
        public double? Revenue { get; set; }
    }
    public class RevenueVsIncomeChartData : RevenueVsIncome
    {
        public string Period { get; set; }
    }
    public class TechnicianStatementDetail
    {
        public string Wo_title { get; set; }
        public string Location { get; set; }
        public DateTime ActDate { get; set; }
        public string RateUnit { get; set; }
        public DateTime? PaidDate { get; set; }
        public int ActivityID { get; set; }
        public decimal OActQty { get; set; }
        public string RateDescr { get; set; }
        public double Cost { get; set; }
        public double? PenalizeAmount { get; set; }
        public double PenalizeCost { get; set; }
        public bool AddMaterial { get; set; }
        public int Penalty { get; set; }
    }
}