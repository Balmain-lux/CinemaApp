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
    /// Логика взаимодействия для ManageEmployeesPage.xaml
    /// </summary>
    public partial class ManageEmployeesPage : Page
    {
        public ManageEmployeesPage()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            // Получаем только сотрудников (роль Employee)
            var employeeRole = Connection.db.Roles.FirstOrDefault(r => r.RoleName == "Employee");

            if (employeeRole != null)
            {
                var employees = Connection.db.Users
                    .Where(u => u.RoleID == employeeRole.RoleID)
                    .ToList();

                EmployeesList.ItemsSource = employees;
            }
        }

        private void btnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка полей
                if (string.IsNullOrWhiteSpace(txtLogin.Text))
                {
                    MessageBox.Show("Введите логин!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    MessageBox.Show("Введите пароль!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    MessageBox.Show("Введите ФИО!");
                    return;
                }

                // Проверка уникальности логина
                if (Connection.db.Users.Any(u => u.Login == txtLogin.Text))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                    return;
                }

                // Получаем роль Employee
                var employeeRole = Connection.db.Roles.FirstOrDefault(r => r.RoleName == "Employee");
                if (employeeRole == null)
                {
                    MessageBox.Show("Роль 'Employee' не найдена!");
                    return;
                }

                // Создание сотрудника
                var newEmployee = new Users
                {
                    Login = txtLogin.Text,
                    Password = txtPassword.Password,
                    FullName = txtFullName.Text,
                    Email = "",
                    Phone = "",
                    RoleID = employeeRole.RoleID,
                    RegistrationDate = DateTime.Now,
                    IsActive = true
                };

                Connection.db.Users.Add(newEmployee);
                Connection.db.SaveChanges();

                MessageBox.Show("Сотрудник успешно добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Очистка формы
                txtLogin.Text = "";
                txtPassword.Password = "";
                txtFullName.Text = "";

                LoadEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении сотрудника: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Сбросить пароль на '123456'?",
                                       "Подтверждение",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var btn = sender as Button;
                    int userID = (int)btn.Tag;
                    var user = Connection.db.Users.Find(userID);

                    if (user != null)
                    {
                        user.Password = "123456";
                        Connection.db.SaveChanges();
                        MessageBox.Show("Пароль сброшен на '123456'!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void btnDeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить сотрудника?",
                                        "Подтверждение",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var btn = sender as Button;
                    int userID = (int)btn.Tag;
                    var user = Connection.db.Users.Find(userID);

                    if (user != null)
                    {
                        // Мягкое удаление
                        user.IsActive = false;
                        Connection.db.SaveChanges();

                        LoadEmployees();
                        MessageBox.Show("Сотрудник удален!");
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
