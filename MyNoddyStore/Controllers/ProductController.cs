using System;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using MyNoddyStore.Abstract;
using MyNoddyStore.Entities;
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

        #region legacy pattern code
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
        #endregion


        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ViewResult List(string category, int page = 1)
        {
            int remainingMilliseconds = Session.GetRemainingTimeOrSetDefault(); // countdown time variable. todo set this to get only.
            Cart cart = new Cart();

            int productId = 0;
            string updateMsg = string.Empty;
            //ViewBag.remainingTime = remainingMilliseconds;

            // Check if "cartObj" key exists
            if (Session["cartObj"] != null)
            {
                // get passed object
                cart = (Cart)Session["cartObj"];
            }

            // Check if "myDictionary" key exists
            if (TempData["myDictionary"] != null)
            {
                // get category and page
                Dictionary<string, object> dict = TempData["myDictionary"] as Dictionary<string, object>;
                category = ((string)dict["category"] == string.Empty ? null : (string)dict["category"]); //set this to null if empty string
                page = (int)dict["page"];
                productId = (int)dict["productId"];
                updateMsg = (string)dict["message"];
            }

            #region legacy pattern code
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
            #endregion

            //merge product and cart info
            var productList = repository.Products.Where(p => category == null || p.Categories.EmptyArrayIfNull().Contains(category))
                    .OrderBy(p => p.ProductID)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize);

            MergeProductsStockWithCart(productList, cart);

            ProductsListViewModel model = new ProductsListViewModel
            {
                Products = productList,
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

        //please note this merge is necessary only because we have a bodged the view model. Don't use this in any original code!
        private void MergeProductsStockWithCart(IEnumerable<Product> products, Cart cart)
        {
            IEnumerable<CartLine> lineList = cart.Lines;
            
            foreach (var pr in products)
            {
                foreach(var lineItem in lineList)
                {
                    if (lineItem.Product.ProductID == pr.ProductID)
                    {
                        pr.MyQuantity = lineItem.Quantity;
                        pr.StockCount = pr.InitialStockCount - pr.MyQuantity;
                    }
                }

            }
        }
    }
}