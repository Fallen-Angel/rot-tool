using Homeworld2.ROT;
using RotTool.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RotTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Format _format;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var about = new About();
            about.ShowDialog();
        }

        private void Window_Drop_1(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            // Note that you can have more than one file.
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Assuming you have one file that you care about, pass it off to whatever
            // handling code you have defined.
            foreach (var name in files)
            {
                TextureViewModel texture;
                if (Path.GetExtension(name)?.ToUpperInvariant() == ".ROT")
                {
                    using (var file = File.Open(name, FileMode.Open))
                    {
                        texture = new TextureViewModel(ROT.Read(file));
                    }

                    var encoder = new PngBitmapEncoder();
                    var frame = BitmapFrame.Create(texture.Bitmap);
                    encoder.Frames.Add(frame);

                    using (var file = File.Create(name + ".png"))
                    {
                        encoder.Save(file);
                    }
                }
                else
                {
                    BitmapDecoder decoder;
                    using (var file = File.Open(name, FileMode.Open))
                    {
                        decoder = BitmapDecoder.Create(file, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    }

                    var bitmap = decoder.Frames[0];
                        
                    texture = new TextureViewModel(new ROT(bitmap.PixelWidth, bitmap.PixelHeight, _format));
                    texture.GenerateMipmaps(bitmap);

                    using (var file = File.Create(name + ".rot"))
                    {
                        texture.Write(file);
                    }
                }
            }
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            _format = Format.RGBA32;
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            _format = Format.DXT1;
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            _format = Format.DXT3;
        }

        private void RadioButton_Checked_4(object sender, RoutedEventArgs e)
        {
            _format = Format.DXT5;
        }
    }
}
