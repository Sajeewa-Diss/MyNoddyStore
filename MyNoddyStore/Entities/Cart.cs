using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNoddyStore.Entities
{
    public class Cart
    {
        public List<CartLine> lineCollection = new List<CartLine>(); //todo keep this private??
        public List<CartLine> lineCollectionOther = new List<CartLine>();
        
        //legacy method in original pattern
        //public void AddItem(Product product, int quantity)
        //{
        //    CartLine line = lineCollection
        //    .Where(p => p.Product.ProductID == product.ProductID)
        //    .FirstOrDefault();
        //    if (line == null)
        //    {
        //        lineCollection.Add(new CartLine
        //        {
        //            Product = product,
        //            Quantity = quantity
        //        });
        //    }
        //    else
        //    {
        //        line.Quantity += quantity;
        //    }
        //}

        //note that both the product.MyQuantity property and line.Quantity property will be matched within this method.
        public void AddItem(Product product)
        {
            CartLine line = lineCollection
            .Where(p => p.Product.ProductID == product.ProductID)
            .FirstOrDefault();
            if (line == null)
            {
                lineCollection.Add(new CartLine
                {
                    Product = product,
                    Quantity = product.MyQuantity
                });
            }
            else
            {
                line.Quantity = product.MyQuantity; //we use MyQuantity value as a getter elsewhere.
            }
        }

        public void RemoveLine(Product product)
        {
            lineCollection.RemoveAll(l => l.Product.ProductID == product.ProductID);
        }

        public decimal ComputeTotalValue()
        {
            return lineCollection.Sum(e => e.Product.Price * e.Quantity);
        }

        public void Clear()
        {
            lineCollection.Clear();
        }

        public IEnumerable<CartLine> Lines
        {
            get { return lineCollection; }
        }

        public IEnumerable<CartLine> LinesOther
        {
            get { return lineCollectionOther; }
        }
    }

    public class CartLine
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }       //note this property is an exact mirror of the product's MyQuanity or OtherQuantity property, 
                                                 //used by the user-player and computer-player respectively in their respective line collections.
    }
}

//public int UpdateCartCount(int id, int cartCount)
//{
//    // Get the cart 
//    var cartItem = storeDB.Carts.Single(
//        cart => cart.CartId == ShoppingCartId
//        && cart.RecordId == id);

//    int itemCount = 0;

//    if (cartItem != null)
//    {
//        if (cartCount > 0)
//        {
//            cartItem.Count = cartCount;
//            itemCount = cartItem.Count;
//        }
//        else
//        {
//            storeDB.Carts.Remove(cartItem);
//        }
//        // Save changes 
//        storeDB.SaveChanges();
//    }
//    return itemCount;
//}