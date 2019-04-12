using System;
using System.Linq;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
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
        private IOrderProcessor orderProcessor;
        private string messageString;

        public CartController(IProductRepository repo, IOrderProcessor proc)
        {
            repository = repo;
            orderProcessor = proc;
            //cartService = set this here if req'd
        }

        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ViewResult Index(Cart cart, string returnUrl)
        {

            //if (TempData["myDictionary"] != null)
            //{
            //    // get category and page
            //    Dictionary<string, object> dict = TempData["myDictionary"] as Dictionary<string, object>;
            //    category = ((string)dict["category"] == string.Empty ? null : (string)dict["category"]); //set this to null if empty string
            //    page = (int)dict["page"];
            //}
            

            ViewBag.remainingTime = 50000; //todo set this

            //ViewBag.SomeData = cartService.GetSomeData();

            return View(new CartIndexViewModel
            {
                ReturnUrl = returnUrl,
                UpdateMessage = messageString,
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

            //When returning to the controller, always update the cart with simulated activity by the computer-player.
            SimulateSweepUser(cart);

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
                    updateMsg = BalanceCartTransaction(cart, product, MyQuantity);
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

        [HttpGet]
        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ViewResult Checkout()
        {
            return View(new ShippingDetails());
        }

        [HttpPost]
        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ViewResult Checkout(Cart cart, ShippingDetails shippingDetails)
        {
            if (cart.Lines.Count() == 0)
            {
                ModelState.AddModelError("", "Sorry, your cart is empty!");
            }
            if (ModelState.IsValid)
            {
                orderProcessor.ProcessOrder(cart, shippingDetails);
                cart.Clear();
                return View("Completed");
            }
            else
            {
                return View(shippingDetails);
            }
        }

        //Simulate another user shopping up to this point in time or until the end of the sweep time-period.
        //This method will add one item to cart per second of shopping allowed or remaining.
        private void SimulateSweepUser(Cart cart, bool shopToEnd = false)
        {
            //System.Diagnostics.Debug.WriteLine("simulate sweep method entered");

            int lastProdId = 0;
            int numItemsToAdd = 0;

            //ensure that the sweep user hasn't yet finished
            bool sweepCompleted = Session.GetShoppingByOtherPlayerCompleted(); //todo set this somewhere else to remove null reference exception  - check for all other such objects.
            if (sweepCompleted){
                return;          //Operation has completed. No need to simulate shopping.
            }

            int maxItemLimit = HtmlHelpers.AdHocHelpers.simulatedShoppingItemLimit;
            int totalMilliseconds = HtmlHelpers.AdHocHelpers.shoppingTimeMilliseconds;
            int currentItemQuantity = SumOtherQuantity(cart);

            //ensure we are within time. If so, calculate number of seconds of shopping time to simulate. If not, shop to end of period.
            int remainingMilliseconds = Session.GetRemainingTime();
            if (remainingMilliseconds <= 0){ //if shopping time has expired, then shop to the end of the time period (and set appropriate flags).
                shopToEnd = true;
            }

            //if shopping to end, add all remaining items. Else add items with respect to the current-expired time only (as a simple ratio, say).
            if (shopToEnd){
                numItemsToAdd = maxItemLimit - currentItemQuantity; 
            } else { // some casting ius req'd to stop integer ratios tending to zero.
                numItemsToAdd = (int)(maxItemLimit * (((double)totalMilliseconds - (double)remainingMilliseconds) / (double)totalMilliseconds)) - currentItemQuantity;
            }

            //int x = Session.GetLastItemAddedByOtherPlayer();
            //int z = Session.GetCountdownRandomizerValue();


            //AdHocHelpers.LastItemAddedByOtherPlayer { get; set; }     //used to cycle through the products inventory. 
            //AdHocHelpers.ShoppingByOtherPlayerCompleted { get; set; }

            //set the two static properties.
            Session.SetLastItemAddedByOtherPlayer(lastProdId);
            if (shopToEnd) //if shopping time has completed, set appropriate flag.
            { 
                Session.SetShoppingByOtherPlayerCompleted(true);
            }

        }

        private int SumOtherQuantity(Cart cart)
        {
            int total = 0;
            //update product quantity using cartline. todo rewrite this as a linq expression.
            foreach (var line in cart.Lines)
            {
                total += line.Product.OtherQuantity;
            }
            return total;
        }

        //Balance stock and quantities in current cart update request
        private string BalanceCartTransaction(Cart cart, Product product, int newQuantity){

            string messageString = "Updated";

            if (newQuantity < 0 || newQuantity > 5)
            {
                messageString = "invalid number of items";
            }

            //return product's current quantity to stock.
            product.StockCount += product.MyQuantity;
            product.MyQuantity = 0;
            cart.RemoveLine(product);

            //re-add new quantity where stock allows.
            if (newQuantity > 0) { 
                if (product.StockCount >= newQuantity) // the update can be done
                {
                    product.MyQuantity = newQuantity;
                    product.StockCount -= newQuantity;
                    cart.AddItem(product);
                }
                else if (product.StockCount != 0)  // there is some stock. The update can be done only partially
                {
                    product.MyQuantity = product.StockCount;
                    product.StockCount = 0;
                    cart.AddItem(product);
                    messageString = "Added partially (no stock)";
                }
                else  // the update can't be done. No stock.
                {
                    messageString = "Failed (no stock)";
                }
            }
            return messageString;
        }

    }
}