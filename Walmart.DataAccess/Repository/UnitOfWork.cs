

using FoodHolic.DataAccess.Repository.IRepository;
using Walmart.DataAccess.Repository;
using Walmart.DataAccess.Repository.IRepository;
using WalmartWeb.DataAccess;

namespace FoodHolic.DataAccess.Repository
{
    public class UnitOfWork :IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public ICategoryRepository Category { get; private set; }

        public IProductRepository Product { get; private set; }

        public UnitOfWork(ApplicationDbContext db) 
        {
            _db = db;

            Category = new CategoryRepository(_db);

            Product = new ProductRepository(_db);
        }

        public void Commit()
        {
            _db.SaveChanges();
        }
    }
}
