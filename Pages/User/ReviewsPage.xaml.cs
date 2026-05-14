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
    /// Логика взаимодействия для ReviewsPage.xaml
    /// </summary>
    public partial class ReviewsPage : Page
    {
        public ReviewsPage()
        {
            InitializeComponent();
            LoadUserReviews();
        }

        private void LoadUserReviews()
        {
            var reviews = Connection.db.Reviews
                .Where(r => r.UserID == App.CurrentUserID)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();

            ReviewsList.ItemsSource = reviews;
        }

        private void btnEditReview_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int reviewID = (int)btn.Tag;
            var review = Connection.db.Reviews.Find(reviewID);

            if (review != null)
            {
                var dialog = new AddReviewDialog(review);
                dialog.Owner = Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    LoadUserReviews();
                }
            }
        }

        private void btnDeleteReview_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить этот отзыв?",
                                        "Подтверждение",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var btn = sender as Button;
                int reviewID = (int)btn.Tag;
                var review = Connection.db.Reviews.Find(reviewID);

                try
                {
                    Connection.db.Reviews.Remove(review);
                    Connection.db.SaveChanges();
                    LoadUserReviews();
                    MessageBox.Show("Отзыв удален!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                }
            }
        }

        private void btnAddReview_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddReviewDialog();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                LoadUserReviews();
            }
        }
    }
}
