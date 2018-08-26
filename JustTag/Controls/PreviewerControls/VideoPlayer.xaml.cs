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
using JustTag.Tagging;

namespace JustTag.Controls.PreviewerControls
{
    // TODO: Consider completely replacing this control with a different external library, because FFME
    // has been more trouble than it's worth.  For this reason, I'm not going to bother migrating this
    // class.
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl, IPreviewerControl
    {
        private FileInfo currentFile;

        private double cachedGifDuration = 0;       // FFME does't properly return the length of animated gifs, so we need
                                                    // to calculate it ourselves when we load it.

        private bool shouldBeMuted = true;          // FFME doesn't let us mute it if there is no sound, so we need to keep
                                                    // track of this ourselves.

        public VideoPlayer()
        {
            InitializeComponent();
            UpdateControls();
        }


        // Misc methods

        public bool CanOpen(TaggedFilePath file) => !file.IsFolder;   // TODO: Only return true if it's a video file.

        /// <summary>
        /// Opens the given file in the video player.
        /// </summary>
        /// <param name="selectedFile"></param>
        public async Task OpenPreview(TaggedFilePath selectedFile)
        {
            currentFile = selectedFile.ToFSInfo() as FileInfo;

            // If it's a gif, calculate its duration
            if (selectedFile.Extension.ToLower() == ".gif")
                cachedGifDuration = CalculateGifDuration(currentFile.FullName);

            // Open the file and autoplay it
            await videoPlayer.Open(new Uri(currentFile.FullName));
            await videoPlayer.Play();

            UpdateControls();
            volumeSlider.Value = volumeSlider.Maximum * videoPlayer.Volume;
        }

        /// <summary>
        /// Unloads the currently-loaded video.
        /// </summary>
        public Task ClosePreview() => videoPlayer.Close();

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

            // Update the play button
            playButton.Content = videoPlayer.IsPlaying ? "Pause" : "Play";

            // Update the volume controls
            videoPlayer.IsMuted = shouldBeMuted;
            volumeSlider.IsEnabled = !shouldBeMuted;
            volumeIcon.Visibility = shouldBeMuted ? Visibility.Hidden : Visibility.Visible;
            volumeMutedIcon.Visibility = shouldBeMuted ? Visibility.Visible : Visibility.Hidden;
        }


        // Event handlers

        private void videoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // Swallowing exceptions is bad juju.
            #if DEBUG
            MessageBox.Show(e.ErrorException.Message);
            # endif
        }

        private async void playButton_Click(object sender, RoutedEventArgs e)
        {
            // Play/pause the video
            await PlayOrPause(!videoPlayer.IsPlaying);
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

        private async void videoTimeSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Don't let the slider move on its own while the user is dragging it
            await PlayOrPause(false);
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

        private async void videoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            // If it's a video, loop it.
            await videoPlayer.Stop();
            await PlayOrPause(true);
        }
    }
}
