using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CBookReader
{
    class ImageLoader : IBitmapSourceLoader
    {
        public event Action<int> UploadedFilesCountChanged;
        public event Action<int> UploadedFilesNumberChanged;

        public List<BitmapSource> Load(List<string> pathes,
            List<string> aviableFormats)
        {
            try
            {
                List<BitmapSource> pages = new List<BitmapSource>();

                UploadedFilesCountChanged?.Invoke(pathes.Count);
                int i = 0;

                foreach (var path in pathes)
                {
                    string ext = path.Substring(path.LastIndexOf('.'));

                    if (aviableFormats.Contains(ext))
                    {
                        BitmapImage page = new BitmapImage();
                        page.BeginInit();
                        page.CacheOption = BitmapCacheOption.OnLoad;
                        page.UriSource = new Uri(path);
                        page.EndInit();
                        page.Freeze();
                        pages.Add(page);
                    }

                    UploadedFilesNumberChanged?.Invoke(++i);
                }

                return pages;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
