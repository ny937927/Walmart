using Microsoft.AspNetCore.Mvc;
using Walmart.DataAccess.Repository.IRepository;
using Walmart.Model.Models;

namespace WalmartWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ContactController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _db;

        public ContactController(ILogger<HomeController> logger, IUnitOfWork db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            Contact contact = new Contact();
            return View(contact);
        }

        public IActionResult SendMsg(Contact contact)
        {
            if(ModelState.IsValid)
            {
                _db.Contact.Add(contact);
                _db.Commit();
                TempData["success"]="Thank you " +contact.Name+" for contacting us!! We will get back to you in next 24 hrs.";

                return RedirectToAction("Index","Home");

            }
            return View(contact);
        }
    }
}
