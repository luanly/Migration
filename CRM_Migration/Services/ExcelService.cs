using CRM_Migration.Models;
using CRM_Migration.Utils;
using CRM_Migration.ViewModels;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;

namespace CRM_Migration.Services
{
    public static class ExcelService
    {
        public static IEnumerable<CRMUserViewModel> ReadExcelFile(string filePath)
        {
            var users = new List<CRMUserViewModel>();
            try
            {
                FileInfo file = new FileInfo(filePath);
                var package = new ExcelPackage(file);
                var sheet = package.Workbook.Worksheets[0];
                int columnCount = sheet.Dimension.End.Column;
                int rowCount = sheet.Dimension.End.Row;

                for (var r = 2; r <= rowCount; r++)
                {
                    var linked = sheet.Cells[r, 4].Value?.ToString();
                    var contactEmail = sheet.Cells[r, 6].Value?.ToString();
                    var firstName = sheet.Cells[r, 8].Value?.ToString();
                    var lastName = sheet.Cells[r, 9].Value?.ToString();
                    var provider = StringUtil.GetProvider(sheet.Cells[r, 11].Value?.ToString());
                    var language = StringUtil.GetLanguage(sheet.Cells[r, 10].Value?.ToString());

                    if (string.IsNullOrEmpty(linked))
                    {
                        continue;
                    }

                    else
                    {
                        users.Add(new CRMUserViewModel
                        {
                            //Linked = linked,

                            LinkedEmailAccount = StringUtil.IsValidEmail(linked) ? linked : null,
                            LinkedAccount = !StringUtil.IsValidEmail(linked) ? new LinkedAccount { 
                                IdentityProviderId = provider,
                                NameIdentifier = linked
                            } : null,
                            Provider = provider,
                            Email = !string.IsNullOrEmpty(contactEmail) ? contactEmail : linked,
                            IsVerified = sheet.Cells[r, 5].Value?.ToString() == "Yes" || sheet.Cells[r, 5].Value?.ToString() == "Active",
                            Key = sheet.Cells[r, 7].Value?.ToString(),

                            //Maximum length for GivenName and SurName for Azure user is 64
                            FirstName = firstName?.Length > 64 ? firstName.Substring(0, 63) : firstName,
                            LastName = lastName?.Length > 64 ? lastName.Substring(0, 63) : lastName,

                            Language = language,
                        });
                    }
                }

                return users;
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }
        public static void WriteExcelFile(string filePath, IList<CRMUserViewModel> records)
        {
            var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Unsuccessful contacts");

            sheet.Row(1).Style.Font.Bold = true;
            sheet.Row(1).Style.Fill.BackgroundColor.SetColor(Color.Azure);

            //Set headers name
            sheet.Cells[1, 1].Value = "Email(NameIdentifier)";
            sheet.Cells[1, 2].Value = "Email (Contact) (Contact)";
            sheet.Cells[1, 3].Value = "Key (Contact) (Contact)";
            sheet.Cells[1, 4].Value = "First Name (Contact) (Contact)";
            sheet.Cells[1, 5].Value = "Last Name (Contact) (Contact)";
            sheet.Cells[1, 6].Value = "Language (Contact) (Contact)";
            sheet.Cells[1, 7].Value = "IdentityProviderId";

            var recordIndex = 2;

            foreach (var r in records)
            {
                sheet.Cells[recordIndex, 1].Value = r.Linked;
                sheet.Cells[recordIndex, 2].Value = r.Email;
                sheet.Cells[recordIndex, 3].Value = r.Key;
                sheet.Cells[recordIndex, 4].Value = r.FirstName;
                sheet.Cells[recordIndex, 5].Value = r.LastName;
                sheet.Cells[recordIndex, 6].Value = r.Language;
                sheet.Cells[recordIndex, 7].Value = r.Provider;
                recordIndex++;
            }
        }
    }
}
