using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EDennis.EFBase.Tests
{

    /// <summary>
    /// The methods in this class are invoked once for each test run.
    /// The constructor is invoked before the test run and Dispose
    /// is invoked after the test run.
    /// </summary>
    public class DatabaseFixture : IDisposable {

        public static readonly string DATABASE = "a5921a9c76f7b466bbceb7227b855fb63";
        public static readonly string CONNECTION = $"Server=(localdb)\\mssqllocaldb;Initial Catalog={DATABASE};Integrated Security=SSPI";
        public static readonly DbContextOptions<DbContext> OPTIONS =
        new DbContextOptionsBuilder<DbContext>()
            .UseSqlServer(CONNECTION).Options;


        /// <summary>
        /// Invoked before the test run
        /// </summary>
        public DatabaseFixture() {
            string sql = File.ReadAllText("create.sql");
            SqlExecutor.Execute(CONNECTION, sql);
        }


        /// <summary>
        /// Invoked after the test run
        /// </summary>
        public void Dispose() {
            string sql = File.ReadAllText("drop.sql");
            SqlExecutor.Execute(CONNECTION, sql);
        }

    }
}
