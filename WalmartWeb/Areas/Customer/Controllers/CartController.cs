using Walmart.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using System.Security.Claims;
using Walmart.Model.Models;

namespace WalmartWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _db;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork db)
        {
            _db = db; 
        }

        public IActionResult Index()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity?)User.Identity;
            string userId = (claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier))?.Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _db.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetCartPrice(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {

            ClaimsIdentity claimsIdentity = (ClaimsIdentity?)User.Identity;
            string userId = (claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier))?.Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _db.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _db.ApplicationUser.Get(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetCartPrice(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }


        public IActionResult Plus(int? cartId)
        {
            ShoppingCart cartFromDb = _db.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _db.ShoppingCart.Update(cartFromDb);
            _db.Commit();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int? cartId)
        {
            ShoppingCart cartFromDb = _db.ShoppingCart.Get(u => u.Id == cartId);
            if(cartFromDb.Count <= 1)
            {
                //remove from cart
                _db.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                //Decrease the cart count from 1 value
                cartFromDb.Count -= 1;
                _db.ShoppingCart.Update(cartFromDb);
            }
           
            
            _db.Commit();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int? cartId)
        {
            ShoppingCart cartFromDb = _db.ShoppingCart.Get(u => u.Id == cartId);
            _db.ShoppingCart.Remove(cartFromDb);
            _db.Commit();
            return RedirectToAction(nameof(Index));
        }


        private double GetCartPrice(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if(shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}
