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

namespace CinemaApp.Pages.Common
{
    /// <summary>
    /// Логика взаимодействия для MoviesPage.xaml
    /// </summary>
    public partial class MoviesPage : Page
    {
        private List<Button> genreButtons = new List<Button>();
        private string selectedGenre = "Все";

        public MoviesPage()
        {
            InitializeComponent();
            LoadGenres();
            LoadMovies("Все");
        }

        private void LoadGenres()
        {
            var genres = Connection.db.Genres.ToList();

            foreach (var genre in genres)
            {
                Button btnGenre = new Button
                {
                    Content = genre.GenreName,
                    Width = 120,
                    Height = 45,
                    Margin = new Thickness(0, 0, 10, 10),
                    Tag = genre.GenreName
                };

                btnGenre.Click += (s, e) =>
                {
                    var btn = s as Button;
                    selectedGenre = btn.Tag.ToString();
                    LoadMovies(selectedGenre);
                    UpdateGenreButtonStyles(btn);
                };

                GenresPanel.Items.Add(btnGenre);
                genreButtons.Add(btnGenre);
            }

            UpdateGenreButtonStyles(null); // Выделить "Все"
        }

        private void UpdateGenreButtonStyles(Button selectedButton)
        {
            // Сбросить все кнопки
            foreach (var btn in genreButtons)
            {
                btn.Style = (Style)FindResource("SecondaryButton");
            }

            // Выделить выбранную
            if (selectedButton != null)
            {
                selectedButton.Style = (Style)FindResource("PrimaryButton");
            }
        }

        private void LoadMovies(string genreFilter)
        {
            MoviesPanel.Children.Clear();

            List<Movies> movies;

            if (genreFilter == "Все")
            {
                movies = Connection.db.Movies.Where(m => m.IsActive == true).ToList();
            }
            else
            {
                var genreID = Connection.db.Genres
                    .FirstOrDefault(g => g.GenreName == genreFilter)?.GenreID;

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
                Width = 250,
                Height = 400,
                Margin = new Thickness(10)
            };

            StackPanel panel = new StackPanel();

            // Постер (заглушка, если нет картинки)
            TextBlock posterPlaceholder = new TextBlock
            {
                Text = "🎬",
                FontSize = 60,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 10)
            };

            // Название фильма
            TextBlock title = new TextBlock
            {
                Text = movie.Title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("PrimaryBrush"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Жанры
            var genres = Connection.db.MovieGenres
                .Where(mg => mg.MovieID == movie.MovieID)
                .Join(Connection.db.Genres,
                    mg => mg.GenreID,
                    g => g.GenreID,
                    (mg, g) => g.GenreName)
                .ToList();

            TextBlock genresText = new TextBlock
            {
                Text = string.Join(", ", genres),
                FontSize = 12,
                Foreground = (Brush)FindResource("GrayBrush"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Длительность и год
            TextBlock info = new TextBlock
            {
                Text = $"{movie.Duration} мин | {movie.AgeRestriction} | {movie.ReleaseYear}",
                FontSize = 12,
                Foreground = (Brush)FindResource("GrayBrush"),
                Margin = new Thickness(0, 0, 0, 15)
            };

            // Кнопка "К сеансам"
            Button btnSessions = new Button
            {
                Content = "К сеансам",
                Style = (Style)FindResource("PrimaryButton"),
                Width = 220,
                Tag = movie.MovieID
            };

            btnSessions.Click += (s, e) =>
            {
                var btn = s as Button;
                int movieID = (int)btn.Tag;

                // Переход на страницу выбора сеансов для конкретного фильма
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.MainFrame.Navigate(
                    new Pages.Common.SessionSelectionPage());
            };

            panel.Children.Add(posterPlaceholder);
            panel.Children.Add(title);
            panel.Children.Add(genresText);
            panel.Children.Add(info);
            panel.Children.Add(btnSessions);

            card.Child = panel;
            return card;
        }

        private void btnAllGenres_Click(object sender, RoutedEventArgs e)
        {
            selectedGenre = "Все";
            LoadMovies("Все");
            UpdateGenreButtonStyles(null);
        }
    }
}
