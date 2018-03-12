using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.EFBase {

    public interface IBaseRepo<TEntity>
            where TEntity : class {

        TEntity GetById(object[] keyValues);
        Task<TEntity> GetByIdAsync(object[] keyValues);
        TEntity Create(TEntity entity);
        Task<TEntity> CreateAsync(TEntity entity);
        TEntity Update(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        void Delete(TEntity entity);
        void DeleteAsync(TEntity entity);
        void Delete(object[] keyValues);
        void DeleteAsync(object[] keyValues);
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
    public class SqlServerRepo<TEntity> : IBaseRepo<TEntity>, IDisposable
                where TEntity : class, new() {

        protected SqlServerContext _context;
        protected DbSet<TEntity> _dbset;
        protected IDbContextTransaction _trans;
        protected bool _autoRollback;

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
        /// Creates a new entity from the provided input
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <returns>The created entity</returns>
        public virtual TEntity Create(TEntity entity) {
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
            if (_context.Entry(entity).State == EntityState.Detached) {
                _dbset.Attach(entity);
            }
            _dbset.Remove(entity);
            _context.SaveChanges();
        }

        /// <summary>
        /// Asychronously deletes the provided entity
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        public virtual async void DeleteAsync(TEntity entity) {
            if (_context.Entry(entity).State == EntityState.Detached) {
                _dbset.Attach(entity);
            }
            _dbset.Remove(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes the entity whose primary keys match the provided input
        /// </summary>
        /// <param name="keyValues">The primary key as key-value object array</param>
        public virtual void Delete(params object[] keyValues) {
            TEntity entity = _dbset.Find(keyValues);
            Delete(entity);
        }

        /// <summary>
        /// Asynchrously deletes the entity whose primary keys match the provided input
        /// </summary>
        /// <param name="keyValues">The primary key as key-value object array</param>
        public virtual void DeleteAsync(params object[] keyValues) {
            TEntity entity = _dbset.Find(keyValues);
            DeleteAsync(entity);
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


    }
}
