using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNoddyStore.Entities
{
    public class Cart
    {
        private List<CartLine> lineCollection = new List<CartLine>(); //todo keep this private??
        private List<CartLine> lineCollectionOther = new List<CartLine>();
        
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


        //adds an empty line item to LineCollectionOther (if not already extant).
        public void AddEmptyLineOther(Product product)
        {
            CartLine line = lineCollectionOther
            .Where(p => p.Product.ProductID == product.ProductID)
            .FirstOrDefault();
            if (line == null)
            {
                product.OtherQuantity = 0; //no items added yet.
                lineCollectionOther.Add(new CartLine
                {
                    Product = product,
                    Quantity = product.OtherQuantity
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

        public int LinesOtherCount
        {
            get { return lineCollectionOther.Count(); }
        }
    }

    public class CartLine
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }       //note this property is an exact mirror of the product's MyQuanity or OtherQuantity property, 
                                                 //used by the user-player and AI-player respectively in their respective line collections.
    }

    //a class to amalgamate user and AI shopping cart results.
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

    //simple IComparer for merging cartlines (a noddy demo of its use)
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
