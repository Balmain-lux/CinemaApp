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
    /// Логика взаимодействия для AdminStatsPage.xaml
    /// </summary>
    public partial class AdminStatsPage : Page
    {
        public AdminStatsPage()
        {
            InitializeComponent();
            LoadTopMovies();
            datePicker.SelectedDate = DateTime.Today;
            LoadDailyStats(DateTime.Today);
        }

        private void LoadTopMovies()
        {
            var topMovies = Connection.db.Movies
                .Select(m => new
                {
                    Movie = m,
                    BookingCount = m.Sessions
                        .SelectMany(s => s.Bookings)
                        .Count(b => b.StatusID == 2)
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(5)
                .ToList();

            TopMoviesList.ItemsSource = topMovies;
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (datePicker.SelectedDate.HasValue)
            {
                LoadDailyStats(datePicker.SelectedDate.Value);
            }
        }

        private void LoadDailyStats(DateTime date)
        {
            DateTime startDate = date.Date;
            DateTime endDate = startDate.AddDays(1);

            // Сначала получаем данные из БД без ToString()
            var sessions = Connection.db.Sessions
                .Where(s => s.SessionDateTime >= startDate && s.SessionDateTime < endDate)
                .Select(s => new
                {
                    SessionDateTime = s.SessionDateTime,
                    MovieTitle = s.Movies.Title,
                    HallName = s.Halls.HallName,
                    Revenue = s.Bookings
                        .Where(b => b.StatusID == 2)
                        .Sum(b => (decimal?)b.TotalPrice) ?? 0
                })
                .OrderBy(x => x.SessionDateTime)
                .ToList();

            // Преобразуем время в строку уже после получения данных (в памяти)
            var stats = sessions.Select(s => new
            {
                Time = s.SessionDateTime.ToString("HH:mm"),
                s.MovieTitle,
                s.HallName,
                s.Revenue
            }).ToList();

            DailyStatsList.ItemsSource = stats;
        }
    }
}
