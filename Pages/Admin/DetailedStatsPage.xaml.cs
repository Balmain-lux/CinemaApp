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
    /// Логика взаимодействия для DetailedStatsPage.xaml
    /// </summary>
    public partial class DetailedStatsPage : Page
    {
        public DetailedStatsPage()
        {
            InitializeComponent();
            // Установка периода по умолчанию (последние 30 дней)
            dpStart.SelectedDate = DateTime.Today.AddDays(-30);
            dpEnd.SelectedDate = DateTime.Today;

            LoadStatistics(dpStart.SelectedDate.Value, dpEnd.SelectedDate.Value);
        }

        private void LoadStatistics(DateTime startDate, DateTime endDate)
        {
            try
            {
                // ИСПРАВЛЕНО: используем .Date свойство
                var start = startDate.Date;
                var end = endDate.Date;

                // Получаем бронирования за период
                var bookings = Connection.db.Bookings
                    .Where(b => b.BookingDateTime >= start && b.BookingDateTime <= end)
                    .ToList();

                // Общая статистика
                int totalBookings = bookings.Count();
                int paidBookings = bookings.Count(b => b.StatusID == 2);
                int cancelledBookings = bookings.Count(b => b.StatusID == 3);
                decimal totalRevenue = bookings.Where(b => b.StatusID == 2).Sum(b => b.TotalPrice);
                decimal averageCheck = paidBookings > 0 ? totalRevenue / paidBookings : 0;

                txtTotalBookings.Text = totalBookings.ToString();
                txtPaidBookings.Text = paidBookings.ToString();
                txtCancelledBookings.Text = cancelledBookings.ToString();
                txtTotalRevenue.Text = $"{totalRevenue:F2} руб";
                txtAverageCheck.Text = $"{averageCheck:F2} руб";

                // Статистика по фильмам
                var moviesStats = Connection.db.Movies
                    .Select(m => new
                    {
                        m.Title,
                        BookingsCount = m.Sessions
                            .SelectMany(s => s.Bookings)
                            .Count(b => b.BookingDateTime >= start && b.BookingDateTime <= end),
                        Revenue = m.Sessions
                            .SelectMany(s => s.Bookings)
                            .Where(b => b.BookingDateTime >= start &&
                                       b.BookingDateTime <= end &&
                                       b.StatusID == 2)
                            .Sum(b => (decimal?)b.TotalPrice) ?? 0
                    })
                    .Where(m => m.BookingsCount > 0)
                    .OrderByDescending(m => m.Revenue)
                    .ToList();

                MoviesStatsList.ItemsSource = moviesStats;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (dpStart.SelectedDate.HasValue && dpEnd.SelectedDate.HasValue)
            {
                LoadStatistics(dpStart.SelectedDate.Value, dpEnd.SelectedDate.Value);
            }
            else
            {
                MessageBox.Show("Выберите период!");
            }
        }
    }
}
