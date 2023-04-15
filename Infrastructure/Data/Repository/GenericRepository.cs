using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq;
using System.Linq.Expressions;

namespace Infrastructure.Data.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly StoreDbContext _context;
        private readonly DbSet<T> _dbSet;
        public GenericRepository(StoreDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        #region GetById
        public async Task<T> GetByIdAsync(int id, List<Expression<Func<T, object>>> includeExpressions, Expression<Func<T, bool>> criteria)
        {
            return await ApplySpecs(id, includeExpressions, criteria: criteria).FirstOrDefaultAsync();
        }
        public async Task<T> GetByIdAsync(int id, List<Expression<Func<T, object>>> includeExpressions)
        {
            return await ApplySpecs(id, includeExpressions: includeExpressions).FirstOrDefaultAsync();
        }
        public async Task<T> GetByIdAsync(int id, Expression<Func<T, object>> includeExpression, Expression<Func<T, bool>> criteria)
        {
            return await ApplySpecs(id, includeExpression: includeExpression, criteria: criteria).FirstOrDefaultAsync();
        }
        public async Task<T> GetByIdAsync(int id, Expression<Func<T, object>> includeExpression)
        {
            return await ApplySpecs(id, includeExpression: includeExpression).FirstOrDefaultAsync();
        }
        public async Task<T> GetByIdAsync(int id)
        {
            return await ApplySpecs(id).FirstOrDefaultAsync();
        }
        #endregion

        #region GetByAll
        public async Task<IReadOnlyList<T>> ListAllAsync(List<Expression<Func<T, object>>> includeExpression, Expression<Func<T, bool>> criteria)
        {
            return await ApplySpecs(includeExpressions: includeExpression, criteria: criteria).ToListAsync();
        }
        public async Task<IReadOnlyList<T>> ListAllAsync(List<Expression<Func<T, object>>> includeExpression)
        {
            return await ApplySpecs(includeExpressions: includeExpression).ToListAsync();
        }
        public async Task<IReadOnlyList<T>> ListAllAsync(Expression<Func<T, object>> includeExpression, Expression<Func<T, bool>> criteria)
        {
            return await ApplySpecs(includeExpression: includeExpression, criteria: criteria).ToListAsync();
        }
        public async Task<IReadOnlyList<T>> ListAllAsync(Expression<Func<T, bool>> criteria)
        {
            return await ApplySpecs( criteria: criteria).ToListAsync();
        }
        public async Task<IReadOnlyList<T>> ListAllAsync(Expression<Func<T, object>> includeExpression)
        {
            return await ApplySpecs(includeExpression: includeExpression).ToListAsync();
        }
        public async Task<IReadOnlyList<T>> ListAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
        #endregion

        public async void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public async void AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }
        public void AddList(IEnumerable<T> entities)
        {
            _context.Set<T>().AddRange(entities);
        }
        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }
        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
        public void SaveChanges()
        {
            _context.SaveChanges();
        }


        private IQueryable<T> ApplySpecs(int? id = null, List<Expression<Func<T, object>>>? includeExpressions = null, Expression<Func<T, object>>? includeExpression = null, Expression<Func<T, bool>>? criteria = null)
        {
            var query = _context.Set<T>().AsQueryable();

            if (id != null)
                query = query.Where(x => x.Id == id);

            if (criteria != null)
                query = query.Where(criteria);

            if (includeExpression != null)
                query = query.Include(includeExpression);
            else if (includeExpressions != null)
                query = includeExpressions.Aggregate(query, (query, include) => query.Include(include));

            return query;
        }

    }
}
