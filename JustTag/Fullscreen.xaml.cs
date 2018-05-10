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
        private int currentFileIndex = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"> All of the files that the user can flip between with the arrow buttons</param>
        /// <param name="currentFile"> The file start with showing</param>
        public Fullscreen(FileInfo[] files, FileInfo currentFile)
        {
            InitializeComponent();
            this.files = files;

            // Open the starting file
            currentFileIndex = Array.IndexOf(files, currentFile);

            // If there is no such file(eg: if it is a folder), just default to the first
            if (currentFileIndex < 0)
                currentFileIndex = 0;

            // Show the file
            videoPlayer.ShowFilePreview(files[currentFileIndex]);
        }

        private void normalScreenButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
