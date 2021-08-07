using LinqKit;
using Microsoft.AspNetCore.Html;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TeliconLatest.DataEntities;
using TeliconLatest.Models;

namespace TeliconLatest.Reusables
{
    public static class Extensions
    {
        public static double GetRateAmountForDate(this ADM01100 source, DateTime date)
        {
            if (source.StartDate <= date)
            {
                var obj = source.ADM01250.Where(x => x.StartDate <= date && x.EndDate >= date).OrderBy(x => x.EndDate).FirstOrDefault();
                return obj != null ? obj.Amount : 0;
            }
            //return source.RateAmount;
            var a = source.ADM01150.Where(x => x.StartDate <= date && x.EndDate >= date).OrderByDescending(x => x.EndDate).FirstOrDefault();
            return a != null ? a.Amount : 0;
        }
        public static double GetClientRateAmountForDate(this ADM01100 source, DateTime date)
        {
            if (source.ADM01150.Count > 0)
            {
                var data = source.ADM01150.Where(x => x.StartDate <= date && x.EndDate >= date);
                return data != null && data.Any() ? data.OrderByDescending(x => x.EndDate).FirstOrDefault().Amount : 0;
            }
            else
            {
                return 0;// source.RateAmount;
            }
        }
        public static double GetPaymentRateAmountForDate(this ADM01100 source, DateTime date)
        {
            if (source.ADM01250.Count > 0)
            {
                return source.ADM01250.Where(x => x.StartDate <= date && x.EndDate >= date).OrderByDescending(x => x.EndDate).FirstOrDefault().Amount;
            }
            else
            {
                return 0;// source.RateAmount;
            }
        }
        public static List<Dictionary<string, string>> ToStringArray(this IQueryable source)
        {
            Type type = source.ElementType;
            var properties = type.GetProperties();
            Dictionary<string, string> strs;
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            foreach (var i in source)
            {
                strs = new Dictionary<string, string>();
                foreach (var p in properties)
                {
                    object value = p.GetValue(i, null);
                    strs.Add(p.Name, value == null ? null : value?.ToString());
                }
                list.Add(strs);
            }
            return list;
        }
        public static IQueryable<T> Search<T>(this IQueryable<T> source, List<ColumnsParam> cols, SearchParam search)
        {
            if (!string.IsNullOrEmpty(search.value))
            {
                Expression<Func<T, bool>> predicate = PredicateBuilder.False<T>();
                foreach (var col in cols)
                {
                    string colName = "";
                    if (col.name == "PayScale")
                    {
                        colName = "payScale";
                    }
                    else
                    {
                        colName = col.name;
                    }
                    var t = typeof(T).GetProperties();
                    var property = typeof(T).GetProperty(colName);
                    Type propertyType = null;
                    if (property != null)
                        propertyType = property.PropertyType;
                    else
                    {
                        property = t.FirstOrDefault(x => x.Name == colName);
                        propertyType = property.PropertyType;
                    }
                    var parameterExp = Expression.Parameter(typeof(T), "type");
                    Expression propertyExp = Expression.Property(parameterExp, col.name);
                    if ((propertyType.Name != "DateTime") && Nullable.GetUnderlyingType(propertyType) == null)
                    {
                        if (propertyType != typeof(string))
                        {
                            propertyExp = Expression.Convert(propertyExp, typeof(double?));
                            //var stringConvertMethod = typeof(System.Data.Entity.SqlServer.SqlFunctions).GetMethod("StringConvert", new[] { typeof(double?) });
                            //propertyExp = Expression.Call(stringConvertMethod, propertyExp);
                        }
                        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        var someValue = Expression.Constant(search.value, typeof(string));
                        var containsMethodExp = Expression.Call(propertyExp, method, someValue);
                        var lambda = Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
                        predicate = predicate.Or(lambda);
                    }
                }
                return source.AsExpandable().Where(predicate);
            }
            return source;
        }
        public static IQueryable<T> Search<T>(this IQueryable<T> source, List<ColumnsParam> cols, SearchParam search, bool isSearchOnDB)
        {
            if (isSearchOnDB)
                return source.Search(cols, search);
            else
                return source.SearchInList(cols, search);
        }
        private static IQueryable<T> SearchInList<T>(this IQueryable<T> source, List<ColumnsParam> cols, SearchParam search)
        {
            if (!string.IsNullOrEmpty(search.value))
            {
                Expression<Func<T, bool>> predicate = PredicateBuilder.False<T>();
                foreach (var col in cols)
                {
                    string colName = "";
                    if (col.name == "PayScale")
                    {
                        colName = "payScale";
                    }
                    else
                    {
                        colName = col.name;
                    }
                    var propertyType = typeof(T).GetProperty(colName).PropertyType;
                    var parameterExp = Expression.Parameter(typeof(T), "type");
                    Expression propertyExp = Expression.Property(parameterExp, col.name);
                    if ((propertyType.Name != "DateTime") && Nullable.GetUnderlyingType(propertyType) == null)
                    {
                        if (propertyType != typeof(string))
                        {
                            propertyExp = Expression.Convert(propertyExp, typeof(double?));
                            //var stringConvertMethod = typeof(System.Data.Entity.SqlServer.SqlFunctions).GetMethod("StringConvert", new[] { typeof(double?) });
                            //propertyExp = Expression.Call(stringConvertMethod, propertyExp);
                        }
                        var method = typeof(string).GetMethod("IndexOf", new[] { typeof(string), typeof(StringComparison) });
                        var someValue = Expression.Constant(search.value, typeof(string));
                        var someValue2 = Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison));
                        var methodExp = Expression.Call(propertyExp, method, new[] { someValue, someValue2 });
                        var methodCompar = Expression.GreaterThanOrEqual(methodExp, Expression.Constant(0, typeof(int)));
                        var lambda = Expression.Lambda<Func<T, bool>>(methodCompar, parameterExp);
                        predicate = predicate.Or(lambda);
                    }
                }
                return source.AsExpandable().Where(predicate);
            }
            return source;
        }
        public static string NormalizeSentence(this string source, bool UppCaseFirstLetterAfterSpace)
        {
            var chars = source.ToCharArray();
            StringBuilder str = new StringBuilder();
            for(int x = 0; x < chars.Length; x++){
                string character = chars[x].ToString();
                char ch = chars[x];
                if (!string.IsNullOrEmpty(character) && chars.Length >= 3 && ch != '.' && ch != ' ')
                {
                    if (x == 0 ||
                        (x > 0 && Char.IsLetter(ch) && (x + 2 <= chars.Length &&
                        ((chars[x + 1] == '.' && Char.IsLetter(chars[x + 2]))) ||
                        chars[x - 1] == '.' || (chars[x - 1] == ' ' && UppCaseFirstLetterAfterSpace) || 
                        (x > 1 && source.Substring(x - 2, 2) == ". "))))
                    {
                        str.Append(character.ToUpper());
                        continue;
                    }   
                }
                str.Append(character);
            }
            return str.ToString();
        }
        public static string AddOrdinal(this int num)
        {
            if (num <= 0) return num.ToString();
            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
                default:
                    break;
            }
            return (num % 10) switch
            {
                1 => num + "st",
                2 => num + "nd",
                3 => num + "rd",
                _ => num + "th",
            };
        }
        public static DataTable GetDataTableFromExcel(Stream path, bool hasHeader = true)
        {
            using ExcelPackage pck = new ExcelPackage(path);
            var ws = pck.Workbook.Worksheets.First();
            DataTable tbl = new DataTable();
            foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
            {
                tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
            }
            var startRow = hasHeader ? 2 : 1;
            for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
            {
                var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                DataRow row = tbl.Rows.Add();
                foreach (var cell in wsRow)
                {
                    row[cell.Start.Column - 1] = cell.Text;
                }
            }
            return tbl;
        }
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string sortColumn, bool descending)
        {
            // Dynamically creates a call like this: query.OrderBy(p =&gt; p.SortColumn)
            var parameter = Expression.Parameter(typeof(T), "p");

            string command = "OrderBy";

            if (descending)
            {
                command = "OrderByDescending";
            }

            Expression resultExpression = null;

            var property = typeof(T).GetProperty(sortColumn);
            // this is the part p.SortColumn
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);

            // this is the part p =&gt; p.SortColumn
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            // finally, call the "OrderBy" / "OrderByDescending" method with the order by lamba expression
            resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { typeof(T), property.PropertyType },
               query.Expression, Expression.Quote(orderByExpression));

            return query.Provider.CreateQuery<T>(resultExpression);
        }
    }
    public static class HtmlContentExtensions
    {
        public static string ToHtmlString(this IHtmlContent htmlContent)
        {
            if (htmlContent is HtmlString htmlString)
            {
                return htmlString.Value;
            }

            using StringWriter writer = new StringWriter();
            htmlContent.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
            return writer.ToString();
        }
    }
}