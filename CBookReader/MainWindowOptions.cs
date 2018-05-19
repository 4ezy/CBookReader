using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CBookReader
{
    [Serializable]
    public class MainWindowOptions
    {
        public bool ToWidthIfLarge { get; set; }
        public bool ToHeightIfLarge { get; set; }
        public bool ToWidthIfSmall { get; set; }
        public bool ToHeigthIfSmall { get; set; }

        public bool Fullscreen { get; set; }
        public bool MenuVisibile { get; set; }
        public bool ArrowsVisible { get; set; }
        public bool ToolbarVisible { get; set; }

        public bool VertScroll { get; set; }
        public bool HorzScroll { get; set; }

        public MainWindowOptions() { }

        public MainWindowOptions(bool toWidthIfLarge, bool toHeightIfLarge,
            bool toWidthIfSmall, bool toHeigthIfSmall, bool fullscreen,
            bool menuVisibile, bool arrowsVisible, bool toolbarVisible,
            bool vertScroll, bool horzScroll)
        {
            this.ToWidthIfLarge = toWidthIfLarge;
            this.ToHeightIfLarge = toHeightIfLarge;
            this.ToWidthIfSmall = toWidthIfSmall;
            this.ToHeigthIfSmall = toHeigthIfSmall;
            this.Fullscreen = fullscreen;
            this.MenuVisibile = menuVisibile;
            this.ToolbarVisible = toolbarVisible;
            this.ArrowsVisible = arrowsVisible;
            this.VertScroll = vertScroll;
            this.HorzScroll = horzScroll;
        }

        public void Serialize(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(MainWindowOptions));

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                xmlSerializer.Serialize(fs, this);
            }
        }

        public void Deserialize(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(MainWindowOptions));

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                MainWindowOptions options = xmlSerializer.Deserialize(fs) as MainWindowOptions;

                this.ToWidthIfLarge = options.ToWidthIfLarge;
                this.ToHeightIfLarge = options.ToHeightIfLarge;
                this.ToWidthIfSmall = options.ToWidthIfSmall;
                this.ToHeigthIfSmall = options.ToHeigthIfSmall;
                this.Fullscreen = options.Fullscreen;
                this.MenuVisibile = options.MenuVisibile;
                this.ToolbarVisible = options.ToolbarVisible;
                this.ArrowsVisible = options.ArrowsVisible;
                this.VertScroll = options.VertScroll;
                this.HorzScroll = options.HorzScroll;
            }
        }
    }
}
