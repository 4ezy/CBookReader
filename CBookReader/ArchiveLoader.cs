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
    class ArchiveLoader : IBitmapSourceLoader
    {
        public event Action<int> UploadedFilesCountChanged;
        public event Action<int> UploadedFilesNumberChanged;

        public List<BitmapSource> Load(List<string> pathes, 
            List<string> aviableFormats)
        {
            int entriesCount = 0;

            foreach (var path in pathes)
            {
                using (Stream stream = File.OpenRead(path))
                using (var reader = ReaderFactory.Open(stream))
                {
                    while (reader.MoveToNextEntry())
                    {
                        entriesCount++;
                    }
                }
            }

            UploadedFilesCountChanged?.Invoke(entriesCount);

            int i = 0;
            List<BitmapSource> pages = new List<BitmapSource>();

            foreach (var path in pathes)
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
                                    page.Freeze();
                                    pages.Add(page);
                                }
                            }
                        }

                        UploadedFilesNumberChanged?.Invoke(++i);
                    }
                }
            }

            return pages;
        }
    }
}
