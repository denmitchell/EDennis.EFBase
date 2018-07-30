using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EDennis.EFBase {

    public interface ISqlRepo<TEntity,TContext>
            where TEntity : class, new()
            where TContext : DbContext, new() {

        bool Exists(params object[] keyValues);
        Task<Boolean> ExistsAsync(params object[] keyValues);
        TEntity GetById(params object[] keyValues);
        Task<TEntity> GetByIdAsync(params object[] keyValues);
        TEntity Create(TEntity entity);
        Task<TEntity> CreateAsync(TEntity entity);
        TEntity Update(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        void Delete(TEntity entity);
        Task DeleteAsync(TEntity entity);
        void Delete(params object[] keyValues);
        Task DeleteAsync(params object[] keyValues);
    }

    /// <summary>
    /// This class can serve as a base repository for an
    /// Entity Framework Core project targeting SQL Server 2016
    /// and higher.  Each entity can have its own repository that 
    /// derives from this class.  The class allows for auto-rollback 
    /// during unit testing by using one of the overloaded constructors
    /// </summary>
    /// <typeparam name="TEntity">The name of the entity</typeparam>
    public class SqlRepo<TEntity,TContext> : ISqlRepo<TEntity,TContext>
            where TEntity : class, new()
            where TContext : DbContext, new(){

        protected TContext Context { get; }
        protected DbSet<TEntity> _dbset;
        protected TestingTransaction<TContext> Transaction { get; }


        /// <summary>
        /// Constructs a new SqlRepo object using the provided DbContext
        /// and the built-in (default) transaction handling
        /// </summary>
        /// <param name="context">Entity Framework DbContext</param>
        public SqlRepo(TContext context) {
            Context = context;

            //attach context to transaction, when appropriate
            _dbset = Context.Set<TEntity>();
        }



        /// <summary>
        /// Constructs a new SqlRepo object using the provided DbContext and DbTransaction
        /// </summary>
        /// <param name="context">Entity Framework DbContext</param>
        /// <param name="trans">Subclass of DbTransaction that rolls back on Dispose (may be null)</param>
        public SqlRepo(TContext context, TestingTransaction<TContext> trans) {
            Transaction = trans;
            Context = context;

            //attach context to transaction, when appropriate
            if (trans != null && trans.Context != context
                && trans.TransactionState == TransactionState.Begun)
                    trans.AttachContext(context);

            _dbset = Context.Set<TEntity>();
        }


        /// <summary>
        /// Restarts a transaction, if a transaction is not null
        /// </summary>
        public void RestartTransaction() {
            if(Transaction != null)
                Transaction.Restart();
        }


        /// <summary>
        /// Retrieves the entity with the provided primary key values
        /// </summary>
        /// <param name="keyValues">primary key provided as key-value object array</param>
        /// <returns>Entity whose primary key matches the provided input</returns>
        public virtual TEntity GetById(params object[] keyValues) {
            return _dbset.Find(keyValues);
        }


        /// <summary>
        /// Asychronously retrieves the entity with the provided primary key values.
        /// </summary>
        /// <param name="keyValues">primary key provided as key-value object array</param>
        /// <returns>Entity whose primary key matches the provided input</returns>
        public virtual async Task<TEntity> GetByIdAsync(params object[] keyValues) {
            return await _dbset.FindAsync(keyValues);
        }


        /// <summary>
        /// Retrieves a page of all records defined by the provided LINQ expression
        /// </summary>
        /// <param name="linqExpression">Valid LINQ expression</param>
        /// <param name="pageNumber">The target result page</param>
        /// <param name="pageSize">The number of record per page</param>
        /// <returns>A list of all TEntity objects</returns>
        public virtual List<TEntity> GetByLinq(Expression<Func<TEntity,bool>> linqExpression, int pageNumber, int pageSize) {
            return _dbset.Where(linqExpression)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }


        /// <summary>
        /// Asynchronously retrieves a page of all records defined by the provided LINQ expression.
        /// </summary>
        /// <param name="linqExpression">Valid LINQ expression</param>
        /// <param name="pageNumber">The target result page</param>
        /// <param name="pageSize">The number of record per page</param>
        /// <returns>A list of all TEntity objects</returns>
        public virtual async Task<List<TEntity>> GetByLinqAsync(Expression<Func<TEntity, bool>> linqExpression, int pageNumber, int pageSize) {
            return await _dbset.Where(linqExpression)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }



        /// <summary>
        /// Determines if an object with the given primary key values
        /// exists in the context.
        /// </summary>
        /// <param name="keyValues">primary key values</param>
        /// <returns></returns>
        public async Task<Boolean> ExistsAsync(params object[] keyValues) {
            var x = await _dbset.FindAsync(keyValues);
            var exists = (x != null);
            Context.Entry(x).State = EntityState.Detached;
            return exists;
            //return await context.Items.AnyAsync(i => i.ItemId == id);
        }


        /// <summary>
        /// Determines if an object with the given primary key values
        /// exists in the context.
        /// </summary>
        /// <param name="keyValues">primary key values</param>
        /// <returns>true if an entity with the provided keys exists</returns>
        public bool Exists(params object[] keyValues) {
            var x = _dbset.Find(keyValues);
            var exists = (x != null);
            Context.Entry(x).State = EntityState.Detached;
            return exists;
        }






        /// <summary>
        /// Creates a new entity from the provided input
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <returns>The created entity</returns>
        public virtual TEntity Create(TEntity entity) {
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot create a null {entity.GetType().Name}");

            _dbset.Add(entity);
            Context.SaveChanges();
            return entity;
        }


        /// <summary>
        /// Asynchronously creates a new entity from the provided input
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <returns>The created entity</returns>
        public virtual async Task<TEntity> CreateAsync(TEntity entity) {
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot create a null {entity.GetType().Name}");

            _dbset.Add(entity);
            await Context.SaveChangesAsync();
            return entity;
        }


        /// <summary>
        /// Updates the provided entity
        /// </summary>
        /// <param name="entity">The new data for the entity</param>
        /// <returns>The newly updated entity</returns>
        public virtual TEntity Update(TEntity entity) {
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot update a null {entity.GetType().Name}");

            Context.Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
            Context.SaveChanges();

            return entity;
        }


        /// <summary>
        /// Asynchronously updates the provided entity
        /// </summary>
        /// <param name="entity">The new data for the entity</param>
        /// <returns>The newly updated entity</returns>
        public virtual async Task<TEntity> UpdateAsync(TEntity entity) {

            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot update a null {entity.GetType().Name}");

            Context.Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            return entity;
        }

        /// <summary>
        /// Deletes the provided entity
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        public virtual void Delete(TEntity entity) {
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot delete a null {entity.GetType().Name}");

            if (Context.Entry(entity).State == EntityState.Detached)
                _dbset.Attach(entity);

            _dbset.Remove(entity);
            Context.SaveChanges();
        }

        /// <summary>
        /// Asychronously deletes the provided entity
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        public virtual async Task DeleteAsync(TEntity entity) {
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot delete a null {entity.GetType().Name}");

            if (Context.Entry(entity).State == EntityState.Detached)
                _dbset.Attach(entity);

            _dbset.Remove(entity);
            await Context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes the entity whose primary keys match the provided input
        /// </summary>
        /// <param name="keyValues">The primary key as key-value object array</param>
        public virtual void Delete(params object[] keyValues) {
            TEntity entity = _dbset.Find(keyValues);
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot find {new TEntity().GetType().Name} object with key value = {PrintKeys(keyValues)}");

            Delete(entity);
        }

        /// <summary>
        /// Asynchrously deletes the entity whose primary keys match the provided input
        /// </summary>
        /// <param name="keyValues">The primary key as key-value object array</param>
        public virtual async Task DeleteAsync(params object[] keyValues) {
            TEntity entity = _dbset.Find(keyValues);
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot find {new TEntity().GetType().Name} object with key value = {PrintKeys(keyValues)}");

            await DeleteAsync(entity);
        }

        private string PrintKeys(params object[] keyValues) {
            return "[" + String.Join(",", keyValues) + "]";
        }



    }
}
