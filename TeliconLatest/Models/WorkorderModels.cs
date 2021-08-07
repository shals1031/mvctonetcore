using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TeliconLatest.Models
{
    public class ConstructorPartial{
        public int WorkOrderId { get; set; }
        public int InvoiceId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public List<DateTime> Dates { get; set; }
        public List<SelectListItem> InvoiceNos { get; set; }
        public string[] UserRole { get; set; }
    }
    public class MatConstructorPartial
    {
        public int WOActId { get; set; }
        public int ActivityId { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public List<ConstructorActivity> ConstructorActivities { get; set; }
        public string[] UserRole { get; set; }
    }
    public class ConstructorActivity
    {
        public decimal ActivityAmount { get; set; }
        public int ActivityID { get; set; }
        public bool HasMaterials { get; set; }
        public bool AddMaterial { get; set; }
        public int RID { get; set; }
        public decimal MaxQty { get; set; }
        public bool CapturedMaterials { get; set; }
        public bool MatsReq { get; set; }
    }
    public class ConstructorActivityDisplay
    {
        public string Description { get; set; }
        public int ActivityID { get; set; }
        public decimal MaxQty { get; set; }
    }
    public class WorkOrderActivityMetarial
    {
        public int RID { get; set; }
        public int MetarialId { get; set; }
        public decimal Amount { get; set; }
    }
    public class WorkOrderActivity
    {
        public int RID { get; set; }
        public int RecID { get; set; }
        public decimal Amount { get; set; }
        public bool HasMetarials { get; set; }
        public List<WorkOrderActivityMetarial> Metarials { get; set; }
    }
    public class WorkOrderRecordSet
    {
        public int WID { get; set; }
        public int? InvID { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string AdtnlDetails { get; set; }
        public List<WorkOrderActivity> Activities { get; set; }
    }

    public class ConstructorQuotationActivity
    {
        public decimal ActivityAmount { get; set; }
        public string ActivityDesc { get; set; }
        public int RecID { get; set; }
        public double Rate { get; set; }
    }

    public class ConstructorSInvoiceActivity
    {
        public decimal ActivityAmount { get; set; }
        public int ActivityId { get; set; }
        public int RecID { get; set; }
        public double Rate { get; set; }
    }

    public class QuotationActivity
    {        
        public int RecID { get; set; }
        public string ActDescr { get; set; }
        public decimal Qty { get; set; }
        public double Rate { get; set; }
    }
    public class SInvoiceActivity
    {
        public int RecID { get; set; }
        public int ActId { get; set; }
        public decimal Qty { get; set; }
        public double Rate { get; set; }
    }
    public class QuotationRecordSet
    {
        public int QID { get; set; }
        public List<QuotationActivity> Activities { get; set; }
    }

    public class SInvoiceRecordSet
    {
        public int SIID { get; set; }
        public List<SInvoiceActivity> Activities { get; set; }
    }

    public class TeamMember
    {
        public double? Portion { get; set; }
        public string ImgUrl { get; set; }
        public string Name { get; set; }
        public int ID { get; set; }
        public bool IsLead { get; set; }
        public string PlateNo { get; set; }
        public int PayScale { get; set; }
        public string PayScaleName { get; set; }
        public bool IsHide { get; set; }
        public string TechNumber { get; set; }
    }
    public class ContractorOnTeam
    {
        public int ID { get; set; }
        public double Share { get; set; }
        public bool IsLead { get; set; }
    }
    public class TeamForUpdate
    {
        public List<ContractorOnTeam> Team { get; set; }
        public int WID { get; set; }
    }
    public class WorkOrderPartial
    {
        public int WOId { get; set; }
        public string Title { get; set; }
        public double Total { get; set; }
        public string Status { get; set; }
        public bool IsMerged { get; set; }
    }
    public class WorkOrderReferencePartial
    {
        public string ReferenceCode { get; set; }
        public int Count { get; set; }
    }
    public class WorkOrderSplitFilter
    {
        public int companyId { get; set; }
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }
        public double? managementSplitTo { get; set; }
        public double? managementSplitFrom { get; set; }
        public double? contractorSplitTo { get; set; }
        public double? contractorSplitFrom { get; set; }
    }
    public class WorkOrderSplitData
    {
        public string referenceCode { get; set; }
        public string name { get; set; }
        public double managementSplit { get; set; }
        public double contractorSplit { get; set; }
        public List<WorkOrderSplitTeamData> team { get; set; }
    }
    public class WorkOrderSplitTeamData
    {
        public string fullName { get; set; }
        public double split { get; set; }
    }
    public class ContractorStatementReport
    {
        public string EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }
        public string Level { get; set; }
        public double Total { get; set; }
        public double PenalizeTotal { get; set; }
        public bool IsEarning { get; set; }
    }
    public class ContractorPaySlipReport
    {
        public string EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }
        public string Level { get; set; }
        public double Total { get; set; }
        public double PenalizeTotal { get; set; }
        public bool IsEarning { get; set; }
        public int ConId { get; set; }
        public DateTime PayDate { get; set; }
        public string BankAcc { get; set; }
        public DateTime? EngagementDt { get; set; }
        public string TRN { get; set; }
        public string NIS { get; set; }
        public string ContNo { get; set; }
    }
    public class YTDPaymentReport
    {
        public string EmployeeID { get; set; }
        public int ConId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? PaidDate { get; set; }
        public double Total { get; set; }
    }
    public class YTDPeriodDetails
    {
        public int ContractorID { get; set; }
        public DateTime PaidDate { get; set; }
        public int Workid { get; set; }
        public string Wo_ref { get; set; }
        public string Wo_title { get; set; }
        public double PenalizeAmount { get; set; }
        public double Total { get; set; }
        public string PeriosName { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class RoleTaskList
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public bool IsSelected { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
    }

    public class ActMaterialList
    {
        public int ActMatId { get; set; }
        public int MatId { get; set; }
        public string MatName { get; set; }
        public int Qty { get; set; }
        public bool IsSelected { get; set; }
    }

    public class DepartmentActivityList
    {
        public int DepActId { get; set; }
        public int ActId { get; set; }
        public string Client { get; set; }
        public string ActName { get; set; }
        public bool IsSelected { get; set; }
    }

    public class ContractorStatmentsFilter
    {
        public string sortBy { get; set; }
        public string direction { get; set; }
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }
        public int? periodStart { get; set; }
        public int? periodStartYear { get; set; }
        public int? periodEnd { get; set; }
        public int? periodEndYear { get; set; }
    }

    public  class PaymentDetailFilter
    {
        public string sortBy { get; set; }
        public string direction { get; set; }
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }
    }

    public class CategoryInvoiceStatmentFilter
    {
        public string sortBy { get; set; }
        public string direction { get; set; }
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }
        public string CategorySelection { get; set; }        
    }
    public class CategorInvoiceStatmentInvoiceSummaryFilter
    {
        public DateTime fromDate { get; set; }
        public DateTime toDate { get; set; }
        public int classId { get; set; }        
    }
    public class DispatchedWOStatmentFilter
    {
        public string sortBy { get; set; }
        public string direction { get; set; }
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }        
    }

    public class ClientInvoiceFilter
    {
        public int InvoiceNo { get; set; }
        public bool HasBreakDown { get; set; }
    }

    public class QuotationFilter
    {
        public int id { get; set; }
    }

    public class SInvoiceFilter
    {
        public int id { get; set; }
    }

    public class YTDPaymentsFilter
    {
        public int? periodStartYear { get; set; }
        public int? periodEndYear { get; set; }
    }

    public class YTDPaymentsPeriodDetailFilter
    {
        public DateTime fromDate { get; set; }
        public DateTime toDate { get; set; }
        public int ConId { get; set; }
    }

    public class StatementDetailFilter
    {
        public string pdate { get; set; }
        public int id { get; set; }
    }
    public class PeriodDetailFilter
    {
        public string pdate { get; set; }
        public int id { get; set; }
    }

    public class POTrackingFilter
    {
        public string pONum { get; set; }
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }
    }

    public class POSummaryFilter
    {
        public int? periodStart { get; set; }
        public int? periodStartYear { get; set; }
        public int? periodEnd { get; set; }
        public int? periodEndYear { get; set; }
    }

    public class POSummaryDetaisFilter
    {
        public int? yearFrom { get; set; }
        public int? yearTo { get; set; }
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }
        public string poNum { get; set; }
    }

    public class MaterialUsageFilter
    {
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }
    }

    public class MaterialUsageDetailFilter
    {
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }
        public int matId { get; set; }
    }


    public class NewInvoiceFilter
    {
        public int client { get; set; }
        public string po { get; set; }
        public int? classifi { get; set; }
        public int? region { get; set; }
        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }
    }

    public class QConstructorPartial
    {
        public int QuotationId { get; set; }
        public string Title { get; set; }
        public List<ConstructorQuotationActivity> QConstructorActivities { get; set; }
        public string[] UserRole { get; set; }
    }

    public class SIConstructorPartial
    {
        public int SInvoiceId { get; set; }
        public string Title { get; set; }
        public List<ConstructorSInvoiceActivity> SIConstructorActivities { get; set; }
        public string[] UserRole { get; set; }
    }

    public class ServiceWorkOrder
    {
        public int WID { get; set; }
        [Required]
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        [Required]
        [Display(Name = "Location")]
        public int LocationID { get; set; }
        [Required(ErrorMessage = "Please enter a reference")]
        [StringLength(20)]
        [Display(Name = "Reference")]
        public string Reference { get; set; }
        //[Required(ErrorMessage = "Please enter a Title")]
        //[StringLength(150)]
        //[Display(Name = "Title")]
        //public string Title { get; set; }
        [Required]
        [Display(Name = "Area")]
        public int AreaID { get; set; }
        [Required]
        [Display(Name = "Classification")]
        public int ClassificationID { get; set; }
        [Required]
        [Display(Name = "Client")]
        public int Wo_client { get; set; }

        public List<TeamMember> TeamMembers { get; set; }
        [Required]
        [Display(Name = "Own Vehicle")]
        public int OwnVehicle { get; set; }
        [Required]
        [Display(Name = "Customer Account Number")]
        [StringLength(7)]
        public string CustomerAccount { get; set; }
        [Required]
        [Display(Name = "Customer Name")]
        [StringLength(35)]
        public string CustomerName { get; set; }
        [Display(Name = "Customer Address 1")]
        [StringLength(35)]
        public string CustomerAddress1 { get; set; }
        [Display(Name = "Customer Address 2")]
        [StringLength(35)]
        public string CustomerAddress2 { get; set; }
        [Required]
        [Display(Name = "Customer Type")]
        public string CustomerType { get; set; }
        [Required]
        [Display(Name = "Service")]
        public int Service { get; set; }
        [Required]
        [Display(Name = "Service Type")]
        public int ServiceType { get; set; }

        [Display(Name = "Watch")]
        public int Watch { get; set; }
        [Display(Name = "Talk")]
        public int Talk { get; set; }
        [Display(Name = "Click")]
        public int Click { get; set; }

        [Display(Name = "QA100")]
        public int QA100 { get; set; }
        [Display(Name = "Working")]
        public int Working { get; set; }
        [Display(Name = "Installation")]
        public bool Installation { get; set; }
        [Display(Name = "Rewire Cable")]
        public bool RewireCable { get; set; }
        [Display(Name = "Rewire Outlets")]
        public bool RewireOutlets { get; set; }
        [Display(Name = "Service Calls")]
        public bool ServiceCalls { get; set; }
        [Display(Name = "Extra Outlets")]
        public bool ExtraOutlets { get; set; }
        [Display(Name = "Non Standard")]
        public bool NonStandard { get; set; }
        [Display(Name = "Not Done")]
        public bool Rejected { get; set; }
        public System.Guid CreateBy { get; set; }
        [Display(Name = "Vehicle")]
        [Required(ErrorMessage = "Please select a vehicle")]
        public string PlateNo { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.Time)]
        [Display(Name = "In Time", Description = "Number of Hours and Minutes after Midnight Central Timezone")]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm\:ss}", ApplyFormatInEditMode = true)]
        public TimeSpan LogIn { get; set; }
        [Display(Name = "In Time")]
        [RegularExpression(@"^([0-1]\d|2[0-3]):([0-5]\d)(:([0-5]\d))?$", ErrorMessage = "Invalid Time")]
        public string LogInValue {
            get
            {
                return LogIn.ToString(@"hh\:mm\:ss");
            }
            set
            {
                LogIn = TimeSpan.Parse(value);
            }
        }
        [DataType(System.ComponentModel.DataAnnotations.DataType.Time)]
        [Display(Name = "Out Time", Description = "Number of Hours and Minutes after Midnight Central Timezone")]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm\:ss}", ApplyFormatInEditMode = true)]
        public TimeSpan LogOut { get; set; }
        [Display(Name = "Out Time")]
        [RegularExpression(@"^([0-1]\d|2[0-3]):([0-5]\d)(:([0-5]\d))?$", ErrorMessage = "Invalid Time")]
        public string LogOutValue
        {
            get
            {
                return LogOut.ToString(@"hh\:mm\:ss");
            }
            set
            {
                LogOut = TimeSpan.Parse(value);
            }
        }
        public int RG6M { get; set; }
        public int RG6I { get; set; }
        public int RG11 { get; set; }
        [Display(Name = "Remarks")]
        public string Remarks { get; set; }
        public int RemarksId { get; set; }
    }
    public class SelectListItemExt : SelectListItem
    {
        public string term { get; set; }
        public int id { get; set; }
        public string imgUrl { get; set; }
        public string payScale { get; set; }
        public string PlateNo { get; set; }
    }

    public class ContractorsDailyInstallationFilter
    {
        public DateTime? FilterDate { get; set; }
        public int? ContrctorId { get; set; }
    }
    public class DailyWorkOrderData
    {
        public List<ContractorsDailyInstallationData> orders { get; set; }
        public List<Compensation> Compensations { get; set; }
    }
    public class ContractorsDailyInstallationData
    {
        public DateTime InstallationDate { get; set; }
        public int ContractorId { get; set; }
        public string ContractorName { get; set; }
        public string TechNo { get; set; }
        public string VehicleRegistration { get; set; }
        public double Price { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ReturnTime { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string NE { get; set; }
        public int ServiceInstalledW { get; set; }
        public int ServiceInstalledT { get; set; }
        public int ServiceInstalledC { get; set; }
        public int ServiceInstalledND { get; set; }
        public int QA100 { get; set; }
        public int Working { get; set; }
        public TimeSpan InTime { get; set; }
        public TimeSpan OutTime { get; set; }
        public int RG6M { get; set; }
        public int RG6I { get; set; }
        public int RG11 { get; set; }
        public string Remarks { get; set; }
    }
    public class Compensation
    {
        public DateTime InstallationDate { get; set; }
        public int ContractorId { get; set; }
        public int WorkOrderId { get; set; }
        public int AccountNumber { get; set; }
        public bool Installation { get; set; }
        public int Service { get; set; }
        public bool RewireCable { get; set; }
        public bool RewireOutlets { get; set; }
        public bool ServiceCalls { get; set; }
        public bool ExtraOutlets { get; set; }
        public bool NonStandard { get; set; }
        public bool Rejected { get; set; }
        public double Total { get; set; }
    }

    public class WorkOrderSummaryFilter
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
    public class WorkOrderSummaryData
    {
        public DateTime Date { get; set; }

        public string InstallationUnit { get; set; }
        public double InstallationRate { get; set; }
        public int InstallationQty { get; set; }
        public double InstallationTotal { get; set; }

        public string RewireCableUnit { get; set; }
        public double RewireCableRate { get; set; }
        public int RewireCableQty { get; set; }
        public double RewireCableTotal { get; set; }

        public string ExtraOutletsUnit { get; set; }
        public double ExtraOutletsRate { get; set; }
        public int ExtraOutletsQty { get; set; }
        public double ExtraOutletsTotal { get; set; }

        public string RewireOutletUnit { get; set; }
        public double RewireOutletRate { get; set; }
        public int RewireOutletQty { get; set; }
        public double RewireOutletTotal { get; set; }

        public string NonStandardUnit { get; set; }
        public double NonStandardRate { get; set; }
        public int NonStandardQty { get; set; }
        public double NonStandardTotal { get; set; }

        public string RejectedUnit { get; set; }
        public double RejectedRate { get; set; }
        public int RejectedQty { get; set; }
        public double RejectedTotal { get; set; }

        public string ServiceCallsUnit { get; set; }
        public double ServiceCallsRate { get; set; }
        public int ServiceCallsQty { get; set; }
        public double ServiceCallsTotal { get; set; }
    }

    public class InstallationSummaryFilter
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
    public class InstallationSummaryData
    {
        public int ContractorId { get; set; }
        public string Contractor { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string InstallationUnit { get; set; }
        public double InstallationRate { get; set; }
        public int InstallationQty { get; set; }
        public double InstallationTotal { get; set; }

        public string RewireCableUnit { get; set; }
        public double RewireCableRate { get; set; }
        public int RewireCableQty { get; set; }
        public double RewireCableTotal { get; set; }

        public string ExtraOutletsUnit { get; set; }
        public double ExtraOutletsRate { get; set; }
        public int ExtraOutletsQty { get; set; }
        public double ExtraOutletsTotal { get; set; }

        public string RewireOutletUnit { get; set; }
        public double RewireOutletRate { get; set; }
        public int RewireOutletQty { get; set; }
        public double RewireOutletTotal { get; set; }

        public string NonStandardUnit { get; set; }
        public double NonStandardRate { get; set; }
        public int NonStandardQty { get; set; }
        public double NonStandardTotal { get; set; }

        public string RejectedUnit { get; set; }
        public double RejectedRate { get; set; }
        public int RejectedQty { get; set; }
        public double RejectedTotal { get; set; }

        public string ServiceCallsUnit { get; set; }
        public double ServiceCallsRate { get; set; }
        public int ServiceCallsQty { get; set; }
        public double ServiceCallsTotal { get; set; }
    }

    public class DailyWorkSheetFilter
    {        
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        [Required]
        public int? ContrctorId { get; set; }
    }
    public class DailyWorkSheetData
    {
        public DateTime Date { get; set; }
        public string Ref { get; set; }
        public int ContractorId { get; set; }
        public string Contractor { get; set; }
        public int WorkOrderId { get; set; }
        public string Customer { get; set; }
        public int ActivityId { get; set; }
        public string Activity { get; set; }
        public string Unit { get; set; }
        public double Rate { get; set; }
        public int Qty { get; set; }
        public double Total { get; set; }
    }

    public class ActivitySummaryFilter
    {
        public int conId { get; set; }
        public int year { get; set; }
        public int? month { get; set; }
        public int? day { get; set; }
        public int? quarter { get; set; }
        public int? period { get; set; }
        public double? dateRangeFrom { get; set; }
        public double? dateRangeTo { get; set; }
    }

    public class ContractorBankPayments
    {
        public double Amount { get; set; }
        public string Currency { get; set; }
        public string BankBranchCode { get; set; }
        public string BankId { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class InvoiceSummaryFilter
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int? ClassId { get; set; }
        public int? RegonId { get; set; }
        public string PONum { get; set; }
    }
}