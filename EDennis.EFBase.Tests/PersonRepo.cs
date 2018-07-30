
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using System.Data;

namespace EDennis.EFBase.Tests {

    public class PersonRepo : SqlRepo<Person,PersonContext> {


        public PersonRepo(PersonContext context, TestingTransaction<PersonContext> trans) :
            base(context, trans) { }

        public void ThrowException() {
            throw new Exception("To see if rollback still occurs");
        }


        public List<Person> GetByLastName(string lastName) {
            var query = _dbset
                .Where(p => p.LastName == lastName);
            return query.ToList();
        }

        public List<Person> GetAll() {
            var query = _dbset;
            return query.ToList();
        }

    }


}
