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
}