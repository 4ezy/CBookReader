using Microsoft.Win32;
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
                this.ResizeImage();

                if (this.ComicBook.CurrentPage == 0)
                {
                    this.firstPageMenuItem.IsEnabled = false;
                    this.backPageMenuItem.IsEnabled = false;
                    
                    if (this.arrowsVisibleMenuItem.IsChecked)
                        this.backRect.Visibility = Visibility.Collapsed;
                }
                else if (this.ComicBook.CurrentPage > 0)
                {
                    this.firstPageMenuItem.IsEnabled = true;
                    this.backPageMenuItem.IsEnabled = true;

                    if (this.arrowsVisibleMenuItem.IsChecked)
                        this.backRect.Visibility = Visibility.Visible;
                }

                if (this.ComicBook.CurrentPage == this.ComicBook.Pages.Count - 1)
                {
                    this.lastPageMenuItem.IsEnabled = false;
                    this.nextPageMenuItem.IsEnabled = false;
                    
                    if (this.arrowsVisibleMenuItem.IsChecked)
                        this.nextRect.Visibility = Visibility.Collapsed;
                }
                else if (this.ComicBook.CurrentPage < this.ComicBook.Pages.Count - 1)
                {
                    this.lastPageMenuItem.IsEnabled = true;
                    this.nextPageMenuItem.IsEnabled = true;

                    if (this.arrowsVisibleMenuItem.IsChecked)
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

        private void FirstButton_Click(object sender, RoutedEventArgs e) =>
            this.FirstPage();

        private void BackButton_Click(object sender, RoutedEventArgs e) =>
            this.BackPage();

        private void NextButton_Click(object sender, RoutedEventArgs e) =>
            this.NextPage();

        private void LastButton_Click(object sender, RoutedEventArgs e) =>
            this.LastPage();

        private void BackRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
            this.BackPage();

        private void NextRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
            this.NextPage();

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                Title = "Открыть",
                Multiselect = true,
                Filter =
                "Комиксы (*.cbr,*.cbz,*.cbt,*.cb7)|*.cbr;*.cbz;*.cbt;*.cb7|" +
                "Архивы (*.rar,*.zip,*.tar,*.7zip)|*.rar;*.zip;*.tar;*.7zip|" +
                "Изображения (*.jpg,*.jpeg,*.bmp,*.png,*.gif,*.tif,*.tiff)|" +
                "*.jpg;*.jpeg;*.bmp;*.png;*.gif;*.tif;*.tiff"
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
                    Description = "Выберите папку для сохранения файлов",
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
            }
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.saveMenuItem.IsEnabled = false;
            this.saveAllMenuItem.IsEnabled = false;
            this.closeMenuItem.IsEnabled = false;
            this.ComicBook.Pages.Clear();
            this.ComicBook.CurrentPage = -1;
            this.image.Source = null;
        }

        private void FullscreenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item.IsChecked)
                this.WindowStyle = WindowStyle.None;
            else
                this.WindowStyle = WindowStyle.SingleBorderWindow;
        }

        private void MaximizeMenuItem_Click(object sender, RoutedEventArgs e) =>
            this.WindowState = WindowState.Maximized;

        private void MinimizeMenuItem_Click(object sender, RoutedEventArgs e) =>
            this.WindowState = WindowState.Minimized;

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e) =>
            this.Close();

        private void NextPageMenuItem_Click(object sender, RoutedEventArgs e) =>
            this.NextPage();

        private void BackPageMenuItem_Click(object sender, RoutedEventArgs e) =>
            this.BackPage();

        private void FirstPageMenuItem_Click(object sender, RoutedEventArgs e) =>
            this.FirstPage();

        private void LastPageMenuItem_Click(object sender, RoutedEventArgs e) =>
            this.LastPage();

        private void VerticalScrollVisibleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.verticalScrollVisibleMenuItem.IsChecked)
            {
                this.imageScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                this.nextRect.Margin = new Thickness(
                    this.nextRect.Margin.Left,
                    this.nextRect.Margin.Top,
                    this.nextRect.Margin.Right + 17,
                    this.nextRect.Margin.Bottom);
                this.nextPoly.Margin = new Thickness(
                    this.nextPoly.Margin.Left,
                    this.nextPoly.Margin.Top,
                    this.nextPoly.Margin.Right + 17,
                    this.nextPoly.Margin.Bottom);
            }
            else
            {
                this.imageScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                this.nextRect.Margin = new Thickness(
                    this.nextRect.Margin.Left,
                    this.nextRect.Margin.Top,
                    this.nextRect.Margin.Right - 17,
                    this.nextRect.Margin.Bottom);
                this.nextPoly.Margin = new Thickness(
                    this.nextPoly.Margin.Left,
                    this.nextPoly.Margin.Top,
                    this.nextPoly.Margin.Right - 17,
                    this.nextPoly.Margin.Bottom);
            }
        }

        private void HorizontalScrollVisibleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.horizontalScrollVisibleMenuItem.IsChecked)
                this.imageScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            else
                this.imageScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        private void ToolbarVisibleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.toolbarVisibleMenuItem.IsChecked)
                this.toolbarStackPanel.Visibility = Visibility.Visible;
            else
                this.toolbarStackPanel.Visibility = Visibility.Collapsed;
        }

        private void ArrowsVisibleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.arrowsVisibleMenuItem.IsChecked)
            {
                if (this.ComicBook.Pages.Count - 1 == this.ComicBook.CurrentPage)
                    this.nextRect.Visibility = Visibility.Collapsed;
                else
                    this.nextRect.Visibility = Visibility.Visible;

                if (this.ComicBook.CurrentPage == 0)
                    this.backRect.Visibility = Visibility.Collapsed;
                else
                    this.backRect.Visibility = Visibility.Visible;
            }
            else
            {
                this.backRect.Visibility = Visibility.Collapsed;
                this.nextRect.Visibility = Visibility.Collapsed;
            }
        }

        private void MenuVisibleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.menuVisibleMenuItem.IsChecked)
                this.menu.Visibility = Visibility.Visible;
            else
                this.menu.Visibility = Visibility.Collapsed;
        }

        private void ResizeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.ResizeImage();
        }

        private void ResizeImage()
        {
            if (this.ComicBook.Pages.Count > 0)
            {
                BitmapImage page = this.ComicBook.Pages[this.ComicBook.CurrentPage];
                bool isSizeChanged = false;
                double windowWidth = 0;
                double windowHeight = 0;

                if (this.WindowState == WindowState.Maximized)
                {
                    windowWidth = SystemParameters.PrimaryScreenWidth;
                    windowHeight = SystemParameters.PrimaryScreenHeight;
                }
                else
                {
                    windowWidth = this.Width;
                    windowHeight = this.Height;
                }

                if ((this.strWidthLargeMenuItem.IsChecked && page.PixelWidth > windowWidth) ||
                   (this.strWidthSmallMenuItem.IsChecked && page.PixelWidth < windowWidth))
                {
                    this.image.Source = ImageTransformHelper.StretchToWidth(
                        page, windowWidth);
                    isSizeChanged = true;
                }

                if ((this.strHeightLargeMenuItem.IsChecked && page.PixelHeight > windowHeight) ||
                    (this.strHeihtSmallMenuItem.IsChecked && page.PixelHeight < windowHeight))
                {
                    this.image.Source = ImageTransformHelper.StretchToHeight(
                        page, windowHeight);
                    isSizeChanged = true;
                }

                if (!isSizeChanged && this.image.Source != page)
                    this.image.Source = this.ComicBook.Pages[this.ComicBook.CurrentPage];
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ResizeImage();
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
