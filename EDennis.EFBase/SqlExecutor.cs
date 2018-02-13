using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace EDennis.EFBase {

    /// <summary>
    /// Provides a means of executing SQL Server SQL statements separated by 
    /// GO blocks
    /// </summary>
    public class SqlExecutor {

        /// <summary>
        /// Executes SQL using the provided connection string
        /// </summary>
        /// <param name="connectionString">Valid connection string</param>
        /// <param name="sql">One or more SQL statements</param>
        public static void Execute(string connectionString, string sql) {

            var pattern = @"^\W*go\W*;?\W*$";
            var statements = Regex.Split(sql, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var dbConnection = connectionString;
            var dbName = Regex.Replace(dbConnection,@".*(?:Database|Initial Catalog)\W*=\W*([A-Za-z0-9_]*)\W*;.*","$1");
            var masterConnection = dbConnection.Replace(dbName, "master");

            ExecuteStatement(statements, masterConnection, dbConnection);

        }

        /// <summary>
        /// Executes SQL using the provided DbContext
        /// </summary>
        /// <param name="context">Valid DbContext</param>
        /// <param name="sql">One or more SQL statements</param>
        public static void Execute(DbContext context, string sql) {

            var pattern = @"^\W*go\W*;?\W*$";
            var statements = Regex.Split(sql, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var dbConnection = context.Database.GetDbConnection().ConnectionString;
            var dbName = context.Database.GetDbConnection().Database;
            var masterConnection = dbConnection.Replace(dbName, "master");

            ExecuteStatement(statements, masterConnection, dbConnection);
        }


        /// <summary>
        /// Executes one or more SQL Statements
        /// </summary>
        /// <param name="statements">An array of statements</param>
        /// <param name="masterConnection">Connection string for master database</param>
        /// <param name="dbConnection">Connection string for target database</param>
        private static void ExecuteStatement(string[] statements, 
                string masterConnection, string dbConnection) {
            foreach (var s in statements) {
                if (Regex.IsMatch(s, @"[drop|create]\W*database")) {
                    using (var mCxn = new SqlConnection(masterConnection)) {
                        mCxn.Open();
                        using (var cmd = new SqlCommand(s, mCxn)) {
                            cmd.ExecuteNonQuery();
                        }
                    }
                } else {
                    using (var dbCxn = new SqlConnection(dbConnection)) {
                        dbCxn.Open();
                        using (var cmd = new SqlCommand(s, dbCxn)) {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }


        }

    }
}
