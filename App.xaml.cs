using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CinemaApp
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static int CurrentUserID { get; set; }
        public static string CurrentUserRole { get; set; }
        public static string CurrentUserName { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Проверка подключения к БД
            if (!DataBase.Connection.TestConnection())
            {
                MessageBox.Show("Ошибка подключения к базе данных!",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }
    }
}
