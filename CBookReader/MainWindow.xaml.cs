using Microsoft.Win32;
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

namespace CBookReader
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ComicBook ComicBook { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.ComicBook = new ComicBook();
            this.ComicBook.CurrentPageChanged += (() => 
            {
                this.image.Source = this.ComicBook.Pages[this.ComicBook.CurrentPage];

                if (this.ComicBook.CurrentPage == 0)
                {
                    this.firstButton.IsEnabled = false;
                    this.backButton.IsEnabled = false;
                }
                else if (this.ComicBook.CurrentPage > 0)
                {
                    this.firstButton.IsEnabled = true;
                    this.backButton.IsEnabled = true;
                }

                if (this.ComicBook.CurrentPage == this.ComicBook.Pages.Count - 1)
                {
                    this.lastButton.IsEnabled = false;
                    this.nextButton.IsEnabled = false;
                }
                else if (this.ComicBook.CurrentPage < this.ComicBook.Pages.Count - 0)
                {
                    this.lastButton.IsEnabled = true;
                    this.nextButton.IsEnabled = true;
                }
            });
        }

        private void Rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            rectangle.Fill = new SolidColorBrush(Colors.Black);
        }

        private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            rectangle.Fill = new SolidColorBrush(Colors.Transparent);
        }

        private void ToolbarButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            button.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                ShadowDepth = 1
            };
        }

        private void ToolbarButton_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            button.Effect = null;
        }

        private void ToolbarButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            button.Effect = null;
        }

        private void ToolbarButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            button.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                ShadowDepth = 1
            };
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                Title = "Открыть",
                Multiselect = true,
                Filter = "Все поддерживаемые (*.*)" +
                "|*.rar;*.zip;*.tar;*.7zip;" +
                "*.cbr;*.cbz;*.cbt;*.cb7;" +
                "*.jpg;*.jpeg;*.bmp;*.png;*.gif;*.tif;*.tiff" +
                "|Архивы (*.rar, *.zip, *.tar, *.7zip)|*.rar;*.zip;*.tar;*.7zip" +
                "|Комиксы (*.cbr, *.cbz, *.cbt, *.cb7)|*.cbr;*.cbz;*.cbt;*.cb7" +
                "|Изображения (*.jpg, *.jpeg, *.bmp, *.png, *.gif, *.tif, *.tiff)" +
                "|*.jpg;*.jpeg;*.bmp;*.png;*.gif;*.tif;*.tiff",
            };

            if (ofd.ShowDialog() == true)
            {
                this.ComicBook.Pages.Clear();

                foreach (string path in ofd.FileNames)
                {
                    string fileExt = path.Substring(path.LastIndexOf('.'));

                    if (ComicBook.AviableArchiveFormats.Contains(fileExt))
                    {
                        this.ComicBook.UnpackAllPages(path);
                    }
                    else if (ComicBook.AviableComicFormats.Contains(fileExt))
                    {
                        this.ComicBook.UnpackAllPages(path);
                    }
                    else if (ComicBook.AviableImageFormats.Contains(fileExt))
                    {
                        BitmapImage page = new BitmapImage(new Uri(path));
                        this.ComicBook.Pages.Add(page);
                    }
                }

                this.image.Source = this.ComicBook.Pages.FirstOrDefault();
                this.ComicBook.CurrentPage = 0;
            }
        }

        private void FirstButton_Click(object sender, RoutedEventArgs e)
        {
            this.FirstPage();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.BackPage();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            this.NextPage();
        }

        private void LastButton_Click(object sender, RoutedEventArgs e)
        {
            this.LastPage();
        }

        private void FirstPage()
        {
            if (this.ComicBook.Pages.Count != 0)
                this.ComicBook.CurrentPage = 0;
        }

        private void BackPage()
        {
            if (this.ComicBook.Pages.Count != 0)
                this.ComicBook.CurrentPage--;
        }

        private void NextPage()
        {
            if (this.ComicBook.Pages.Count != 0)
                this.ComicBook.CurrentPage++;
        }
        private void LastPage()
        {
            if (this.ComicBook.Pages.Count != 0)
                this.ComicBook.CurrentPage = this.ComicBook.Pages.Count - 1;
        }
    }
}
