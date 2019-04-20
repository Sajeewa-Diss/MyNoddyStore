using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyNoddyStore.Entities;

namespace MyNoddyStore.Models
{
    public class CartIndexViewModel
    {
        public Cart Cart { get; set; }
        public string ReturnUrl { get; set; }
        public double CountDownMilliseconds { get; set; }
    }
}