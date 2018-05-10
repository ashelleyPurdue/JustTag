using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.IO;

namespace JustTag
{
    /// <summary>
    /// Interaction logic for Fullscreen.xaml
    /// </summary>
    public partial class Fullscreen : Window
    {
        private FileInfo[] files;

        public Fullscreen(FileInfo[] files)
        {
            InitializeComponent();
            this.files = files;

            // Open the first file in the list
            // TODO: pass in the starting file as an argument
            videoPlayer.ShowFilePreview(files[0]);
        }
    }
}
