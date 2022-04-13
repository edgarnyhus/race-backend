using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions
{
    public class UsersException : Exception
    {
        public UsersException()
        {

        }
        public UsersException(string message)
            : base(message)
        {

        }

        public UsersException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
