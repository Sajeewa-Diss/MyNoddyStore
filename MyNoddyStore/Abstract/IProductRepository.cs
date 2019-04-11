using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNoddyStore.Entities;

namespace MyNoddyStore.Abstract
{
    public interface IProductRepository
    {
        IEnumerable<Product> Products { get; }
        
        //string TestConnection();

    }
}