﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PoweredSoft.Data.Core
{
    public interface IDbContextFactory
    {
        IQueryable<T> GetQueryable<T>()
            where T : class;

        IQueryable GetQueryable(Type type);
        void Add(object entity);
        void Remove(object entity);
        int SaveChanges();
        Task<int> SaveChangesAsync();
        IEnumerable<PropertyInfo> GetKeyProperties(Type entityType);
        IEnumerable<Expression<Func<TEntity, object>>> GetKeyProperties<TEntity>();
    }
}
