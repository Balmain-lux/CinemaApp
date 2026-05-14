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
using CinemaApp.DataBase;

namespace CinemaApp.Pages.Admin
{
    /// <summary>
    /// Логика взаимодействия для AdminDashboardPage.xaml
    /// </summary>
    public partial class AdminDashboardPage : Page
    {
        public AdminDashboardPage()
        {
            InitializeComponent();
            LoadStatistics();

            // По умолчанию показываем статистику
            AdminContentFrame.Navigate(new AdminStatsPage());
        }

        private void LoadStatistics()
        {
            try
            {
                // Количество фильмов
                txtMovieCount.Text = Connection.db.Movies.Count(m => m.IsActive == true).ToString();

                // Количество пользователей
                txtUserCount.Text = Connection.db.Users.Count().ToString();

                // Количество бронирований
                txtBookingCount.Text = Connection.db.Bookings.Count().ToString();

                // Общая выручка (оплаченные бронирования)
                var revenue = Connection.db.Bookings
                    .Where(b => b.StatusID == 2) // Оплачено
                    .Sum(b => (decimal?)b.TotalPrice) ?? 0;
                txtRevenue.Text = revenue.ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}");
            }
        }

        private void btnStats_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminStatsPage());
        }

        private void btnManageMovies_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new ManageMoviesPage());
        }

        private void btnManageSessions_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new ManageSessionsPage());
        }

        private void btnManageEmployees_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new ManageEmployeesPage());
        }

        private void btnManageHalls_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new ManageHallsPage());
        }

        private void btnAllReviews_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AllReviewsPage());
        }

        private void btnDetailsStats_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new DetailedStatsPage());
        }
    }
}
