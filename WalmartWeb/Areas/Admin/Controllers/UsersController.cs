using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Walmart.DataAccess.Repository;
using Walmart.DataAccess.Repository.IRepository;
using Walmart.Model.Models;
using Walmart.Utility;
using static WalmartWeb.Areas.Identity.Pages.Account.RegisterModel;

namespace WalmartWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UsersController : Controller
    {
        private readonly IUnitOfWork _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;  //for adding role to the user
        public string? Role { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> RoleList { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> CompanyList { get; set; }


        public UsersController(IUnitOfWork db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager; // for adding role to the user
           
        }


        public IActionResult Index()
        {

            IEnumerable <ApplicationUser> users = _db.ApplicationUser.GetAll(includeProperties:"Role");
            return View(users);
        }

        public IActionResult Upsert(string? Id)
        {

            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole { Name = SD.Role_Customer }).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole { Name = SD.Role_Company }).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole { Name = SD.Role_Admin }).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole { Name = SD.Role_Employee }).GetAwaiter().GetResult();
            }


            RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i
            });

            CompanyList = _db.Company.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });



            //RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            //{
            //    Text = i,
            //    Value = i
            //});

            //CompanyList = _db.Company.GetAll().Select(i => new SelectListItem
            //{
            //    Text = i.Name,
            //    Value = i.Id.ToString()
            //});
            ViewBag.Role = RoleList;
            ViewBag.Company = CompanyList;
            ApplicationUser user = new();
            if (String.IsNullOrEmpty(Id))
            {
               
                return View(user);
            }
            else
            {
                user = _db.ApplicationUser.Get(u => u.Id == Id,includeProperties:"Role");
                
                return View(user);
            }
        }

        [HttpPost]
        public  IActionResult Upsert(ApplicationUser user)
        {
           var userFromDb = _db.ApplicationUser.Get(x => x.Id == user.Id, includeProperties: "Role");
           
            if(userFromDb != null)
            {
                //Update
                string type = "update";
                AddAndUpdateUserData(user, type);
                _db.ApplicationUser.Update(user);
                _db.Commit();
                TempData["success"] = "User Updated Successful!!";
                return RedirectToAction("Index");


            }
            else
            {
                //Add
                string type = "add";
                AddAndUpdateUserData(user,  type);
                _db.ApplicationUser.Add(user);
                _db.Commit();
                TempData["success"] = "User Added Successful!!";
                return RedirectToAction("Index");

            }


            //if (!String.IsNullOrEmpty(user.Id))
            //{

            //    UpdateUser(user);
            //  TempData["success"] = "User Updated Successful!!";
            //    return RedirectToAction("Index");
            //}
            //else
            //{
            //var test1= ViewBag.Role;
            //var test2 = ViewBag.Company;

            //    _db.ApplicationUser.Add(user);
            //    _db.Commit();
            //    TempData["success"] = "Company Added Successful!!";
            //    return RedirectToAction("Index");
            //}

            return View(user);
        }

        public async void AddAndUpdateUserData(ApplicationUser user, string type)
        {
            if (user.RoleId != null)
            {
                var role = _roleManager.FindByNameAsync(user.RoleId).GetAwaiter().GetResult();
                user.RoleId = role.Id;

            }
            else
            {
                var role = _roleManager.FindByNameAsync(SD.Role_Customer).GetAwaiter().GetResult();
                user.RoleId = role.Id;
            }
            if (type =="add")
            {  
                user.UserName = user.Email;
                user.NormalizedEmail = user.Email.ToUpper();
                user.NormalizedUserName = user.Email.ToUpper();
                user.PasswordHash = "AQAAAAIAAYagAAAAEKbsIIsdXd57D5apiMdEe2OWHIIQeRqoLwwnqQZPHHaAfmJxs3fMs6PrHc21oOu0OQ==";
            }
            else
            {

                // Save the changes asynchronously
             
                user.UserName = user.Email;
                user.NormalizedEmail = user.Email.ToUpper();
                user.NormalizedUserName = user.Email.ToUpper();
                //var result = _userManager.UpdateAsync(user).GetAwaiter().GetResult();

            }

        }



        //public IActionResult Delete(string? Id)
        //{



        //    if (!String.IsNullOrEmpty(Id))
        //    {
        //        var user = _db.ApplicationUser.Get(x => x.Id == Id);
        //        _db.ApplicationUser.Remove(user);
        //        _db.Commit();
        //        TempData["success"] = "User Deleted Successful!!";
        //        return RedirectToAction("Index");
        //    }


        //    return View();
        //}

        [HttpDelete]
        public IActionResult Delete(string? id)
        {
            ApplicationUser userToBeDeleted = _db.ApplicationUser.Get(u => u.Id == id);
            if (userToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleteing" });
            }
            _db.ApplicationUser.Remove(userToBeDeleted);
            _db.Commit();
            TempData["success"] = "User Deleted Successfull!!";
            return Json(new { success = true, message = "Delete Successful!!" });

        }


        
        }
    }
