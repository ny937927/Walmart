using System.Diagnostics;
using FoodHolic.DataAccess.Repository.IRepository;
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

        public IActionResult Details(int? id)
        {
            Product products = _db.Product.Get(u => u.Id == id, includeProperties: "Category");
            return View(products);
        }

        public IActionResult Privacy()
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
