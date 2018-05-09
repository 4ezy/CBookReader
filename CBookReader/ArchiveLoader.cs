using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CBookReader
{
    class ArchiveLoader : IFileLoader
    {
        public object Load(string path, ref List<BitmapSource> pages)
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
                                pages.Add(page);
                            }
                        }
                    }
                }
            }
        }
    }
}
