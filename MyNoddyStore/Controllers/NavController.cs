using System.Collections.Generic;
using System.Web.Mvc;
using MyNoddyStore.Abstract;
using System.Linq;
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

            IEnumerable<string> categories = repository.Products
            .Select(x => x.Category)
            .Distinct()
            .OrderBy(x => x);

            return PartialView("Menu", categories); //todo switch to flexmenu for mobiles
        }
    }

}