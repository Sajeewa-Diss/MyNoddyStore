using System.Collections.Generic;
using System.Web.Mvc;
using MyNoddyStore.Abstract;
using System.Linq;
using MyNoddyStore.HtmlHelpers;
namespace MyNoddyStore.Controllers
{
    public class NavController : Controller
    {
        private IProductRepository repository;

        public NavController(IProductRepository repo)
        {
            repository = repo;
        }

        public PartialViewResult Menu(string category = null){

            ViewBag.SelectedCategory = category;

            #region legacy pattern code
            //original code works for one category per product (category always has single item as a string).
            //IEnumerable<string> categoryEnumerable = repository.Products
            //.Select(x => x.Category)
            //.Distinct()
            //.OrderBy(x => x);
            #endregion

            //new code works for multiple categories per product (as a string array).
            IEnumerable<string> categoryEnumerable = repository.Products
            .Select(x => x.Categories.EmptyArrayIfNull()).SelectMany(x => x).ToList()
            .Distinct()
            .OrderBy(x => x);

            categoryEnumerable = categoryEnumerable.Where(s => !string.IsNullOrWhiteSpace(s)); //remove any empty string for category (defensive coding).

            return PartialView("FlexMenu", categoryEnumerable);
            //return PartialView("Menu", categoryEnumerable); legacy code pattern
        }
    }

}