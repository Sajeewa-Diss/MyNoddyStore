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
                new Product { ProductID = 1, Name = "Kayak", Description = "A boat for one person", Category = "Watersports", Price = 275M }, 
                new Product { ProductID = 2, Name = "Lifejacket", Description = "Protective and fashionable", Category = "Watersports", Price = 48.95M }, 
                new Product { ProductID = 3, Name = "Football", Description = "FIFA-approved", Category = "Football", Price = 19.5M }, 
                new Product { ProductID = 4, Name = "Corner Flags", Description = "For the field", Category = "Football", Price = 34.95M }, 
                new Product { ProductID = 5, Name = "Stadium", Description = "Flat-packed 35 K seater. One", Category = "Football", Price = 79500M }, 
                new Product { ProductID = 6, Name = "Thinking Cap", Description = "Improve brain-power", Category = "Chess", Price = 16M }, 
                new Product { ProductID = 7, Name = "Unsteady Chair", Description = "Secret advantage", Category = "Chess", Price = 29.95M }, 
                new Product { ProductID = 8, Name = "Human Chess Board", Description = "A fun family game", Category = "Chess", Price = 75M }, 
                new Product { ProductID = 9, Name = "Bling-bling King", Description = "Diamond-studded", Category = "Chess", Price = 1200M }});

            container.RegisterInstance<IProductRepository>(mock.Object);

            container.RegisterType<IOrderProcessor, EmailOrderProcessor>(new InjectionConstructor(emailSettings));

        }
    }
}