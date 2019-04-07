using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyNoddyStore.Abstract;
using MyNoddyStore.Entities;

namespace MyNoddyStore.Concrete
{
    public class HomeService : ICartService
    {
        private readonly string _data;

        public HomeService(string data)
        {
            _data = data;
        }

        public string GetSomeData()
        {
            return _data;
        }
    }
}