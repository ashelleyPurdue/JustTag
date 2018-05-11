using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace JustTag
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        public bool IsVideo
        {
            get
            {
                // It's a video if it has more than one frame.
                return GetCurrentVideoDuration() > videoPlayer.VideoFrameLength;
            }
        }

        private double cachedGifDuration = 0;       // FFME does't properly return the length of animated gifs, so we need
                                                    // to calculate it ourselves when we load it.

        private bool shouldBeMuted = true;          // FFME doesn't let us mute it if there is no sound, so we need to keep
                                                    // track of this ourselves.
           
        private bool isFullscreen = false;

        public VideoPlayer()
        {
            InitializeComponent();
            UpdateControls();
        }


        // Misc methods

        /// <summary>
        /// Opens the given file in the video player.
        /// </summary>
        /// <param name="selectedFile"></param>
        public async Task Open(FileInfo selectedFile)
        {
            // If it's a gif, calculate its duration
            if (selectedFile.Extension.ToLower() == ".gif")
                cachedGifDuration = CalculateGifDuration(selectedFile.FullName);

            // Open the file and autoplay it
            await videoPlayer.Open(new Uri(selectedFile.FullName));
            await videoPlayer.Play();

            UpdateControls();
            volumeSlider.Value = volumeSlider.Maximum * videoPlayer.Volume;
        }

        /// <summary>
        /// Unloads the currently-loaded video.
        /// </summary>
        public Task UnloadVideo()
        {
            return videoPlayer.Close();
        }

        private async Task PlayOrPause(bool play)
        {
            // Play/pause the video
            if (play)
                await videoPlayer.Play();
            else
                await videoPlayer.Pause();

            UpdateControls();
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

        private void UpdateControls()
        {
            // Only show the controls if the mouse is over
            videoControls.Visibility = IsMouseOver ? Visibility.Visible : Visibility.Hidden;

            // Hide all video-specific controls(play button, slider, etc) if it's not a video
            Visibility v = IsVideo ? Visibility.Visible : Visibility.Hidden;
            playButton.Visibility = v;
            videoTimeSlider.Visibility = v;
            volumeControls.Visibility = v;

            // Update the play button
            playButton.Content = videoPlayer.IsPlaying ? "Pause" : "Play";
            playButton.Visibility = IsVideo ? Visibility.Visible : Visibility.Hidden;

            // Update the volume controls
            videoPlayer.IsMuted = shouldBeMuted;
            volumeSlider.IsEnabled = !shouldBeMuted;
            volumeIcon.Visibility = shouldBeMuted ? Visibility.Hidden : Visibility.Visible;
            volumeMutedIcon.Visibility = shouldBeMuted ? Visibility.Visible : Visibility.Hidden;

            // Update the full screen button
            fullScreenButton.IsEnabled = videoPlayer.Source != null;
            fullScreenButton.Content = isFullscreen ? "Normal size" : "Fullscreen";
        }


        // Event handlers

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

        private void muteButton_Click(object sender, RoutedEventArgs e)
        {
            shouldBeMuted = !shouldBeMuted;
            UpdateControls();
        }

        private void UserControl_MouseEnterOrLeave(object sender, MouseEventArgs e)
        {
            e.Handled = false;
            UpdateControls();
        }

        private void fullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            Window currentWindow = Window.GetWindow(this);

            // If we're already in fullscreen mode, leave it.
            if (isFullscreen)
            {
                currentWindow.Close();
                return;
            }

            // Hide the current window
            currentWindow.Visibility = Visibility.Hidden;

            // Show the fullscreen window
            isFullscreen = true;
            UpdateControls();

            FileInfo currentFile = new FileInfo(videoPlayer.Source.AbsolutePath);
            Fullscreen fullscreen = new Fullscreen(this, currentFile);
            fullscreen.ShowDialog();

            // Fullscreen was closed, so switch back to the old window
            isFullscreen = false;
            UpdateControls();
            currentWindow.Visibility = Visibility.Visible;
        }
    }
}
