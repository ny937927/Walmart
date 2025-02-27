﻿

using Walmart.DataAccess.Repository.IRepository;
using Walmart.DataAccess.Repository;
using Walmart.DataAccess.Repository.IRepository;
using WalmartWeb.DataAccess;
using Microsoft.AspNetCore.Identity;

namespace Walmart.DataAccess.Repository
{
    public class UnitOfWork :IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public ICategoryRepository Category { get; private set; }

        public IProductRepository Product { get; private set; }

        public ICompanyRepository Company { get; private set; }

        public IShoppingCartRepository ShoppingCart { get; private set; }

        public IApplicationUserRepository ApplicationUser { get; private set; }

        public IContactRepository Contact { get; private set; }

        public IOrderHeaderRepository OrderHeader { get; private set; }

        public IOrderDetailRepository OrderDetail { get; private set; }

      

        public UnitOfWork(ApplicationDbContext db) 
        {
            _db = db;

            Category = new CategoryRepository(_db);

            Product = new ProductRepository(_db);

            Company = new CompanyRepository(_db);

            ShoppingCart = new ShoppingCartRepository(_db);

            ApplicationUser = new ApplicationUserRepository(_db);

            OrderHeader = new OrderHeaderRepository(_db);

            OrderDetail = new OrderDetailRepository(_db);

            Contact = new ContactRepository(_db);
        }

        public void Commit()
        {
            _db.SaveChanges();
        }
    }
}
