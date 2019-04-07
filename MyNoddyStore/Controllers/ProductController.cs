using System;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using MyNoddyStore.Abstract;
using MyNoddyStore.Models;
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

        public ViewResult List(string category, int page = 1)
        {
            //set countdown variable again
            ViewBag.remainingTime = 909;
            DateTime countdownTime;
            double remainingMilliseconds = 0;

            //if countdown already started, don't do anything
            //string value = Session.GetDataFromSession<string>("countdownTimeCsKey");
            DateTime? sessionVal = Session.GetDataFromSession<DateTime>("countdownTimeCsKey");

            if (sessionVal is null)
            {
                countdownTime = DateTime.MinValue;
            }
            else
            {
                countdownTime = (DateTime)sessionVal;
            }

            if (countdownTime == DateTime.MinValue)
            {
                //System.Diagnostics.Debug.WriteLine("null time found");
                //set a new countdown time
                Session.SetDataToSession<string>("countdownTimeCsKey", DateTime.Now.AddMilliseconds(40000));
                remainingMilliseconds = 40000;
            } else
            {
                TimeSpan remaining = countdownTime - DateTime.Now;
                remainingMilliseconds = remaining.TotalMilliseconds;
            }



            // Check if your key exists
            if (TempData["myDictionary"] != null)
            {
                // Grab your activity
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
    }
}