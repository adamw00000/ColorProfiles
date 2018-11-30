using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorProfiles
{
    class MainWindowViewModel: INotifyPropertyChanged
    {
        readonly ColorSpace sRGB = new SRGBColorSpace();
        readonly ColorSpace wideGamut = new WideGamutColorSpace();

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public List<ColorSpace> ColorSpaces { get; }

        public ColorSpace SourceColorSpace { get; set; }
        public ColorSpace TargetColorSpace { get; set; }

        public WriteableBitmap Image { get; set; } = new WriteableBitmap(new BitmapImage(new Uri("Images/widegamut.jpg", UriKind.Relative)));
        public WriteableBitmap ConvertedImage { get; set; }

        public ICommand ConvertCommand { get; }

        public MainWindowViewModel()
        {
            ColorSpaces = new List<ColorSpace> { sRGB, wideGamut };
            SourceColorSpace = sRGB;
            TargetColorSpace = wideGamut;

            ConvertCommand = new RelayCommand<object>((param) => ConvertColorSpaces());

            ConvertColorSpaces();
        }

        private void ConvertColorSpaces()
        {
            ConvertedImage = new WriteableBitmap(Image);
            FirePropertyChanged(nameof(ConvertedImage));

            int height = ConvertedImage.PixelHeight;
            int width = ConvertedImage.PixelWidth;

            ConvertedImage.Lock();
            var ptr = ConvertedImage.BackBuffer;
            int stride = ConvertedImage.BackBufferStride;

            Parallel.For(0, height, (x) =>
            {
                for (int y = 0; y < width; y++)
                {
                    Color color;
                    int myPtr;
                    unsafe
                    {
                        myPtr = (int)ptr + x * stride + y * 4;
                        int colorData = *((int*)myPtr);
                        color = Color.FromArgb(255, (byte)(colorData >> 16),
                            (byte)(colorData >> 8),
                            (byte)(colorData >> 0));
                    }

                    var vXYZ = SourceColorSpace.RGBToXYZ(color);
                    color = TargetColorSpace.XYZToRGB(vXYZ);

                    unsafe
                    {
                        myPtr = (int)ptr + x * stride + y * 4;
                        int colorData = color.R << 16;
                        colorData |= color.G << 8;
                        colorData |= color.B << 0;

                        *((int*)myPtr) = colorData;
                    }

                    //ConvertedImage.SetPixel(x, y, color);
                }
            });

            ConvertedImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));

            ConvertedImage.Unlock();
        }
    }
}