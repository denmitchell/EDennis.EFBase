using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EDennis.EFBase {

    /// <summary>
    /// This class provides a base context that includes a SqlJson
    /// entity, which holds results of a SQL Server 2016+ FOR JSON result.
    /// 
    /// NOTE: This SqlJsonResult should transition to DbQuery, once
    /// that feature is out of preview.
    /// </summary>
    public static class DbContextExtensions {

        /// <summary>
        /// Detaches all entities from the ChangeTracker.  This is
        /// needed for integration testing scenarios in which the
        /// repo and context are injected as singletons.  In such a
        /// case, call this after calling Rollback().
        /// </summary>
        public static void Reset(this DbContext obj) {

            foreach (var dbEntityEntry in obj.ChangeTracker.Entries().ToList()) {
                if (dbEntityEntry.Entity != null) {
                    dbEntityEntry.State = EntityState.Detached;
                }
            }

        }



    }
}
