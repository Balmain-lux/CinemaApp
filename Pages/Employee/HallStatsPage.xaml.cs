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

namespace CinemaApp.Pages.Employee
{
    /// <summary>
    /// Логика взаимодействия для HallStatsPage.xaml
    /// </summary>
    public partial class HallStatsPage : Page
    {
        public HallStatsPage()
        {
            InitializeComponent();
            LoadHalls();
            dpDate.SelectedDate = DateTime.Today;
            LoadStats();
        }

        private void LoadHalls()
        {
            var halls = Connection.db.Halls.ToList();
            cmbHall.ItemsSource = halls;
            if (halls.Any())
                cmbHall.SelectedIndex = 0;
        }

        private void cmbHall_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStats();
        }

        private void dpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStats();
        }

        private void LoadStats()
        {
            if (cmbHall.SelectedValue == null || !dpDate.SelectedDate.HasValue)
                return;

            int hallID = (int)cmbHall.SelectedValue;
            DateTime date = dpDate.SelectedDate.Value;

            LoadSessions(hallID, date);
            LoadPopularMovies();
        }

        private void LoadSessions(int hallID, DateTime date)
        {
            DateTime startDate = date.Date;
            DateTime endDate = startDate.AddDays(1);

            var sessions = Connection.db.Sessions
                .Where(s => s.HallID == hallID &&
                           s.SessionDateTime >= startDate &&
                           s.SessionDateTime < endDate &&
                           s.IsActive == true)
                .OrderBy(s => s.SessionDateTime)
                .ToList();

            // Добавляем подсчет бронирований для каждого сеанса
            var sessionsWithCount = sessions.Select(s => new
            {
                s.SessionID,
                s.SessionDateTime,
                s.Movies,
                BookingsCount = s.Bookings.Count(b => b.StatusID == 1 || b.StatusID == 2)
            }).ToList();

            SessionsList.ItemsSource = sessionsWithCount;
        }

        private void LoadPopularMovies()
        {
            var popularMovies = Connection.db.Movies
                .Select(m => new
                {
                    m.Title,
                    BookingsCount = m.Sessions
                        .SelectMany(s => s.Bookings)
                        .Count(b => b.StatusID == 2)
                })
                .Where(m => m.BookingsCount > 0)
                .OrderByDescending(m => m.BookingsCount)
                .Take(5)
                .ToList();

            PopularMoviesList.ItemsSource = popularMovies;
        }
    }
   }
