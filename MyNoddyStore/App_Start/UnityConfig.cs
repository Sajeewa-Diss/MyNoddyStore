using System;
using Unity;
using Unity.Injection;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Moq;
using MyNoddyStore.Abstract;
using MyNoddyStore.Concrete;
using MyNoddyStore.Entities;
//using Unity.Injection;
//using System.Configuration;

namespace MyNoddyStore
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            EFProductRepository newRepo = new EFProductRepository();

            // Register your type's mappings here.
            #region mock construct
            //var mock = new Mock<IProductRepository>();
            //mock.Setup(m => m.Products).Returns(GetProductsList());
            //container.RegisterInstance<IProductRepository>(mock.Object); //note this repository object is shared by all browser sessions.
            #endregion

            //use Hangfire somewhere??.
            container.RegisterInstance<IProductRepository>(newRepo); //note this is a singleton shared by all controllers, and by all borwser requests.

            #region container examples
            //container.RegisterType<IOrderProcessor, EmailOrderProcessor>(new InjectionConstructor(emailSettings));

            //container.RegisterType<IEmail, Email>(new InjectionFactory(c => new Email("To Name","to@email.com")));
            //var email = container.Resolve<IEmail>();
            //container.RegisterType<OperationEntity>("email", new ContainerControlledLifetimeManager(), new InjectionConstructor(email));

            //container.RegisterType<IEmail, Email>(new ContainerControlledLifetimeManager(), new InjectionFactory(c => new Email("To Name","to@email.com")));
            //var opEntity = container.Resolve<OperationEntity>();

            //container.RegisterType<IEmail, Email>(new ContainerControlledLifetimeManager());

            //    container.RegisterType<IDemo, Demo>(new ContainerControlledLifetimeManager());
            //    var demoA = container.Resolve<IDemo>();
            //    var demoB = container.Resolve<IDemo>();
            #endregion

        }

        //private static List<Product> GetProductsList()
        //{
        //    List<Product>productList = new List<Product>{
        //        new Product { ProductID = 1, Name = "Aadvark", Description = "Customers who ordered this also ordered: Termite mounds.", ShortDescription = "Ant-free zone", Picture = "aadvark",
        //               Categories = new string[] { "Pets" }, Price = 900M, InitialStockCount = 4, StockCount = 4, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 2, Name = "Camera", Description = "A single item in stock.", ShortDescription = "Digital SLR", Picture = "camera",
        //               Categories = new string[] { "Gifts" }, Price = 450M, InitialStockCount = 1, StockCount = 1, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 3, Name = "Caviar", Description = "Luxury Edition.", ShortDescription = "Beluga", Picture = "caviar",
        //               Categories = new string[] { "Food", "Gifts" }, Price = 70M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 4, Name = "Caviar (Vegan)", Description = "A miracle of science.", ShortDescription = "Scientific", Picture = "caviarvegan",
        //               Categories = new string[] { "Food", "Gifts" }, Price = 18M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 5, Name = "Champagne", Description = "Bollinger 1974.", ShortDescription = "Boli '74", Picture = "champagne",
        //               Categories = new string[] { "Food", "Gifts" }, Price = 55M, InitialStockCount = 8, StockCount = 8, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 6, Name = "Chateauneuf", Description = "du pape 1974.", ShortDescription = "dupape '74", Picture = "chateauneuf",
        //               Categories = new string[] { "Food", "Gifts" }, Price = 35M, InitialStockCount = 8, StockCount = 8, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 7, Name = "Cherries", Description = "Punnet of cherries.", ShortDescription = "Fresh", Picture = "cherry",
        //               Categories = new string[] { "Food" }, Price = 3M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 8, Name = "Blue Dress", Description = "or is it gold?", ShortDescription = "Cotton", Picture = "dressblue",
        //               Categories = new string[] { "Fashion" }, Price = 85M, InitialStockCount = 10, StockCount = 10, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 9, Name = "Red Dress", Description = "Hot from Milan.", ShortDescription = "Cotton", Picture = "dressred",
        //               Categories = new string[] { "Fashion" }, Price = 75M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 10, Name = "Goldfish", Description = "From our own aquarium.", ShortDescription = "memory-enhanced", Picture = "goldfish",
        //               Categories = new string[] { "Pets" }, Price = 8M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 11, Name = "Honey", Description = "Orange Blossom.", ShortDescription = "Egyptian", Picture = "honeyjar",
        //               Categories = new string[] { "Food", "Gifts" }, Price = 2.5M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 12, Name = "KiwiFruit", Description = "Fresh. A kilo of.", ShortDescription = "1 Kg", Picture = "kiwi",
        //               Categories = new string[] { "Food" }, Price = 1.5M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 13, Name = "Luxury Candle", Description = "Goji Berry.", ShortDescription = "Scented", Picture = "scentedcandle",
        //               Categories = new string[] { "Gifts" }, Price = 4.5M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 14, Name = "Oranges", Description = "Seville. A kilo of.", ShortDescription = "Seville", Picture = "orange",
        //               Categories = new string[] { "Food" }, Price = 2M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 15, Name = "Pineapples", Description = "Hawaiian Grade A.", ShortDescription = "Hawaiian", Picture = "pineapple",
        //               Categories = new string[] { "Food" }, Price = 3.5M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 16, Name = "Ruby Ring", Description = "Four in stock initially.", ShortDescription = "Last 4", Picture = "ring",
        //               Categories = new string[] { "Fashion", "Gifts" }, Price = 750M, InitialStockCount = 4, StockCount = 4, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 17, Name = "Shoes (blue)", Description = "Blue running-shoes.", ShortDescription = "Going fast", Picture = "shoesblue",
        //               Categories = new string[] { "Fashion", "Gifts" }, Price = 60M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 18, Name = "Shoes (red)", Description = "Red running-shoes.", ShortDescription = "Going fast", Picture = "shoesred",
        //               Categories = new string[] { "Fashion", "Gifts" }, Price = 50M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 19, Name = "Strawberries",Description = "Punnet of strawberries.", ShortDescription = "Kentish", Picture = "strawberry",
        //               Categories = new string[] { "Food" }, Price = 2.5M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 20, Name = "Toy Set", Description = "City Edition.", ShortDescription = "Classic", Picture = "toyset",
        //               Categories = new string[] { "Gifts" }, Price = 35M, InitialStockCount = 10, StockCount = 10, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 21, Name = "Unicorn", Description = "An item never in stock.", ShortDescription = "No stock", Picture = "unicorn",
        //               Categories = new string[] { "Gifts", "Pets" }, Price = 90000M, InitialStockCount = 0, StockCount = 0, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 22, Name = "Smartwatch", Description = "Also tells the time.", ShortDescription = "Android", Picture = "smartwatch",
        //               Categories = new string[] { "Fashion", "Gifts" }, Price = 350M, InitialStockCount = 6, StockCount = 6, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 23, Name = "Wristwatch", Description = "Sporty and elegant.", ShortDescription = "Tick-tock", Picture = "wristwatch",
        //               Categories = new string[] { "Fashion", "Gifts" }, Price = 180M, InitialStockCount = 6, StockCount = 6, MyQuantity = 0, OtherQuantity = 0 },
        //        new Product { ProductID = 24, Name = "Zipwire", Description = "An item with no categories.", ShortDescription = "Category-less", Picture = "zipwire",
        //            //   Categories = new string[] { "Circus" },
        //            Price = 15.99M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 }
        //        };
        //    return productList;
        //}
    }
}