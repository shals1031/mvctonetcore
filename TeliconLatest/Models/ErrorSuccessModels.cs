namespace TeliconLatest.Models
{
    public class JsonReturnParams
    {
        public string Msg { get; set; }
        public string Code { get; set; }
        public object Additional { get; set; }        
    }
    public class SWOJsonReturnParams : JsonReturnParams
    {
        public int WID { get; set; }
    }
}