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

            int remainingMilliseconds = Session.GetRemainingTime();

            //When returning to the controller, always update the cart with simulated activity by the NPC.
            IEnumerable<Product> list = repository.Products.ToList<Product>();
            Session.RunNpcSweep(cart, list);

            Session["cartObj"] = cart;

            return View(new CartIndexViewModel
            {
                ReturnUrl = returnUrl,
                CountDownMilliseconds = remainingMilliseconds,
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
        //Although this second option is a candidate for an Ajax upload of the partial view, we in fact reload the whole screen to refresh any updates to the stock of all displayed items.
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public RedirectToRouteResult UpdateCart(Cart cart, int productId, int MyQuantity, string returnUrl, int pageNumber, string categoryString, string submitUpdate) //, string submitCheckout)
        {
            string updateMsg = "";

            //When returning to the controller, always update the cart with simulated activity by the NPC.
            IEnumerable<Product> list = repository.Products.ToList<Product>();
            Session.RunNpcSweep(cart, list);

            //store the cart in session.
            Session["cartObj"] = cart;

            //store the pageNumber and categoryString params in temp data (this is kind of a bodge). Add any other necessary data.
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("page", pageNumber);
            dict.Add("category", categoryString);

            if (submitUpdate == null) { // User has selected "View Cart"
                dict.Add("productId", 0);
                dict.Add("message", string.Empty);
                TempData["navDictionary"] = dict;       // Store it in the TempData.
                return RedirectToAction("Index", new { returnUrl });
            }
            else // User has selected "Update Cart"
            {
                Product product = repository.Products.FirstOrDefault(p => p.ProductID == productId);
                if (product != null)
                {
                    updateMsg = cart.BalanceCartTransaction(product, MyQuantity, Session.GetRemainingTime());
                }
                dict.Add("productId", productId);
                dict.Add("message", updateMsg);
                TempData["navDictionary"] = dict;       // Store it in the TempData
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

            if (TempData["npcCart"] != null)      //we use this workaround to pass data because the Product List page doesn't have a workable model to update the cart info.
            {
                cart.LinesOther = (IEnumerable<CartLine>)TempData["npcCart"];
            }
            else
            {
                TempData["npcCart"] = cart.LinesOther;
            }

            foreach (var lineOther in cart.LinesOther)
            {
                lineOther.Product.OtherQuantity = lineOther.Quantity;
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
        
        //We use a wrapper action method for Checkout action in order to prevent any other means to access it (such as browser navigation).
        //This is a GET method call so cart param is reconstructed from the model binder. 2nd optional param is a querystring indicator that client-side time has expired.
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult CheckoutCaller(Cart cart, int outOfTime = 0)
        {
            Session.SetUserJustClickedCheckout(true);
            TempData["checkoutCart"] = cart;       //pass method data to redirect action using tempdata.
            TempData["outOfTimeFlag"] = outOfTime;
            return RedirectToAction("Checkout");
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        //public ActionResult Checkout(Cart cart) //legacy code.
        public ActionResult Checkout()
        {
            int outOfTime;

            //if no game in progress then go back to the intro page.
            if (!Session.GetGameInProgress()) //this variable is always true or null
            {
                return RedirectToAction("Index", "Home");
            }

            //if we are returning to checkout using browser navigation after a game restart, then redirect to home screen.
            if (!Session.GetUserJustClickedCheckout())
            {
                return RedirectToAction("Index", "Home");
            }

            Cart cart = (Cart)TempData["checkoutCart"]; //collect the temp data cart object.
            outOfTime = (int)TempData["outOfTimeFlag"];

            //When checking out, always update the cart with simulated activity by the NPC.
            IEnumerable<Product> list = repository.Products.ToList<Product>();
            Session.RunNpcSweep(cart, list, true);                             //shopToEnd indicator set true!

            //Get new model object with merged cart lines
            List<MergedCartLine> modelList = MergeCartLines(cart);
            if (modelList.Count() == 0)
            {
                ModelState.AddModelError("", "Sorry, no items in either cart!");
            }

            decimal userTotal = modelList.Sum(x => x.ComputedUserTotal);
            decimal aiTotal = modelList.Sum(x => x.ComputedAITotal);

            //Pass the results meta data as viewbag item.
            ViewBag.UserMessage = (string)CalculateCheckoutResult(userTotal, aiTotal, outOfTime);
            //Rather than trying some complex calculation in the view, we will pass the results totals in viewbag
            ViewBag.UserQuanTotal = modelList.Sum(x => x.Quantity);
            ViewBag.AIQuanTotal = modelList.Sum(x => x.QuantityOther);
            ViewBag.UserTotal = userTotal; // modelList.Sum(x => x.ComputedUserTotal);
            ViewBag.AITotal = aiTotal; // modelList.Sum(x => x.ComputedAITotal);

            //clear out any game baggage.
            Session.Clear();
            TempData.Clear();

            //return View(cart); //legacy code
            return View(modelList);
        }

        //a private method to merge user and NPC cart lists for results screen.
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

        //generates the string message of the game result.
        private string CalculateCheckoutResult(decimal userTotal, decimal aiTotal, int outOfTimeFlag)
        {
            //If clientside outOfTimeFlag expired, or serverside time-limit exceeded, then show out of time message.
            int remainingMilliseconds = Session.GetRemainingTimeAtCheckout(); // countdown time variable.

            if ((outOfTimeFlag == 1) || (remainingMilliseconds < 0) || (remainingMilliseconds == int.MinValue))
            {
                return "Sorry. Out of time.";
            }
            if (userTotal < aiTotal)
            {
                return "gg, but you lost this time.";
            }
            if (userTotal == aiTotal)
            {
                return "gg, but the game was drawn.";
            }
            return AdHocHelpers.gameResultSuccessMsg; //if we reach this code, the user has won the game.
        }
    }
}