using System;
using System.Text;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
using MyNoddyStore.Models;
namespace MyNoddyStore.HtmlHelpers
{
    public static class PagingHelpers   // a public static class allows extension method!
    {
        public static MvcHtmlString PageLinks(this HtmlHelper html,  //this is created as extension method for this class type!
                                                PagingInfo pagingInfo,     // this object's data is used to generate navigation HTML
                                                Func<int, string> pageUrl)  // this is a delegate used to generate links
        {
            StringBuilder result = new StringBuilder();
            for (int i = 1; i <= pagingInfo.TotalPages; i++)
            {
                TagBuilder tag = new TagBuilder("a");
                tag.MergeAttribute("href", pageUrl(i));
                tag.InnerHtml = i.ToString();
                if (i == pagingInfo.CurrentPage)
                {
                    tag.AddCssClass("selected");
                    tag.AddCssClass("btn-primary");
                }
                tag.AddCssClass("btn btn-default");
                result.Append(tag.ToString());
            }
            return MvcHtmlString.Create(result.ToString());
        }

        //Helper method required for paging.
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> sequence)
        {
            return sequence.Where(e => e != null);
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence)
            where T : struct
        {
            return sequence.Where(e => e != null).Select(e => e.Value);
        }

        public static IEnumerable<T> EmptyArrayIfNull<T>(this IEnumerable<T> sequence) //todo clean up
        {
            if (sequence == null) {
                string[] emptyArray = new string[]{String.Empty}; // handle a product with no arrays by using a dummy string array of one empty item.

                //public static IEnumerable<SomeType> AllEnums = Enum.GetValues(typeof(SomeType)).Cast<SomeType>().ToList();
                //IEnumerable<string> emptyStringList = Enum.GetValues(typeof(String)).Cast<string>().ToList();
                //IEnumerable<T> d = emptyArray.Cast<T>();

                //IEnumerable<T>d = Enum.GetValues(typeof(String)).Cast<T>().ToList();

                return emptyArray.Cast<T>(); //we cast to type T but this is superfluous as we are only handling null string arrays here!
            }
            else
            {
                return sequence;
            }
        }

    }
}