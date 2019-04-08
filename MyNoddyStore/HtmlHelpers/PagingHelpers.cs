using System;
using System.Text;
//using System.Web.SessionState;
using System.Web.Mvc;

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

    }
}