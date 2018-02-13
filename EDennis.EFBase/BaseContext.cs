using Microsoft.EntityFrameworkCore;

namespace EDennis.EFBase {

    /// <summary>
    /// This class provides a base context that includes a SqlJson
    /// entity, which holds results of a SQL Server FOR JSON result.
    /// </summary>
    public class BaseContext : DbContext {

        /// <summary>
        /// Constructs a new instance of ContextBase, propagating
        /// DbContextOptions to the parent class (DbContext)
        /// </summary>
        /// <param name="options"></param>
        public BaseContext(DbContextOptions<DbContext> options) :
            base(options) {
        }

        //holds FOR JSON result sets
        public virtual DbSet<SqlJson> SqlJsonResult { get; set; }

    }
}
