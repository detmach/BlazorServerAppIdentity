using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Models;
using Repos.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface IUnitOfWork
    {
        UserManagerRepo UserManager { get; }

        ApplicationDbContext Db { get; }
        Task SaveAsync();
        int Save();
    }
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ILogger Logger;
        private readonly ILoggerFactory logger;
        public ApplicationDbContext Db { get; set; }
        public readonly RoleManager<ApplicationRole> RoleManager;
        public readonly IDistributedCache Cache;
        public UserManagerRepo UserManager { get; set; }

        //private readonly IRazorViewToStringRenderer Renderer;

        #region oluşturucu
        public UnitOfWork(ILoggerFactory logger, ApplicationDbContext db, RoleManager<ApplicationRole> roleManager, IDistributedCache cache, UserManagerRepo userManager)
        {
            Logger = logger.CreateLogger("UnitOfWork");
            Db = db;
            RoleManager = roleManager;
            Cache = cache;
            UserManager = userManager;
        }
        #endregion
        
        #region IDisposable Members
        // Burada IUnitOfWork arayüzüne implemente ettiğimiz IDisposable arayüzünün Dispose Patternini implemente ediyoruz.
        private bool disposed = false;

        public Task SaveAsync()
        {
            return Db.SaveChangesAsync();
        }
        public int Save()
        {
            return Db.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Db.Dispose();
                }
            }

            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
