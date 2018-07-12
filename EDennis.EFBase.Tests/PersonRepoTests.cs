using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.EFBase.Tests {
    public class PersonRepoTests {

        private PersonContext context;
        private ITestOutputHelper output;

        public PersonRepoTests(ITestOutputHelper output) {
            context = new PersonContext();
            this.output = output;
        }


        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        public void DeleteIdException(int id) {
            using (var repo = new PersonRepo(context, true)) {
                var ex = Assert.Throws<MissingEntityException>(() => repo.Delete(id));
                Assert.Equal($"Cannot find Person object with key value = [{id}]", ex.Message);
            }
        }

        [Fact]
        public void GetByLinq() {
            using (var repo = new PersonRepo(context, true)) {

                repo.Create(new Person { LastName = "Davis", FirstName = "Jane" });
                repo.Create(new Person { LastName = "Davis", FirstName = "John" });
                repo.Create(new Person { LastName = "Jones", FirstName = "Bob" });

                var actual = repo.GetByLinq(p => p.LastName == "Davis",1,2);
                Assert.Equal(2, actual.Count);
            }
        }


    }
}
