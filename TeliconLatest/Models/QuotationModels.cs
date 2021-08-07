using System;
using System.Collections.Generic;

namespace TeliconLatest.Models
{
    public class RequesterData
    {
        public DateTime Date { get; set; }
        public string ReferenceNo { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ClientName { get; set; }        
    }
    public class SInvoiceRequesterData
    {
        public DateTime Date { get; set; }
        public string ReferenceNo { get; set; }
        public string Attn { get; set; }
        public string Addr { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ClientName { get; set; }
        public string PreparedBy { get; set; }
    }
    public class QuotationActivities
    {
        public decimal ActivityQty { get; set; }
        public string ActivityDesc { get; set; }
        public double ActivityCost { get; set; }
    }
    public class Quotation
    {
        public RequesterData Client { get; set; }        
        public string Title { get; set; }
        public List<QuotationActivities> Activities { get; set; }
    }

    public class SInvoiceActivities
    {
        public decimal ActivityQty { get; set; }
        public int ActivityId { get; set; }
        public string ActivityDesc { get; set; }
        public double ActivityCost { get; set; }
    }

    public class SInvoice
    {
        public SInvoiceRequesterData Client { get; set; }
        public double GCTRate { get; set; }
        public string Title { get; set; }
        public List<SInvoiceActivities> Activities { get; set; }
    }
}