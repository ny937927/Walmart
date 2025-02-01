using Walmart.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Walmart.Model.Models;
using Walmart.Utility;

namespace WalmartWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _db;

        public CompanyController(IUnitOfWork db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Company> companies= _db.Company.GetAll().ToList();

            return View(companies);
        }
        public IActionResult Upsert(int? Id)
        {
            if (Id == 0 || Id == null)
            {
                Company company = new Company();
                return View(company);
            }
            else
            {
                Company company = _db.Company.Get(u => u.Id == Id);
                return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id != 0)
                {
                    _db.Company.Update(company);
                    _db.Commit();
                    TempData["success"] = "Company Updated Successful!!";
                    return RedirectToAction("Index");
                }
                else
                {
                    _db.Company.Add(company);
                    _db.Commit();
                    TempData["success"] = "Company Added Successful!!";
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        public IActionResult Edit(int? Id)
        {
            if (Id != 0 || Id != null)
            {
                Company company = _db.Company.Get(u => u.Id == Id);
                if (company != null)
                {
                    return View(company);
                }
            }
            return View();
        }

        [HttpPost]
        public IActionResult Edit(Company company)
        {
            if (ModelState.IsValid)
            {
                _db.Company.Update(company);
                _db.Commit();
            }
            return View();
        }

        #region APICall
        [HttpGet]
        public IActionResult GetAll()
        {
            IEnumerable<Company> companies = _db.Company.GetAll();
            return Json(new { data = companies });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            Company companyToBeDeleted = _db.Company.Get(u => u.Id == id);
            if (companyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleteing" });
            }
            _db.Company.Remove(companyToBeDeleted);
            _db.Commit();
            return Json(new { success = true, message = "Delete Successful!!" });

        }

        #endregion

    }
}
