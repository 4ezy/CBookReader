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
        public List<BitmapSource> Pages { get; set; }
        public static List<string> AviableArchiveFormats { get; } = new List<string>
        {
            ".rar",
            ".zip",
            ".tar",
            ".7zip"
        };

        public static List<string> AviableComicFormats { get; } = new List<string>
        {
            ".cbr",
            ".cbz",
            ".cbt",
            ".cb7"
        };

        public static List<string> AviableImageFormats { get; } = new List<string>
        {
            ".jpg",
            ".jpeg",
            ".bmp",
            ".png",
            ".gif",
            ".tif",
            ".tiff"
        };

        public event Action CurrentPageChanged;
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

        public ComicBook()
        {
            this.Pages = new List<BitmapSource>();
            this.currentPage = -1;
        }

        public ComicBook(List<BitmapSource> pages) : this() =>
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

        public bool FirstPage()
        {
            bool isOperationExecuted = false;

            if (this.Pages.Count != 0)
            {
                this.CurrentPage = 0;
                isOperationExecuted = true;
            }

            return isOperationExecuted;
        }

        public bool BackPage()
        {
            bool isOperationExecuted = false;

            if (this.Pages.Count != 0)
            {
                this.CurrentPage--;
                isOperationExecuted = true;
            }

            return isOperationExecuted;
        }

        public bool NextPage()
        {
            bool isOperationExecuted = false;

            if (this.Pages.Count != 0)
            {
                this.CurrentPage++;
                isOperationExecuted = true;
            }

            return isOperationExecuted;
        }

        public bool LastPage()
        {
            bool isOperationExecuted = false;

            if (this.Pages.Count != 0)
            {
                this.CurrentPage = this.Pages.Count - 1;
                isOperationExecuted = true;
            }

            return isOperationExecuted;
        }
    }
}
