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

namespace CinemaApp.Pages.Guest
{
    /// <summary>
    /// Логика взаимодействия для GuestSessionPage.xaml
    /// </summary>
    public partial class GuestSessionPage : Page
    {
        private int? movieID;
        public GuestSessionPage(int? movieID = null)
        {
            InitializeComponent();
            this.movieID = movieID;
            datePicker.SelectedDate = DateTime.Today;

            if (movieID.HasValue)
            {
                var movie = Connection.db.Movies.Find(movieID.Value);
                if (movie != null)
                {
                    txtTitle.Text = $"Сеансы фильма: {movie.Title}";
                }
            }

            LoadSessions();
        }

        private void LoadSessions()
        {
            if (!datePicker.SelectedDate.HasValue) return;

            DateTime selectedDate = datePicker.SelectedDate.Value;

            // ИСПРАВЛЕНО: используем начальную и конечную дату для фильтрации
            DateTime startDate = selectedDate.Date;
            DateTime endDate = startDate.AddDays(1);

            var query = Connection.db.Sessions
                .Where(s => s.IsActive == true &&
                           s.SessionDateTime >= startDate &&
                           s.SessionDateTime < endDate)
                .AsQueryable();

            if (movieID.HasValue)
            {
                query = query.Where(s => s.MovieID == movieID.Value);
            }

            var sessions = query.OrderBy(s => s.SessionDateTime).ToList();

            var sessionsData = sessions.Select(s => new
            {
                s.SessionID,
                MovieTitle = s.Movies.Title,
                Time = s.SessionDateTime.ToString("HH:mm"),
                HallName = s.Halls.HallName,
                Price = s.BasePrice,
                FreeSeats = GetFreeSeatsCount(s.SessionID)
            }).ToList();

            SessionsList.ItemsSource = sessionsData;
        }

        private int GetFreeSeatsCount(int sessionID)
        {
            var session = Connection.db.Sessions.Find(sessionID);
            if (session == null) return 0;

            int totalSeats = Connection.db.Seats.Count(s => s.HallID == session.HallID);
            int bookedSeats = Connection.db.BookingSeats
                .Join(Connection.db.Bookings,
                    bs => bs.BookingID,
                    b => b.BookingID,
                    (bs, b) => new { bs, b })
                .Count(x => x.b.SessionID == sessionID && (x.b.StatusID == 1 || x.b.StatusID == 2));

            return totalSeats - bookedSeats;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.MainFrame.Navigate(new AuthPage("login"));
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.MainFrame.Navigate(new AuthPage("register"));
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSessions();
        }
    }
}
