using System;
using System.Collections.Generic;

namespace TeliconLatest.Models
{
    public class ClientData
    {
        public string Address { get; set; }
        public string Attention { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Currency { get; set; }
        public int InvoiceNo { get; set; }
        public string User { get; set; }
        public string ReferenceNo { get; set; }
        public string GCTNo { get; set; }
        public DateTime Date { get; set; }
        public string PONo { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Region { get; set; }
    }
    public class ClientInvoiceActivities
    {
        public decimal ActivityQty { get; set; }
        public DateTime ActivityDate { get; set; }
        public string ActivityLocation { get; set; }
        public string ActivityDesc { get; set; }
        public double ActivityCost { get; set; }
        public string Comments { get; set; }
        public string AltCode { get; set; }
        public string RefNum { get; set; }
        public int ActId { get; set; }
        public DateTime WOSubmittedDate { get; set; }
        public string WOTitle { get; set; }
        public int WoId { get; set; }
        public string TeamName { get; set; }
    }

    public class ClientInvoiceActivityMaterial
    {
        public decimal MatQty { get; set; }
        public DateTime ActivityDate { get; set; }
        public string ActivityLocation { get; set; }
        public string MatDesc { get; set; }
        public double MatCost { get; set; }
        public string Comments { get; set; }
    }

    public class ClientInvoice
    {
        public DateTime Date { get; set; }
        public ClientData Client { get; set; }
        public decimal GCT { get; set; }
        public string Title { get; set; }
        public List<ClientInvoiceActivities> Activities { get; set; }
        public List<ClientInvoiceActivityMaterial> Materials { get; set; }
    }
    public class SummaryList
    {
        public int InvoiceNo { get; set; }
        public int InvoiceID { get; set; }
        public string RefNo { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
    }
    public class SummaryPrintOut
    {
        public int InvoiceID { get; set; }
        public List<string> Contractors { get; set; }
        public string RefNo { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public decimal GCT { get; set; }
        public decimal GrandTotal { get; set; }
        public bool IsInCategory { get; set; }
    }
    public class SummaryPrintOutWithClient
    {
        public ClientData Client { get; set; }
        public List<SummaryPrintOut> PrintData { get; set; }
    }
    public class InvoiceBreakDown
    {
        public string Title { get; set; }
        public string RefNo { get; set; }
        public string PONo { get; set; }
        public string RequestedBy { get; set; }
        public DateTime DateRequested { get; set; }
        public DateTime DateDispatched { get; set; }
        public List<ShortActivity> Activities { get; set; }
    }
    public class ShortActivity
    {
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
        public decimal Qty { get; set; }
    }
    public class SummaryModelForDay : DataTablesParam
    {
        public int clientID { get; set; }
        public DateTime date { get; set; }
    }
    public class SummaryModelForRange : DataTablesParam
    {
        public int clientID { get; set; }
        public string to { get; set; }
        public string from { get; set; }
    }

    public class InvoiceExportToExcelModel
    {
        public int from { get; set; }
        public int to { get; set; }
    }
    public class InvoiceSummaryData
    {
        public int InvoiceID { get; set; }
        public int InvoiceNo { get; set; }
        public int WOID { get; set; }
        public string PONum { get; set; }
        public string RefNo { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public List<InvoiceSummaryItem> Items { get; set; }
    }
    public class InvoiceSummaryItem
    {
        public int WOID { get; set; }
        public int ActId { get; set; }
        public string ActDescr { get; set; }
        public string ActCode { get; set; }
        public string Loc { get; set; }
        public string LocDescr { get; set; }
        public decimal Qty { get; set; }
    }
}