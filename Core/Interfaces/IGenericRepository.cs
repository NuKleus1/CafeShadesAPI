using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IGenericRepository<T>
    {
        //Task<T> GetByIdAsync(int id);
        //Task<IReadOnlyList<T>> ListAllAsync();
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        void SaveChanges();
        Task<T> GetByIdAsync(int id, List<Expression<Func<T, object>>> includeExpressions, Expression<Func<T, bool>> criteria);
        Task<T> GetByIdAsync(int id, List<Expression<Func<T, object>>> includeExpressions);
        Task<T> GetByIdAsync(int id, Expression<Func<T, object>> includeExpression, Expression<Func<T, bool>> criteria);
        Task<T> GetByIdAsync(int id, Expression<Func<T, object>> includeExpression);
        Task<T> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> ListAllAsync(List<Expression<Func<T, object>>> includeExpressions, Expression<Func<T, bool>> criteria);
        Task<IReadOnlyList<T>> ListAllAsync(List<Expression<Func<T, object>>> includeExpressions);
        Task<IReadOnlyList<T>> ListAllAsync(Expression<Func<T, object>> includeExpression, Expression<Func<T, bool>> criteria);
        Task<IReadOnlyList<T>> ListAllAsync(Expression<Func<T, object>> includeExpression);
        Task<IReadOnlyList<T>> ListAllAsync(Expression<Func<T, bool>> criteria);
        Task<IReadOnlyList<T>> ListAllAsync();
    }
}
