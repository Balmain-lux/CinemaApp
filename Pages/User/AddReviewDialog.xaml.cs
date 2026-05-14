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
    /// Логика взаимодействия для AddReviewDialog.xaml
    /// </summary>
    public partial class AddReviewDialog : Window
    {
        private Reviews editingReview;
        private int selectedRating = 0;
        public AddReviewDialog(Reviews review = null)
        {
            InitializeComponent();
            editingReview = review;
            LoadMovies();

            if (review != null)
            {
                Title = "Редактирование отзыва";
                LoadReviewData();
            }
            else
            {
                Title = "Новый отзыв";
            }
        }

        private void LoadMovies()
        {
            // Только фильмы, которые пользователь уже смотрел (есть бронирования)
            var watchedMovies = Connection.db.Bookings
                .Where(b => b.UserID == App.CurrentUserID && b.StatusID == 2)
                .Select(b => b.Sessions.Movies)
                .Distinct()
                .ToList();

            cmbMovies.ItemsSource = watchedMovies;
            if (watchedMovies.Any())
                cmbMovies.SelectedIndex = 0;
        }

        private void LoadReviewData()
        {
            // Выбираем фильм
            var movie = Connection.db.Movies.Find(editingReview.MovieID);
            cmbMovies.SelectedValue = movie?.MovieID;
            cmbMovies.IsEnabled = false; // Нельзя менять фильм при редактировании

            // Загружаем рейтинг
            selectedRating = editingReview.Rating;
            UpdateStars(selectedRating);

            // Загружаем комментарий
            txtComment.Text = editingReview.Comment;
        }

        private void star1_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            selectedRating = int.Parse(btn.Tag.ToString());
            UpdateStars(selectedRating);
        }

        private void UpdateStars(int rating)
        {
            Button[] stars = { star1, star2, star3, star4, star5 };

            for (int i = 0; i < stars.Length; i++)
            {
                if (i < rating)
                {
                    stars[i].Content = "★";
                    stars[i].Foreground = Brushes.Gold;
                }
                else
                {
                    stars[i].Content = "☆";
                    stars[i].Foreground = (Brush)FindResource("GrayBrush");
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
                // Проверки
                if (cmbMovies.SelectedValue == null)
                {
                    MessageBox.Show("Выберите фильм!");
                    return;
                }

                if (selectedRating == 0)
                {
                    MessageBox.Show("Поставьте оценку!");
                    return;
                }

                int movieID = (int)cmbMovies.SelectedValue;
                string comment = txtComment.Text.Trim();

                if (editingReview == null)
                {
                    // Проверка на существующий отзыв
                    var existing = Connection.db.Reviews
                        .FirstOrDefault(r => r.UserID == App.CurrentUserID && r.MovieID == movieID);

                    if (existing != null)
                    {
                        MessageBox.Show("Вы уже оставляли отзыв на этот фильм!\nИспользуйте редактирование.",
                            "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Создание нового отзыва
                    var newReview = new Reviews
                    {
                        UserID = App.CurrentUserID,
                        MovieID = movieID,
                        Rating = selectedRating,
                        Comment = comment,
                        ReviewDate = DateTime.Now
                    };

                    Connection.db.Reviews.Add(newReview);
                }
                else
                {
                    // Обновление существующего
                    editingReview.Rating = selectedRating;
                    editingReview.Comment = comment;
                    editingReview.ReviewDate = DateTime.Now;
                }

                Connection.db.SaveChanges();

                MessageBox.Show("Отзыв сохранен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

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
