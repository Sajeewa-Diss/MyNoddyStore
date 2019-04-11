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


        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ViewResult List(string category, int page = 1)
        {
            int remainingMilliseconds = Session.GetRemainingTimeOrSetDefault(); // countdown time variable. todo set this to get only.

            int productId = 0;
            string updateMsg = string.Empty;
            //ViewBag.remainingTime = remainingMilliseconds;

            // Check if our key exists
            if (TempData["myDictionary"] != null)
            {
                // get category and page
                Dictionary<string, object> dict = TempData["myDictionary"] as Dictionary<string, object>;
                category = ((string)dict["category"] == string.Empty ? null : (string)dict["category"]); //set this to null if empty string
                page = (int)dict["page"];
                productId = (int)dict["productId"];
                updateMsg = (string)dict["message"];
            }


            //Original code correctly works for one category per product only.
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
                CountDownMilliseconds = remainingMilliseconds,
                UpdatedProductId = productId,
                UpdatedMessage = updateMsg
            };

            //ViewBag.productId = (int)productId; //todo remove these two viewbag data (check not used).
            //ViewBag.statusMsg = (string)updateMsg;
            return View(model);
        }
    }
}