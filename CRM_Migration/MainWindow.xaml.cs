using OfficeOpenXml;
using System.Windows;
using CRM_Migration.ViewModels;

namespace CRM_Migration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
