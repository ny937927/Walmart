using Walmart.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Walmart.Model.Models;
using Walmart.Utility;

namespace WalmartWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {

        private readonly IUnitOfWork _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Common common;

        public ProductController(IUnitOfWork db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            common = new Common();
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _db.Product.GetAll(includeProperties: "Category").ToList();


            return View(products);
        }


        public IActionResult Upsert(int? Id)
        {
            //ViewBag.CategoryList = CategoryList;
            //ViewData["CategoryList"] = CategoryList; // Instead of using ViewBag and ViewData, we prefer to use object 

            ProductVM productVM = new()
            {
                CategoryList = _db.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };

            if (Id == 0 || Id == null)
            {
                //For Create 
                return View(productVM);
            }
            else
            {
                //For Update
                productVM.Product = _db.Product.Get(x => x.Id == Id, includeProperties: "Category");
                return View(productVM);
            }

        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    //---This section is for Update -- Deleting old file and copying new file----------------
                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\')); // removing slash(\) from starting on the string from productVM.Product.ImageUrl
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                    }
                    //--------------------------------------------------------------------------------------

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        // new Common().GetResizeImage(fileStream,591,709);
                        file.CopyTo(fileStream);

                    }



                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if (productVM.Product.Id == 0)
                {
                    _db.Product.Add(productVM.Product);
                    TempData["success"] = "Product Created successfully!!";
                }
                else
                {
                    _db.Product.Update(productVM.Product);
                    TempData["success"] = "Product Updated successfully!!";
                }
                _db.Commit();

                return RedirectToAction("Index");
            }
            else
            {
                //We will not get error(Null reference Error) if some error will be there and data can't be saved.
                productVM.CategoryList = _db.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }

        }

        #region APIs call
        [HttpGet]
        public IActionResult GetAll()
        {
            IEnumerable<Product> products = _db.Product.GetAll(includeProperties: "Category");
            return Json(new { data = products});
        }

        [HttpDelete]
        public IActionResult Delete(int? Id)
        {
            if(Id != 0)
            {
                Product productToBeDeleted = _db.Product.Get(u=> u.Id == Id,includeProperties:"Category");
                if (productToBeDeleted == null)
                {
                    return Json(new { success = false, message = "Error while deleteing!!" });
                }
                _db.Product.Remove(productToBeDeleted);
                _db.Commit();
                return Json(new { success = false, message = "Product Deleted Successful!!" });
            }
            return View();
        }


        #endregion
    }
}
