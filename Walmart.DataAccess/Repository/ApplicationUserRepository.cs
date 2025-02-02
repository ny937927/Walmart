using Walmart.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Walmart.Model.Models;
using WalmartWeb.DataAccess;
using Microsoft.AspNetCore.Identity;

namespace Walmart.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _context;

        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _context = db;
        }

        public void Update(ApplicationUser applicationUser)
        {
            ApplicationUser user = new()
            {
                UserName = applicationUser.UserName,
                Email = applicationUser.Email,
                Name = applicationUser.Name,
                RoleId = applicationUser.RoleId,
                CompanyId = applicationUser.CompanyId,
                NormalizedEmail = applicationUser.NormalizedEmail,
                NormalizedUserName = applicationUser.NormalizedUserName,
                PhoneNumber = applicationUser.PhoneNumber,
                City = applicationUser.City,
                StreetAddress = applicationUser.StreetAddress,
                PostalCode = applicationUser.PostalCode,
                State = applicationUser.State,
                SecurityStamp = applicationUser.SecurityStamp,
                ConcurrencyStamp = applicationUser.ConcurrencyStamp,
                PasswordHash = applicationUser.PasswordHash
                

            };

            _context.Users.Update(user);
        }
    }
}
