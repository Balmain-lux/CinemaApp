using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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


namespace CinemaApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // По умолчанию показываем фильмы
            MainFrame.Navigate(new Pages.Common.MoviesPage());
            UpdateNavigationForRole();
        }

        public void UpdateNavigationForRole()
        {
            if (App.CurrentUserID > 0)
            {
                // Пользователь авторизован
                txtUserName.Text = App.CurrentUserName;
                txtUserName.Visibility = Visibility.Visible;
                btnLogin.Visibility = Visibility.Collapsed;
                btnRegister.Visibility = Visibility.Collapsed;
                btnProfile.Visibility = Visibility.Visible;
                btnLogout.Visibility = Visibility.Visible;

                // Дополнительные кнопки для админа
                if (App.CurrentUserRole == "Admin")
                {
                    btnProfile.Content = "Админ панель";
                }
                else if (App.CurrentUserRole == "Employee")
                {
                    btnProfile.Content = "Панель сотрудника";
                }
                else
                {
                    btnProfile.Content = "Мой профиль";
                }
            }
            else
            {
                // Гость
                txtUserName.Visibility = Visibility.Collapsed;
                btnLogin.Visibility = Visibility.Visible;
                btnRegister.Visibility = Visibility.Visible;
                btnProfile.Visibility = Visibility.Collapsed;
                btnLogout.Visibility = Visibility.Collapsed;
            }
        }


        private void btnMovies_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUserID == 0)
                MainFrame.Navigate(new Pages.Guest.GuestMoviesPage());
            else
                MainFrame.Navigate(new Pages.Common.MoviesPage());
        }

        private void btnSessions_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUserID == 0)
                MainFrame.Navigate(new Pages.Guest.GuestSessionPage());
            else
                MainFrame.Navigate(new Pages.Common.SessionSelectionPage());
        }

        

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.Guest.AuthPage("login"));
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.Guest.AuthPage("register"));
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            switch (App.CurrentUserRole)
            {
                case "Admin":
                    MainFrame.Navigate(new Pages.Admin.AdminDashboardPage());
                    break;
                case "Employee":
                    MainFrame.Navigate(new Pages.Employee.EmployeeDashboardPage());
                    break;
                default:
                    MainFrame.Navigate(new Pages.User.UserCabinetPage());
                    break;
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUserID = 0;
            App.CurrentUserRole = null;
            App.CurrentUserName = null;
            UpdateNavigationForRole();
            MainFrame.Navigate(new Pages.Common.MoviesPage());
        }
    }
}
