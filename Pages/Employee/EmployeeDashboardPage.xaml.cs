using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CinemaApp.Pages.Common;

namespace CinemaApp.Pages.Employee
{
    /// <summary>
    /// Логика взаимодействия для EmployeeDashboardPage.xaml
    /// </summary>
    public partial class EmployeeDashboardPage : Page
    {
        public EmployeeDashboardPage()
        {
            InitializeComponent();
            // По умолчанию показываем смены
            EmployeeContentFrame.Navigate(new EmployeeShiftsPage());
        }

        private void btnMyShifts_Click(object sender, RoutedEventArgs e)
        {
            EmployeeContentFrame.Navigate(new EmployeeShiftsPage());
        }

        private void btnSellTickets_Click(object sender, RoutedEventArgs e)
        {
            EmployeeContentFrame.Navigate(new SessionSelectionPage());
        }

        private void btnHallStats_Click(object sender, RoutedEventArgs e)
        {
            EmployeeContentFrame.Navigate(new HallStatsPage());
        }
    }
}
