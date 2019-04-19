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
        private IProductRepository repository;

        public CartController(IProductRepository repo)
        {
            repository = repo;
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Index(Cart cart, string returnUrl)
        {

            #region legacy pattern code
            //if (TempData["navDictionary"] != null)
            //{
            //    // get category and page
            //    Dictionary<string, object> dict = TempData["navDictionary"] as Dictionary<string, object>;
            //    category = ((string)dict["category"] == string.Empty ? null : (string)dict["category"]); //set this to null if empty string
            //    page = (int)dict["page"];
            //}
            #endregion

            //if no game in progress then go back to the intro page.
            if (!Session.GetGameInProgress()) //this variable is always true or null
            {
                return RedirectToAction("Index", "Home");
            }

            //When returning to the controller, always update the cart with simulated activity by the NPC.
            IEnumerable<Product> list = repository.Products.ToList<Product>();
            Session.RunNpcSweep(cart, list);

            //ViewBag.remainingTime = 50000; //todo set this
            Session["cartObj"] = cart;

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
        //    }
        //    return RedirectToAction("Index", new { returnUrl });
        //}
        #endregion


        //This method can be called in two ways. If user simply wants to view the cart we construct a simple redirect. If user wants to add to cart, we reload the same page with the items updated.
        //Although this second option is a candidate for an Ajax upload of the partial view, we in fact relaod the whole screen to refresh any updates to the stock of all displayed items.
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public RedirectToRouteResult UpdateCart(Cart cart, int productId, int MyQuantity, string returnUrl, int pageNumber, string categoryString, string submitUpdate) //, string submitCheckout)
        {
            string updateMsg = "";

            //When returning to the controller, always update the cart with simulated activity by the NPC.
            IEnumerable<Product> list = repository.Products.ToList<Product>();
            Session.RunNpcSweep(cart, list);

            //store the pageNumber and categoryString params in temp data (this is kind of a bodge). Add any other necessary data.
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("page", pageNumber);
            dict.Add("category", categoryString);

            if (submitUpdate == null) { // User has selected "View Cart"
                dict.Add("productId", 0);
                dict.Add("message", string.Empty);
                TempData["navDictionary"] = dict;       // Store it in the TempData.
                Session["cartObj"] = cart;
                return RedirectToAction("Index", new { returnUrl });
            }
            else // User has selected "Update Cart"
            {
                Product product = repository.Products.FirstOrDefault(p => p.ProductID == productId);
                if (product != null)
                {
                    updateMsg = cart.BalanceCartTransaction(product, MyQuantity);
                }
                dict.Add("productId", productId);
                dict.Add("message", updateMsg);
                TempData["navDictionary"] = dict;       // Store it in the TempData
                Session["cartObj"] = cart;
                return RedirectToAction("List", "Product");
            }
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
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


        [OutputCache(Duration = 1, VaryByParam = "None")]
        public PartialViewResult Summary(Cart cart)
        {
            //update product quantity using cartline
            foreach (var line1 in cart.Lines)
            {
                line1.Product.MyQuantity = line1.Quantity;
            }

            if (TempData["npcCart"] != null)      //we use this workaround because the Product List page doesn't have a workable model to update the cart info.
            {
                cart.LinesOther = (IEnumerable<CartLine>)TempData["npcCart"];
            }
            else
            {
                TempData["npcCart"] = cart.LinesOther;
            }

            foreach (var line2 in cart.LinesOther)
            {
                line2.Product.OtherQuantity = line2.Quantity;
            }

            int remainingMilliseconds = Session.GetRemainingTime();
            ViewBag.remainingTime = remainingMilliseconds;

            return PartialView(cart);
        }

        #region legacy pattern
        //[HttpGet]
        //public ViewResult Checkout()
        //{
        //    return View(new ShippingDetails());
        //}
        #endregion

        //[HttpPost]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Checkout(Cart cart)
        {
            //if no game in progress then go back to the intro page.
            if (!Session.GetGameInProgress()) //this variable is always true or null
            {
                return RedirectToAction("Index", "Home");
            }

            //When checking out, always update the cart with simulated activity by the NPC.
            IEnumerable<Product> list = repository.Products.ToList<Product>();
            Session.RunNpcSweep(cart, list, true); //shopToEnd indicator set

            //Get new model object with merged cart lines
            List<MergedCartLine> modelList = MergeCartLines(cart);

            if (modelList.Count() == 0)
            {
                ModelState.AddModelError("", "Sorry, no items in either cart!");
            }

            //Rather than trying some complex calculation in the view, we will pass the totals in a viewbag
            ViewBag.UserTotal = modelList.Sum(x => x.ComputedUserTotal);
            ViewBag.AITotal = modelList.Sum(x => x.ComputedAITotal);

            //clear out any game baggage.
            Session.Clear();
            TempData["navDictionary"] = null;
            TempData["npcCart"] = null;

            //return View(cart); //legacy code
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

                //finally remove any zero values rows (coding defensively)
                if (item3.Quantity == 0 && item3.QuantityOther == 0)
                {
                    mergedList.Remove(item3);
                }
            }
            return mergedList.OrderBy(x => x.Product.ProductID).ToList(); //order by the product id.
        }

    }
}