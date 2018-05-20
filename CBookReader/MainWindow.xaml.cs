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
using Forms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace CBookReader
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Book ComicBook { get; set; }
        private Point GrabPoint { get; set; }
        private double VertScrollOffset { get; set; }
        private double HorzScrollOffset { get; set; }
        private bool IsImageGrabbing { get; set; }
        private bool IsScaled { get; set; }
        private bool SearchMode { get; set; }
        private double scaleX;
        private double scaleY;
        private static readonly double scaleStep = 0.05;
        private static readonly string optionsPath = "options.xml";

        public static readonly DependencyProperty BrightnessProperty =
            DependencyProperty.Register("Brightness", typeof(double),
                typeof(MainWindow), new UIPropertyMetadata(0d));

        public static readonly DependencyProperty ContrastProperty =
            DependencyProperty.Register("Contrast", typeof(double),
                typeof(MainWindow), new UIPropertyMetadata(0d));

        public double Brightness {
            get { return (double)this.GetValue(BrightnessProperty); }
            set { this.SetValue(BrightnessProperty, value); }
        }
        public double Contrast
        {
            get { return (double)this.GetValue(ContrastProperty); }
            set { this.SetValue(ContrastProperty, value); }
        }

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
            BookFactory bookFactory = new BookFactory();
            this.ComicBook = bookFactory.CreateBook(BookTypes.ComicBook);
            this.image.Cursor = MainWindow.LoadCursorFromResource("Resources\\Cursors\\grab.cur");
            this.pageCountLabel.Content = "/0";
            this.pageNumberTextBox.Text = "0";

            this.ComicBook.CurrentPageChanged += (() =>
            {
                this.SetCurrentBrightContrast();
                Size sz = this.GetTrueWindowSize(new Size(this.ActualWidth, this.ActualHeight));
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

        private void OpenCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCmdExecuted(object sender, ExecutedRoutedEventArgs e)
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
                this.nextPageMenuItem.IsEnabled = true;
                this.lastPageMenuItem.IsEnabled = true;
                this.ComicBook.Pages.Clear();
                this.progressBar.Visibility = Visibility.Visible;
                this.progressBar.Maximum = ofd.FileNames.Count();

                Task.Factory.StartNew(() =>
                {
                    List<string> archivesPathes = new List<string>();
                    List<string> imagePathes = new List<string>();

                    foreach (string path in ofd.FileNames)
                    {
                        string fileExt = path.Substring(path.LastIndexOf('.'));

                        if (this.ComicBook.AviableArchiveFormats.Contains(fileExt) ||
                            this.ComicBook.AviableComicFormats.Contains(fileExt))
                        {
                            archivesPathes.Add(path);
                        }
                        else if (this.ComicBook.AviableImageFormats.Contains(fileExt))
                        {
                            imagePathes.Add(path);
                        }
                    }

                    this.ComicBook.BitmapSouceLoader = new ArchiveLoader();
                    this.ComicBook.BitmapSouceLoader.UploadedFilesCountChanged += UploadedFileCountChanged;
                    this.ComicBook.BitmapSouceLoader.UploadedFilesNumberChanged += UploadedFileNumberChanged;
                    this.ComicBook.Load(archivesPathes);

                    this.ComicBook.BitmapSouceLoader = new ImageLoader();
                    this.ComicBook.BitmapSouceLoader.UploadedFilesCountChanged += UploadedFileCountChanged;
                    this.ComicBook.BitmapSouceLoader.UploadedFilesNumberChanged += UploadedFileNumberChanged;
                    this.ComicBook.Load(imagePathes);

                    BitmapSource source = this.ComicBook.Pages.First();
                    int width = source.PixelWidth;
                    int height = source.PixelHeight;

                    this.Dispatcher.Invoke(() =>
                    {
                        if (this.ComicBook.Pages.Count != 0)
                        {
                            this.progressBar.Visibility = Visibility.Collapsed;

                            foreach (BitmapSource page in this.ComicBook.Pages)
                            {
                                this.ComicBook.PagesBrightContrast.Add(new BrightContrast());
                            }

                            this.image.Source = ImageTransformHelper.Stretch(
                                source, width, height,
                                out scaleX, out scaleY);

                            this.image.Width = Math.Floor((double)width);
                            this.image.Height = Math.Floor((double)Height);
                            this.saveMenuItem.IsEnabled = true;
                            this.saveAllMenuItem.IsEnabled = true;
                            this.closeMenuItem.IsEnabled = true;
                            this.pageCountLabel.Content = "/" + this.ComicBook.Pages.Count.ToString();
                            this.pageNumberTextBox.Text = "1";
                            this.ComicBook.CurrentPage = 0;
                        }
                    });
                });
            }
        }

        private void SaveCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (saveMenuItem.IsEnabled)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void SaveCmdExecuted(object sender, ExecutedRoutedEventArgs e)
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

        private void FullscreenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.fullscreenMenuItem.IsChecked)
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

                bool isSizeChanged = false;
                BitmapSource page = this.ComicBook.Pages[this.ComicBook.CurrentPage];
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

                if (!isSizeChanged && this.image.Source != page)
                {
                    BitmapSource src = ImageTransformHelper.Stretch(
                        page, page.PixelWidth, page.PixelHeight, out double scX, out double scY);

                    if (IsScaled)
                        src = ImageTransformHelper.Scale(src, scaleX, scaleY);

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

            this.Zoom(e.Delta);
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

        private void PageNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(IsGood);
        }

        private void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            var stringData = (string)e.DataObject.GetData(typeof(string));
            if (stringData == null || !stringData.All(IsGood))
                e.CancelCommand();
        }

        bool IsGood(char c)
        {
            if (c >= '0' && c <= '9')
                return true;
            return false;
        }

        private void PageNumberTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    bool isOk = this.ComicBook.GoToPage(
                        Convert.ToInt32(this.pageNumberTextBox.Text));

                    if (isOk)
                    {
                        this.imageScroll.ScrollToHome();
                        this.imageScroll.ScrollToLeftEnd();
                    }
                }
            }
            catch (Exception) { }
        }

        private void PageNumberTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Clear();
        }

        private void PageNumberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (this.ComicBook.Pages.Count == 0)
            {
                this.pageNumberTextBox.Text = "0";
            }
            else if (textBox.Text == string.Empty || textBox.Text != (this.ComicBook.CurrentPage + 1).ToString())
            {
                this.pageNumberTextBox.Text = (this.ComicBook.CurrentPage + 1).ToString();
            }

            if (SearchMode)
            {
                this.toolbarStackPanel.Visibility = Visibility.Collapsed;
                SearchMode = false;
            }
        }

        private void RotateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.image.Source != null)
            {
                this.ComicBook.Pages[this.ComicBook.CurrentPage] =
                    ImageTransformHelper.Rotate(
                        this.ComicBook.Pages[this.ComicBook.CurrentPage], 90);

                BitmapSource src = this.ComicBook.Pages[this.ComicBook.CurrentPage];
                src = ImageTransformHelper.Stretch(
                        src, src.PixelWidth, src.PixelHeight, out double scX, out double scY);

                if (IsScaled)
                    src = ImageTransformHelper.Scale(src, scaleX, scaleY);

                this.image.Source = src;
                Size sz = this.GetTrueWindowSize(new Size(this.ActualWidth, this.ActualHeight));
                this.ResizeImage(sz.Width, sz.Height);
            }
        }

        private void CloseCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.closeMenuItem.IsEnabled)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void CloseCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.saveMenuItem.IsEnabled = false;
            this.saveAllMenuItem.IsEnabled = false;
            this.closeMenuItem.IsEnabled = false;
            this.ComicBook.Pages.Clear();
            this.ComicBook.PagesBrightContrast.Clear();
            this.ComicBook.CurrentPage = -1;
            this.image.Source = null;
            this.pageNumberTextBox.Text = "0";
            this.pageCountLabel.Content = "/0";
        }

        private void BackRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.PreviousPageCmdExecuted(null, null);
        }

        private void NextRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.NextPageCmdExecuted(null, null);
        }

        private void NextPageCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.nextPageMenuItem.IsEnabled)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void NextPageCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            bool isOk = this.ComicBook.NextPage();

            if (isOk)
            {
                this.imageScroll.ScrollToHome();
                this.imageScroll.ScrollToLeftEnd();
                this.pageNumberTextBox.Text = (this.ComicBook.CurrentPage + 1).ToString();
            }
        }

        private void PreviousPageCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.backPageMenuItem.IsEnabled)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void PreviousPageCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            bool isOk = this.ComicBook.PreviousPage();

            if (isOk)
            {
                this.imageScroll.ScrollToEnd();
                this.imageScroll.ScrollToRightEnd();
                this.pageNumberTextBox.Text = (this.ComicBook.CurrentPage + 1).ToString();
            }
        }

        private void FirstPageCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.firstPageMenuItem.IsEnabled)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void FirstPageCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            bool isOk = this.ComicBook.FirstPage();

            if (isOk)
            {
                this.imageScroll.ScrollToHome();
                this.imageScroll.ScrollToLeftEnd();
                this.pageNumberTextBox.Text = (this.ComicBook.CurrentPage + 1).ToString();
            }
        }

        private void LastPageCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.lastPageMenuItem.IsEnabled)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void LastPageCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            bool isOk = this.ComicBook.LastPage();

            if (isOk)
            {
                this.imageScroll.ScrollToHome();
                this.imageScroll.ScrollToLeftEnd();
                this.pageNumberTextBox.Text = (this.ComicBook.CurrentPage + 1).ToString();
            }
        }

        private void NextPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool isOk = this.ComicBook.NextPage();

            if (isOk)
            {
                this.imageScroll.ScrollToHome();
                this.imageScroll.ScrollToLeftEnd();
                this.pageNumberTextBox.Text = (this.ComicBook.CurrentPage + 1).ToString();
            }
        }

        private void BackPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool isOk = this.ComicBook.PreviousPage();

            if (isOk)
            {
                this.imageScroll.ScrollToEnd();
                this.imageScroll.ScrollToRightEnd();
                this.pageNumberTextBox.Text = (this.ComicBook.CurrentPage + 1).ToString();
            }
        }

        private void FirstPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool isOk = this.ComicBook.FirstPage();

            if (isOk)
            {
                this.imageScroll.ScrollToHome();
                this.imageScroll.ScrollToLeftEnd();
                this.pageNumberTextBox.Text = (this.ComicBook.CurrentPage + 1).ToString();
            }
        }

        private void LastPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool isOk = this.ComicBook.LastPage();

            if (isOk)
            {
                this.imageScroll.ScrollToHome();
                this.imageScroll.ScrollToLeftEnd();
                this.pageNumberTextBox.Text = (this.ComicBook.CurrentPage + 1).ToString();
            }
        }

        private void RotateCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.rotateMenuItem.IsEnabled)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void RotateCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.RotateMenuItem_Click(null, null);
        }

        private void GoToPageCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void GoToPageCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.toolbarStackPanel.Visibility != Visibility.Visible)
            {
                this.toolbarStackPanel.Visibility = Visibility.Visible;
                this.pageNumberTextBox.Focus();
                SearchMode = true;
            }
            else
                this.pageNumberTextBox.Focus();
        }

        private void ScrollPageDownCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ScrollPageDownExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.imageScroll.ScrollToVerticalOffset(
                this.imageScroll.VerticalOffset + this.image.Height / 4);
        }

        private void ScrollPageUpCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ScrollPageUpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.imageScroll.ScrollToVerticalOffset(
                this.imageScroll.VerticalOffset - this.image.Height / 4);
        }

        private void ScrollPageLeftCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ScrollPageLeftExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.imageScroll.ScrollToHorizontalOffset(
                this.imageScroll.HorizontalOffset - this.image.Width / 4);
        }

        private void ScrollPageRightCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ScrollPageRightExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.imageScroll.ScrollToHorizontalOffset(
                this.imageScroll.HorizontalOffset + this.image.Width / 4);
        }

        private void ImageProcessingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            BrightContrast brightContrast = this.ComicBook.PagesBrightContrast[this.ComicBook.CurrentPage];
            ImageProcessingWindow imageProcWindow = new ImageProcessingWindow(
                brightContrast.Brightness, brightContrast.Contrast);

            imageProcWindow.BrightnessChanged += ((b) =>
            {
                this.Brightness = b;
                brightContrast.Brightness = b;
            });

            imageProcWindow.ContrastChanged += ((c) =>
            {
                this.Contrast = c;
                brightContrast.Contrast = c;
            });

            imageProcWindow.ProcessingForAllPagesChanged += (() =>
            {
                for (int i = 0; i < this.ComicBook.PagesBrightContrast.Count; i++)
                {
                    this.ComicBook.PagesBrightContrast[i].Brightness = this.Brightness;
                    this.ComicBook.PagesBrightContrast[i].Contrast = this.Contrast;
                }
            });

            imageProcWindow.Show();
        }

        private void SetCurrentBrightContrast()
        {
            if (this.ComicBook.CurrentPage > 0 && 
                this.ComicBook.CurrentPage < this.ComicBook.Pages.Count)
            {
                this.Brightness = this.ComicBook.PagesBrightContrast[this.ComicBook.CurrentPage].Brightness;
                this.Contrast = this.ComicBook.PagesBrightContrast[this.ComicBook.CurrentPage].Contrast;
            }
        }

        private void IncreaseZoomCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void IncreaseZoomCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Zoom(1);
        }

        private void DecreaseZoomCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DecreaseZoomCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Zoom(-1);
        }

        private void Zoom(int delta)
        {
            if (this.image.Source == null)
                return;

            if (scaleX >= scaleStep * 2 && scaleY >= scaleStep * 2)
            {
                if (delta > 0)
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
                if (delta > 0)
                {
                    this.scaleX += scaleStep;
                    this.scaleY += scaleStep;
                }
            }

            Point pt = Mouse.GetPosition(this.imageScroll);
            pt.X += this.imageScroll.HorizontalOffset;
            pt.Y += this.imageScroll.VerticalOffset;
            BitmapSource src = ImageTransformHelper.Scale(
                this.ComicBook.Pages[this.ComicBook.CurrentPage],
                this.scaleX, this.scaleY, pt.X, pt.Y);
            this.image.Source = src;
            this.image.Width = Math.Floor(src.Width);
            this.image.Height = Math.Floor(src.Height);
            this.IsScaled = true;
        }

        private void DecreaseZoomMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Zoom(-1);
        }

        private void IncreaseZoomMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Zoom(1);
        }

        private void StopCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void StopCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(optionsPath))
            {
                MainWindowOptions options = new MainWindowOptions();
                options.Deserialize(optionsPath);

                this.strWidthLargeMenuItem.IsChecked = options.ToWidthIfLarge;
                this.strHeightLargeMenuItem.IsChecked = options.ToHeightIfLarge;
                this.strWidthSmallMenuItem.IsChecked = options.ToWidthIfSmall;
                this.strHeihtSmallMenuItem.IsChecked = options.ToHeigthIfSmall;
                this.fullscreenMenuItem.IsChecked = options.Fullscreen;
                this.FullscreenMenuItem_Click(this.fullscreenMenuItem, null);
                this.menuVisibleMenuItem.IsChecked = options.MenuVisibile;
                this.MenuVisibleMenuItem_Click(this.menuVisibleMenuItem, null);
                this.arrowsVisibleMenuItem.IsChecked = options.ArrowsVisible;
                this.ArrowsVisibleMenuItem_Click(this.arrowsVisibleMenuItem, null);
                this.toolbarVisibleMenuItem.IsChecked = options.ToolbarVisible;
                this.ToolbarVisibleMenuItem_Click(this.toolbarVisibleMenuItem, null);
                this.verticalScrollVisibleMenuItem.IsChecked = options.VertScroll;
                this.VerticalScrollVisibleMenuItem_Click(this.verticalScrollVisibleMenuItem, null);
                this.horizontalScrollVisibleMenuItem.IsChecked = options.HorzScroll;
                this.HorizontalScrollVisibleMenuItem_Click(this.horizontalScrollVisibleMenuItem, null);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindowOptions options = new MainWindowOptions(
                this.strWidthLargeMenuItem.IsChecked,
                this.strHeightLargeMenuItem.IsChecked,
                this.strWidthSmallMenuItem.IsChecked,
                this.strHeihtSmallMenuItem.IsChecked,
                this.fullscreenMenuItem.IsChecked,
                this.menuVisibleMenuItem.IsChecked,
                this.arrowsVisibleMenuItem.IsChecked,
                this.toolbarVisibleMenuItem.IsChecked,
                this.verticalScrollVisibleMenuItem.IsChecked,
                this.horizontalScrollVisibleMenuItem.IsChecked);
            options.Serialize(optionsPath);
        }

        private void UploadedFileCountChanged(int i)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.progressBar.Visibility = Visibility.Visible;
                this.progressBar.Minimum = 0;
                this.progressBar.Maximum = i;
                this.progressBar.Value = 0;
            });
        }

        private void UploadedFileNumberChanged(int i)
        {
            this.Dispatcher.Invoke(() => this.progressBar.Value = i);
        }
    }
}
