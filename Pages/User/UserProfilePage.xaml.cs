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

namespace CinemaApp.Pages.User
{
    /// <summary>
    /// Логика взаимодействия для UserProfilePage.xaml
    /// </summary>
    public partial class UserProfilePage : Page
    {
        private Users currentUser;
        public UserProfilePage()
        {
            InitializeComponent(); 
            LoadUserData();
            LoadUserStats();
        }

        private void LoadUserData()
        {
            currentUser = Connection.db.Users.Find(App.CurrentUserID);

            if (currentUser != null)
            {
                txtFullName.Text = currentUser.FullName;
                txtLogin.Text = currentUser.Login;
                txtEmail.Text = currentUser.Email;
                txtPhone.Text = currentUser.Phone;
                txtRegDate.Text = currentUser.RegistrationDate?.ToString("dd.MM.yyyy") ?? "Не указана";
            }
        }

        private void LoadUserStats()
        {
            // Всего бронирований
            int totalBookings = Connection.db.Bookings
                .Count(b => b.UserID == App.CurrentUserID);
            txtTotalBookings.Text = totalBookings.ToString();

            // Потрачено
            decimal totalSpent = Connection.db.Bookings
                .Where(b => b.UserID == App.CurrentUserID && b.StatusID == 2)
                .Sum(b => (decimal?)b.TotalPrice) ?? 0;
            txtTotalSpent.Text = totalSpent.ToString("F2");

            // Количество отзывов
            int totalReviews = Connection.db.Reviews
                .Count(r => r.UserID == App.CurrentUserID);
            txtTotalReviews.Text = totalReviews.ToString();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentUser != null)
                {
                    currentUser.Email = txtEmail.Text;
                    currentUser.Phone = txtPhone.Text;

                    Connection.db.SaveChanges();

                    // Обновляем глобальное имя
                    App.CurrentUserName = currentUser.FullName;

                    MessageBox.Show("Данные успешно обновлены!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем навигацию
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow?.UpdateNavigationForRole();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ChangePasswordDialog();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                MessageBox.Show("Пароль успешно изменен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
