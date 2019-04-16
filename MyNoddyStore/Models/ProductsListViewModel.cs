using System.Collections.Generic;
using MyNoddyStore.Entities;

namespace MyNoddyStore.Models
{
    public class ProductsListViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        //public Cart Cart1 { get; set; }               //todo remove??
        public PagingInfo PagingInfo { get; set; }
        public string CurrentCategory { get; set; }
        public double CountDownMilliseconds { get; set; }
        public int UpdatedProductId { get; set; }
        public string UpdatedMessage { get; set; }
    }
}