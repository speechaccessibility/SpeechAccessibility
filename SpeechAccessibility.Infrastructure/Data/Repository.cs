using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SpeechAccessibility.Core.Interfaces;

namespace SpeechAccessibility.Infrastructure.Data
{
    public abstract class Repository<TContext, T> : IRepository<T> where T : class where TContext : DbContext
    {

        //private readonly DbSet<T> _entities;
        protected TContext _context;
        public Repository(TContext context)
        {
            this._context = context;
            //_entities = _dbContext.Set<T>();
        }

        public IQueryable<T> GetAll()
        {
            return _context.Set<T>();
        }

       
        public IQueryable<T> Find(Expression<Func<T, bool>> predicate)
        {
           
            return _context.Set<T>().Where(predicate);
        }

       
        public void Insert(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
           
            _context.Add(entity);
            _context.SaveChanges();
        }

        public void Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            
            _context.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            _context.Remove(entity);
            _context.SaveChanges();
        }

        public void AddRange(IEnumerable<T> entities)
        {
            _context.AddRange(entities);
            _context.SaveChanges();
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
             _context.RemoveRange(entities);
            _context.SaveChanges();
        }



    }
}
