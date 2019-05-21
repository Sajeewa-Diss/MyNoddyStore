using MyNoddyStore.Abstract;
using MyNoddyStore.Entities;
using System.Collections.Generic;
namespace MyNoddyStore.Concrete
{
    public class EFProductRepository : IProductRepository
    {
        public IEnumerable<Product> Products
        {
            get {
                IEnumerable<Product> newRepo = GetProductsList();
                return newRepo;
            }
        }

        //public string TestConnection() {
        //    return context.TestConnection();
        //}

        private static IEnumerable<Product> GetProductsList()
        {
            List<Product> productList = new List<Product>{
                new Product { ProductID = 1, Name = "Aadvark", Description = "Customers who ordered this also ordered: Termite mounds.", ShortDescription = "Ant-free zone", Picture = "aadvark",
                       Categories = new string[] { "Pets" }, Price = 550M, InitialStockCount = 4, StockCount = 4, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 2, Name = "Camera", Description = "A single item in stock.", ShortDescription = "Low stock", Picture = "camera",
                       Categories = new string[] { "Gifts" }, Price = 450M, InitialStockCount = 1, StockCount = 1, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 3, Name = "Caviar", Description = "Fishy luxury.", ShortDescription = "Beluga", Picture = "caviar",
                       Categories = new string[] { "Food", "Gifts" }, Price = 70M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 4, Name = "Caviar (Vegan)", Description = "A miracle of science.", ShortDescription = "Scientific", Picture = "caviarvegan",
                       Categories = new string[] { "Food", "Gifts" }, Price = 18M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 5, Name = "Champagne", Description = "Bollinger 1974.", ShortDescription = "Boli '74", Picture = "champagne",
                       Categories = new string[] { "Food", "Gifts" }, Price = 55M, InitialStockCount = 8, StockCount = 8, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 6, Name = "Chateauneuf", Description = "du pape 1974.", ShortDescription = "dupape '74", Picture = "chateauneuf",
                       Categories = new string[] { "Food", "Gifts" }, Price = 35M, InitialStockCount = 8, StockCount = 8, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 7, Name = "Cherries", Description = "Punnet of cherries.", ShortDescription = "Fresh", Picture = "cherry",
                       Categories = new string[] { "Food" }, Price = 3M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 8, Name = "Blue Dress", Description = "or is it gold?", ShortDescription = "Cotton", Picture = "dressblue",
                       Categories = new string[] { "Fashion" }, Price = 85M, InitialStockCount = 10, StockCount = 10, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 9, Name = "Red Dress", Description = "Hot from Milan.", ShortDescription = "Cotton", Picture = "dressred",
                       Categories = new string[] { "Fashion" }, Price = 75M, InitialStockCount = 10, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 10, Name = "Goldfish", Description = "From our own aquarium.", ShortDescription = "memory-enhanced", Picture = "goldfish",
                       Categories = new string[] { "Pets" }, Price = 8M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 11, Name = "Honey", Description = "Orange Blossom.", ShortDescription = "Egyptian", Picture = "honeyjar",
                       Categories = new string[] { "Food", "Gifts" }, Price = 2M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 12, Name = "KiwiFruit", Description = "Fresh. A kilo of.", ShortDescription = "1 Kg", Picture = "kiwi",
                       Categories = new string[] { "Food" }, Price = 2M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 13, Name = "Luxury Candle", Description = "Goji Berry.", ShortDescription = "Scented", Picture = "scentedcandle",
                       Categories = new string[] { "Gifts" }, Price = 4M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 14, Name = "Oranges", Description = "Seville. A kilo of.", ShortDescription = "Seville", Picture = "orange",
                       Categories = new string[] { "Food" }, Price = 2M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 15, Name = "Pineapples", Description = "Hawaiian Grade A.", ShortDescription = "Hawaiian", Picture = "pineapple",
                       Categories = new string[] { "Food" }, Price = 3M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 16, Name = "Ruby Ring", Description = "Four in stock initially.", ShortDescription = "My precious", Picture = "ring",
                       Categories = new string[] { "Fashion", "Gifts" }, Price = 550M, InitialStockCount = 4, StockCount = 4, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 17, Name = "Shoes (blue)", Description = "Blue running-shoes.", ShortDescription = "Going fast", Picture = "shoesblue",
                       Categories = new string[] { "Fashion", "Gifts" }, Price = 60M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 18, Name = "Shoes (red)", Description = "Red running-shoes.", ShortDescription = "Going fast", Picture = "shoesred",
                       Categories = new string[] { "Fashion", "Gifts" }, Price = 50M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 19, Name = "Strawberries",Description = "Punnet of strawberries.", ShortDescription = "Kentish", Picture = "strawberry",
                       Categories = new string[] { "Food" }, Price = 3M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 20, Name = "Toy Set", Description = "City Edition.", ShortDescription = "Classic", Picture = "toyset",
                       Categories = new string[] { "Gifts" }, Price = 35M, InitialStockCount = 10, StockCount = 10, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 21, Name = "Unicorn", Description = "An item never in stock.", ShortDescription = "No stock", Picture = "unicorn",
                       Categories = new string[] { "Pets" }, Price = 999.99M, InitialStockCount = 0, StockCount = 0, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 22, Name = "Smartwatch", Description = "Also tells the time.", ShortDescription = "Android", Picture = "smartwatch",
                       Categories = new string[] { "Fashion", "Gifts" }, Price = 350M, InitialStockCount = 6, StockCount = 6, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 23, Name = "Wristwatch", Description = "Sporty and elegant.", ShortDescription = "Tick-tock", Picture = "wristwatch",
                       Categories = new string[] { "Fashion", "Gifts" }, Price = 180M, InitialStockCount = 6, StockCount = 6, MyQuantity = 0, OtherQuantity = 0 },
                new Product { ProductID = 24, Name = "Zipwire", Description = "An item with no categories.", ShortDescription = "Category-less", Picture = "zipwire",
                    //   Categories = new string[] { "Circus" },
                    Price = 16M, InitialStockCount = 100, StockCount = 100, MyQuantity = 0, OtherQuantity = 0 }
                };
            return productList;
        }
    }
}
