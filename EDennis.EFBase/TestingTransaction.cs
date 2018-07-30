using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace EDennis.EFBase {

    /// <summary>
    /// NOTE: This class is only designed to be used
    /// with testing with a Local DB.
    /// 
    /// This class encapsulates a database transaction that
    /// can be "restarted."  In reality the restart merely
    /// commits or rolls back the current transaction and
    /// starts a new transaction.
    /// 
    /// There are two noteworthy features of this class:
    /// (1) The rollback method ensures that all transactions
    /// (for all connections) are rolled back.
    /// (2) The rollback method also resets all sequences
    /// </summary>
    /// <typeparam name="TContext">A specific implementation of DbContext</typeparam>
    public class TestingTransaction<TContext> : IDisposable
        where TContext : DbContext
        {

        //the underlying transaction
        private IDbContextTransaction _trans;

        //a reference to the DbConext
        public TContext Context { get; set; }

        //the transaction ID of the underlying transaction
        public Guid TransactionId { get; set; }

        //the state of the transaction
        public TransactionState TransactionState { get; set; }

        //by default, will the transaction roll back or commit?
        public TransactionState DefaultResolution { get; set; }

        //a serializable isolation level is suitable for testing scenarios
        public IsolationLevel IsolationLevel => IsolationLevel.Serializable;

        //The database connection associated with the transaction and context
        protected DbConnection DbConnection { get; set; }

        /// <summary>
        /// Constructs a new transaction with the provided DbContext
        /// and a default resolution of rollback
        /// </summary>
        /// <param name="context">A valid DbContext subclass</param>
        public TestingTransaction(TContext context) {
            DefaultResolution = TransactionState.RolledBack;
            Restart(context);
        }

        /// <summary>
        /// Constructs a new transaction with the provided DbContext
        /// and provided resolution (rollback or commit)
        /// </summary>
        /// <param name="context">A valid DbContext subclass</param>
        /// <param name="defaultResolution">whether to rollback or commmit by default</param>
        public TestingTransaction(TContext context, TransactionState defaultResolution) {
            DefaultResolution = defaultResolution;
            Restart(context);
        }

        /// <summary>
        /// Restarts the transaction with the same context
        /// </summary>
        public void Restart() {
            Restart(Context);
        }

        /// <summary>
        /// Restarts the transaction with a new context
        /// </summary>
        /// <param name="context">A valid DbContext</param>
        public void Restart(TContext context) {

            //if the current transaction isn't null and is in a state
            //that can be committed or rolled back, then commit or rollback
            if (_trans != null && TransactionState == TransactionState.Begun) {
                if (DefaultResolution == TransactionState.Committed)
                    Commit();
                else
                    Rollback();
            }

            //build a new transaction using the database connection
            //from the provided context
            DbConnection = context.Database.GetDbConnection();
            Context = context;
            context.Database.AutoTransactionsEnabled = false;
            _trans = context.Database.BeginTransaction(IsolationLevel);
            TransactionState = TransactionState.Begun;
            TransactionId = _trans.TransactionId;
        }


        /// <summary>
        /// Attach the current context
        /// </summary>
        /// <param name="context"></param>
        public void AttachContext(TContext context) {
            if (context.Database.CurrentTransaction == null)
                context.Database.UseTransaction(_trans.GetDbTransaction());
            else
                Restart(context);
        }


        /// <summary>
        /// Commits the current transaction
        /// </summary>
        public void Commit() {
            _trans.Commit();
            TransactionState = TransactionState.Committed;
        }

        /// <summary>
        /// NOTE: Rolls back all transactions across all connections
        /// Also, resets all sequences
        /// </summary>
        public void Rollback() {
            _trans.Rollback();
            SqlExecutor.Execute(DbConnection.ConnectionString, $"IF EXISTS (SELECT * FROM sys.sysprocesses WHERE open_tran = 1) BEGIN ALTER DATABASE {DbConnection.Database} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; ALTER DATABASE {DbConnection.Database} SET MULTI_USER; END");
            TransactionState = TransactionState.RolledBack;
            SequenceResetter.ResetAllSequences(DbConnection.ConnectionString);
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Commits or rollsback the transaction prior to disposing the object
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing && _trans != null && TransactionState == TransactionState.Begun) {
                    if (DefaultResolution == TransactionState.Committed)
                        Commit();
                    else
                        Rollback();
                }
                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(true);
        }
        #endregion

    }
}
