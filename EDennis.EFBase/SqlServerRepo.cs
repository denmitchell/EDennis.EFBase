using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.EFBase {

    public interface ISqlServerRepo<TEntity>
            where TEntity : class {

        IDbContextTransaction CurrentTransaction { get; }
        void StartTransaction();
        void Rollback();
        void EnableAutoRollback();
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
        string GetJson(string sql);
        Task<string> GetJsonAsync(string sql);
    }

    /// <summary>
    /// This class can serve as a base repository for an
    /// Entity Framework Core project targeting SQL Server 2016
    /// and higher.  Each entity can have its own repository that 
    /// derives from this class.  The class allows for auto-rollback 
    /// during unit testing by using one of the overloaded constructors
    /// </summary>
    /// <typeparam name="TEntity">The name of the entity</typeparam>
    public class SqlServerRepo<TEntity> : ISqlServerRepo<TEntity>, IDisposable
                where TEntity : class, new() {

        protected SqlServerContext _context;
        protected DbSet<TEntity> _dbset;
        protected IDbContextTransaction _trans;
        protected bool _autoRollback;

        /// <summary>
        /// Returns the current transaction, if one
        /// has been specified, or null.
        /// </summary>
        public IDbContextTransaction CurrentTransaction {
            get {
                return _trans;
            }
        }

        /// <summary>
        /// Constructs a new BaseRepo object for use in testing.
        /// </summary>
        /// <param name="context">DbContext subclass that includes
        /// a DbSet for pure JSON results</param>
        /// <param name="autoRollback">specify as true to include
        /// autoRollback</param>
        public SqlServerRepo(SqlServerContext context, bool autoRollback = false) {
            _context = context;
            _dbset = _context.Set<TEntity>();

            if (autoRollback) {
                _context.Database.AutoTransactionsEnabled = false;
                _autoRollback = autoRollback;

                _trans = _context.Database.BeginTransaction(
                    IsolationLevel.Serializable);
            }
        }


        /// <summary>
        /// Constructs a new BaseRepo object for use in testing.
        /// </summary>
        /// <param name="context">DbContext subclass that includes
        /// a DbSet for pure JSON results</param>
        /// <param name="transaction">use an existing transaction</param>
        public SqlServerRepo(SqlServerContext context, IDbContextTransaction transaction) {
            _context = context;
            _dbset = _context.Set<TEntity>();

            _trans = transaction;
        }


        /// <summary>
        /// Turns on the autorollback feature.  
        /// This is useful in situations where the repo is
        /// dependency injected into a class (e.g., a 
        /// controller) and autorollback depends upon
        /// logic in that class (e.g., value of 
        /// IHostingEnvironment)
        /// 
        /// Note that any transactions prior to turning on 
        /// auto rollback will not be rolled back.
        /// </summary>
        public void EnableAutoRollback() {
            _context.Database.AutoTransactionsEnabled = false;
            _autoRollback = true;

            if (_context.Database.CurrentTransaction == null)
                _trans = _context.Database.BeginTransaction(
                    IsolationLevel.Serializable);
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
        /// Determines if an object with the given primary key values
        /// exists in the context.
        /// </summary>
        /// <param name="keyValues">primary key values</param>
        /// <returns></returns>
        public async Task<Boolean> ExistsAsync(params object[] keyValues) {
            var x = await _dbset.FindAsync(keyValues);
            var exists = (x != null);
            _context.Entry(x).State = EntityState.Detached;
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
            _context.Entry(x).State = EntityState.Detached;
            return exists;
        }


        /// <summary>
        /// Starts a transaction, which can be subsequently rolled back.
        /// </summary>
        public void StartTransaction() {
            _context.Database.AutoTransactionsEnabled = false;
            if (_context.Database.CurrentTransaction == null)
                _trans = _context.Database.BeginTransaction(
                    IsolationLevel.Serializable);
        }


        /// <summary>
        /// Rolls back the current transaction, and resets all
        /// sequences.
        /// </summary>
        public void Rollback() {
            if (_context.Database.CurrentTransaction != null) {
                _trans.Rollback();
                SequenceResetter.ResetAllSequences(_context);
                _trans.Dispose();
            }
        }


        /// <summary>
        /// Detaches all entities from the ChangeTracker.  This is
        /// needed for integration testing scenarios in which the
        /// repo and context are injected as singletons.  In such a
        /// case, call this after calling Rollback().
        /// </summary>
        public void ResetContext() {

            if (_context?.ChangeTracker?.Entries() == null)
                return;

            foreach (var dbEntityEntry in _context.ChangeTracker.Entries()) {
                if (dbEntityEntry.Entity != null) {
                    dbEntityEntry.State = EntityState.Detached;
                }
            }

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
            _context.SaveChanges();
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
            await _context.SaveChangesAsync();
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

            _context.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();

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

            _context.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();

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

            if (_context.Entry(entity).State == EntityState.Detached)
                _dbset.Attach(entity);

            _dbset.Remove(entity);
            _context.SaveChanges();
        }

        /// <summary>
        /// Asychronously deletes the provided entity
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        public virtual async Task DeleteAsync(TEntity entity) {
            if (entity == null)
                throw new MissingEntityException(
                    $"Cannot delete a null {entity.GetType().Name}");

            if (_context.Entry(entity).State == EntityState.Detached)
                _dbset.Attach(entity);

            _dbset.Remove(entity);
            await _context.SaveChangesAsync();
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

        /// <summary>
        /// Executes a FOR JSON SELECT statement and returns the result as
        /// a string.
        /// </summary>
        /// <param name="sql">The FOR JSON SQL to execute</param>
        /// <returns>A JSON string representing the resultset</returns>
        public virtual string GetJson(string sql) {

            //simple guard against SQL Injection
            sql = sql.Replace(";", "").TrimStart();

            //simple guard against SQL Injection
            if (sql.Substring(0, 6).ToUpper() != "SELECT")
                throw new FormatException("GetJson SQL must begin with SELECT");

            //wraps FOR JSON SELECT statement such that results are conveyed as a column named "json"
            sql = "declare @j varchar(max) = (" + sql + "); select @j json;";

            //use LINQ to get the results
            var result = _context.SqlJsonResult
                    .AsNoTracking()
                    .FromSql(sql)
                    .FirstOrDefault()
                    .Json;

            //return the results
            return result;
        }


        /// <summary>
        /// Executes a FOR JSON SELECT statement and returns the result as
        /// a string.
        /// </summary>
        /// <param name="sql">The FOR JSON SQL to execute</param>
        /// <returns>A JSON string representing the resultset</returns>
        public virtual async Task<string> GetJsonAsync(string sql) {

            //simple guard against SQL Injection
            sql = sql.Replace(";", "").TrimStart();

            //simple guard against SQL Injection
            if (sql.Substring(0, 6).ToUpper() != "SELECT")
                throw new FormatException("GetJson SQL must begin with SELECT");

            //wraps FOR JSON SELECT statement such that results are conveyed as a column named "json"
            sql = "declare @j varchar(max) = (" + sql + "); select @j json;";

            //use LINQ to get the results
            var result = await _context.SqlJsonResult
                    .AsNoTracking()
                    .FromSql(sql)
                    .FirstOrDefaultAsync();

            //return the results
            return result.Json;
        }

        /// <summary>
        /// Disposes of the Repository
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the Repository, calling Rollback and
        /// resetting sequences, if indicated
        /// </summary>
        /// <param name="disposing">whether object is disposing</param>
        protected virtual void Dispose(bool disposing) {
            if (disposing && _autoRollback &&
                (_context.Database.GetDbConnection() != null)
                && _context.Database.CurrentTransaction != null) {
                _trans.Rollback();
                SequenceResetter.ResetAllSequences(_context);
                _trans.Dispose();
            }
        }

        private string PrintKeys(params object[] keyValues) {
            return "[" + String.Join(",", keyValues) + "]";
        }


    }
}
