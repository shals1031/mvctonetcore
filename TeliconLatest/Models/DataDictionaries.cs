using System.Collections.Generic;

namespace TeliconLatest.Models
{
    public static class DataDictionaries
    {
        public static Dictionary<string, string> TeliconCodes = new Dictionary<string, string>()
        {
            { "100", "OK" },
            { "200", "Error Saving" },
            { "300", "Error Updating" },
            { "400", "Error Deleting" },
            { "600", "Error Retrieving List" },
            { "700", "Error Loading Page" },
            { "800", "General Error" }
        };
        public static Dictionary<int, string> ClientClass = new Dictionary<int, string>()
        {
            { 0, "Regular" },
	        { 1, "Special" }
        };
        public static Dictionary<string, string> WordOrderStatuses = new Dictionary<string, string>()
	    {
            { "a", "All" },
	        { "n", "New" },
            { "s", "Submitted" },
            { "p", "Processing" },
            { "v", "Verified" },
            { "l", "Locked" },
            { "d", "Details Added" },
            { "i", "Invoiced" }
	    };
        public static Dictionary<string, string> InvoiceStatuses = new Dictionary<string, string>()
	    {
            { "o", "OK" },
            { "b", "Batched" },
	        { "r", "Reversed" },
            { "c", "Cancelled" }
	    };
        public static Dictionary<int, string> UserTypes = new Dictionary<int, string>()
        {
            { 0, "Technician" },
            { 1, "Supervisor" }
        };
        public static Dictionary<string, string> AdminRoles = new Dictionary<string, string>()
        {
            {"Admin", "Reg. Administrator" },
            {"SuperAdmin", "Head Administrator" },
            {"AppAdmin", "Application Administrator" },
        };
        public static Dictionary<string, string> AllRoles = new Dictionary<string, string>()
        {
            { "Admin", "Reg. Administrator" },
            { "SuperAdmin", "Head Administrator" },
            { "AppAdmin", "Application Administrator" },
            { "Technician" , "Technician" },
            { "Supervisor", "Supervisor" },
            { "Accounts", "Accounts" },
            { "Flow CTO", "Flow CTO" },
            { "HR & Admin", "HR & Admin" },
            { "Manager", "Manager" }
        };
        public static Dictionary<string, string> Parishes = new Dictionary<string, string>()
        {
            { "KGN", "Kingston" },
	        { "HAN", "Hanover" },
            { "AND", "St. Andrew" },
            { "MAN", "Manchester" },
            { "POR", "Portland" },
            { "ANN", "St. Ann" },
            { "CAT", "St. Catherine" },
            { "ELI", "St. Elizabeth" },
	        { "JAM", "St. James" },
            { "MAR", "St. Mary" },
            { "THO", "St. Thomas" },
            { "TRE", "Trelawny" },
            { "WML", "Westmoreland" },
            { "CLA", "Clarendon" }
        };
        public static Dictionary<string, string> Units = new Dictionary<string, string>()
        {
            { "EA", "Each" },
            { "PC", "Piece" },
            { "HR", "Per Hour" },
            { "WK", "Per Week" },
            { "FN", "Per Fortnight" },
            { "MT", "Per Month" },
            { "SM", "Per Sqaure Meter" },
            { "PF", "Per Foot" }
        };
        public static Dictionary<int, string> Months = new Dictionary<int, string>()
        {
            { 1, "January" },
            { 2, "February" },
            { 3, "March" },
            { 4, "April" },
            { 5, "May" },
            { 6, "June" },
            { 7, "July" },
            { 8, "August" },
            { 9, "September" },
            { 10, "October" },
            { 11, "November" },
            { 12, "December" }
        };
        public static Dictionary<int, string> Quarters = new Dictionary<int, string>()
        {
            { 3, "1st Quarter" },
            { 6, "2nd Quarter" },
            { 9, "3rd Quarter" },
            { 12, "4th Quarter" },
        };
        public static List<string> Relatives = new List<string>
        {
            "Wife",
            "Husband",
            "Son",
            "Daughter",
            "Grandson",
            "Granddaughter",
            "Mother",
            "Father",
            "Brother",
            "Sister",
            "Aunt",
            "Uncle",
            "Grandmother",
            "Grandfather",
            "Cousin",
            "Niece",
            "Nephew",
            "Other"
        };
        public static Dictionary<int, string> OwnerTypes = new Dictionary<int, string>()
        {
            { 1, "Company" },
            { 2, "Contractor" },
            { 3, "Lease" }
        };

        public static Dictionary<int, string> Tasks = new Dictionary<int, string>()
        {
            {1, "Roles"},
            {2, "Users"},
            {3, "Clients"},
            {4, "Contractors"},
            {5, "Activities"},
            {6, "Materials"},
            {7, "Classifications"},
            {8, "Departments"},
            {9, "Deductions"},
            {10, "Periods"},
            {11, "Vehicles"},
            {12, "Zones"},
            {13, "Areas"},
            {14, "POs"},
            {15, "Work Orders"},
            {16, "Work Order Invoices"},
            {17, "Invoice Summary"},
            {18, "Batches"},
            {19, "Print Multiple Invoices"},
            {20, "Quotation"},
            {21, "Invoice Export to Excel"},
            {22, "Contractor Earnings"},
            {23, "Work Order Splits"},
            {24, "Payment Detail"},
            {25, "Invoice By Category"},
            {26, "Dispatched WO"},
            {27, "Annual Contractor Payment"},
            {28, "Contractor Bank Payments"},
            {29, "Contractor Pay Slip"},
            {30, "PO Tracking"},
            {31, "PO Summary"},
            {32, "Material Usage"},
            {33, "Import Data"},
            {34, "Export Data"},
            {35, "Backup"},
            {36, "Restore"},
            {37, "Bank"},
            {38, "Branch"},
            {39, "Invoice Summary Report"},
            {40, "Annual Payroll"},
            {41, "Standby Invoice"}
        };
    }
}