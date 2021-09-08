using CRM_Migration.Models;
using CRM_Migration.Services;
using CRM_Migration.Utils;
using CRM_Migration.ViewModels;
using CrmWeb = SwissAcademic.Crm.Web;
using Microsoft.Win32;
using OfficeOpenXml;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace CRM_Migration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<CRMUser> _users;
        private ILogger _logger;
        public MainWindow()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _logger = new LoggerConfiguration().WriteTo.File(Environment.GetEnvironmentVariable("LogFileLocation"), rollingInterval: RollingInterval.Day).CreateLogger();

            InitializeComponent();
            AzureB2CService.Initialize();

            DataContext = this;
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            _users = new List<CRMUser>();
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx;*.xlsm",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                txtB_Log.Text = "";
                AzureB2CService.ErrorUsers = new List<CRMUserViewModel>();

                try
                {
                    var filePaths = openFileDialog.FileNames;
                    var result = new List<CRMUserViewModel>();
                    foreach (var filePath in filePaths)
                    {
                        result.AddRange(ExcelService.ReadExcelFile(filePath));
                    }

                    _users = GetUsers(result);

                    txt_Msg.Text = $"{result.Count()} account(s) found";
                    txt_Msg2.Text = $"{_users.Count()} contact(s) found";

                    if (_users.Count() > 0)
                    {
                        btn_Migrate.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ERROR: {ex}");
                }
            }
        }

        private async void btnMigrate_Click(object sender, RoutedEventArgs e)
        {
            var startTime = DateTime.Now;

            btn_Migrate.IsEnabled = false;
            btn_Open.IsEnabled = false;
            txt_Progress.Text = "Processing...";

            await Migrate(_users);

            if (AzureB2CService.ErrorRecords > 0)
            {
                ShowErrorLog();
            }

            btn_Open.IsEnabled = true;
            btn_Migrate.IsEnabled = true;
            txt_Progress.Text = "Completed";

            var executedTime = (DateTime.Now - startTime).TotalSeconds;
            txtB_Log.Text += $"\n\n--------- Migration done in {executedTime} seconds ---------";
            MessageBox.Show($"Migration done in {executedTime} seconds");
        }

        private void ShowErrorLog()
        {
            txt_Progress.Text += $" with {AzureB2CService.ErrorRecords} error(s)";

            var errorMsg = JsonConvert.SerializeObject(AzureB2CService.ErrorUsers, Formatting.Indented);
            txtB_Log.Text = errorMsg;
            _logger.Error(errorMsg);
        }

        private async Task Migrate(IList<CRMUser> users)
        {
            await CrmWeb.CrmWebApi.Connect(Environment.GetEnvironmentVariable("CrmClientId"),
                                    Environment.GetEnvironmentVariable("CitaviWeb_CrmOnline_EmailAddresses").Split(';'),
                                    Environment.GetEnvironmentVariable("CitaviWeb_CrmOnline"));

            var tasks = users.Select(async u =>
            {
                await AzureB2CService.MigrateUser(u);
            });

            await Task.WhenAll(tasks);

            CrmWeb.CrmWebApi.Disconnect();
        }

        private List<CRMUser> GetUsers(IList<CRMUserViewModel> usersViewModels)
        {
            var users = usersViewModels.GroupBy(u => new { u.Email, u.Key }).Select(g => new CRMUser
            {
                LinkedEmailAccounts = g.Where(u => u.LinkedEmailAccount != null).Any() ?
                                    g.Select(u => u.LinkedEmailAccount).Where(e => e != null).ToList() : null,
                LinkedAccounts = g.Where(u => u.LinkedAccount != null).Any() ?
                                g.Select(u => u.LinkedAccount).Where(a => a != null).ToList() : null,
                Email = g.Select(u => u.Email).First(),
                Verified = g.Select(u => u.IsVerified).FirstOrDefault(),
                Key = g.Select(u => u.Key).FirstOrDefault(),
                FirstName = g.Select(u => u.FirstName).FirstOrDefault(),
                LastName = g.Select(u => u.LastName).FirstOrDefault(),
                Language = StringUtil.GetLanguage(g.Select(u => u.Language).FirstOrDefault()),
            }).ToList();

            return users;
        }

        //private List<CRMUserViewModel> CleanData(IList<CRMUserViewModel> data)
        //{
        //    var result = data.GroupBy(u => u.Linked).Select(g => g.Where(u => !string.IsNullOrEmpty(u.Email)).Any() ?
        //                g.Where(u => !string.IsNullOrEmpty(u.Email)).FirstOrDefault() :
        //                g.FirstOrDefault()).ToList();

        //    foreach (var r in result)
        //    {
        //        if (StringUtil.IsValidEmail(r.Linked))
        //        {
        //            r.LinkedEmailAccount = r.Linked;
        //        }
        //        else
        //        {
        //            r.LinkedAccount = new LinkedAccount
        //            {
        //                NameIdentifier = r.Linked,
        //                IdentityProviderId = r.Provider
        //            };
        //        }
        //    }

        //    return result;
        //}
    }
}
