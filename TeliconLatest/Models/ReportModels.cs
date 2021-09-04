using System;

namespace TeliconLatest.Models
{

    public partial class PaymentDetailStatement
    {
        public long? RN { get; set; }
        public int Workid { get; set; }
        public string Wo_ref { get; set; }
        public string Wo_title { get; set; }
        public DateTime Dispatchdt { get; set; }
        public string Status { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? dateVerified { get; set; }
        public int ConID { get; set; }
        public string Name { get; set; }
    }

    public partial class CategoryInvoiceStatement
    {
        public decimal? TotalAmount { get; set; }
        public string Category { get; set; }
        public int ClassID { get; set; }
    }

    public partial class CategoryInvoiceStatementInvoiceSummary
    {
        public int InvoiceNum { get; set; }
        public string InvoiceTitle { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal? Total { get; set; }
        public string Wo_ref { get; set; }
        public string Category { get; set; }
        public int ClassID { get; set; }
        public double? GCT { get; set; }
        public string Contractor { get; set; }
    }
    public partial class DispatchWOStatementReport
    {
        public string Wo_ref { get; set; }
        public string Wo_title { get; set; }
        public DateTime Dispatchdt { get; set; }
        public bool SpliceDocs { get; set; }
        public string Name { get; set; }
        public long? RN { get; set; }
        public int ConID { get; set; }
    }

    public partial class ContractorBankPaymentsReport
    {
        public int ContractorID { get; set; }
        public double? Total { get; set; }
        public double? PenalizeTotal { get; set; }
    }

}
