using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.EFBase {
    public class MissingEntityException : Exception {
        public MissingEntityException(string message) 
            : base(message) {
        }
    }
}
