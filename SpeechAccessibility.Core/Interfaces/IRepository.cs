using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SpeechAccessibility.Core.Interfaces
{
    public interface IRepository<T> 
    {
        IQueryable<T> GetAll();
        IQueryable<T> Find(Expression<Func<T, bool>> predicate);
        void Insert(T entity);
        void Update(T entity);
        void Delete(T entity);
        void RemoveRange(IEnumerable<T> entities);
        void AddRange(IEnumerable<T> entities);

    }
}
