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
using JustTag.Controls.PreviewerControls;

namespace JustTag.Pages
{
    /// <summary>
    /// Interaction logic for Fullscreen.xaml
    /// This whole thing is one giant hack
    /// </summary>
    public partial class Fullscreen : Window
    {
        private FileSystemInfo[] browsableFiles;

        private int currentFileIndex = 0;
        private FilePreviewer filePreviewer;

        private Grid oldPreviewerParent;
        private int oldPreviewerParentIndex;  // The index of videoPlayer in oldPreviewerParent.Children.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePreviewer"> We pass the file previewer object in from MainWindow so it will have the same state</param>
        /// <param name="browsableFiles"> All of the files that the user can flip between with the arrow buttons</param>
        /// <param name="currentFile"> The file start with showing</param>
        public Fullscreen(FilePreviewer filePreviewer, FileSystemInfo[] browsableFiles, int currentFileIndex)
        {
            InitializeComponent();

            // Open the starting file
            this.browsableFiles = browsableFiles;
            this.currentFileIndex = currentFileIndex;

            // HACK: Embed the file previewer in this window
            // This way it will have the same state(time, volume, etc.)
            this.filePreviewer = filePreviewer;

            oldPreviewerParent = filePreviewer.Parent as Grid;
            oldPreviewerParentIndex = oldPreviewerParent.Children.IndexOf(filePreviewer);
            oldPreviewerParent.Children.Remove(filePreviewer);

            grid.Children.Insert(0, filePreviewer);
        }


        // Misc methods

        private void UpdateUI()
        {
            currentFileIndex = Utils.WrapIndex(currentFileIndex, browsableFiles.Length); // Wrap the index around
            filePreviewer.OpenPreview(browsableFiles[currentFileIndex]);                 // Show the file
        }


        // Event handlers

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // HACK: Restore the video player to its old parent
            grid.Children.Remove(filePreviewer);
            oldPreviewerParent.Children.Insert(oldPreviewerParentIndex, filePreviewer);
        }

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
