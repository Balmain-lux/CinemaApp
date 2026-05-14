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

namespace CinemaApp.Pages.Common
{
    /// <summary>
    /// Логика взаимодействия для SessionSelectionPage.xaml
    /// </summary>
    public partial class SessionSelectionPage : Page
    {
        private int? movieID;

        public SessionSelectionPage()
        {
            InitializeComponent();

            this.movieID = null;
            datePicker.SelectedDate = DateTime.Today;
            LoadMovieInfo();
            LoadSessions(DateTime.Today);
        }

        private void LoadMovieInfo()
        {
            if (movieID.HasValue && movieID.Value > 0)
            {
                var movie = Connection.db.Movies.Find(movieID.Value);
                if (movie != null && MovieInfoPanel != null)
                {
                    MovieInfoPanel.Visibility = Visibility.Visible;
                    txtMovieTitle.Text = movie.Title;
                    txtMovieDesc.Text = movie.Description;
                }
            }
            else
            {
                if (MovieInfoPanel != null)
                    MovieInfoPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadSessions(DateTime date)
        {
            SessionsPanel.Children.Clear();

            var query = Connection.db.Sessions
                .Where(s => s.IsActive == true)
                .AsQueryable();

            // Фильтр по фильму
            if (movieID.HasValue)
            {
                query = query.Where(s => s.MovieID == movieID.Value);
            }

            var sessions = query.ToList()
                .Where(s => s.SessionDateTime.Date == date.Date)
                .OrderBy(s => s.SessionDateTime)
                .ToList();

            if (sessions.Count == 0)
            {
                TextBlock noSessions = new TextBlock
                {
                    Text = "На выбранную дату сеансов нет",
                    FontSize = 16,
                    Foreground = (Brush)FindResource("GrayBrush"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                SessionsPanel.Children.Add(noSessions);
                return;
            }

            foreach (var session in sessions)
            {
                SessionsPanel.Children.Add(CreateSessionCard(session));
            }
        }

        private Border CreateSessionCard(Sessions session)
        {
            var movie = Connection.db.Movies.Find(session.MovieID);
            var hall = Connection.db.Halls.Find(session.HallID);

            // Подсчет свободных мест
            var totalSeats = Connection.db.Seats.Count(s => s.HallID == session.HallID);
            var bookedSeats = Connection.db.BookingSeats
                .Join(Connection.db.Bookings,
                    bs => bs.BookingID,
                    b => b.BookingID,
                    (bs, b) => new { bs, b })
                .Count(x => x.b.SessionID == session.SessionID
                    && (x.b.StatusID == 1 || x.b.StatusID == 2));

            var freeSeats = totalSeats - bookedSeats;

            Border card = new Border
            {
                Style = (Style)FindResource("CardBorder"),
                Margin = new Thickness(0, 5, 0, 5)
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });

            StackPanel infoPanel = new StackPanel();

            TextBlock movieTitle = new TextBlock
            {
                Text = movie?.Title ?? "Неизвестный фильм",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("PrimaryBrush")
            };

            TextBlock timeInfo = new TextBlock
            {
                Text = $"{session.SessionDateTime:HH:mm} | Зал: {hall?.HallName} | {freeSeats} свободных мест",
                FontSize = 14,
                Foreground = (Brush)FindResource("GrayBrush"),
                Margin = new Thickness(0, 5, 0, 0)
            };

            TextBlock priceInfo = new TextBlock
            {
                Text = $"Цена: от {session.BasePrice} руб.",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 5, 0, 0)
            };

            infoPanel.Children.Add(movieTitle);
            infoPanel.Children.Add(timeInfo);
            infoPanel.Children.Add(priceInfo);

            Button btnBook = new Button
            {
                Content = "Забронировать",
                Style = (Style)FindResource("PrimaryButton"),
                Width = 160,
                Height = 40,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Tag = session.SessionID
            };

            btnBook.Click += (s, e) =>
            {
                var btn = s as Button;
                int sessionID = (int)btn.Tag;

                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.MainFrame.Navigate(new BookingPage(sessionID));
            };

            Grid.SetColumn(infoPanel, 0);
            Grid.SetColumn(btnBook, 1);

            grid.Children.Add(infoPanel);
            grid.Children.Add(btnBook);

            card.Child = grid;
            return card;
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (datePicker.SelectedDate.HasValue)
            {
                LoadSessions(datePicker.SelectedDate.Value);
            }
        }
    }
}
