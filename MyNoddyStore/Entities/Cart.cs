using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNoddyStore.Entities
{
    //this object represents two carts, the user cart and the NPC cart.
    public class Cart
    {
        private List<CartLine> lineCollection = new List<CartLine>();
        private List<CartLine> lineCollectionOther = new List<CartLine>();

        #region legacy pattern code
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
        #endregion

        //Add or update product in the user cartline. 
        //note all product object details has been cross-checked before this method call.
        public void AddItem(Product productIn)
        {
            CartLine line = lineCollection
            .Where(p => p.Product.ProductID == productIn.ProductID)
            .FirstOrDefault();
            if (line == null)
            {
                lineCollection.Add(new CartLine
                {
                    Product = productIn,
                    Quantity = productIn.MyQuantity 
                });
            }
            else
            {
                line.Product = productIn;             //refresh the product data with values passed in. 
                line.Quantity = productIn.MyQuantity; //we use MyQuantity value as a getter elsewhere.
            }
        }

        //Add or update product in the NPC cartline.
        //note that all product object details have been cross-checked before this method call.
        public void AddItemOther(Product productIn)
        {
            CartLine line = lineCollectionOther
            .Where(p => p.Product.ProductID == productIn.ProductID)
            .FirstOrDefault();
            if (line == null)
            {
                lineCollectionOther.Add(new CartLine
                {
                    Product = productIn,               //refresh the product data with values passed in. 
                    Quantity = productIn.OtherQuantity //we use OtherQuantity value as a getter elsewhere.
                });
            }
            else
            {
                line.Product = productIn;             //refresh the product data with values passed in. 
                line.Quantity = productIn.OtherQuantity; //we use OtherQuantity value as a getter elsewhere.
            }
        }

        //This method is not req'd by the NPC so only applied to the user cart line (if they choose to use it).
        public void RemoveLine(Product productIn)
        {
            lineCollection.RemoveAll(l => l.Product.ProductID == productIn.ProductID);
        }


        //adds an empty line item to LineCollectionOther (if not already extant).
        public void AddEmptyLineOther(Product productIn)
        {
            CartLine line = lineCollectionOther
            .Where(p => p.Product.ProductID == productIn.ProductID)
            .FirstOrDefault();
            if (line == null)
            {
                productIn.OtherQuantity = 0; //no items added yet.
                lineCollectionOther.Add(new CartLine
                {
                    Product = productIn,
                    Quantity = productIn.OtherQuantity
                });
            }
            else
            {
                //do nothing if line is extant.
            }
        }

        public decimal ComputeTotalValue()
        {
            return lineCollection.Sum(e => e.Product.Price * e.Quantity);
        }

        public decimal ComputeTotalValueOther()
        {
            return lineCollectionOther.Sum(e => e.Product.Price * e.Quantity);
        }

        public int ComputeTotalQuantities()
        {
            return lineCollection.Sum(e => e.Quantity);
        }

        public int ComputeTotalQuantitiesOther()
        {
            return lineCollectionOther.Sum(e => e.Quantity);
        }

        public void Clear()
        {
            lineCollection.Clear();
        }

        public IEnumerable<CartLine> Lines
        {
            get { return lineCollection; }
            set { lineCollection = value.ToList(); }
        }

        public IEnumerable<CartLine> LinesOther
        {
            get { return lineCollectionOther; }
            set { lineCollectionOther = value.ToList(); }
        }

        public int LinesCount
        {
            get { return lineCollection.Count(); }
        }

        public int LinesOtherCount
        {
            get { return lineCollectionOther.Count(); }
        }
    }

    public class CartLine
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }       //note this property is an exact mirror of the product's MyQuanity or OtherQuantity property, 
                                                 //used by the user-player and NPC-player respectively in their respective line collections.
    }

    //a class to amalgamate user and NPC shopping cart results.
    public class MergedCartLine
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public int QuantityOther { get; set; }

        public decimal ComputedUserTotal
        {
            get { return Product.Price * Quantity; }
        }

        public decimal ComputedAITotal
        {
            get { return Product.Price * QuantityOther; }
        }
    }

    //simple IEqualityComparer for merging cartlines (a noddy demo of its use).
    public class SimpleCartLineComparer : IEqualityComparer<MergedCartLine>
    {
        public bool Equals(MergedCartLine x, MergedCartLine y)
        {
            return x.Product.ProductID == y.Product.ProductID;
        }

        public int GetHashCode(MergedCartLine obj)
        {
            return obj.Product.ProductID.GetHashCode();
        }
    }
}
