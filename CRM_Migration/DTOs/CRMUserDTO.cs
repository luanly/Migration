using CRM_Migration.Models;

namespace CRM_Migration.DTOs
{
    public class CRMUserDTO
    {
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
