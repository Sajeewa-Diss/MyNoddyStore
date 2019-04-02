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


            EmailSettings emailSettings = new EmailSettings
            {
                WriteAsFile = bool.Parse(ConfigurationManager.AppSettings["Email.WriteAsFile"] ?? "false")
            };


            //Console.WriteLine("RegisterTypesCalled"); //todo remove me
            System.Diagnostics.Debug.WriteLine("how often am i called");

            // TODO: Register your type's mappings here.
            var mock = new Mock<IProductRepository>();

            mock.Setup(m => m.Products).Returns(new List<Product> { //a list automatically implements IEnumerable!.
                //new Product { ProductID = 1, Name = "Kayak", Description = "A boat for one person", Category = "Watersports", CategoryArray = new string[] {"Watersports", "Chess"}, Price = 275M, StockCount = 9, MyQuantity = 0, OtherQuantity = 0 }, 
                //new Product { ProductID = 2, Name = "Lifejacket", Description = "Protective and fashionable", Category = "Watersports", CategoryArray = new string[] {"Watersports"}, Price = 48.95M, StockCount = 10, MyQuantity = 0, OtherQuantity = 0 }, 
                //new Product { ProductID = 3, Name = "Football", Description = "FIFA-approved", Category = "Football", CategoryArray = new string[] {"Football", "Chess"}, Price = 19.5M, StockCount = 11, MyQuantity = 3, OtherQuantity = 0 }, 
                //new Product { ProductID = 4, Name = "Corner Flags", Description = "For the field", Category = "Football", CategoryArray = new string[] {"Football"}, Price = 34.95M, StockCount = 07, MyQuantity = 01, OtherQuantity = 0 }, 
                //new Product { ProductID = 5, Name = "Stadium", Description = "Flat-packed 35 K seater. One", Category = "Football", CategoryArray = new string[] {"Football"}, Price = 79500M, StockCount = 06, MyQuantity = 0, OtherQuantity = 0 }, 
                //new Product { ProductID = 6, Name = "Thinking Cap", Description = "Improve brain-power", Category = "Chess", CategoryArray = new string[] {"Chess"}, Price = 16M, StockCount = 05, MyQuantity = 0, OtherQuantity = 0 }, 
                //new Product { ProductID = 7, Name = "Unsteady Chair", Description = "Secret advantage", Category = "Chess", CategoryArray = new string[] {"Chess"}, Price = 29.95M, StockCount = 01, MyQuantity = 02, OtherQuantity = 0 }, 
                //new Product { ProductID = 8, Name = "Human Chess Board", Description = "A fun family game", Category = "Chess", CategoryArray = new string[] {"Chess"}, Price = 75M, StockCount = 01, MyQuantity = 0, OtherQuantity = 0 },
                //new Product { ProductID = 9, Name = "Blob", Description = "without categories", Category = "Chess", Price = 75M, StockCount = 01, MyQuantity = 0, OtherQuantity = 0 },
                //new Product { ProductID = 10, Name = "Bling-bling King", Description = "Diamond-studded", Category = "Chess", CategoryArray = new string[] {"Chess"}, Price = 1200M, StockCount = 0, MyQuantity = 0, OtherQuantity = 0 }});


            new Product
            {
                ProductID = 1,
                Name = "Aadvark",
                Description = "Customers who ordered this also ordered: Termite mounds.",
                ShortDescription = "Ant-free zone", Picture = "aadvark",
                Categories = new string[] { "Pets" },
                Price = 105M,
                StockCount = 3, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 2,
                Name = "Camera",
                Description = "A single item (soon out of stock).",
                ShortDescription = "Digital SLR", Picture = "camera",
                Categories = new string[] { "Gifts" },
                Price = 450M,
                StockCount = 1, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 3,
                Name = "Caviar",
                Description = "Luxury Edition.",
                ShortDescription = "Beluga", Picture = "caviar",
                Categories = new string[] { "Food", "Gifts" },
                Price = 70M,
                StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 4,
                Name = "Caviar (Vegan)",
                Description = "A miracle of science.",
                ShortDescription = "Scientific", Picture = "caviarvegan",
                Categories = new string[] { "Food", "Gifts" },
                Price = 18M,
                StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 5,
                Name = "Champagne",
                Description = "Bollinger 1974.",
                ShortDescription = "Boli '74", Picture = "champagne",
                Categories = new string[] { "Food", "Gifts" },
                Price = 45M,
                StockCount = 10, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 6,
                Name = "Chateauneuf",
                Description = "du pape 1974.",
                ShortDescription = "dupape '74", Picture = "chateauneuf",
                Categories = new string[] { "Food", "Gifts" },
                Price = 35M,
                StockCount = 10, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 7,
                Name = "Cherries",
                Description = "Punnet of cherries.",
                ShortDescription = "Fresh", Picture = "cherry",
                Categories = new string[] { "Food" },
                Price = 3M,
                StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 8,
                Name = "Blue Dress",
                Description = "or is it gold?",
                ShortDescription = "Cotton", Picture = "dressblue",
                Categories = new string[] { "Fashion" },
                Price = 85M,
                StockCount = 10, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 9,
                Name = "Red Dress",
                Description = "Hot from Milan.",
                ShortDescription = "Cotton", Picture = "dressred",
                Categories = new string[] { "Fashion" },
                Price = 75M,
                StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 10,
                Name = "Goldfish",
                Description = "From our own aquarium.",
                ShortDescription = "memory-enhanced", Picture = "goldfish",
                Categories = new string[] { "Pets" },
                Price = 8M,
                StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 11,
                Name = "KiwiFruit",
                Description = "Fresh. A kilo of.",
                ShortDescription = "1 Kg", Picture = "kiwi",
                Categories = new string[] { "Food" },
                Price = 1.5M,
                StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 12,
                Name = "Oranges",
                Description = "Seville. A kilo of.",
                ShortDescription = "Seville", Picture = "orange",
                Categories = new string[] { "Food" },
                Price = 2M,
                StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },


            new Product
            {
                ProductID = 13,
                Name = "Pineapples",
                Description = "Hawaiian Grade A.",
                ShortDescription = "Hawaiian", Picture = "pineapple",
                Categories = new string[] { "Food" },
                Price = 3.5M,
                StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 14,
                Name = "Ruby Ring",
                Description = "Three in stock initially.",
                ShortDescription = "Last 3", Picture = "ring",
                Categories = new string[] { "Fashion", "Gifts" },
                Price = 1500M,
                StockCount = 3, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 15,
                Name = "Shoes (blue)",
                Description = "Blue running-shoes.",
                ShortDescription = "Going fast", Picture = "shoesblue",
                Categories = new string[] { "Fashion", "Gifts" },
                Price = 60M,
                StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 16,
                Name = "Shoes (red)",
                Description = "Red running-shoes.",
                ShortDescription = "Going fast", Picture = "shoesred",
                Categories = new string[] { "Fashion", "Gifts" },
                Price = 50M,
                StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 17,
                Name = "Strawberries",
                Description = "Punnet of strawberries.",
                ShortDescription = "Kentish", Picture = "strawberry",
                Categories = new string[] { "Food" },
                Price = 2.5M,
                StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 18,
                Name = "Unicorn",
                Description = "An item never in stock.",
                ShortDescription = "No stock", Picture = "unicorn",
                Categories = new string[] { "Gifts", "Pets" },
                Price = 90000M,
                StockCount = 0, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 19,
                Name = "Wristwatch",
                Description = "Sporty and elegant.",
                ShortDescription = "Tick-tock", Picture = "wristwatch",
                Categories = new string[] { "Fashion", "Gifts" },
                Price = 120M,
                StockCount = 10, MyQuantity = 0, OtherQuantity = 0 },

            new Product
            {
                ProductID = 20,
                Name = "Zipwire",
                Description = "An item with no categories.",
                ShortDescription = "Category-less", Picture = "zipwire",
                //Categories = new string[] { "Circus" },
                Price = 15.99M,
                StockCount = 10, MyQuantity = 0, OtherQuantity = 0 }
            });

            container.RegisterInstance<IProductRepository>(mock.Object);

            container.RegisterType<IOrderProcessor, EmailOrderProcessor>(new InjectionConstructor(emailSettings));

        }
    }
}