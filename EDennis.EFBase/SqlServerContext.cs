using Microsoft.EntityFrameworkCore;

namespace EDennis.EFBase {

    /// <summary>
    /// This class provides a base context that includes a SqlJson
    /// entity, which holds results of a SQL Server 2016+ FOR JSON result.
    /// 
    /// NOTE: This SqlJsonResult should transition to DbQuery, once
    /// that feature is out of preview.
    /// </summary>
    public class SqlServerContext : DbContext {

        /// <summary>
        /// Constructor used by MVC, when options are passed in via Startup
        /// </summary>
        /// <param name="options">configuration options</param>
        public SqlServerContext(DbContextOptions<DbContext> options) :
            base(options) { }

        /// <summary>
        /// Default constructor.  Use this when configuring via 
        /// OnConfiguring
        /// </summary>
        public SqlServerContext() { }


        //holds FOR JSON result sets
        public virtual DbSet<SqlJson> SqlJsonResult { get; set; }

    }
}
