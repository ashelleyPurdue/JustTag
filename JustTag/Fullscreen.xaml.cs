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


        // Misc methods

        private void UpdateUI()
        {
            currentFileIndex = Utils.WrapIndex(currentFileIndex, files.Length); // Wrap the index around
            videoPlayer.ShowFilePreview(files[currentFileIndex]);               // Show the file
        }


        // Event handlers

        private void normalScreenButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Close if it's the escape key
            if (e.Key == Key.Escape)
            {
                Close();
                return;
            }

            // Navigate left/right if it's the left/right keys
            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                if (e.Key == Key.Left)
                    currentFileIndex--;
                else
                    currentFileIndex++;

                UpdateUI();
                return;
            }
        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            currentFileIndex--;
            UpdateUI();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            currentFileIndex++;
            UpdateUI();
        }
    }
}
