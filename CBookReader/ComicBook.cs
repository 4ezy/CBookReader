using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using SharpCompress.Readers;
using SharpCompress.Archives.Rar;

namespace CBookReader
{
    internal sealed class ComicBook
    {
        public List<BitmapImage> Pages { get; set; }
        public event Action CurrentPageChanged;
        public static List<string> AviableArchiveFormats => aviableArchiveFormats;
        public static List<string> AviableComicFormats => aviableComicFormats;
        public static List<string> AviableImageFormats => aviableImageFormats;
        private int currentPage;

        public int CurrentPage
        {
            set
            {
                this.currentPage = value;
                CurrentPageChanged();
            }

            get => this.currentPage;
        }

        private static readonly List<string> aviableArchiveFormats = new List<string>
        {
            ".rar",
            ".zip",
            ".tar",
            ".7zip"
        };

        private static readonly List<string> aviableComicFormats = new List<string>
        {
            ".cbr",
            ".cbz",
            ".cbt",
            ".cb7"
        };

        private static readonly List<string> aviableImageFormats = new List<string>
        {
            ".jpg",
            ".jpeg",
            ".bmp",
            ".png",
            ".gif",
            ".tif",
            ".tiff"
        };

        public ComicBook()
        {
            this.Pages = new List<BitmapImage>();
            this.currentPage = -1;
        }

        public ComicBook(List<BitmapImage> pages) : this() =>
            this.Pages = pages ?? throw new ArgumentNullException();

        public void UnpackAllPages(string path)
        {
            using (Stream stream = File.OpenRead(path))
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        string ext = reader.Entry.Key.Substring(
                            reader.Entry.Key.LastIndexOf('.'));

                        if (ComicBook.AviableImageFormats.Contains(ext))
                        {
                            using (MemoryStream ms = new MemoryStream((int)reader.Entry.Size))
                            {
                                reader.WriteEntryTo(ms);
                                ms.Position = 0;
                                BitmapImage page = new BitmapImage();
                                page.BeginInit();
                                page.StreamSource = ms;
                                page.CacheOption = BitmapCacheOption.OnLoad;
                                page.EndInit();
                                this.Pages.Add(page);
                            }
                        }
                    }
                }
            }
        }
    }
}
