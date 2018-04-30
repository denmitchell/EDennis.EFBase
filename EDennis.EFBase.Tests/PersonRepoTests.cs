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

    }
}
