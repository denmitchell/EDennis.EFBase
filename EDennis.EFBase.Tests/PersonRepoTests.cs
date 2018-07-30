using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.EFBase.Tests {
    public class PersonRepoTests : UnitTestBase<PersonContext>{

        private ITestOutputHelper output;
        private PersonRepo repo;

        public PersonRepoTests(ITestOutputHelper output) {
            this.output = output;
            repo = new PersonRepo(Context, Transaction);
        }


        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        public void DeleteIdException(int id) {
            var ex = Assert.Throws<MissingEntityException>(() => repo.Delete(id));
            Assert.Equal($"Cannot find Person object with key value = [{id}]", ex.Message);
        }

        [Fact]
        public void GetByLinq() {
            repo.Create(new Person { LastName = "Davis", FirstName = "John" });
            repo.Create(new Person { LastName = "Jones", FirstName = "Bob" });
            var actual = repo.GetByLinq(p => p.LastName == "Davis", 1, 2);
            Assert.Single(actual);
        }


    }
}
