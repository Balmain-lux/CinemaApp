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

namespace CinemaApp.Pages.Employee
{
    /// <summary>
    /// Логика взаимодействия для EmployeeShiftsPage.xaml
    /// </summary>
    public partial class EmployeeShiftsPage : Page
    {
        public EmployeeShiftsPage()
        {
            InitializeComponent();
            LoadShifts();
        }

        private void LoadShifts()
        {
            var shifts = Connection.db.Shifts
                .Where(s => s.EmployeeID == App.CurrentUserID)
                .OrderByDescending(s => s.ShiftDate)
                .ToList();

            ShiftsList.ItemsSource = shifts;
        }

        private void btnAddShift_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dpShiftDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите дату!");
                    return;
                }

                TimeSpan startTime;
                TimeSpan endTime;

                if (!TimeSpan.TryParse(txtStartTime.Text, out startTime))
                {
                    MessageBox.Show("Неверный формат времени начала! Используйте ЧЧ:ММ");
                    return;
                }

                if (!TimeSpan.TryParse(txtEndTime.Text, out endTime))
                {
                    MessageBox.Show("Неверный формат времени окончания! Используйте ЧЧ:ММ");
                    return;
                }

                if (endTime <= startTime)
                {
                    MessageBox.Show("Время окончания должно быть позже времени начала!");
                    return;
                }

                var newShift = new Shifts
                {
                    EmployeeID = App.CurrentUserID,
                    ShiftDate = dpShiftDate.SelectedDate.Value,
                    StartTime = startTime,
                    EndTime = endTime
                };

                Connection.db.Shifts.Add(newShift);
                Connection.db.SaveChanges();

                MessageBox.Show("Смена добавлена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LoadShifts();

                // Сброс
                txtStartTime.Text = "09:00";
                txtEndTime.Text = "17:00";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void btnDeleteShift_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Удалить эту смену?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var btn = sender as Button;
                    int shiftID = (int)btn.Tag;
                    var shift = Connection.db.Shifts.Find(shiftID);

                    if (shift != null)
                    {
                        Connection.db.Shifts.Remove(shift);
                        Connection.db.SaveChanges();
                        LoadShifts();
                        MessageBox.Show("Смена удалена!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }
    }
}
