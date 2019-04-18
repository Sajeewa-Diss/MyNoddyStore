using System.Collections.Generic;
using MyNoddyStore.Entities;

namespace MyNoddyStore.Models
{
    public class ProductsListViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public int TotalQuantity { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public string CurrentCategory { get; set; }
        public double CountDownMilliseconds { get; set; }
        public int UpdatedProductId { get; set; }
        public string UpdatedMessage { get; set; }
    }
}