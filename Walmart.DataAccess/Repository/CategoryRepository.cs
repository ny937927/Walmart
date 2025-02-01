using Walmart.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Walmart.Model.Models;
using WalmartWeb.DataAccess;

namespace Walmart.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _context = db;
        }

        public void Update(Category category)
        {
           _context.Categories.Update(category);
        }
    }
}
