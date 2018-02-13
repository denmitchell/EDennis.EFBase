using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq;
using System.Data;

namespace EDennis.EFBase.Tests {

    public class PersonRepo : BaseRepo<Person> {


        public PersonRepo(PersonContext context) :
            base(context, true){ }


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
