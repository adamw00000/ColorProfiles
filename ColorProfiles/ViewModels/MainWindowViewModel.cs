using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private ColorSpace _sourceColorSpace;
        private ColorSpace _targetColorSpace;
        private WriteableBitmap _image = new WriteableBitmap(new BitmapImage(new Uri("Images/mountain.jpg", UriKind.Relative)));
        private WriteableBitmap _convertedImage;
        
        public ObservableCollection<ColorSpace> ColorSpaceList { get; private set; }
        
        public ColorSpace SourceColorSpace { get => _sourceColorSpace; set
            {
                _sourceColorSpace = value;
                FirePropertyChanged(nameof(SourceColorSpace));
            }
        }

        public ColorSpace TargetColorSpace { get => _targetColorSpace;
            set
            {
                _targetColorSpace = value;
                FirePropertyChanged(nameof(TargetColorSpace));
            }
        }

        public WriteableBitmap Image { get => _image; set
            {
                _image = value;
                FirePropertyChanged(nameof(Image));
            }
        }
        public WriteableBitmap ConvertedImage { get => _convertedImage;
            set
            {
                _convertedImage = value;
                FirePropertyChanged(nameof(ConvertedImage));
            }
        }

        public ICommand ConvertCommand { get; private set; }
        public ICommand ChooseImageCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand SerializeCommand { get; private set; }
        public ICommand DeserializeCommand { get; private set; }
        public ICommand SaveResultCommand { get; private set; }

        public MainWindowViewModel()
        {
            InitializeBindings();
            ConvertColorSpaces();
        }

        private void InitializeBindings()
        {
            ColorSpaceList = ColorSpaces.ColorSpaceList;
            SourceColorSpace = ColorSpaces.sRGB;
            TargetColorSpace = ColorSpaces.wideGamut;
            ConvertCommand = new RelayCommand<object>((param) => ConvertColorSpaces());
            ChooseImageCommand = new RelayCommand<object>((param) => ChooseImage());
            SaveCommand = new RelayCommand<ColorSpace>((param) => SaveColorSpace(param));
            SerializeCommand = new RelayCommand<object>(async (param) => await SerializeColorSpaces());
            DeserializeCommand = new RelayCommand<object>((param) => DeserializeColorSpaces());
            SaveResultCommand = new RelayCommand<object>((param) => SaveResultImage());
        }

        private void SaveColorSpace(ColorSpace colorSpace)
        {
            string name = "User Color Space";
            InputDialog dialog = new InputDialog { Value = "User Color Space", Title = "Save color space" };

            var result = dialog.ShowDialog();
            if (result.HasValue && !result.Value)
                return;

            name = dialog.Value;

            ColorSpace newColorSpace = new ColorSpace(name, false,
                colorSpace.xw, colorSpace.yw,
                colorSpace.xr, colorSpace.yr,
                colorSpace.xg, colorSpace.yg,
                colorSpace.xb, colorSpace.yb,
                colorSpace.gamma);
            ColorSpaces.ColorSpaceList.Add(newColorSpace);

            if (colorSpace == TargetColorSpace)
                TargetColorSpace = newColorSpace;
            else
                SourceColorSpace = newColorSpace;
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
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dialog.FileName);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                Image = new WriteableBitmap(bitmap);
                ConvertColorSpaces();
            }
            catch
            {
                MessageBox.Show("Invalid file format");
            }
        }

        private void ConvertColorSpaces()
        {
            if (SourceColorSpace == null || TargetColorSpace == null)
                return;

            RecalculateMatrices();

            ConvertedImage = new WriteableBitmap(Image);
            int height = ConvertedImage.PixelHeight;
            int width = ConvertedImage.PixelWidth;

            ConvertedImage.Lock();
            var ptr = ConvertedImage.BackBuffer;
            int stride = ConvertedImage.BackBufferStride;

            Parallel.For(0, height, (x) =>
            {
                for (int y = 0; y < width; y++)
                {
                    CalculateColor(ptr, x, y, stride);
                }
            });

            ConvertedImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
            ConvertedImage.Unlock();
        }

        private void CalculateColor(IntPtr ptr, int x, int y, int stride)
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
                int colorData = newColor.R << 16;
                colorData |= newColor.G << 8;
                colorData |= newColor.B << 0;

                *((int*)myPtr) = colorData;
            }
        }

        private void RecalculateMatrices()
        {
            if (SourceColorSpace.Editable)
                SourceColorSpace.RecalculateMatrices();
            if (TargetColorSpace.Editable)
                TargetColorSpace.RecalculateMatrices();
        }

        private async Task SerializeColorSpaces()
        {
            string serialized = JsonConvert.SerializeObject(ColorSpaces.ColorSpaceList);

            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog
            {
                DefaultExt = "json",
                Filter = "JSON Files|*.json",
                InitialDirectory = Directory.GetCurrentDirectory(),
                FileName = "ColorSpaces.json"
            };
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            if (Path.GetExtension(dialog.FileName).ToLower() != ".json")
            {
                MessageBox.Show("Invalid extension!");
                return;
            }

            using (StreamWriter writer = new StreamWriter(dialog.FileName))
            {
                await writer.WriteAsync(serialized);
                MessageBox.Show("Saving successful!");
            }
        }

        private void DeserializeColorSpaces()
        {
            string serialized;

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "JSON Files|*.json",
                InitialDirectory = Directory.GetCurrentDirectory(),
                FileName = "ColorSpaces.json"
            };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            if (Path.GetExtension(dialog.FileName).ToLower() != ".json")
            {
                MessageBox.Show("Invalid extension!");
                return;
            }

            using (StreamReader reader = new StreamReader(dialog.FileName))
            {
                try
                {
                    serialized = reader.ReadToEnd();
                }
                catch
                {
                    MessageBox.Show("Loading failed!");
                    return;
                }
            }
            
            var collection = JsonConvert.DeserializeObject<ObservableCollection<ColorSpace>>(serialized);
            if (collection.Count == 0)
            {
                MessageBox.Show("Loading failed - list cannot be empty!");
                return;
            }

            ColorSpaces.ColorSpaceList.Clear();
            foreach (var colorSpace in collection)
            {
                ColorSpaces.ColorSpaceList.Add(colorSpace);
            }

            SourceColorSpace = ColorSpaceList[0];
            if (collection.Count == 1)
                TargetColorSpace = ColorSpaceList[0];
            else
                TargetColorSpace = ColorSpaceList[1];

            MessageBox.Show("Loading successful!");
        }

        private void SaveResultImage()
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog
            {
                DefaultExt = "png",
                Filter = "PNG Files|*.png",
                InitialDirectory = Directory.GetCurrentDirectory() + "\\Images",
                FileName = "output.png"
            };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            if (Path.GetExtension(dialog.FileName).ToLower() != ".png")
            {
                MessageBox.Show("Invalid extension!");
                return;
            }

            using (FileStream stream = new FileStream(dialog.FileName, FileMode.Create))
            {
                PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                encoder5.Frames.Add(BitmapFrame.Create(ConvertedImage));
                encoder5.Save(stream);
                stream.Close();
            }

            MessageBox.Show("Saving successful!");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}