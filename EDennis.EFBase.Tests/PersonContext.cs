using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EDennis.EFBase.Tests {
    public partial class PersonContext : BaseContext {

        public PersonContext(DbContextOptions<DbContext> options) :
            base(options) { }        

        public virtual DbSet<Person> Person { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Initial Catalog=PersonRepo;Integrated Security=SSPI");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Person>(entity => {
                entity.Property(e => e.PersonId).HasDefaultValueSql("(NEXT VALUE FOR [seqPerson])");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.HasSequence<int>("seqPerson");
        }
    }
}
