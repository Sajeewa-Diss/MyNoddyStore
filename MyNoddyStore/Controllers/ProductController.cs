using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
using MyNoddyStore.Abstract;
using MyNoddyStore.Models;

namespace MyNoddyStore.Controllers
{
    public class ProductController : Controller
    {
        private IProductRepository repository;        // a private object that implements IProductRepository 
        public int PageSize = 5;

        public ProductController(IProductRepository productRepository)
        {  // constructor supplying the product list
            this.repository = productRepository;
        }

        //public ViewResult List()
        //{
        //    return View(repository.Products);
        //}

        //public ViewResult List(int page = 1)
        //{ 
        //    return View(repository.Products  
        //        .OrderBy(p => p.ProductID)
        //        .Skip((page - 1) * PageSize)
        //    .Take(PageSize));
        //}

        //[HttpGet]
        public ViewResult List(string category, int page = 1)
        {
            //set counddown variable again
            ViewBag.remainingTime = 909;

            // Check if your key exists
            if (TempData["myDictionary"] != null)
            {
                // Grab your activity
                Dictionary<string, object> dict = TempData["myDictionary"] as Dictionary<string, object>;
                category = ((string)dict["category"] == string.Empty ? null : (string)dict["category"]); //set this to null if empty string
                page = (int)dict["page"];
            }

            ProductsListViewModel model = new ProductsListViewModel
            {
                Products = repository.Products
                    .Where(p => category == null || p.Category == category)
                    .OrderBy(p => p.ProductID)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = PageSize,
                    TotalItems = category == null ? repository.Products.Count() : repository.Products.Where(e => e.Category == category).Count()
                },
                CurrentCategory = category
            };
            return View(model);
        }
    }
}