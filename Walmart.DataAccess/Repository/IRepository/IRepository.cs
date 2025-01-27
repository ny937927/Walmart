using System.Linq.Expressions;

namespace Walmart.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(string? includeProperties = null);

        T Get(Expression<Func<T, bool>> filters, string? includeProperties = null); // Expression<Func<T,bool>> this we used because we are going to use linq query here ex. (u => u.Id)

        void Add(T entity);

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);

    }
}
