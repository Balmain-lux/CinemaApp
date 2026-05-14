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
    /// Логика взаимодействия для ManageSessionsPage.xaml
    /// </summary>
    public partial class ManageSessionsPage : Page
    {
        public ManageSessionsPage()
        {
            InitializeComponent();
            LoadMovies();
            LoadHalls();
            LoadSessions();
            dpDate.SelectedDate = DateTime.Today;
        }

        private void LoadMovies()
        {
            var movies = Connection.db.Movies.Where(m => m.IsActive == true).ToList();
            cmbMovie.ItemsSource = movies;
            if (movies.Any())
                cmbMovie.SelectedIndex = 0;
        }

        private void LoadHalls()
        {
            var halls = Connection.db.Halls.ToList();
            cmbHall.ItemsSource = halls;
            if (halls.Any())
                cmbHall.SelectedIndex = 0;
        }

        private void LoadSessions()
        {
            var sessions = Connection.db.Sessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionDateTime)
                .ToList();

            SessionsList.ItemsSource = sessions;
        }

        private void btnAddSession_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка выбора фильма
                if (cmbMovie.SelectedValue == null)
                {
                    MessageBox.Show("Выберите фильм!");
                    return;
                }

                // Проверка выбора зала
                if (cmbHall.SelectedValue == null)
                {
                    MessageBox.Show("Выберите зал!");
                    return;
                }

                // Проверка даты
                if (!dpDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите дату!");
                    return;
                }

                // Проверка времени
                if (string.IsNullOrWhiteSpace(txtTime.Text))
                {
                    MessageBox.Show("Введите время!");
                    return;
                }

                // Проверка цены
                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
                {
                    MessageBox.Show("Введите корректную цену!");
                    return;
                }

                // Формирование даты и времени
                DateTime dateTime;
                try
                {
                    var timeParts = txtTime.Text.Split(':');
                    int hours = int.Parse(timeParts[0]);
                    int minutes = int.Parse(timeParts[1]);

                    dateTime = new DateTime(
                        dpDate.SelectedDate.Value.Year,
                        dpDate.SelectedDate.Value.Month,
                        dpDate.SelectedDate.Value.Day,
                        hours, minutes, 0);
                }
                catch
                {
                    MessageBox.Show("Неверный формат времени! Используйте ЧЧ:ММ");
                    return;
                }

                // Создание сеанса
                var newSession = new Sessions
                {
                    MovieID = (int)cmbMovie.SelectedValue,
                    HallID = (int)cmbHall.SelectedValue,
                    SessionDateTime = dateTime,
                    BasePrice = price,
                    IsActive = true
                };

                Connection.db.Sessions.Add(newSession);
                Connection.db.SaveChanges();

                MessageBox.Show("Сеанс успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LoadSessions();

                // Сброс формы
                txtPrice.Text = "350";
                txtTime.Text = "14:00";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении сеанса: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDeleteSession_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить этот сеанс?\n" +
                                        "Все связанные бронирования будут удалены!",
                                        "Подтверждение",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var btn = sender as Button;
                    int sessionID = (int)btn.Tag;
                    var session = Connection.db.Sessions.Find(sessionID);

                    if (session != null)
                    {
                        // Мягкое удаление
                        session.IsActive = false;
                        Connection.db.SaveChanges();

                        LoadSessions();
                        MessageBox.Show("Сеанс удален!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                }
            }
        }
    }
}
