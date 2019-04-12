using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNoddyStore.Entities;
using System.Data.Entity;
using System.Data.SqlClient;

namespace MyNoddyStore.Concrete
{

    #region legacy pattern
    //class EFDbContext : DbContext
    //{
    //    public DbSet<Product> Products { get; set; }


    //    public string TestConnection()
    //    {
    //        try
    //        {
    //            this.Database.Connection.Open();
    //            this.Database.Connection.Close();
    //        }
    //        catch (SqlException)
    //        {
    //            return "connection error";
    //        }
    //        return "connection success";
    //    }

    //}
    #endregion
}
