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
    /// Логика взаимодействия для BookingPage.xaml
    /// </summary>
    public partial class BookingPage : Page
    {
        private int sessionID;
        private Sessions currentSession;
        private List<int> selectedSeatIDs = new List<int>();
        private Dictionary<int, Button> seatButtons = new Dictionary<int, Button>();
        private decimal totalPrice = 0;
        public BookingPage(int sessionID, List<int> selectedSeats = null)
        {
            InitializeComponent();
            this.sessionID = sessionID;
            if (selectedSeats != null)
            {
                this.selectedSeatIDs = selectedSeats;
            }
            LoadSessionInfo();
            LoadSeats();
        }



        private void LoadSessionInfo()
        {
            currentSession = Connection.db.Sessions.Find(sessionID);
            if (currentSession == null) return;

            var movie = Connection.db.Movies.Find(currentSession.MovieID);
            var hall = Connection.db.Halls.Find(currentSession.HallID);

            txtSessionInfo.Text = $"{movie?.Title} | {currentSession.SessionDateTime:dd.MM.yyyy HH:mm} | {hall?.HallName}";
        }

        private void LoadSeats()
        {
            SeatsGrid.Children.Clear();
            SeatsGrid.RowDefinitions.Clear();
            seatButtons.Clear();

            if (currentSession == null) return;

            var seats = Connection.db.Seats
                .Where(s => s.HallID == currentSession.HallID)
                .OrderBy(s => s.RowNumber)
                .ThenBy(s => s.SeatNumber)
                .ToList();

            // Получаем занятые места
            var bookedSeatIDs = Connection.db.BookingSeats
                .Join(Connection.db.Bookings,
                    bs => bs.BookingID,
                    b => b.BookingID,
                    (bs, b) => new { bs, b })
                .Where(x => x.b.SessionID == sessionID
                    && (x.b.StatusID == 1 || x.b.StatusID == 2))
                .Select(x => x.bs.SeatID)
                .ToList();

            var maxRow = seats.Max(s => s.RowNumber);
            var maxSeatInRow = seats.Max(s => s.SeatNumber);

            // Создаем строки
            for (int i = 0; i < maxRow; i++)
            {
                SeatsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Создаем колонки
            for (int j = 0; j < maxSeatInRow + 1; j++)
            {
                SeatsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            // Размещаем места
            foreach (var seat in seats)
            {
                Button btnSeat = new Button
                {
                    Content = seat.SeatNumber,
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(5),
                    Tag = seat.SeatID
                };

                // Стиль в зависимости от статуса
                if (bookedSeatIDs.Contains(seat.SeatID))
                {
                    btnSeat.Background = (Brush)FindResource("GrayBrush");
                    btnSeat.IsEnabled = false;
                    btnSeat.ToolTip = "Занято";
                }
                else
                {
                    btnSeat.Background = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                    btnSeat.ToolTip = $"Ряд {seat.RowNumber}, Место {seat.SeatNumber} - {seat.Category}";

                    btnSeat.Click += BtnSeat_Click;
                }

                // VIP места выделяем
                if (seat.Category == "VIP")
                {
                    btnSeat.BorderBrush = new SolidColorBrush(Colors.Gold);
                    btnSeat.BorderThickness = new Thickness(2);
                }

                Grid.SetRow(btnSeat, seat.RowNumber - 1);
                Grid.SetColumn(btnSeat, seat.SeatNumber);

                SeatsGrid.Children.Add(btnSeat);
                seatButtons[seat.SeatID] = btnSeat;
            }

            // Добавляем номера рядов
            for (int i = 0; i < maxRow; i++)
            {
                TextBlock rowLabel = new TextBlock
                {
                    Text = $"Ряд {i + 1}",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5),
                    FontWeight = FontWeights.Bold
                };

                Grid.SetRow(rowLabel, i);
                Grid.SetColumn(rowLabel, 0);
                SeatsGrid.Children.Add(rowLabel);
            }
        }

        private void BtnSeat_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int seatID = (int)btn.Tag;

            if (selectedSeatIDs.Contains(seatID))
            {
                selectedSeatIDs.Remove(seatID);
                btn.Background = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
            }
            else
            {
                selectedSeatIDs.Add(seatID);
                btn.Background = (Brush)FindResource("PrimaryBrush");
                btn.Foreground = (Brush)FindResource("WhiteBrush");
            }

            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            decimal totalPrice = 0;

            foreach (var seatID in selectedSeatIDs)
            {
                var seat = Connection.db.Seats.Find(seatID);
                decimal seatPrice = currentSession.BasePrice;

                if (seat?.Category == "VIP")
                {
                    seatPrice *= 1.5m;
                }

                totalPrice += seatPrice;
            }

            txtTotalPrice.Text = $"Итого: {totalPrice:F2} руб.";
            txtSelectedSeats.Text = $"Выбрано мест: {selectedSeatIDs.Count}";
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSeatIDs.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одно место!",
                               "Предупреждение",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            if (App.CurrentUserID == 0)
            {
                MessageBox.Show("Для бронирования необходимо авторизоваться!",
                               "Требуется авторизация",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
                return;
            }

            // Переход к странице оплаты
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.MainFrame.Navigate(
                new Pages.User.PaymentPage(sessionID, selectedSeatIDs));
        }

        
    }
}
