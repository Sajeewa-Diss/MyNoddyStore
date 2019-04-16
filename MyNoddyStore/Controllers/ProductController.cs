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


        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ViewResult List(string category, int page = 1)
        {
            int remainingMilliseconds = Session.GetRemainingTimeOrSetDefault(); // countdown time variable. todo set this to get only? probably not.
            Cart cart = new Cart();

            int productId = 0;
            string updateMsg = string.Empty;
            //ViewBag.remainingTime = remainingMilliseconds;

            // simulate further shopping by the NPC
            IEnumerable<Product> list = repository.Products.ToList<Product>();
            Session.RunNpcSweep(cart, list);

            //var x = cart.LinesOther;

            // Check if "cartObj" key exists
            if (Session["cartObj"] != null)
            {
                // get passed object
                cart = (Cart)Session["cartObj"];

                //if cart has no other lines we need to get the stored value above.
                //cart.LinesOther = x;
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
                //Cart1 = cart,
                UpdatedProductId = productId,
                UpdatedMessage = updateMsg
            };

            //ViewBag.productId = (int)productId; //todo remove these two viewbag data (check not used).
            //ViewBag.statusMsg = (string)updateMsg;
            return View(model);
        }

        //note this merge is necessary only because we have a bodged the view model. Don't use this pattern in any original code!
        private void MergeProductsStockWithCart(IEnumerable<Product> products, Cart cart)
        {
            IEnumerable<CartLine> linesList = cart.Lines;
            IEnumerable<CartLine> linesOtherList = cart.LinesOther;

            foreach (var pr in products)
            {
                //reset each products data
                pr.MyQuantity = 0;
                pr.OtherQuantity = 0;
                pr.StockCount = 0;

                //first update using other cartline
                foreach (CartLine item in linesOtherList)
                {
                    if (item.Product.ProductID == pr.ProductID)
                    {
                        if (item.Quantity != item.Product.OtherQuantity)
                        {
                            Debug.WriteLine("pete tong *******     ******      ******      ******     ******      *****     *****     *****      *****     ******     *****");
                        }
                        pr.OtherQuantity = item.Product.OtherQuantity;
                    }
                }

                //next update using user cartline
                foreach (CartLine item in linesList)
                {
                    if (item.Product.ProductID == pr.ProductID)
                    {
                        pr.MyQuantity = item.Quantity;
                    }
                }
                //recalculate the current stock count
                pr.StockCount = pr.InitialStockCount - pr.OtherQuantity - pr.MyQuantity;

            }
        }
    }
}