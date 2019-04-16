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
        public const int NpcShoppingItemLimit = 60;
        public const int shoppingTimeMilliseconds = 61000;
        public const int shoppingStartDelayMilliseconds = 5000;  //sets the head-start given to human-player.
        public const int maxCartLineItemLimit = 5;               //per-line limit in shopping cart (note this const is also matched in View page Javascript). 

        #region AdHoc Helper Methods
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


        public static bool GetShoppingByNpcCompleted(this HttpSessionStateBase session)
        {
            return session.GetDataFromSession<bool>("shoppingCompleted");
        }

        public static void SetShoppingByNpcCompleted(this HttpSessionStateBase session, bool value)
        {
            session.SetDataToSession<bool>("shoppingCompleted", value);
        }

        //if countdown already started, return the remaining time. Otherwise return a start-time value.
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
        #endregion

        //Simulate NPC shopping up to this point in time or until the end of the sweep time-period.
        //This method will add one items to cart at the rate specified by the constants above (currently an item per second).
        public static void RunNpcSweep(this HttpSessionStateBase session, Cart cart, IEnumerable<Product> repoProdList, bool shopToEnd = false)
        {
            //int testNPCquantity = 20;
            
            //ensure that the sweep user hasn't yet finished
            bool sweepCompleted = session.GetShoppingByNpcCompleted(); //todo set this somewhere else to remove null reference exception  - check for all other such objects.
            if (sweepCompleted)
            {
                return;          //Operation has completed. No need to run cart sweep simulation.
            }

            int delayMilliseconds = shoppingStartDelayMilliseconds;
            int maxItemLimit = NpcShoppingItemLimit;
            int totalMilliseconds = shoppingTimeMilliseconds;
            int currentTotalNpcQuantities = SumNpcQuantities(cart);

            //if (currentTotalNpcQuantities == testNPCquantity) { return; } //todo remove

            //ensure we are within time. If so, calculate number of seconds of shopping time to simulate. If not, shop to end of period.
                int remainingMilliseconds = session.GetRemainingTime();
            if (remainingMilliseconds <= 0)
            { //if shopping time has expired, then shop to the end of the time period (NPC always completes a full sweep successfully) and set appropriate flag later.
                shopToEnd = true;
            } else {
                //Delay NPC sweep until set time from start.
                if(delayMilliseconds > totalMilliseconds - remainingMilliseconds)
                {
                    return; 
                }
            }

            //initialise req'd variables
            //List<Product> prodList = repoProdList.ToList<Product>();  //new List<Product>(); todo remove?????
            int lastProdId = 0;
            int numItemsToAdd = 0;

            //if shopping to end, shop for all remaining items. Else add items with respect to the current-expired time only (as a simple ratio, say).
            if (shopToEnd)
            {
                numItemsToAdd = maxItemLimit - currentTotalNpcQuantities;
            }
            else
            { // some casting is req'd to stop integer ratios tending to zero.
                numItemsToAdd = (int)(maxItemLimit * (((double)totalMilliseconds - (double)remainingMilliseconds) / (double)totalMilliseconds)) - currentTotalNpcQuantities;
            }

            //numItemsToAdd = testNPCquantity;  //todo reset
            //shopToEnd = false; //todo reset

            int rendom1or2 = session.GetCountdownRandomizerValue();  //set this variable from a session get.
            lastProdId = session.GetLastItemAddedByOtherPlayer();    //ditto

            //call the sweep do method. An updated last prod id will be returned upon completion.

            TimeSpan ts1 = new TimeSpan(DateTime.UtcNow.Ticks);
            double ms1 = ts1.TotalMilliseconds;
            //System.Diagnostics.Debug.WriteLine("start of calling dosweep" + DateTime.Now.Ticks.ToString());
            lastProdId = DoSweep(cart, repoProdList, numItemsToAdd, lastProdId, rendom1or2);
            TimeSpan ts2 = new TimeSpan(DateTime.UtcNow.Ticks);
            double ms2 = ts2.TotalMilliseconds;

            System.Diagnostics.Debug.WriteLine("timespan milliseconds: " + (ms2 - ms1).ToString());


            //set session properties if req'd.
            session.SetLastItemAddedByAIPlayer(lastProdId);
            if (shopToEnd) //if shopping time has completed, set appropriate flag.
            {
                session.SetShoppingByNpcCompleted(true);
            }

        }

        //Balance stock and quantities in current cart update request.
        public static string BalanceCartTransaction(this Cart cart, Product product, int newQuantity)
        {
            //first update the product stock details using the current cart.
            BalanceCurrentProductStockWrtCart(cart, product);

            string messageString = "";

            if (newQuantity < 0 || newQuantity > maxCartLineItemLimit)
            {
                messageString = "invalid number of items";
            }

            //return this product's current quantity to stock.
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
                    messageString = "Part added (no stock)";
                }
                else  // the update can't be done. No stock.
                {
                    messageString = "Failed (no stock)";
                }
            }

            //todo if time expired. Append extra message text.
            //User is allowed to continue sweep after time has expired (just because this is a shopping demo). But a warning message is appended.
            //the NPC cart would have stopped sweep, so only the success message will be updated in practice.

            return messageString;
        }

        #region private methods
        //Add the specified number of items to the AI user's cartline.
        private static int DoSweep(Cart cart, IEnumerable<Product> prodlist, int numItems, int lastProdIdAdded, int random1or2)
        {
            int numItemsRemaining = numItems;

            //first balance the items in current repository with cart quantities (to update which items still in stock).
            BalanceRepositoryWrtCart(cart, prodlist);

            //if no items yet in AI cartline, add five of the ten most expensive lines, randomly (but add no quantities yet). 
            if (cart.LinesOtherCount == 0)
            {
                AddFiveNewItemsToNpcLine(cart, prodlist, random1or2);
            }

            //Also add all current cartlines in human user's cart to AI cart (but add no quantities yet).
            AddUserCartLinesToAICartLines(cart);

            //Cycle through cartlines already created (from correct continue position) buying one item from each line until required number of items have been added.
            //Do this in each sweep in case user player has returned items to stock (just because they can).
            int returnedProdId = 0;
            returnedProdId = AddItemsToNpcCartlinesCyclically(cart, prodlist, ref numItemsRemaining, lastProdIdAdded);

            //int numItemsStillRemaining = numItemsRemaining;

            //if no more items possible to buy (if max limit or stockcount has been used), choose next product not on list and add it to cartline.
            //This time buy as many items as possible and so on until the required number has been reached.
            if (numItemsRemaining > 0)
            {
                returnedProdId = AddItemsToNpcCartlinesInBlocks(cart, prodlist, numItemsRemaining, returnedProdId);
            }

            //rturn the id of the last item added to cart.
            return returnedProdId;
        }

        //Add an item to each current NPC cartline until the req'd number of items added, or until all possible items added.
        //returns the last prod id successfully added to NPC cart.
        private static int AddItemsToNpcCartlinesCyclically(Cart cart, IEnumerable<Product> prodlist, ref int numItemsRemaining, int lastProdIdAdded)     //todo remove prodlist argument??
        {
            int numItemsToAdd = numItemsRemaining;
            int numItemsAdded = 0;
            int previousProdId = lastProdIdAdded;

            while (numItemsToAdd != numItemsAdded)
            {
                //deduce the next NPC cartline to try to increment
                CartLine nextLineOther = (CartLine)cart.LinesOther 
                    .Where(c => c.Product.ProductID > previousProdId) //todo question how to cycle from end back to begining of prod list??
                    .Where(c => c.Product.OtherQuantity < maxCartLineItemLimit)
                    .Where(c => c.Product.StockCount > 0)
                    .OrderBy(c =>c.Product.ProductID)
                    .FirstOrDefault();

                if (nextLineOther != null)
                {
                    //get the product to add.
                    Product prodToAdd = nextLineOther.Product;
                    //when adding to NPC cart cyclically, we always increment in steps of one only.
                    int returnIncr = cart.TryIncrementNpcCart(prodToAdd, 1);
                    if (returnIncr == 1)
                    {
                        numItemsAdded += 1;
                        previousProdId = prodToAdd.ProductID;
                        lastProdIdAdded = prodToAdd.ProductID;
                    }
                }
                else //no qualifying lines found that can be incremeneted.
                {
                    //check if previousProdId is zero (then we have cycled all the way through the cart line without finding a line to incremenet).
                    if (previousProdId == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("break out of loop"); //todo remove all debug.writeline statements.
                        break;
                    }
                    previousProdId = 0; //go back to start of list
                    System.Diagnostics.Debug.WriteLine("back to top of list");
                }
                //previousProdId += 1; //in each iteration, increment the product id, reset from end of list if req'd.
                //previousProdId = ((previousProdId + 1) % prodlist.Count()) - 1; //Warning! This expression relies on all items in inventory being numbered from 1 to n with no gaps.
                
            
            /*
                 Product prodToAdd = cart.LinesOther.pr .p.(p => p.Product.ProductID == product.ProductID)
               //no matching item in cart
               product.OtherQuantity = 0;
            else
               //matching item found
               product.OtherQuantity = lineOther.Quantity;
            product.StockCount = product.InitialStockCount - product.MyQuantity - product.OtherQuantity;
                                */
            }
            numItemsRemaining = numItemsToAdd - numItemsAdded; //this result is passed back by ref.
            return lastProdIdAdded;
        }

        //Increment NPC cart with a product and additional quantity and then return the actual additional quantity successfully added.
        private static int TryIncrementNpcCart(this Cart cart, Product product, int increment)
        {
            ////first update the product stock details using the current cart.
            ////BalanceCurrentProductStockWrtCart(cart, product);
            int returnIncrement = 0;

            while (returnIncrement != increment) //loop until we have incremented the req'd amount.
            {
                //deduce the product's NPC cart quantity
                int npcCartQuantity = product.OtherQuantity;

                if (npcCartQuantity >= maxCartLineItemLimit)
                {
                    break; //abort the loop (returns the quantity added so far).
                }

                //deduce the stock availablity.
                if (product.StockCount <= 0)
                {
                    break; //abort the loop (returns the quantity added so far).
                }

                //increment
                product.OtherQuantity = product.OtherQuantity + 1;
                product.StockCount -= 1;
                returnIncrement += 1;
                cart.AddItemOther(product);
                //lastProdIdAdded = product.ProductID;
            }
            //todo if time expired. Append extra message text.
            //User is allowed to continue sweep after time has expired (just because this is a shopping demo). But a warning message is appended.
            //the NPC cart would have stopped sweep, so only the success message will be updated in practice.

            return returnIncrement;
        }

















        //Add max possible items to a new NPC cartline until the required number is reached.
        private static int AddItemsToNpcCartlinesInBlocks(Cart cart, IEnumerable<Product> prodlist, int numItems, int lastProdIdAdded)
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


        //Mirror the user cartlines in AI cartline (to mimic the cart-adding behaviour).
        private static void AddUserCartLinesToAICartLines(Cart cart)
        {
            foreach (CartLine line in cart.Lines)
            {
                cart.AddEmptyLineOther(line.Product);
            }
        }

        //if no items yet in AI cartline, add five of the ten most expensive lines, randomly (but add no quantities yet). 
        private static void AddFiveNewItemsToNpcLine(Cart cart, IEnumerable<Product> prodlist, int random1or2)
        {
            //next get the ten most expensive items still in stock.
            IEnumerable<Product> dearItems = prodlist.OrderByDescending(e => e.Price)
                        .Where(e => e.StockCount > 0)
                        .Take(10)                          // takes the top 10 items (after the sort)
                        .OrderBy(e => e.ProductID);        //re-order by product id

            //get every nth item stating at 0 or 1 randomly.
            int nth = 2; //.i.e. every second item.
            int skipper = random1or2 - 1; //we either skip 1 item or skip zero.
            IEnumerable<Product>  npcFiveItems = dearItems.Skip(skipper).Where((x, i) => i % nth == 0);
            //.Select(e => new { e.Name, e.Price });
            AddEmptyLinesToOtherCart(cart, npcFiveItems);
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
                BalanceCurrentProductStockWrtCart(cart, pr);
            }
        }

        private static int SumNpcQuantities(Cart cart)
        {
            int total = 0;
            foreach (var item in cart.LinesOther)
            {
                total += item.Quantity;
            }
            return total;
        }

        //Balance this product's stock details using cart info.
        private static void BalanceCurrentProductStockWrtCart(Cart cart, Product product)
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

            //also update any data from NPC line
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
            //finally balance the remaining stock value.
            product.StockCount = product.InitialStockCount - product.MyQuantity - product.OtherQuantity;
        }
        #endregion

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