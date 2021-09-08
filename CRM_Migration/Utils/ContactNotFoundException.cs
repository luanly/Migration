using System;

namespace CRM_Migration.Utils
{
    public class ContactNotFoundException : Exception
    {
        public ContactNotFoundException()
        {
        }

        public ContactNotFoundException(string message)
            : base(message)
        {
        }

        public ContactNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
