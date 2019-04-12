using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNoddyStore.Entities
{
    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string Picture { get; set; }
        public decimal Price { get; set; }
        //public string Category { get; set; } //legacy
        public string[] Categories { get; set; }
        public int InitialStockCount { get; set; }
        public int StockCount { get; set; }
        public int MyQuantity { get; set; }
        public int OtherQuantity { get; set; }
    }
}
