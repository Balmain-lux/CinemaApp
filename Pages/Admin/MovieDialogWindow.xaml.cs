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

namespace CinemaApp.Pages.Admin
{
    /// <summary>
    /// Логика взаимодействия для MovieDialogWindow.xaml
    /// </summary>
    public partial class MovieDialogWindow : Window
    {
        private Movies editingMovie;
        public MovieDialogWindow(Movies movie = null)
        {
            InitializeComponent();
            editingMovie = movie;
            LoadGenres();

            if (movie != null)
            {
                LoadMovieData();
                Title = "Редактирование фильма";
            }
            else
            {
                Title = "Добавление фильма";
            }
        }

        private void LoadGenres()
        {
            var genres = Connection.db.Genres.ToList();
            lstGenres.ItemsSource = genres;
            lstGenres.DisplayMemberPath = "GenreName";
            lstGenres.SelectedValuePath = "GenreID";
        }

        private void LoadMovieData()
        {
            txtTitle.Text = editingMovie.Title;
            txtDescription.Text = editingMovie.Description;
            txtDuration.Text = editingMovie.Duration.ToString();
            txtYear.Text = editingMovie.ReleaseYear?.ToString();
            txtAge.Text = editingMovie.AgeRestriction;
            txtCountry.Text = editingMovie.Country;
            txtDirector.Text = editingMovie.Director;

            // Выбираем жанры фильма
            var movieGenres = Connection.db.MovieGenres
                .Where(mg => mg.MovieID == editingMovie.MovieID)
                .Select(mg => mg.GenreID)
                .ToList();

            foreach (var item in lstGenres.Items)
            {
                var genre = item as Genres;
                if (genre != null && movieGenres.Contains(genre.GenreID))
                {
                    lstGenres.SelectedItems.Add(item);
                }
            }
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
                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    MessageBox.Show("Введите название фильма!");
                    return;
                }

                if (!int.TryParse(txtDuration.Text, out int duration))
                {
                    MessageBox.Show("Введите корректную длительность!");
                    return;
                }

                if (editingMovie == null)
                {
                    // Создание нового фильма
                    editingMovie = new Movies
                    {
                        IsActive = true
                    };
                    Connection.db.Movies.Add(editingMovie);
                }

                // Заполнение данных
                editingMovie.Title = txtTitle.Text;
                editingMovie.Description = txtDescription.Text;
                editingMovie.Duration = duration;
                editingMovie.ReleaseYear = string.IsNullOrWhiteSpace(txtYear.Text) ? (int?)null : int.Parse(txtYear.Text);
                editingMovie.AgeRestriction = txtAge.Text;
                editingMovie.Country = txtCountry.Text;
                editingMovie.Director = txtDirector.Text;

                Connection.db.SaveChanges();

                // Обновление жанров
                var currentGenres = Connection.db.MovieGenres
                    .Where(mg => mg.MovieID == editingMovie.MovieID)
                    .ToList();

                foreach (var genre in currentGenres)
                {
                    Connection.db.MovieGenres.Remove(genre);
                }

                foreach (var item in lstGenres.SelectedItems)
                {
                    var genre = item as Genres;
                    if (genre != null)
                    {
                        Connection.db.MovieGenres.Add(new MovieGenres
                        {
                            MovieID = editingMovie.MovieID,
                            GenreID = genre.GenreID
                        });
                    }
                }

                Connection.db.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }
    }
}
