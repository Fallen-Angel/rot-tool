using Homeworld2.ROT;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                foreach (var name in files)
                {
                    ROT rot;
                    if (System.IO.Path.GetExtension(name) == ".rot")
                    {
                        using (var file = File.Open(name, FileMode.Open))
                        {
                            rot = ROT.Read(file);
                        }

                        var encoder = new PngBitmapEncoder();
                        var frame = BitmapFrame.Create(rot.Bitmap);
                        encoder.Frames.Add(frame);

                        using (var file = File.Open(name + ".png", FileMode.Create))
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

                        var frame = decoder.Frames[0];

                        var bitmap = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);

                        rot = new ROT { Format = _format };
                        rot.GenerateMipmaps(bitmap);

                        using (var file = File.Open(name + ".rot", FileMode.Create))
                        {
                            rot.Write(file);
                        }
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
