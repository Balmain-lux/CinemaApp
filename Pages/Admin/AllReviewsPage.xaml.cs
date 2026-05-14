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
    /// Логика взаимодействия для AllReviewsPage.xaml
    /// </summary>
    public partial class AllReviewsPage : Page
    {
        public AllReviewsPage()
        {
            InitializeComponent();
            LoadReviews();
        }

        private void LoadReviews()
        {
            var reviews = Connection.db.Reviews
                .OrderByDescending(r => r.ReviewDate)
                .ToList();

            ReviewsList.ItemsSource = reviews;
        }
    }
}
