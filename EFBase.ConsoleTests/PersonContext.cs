using EDennis.MigrationsExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EDennis.EFBase.ConsoleAppTest {


    public partial class PersonContext : DbContext {


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseSqlServer("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=PersonAddress02;Integrated Security=SSPI;")
                .ReplaceService<IMigrationsSqlGenerator, TemporalMigrationsSqlGenerator>();
            }
        }


        public virtual DbSet<Person> Person { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            modelBuilder.HasSequence<int>("seqPerson").StartsAt(1);

            modelBuilder.Entity<Person>()
                .ToTable("Person")
                .HasKey(p => p.PersonId);

            modelBuilder.Entity<Person>().Property(p => p.PersonId)
                .HasDefaultValueSql("next value for seqPerson");
            modelBuilder.Entity<Person>().Property(p => p.FirstName)
                .HasMaxLength(20);
            modelBuilder.Entity<Person>().Property(p => p.FirstName)
                .HasMaxLength(20);

            modelBuilder.Entity<Person>().Property(p => p.SysUserId)
                .HasDefaultValueSql("((0))");
            modelBuilder.Entity<Person>().Property(p => p.SysStart)
                .HasDefaultValueSql("(getdate())")
                .ValueGeneratedOnAddOrUpdate();
            modelBuilder.Entity<Person>().Property(p => p.SysEnd)
                .HasDefaultValueSql("(CONVERT(datetime2, '9999-12-31 23:59:59.9999999'))")
                .ValueGeneratedOnAddOrUpdate();

        }
    }
}
