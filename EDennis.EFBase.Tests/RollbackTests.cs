using EDennis.NetCoreTestingUtilities.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.EFBase.Tests {

    public class RollbackTests : UnitTestBase<PersonContext> {

        private readonly ITestOutputHelper _output;
        private PersonRepo repo;


        public RollbackTests(ITestOutputHelper output) {
            _output = output;
            repo = new PersonRepo(Context, Transaction);
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void InsertAndUpdate(int testId) {

            repo.Create(new Person { LastName = "Smith", FirstName = "Jane" });
            repo.Create(new Person { LastName = "Smith", FirstName = "John" });
            repo.Create(new Person { LastName = "Jones", FirstName = "Bob" });

            List<Person> persons = repo.GetByLastName("Smith");

            persons[1].FirstName = "Stu";
            repo.Update(persons[1]);

            _output.WriteLine($"{ testId}:\n {persons.ToJsonString()}");

            var expected = new List<Person>().FromJsonPath("InsertAndUpdate.json");
            var actual = repo.GetByLastName("Smith") as List<Person>;
            Assert.True(actual.IsEqual(expected, new string[] { "SysStart", "SysEnd", "SysUserId" }));

        }



        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        public void InsertAndDelete(int testId) {

            repo.Create(new Person { LastName = "Rodriquez", FirstName = "Juan" });
            repo.Create(new Person { LastName = "Lee", FirstName = "June" });
            repo.Create(new Person { LastName = "Evans", FirstName = "Christa" });

            repo.Delete(4);
            List<Person> persons = repo.GetAll();

            _output.WriteLine($"{ testId}:\n {persons.ToJsonString()}");

            var expected = new List<Person>().FromJsonPath("InsertAndDelete.json");
            var actual = persons;

            Assert.True(actual.IsEqual(expected, new string[] { "SysStart", "SysEnd", "SysUserId" }));

        }



        [Fact]
        public void InsertAndThrowException() {

            repo.Create(new Person { LastName = "Barker", FirstName = "Bob" });
            repo.Create(new Person { LastName = "Hall", FirstName = "Monty" });

            Assert.Throws<Exception>(() => repo.ThrowException());


            repo = new PersonRepo(Context, null);

            var expected = 4;
            var actual = repo.GetAll().Count;
            Assert.Equal(expected, actual);


        }


    }
}

