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
    /// Логика взаимодействия для PaymentPage.xaml
    /// </summary>
    public partial class PaymentPage : Page
    {
        private int sessionID;
        private List<int> seatIDs;
        private decimal totalPrice;
        public PaymentPage(int sessionID, List<int> seatIDs)
        {
            InitializeComponent();
            this.sessionID = sessionID;
            this.seatIDs = seatIDs;

            LoadPaymentMethods();
            CalculateTotal();
            ShowOrderDetails();
        }

        private void LoadPaymentMethods()
        {
            var methods = Connection.db.PaymentMethods.ToList();
            cmbPaymentMethod.ItemsSource = methods;
            cmbPaymentMethod.DisplayMemberPath = "MethodName";
            cmbPaymentMethod.SelectedValuePath = "PaymentMethodID";
            cmbPaymentMethod.SelectedIndex = 0;

            cmbPaymentMethod.SelectionChanged += (s, e) =>
            {
                if (cmbPaymentMethod.SelectedValue != null)
                {
                    int methodID = (int)cmbPaymentMethod.SelectedValue;
                    CardPaymentPanel.Visibility = methodID == 1
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            };
        }

        private void CalculateTotal()
        {
            var session = Connection.db.Sessions.Find(sessionID);
            totalPrice = 0;

            foreach (var seatID in seatIDs)
            {
                var seat = Connection.db.Seats.Find(seatID);
                decimal seatPrice = session.BasePrice;

                if (seat?.Category == "VIP")
                    seatPrice *= 1.5m;

                totalPrice += seatPrice;
            }

            txtTotalAmount.Text = $"Итого к оплате: {totalPrice:F2} руб.";
        }

        private void ShowOrderDetails()
        {
            var session = Connection.db.Sessions.Find(sessionID);
            var movie = Connection.db.Movies.Find(session.MovieID);
            var hall = Connection.db.Halls.Find(session.HallID);

            string seatsInfo = "";
            foreach (var seatID in seatIDs)
            {
                var seat = Connection.db.Seats.Find(seatID);
                seatsInfo += $"\nРяд {seat.RowNumber}, Место {seat.SeatNumber} ({seat.Category})";
            }

            txtOrderDetails.Text = $"Фильм: {movie.Title}\n" +
                                   $"Дата и время: {session.SessionDateTime:dd.MM.yyyy HH:mm}\n" +
                                   $"Зал: {hall.HallName}\n" +
                                   $"Места: {seatsInfo}";
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.MainFrame.Navigate(
                new Common.BookingPage(sessionID));
        }

        private void btnPay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbPaymentMethod.SelectedValue == null)
                {
                    MessageBox.Show("Выберите способ оплаты!");
                    return;
                }

                // Создание бронирования
                string seatIDsStr = string.Join(",", seatIDs);
                var result = Connection.db.CreateBooking(
                    App.CurrentUserID,
                    sessionID,
                    null,
                    null,
                    seatIDsStr,
                    totalPrice);

                // В реальном проекте здесь была бы интеграция с платежной системой

                MessageBox.Show($"Бронирование успешно создано!\nСумма: {totalPrice:F2} руб.",
                              "Успешно",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                // Переход в личный кабинет
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.MainFrame.Navigate(new UserCabinetPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании бронирования: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
    }
}
