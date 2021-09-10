using CRM_Migration.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CRM_Migration.Models
{
    public class CRMUser
    {
        public string Email { get; set; }
        public bool Verified { get; set; }
        public bool Status { get; set; }
        public IList<string> LinkedEmailAccounts { get; set; }
        public IList<LinkedAccount> LinkedAccounts { get; set; }
        public string Key { get; set; }
        public string FirstName{ get; set; }
        public string LastName { get; set; }
        public string Language { get; set; }
        public string ErrorMessage { get; set; }
        public string AzureObjectId { get; set; }
        public CRMUserDTO Select { get; internal set; }
    }
    public class LinkedAccount
    {
        public string NameIdentifier { get; set; }
        public string IdentityProviderId { get; set; }
    }
}
