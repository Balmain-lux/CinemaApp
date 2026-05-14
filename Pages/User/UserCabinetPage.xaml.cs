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
using CinemaApp.Pages;

namespace CinemaApp.Pages.User
{
    /// <summary>
    /// Логика взаимодействия для UserCabinetPage.xaml
    /// </summary>
    public partial class UserCabinetPage : Page
    {
        public UserCabinetPage()
        {
            InitializeComponent();
            LoadBookings();
            LoadReviews();

            ProfileFrame.Navigate(new UserProfilePage());
        }

        private void LoadBookings()
        {
            BookingsPanel.Children.Clear();

            var bookings = Connection.db.Bookings
                .Where(b => b.UserID == App.CurrentUserID)
                .OrderByDescending(b => b.BookingDateTime)
                .ToList();

            foreach (var booking in bookings)
            {
                var session = Connection.db.Sessions.Find(booking.SessionID);
                var movie = Connection.db.Movies.Find(session?.MovieID);
                var hall = Connection.db.Halls.Find(session?.HallID);
                var status = Connection.db.BookingStatuses.Find(booking.StatusID);

                Border card = new Border
                {
                    Style = (Style)FindResource("CardBorder"),
                    Margin = new Thickness(0, 5, 0, 5)
                };

                StackPanel panel = new StackPanel();

                TextBlock movieTitle = new TextBlock
                {
                    Text = movie?.Title ?? "Неизвестный фильм",
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Foreground = (Brush)FindResource("PrimaryBrush")
                };

                TextBlock sessionInfo = new TextBlock
                {
                    Text = $"{session?.SessionDateTime:dd.MM.yyyy HH:mm} | {hall?.HallName}",
                    Margin = new Thickness(0, 5, 0, 5)
                };

                TextBlock statusText = new TextBlock
                {
                    Text = $"Статус: {status?.StatusName}",
                    FontWeight = FontWeights.SemiBold
                };

                TextBlock priceText = new TextBlock
                {
                    Text = $"Сумма: {booking.TotalPrice:F2} руб.",
                    Margin = new Thickness(0, 0, 0, 10)
                };

                panel.Children.Add(movieTitle);
                panel.Children.Add(sessionInfo);
                panel.Children.Add(statusText);
                panel.Children.Add(priceText);

                // Кнопка отмены
                if (booking.StatusID == 1) // Активно
                {
                    Button btnCancel = new Button
                    {
                        Content = "Отменить бронирование",
                        Style = (Style)FindResource("SecondaryButton"),
                        Width = 200,
                        Tag = booking.BookingID
                    };

                    btnCancel.Click += (s, e) =>
                    {
                        var btn = s as Button;
                        int bookingID = (int)btn.Tag;
                        CancelBooking(bookingID);
                    };

                    panel.Children.Add(btnCancel);
                }

                card.Child = panel;
                BookingsPanel.Children.Add(card);
            }
        }

        private void CancelBooking(int bookingID)
        {
            var result = MessageBox.Show("Вы уверены, что хотите отменить бронирование?",
                                        "Подтверждение",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var booking = Connection.db.Bookings.Find(bookingID);
                    if (booking != null)
                    {
                        booking.StatusID = 3; // Отменено
                        Connection.db.SaveChanges();
                        LoadBookings();
                        MessageBox.Show("Бронирование отменено!");
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при отмене: {ex.Message}");
                }
            }
        }

        private void LoadReviews()
        {
            ReviewsPanel.Children.Clear();

            var reviews = Connection.db.Reviews
                .Where(r => r.UserID == App.CurrentUserID)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();

            foreach (var review in reviews)
            {
                var movie = Connection.db.Movies.Find(review.MovieID);

                Border card = new Border
                {
                    Style = (Style)FindResource("CardBorder"),
                    Margin = new Thickness(0, 5, 0, 5)
                };

                StackPanel panel = new StackPanel();

                TextBlock movieTitle = new TextBlock
                {
                    Text = movie?.Title ?? "Неизвестный фильм",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = (Brush)FindResource("PrimaryBrush")
                };

                // ИСПРАВЛЕННАЯ СТРОКА (используем string вместо char)
                string stars = new string('⭐', review.Rating);
                TextBlock ratingText = new TextBlock
                {
                    Text = $"Оценка: {stars}",
                    FontSize = 14,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                TextBlock commentText = new TextBlock
                {
                    Text = review.Comment,
                    TextWrapping = TextWrapping.Wrap
                };

                panel.Children.Add(movieTitle);
                panel.Children.Add(ratingText);
                panel.Children.Add(commentText);

                card.Child = panel;
                ReviewsPanel.Children.Add(card);
            }
        }



        private void btnAddReview_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.MainFrame.Navigate(new ReviewsPage());
        }
    }
}
