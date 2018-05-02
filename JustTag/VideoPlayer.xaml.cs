﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JustTag
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        private double cachedGifDuration = 0;       // FFME does't properly return the length of animated gifs, so we need
                                                    // to calculate it ourselves when we load it.
       
       
        public VideoPlayer()
        {
            InitializeComponent();
        }


        // Misc methods

        /// <summary>
        /// Opens the given file in the video player.
        /// </summary>
        /// <param name="selectedFile"></param>
        public void ShowFilePreview(FileInfo selectedFile)
        {
            // Disable the navigation controls
            videoControls.IsEnabled = false;

            // If it's a gif, calculate its duration
            if (selectedFile.Extension.ToLower() == ".gif")
                cachedGifDuration = CalculateGifDuration(selectedFile.FullName);

            // Put it in the media element
            videoPlayer.Open(new Uri(selectedFile.FullName));
        }

        /// <summary>
        /// Unloads the currently-loaded video.
        /// </summary>
        public void UnloadVideo()
        {
            videoPlayer.Source = null;
        }

        private void PlayOrPause(bool play)
        {
            // Play/pause the video
            if (play)
                videoPlayer.Play();
            else
                videoPlayer.Pause();

            // Update the play button's text
            playButton.Content = play ? "Pause" : "Play";
        }

        private double CalculateGifDuration(string filePath)
        {
            // Algorithm taken from https://stackoverflow.com/questions/47343230/how-do-you-get-the-duration-of-a-gif-file-in-c
            int totalDuration = 0;

            using (var image = System.Drawing.Image.FromFile(filePath))
            {
                double minimumFrameDelay = 16;  // TODO: Calculate from the framerate?

                var frameDimension = new System.Drawing.Imaging.FrameDimension(image.FrameDimensionsList[0]);
                int frameCount = image.GetFrameCount(frameDimension);

                for (int f = 0; f < frameCount; f++)
                {
                    byte[] delayPropertyBytes = image.GetPropertyItem(20736).Value;
                    int frameDelay = BitConverter.ToInt32(delayPropertyBytes, f * 4) * 10;
                    // Minimum delay is 16 ms. It's 1/60 sec i.e. 60 fps
                    totalDuration += (frameDelay < minimumFrameDelay ? (int)minimumFrameDelay : frameDelay);
                }
            }

            // Convert total duration from milliseconds to seconds
            double durationSeconds = 0.001 * totalDuration;
            return durationSeconds;
        }

        private double GetCurrentVideoDuration()
        {
            // Gets the duration in seconds of the currently open video.
            // If it's not a video or it's not open, returns 0

            // If it's a gif, handle it differently;
            if (videoPlayer.MediaFormat == "gif")
                return cachedGifDuration;

            // If it's not a video, return 0
            if (!videoPlayer.NaturalDuration.HasTimeSpan)
                return 0;

            // It's a normal video, so we can just use the duration property
            return videoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
        }


        // Event handlers

        private void videoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            // If it's a video, enable the playback controls
            videoControls.IsEnabled = videoPlayer.CanPause;
        }

        private void videoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // Swallowing exceptions is bad juju.
            #if DEBUG
            MessageBox.Show(e.ErrorException.Message);
            # endif
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            // Play/pause the video
            PlayOrPause(!videoPlayer.IsPlaying);
        }

        private void videoTimeSlider_MouseMoved(object sender, MouseEventArgs e)
        {
            // Don't do anything if the user isn't holding a button
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            // Find the time to skip to
            double percent = videoTimeSlider.Value / videoTimeSlider.Maximum;
            double time = GetCurrentVideoDuration() * percent;

            // Jump to the time
            videoPlayer.Position = TimeSpan.FromSeconds(time);
        }

        private void videoTimeSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Don't let the slider move on its own while the user is dragging it
            PlayOrPause(false);
        }

        private void videoPlayer_PositionChanged(object sender, Unosquare.FFME.Events.PositionChangedRoutedEventArgs e)
        {
            // Don't do anything if the open file has no duration
            double duration = GetCurrentVideoDuration();

            if (duration == 0)
                return;

            // Update the slider to match the video time
            double percent = videoPlayer.Position.TotalSeconds / GetCurrentVideoDuration();

            videoTimeSlider.Value = percent * videoTimeSlider.Maximum;
        }

        private void volumeControls_MouseEnter(object sender, MouseEventArgs e)
        {
            // Show the volume slider
            volumeSlider.Visibility = Visibility.Visible;
        }

        private void volumeControls_MouseLeave(object sender, MouseEventArgs e)
        {
            // Hide the volume slider
            volumeSlider.Visibility = Visibility.Hidden;
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Change the volume
            videoPlayer.Volume = volumeSlider.Value / volumeSlider.Maximum;
        }
    }
}
