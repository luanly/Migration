using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class CrmUserAccountValidation
    {
        public static bool EmailIsValid(string email)
        {
            return RegexUtility.IsValidEmailAddress(email?.Trim());
        }

        public static async Task<ValidationResult> EmailMustNotAlreadyExistsOthenThanUser(Contact contactWithThisEmail, string email, CrmDbContext context)
        {
            var result = await context.Fetch(FetchXmlExpression.Create<Contact>(new Query.FetchXml.EmailExists(email).TransformText()));
            if (result == null || !result.Any())
            {
                return null;
            }

            foreach (var contactKey in result.Select(entity => entity["new_key"].ToString()))
            {
                if (contactKey != contactWithThisEmail.Key)
                {
                    return new ValidationResult($"{email} already exists");
                }
            }

            return null;
        }
    }
}
