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
using System.Windows.Shapes;
using CinemaApp.DataBase;

namespace CinemaApp.Pages.User
{
    /// <summary>
    /// Логика взаимодействия для ChangePasswordDialog.xaml
    /// </summary>
    public partial class ChangePasswordDialog : Window
    {
        public ChangePasswordDialog()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string oldPass = txtOldPassword.Password;
                string newPass = txtNewPassword.Password;
                string confirmPass = txtConfirmPassword.Password;

                // Проверка текущего пароля
                var user = Connection.db.Users.Find(App.CurrentUserID);
                if (user.Password != oldPass)
                {
                    MessageBox.Show("Неверный текущий пароль!");
                    return;
                }

                // Проверка нового пароля
                if (string.IsNullOrWhiteSpace(newPass))
                {
                    MessageBox.Show("Введите новый пароль!");
                    return;
                }

                if (newPass.Length < 3)
                {
                    MessageBox.Show("Пароль должен содержать минимум 3 символа!");
                    return;
                }

                if (newPass != confirmPass)
                {
                    MessageBox.Show("Пароли не совпадают!");
                    return;
                }

                // Сохранение
                user.Password = newPass;
                Connection.db.SaveChanges();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}
