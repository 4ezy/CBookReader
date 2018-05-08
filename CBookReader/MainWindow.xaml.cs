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
using Forms = System.Windows.Forms;
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
        private Point GrabPoint { get; set; }
        private Point UngrabPoint { get; set; }
        private double VertScrollOffset { get; set; }
        private double HorzScrollOffset { get; set; }
        private bool IsImageGrabbing { get; set; }
        private bool IsScaled { get; set; }
        private double scaleX;
        private double scaleY;
        private static readonly double scaleStep = 0.05;

        enum ImageFormat
        {
            JPEG,
            PNG,
            BMP,
            GIF,
            TIFF
        }

        public MainWindow()
        {
            InitializeComponent();
            this.ComicBook = new ComicBook();
            this.image.Cursor = MainWindow.LoadCursorFromResource("Resources\\Cursors\\grab.cur");
            this.ComicBook.CurrentPageChanged += (() =>
            {
                Size sz = this.GetTrueWindowSize(new Size(this.ActualWidth, this.ActualHeight));
                this.IsScaled = false;
                this.ResizeImage(sz.Width, sz.Height);

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
            bool isOk = this.ComicBook.FirstPage();

            if (isOk)
            {
                this.imageScroll.ScrollToHome();
                this.imageScroll.ScrollToLeftEnd();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            bool isOk = this.ComicBook.BackPage();

            if (isOk)
            {
                this.imageScroll.ScrollToEnd();
                this.imageScroll.ScrollToRightEnd();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            bool isOk = this.ComicBook.NextPage();

            if (isOk)
            {
                this.imageScroll.ScrollToHome();
                this.imageScroll.ScrollToLeftEnd();
            }
        }

        private void LastButton_Click(object sender, RoutedEventArgs e)
        {
            bool isOk = this.ComicBook.LastPage();

            if (isOk)
            {
                this.imageScroll.ScrollToHome();
                this.imageScroll.ScrollToLeftEnd();
            }
        }

        private void BackRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool isOk = this.ComicBook.BackPage();

            if (isOk)
            {
                this.imageScroll.ScrollToEnd();
                this.imageScroll.ScrollToRightEnd();
            }
        }

        private void NextRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool isOk = this.ComicBook.NextPage();

            if (isOk)
            {
                this.imageScroll.ScrollToHome();
                this.imageScroll.ScrollToLeftEnd();
            }
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                Title = "Открыть",
                Multiselect = true,
                FilterIndex = 3,
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
                        BitmapImage page = new BitmapImage();
                        page.BeginInit();
                        page.CacheOption = BitmapCacheOption.OnLoad;
                        page.UriSource = new Uri(path);
                        page.EndInit();
                        this.ComicBook.Pages.Add(page);
                    }
                }

                if (this.ComicBook.Pages.Count != 0)
                {
                    BitmapSource source = this.ComicBook.Pages.First();
                    this.image.Source = ImageTransformHelper.Stretch(
                        source, source.PixelWidth, source.PixelHeight,
                        out scaleX, out scaleY);
                    this.image.Width = Math.Floor((double)source.PixelWidth);
                    this.image.Height = Math.Floor((double)source.PixelHeight);
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
            {
                this.Visibility = Visibility.Collapsed;
                this.WindowState = WindowState.Maximized;
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowStyle = WindowStyle.None;
                this.Visibility = Visibility.Visible;
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
                this.ResizeMode = ResizeMode.CanResize;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.Visibility = Visibility.Visible;
            }

            Size sz = this.GetTrueWindowSize(new Size(this.ActualWidth, this.ActualHeight));
                this.ResizeImage(sz.Width, sz.Height);
        }

        private void MaximizeMenuItem_Click(object sender, RoutedEventArgs e) =>
            this.WindowState = WindowState.Maximized;

        private void MinimizeMenuItem_Click(object sender, RoutedEventArgs e) =>
            this.WindowState = WindowState.Minimized;

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e) =>
            this.Close();

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

            Size sz = this.GetTrueWindowSize(new Size(this.ActualWidth, this.ActualHeight));
            this.ResizeImage(sz.Width, sz.Height);
        }

        private void HorizontalScrollVisibleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.horizontalScrollVisibleMenuItem.IsChecked)
                this.imageScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            else
                this.imageScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

            Size sz = this.GetTrueWindowSize(new Size(this.ActualWidth, this.ActualHeight));
            this.ResizeImage(sz.Width, sz.Height);
        }

        private void ToolbarVisibleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.toolbarVisibleMenuItem.IsChecked)
                this.toolbarStackPanel.Visibility = Visibility.Visible;
            else
                this.toolbarStackPanel.Visibility = Visibility.Collapsed;

            Size sz = this.GetTrueWindowSize(new Size(this.ActualWidth, this.ActualHeight));
            this.ResizeImage(sz.Width, sz.Height);
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

            Size sz = this.GetTrueWindowSize(new Size(this.ActualWidth, this.ActualHeight));
            this.ResizeImage(sz.Width, sz.Height);
        }

        private void ResizeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (this.strWidthLargeMenuItem.IsChecked && this.strHeightLargeMenuItem.IsChecked)
            {
                if (item.Name.Equals(this.strWidthLargeMenuItem.Name))
                    this.strHeightLargeMenuItem.IsChecked = false;
                else
                    this.strWidthLargeMenuItem.IsChecked = false;
            }

            if (this.strWidthSmallMenuItem.IsChecked && this.strHeihtSmallMenuItem.IsChecked)
            {
                if (item.Name.Equals(this.strWidthSmallMenuItem.Name))
                    this.strHeihtSmallMenuItem.IsChecked = false;
                else
                    this.strWidthSmallMenuItem.IsChecked = false;
            }

            Size sz = this.GetTrueWindowSize(new Size(this.ActualWidth, this.ActualHeight));
            this.ResizeImage(sz.Width, sz.Height);
        }

        private void ResizeImage(double windowWidth, double windowHeight)
        {
            if (this.ComicBook.Pages.Count > 0)
            {
                if (this.image.Source == null)
                    return;

                BitmapSource page = this.ComicBook.Pages[this.ComicBook.CurrentPage];
                BitmapSource source = this.image.Source as BitmapSource;
                bool isSizeChanged = false;

                Size imageControlSize = this.GetImageControlSize(windowWidth, windowHeight);

                if ((this.strWidthLargeMenuItem.IsChecked && page.PixelWidth > imageControlSize.Width) ||
                    this.strWidthSmallMenuItem.IsChecked && page.PixelWidth < imageControlSize.Width) 
                {
                    BitmapSource src = ImageTransformHelper.StretchToWidth(
                        page, imageControlSize.Width, out scaleX, out scaleY);
                    this.image.Source = src;
                    this.image.Width = Math.Floor(src.Width);
                    this.image.Height = Math.Floor(src.Height);
                    isSizeChanged = true;
                    IsScaled = false;
                }

                if ((this.strHeightLargeMenuItem.IsChecked && page.PixelHeight > imageControlSize.Height) ||
                    this.strHeihtSmallMenuItem.IsChecked && page.PixelHeight < imageControlSize.Height)
                {
                    BitmapSource src = ImageTransformHelper.StretchToHeight(
                        page, imageControlSize.Height, out scaleX, out scaleY);
                    this.image.Source = src;
                    this.image.Width = Math.Floor(src.Width);
                    this.image.Height = Math.Floor(src.Height);
                    isSizeChanged = true;
                    IsScaled = false;
                }

                if (!isSizeChanged && this.image.Source != page && !IsScaled)
                {
                    BitmapSource src = ImageTransformHelper.Stretch(
                        page, page.PixelWidth, page.PixelHeight, out scaleX, out scaleY);
                    this.image.Source = src;

                    this.image.Width = Math.Floor(src.Width);
                    this.image.Height = Math.Floor(src.Height);
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Size sz = this.GetTrueWindowSize(e.NewSize);

            this.ResizeImage(sz.Width, sz.Height);
        }

        private Size GetTrueWindowSize(Size size)
        {
            Size sz = size;
            
            if (this.WindowStyle == WindowStyle.SingleBorderWindow)
            {
                sz.Width -= 16;
                sz.Height -= 16;
            }

            return sz;
        }

        private Size GetImageControlSize(double windowWidth, double windowHeight)
        {
            Size sz = new Size(windowWidth, windowHeight);

            if (this.imageScroll.VerticalScrollBarVisibility == ScrollBarVisibility.Visible)
                sz.Width -= SystemParameters.VerticalScrollBarWidth;

            if (this.imageScroll.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible)
                sz.Height -= SystemParameters.HorizontalScrollBarHeight;

            if (this.WindowStyle == WindowStyle.SingleBorderWindow)
                sz.Height -= SystemParameters.WindowCaptionHeight;

            if (this.menu.Visibility == Visibility.Visible)
                sz.Height -= this.menu.Height;

            if (this.toolbarStackPanel.Visibility == Visibility.Visible)
                sz.Height -= this.toolbarStackPanel.Height;

            return sz;
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool handle = (Keyboard.Modifiers & ModifierKeys.Control) > 0;

            if (!handle)
                return;

            if (scaleX >= scaleStep * 2 && scaleY >= scaleStep * 2)
            {
                if (e.Delta > 0)
                {
                    this.scaleX += scaleStep;
                    this.scaleY += scaleStep;
                }
                else
                {
                    this.scaleX -= scaleStep;
                    this.scaleY -= scaleStep;
                }
            }
            else
            {
                if (e.Delta > 0)
                {
                    this.scaleX += scaleStep;
                    this.scaleY += scaleStep;
                }
            }

            BitmapSource src = ImageTransformHelper.Scale(
                this.ComicBook.Pages[this.ComicBook.CurrentPage],
                this.scaleX, this.scaleY);
            this.image.Source = src;
            this.image.Width = Math.Floor(src.Width);
            this.image.Height = Math.Floor(src.Height);
            this.IsScaled = true;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) =>
            this.imageScroll.IsEnabled = false;

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e) =>
            this.imageScroll.IsEnabled = true;

        private static Cursor LoadCursorFromResource(string resourceName)
        {
            CursorConverter cc = new CursorConverter();
            Cursor cursor = cc.ConvertFrom(resourceName) as Cursor;
            return cursor;
        }

        private void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.image.Cursor = MainWindow.LoadCursorFromResource("Resources\\Cursors\\grabbing.cur");
            this.HorzScrollOffset = this.imageScroll.HorizontalOffset;
            this.VertScrollOffset = this.imageScroll.VerticalOffset;
            Point pt = Mouse.GetPosition(this.imageScroll);
            pt.X += this.HorzScrollOffset;
            pt.Y += this.VertScrollOffset;
            this.GrabPoint = pt;
            this.IsImageGrabbing = true;
        }

        private void Image_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.UngrabImage();
        }

        private void Image_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (IsImageGrabbing)
            {
                Point newPoint = Mouse.GetPosition(this.imageScroll);
                newPoint.X += this.HorzScrollOffset;
                newPoint.Y += this.VertScrollOffset;

                double horzOffset = this.GrabPoint.X - newPoint.X;
                double vertOffset = this.GrabPoint.Y - newPoint.Y;

                this.imageScroll.ScrollToVerticalOffset(
                        this.VertScrollOffset + vertOffset);

                this.imageScroll.ScrollToHorizontalOffset(
                        this.HorzScrollOffset + horzOffset);
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            this.UngrabImage();
        }

        private void UngrabImage()
        {
            this.image.Cursor = MainWindow.LoadCursorFromResource("Resources\\Cursors\\grab.cur");
            this.IsImageGrabbing = false;
        }
    }
}
