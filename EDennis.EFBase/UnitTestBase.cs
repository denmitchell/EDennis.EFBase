using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.EFBase {
    public abstract class UnitTestBase<TContext> : IDisposable 
        where TContext : DbContext, new() {

        protected TContext Context;
        protected TestingTransaction<TContext> Transaction;

        public UnitTestBase() {
            Context = new TContext();
            Transaction = new TestingTransaction<TContext>(Context);
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    Transaction.Dispose();
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
