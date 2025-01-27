using FoodHolic.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Walmart.Model.Models;

namespace WalmartWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _db;

        public CategoryController(IUnitOfWork db)
        {
            _db = db;
        }


        public IActionResult Index()
        {
            IEnumerable<Category> categories = _db.Category.GetAll();
            return View(categories.OrderBy(x => x.DisplayOrder));
        }

        public IActionResult Upsert(int? Id)
        {
            if (Id == 0 || Id == null)
            {
                Category category = new Category();
                return View(category);
            }
            else
            {
                Category category = _db.Category.Get(u => u.Id == Id);
                return View(category);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id != 0)
                {
                    _db.Category.Update(category);
                    _db.Commit();
                    TempData["success"] = "Category Updated Successful!!";
                    return RedirectToAction("Index");
                }
                else
                {
                    _db.Category.Add(category);
                    _db.Commit();
                    TempData["success"] = "Category Added Successful!!";
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        public IActionResult Edit(int? Id)
        {
            if (Id != 0 || Id != null)
            {
                Category category = _db.Category.Get(u => u.Id == Id);
                if (category != null)
                {
                    return View(category);
                }
            }
            return View();
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Category.Update(category);
                _db.Commit();
            }
            return View();
        }

        #region APICall
        [HttpGet]
        public IActionResult GetAll()
        {
            IEnumerable<Category> categories = _db.Category.GetAll();
            return Json(new { data = categories.OrderBy(u => u.DisplayOrder) });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            Category categoryToBeDeleted = _db.Category.Get(u => u.Id == id);
            if (categoryToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleteing" });
            }
            _db.Category.Remove(categoryToBeDeleted);
            _db.Commit();
            return Json(new { success = true, message = "Delete Successful!!" });

        }

        #endregion

    }
}
