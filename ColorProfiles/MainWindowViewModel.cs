using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorProfiles
{
    class MainWindowViewModel: INotifyPropertyChanged
    {
        public List<ColorSpace> ColorSpaceList { get; private set; }

        public ColorSpace SourceColorSpace { get; set; }
        public ColorSpace TargetColorSpace { get; set; }

        public WriteableBitmap Image { get; set; } = new WriteableBitmap(new BitmapImage(new Uri("Images/widegamut.jpg", UriKind.Relative)));
        public WriteableBitmap ConvertedImage { get; set; }

        public ICommand ConvertCommand { get; private set; }
        public ICommand ChooseImageCommand { get; private set; }

        public MainWindowViewModel()
        {
            InitializeBindings();
            ConvertColorSpaces();
        }

        private void InitializeBindings()
        {
            ColorSpaceList = new List<ColorSpace> { ColorSpaces.sRGB, ColorSpaces.wideGamut, ColorSpaces.custom };
            SourceColorSpace = ColorSpaces.sRGB;
            TargetColorSpace = ColorSpaces.wideGamut;
            ConvertCommand = new RelayCommand<object>((param) => ConvertColorSpaces());
            ChooseImageCommand = new RelayCommand<object>((param) => ChooseImage());
        }

        private void ChooseImage()
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp",
                InitialDirectory = System.IO.Directory.GetCurrentDirectory() + "\\Images"
            };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            
            try
            {
                Image = new WriteableBitmap(new BitmapImage(new Uri(dialog.FileName)));
                FirePropertyChanged(nameof(Image));
                ConvertColorSpaces();
            }
            catch(Exception)
            {
                MessageBox.Show("Invalid file format");
            }
        }

        private void ConvertColorSpaces()
        {
            RecalculateMatrices();

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
                    Color newColor = TargetColorSpace.XYZToRGB(vXYZ);

                    unsafe
                    {
                        myPtr = (int)ptr + x * stride + y * 4;
                        int colorData = newColor.R << 16;
                        colorData |= newColor.G << 8;
                        colorData |= newColor.B << 0;

                        *((int*)myPtr) = colorData;
                    }
                }
            });

            ConvertedImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
            ConvertedImage.Unlock();

            FirePropertyChanged(nameof(SourceColorSpace));
            FirePropertyChanged(nameof(TargetColorSpace));
        }

        private void RecalculateMatrices()
        {
            if (SourceColorSpace.Editable)
                SourceColorSpace.RecalculateMatrices();
            if (TargetColorSpace.Editable)
                TargetColorSpace.RecalculateMatrices();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}