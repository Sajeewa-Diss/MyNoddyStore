using System;
using System.Text;
//using System.Web.SessionState;
//using Microsoft.AspNet.Session;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
//using Newtonsoft.Json;
using MyNoddyStore.Abstract;
using MyNoddyStore.Models;
using MyNoddyStore.Entities;

namespace MyNoddyStore.HtmlHelpers
{
    public static class AdHocHelpers   // a public static class allows extension method.
    {
        public const int simulatedShoppingItemLimit = 60;
        public const int shoppingTimeMilliseconds = 61000;
        public const int shoppingStartDelayMilliseconds = 5000;  //sets the head-start given to human-player.
        public const int maxCartLineItemLimit = 5;               //per-line limit in shopping cart (note this const is also matched in View Javascript). 

        //Helper methods required for paging.
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> sequence)
        {
            return sequence.Where(e => e != null);
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence)
            where T : struct
        {
            return sequence.Where(e => e != null).Select(e => e.Value);
        }

        public static IEnumerable<T> EmptyArrayIfNull<T>(this IEnumerable<T> sequence)
        {
            if (sequence == null) {
                //string[] emptyArray = new string[] { };            // handle a product with no categories by returning an empty array object (non-generic).
                //T[] emptyArray = new T[] { };                      // handle a product with no categories by returning an empty array object.
                //return emptyArray.Cast<T>();                       // we could cast to type T (this is valid but superfluous).
                return new T[] { };
            }
            else
            {
                return sequence;
            }
        }
        
        //Helper methods using the session object.
        public static T GetDataFromSession<T>(this HttpSessionStateBase session, string key)  //todo use this values and change this class name or delete these methods
        {
            try { return (T)session[key]; }
            catch { return default(T); }
        }
        /// <summary> 
        /// Set value. 
        /// </summary> 
        /// <typeparam name="T"></typeparam> 
        /// <param name="session"></param> 
        /// <param name="key"></param> 
        /// <param name="value"></param> 
        public static void SetDataToSession<T>(this HttpSessionStateBase session, string key, object value)
        {
            session[key] = value;
        }

        public static int GetLastItemAddedByOtherPlayer(this HttpSessionStateBase session)
        {
            return session.GetDataFromSession<int>("lastItemAdded");
        }

        public static void SetLastItemAddedByAIPlayer(this HttpSessionStateBase session, int value)
        {
            session.SetDataToSession<int>("lastItemAdded", value);
        }

        //used to cycle through the products inventory. 
        public static bool GetShoppingByNpcCompleted(this HttpSessionStateBase session)
        {
            return session.GetDataFromSession<bool>("shoppingCompleted");
        }

        public static void SetShoppingByNpcCompleted(this HttpSessionStateBase session, bool value)
        {
            session.SetDataToSession<bool>("shoppingCompleted", value);
        }

        //if countdown already started, return the remaining time. Otherwise return a start value.
        public static int GetRemainingTimeOrSetDefault(this HttpSessionStateBase session)
        {
            int remainingMilliseconds; // countdown time variable

            DateTime countdownTime = session.GetDataFromSession<DateTime>("countdownTimeCsKey");

            if (countdownTime == DateTime.MinValue)
            {
                //set a new countdown time using a global constant.
                session.SetDataToSession<string>("countdownTimeCsKey", DateTime.Now.AddMilliseconds(shoppingTimeMilliseconds));
                remainingMilliseconds = shoppingTimeMilliseconds;
            }
            else
            {
                TimeSpan tsRemaining = countdownTime - DateTime.Now;
                remainingMilliseconds = (int)tsRemaining.TotalMilliseconds;  //convert to integer for passing to view.
            }
            return remainingMilliseconds;
        }

        //if countdown already started, return this value. Otherwise return int.Minvalue.
        public static int GetRemainingTime(this HttpSessionStateBase session)
        {
            int remainingMilliseconds; // countdown time variable
            DateTime countdownTime = session.GetDataFromSession<DateTime>("countdownTimeCsKey");

            if (countdownTime == DateTime.MinValue)
            {
                remainingMilliseconds = int.MinValue;
            }
            else
            {
                TimeSpan tsRemaining = countdownTime - DateTime.Now;
                remainingMilliseconds = (int)tsRemaining.TotalMilliseconds;  //convert to integer for passing to view.
            }
            return remainingMilliseconds;
        }

        //Simulate another user shopping up to this point in time or until the end of the sweep time-period.
        //This method will add one item to cart per second of shopping allowed or remaining.
        public static void RunAISweep(this HttpSessionStateBase session, Cart cart, IEnumerable<Product> prodlist, bool shopToEnd = false)
        {
            List<Product> prodList = new List<Product>();

            //System.Diagnostics.Debug.WriteLine("simulate sweep method entered");

            int lastProdId = 0;
            int numItemsToAdd = 0;

            //ensure that the sweep user hasn't yet finished
            bool sweepCompleted = session.GetShoppingByNpcCompleted(); //todo set this somewhere else to remove null reference exception  - check for all other such objects.
            if (sweepCompleted)
            {
                return;          //Operation has completed. No need to simulate shopping.
            }

            int delayMilliseconds = shoppingStartDelayMilliseconds;
            int maxItemLimit = simulatedShoppingItemLimit;
            int totalMilliseconds = shoppingTimeMilliseconds;
            int currentItemQuantity = SumOtherQuantity(cart);

            //ensure we are within time. If so, calculate number of seconds of shopping time to simulate. If not, shop to end of period.
            int remainingMilliseconds = session.GetRemainingTime();
            if (remainingMilliseconds <= 0)
            { //if shopping time has expired, then shop to the end of the time period (and set appropriate flags).
                shopToEnd = true;
            } else {
                //don't start AI shopping until set time from start
                if(delayMilliseconds > totalMilliseconds - remainingMilliseconds)
                {
                    return; 
                }
            }

            //if shopping to end, shop for all remaining items. Else add items with respect to the current-expired time only (as a simple ratio, say).
            if (shopToEnd)
            {
                numItemsToAdd = maxItemLimit - currentItemQuantity;
            }
            else
            { // some casting ius req'd to stop integer ratios tending to zero.
                numItemsToAdd = (int)(maxItemLimit * (((double)totalMilliseconds - (double)remainingMilliseconds) / (double)totalMilliseconds)) - currentItemQuantity;
            }

            int rendom1or2 = session.GetCountdownRandomizerValue();
            lastProdId = session.GetLastItemAddedByOtherPlayer();

            //call the sweep do method. An updated last prod id will be returned upon completion.
            lastProdId = DoSweep(cart, prodlist, numItemsToAdd, lastProdId, rendom1or2);

            //set the two static properties.
            session.SetLastItemAddedByAIPlayer(lastProdId);
            if (shopToEnd) //if shopping time has completed, set appropriate flag.
            {
                session.SetShoppingByNpcCompleted(true);
            }

        }

        //Add the specified number of items to the AI user's cartline.
        private static int DoSweep(Cart cart, IEnumerable<Product> prodlist, int numItems, int lastProdId, int random1or2)
        {
            //if no items yet in AI cartline, add five of the ten most expensive lines, randomly (but add no quantities yet). 
            if (cart.LinesOtherCount == 0)
            {
                AddFiveNewItemsToOtherLine(cart, prodlist, random1or2);
            }

            //Also add all current cartlines in human user's cart to AI cart (but add no quantities yet).
            AddUserCartLinesToAICartLines(cart);

            //Cycle through cartlines already created (from correct continue position) buying one item from each line until required number of items have been added.
            int returnedProdId = 0;
            returnedProdId = AddItemsToAICartlinesCyclically(cart, numItems, lastProdId);


            //if no more items possible to buy (if max limit or stockcount has been used), choose next product not on list and add it to cartline.
            //This time buy as many items as possible and so on until the required number has been reached.



            //rturn the id of the last item added to cart.
            return returnedProdId;
        }

        private static int AddItemsToAICartlinesCyclically(Cart cart, int numItems, int lastProdBought)
        {
            //for (int i = function2(); i < 100 /*where 100 is the end or another function call to get the end*/; i = function2())
            //{

            //    try
            //    {
            //        //ToDo
            //    }
            //    catch { continue; }
            //}

            return 1;


        }


        //Mirror the user cartlines in AI cartline (to mirror the cart-adding behaviour).
        private static void AddUserCartLinesToAICartLines(Cart cart)
        {
            //first balance the items in current repository
            foreach (CartLine line in cart.Lines)
            {
                cart.AddEmptyLineOther(line.Product);
            }
        }

        private static void AddFiveNewItemsToOtherLine(Cart cart, IEnumerable<Product> prodlist, int random)
        {
            //first balance the items in current repository
            BalanceRepositoryWrtCart(cart, prodlist);

            //next get the ten most expensive items still in stock.
            IEnumerable<Product> dearItems = prodlist.OrderByDescending(e => e.Price)
                        .Where(e => e.StockCount > 0)
                        .Take(10)                          // takes the top 10 items (after the sort)
                        .OrderBy(e => e.ProductID);        //re-order by product id

            //get every nth item stating at 0 or 1 randomly.
            int nth = 2; //.i.e. every second item.
            int skipper = random - 1; //we either skip 1 item or skip zero.
            IEnumerable<Product>  myFiveItems = dearItems.Skip(skipper).Where((x, i) => i % nth == 0);
            //.Select(e => new { e.Name, e.Price });
            AddEmptyLinesToOtherCart(cart, myFiveItems);
        }

        private static void AddEmptyLinesToOtherCart(Cart cart, IEnumerable<Product> prodList)
        {
            //add the items to LinesOther if not yet included (no quantity will be added).
            foreach(Product pr in prodList)
            {
                cart.AddEmptyLineOther(pr);
            }
        }

        private static int GetCountdownRandomizerValue(this HttpSessionStateBase session)
        {
            //returns 1 or 2 randomly based on the session countdown start time.
            int milliseconds = session.GetDataFromSession<DateTime>("countdownTimeCsKey").Millisecond;
            return new System.Random(milliseconds).Next(0, 2) + 1;
        }

        //balance repository items for all products
        private static void BalanceRepositoryWrtCart(Cart cart, IEnumerable<Product> repository)
        {
            foreach (Product pr in repository)
            {
                BalanceCurrentProductStock(cart, pr);
            }
        }

        private static int SumOtherQuantity(Cart cart)
        {
            int total = 0;
            foreach (var item in cart.LinesOther)
            {
                total += item.Quantity;
            }
            return total;
        }

        //Balance stock and quantities in current cart update request.
        public static string BalanceCartTransaction(this Cart cart, Product product, int newQuantity)
        {

            //update the product stock details using the current cart.
            BalanceCurrentProductStock(cart, product);

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
            if (newQuantity > 0)
            {
                if (product.StockCount >= newQuantity) // the update can be done
                {
                    product.MyQuantity = newQuantity;
                    product.StockCount -= newQuantity;
                    cart.AddItem(product);
                    messageString = "Updated";
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

            //todo if time expired. Append extra message text.

            return messageString;
        }

        //Balance this product's stock details using cart info.
        private static void BalanceCurrentProductStock(Cart cart, Product product)
        {
            //update the product stock details using the current cart.
            CartLine line = cart.Lines
                    .Where(p => p.Product.ProductID == product.ProductID)
                    .FirstOrDefault();
            if (line == null)
            {
                //no matching item in cart
                product.MyQuantity = 0;
            }
            else
            {
                //matching item found
                product.MyQuantity = line.Quantity;
            }

            //also update any data from other line
            CartLine lineOther = cart.LinesOther
                .Where(p => p.Product.ProductID == product.ProductID)
                .FirstOrDefault();
            if (lineOther == null)
            {
                //no matching item in cart
                product.OtherQuantity = 0;
            }
            else
            {
                //matching item found
                product.OtherQuantity = lineOther.Quantity;
            }

            product.StockCount = product.InitialStockCount - product.MyQuantity - product.OtherQuantity;
        }


        #region redundant methods. Keep as reference
        //public static void SetObjectAsJson(this ISession session, string key, object value) //todo use this values and change this class name or delete these methods
        //{
        //    session.SetString(key, JsonConvert.SerializeObject(value));
        //}

        //public static T GetObjectFromJson<T>(this ISession session, string key)
        //{
        //    var value = session.GetString(key);

        //    return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        //}
        #endregion

    }
}