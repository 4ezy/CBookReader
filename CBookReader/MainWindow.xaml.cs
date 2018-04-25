﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
                if (this.ComicBook.Pages.Count > 0)
                    this.image.Source = this.ComicBook.Pages[this.ComicBook.CurrentPage];

                if (this.ComicBook.CurrentPage == 0)
                {
                    this.firstButton.IsEnabled = false;
                    this.backButton.IsEnabled = false;
                    this.backPoly.Visibility = Visibility.Collapsed;
                    this.backRect.Visibility = Visibility.Collapsed;
                }
                else if (this.ComicBook.CurrentPage > 0)
                {
                    this.firstButton.IsEnabled = true;
                    this.backButton.IsEnabled = true;
                    this.backPoly.Visibility = Visibility.Visible;
                    this.backRect.Visibility = Visibility.Visible;
                }

                if (this.ComicBook.CurrentPage == this.ComicBook.Pages.Count - 1)
                {
                    this.lastButton.IsEnabled = false;
                    this.nextButton.IsEnabled = false;
                    this.nextPoly.Visibility = Visibility.Collapsed;
                    this.nextRect.Visibility = Visibility.Collapsed;
                }
                else if (this.ComicBook.CurrentPage < this.ComicBook.Pages.Count - 1)
                {
                    this.lastButton.IsEnabled = true;
                    this.nextButton.IsEnabled = true;
                    this.nextPoly.Visibility = Visibility.Visible;
                    this.nextRect.Visibility = Visibility.Visible;
                }
            });
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

        private void BackRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.BackPage();
        }

        private void NextRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.NextPage();
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                Title = "Открыть",
                Multiselect = true,
                Filter = "Все поддерживаемые (*.*)|" +
                "*.rar;*.zip;*.tar;*.7zip;" +
                "*.cbr;*.cbz;*.cbt;*.cb7;" +
                "*.jpg;*.jpeg;*.bmp;" +
                "*.png;*.gif;*.tif;*.tiff|" +
                "Архивы (*.rar,*.zip,*.tar,*.7zip)|*.rar;*.zip;*.tar;*.7zip|" +
                "Комиксы (*.cbr,*.cbz,*.cbt,*.cb7)|*.cbr;*.cbz;*.cbt;*.cb7|" +
                "Изображения (*.jpg,*.jpeg,*.bmp,*.png,*.gif,*.tif,*.tiff)|" +
                "*.jpg;*.jpeg;*.bmp;*.png;*.gif;*.tif;*.tiff",
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

                if (this.ComicBook.Pages.Count != 0)
                {
                    this.image.Source = this.ComicBook.Pages.First();
                    this.ComicBook.CurrentPage = 0;
                    this.saveMenuItem.IsEnabled = true;
                    this.saveAllMenuItem.IsEnabled = true;
                    this.closeMenuItem.IsEnabled = true;
                }
            }
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Title = "Сохранить",
                Filter = "JPEG (*.jpg,*.jpeg)|*.jpg;*.jpeg|" +
                "PNG (*.png)|*.png|" +
                "BMP (*.bmp)|*.bmp|" +
                "GIF (*.gif)|*.gif|" +
                "TIFF (*.tif,*.tiff)|*.tif;*.tiff",
                FileName = "Безымянный"
            };

            if (sfd.ShowDialog() == true)
            {
                BitmapEncoder encoder = null;
                switch (sfd.FilterIndex)
                {
                    case 1:
                        encoder = new JpegBitmapEncoder();
                        break;
                    case 2:
                        encoder = new PngBitmapEncoder();
                        break;
                    case 3:
                        encoder = new BmpBitmapEncoder();
                        break;
                    case 4:
                        encoder = new GifBitmapEncoder();
                        break;
                    case 5:
                        encoder = new TiffBitmapEncoder();
                        break;
                    default:
                        break;
                }

                if (encoder != null)
                {
                    encoder.Frames.Add(
                        BitmapFrame.Create(
                            this.ComicBook.Pages[this.ComicBook.CurrentPage]));

                    using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create))
                        encoder.Save(fs);

                    MessageBox.Show("Файл сохранён", "Успех");
                }
            }
        }

        private void AllAsJpegMenuItem_Click(object sender, RoutedEventArgs e) =>
            SaveAll(ImageFormat.JPEG);

        private void AllAsPngMenuItem_Click(object sender, RoutedEventArgs e) =>
            SaveAll(ImageFormat.PNG);

        private void AllAsBmpMenuItem_Click(object sender, RoutedEventArgs e) =>
            SaveAll(ImageFormat.BMP);

        private void AllAsGifMenuItem_Click(object sender, RoutedEventArgs e) =>
            SaveAll(ImageFormat.GIF);

        private void AllAsTiffMenuItem_Click(object sender, RoutedEventArgs e) =>
            SaveAll(ImageFormat.TIFF);

        private void SaveAll(ImageFormat format)
        {
            System.Windows.Forms.FolderBrowserDialog fbd =
                new System.Windows.Forms.FolderBrowserDialog()
                {
                    Description = "Сохранить все",
                    ShowNewFolderButton = true
                };

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string ext = string.Empty;

                for (int i = 0; i < this.ComicBook.Pages.Count; i++)
                {
                    BitmapEncoder encoder = null;

                    switch (format)
                    {
                        case ImageFormat.JPEG:
                            encoder = new JpegBitmapEncoder();
                            ext = ".jpg";
                            break;
                        case ImageFormat.PNG:
                            encoder = new PngBitmapEncoder();
                            ext = ".png";
                            break;
                        case ImageFormat.BMP:
                            encoder = new BmpBitmapEncoder();
                            ext = ".bmp";
                            break;
                        case ImageFormat.GIF:
                            encoder = new GifBitmapEncoder();
                            ext = ".gif";
                            break;
                        case ImageFormat.TIFF:
                            encoder = new TiffBitmapEncoder();
                            ext = ".tif";
                            break;
                        default:
                            break;
                    }

                    encoder.Frames.Add(BitmapFrame.Create(this.ComicBook.Pages[i]));

                    using (FileStream fs = new FileStream(
                        fbd.SelectedPath + "\\" + i + ext, FileMode.Create))
                        encoder.Save(fs);
                }

                MessageBox.Show("Все файлы сохранены в указанную папку", "Успех");
            }
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.saveMenuItem.IsEnabled = false;
            this.saveAllMenuItem.IsEnabled = false;
            this.closeMenuItem.IsEnabled = false;
            this.ComicBook.Pages.Clear();
            this.ComicBook.CurrentPage = -1;
        }

        private void FullscreenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item.IsChecked)
                this.WindowStyle = WindowStyle.None;
            else
                this.WindowStyle = WindowStyle.SingleBorderWindow;
        }

        private void MaximizeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        private void MinimizeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    enum ImageFormat
    {
        JPEG,
        PNG,
        BMP,
        GIF,
        TIFF
    }
}
