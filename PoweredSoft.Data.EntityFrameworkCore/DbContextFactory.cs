﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using PoweredSoft.Data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PoweredSoft.Data.EntityFrameworkCore
{
    public class DbContextFactory : IDbContextFactory
    {
        private readonly DbContext _context;
        private MethodInfo _setGenericMethod = null;

        public DbContextFactory(DbContext dbContext)
        {
            _context = dbContext;
        }

        public void Add(object entity) => _context.Add(entity);

  

        public IQueryable<T> GetQueryable<T>() where T : class => _context.Set<T>();

        public IQueryable GetQueryable(Type type)
        {
            var setSource = (IDbSetSource)this._context.GetType().GetProperty("SetSource", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_context);
            var ret = (IQueryable)((IDbSetCache)this).GetOrAddSet(setSource, type);
            return ret;
        }

        public void Remove(object entity)
        {
            _context.Remove(entity);
        }

        public int SaveChanges() => _context.SaveChanges();
        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public IEnumerable<PropertyInfo> GetKeyProperties(Type entityType)
        {
            var key = _context.Model.FindEntityType(entityType).FindPrimaryKey();
            var keysProperties = key.Properties.Select(t => t.PropertyInfo);
            return keysProperties;
        }

        public IEnumerable<Expression<Func<TEntity, object>>> GetKeyProperties<TEntity>()
        {
            var keyProps = GetKeyProperties(typeof(TEntity));

            var parameter = Expression.Parameter(typeof(TEntity));
            var result = keyProps
                .Select(keyProp =>
                {
                    var property = Expression.Property(parameter, keyProp);
                    var funcType = typeof(Expression<Func<TEntity, object>>);
                    var lambda = Expression.Lambda(funcType, Expression.Convert(property, typeof(Object)), parameter);
                    return (Expression<Func<TEntity, object>>)lambda;
                })
                .ToList();
            return result;
        }

        /*
        // FOR EF6 when implemented.
        public IEnumerable<MemberExpression> GetKeyProperties(Type entityType)
        {
            foreach (PropertyInfo prop in entityType.GetProperties())
            {
                object[] attrs = prop.GetCustomAttributes(false);

                foreach (object obj in attrs)
                {
                    if (obj.GetType() == typeof(EdmScalarPropertyAttribute))
                    {
                        EdmScalarPropertyAttribute attr = (EdmScalarPropertyAttribute)obj;
                        if (attr.EntityKeyProperty)
                            keyList.Add(prop.Name);
                    }
                }

            }
        }*/

        public Task<T> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken)) => queryable.FirstOrDefaultAsync(cancellationToken);
        public Task<T> FirstOrDefaultAsync<T>(IQueryable<T> queryable, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken)) => queryable.FirstOrDefaultAsync(predicate, cancellationToken);
        public Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default(CancellationToken)) => queryable.ToListAsync(cancellationToken);
    }
}
