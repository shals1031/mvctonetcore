using System;

namespace TeliconLatest.Models
{
    public class TechnicianStat
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string ID { get; set; }
        public int Total { get; set; }
        public int Invoiced { get; set; }
        public int Processing { get; set; }
        public int Submitted { get; set; }
        public int New { get; set; }
        public bool HasSettings { get; set; }
    }
    public class Period
    {
        public DateTime DueDate { get; set; }
        public DateTime PayDate { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int PeriodNo { get; set; }
        public int PeriodYear { get; set; }
    }
    public class PeriodSummary:Period
    {
        public decimal Total { get; set; }
    }
    public class StatementDetail
    {
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Cost { get; set; }
        public decimal Quantity { get; set; }
    }
    public class PeriodDetail
    {
        public decimal Quantity { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public double Rate { get; set; }
        public double Amount { get; set; }
        public double Penalty { get; set; }
        public double Total { get; set; }
    }
}