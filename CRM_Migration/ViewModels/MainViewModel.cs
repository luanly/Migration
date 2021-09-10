using CRMWeb = SwissAcademic.Crm.Web;
using CRM_Migration.DTOs;
using CRM_Migration.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using CRM_Migration.Models;
using Serilog;
using Newtonsoft.Json;

namespace CRM_Migration.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Private props
        private readonly ILogger _logger;
        private List<CRMUser> _groupedUsers { get; set; }

        private string _openedFiles { get; set; }
        private int _totalUsers { get; set; }
        private int _totalErrorUsers { get; set; }
        #endregion

        #region Public props
        public ObservableCollection<CRMUserDTO> Users { get; set; }
        public ObservableCollection<CRMUser> ErrorUsers { get; set; }
        public ICommand OpenCommand { get; set; }
        public ICommand MigrateCommand { get; set; }

        public int TotalUsers
        {
            get
            {
                return _totalUsers;
            }
            set
            {
                _totalUsers = value;
                OnPropertyChanged(nameof(TotalUsers));
                OnPropertyChanged(nameof(HasUsers));
            }
        }

        public int TotalErrorUsers
        {
            get
            {
                return _totalErrorUsers;
            }
            set
            {
                _totalErrorUsers = value;
                OnPropertyChanged(nameof(TotalErrorUsers));
            }
        }

        public bool HasUsers
        {
            get
            {
                return _totalUsers > 0;
            }
        }


        public List<CRMUser> GroupedUsers
        {
            get
            {
                return _groupedUsers;
            }
            set
            {
                _groupedUsers = value;
                OnPropertyChanged(nameof(GroupedUsers));
            }
        }
        public string OpenedFiles
        {
            get
            {
                return _openedFiles;

            }
            set
            {
                _openedFiles = value;
                OnPropertyChanged(nameof(OpenedFiles));
            }
        }
        #endregion

        #region Constructor
        public MainViewModel()
        {

            _logger = new LoggerConfiguration().WriteTo.File(Environment.GetEnvironmentVariable("LogFileLocation"), rollingInterval: RollingInterval.Day).CreateLogger();

            Users = new ObservableCollection<CRMUserDTO>();
            ErrorUsers = new ObservableCollection<CRMUser>();

            OpenCommand = new RelayCommand(Open);
            MigrateCommand = new RelayCommand(Migrate);
            AzureB2CService.Initialize();
        }
        #endregion

        #region Private methods
        private void ClearUsers()
        {
            Users?.Clear();
            ErrorUsers?.Clear();
        }

        private void Open()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.csv;*.xls;*.xlsx;*.xlsm",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ClearUsers();

                try
                {
                    var filePaths = openFileDialog.FileNames;
                    OpenedFiles = string.Join("; ", filePaths.Select(f => Path.GetFileName(f)));
                    foreach (var filePath in filePaths)
                    {
                        var extension = Path.GetExtension(filePath);
                        if (extension == ".csv")
                        {
                            Users.AddRange(FileService.GetAccountsFromCSV(filePath));
                        }
                        else
                        {
                            Users.AddRange(FileService.GetAccountsFromExcel(filePath));
                        }
                    }

                    TotalUsers = Users.Count;
                    GroupedUsers = GetUsers(Users);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ERROR: {ex}");
                }
            }
        }

        private async void Migrate()
        {
            var startTime = DateTime.Now;

            await MigrateUsers();

            var executedTime = (DateTime.Now - startTime).TotalSeconds;
            MessageBox.Show($"Migration done in {executedTime} seconds");
        }

        private async Task MigrateUsers()
        {
            await CRMWeb.CrmWebApi.Connect(Environment.GetEnvironmentVariable("CrmClientId"),
                                    Environment.GetEnvironmentVariable("CitaviWeb_CrmOnline_EmailAddresses").Split(';'),
                                    Environment.GetEnvironmentVariable("CitaviWeb_CrmOnline"));

            var tasks = GroupedUsers.Select(async u =>
            {
                try
                {
                    await AzureB2CService.MigrateUserToB2C(u);
                }
                catch (Exception ex)
                {
                    ErrorUsers.Add(GroupedUsers.Where(user => user.Email == u.Email).Select(user =>
                    {
                        user.ErrorMessage = ex.ToString();
                        return user;
                    }).FirstOrDefault());
                }
            });

            await Task.WhenAll(tasks);

            TotalErrorUsers = ErrorUsers.Count;

            if (TotalErrorUsers > 0)
            {
                _logger.Error(JsonConvert.SerializeObject(ErrorUsers, Formatting.Indented));
            }

            CRMWeb.CrmWebApi.Disconnect();
        }

        private static List<CRMUser> GetUsers(IList<CRMUserDTO> users)
        {
            var groupedUsers = users.GroupBy(u => new { u.Email }).Select(g =>
            {
                var key = g.Select(u => u.Key).Where(k => !string.IsNullOrEmpty(k) && !k.StartsWith("auth0")).Any() ?
                    g.Select(u => u.Key).Where(k => !string.IsNullOrEmpty(k) && !k.StartsWith("auth0")).FirstOrDefault() :
                    g.Select(u => u.Key).FirstOrDefault();

                var user = new CRMUser
                {
                    LinkedEmailAccounts = g.Select(u => u.LinkedEmailAccount).Where(e => e != null)?.ToList(),
                    LinkedAccounts = g.Select(u => u.LinkedAccount).Where(a => a != null)?.ToList(),
                    Email = g.Key.ToString(),
                    Key = key,
                    FirstName = g.Select(u => u.FirstName).Where(n => !string.IsNullOrEmpty(n)).FirstOrDefault(),
                    LastName = g.Select(u => u.LastName).Where(n => !string.IsNullOrEmpty(n)).FirstOrDefault(),
                    Language = g.Select(u => u.Language).FirstOrDefault(),
                };
                return user;
            }).ToList();

            return groupedUsers;
        }

        #endregion
    }
}
