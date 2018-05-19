using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBookReader
{
    class BrightContrast
    {
        public double Brightness { get; set; }
        public double Contrast { get; set; }

        public BrightContrast()
        {
            this.Brightness = 0;
            this.Contrast = 0;
        }

        public BrightContrast(double brightness, double contrast)
        {
            this.Brightness = brightness;
            this.Contrast = contrast;
        }
    }
}
