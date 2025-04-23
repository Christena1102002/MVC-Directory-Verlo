using Microsoft.EntityFrameworkCore;
using mvc.RepoInterfaces;
using mvc.Models;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace mvc.Repositories
{
    public class GeniricRepository<Id,T>: IGeniricRepository<Id, T> where T : class
    {
        private ProjectContext _context;
        protected DbSet<T> dbSet;
        public GeniricRepository(ProjectContext context)
        {
            _context = context;
            dbSet = context.Set<T>();
        }
        public async Task AddAsync(T entity)
        {
             await dbSet.AddAsync(entity);
        }
        public virtual async Task<T> GetByIdAsync(Id id)
        {
            return await dbSet.FindAsync(id);
        }


        public IQueryable<T> GetAll(int size = 0 ,int pageNumber = 1)

        {
            if (pageNumber > 0 && size > 0)
            {
                return dbSet.Skip((pageNumber - 1) * size).Take(size);
            }
            return dbSet;
        }
        public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate,int size=0, int pageNumber = 0)
        {
            IQueryable<T> query = dbSet.Where(predicate);
            if(pageNumber>0 && size>0)
            {
                return query.Skip((pageNumber - 1) * size).Take(size);
            }
            return query;
        }
      
        public async Task<int> CountAsync()
        {
            return await dbSet.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate)
        {
            return await dbSet.CountAsync(predicate);
        }

        public void Update(T updated)
        {
            dbSet.Update(updated);
        }
        public async Task DeleteAsync(Id id)
        {
            T entity = await GetByIdAsync(id);
            if (entity != null)
            {
                dbSet.Remove(entity);
            }
        }
        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        


    }
}
