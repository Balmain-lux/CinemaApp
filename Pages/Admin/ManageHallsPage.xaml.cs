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
    /// Логика взаимодействия для ManageHallsPage.xaml
    /// </summary>
    public partial class ManageHallsPage : Page
    {
        public ManageHallsPage()
        {
            InitializeComponent();
            LoadHalls();
        }

        private void LoadHalls()
        {
            var halls = Connection.db.Halls.ToList();
            HallsList.ItemsSource = halls;
        }

        private void btnAddHall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка названия
                if (string.IsNullOrWhiteSpace(txtHallName.Text))
                {
                    MessageBox.Show("Введите название зала!");
                    return;
                }

                // Проверка вместимости
                if (!int.TryParse(txtCapacity.Text, out int capacity) || capacity <= 0)
                {
                    MessageBox.Show("Введите корректную вместимость!");
                    return;
                }

                // Проверка уникальности названия
                if (Connection.db.Halls.Any(h => h.HallName == txtHallName.Text))
                {
                    MessageBox.Show("Зал с таким названием уже существует!");
                    return;
                }

                // Создание зала
                var newHall = new Halls
                {
                    HallName = txtHallName.Text,
                    Capacity = capacity,
                    Description = txtDescription.Text
                };

                Connection.db.Halls.Add(newHall);
                Connection.db.SaveChanges();

                // Создание мест в зале
                CreateSeatsForHall(newHall.HallID, capacity);

                MessageBox.Show("Зал успешно добавлен с автоматической генерацией мест!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Очистка формы
                txtHallName.Text = "";
                txtCapacity.Text = "50";
                txtDescription.Text = "";

                LoadHalls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении зала: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateSeatsForHall(int hallID, int capacity)
        {
            // Создание мест: 10 рядов
            int rows = 10;
            int seatsPerRow = capacity / rows;

            for (int row = 1; row <= rows; row++)
            {
                for (int seatNum = 1; seatNum <= seatsPerRow; seatNum++)
                {
                    // VIP места в первых 2 рядах
                    string category = (row <= 2) ? "VIP" : "Обычное";

                    var seat = new Seats
                    {
                        HallID = hallID,
                        RowNumber = row,
                        SeatNumber = seatNum,
                        Category = category
                    };

                    Connection.db.Seats.Add(seat);
                }
            }

            Connection.db.SaveChanges();
        }

        private void btnDeleteHall_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить зал?\n" +
                                        "Все связанные сеансы и места будут удалены!",
                                        "Подтверждение",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var btn = sender as Button;
                    int hallID = (int)btn.Tag;
                    var hall = Connection.db.Halls.Find(hallID);

                    if (hall != null)
                    {
                        Connection.db.Halls.Remove(hall);
                        Connection.db.SaveChanges();

                        LoadHalls();
                        MessageBox.Show("Зал удален!");
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
