using System.Collections.Generic;

namespace TeliconLatest.Models
{
    public class DataTablesParam
    {
        public int draw { get; set; }
        public List<ColumnsParam> columns { get; set; }
        public List<OrderParam> order { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public SearchParam search { get; set; }
        public string additional { get; set; }
        public int? year { get; set; }
        public int? client { get; set; }
        public int? zone { get; set; }
        public int? clas { get; set; }

        public DataTablesParam()
        {
            columns = new List<ColumnsParam>();
            order = new List<OrderParam>();
            search = new SearchParam();
        }
    }
    public class ColumnsParam
    {
        public string name { get; set; }
        public int data { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
        public SearchParam search { get; set; }
    }
    public class SearchParam
    {
        public string value { get; set; }
        public bool regex { get; set; }
    }
    public class OrderParam
    {
        public int column { get; set; }
        public string dir { get; set; }
    }
    public class DataTableReturn
    {
        public int draw { get; set; }
        public object additional { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<Dictionary<string, string>> data { get; set; }
    }
}