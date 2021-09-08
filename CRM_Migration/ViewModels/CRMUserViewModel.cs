using CRM_Migration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRM_Migration.ViewModels
{
    public class CRMUserViewModel
    {
        public string Linked { get; set; }
        public string Provider { get; set; }
        public string Email { get; set; }
        public bool IsVerified { get; set; }
        public string LinkedEmailAccount { get; set; }
        public LinkedAccount LinkedAccount { get; set; }
        public string Key { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Language { get; set; }
        public string ErrorMessage { get; set; }
    }
}
