using System.Diagnostics;
using System.Security.Claims;
using Walmart.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Walmart.Model.Models;

namespace WalmartWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _db;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _db.Product.GetAll(includeProperties: "Category");
            //string wwwRootPath = _webHostEnvironment.WebRootPath;
            //string productPath = Path.Combine(wwwRootPath, @"images\product");
            //foreach (Product product in products)
            //{
            //    if (common.resizeImage(Path.Combine(productPath, product.ImageUrl), "ImageUrl", ref productPath))
            //    {
            //        //log Image file resize successful!
            //    }
            //}
            return View(products);
        }

        public IActionResult Details(int Id)
        {
            ShoppingCart cart = new()
            {
                Product = _db.Product.Get(u => u.Id == Id, includeProperties: "Category"),
                Count = 1,
                ProductId = Id
            };


            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var userId = (claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier))?.Value;
            shoppingCart.ApplicationUserId = userId;
            shoppingCart.Id = 0;
            if(ModelState.IsValid)
            {
                ShoppingCart cartFromDb = _db.ShoppingCart.Get(u => u.ApplicationUserId == shoppingCart.ApplicationUserId && u.ProductId == shoppingCart.ProductId);

                if(cartFromDb != null)
                {
                    //Already exist the cart data with the same product, we need to update
                    cartFromDb.Count += shoppingCart.Count;
                    _db.ShoppingCart.Update(cartFromDb);

                }
                else
                {
                    //Add to cart

                    _db.ShoppingCart.Add(shoppingCart);
                }
                _db.Commit();

                TempData["success"] = "Cart Updated successfully!!";
                return RedirectToAction(nameof(Index)); //Instead of ""Index we can write nameof(Index)
            }
            return View(shoppingCart);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    }
