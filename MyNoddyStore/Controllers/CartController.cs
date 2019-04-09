using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using MyNoddyStore.HtmlHelpers;
using MyNoddyStore.Abstract;
using MyNoddyStore.Entities;
using MyNoddyStore.Models;
using MyNoddyStore.AdHocHelpers;

namespace MyNoddyStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService cartService; //todo decide should this be readonly??
        private IProductRepository repository;
        private IOrderProcessor orderProcessor;
        private string messageString;
        //private DateTime countdownTimeCs; 

        public CartController(IProductRepository repo, IOrderProcessor proc)
        {
            repository = repo;
            orderProcessor = proc;
            //cartService = how to set this??
        }

        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ViewResult Index(Cart cart, string returnUrl)
        {
            ViewBag.remainingTime = 50000; //todo set this

            //ViewBag.SomeData = cartService.GetSomeData();

            return View(new CartIndexViewModel
            {
                ReturnUrl = returnUrl,
                UpdateMessage = messageString,
                Cart = cart
            });
        }

        public RedirectToRouteResult AddToCart(Cart cart, int productId, int MyQuantity, string returnUrl)
        {
            Product product = repository.Products
            .FirstOrDefault(p => p.ProductID == productId);
            if (product != null)
            {
                //cart.AddItem(product, 1);
                cart.AddItem(product, MyQuantity);

                //todo decide how to correlate cart line and updated values.
                messageString = "Update successful";
            }
            return RedirectToAction("Index", new { returnUrl });
        }

        //This method can be called in two ways. If user simply wants to view the cart we construct a simple redirect. If user wants to add to cart, we reload the same page with the items updated.
        //Although this second option is a candidate for an Ajax upload of the partial view, we in fact relaod the whole screen to refresh any updates to the stock of all displayed items.
        public RedirectToRouteResult UpdateCart(Cart cart, int productId, int MyQuantity, string returnUrl, int pageNumber, string categoryString, string submitUpdate) //, string submitCheckout)
        {
            string updateMsg;

            if (submitUpdate == null) { // User has selected "View Cart"
                return RedirectToAction("Index", new { returnUrl });
            }
            else // User has selected "Update Cart"
            {
                //store the pageNumber and categoryString params in temp data (this is kind of a bodge).
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("page", pageNumber);
                dict.Add("category", categoryString);  //todo handle null
                TempData["myDictionary"] = dict;       // Store it in the TempData

                Product product = repository.Products.FirstOrDefault(p => p.ProductID == productId);
                if (product != null)
                {
                    updateMsg = BalanceCartTransaction(cart, product, MyQuantity);
                    
                }
                return RedirectToAction("List", "Product"); //, new { returnUrl }); //todo redirect to product list
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

        //alternative type of removeall construct.
        //public ActionResult Remove(Mobiles mob)
        //{
        //    List<Mobiles> li = (List<Mobiles>)Session["cart"];
        //    li.RemoveAll(x => x.slno == mob.slno);
        //    Session["cart"] = li;
        //    Session["count"] = Convert.ToInt32(Session["count"]) - 1;
        //    return RedirectToAction("Myorder", "AddToCart");

        //}

        //Adding Server-side Functions for Refreshing Quantity
        //We do not change the data properties in any model this time.
        //We just add functions to save the album item in the cart to the database and then return the data in JSON format for messaging purpose.
        //In Controllers\ShoppingCartController.cs:
        //[HttpPost]
        //public ActionResult UpdateCartCount(int id, int cartCount)
        //{
        //    // Get the cart 
        //    var cart = ShoppingCart.GetCart(this.HttpContext);

        //    // Get the name of the album to display confirmation 
        //    string albumName = storeDB.Carts
        //        .Single(item => item.RecordId == id).Album.Title;

        //    // Update the cart count 
        //    int itemCount = cart.UpdateCartCount(id, cartCount);

        //    //Prepare messages
        //    string msg = "The quantity of " + Server.HtmlEncode(albumName) +
        //            " has been refreshed in your shopping cart.";
        //    if (itemCount == 0) msg = Server.HtmlEncode(albumName) +
        //            " has been removed from your shopping cart.";
        //    //
        //    // Display the confirmation message 
        //    var results = new ShoppingCartRemoveViewModel
        //    {
        //        Message = msg,
        //        CartTotal = cart.GetTotal(),
        //        CartCount = cart.GetCount(),
        //        ItemCount = itemCount,
        //        DeleteId = id
        //    };
        //    return Json(results);
        //}


        //[HttpPost]
        //public ActionResult UpdateCartCount(int id, int cartCount)
        //{
        //    ShoppingCartRemoveViewModel results = null;
        //    try
        //    {
        //        // Get the cart 
        //        var cart = ShoppingCart.GetCart(this.HttpContext);

        //        // Get the name of the album to display confirmation 
        //        string albumName = storeDB.Carts
        //            .Single(item => item.RecordId == id).Album.Title;

        //        // Update the cart count 
        //        int itemCount = cart.UpdateCartCount(id, cartCount);

        //        //Prepare messages
        //        string msg = "The quantity of " + Server.HtmlEncode(albumName) +
        //                " has been refreshed in your shopping cart.";
        //        if (itemCount == 0) msg = Server.HtmlEncode(albumName) +
        //                " has been removed from your shopping cart.";
        //        //
        //        // Display the confirmation message 
        //        results = new ShoppingCartRemoveViewModel
        //        {
        //            Message = msg,
        //            CartTotal = cart.GetTotal(),
        //            CartCount = cart.GetCount(),
        //            ItemCount = itemCount,
        //            DeleteId = id
        //        };
        //    }
        //    catch
        //    {
        //        results = new ShoppingCartRemoveViewModel
        //        {
        //            Message = "Error occurred or invalid input...",
        //            CartTotal = -1,
        //            CartCount = -1,
        //            ItemCount = -1,
        //            DeleteId = id
        //        };
        //    }
        //    return Json(results);
        //}


        public PartialViewResult Summary(Cart cart)
        {
            //update product quantity using cartline
            foreach(var line in cart.Lines)
            {
                line.Product.MyQuantity = line.Quantity;
            }

            int remainingMilliseconds = Session.GetRemainingTimeOrSetDefault(); //todo remove this viewbag setter!!
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

        //Balance stock and quantities in current cart update request
        private string BalanceCartTransaction(Cart cart, Product product, int newQuantity){

            string messageString = "";

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
                    cart.AddItem(product, product.MyQuantity);
                }
                else if (product.StockCount != 0)  // there is some stock. The update can be done only partially
                {
                    product.MyQuantity = product.StockCount;
                    product.StockCount = 0;
                    cart.AddItem(product, product.MyQuantity);
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