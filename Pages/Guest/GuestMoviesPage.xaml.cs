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
    /// Логика взаимодействия для GuestMoviesPage.xaml
    /// </summary>
    public partial class GuestMoviesPage : Page
    {
        private string selectedGenre = "Все";
        private List<Button> genreButtons = new List<Button>();
        public GuestMoviesPage()
        {
            InitializeComponent();
            LoadGenres();
            LoadMovies();
        }

        private void LoadGenres()
        {
            var genres = Connection.db.Genres.ToList();

            // Кнопка "Все"
            Button btnAll = new Button
            {
                Content = "Все",
                Width = 80,
                Height = 45,
                Margin = new Thickness(0, 0, 10, 5),
                Tag = "Все"
            };
            btnAll.Click += (s, e) => FilterByGenre("Все");
            btnAll.Style = (Style)FindResource("PrimaryButton");
            GenresPanel.Children.Add(btnAll);
            genreButtons.Add(btnAll);

            // Кнопки жанров
            foreach (var genre in genres)
            {
                Button btnGenre = new Button
                {
                    Content = genre.GenreName,
                    Width = 120,
                    Height = 45,
                    Margin = new Thickness(0, 0, 10, 5),
                    Tag = genre.GenreName
                };
                btnGenre.Click += (s, e) => FilterByGenre(btnGenre.Tag.ToString());
                btnGenre.Style = (Style)FindResource("SecondaryButton");
                GenresPanel.Children.Add(btnGenre);
                genreButtons.Add(btnGenre);
            }
        }

        private void FilterByGenre(string genre)
        {
            selectedGenre = genre;

            // Обновляем стили кнопок
            foreach (var btn in genreButtons)
            {
                if (btn.Tag.ToString() == genre)
                    btn.Style = (Style)FindResource("PrimaryButton");
                else
                    btn.Style = (Style)FindResource("SecondaryButton");
            }

            LoadMovies();
        }

        private void LoadMovies()
        {
            MoviesPanel.Children.Clear();

            List<Movies> movies;

            if (selectedGenre == "Все")
            {
                movies = Connection.db.Movies.Where(m => m.IsActive == true).ToList();
            }
            else
            {
                var genreID = Connection.db.Genres
                    .FirstOrDefault(g => g.GenreName == selectedGenre)?.GenreID;

                if (genreID.HasValue)
                {
                    var movieIDs = Connection.db.MovieGenres
                        .Where(mg => mg.GenreID == genreID.Value)
                        .Select(mg => mg.MovieID);

                    movies = Connection.db.Movies
                        .Where(m => m.IsActive == true && movieIDs.Contains(m.MovieID))
                        .ToList();
                }
                else
                {
                    movies = new List<Movies>();
                }
            }

            foreach (var movie in movies)
            {
                MoviesPanel.Children.Add(CreateMovieCard(movie));
            }
        }

        private Border CreateMovieCard(Movies movie)
        {
            Border card = new Border
            {
                Style = (Style)FindResource("CardBorder"),
                Width = 230,
                Margin = new Thickness(10)
            };

            StackPanel panel = new StackPanel();

            // Постер (заглушка)
            TextBlock poster = new TextBlock
            {
                Text = "🎬",
                FontSize = 50,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 15, 0, 10)
            };

            // Название
            TextBlock title = new TextBlock
            {
                Text = movie.Title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("PrimaryBrush"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10, 0, 10, 5)
            };

            // Жанры
            var genres = Connection.db.MovieGenres
                .Where(mg => mg.MovieID == movie.MovieID)
                .Join(Connection.db.Genres,
                    mg => mg.GenreID,
                    g => g.GenreID,
                    (mg, g) => g.GenreName)
                .Take(2)
                .ToList();

            TextBlock genresText = new TextBlock
            {
                Text = string.Join(", ", genres),
                FontSize = 11,
                Foreground = (Brush)FindResource("GrayBrush"),
                Margin = new Thickness(10, 0, 10, 5)
            };

            // Информация
            TextBlock info = new TextBlock
            {
                Text = $"{movie.Duration} мин | {movie.AgeRestriction}",
                FontSize = 11,
                Foreground = (Brush)FindResource("GrayBrush"),
                Margin = new Thickness(10, 0, 10, 10)
            };

            // Кнопка "Подробнее"
            Button btnDetails = new Button
            {
                Content = "Подробнее →",
                Style = (Style)FindResource("SecondaryButton"),
                Height = 45,
                Margin = new Thickness(10, 0, 10, 15),
                Tag = movie.MovieID
            };

            btnDetails.Click += (s, e) =>
            {
                var btn = s as Button;
                int movieID = (int)btn.Tag;
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.MainFrame.Navigate(new GuestSessionPage(movieID));
            };

            panel.Children.Add(poster);
            panel.Children.Add(title);
            panel.Children.Add(genresText);
            panel.Children.Add(info);
            panel.Children.Add(btnDetails);

            card.Child = panel;
            return card;
        }

        private void btnGoToAuth_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.MainFrame.Navigate(new AuthPage("login"));
        }
    }
}
