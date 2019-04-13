using System;
using System.Linq;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Collections.Generic;
using MyNoddyStore.HtmlHelpers;
using MyNoddyStore.Abstract;
using MyNoddyStore.Entities;
using MyNoddyStore.Models;

namespace MyNoddyStore.Controllers
{
    public class CartController : Controller
    {
        //private readonly ICartService cartService;
        private IProductRepository repository;
        //private string messageString;

        public CartController(IProductRepository repo)
        {
            repository = repo;
        }

        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ViewResult Index(Cart cart, string returnUrl)
        {

            #region legacy pattern code
            //if (TempData["myDictionary"] != null)
            //{
            //    // get category and page
            //    Dictionary<string, object> dict = TempData["myDictionary"] as Dictionary<string, object>;
            //    category = ((string)dict["category"] == string.Empty ? null : (string)dict["category"]); //set this to null if empty string
            //    page = (int)dict["page"];
            //}
            #endregion

            //When returning to the controller, always update the cart with simulated activity by the NPC.
            //todo should this method be called here??
            //todo also investigate how much time the AI sweep call costs.
            IEnumerable<Product> list = repository.Products.ToList<Product>();
            Session.RunNpcSweep(cart, list);

            ViewBag.remainingTime = 50000; //todo set this


            //ViewBag.SomeData = cartService.GetSomeData();

            return View(new CartIndexViewModel
            {
                ReturnUrl = returnUrl,
                //UpdateMessage = messageString,
                Cart = cart
            });
        }

        #region legacy pattern code
        //this legacy method is no longer used by our pattern.
        //public RedirectToRouteResult AddToCart(Cart cart, int productId, int MyQuantity, string returnUrl)
        //{
        //    Product product = repository.Products
        //    .FirstOrDefault(p => p.ProductID == productId);
        //    if (product != null)
        //    {
        //        //cart.AddItem(product, 1);
        //        cart.AddItem(product, MyQuantity);

        //        //todo decide how to correlate cart line and updated values.
        //        messageString = "Update successful";
        //    }
        //    return RedirectToAction("Index", new { returnUrl });
        //}
        #endregion


        //This method can be called in two ways. If user simply wants to view the cart we construct a simple redirect. If user wants to add to cart, we reload the same page with the items updated.
        //Although this second option is a candidate for an Ajax upload of the partial view, we in fact relaod the whole screen to refresh any updates to the stock of all displayed items.
        public RedirectToRouteResult UpdateCart(Cart cart, int productId, int MyQuantity, string returnUrl, int pageNumber, string categoryString, string submitUpdate) //, string submitCheckout)
        {
            string updateMsg = ""; //todo handle when time expired with a suitable update message.

            //When returning to the controller, always update the cart with simulated activity by the NPC.
            IEnumerable<Product> list = repository.Products.ToList<Product>();
            Session.RunNpcSweep(cart, list);

            //store the pageNumber and categoryString params in temp data (this is kind of a bodge). Add any other necessary data.
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("page", pageNumber);
            dict.Add("category", categoryString);  //todo handle null

            if (submitUpdate == null) { // User has selected "View Cart"
                dict.Add("productId", 0);
                dict.Add("message", string.Empty);
                TempData["myDictionary"] = dict;       // Store it in the TempData.
                Session["cartObj"] = cart;
                return RedirectToAction("Index", new { returnUrl });
            }
            else // User has selected "Update Cart"
            {
                Product product = repository.Products.FirstOrDefault(p => p.ProductID == productId);
                if (product != null)
                {
                    updateMsg = cart.BalanceCartTransaction(product, MyQuantity);
                    //ViewBag.testMessage = updateMsg;
                }
                dict.Add("productId", productId);
                dict.Add("message", updateMsg);
                TempData["myDictionary"] = dict;       // Store it in the TempData
                Session["cartObj"] = cart;
                return RedirectToAction("List", "Product");
            }
        }

        public RedirectToRouteResult RemoveFromCart(Cart cart, int productId, string returnUrl)
        {
            Product product = repository.Products
            .FirstOrDefault(p => p.ProductID == productId);
            if (product != null)
            {
                cart.RemoveLine(product);
            }
            return RedirectToAction("Index", new { returnUrl });
        }
   

        public PartialViewResult Summary(Cart cart)
        {
            //update product quantity using cartline
            foreach (var line in cart.Lines)
            {
                line.Product.MyQuantity = line.Quantity;
            }

            int remainingMilliseconds = Session.GetRemainingTime();
            ViewBag.remainingTime = remainingMilliseconds;

            return PartialView(cart);
        }

        //[HttpGet]
        ////[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        //public ViewResult Checkout()
        //{
        //    return View(new ShippingDetails());
        //}

        //[HttpPost]
        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ViewResult Checkout(Cart cart)
        {
            //Get new model object with merged cart lines
            List<MergedCartLine> modelList = MergeCartLines(cart);

            if (modelList.Count() == 0)
            {
                ModelState.AddModelError("", "Sorry, something went wrong with checkout!");
            }

            //Rather than trying some complex calculation in the view, we will pass the totals in a viewbag
            ViewBag.UserTotal = modelList.Sum(x => x.ComputedUserTotal);
            ViewBag.AITotal = modelList.Sum(x => x.ComputedAITotal);

            //return View(cart);
            return View(modelList);
        }


        private List<MergedCartLine> MergeCartLines(Cart cart)
        {
            List<CartLine> cartLineList = cart.Lines.ToList<CartLine>();
            List<CartLine> cartLineOtherList = cart.LinesOther.ToList<CartLine>();
            List<MergedCartLine> list = new List<MergedCartLine>();
            List<MergedCartLine> list2 = new List<MergedCartLine>();
            List<MergedCartLine> mergedList = new List<MergedCartLine>();

            //create two lists with zero quantities
            foreach (CartLine item in cartLineList){
                var mergeItem = new MergedCartLine { Product = item.Product, Quantity = 0, QuantityOther = 0 };
                list.Add(mergeItem);}

            foreach (CartLine item2 in cartLineOtherList){
                var mergeItem2 = new MergedCartLine { Product = item2.Product, Quantity = 0, QuantityOther = 0 };
                list2.Add(mergeItem2);}

            mergedList = list2.Union(list, new SimpleCartLineComparer()).ToList< MergedCartLine>(); //this Union is performed on IEnumerable and must be cast to a list.

            //next loop thru and add the quantities
            foreach (var item3 in mergedList.Reverse<MergedCartLine>()) //reverse the order as we may remove items without issues.
            {
                foreach (var item4 in cartLineList)
                {
                    if ((item3.Product.ProductID) == (item4.Product.ProductID))
                        item3.Quantity = item4.Quantity;
                }
                foreach (var item5 in cartLineOtherList)
                {
                    if ((item3.Product.ProductID) == (item5.Product.ProductID))
                        item3.QuantityOther = item5.Quantity;
                }
                //todo finally remove any zero values rows (coding defensively)
                if (item3.Quantity == 0 && item3.QuantityOther == 0)
                {
                    mergedList.Remove(item3);
                }
            }
            return mergedList.OrderBy(x => x.Product.ProductID).ToList(); //order by the product id.
        }

    }
}