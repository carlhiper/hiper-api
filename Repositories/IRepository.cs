using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using System.Threading.Tasks;

namespace Hiper.Api.Repositories
{
    interface IRepository<T>:IDisposable
    {
        IQueryable<T> GetAll();
        T GetSingle(int barId);
        IQueryable<T> FindBy(Expression<Func<T, bool>> predicate);
        Task<List<T>> FindByAsync(Expression<Func<T, bool>> predicate);

        T Add(T entity);
        void Delete(T entity);
        T Edit(T entity);
        void Save();
    }
}
