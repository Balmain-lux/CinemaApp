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
    /// Логика взаимодействия для ManageMoviesPage.xaml
    /// </summary>
    public partial class ManageMoviesPage : Page
    {
        public ManageMoviesPage()
        {
            InitializeComponent();
            LoadMovies();
        }

        private void LoadMovies(string search = "")
        {
            var query = Connection.db.Movies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Title.Contains(search) ||
                                        m.Description.Contains(search));
            }

            MoviesList.ItemsSource = query.OrderByDescending(m => m.MovieID).ToList();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int movieID = (int)btn.Tag;
            var movie = Connection.db.Movies.Find(movieID);

            var dialog = new MovieDialogWindow(movie);
            if (dialog.ShowDialog() == true)
            {
                LoadMovies();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить этот фильм?",
                                       "Подтверждение",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var btn = sender as Button;
                int movieID = (int)btn.Tag;
                var movie = Connection.db.Movies.Find(movieID);

                try
                {
                    // Мягкое удаление
                    movie.IsActive = false;
                    Connection.db.SaveChanges();
                    LoadMovies(txtSearch.Text);
                    MessageBox.Show("Фильм удален!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MovieDialogWindow();
            if (dialog.ShowDialog() == true)
            {
                LoadMovies();
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadMovies(txtSearch.Text);
        }
    }
}
