using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.PortableExecutable;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite;
using SharpVectors.Converters;
using static System.Net.Mime.MediaTypeNames;

namespace WPF_Handover
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            MainFrame.NavigationService.Navigate(new IssuesMain());
        }

        
    }
}