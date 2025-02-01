using Walmart.DataAccess.Repository;
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
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {

        private readonly ApplicationDbContext _db;

        public CompanyRepository(ApplicationDbContext db) :base(db) {
        
            _db = db;
        }

       
        public void Update(Company company)
        {
            _db.Companys.Update(company);

        }
    }
}
