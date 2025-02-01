using Walmart.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Walmart.DataAccess.Repository.IRepository;
using Walmart.Model.Models;
using WalmartWeb.DataAccess;

namespace Walmart.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;   
        }

        public void Update(Product product)
        {
            _db.Products.Update(product);
        }
    }
}
