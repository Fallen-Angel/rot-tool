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
        private ROT ROT;
        private Format format;

        public MainWindow()
        {
            InitializeComponent();
            ROT = new ROT();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void Window_Drop_1(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                foreach (string name in files)
                {
                    if (System.IO.Path.GetExtension(name) == ".rot")
                    {
                        using (FileStream file = File.Open(name, FileMode.Open))
                        {
                            ROT.Read(file);
                        }

                        PngBitmapEncoder png = new PngBitmapEncoder();
                        BitmapFrame frame = BitmapFrame.Create(ROT.Bitmap);
                        png.Frames.Add(frame);

                        using (FileStream file = File.Open(name + ".png", FileMode.Create))
                        {
                            png.Save(file);
                        }

                        using (FileStream file = File.Open(name + " kopia.rot", FileMode.Create))
                        {
                            ROT.Write(file);
                        }
                    }
                    else
                    {
                        BitmapDecoder decoder;
                        using (FileStream file = File.Open(name, FileMode.Open))
                        {
                            decoder = BitmapDecoder.Create(file, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        }

                        BitmapFrame frame = decoder.Frames[0];

                        FormatConvertedBitmap bmp = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);

                        ROT.Format = format; ;
                        ROT.GenerateMipmaps(bmp);

                        using (FileStream file = File.Open(name + ".rot", FileMode.Create))
                        {
                            ROT.Write(file);
                        }
                    }
                }
            }
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            format = Format.RGBA32;
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            format = Format.DXT1;
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            format = Format.DXT3;
        }

        private void RadioButton_Checked_4(object sender, RoutedEventArgs e)
        {
            format = Format.DXT5;
        }
    }
}
