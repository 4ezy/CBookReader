using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace CBookReader
{
    class BrightContrastEffect : ShaderEffect
    {
        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty(
                "Input", typeof(BrightContrastEffect), 0);

        public static readonly DependencyProperty BrightnessProperty =
            DependencyProperty.Register(
                "Brightness",
                typeof(double),
                typeof(BrightContrastEffect),
                new UIPropertyMetadata(
                    0.0, PixelShaderConstantCallback(0)));

        public static readonly DependencyProperty ContrastProperty =
            DependencyProperty.Register(
                "Contrast",
                typeof(double),
                typeof(BrightContrastEffect),
                new UIPropertyMetadata(
                    0.0, PixelShaderConstantCallback(1)));

        private static PixelShader m_shader = new PixelShader()
        {
            UriSource = new Uri(@"pack://application:,,,/CBookReader;component/Resources/Shaders/bcEffect.ps")
        };

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public float Brightness
        {
            get { return (float)GetValue(BrightnessProperty); }
            set { SetValue(BrightnessProperty, value); }
        }

        public float Contrast
        {
            get { return (float)GetValue(ContrastProperty); }
            set { SetValue(ContrastProperty, value); }
        }

        public BrightContrastEffect()
        {
            PixelShader = m_shader;
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(BrightnessProperty);
            UpdateShaderValue(ContrastProperty);
        }
    }
}
