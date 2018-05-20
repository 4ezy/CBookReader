using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CBookReader
{
    interface IBitmapSourceLoader
    {
        event Action<int> UploadedFilesCountChanged;
        event Action<int> UploadedFilesNumberChanged;

        List<BitmapSource> Load(List<string> pathes,
            List<string> aviableFormats);
    }
}
