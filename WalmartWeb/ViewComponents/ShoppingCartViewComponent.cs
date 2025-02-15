using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Walmart.DataAccess.Repository.IRepository;
using Walmart.Utility;

namespace WalmartWeb.ViewComponents
{
    // Its like master page having .cs file to code C# code and have some logic.
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity?)User.Identity;
            var claim = (claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier));
            if (claim != null)
            {
                if(HttpContext.Session.GetInt32(SD.SessionCart) == null)
                {
                    HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Count());
                }
                return View(HttpContext.Session.GetInt32(SD.SessionCart));

            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);

            }

        }

    }
}
