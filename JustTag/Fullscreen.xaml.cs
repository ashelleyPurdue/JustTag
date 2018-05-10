﻿using System;
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
        public static FileInfo[] browsableFiles;

        private int currentFileIndex = 0;
        private VideoPlayer videoPlayer;

        private Grid oldVideoPlayerParent;
        private int oldVideoPlayerParentIndex;  // The index of videoPlayer in oldVideoPlayerParent.Children.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="videoPlayer"> We pass the video player object in from MainWindow so it will have the same state</param>
        /// <param name="browsableFiles"> All of the files that the user can flip between with the arrow buttons</param>
        /// <param name="currentFile"> The file start with showing</param>
        public Fullscreen(VideoPlayer videoPlayer, FileInfo currentFile)
        {
            InitializeComponent();

            // Open the starting file
            currentFileIndex = Array.IndexOf(browsableFiles, currentFile);

            // If there is no such file(eg: if it is a folder), just default to the first
            if (currentFileIndex < 0)
                currentFileIndex = 0;

            // HACK: Embed the video player in this window
            // This way it will have the same state(time, volume, etc.)
            this.videoPlayer = videoPlayer;

            oldVideoPlayerParent = videoPlayer.Parent as Grid;
            oldVideoPlayerParentIndex = oldVideoPlayerParent.Children.IndexOf(videoPlayer);
            oldVideoPlayerParent.Children.Remove(videoPlayer);

            grid.Children.Insert(0, videoPlayer);
        }


        // Misc methods

        private void UpdateUI()
        {
            currentFileIndex = Utils.WrapIndex(currentFileIndex, browsableFiles.Length); // Wrap the index around
            videoPlayer.ShowFilePreview(browsableFiles[currentFileIndex]);               // Show the file
        }


        // Event handlers

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // HACK: Restore the video player to its old parent
            grid.Children.Remove(videoPlayer);
            oldVideoPlayerParent.Children.Insert(oldVideoPlayerParentIndex, videoPlayer);
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