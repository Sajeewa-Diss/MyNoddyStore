using System;
using System.Text;
//using System.Web.SessionState;
//using Microsoft.AspNet.Session;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
//using Newtonsoft.Json;
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
                //string[] emptyArray = new string[] { };            // handle a product with no categories by returning an empty array object.
                //T[] emptyArray = new T[] { };                        // handle a product with no categories by returning an empty array object.

                //return emptyArray; //.Cast<T>();                         //we could cast to type T but this is superfluous.
                return new T[] { };
            }
            else
            {
                return sequence;
            }
        }

        //public static void SetObjectAsJson(this ISession session, string key, object value) //todo use this values and change this class name or delete these methods
        //{
        //    session.SetString(key, JsonConvert.SerializeObject(value));
        //}

        //public static T GetObjectFromJson<T>(this ISession session, string key)
        //{
        //    var value = session.GetString(key);

        //    return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        //}
    }
}