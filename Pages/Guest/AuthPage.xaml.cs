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
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private bool isLoginMode = true;
        public AuthPage(string mode = "login")
        {
            InitializeComponent();

            if (mode == "register")
            {
                SwitchToRegister();
            }
        }



        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            if (isLoginMode)
            {
                Login();
            }
            else
            {
                Register();
            }
        }

        private void Login()
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            // ОТЛАДКА: проверим, есть ли пользователь в БД
            var allUsers = Connection.db.Users.ToList();
            var foundUser = allUsers.FirstOrDefault(u => u.Login == login);

            if (foundUser == null)
            {
                MessageBox.Show($"Пользователь с логином '{login}' не найден в БД!");
                return;
            }

            if (foundUser.Password != password)
            {
                MessageBox.Show($"Пароль неверный! В БД: {foundUser.Password}, Вы ввели: {password}");
                return;
            }

            // Если дошли сюда - все правильно
            App.CurrentUserID = foundUser.UserID;
            App.CurrentUserName = foundUser.FullName;

            var role = Connection.db.Roles.Find(foundUser.RoleID);
            App.CurrentUserRole = role?.RoleName;

            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.UpdateNavigationForRole();
            mainWindow?.MainFrame.Navigate(new Common.MoviesPage());

            MessageBox.Show($"Добро пожаловать, {foundUser.FullName}!");
        }

        private void Register()
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password)
                || string.IsNullOrEmpty(fullName))
            {
                MessageBox.Show("Заполните обязательные поля (логин, пароль, имя)!");
                return;
            }

            if (Connection.db.Users.Any(u => u.Login == login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует!");
                return;
            }

            var userRole = Connection.db.Roles.FirstOrDefault(r => r.RoleName == "User");

            var newUser = new Users
            {
                Login = login,
                Password = password,
                FullName = fullName,
                Email = email,
                Phone = phone,
                RoleID = userRole.RoleID,
                RegistrationDate = DateTime.Now,
                IsActive = true
            };

            Connection.db.Users.Add(newUser);
            Connection.db.SaveChanges();

            MessageBox.Show("Регистрация успешна! Теперь войдите в систему.");
            SwitchToLogin();
        }

        private void btnSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (isLoginMode)
                SwitchToRegister();
            else
                SwitchToLogin();
        }

        private void SwitchToRegister()
        {
            isLoginMode = false;
            txtTitle.Text = "Регистрация";
            btnAction.Content = "Зарегистрироваться";
            btnSwitch.Content = "Уже есть аккаунт? Войти";

            lblFullName.Visibility = Visibility.Visible;
            txtFullName.Visibility = Visibility.Visible;
            lblEmail.Visibility = Visibility.Visible;
            txtEmail.Visibility = Visibility.Visible;
            lblPhone.Visibility = Visibility.Visible;
            txtPhone.Visibility = Visibility.Visible;
        }

        private void SwitchToLogin()
        {
            isLoginMode = true;
            txtTitle.Text = "Вход в систему";
            btnAction.Content = "Войти";
            btnSwitch.Content = "Нет аккаунта? Зарегистрироваться";

            lblFullName.Visibility = Visibility.Collapsed;
            txtFullName.Visibility = Visibility.Collapsed;
            lblEmail.Visibility = Visibility.Collapsed;
            txtEmail.Visibility = Visibility.Collapsed;
            lblPhone.Visibility = Visibility.Collapsed;
            txtPhone.Visibility = Visibility.Collapsed;

            txtLogin.Text = "";
            txtPassword.Password = "";
        }
    }
}
