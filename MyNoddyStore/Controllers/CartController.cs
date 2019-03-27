using System.Linq;
using System.Web.Mvc;
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
            return View(new CartIndexViewModel
            {
                ReturnUrl = returnUrl,
                UpdateMessage = messageString,
                Cart = cart
            });
        }

        public RedirectToRouteResult AddToCart(Cart cart, int productId, int myQuantity, string returnUrl)
        {
            Product product = repository.Products
            .FirstOrDefault(p => p.ProductID == productId);
            if (product != null)
            {
                //cart.AddItem(product, 1);
                cart.AddItem(product, myQuantity);

                //todo decide how to correlate cart line and updated values.
                messageString = "Update successful";
            }
            return RedirectToAction("Index", new { returnUrl });
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
                line.Product.myQuantity = line.Quantity;
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