using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using MyNoddyStore.Abstract;
using MyNoddyStore.Entities;
using MyNoddyStore.Models;


namespace MyNoddyStore.Controllers
{
    public class CartController : Controller
    {
        private IProductRepository repository;
        private IOrderProcessor orderProcessor;
        private string messageString;

        public CartController(IProductRepository repo, IOrderProcessor proc)
        {
            repository = repo;
            orderProcessor = proc;
        }

        public ViewResult Index(Cart cart, string returnUrl)
        {
            ViewBag.remainingTime = 908; //todo remove this??

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
            if (submitUpdate == null) { // User has selected "View Cart"
                return RedirectToAction("Index", new { returnUrl });
            }
            else // User has selected "Update Cart"
            {
                //store the pageNumber and categoryString params in temp data (this is a bodge)
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("page", pageNumber);
                dict.Add("category", categoryString);  //todo handle null
                TempData["myDictionary"] = dict; // Store it in the TempData

                Product product = repository.Products.FirstOrDefault(p => p.ProductID == productId);
                if (product != null)
                {
                    //cart.AddItem(product, 1);
                    cart.AddItem(product, MyQuantity);

                    //todo decide how to correlate cart line and updated values.
                    messageString = "Update successful";
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
            return PartialView(cart);
        }

        [HttpGet]
         public ViewResult Checkout()
        {
            return View(new ShippingDetails());
        }

        [HttpPost]
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

    }
}