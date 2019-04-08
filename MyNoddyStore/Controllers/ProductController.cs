using System;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using MyNoddyStore.Abstract;
using MyNoddyStore.Models;
using MyNoddyStore.AdHocHelpers;
using MyNoddyStore.HtmlHelpers;

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


        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ViewResult List(string category, int page = 1)
        {
            int remainingMilliseconds = Session.GetRemainingTimeOrSetDefault(); // countdown time variable

            ViewBag.remainingTime = remainingMilliseconds;

            // Check if your key exists
            if (TempData["myDictionary"] != null)
            {
                // get category and page
                Dictionary<string, object> dict = TempData["myDictionary"] as Dictionary<string, object>;
                category = ((string)dict["category"] == string.Empty ? null : (string)dict["category"]); //set this to null if empty string
                page = (int)dict["page"];
            }


            //Original code worked for one category per product only.
            //ProductsListViewModel model = new ProductsListViewModel
            //{
            //    Products = repository.Products
            //        .Where(p => category == null || p.Category == category)
            //        .OrderBy(p => p.ProductID)
            //        .Skip((page - 1) * PageSize)
            //        .Take(PageSize),
            //    PagingInfo = new PagingInfo
            //    {
            //        CurrentPage = page,
            //        ItemsPerPage = PageSize,
            //        TotalItems = category == null ? repository.Products.Count() : repository.Products.Where(e => e.Category == category).Count()
            //    },
            //    CurrentCategory = category
            //};

            ProductsListViewModel model = new ProductsListViewModel
            {
                Products = repository.Products
                    .Where(p => category == null || p.Categories.EmptyArrayIfNull().Contains(category))
                    .OrderBy(p => p.ProductID)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = PageSize,
                    TotalItems = category == null ? repository.Products.Count() : repository.Products.Where(e => e.Categories.EmptyArrayIfNull().Contains(category)).Count()
                },
                CurrentCategory = category,
                CountDownMilliseconds = remainingMilliseconds
            };

            return View(model);
        }

        //private int GetCountdownOrSetDefault(int defaultVal = 10)
        //{
        //    int remainingMilliseconds; // countdown time variable

        //    //var context = HttpContext.Current; //.Session;

        //    //if countdown already started, don't do anything
        //    DateTime countdownTime = Session.GetDataFromSession<DateTime>("countdownTimeCsKey");

        //    if (countdownTime == DateTime.MinValue)
        //    {
        //        //set a new countdown time of 31 seconds (1 second extra to account for lag)
        //        Session.SetDataToSession<string>("countdownTimeCsKey", DateTime.Now.AddMilliseconds(40000));
        //        remainingMilliseconds = 41000;
        //    }
        //    else
        //    {
        //        TimeSpan tsRemaining = countdownTime - DateTime.Now;
        //        remainingMilliseconds = (int)tsRemaining.TotalMilliseconds;  //convert to integer for passing to view.
        //    }

        //    return 10;
        //}
    }
}