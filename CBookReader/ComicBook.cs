using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using SharpCompress.Readers;
using SharpCompress.Archives.Rar;
using System.Windows.Controls;

namespace CBookReader
{
    sealed class ComicBook : Book
    {
        public ComicBook() : base() { }

        public ComicBook(List<BitmapSource> pages) : base(pages) { }

        public override List<string> AviableImageFormats { get; } = new List<string>
        {
            ".jpg",
            ".jpeg",
            ".bmp",
            ".png",
            ".gif",
            ".tif",
            ".tiff"
        };

        public override List<string> AviableArchiveFormats { get; } = new List<string>
        {
            ".rar",
            ".zip",
            ".tar",
            ".7zip"
        };

        public override List<string> AviableComicFormats { get; } = new List<string>
        {
            ".cbr",
            ".cbz",
            ".cbt",
            ".cb7"
        };
    }
}
