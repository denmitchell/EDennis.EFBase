using System;
using System.Collections.Generic;

namespace EDennis.EFBase.ConsoleAppTest
{
    public partial class Person
    {
        public int PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int SysUserId { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }
    }
}
