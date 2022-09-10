using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repos.Db;
using Repos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface IGenericRepository<T> where T : class
    {
        T GetById(object id);
        Task<T> GetByIdAsync(object id);
        IEnumerable<T> GetAll(bool track = true);
        Task<IEnumerable<T>> GetAllAsync(bool track = true);
        IEnumerable<T> Find(Expression<Func<T, bool>> expression = null);
        IQueryable<T> IQueryable(Expression<Func<T, bool>> expression = null);
        void Add(T entity);
        void AddOrUpdate(T entity);
        void AddRange(IEnumerable<T> entities);
        void Remove(T entity);
        void Remove(object id);
        void RemoveRange(IEnumerable<T> entities);
        int Count(Expression<Func<T, bool>> expression = null);
        Task<int> CountAsync(Expression<Func<T, bool>> expression = null);
        int Save();
        Task<int> SaveAsync();

        int ExecuteSqlRaw(string sql);
    }
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected ApplicationDbContext context;
        internal DbSet<T> dbSet;
        protected readonly ILogger _logger;

        public GenericRepository(ApplicationDbContext context, ILoggerFactory logger)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
            this._logger = logger.CreateLogger<T>();

        }
        public virtual void Add(T entity)
        {
            dbSet.Add(entity);
        }
        public virtual void AddOrUpdate(T entity)
        {
            dbSet.AddOrUpdate(entity);
        }
        public virtual void AddRange(IEnumerable<T> entities)
        {
            dbSet.AddRange(entities);
        }
        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> expression = null)
        {
            if (expression == null)
                return dbSet.Where(expression);
            return dbSet;
        }
        public virtual IQueryable<T> IQueryable(Expression<Func<T, bool>> expression = null)
        {
            if (expression == null)
                return dbSet;
            return dbSet.Where(expression);
        }
        public virtual IEnumerable<T> GetAll(bool track = true)
        {
            if (track)
                return dbSet.ToList();
            else
                return dbSet.AsNoTracking().ToList();

        }
        public virtual async Task<IEnumerable<T>> GetAllAsync(bool track = true)
        {
            if (track)
                return await dbSet.ToListAsync();
            else
                return await dbSet.AsNoTracking().ToListAsync();
        }
        public virtual T GetById(object id)
        {
            return dbSet.Find(id);
        }
        public virtual async Task<T> GetByIdAsync(object id)
        {
            return await dbSet.FindAsync(id);
        }
        public virtual void Remove(T entity)
        {
            dbSet.Remove(entity);
        }
        public virtual void Remove(object id)
        {
            var p = GetById(id);
            if (p != null)
                Remove(p);
        }
        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }
        public virtual int Count(Expression<Func<T, bool>> expression = null)
        {
            if (expression == null)
                return dbSet.Count();
            return dbSet.Where(expression).Count();
        }
        public virtual Task<int> CountAsync(Expression<Func<T, bool>> expression = null)
        {
            if (expression == null)
                return dbSet.CountAsync();
            return dbSet.Where(expression).CountAsync();

        }

        public int Save()
        {
            return context.SaveChanges();
        }

        public Task<int> SaveAsync()
        {
            return context.SaveChangesAsync();
        }
        public virtual int ExecuteSqlRaw(string sql)
        {
            return context.Database.ExecuteSqlRaw(sql);
        }
    }
}
