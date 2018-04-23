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
        private readonly List<string> aviableFormats = new List<string>
        {
            ".jpg",
            ".jfif",
            ".jpe",
            ".jpeg",
            ".bmp",
            ".dib",
            ".rle",
            ".gif",
            ".tif",
            ".tiff"
        };

        public ComicBook() => this.Pages = new List<BitmapImage>();
        public ComicBook(List<BitmapImage> pages) => this.Pages = pages;

        public void UnpackAll(string path)
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

                        if (aviableFormats.Contains(ext))
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
