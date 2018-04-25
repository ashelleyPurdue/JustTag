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
using System.Windows.Threading;

namespace JustTag
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        private DispatcherTimer sliderUpdateTimer;  // Updates the slider position as the video is playing

        private bool videoPlaying = false;          // MediaElement doesn't have an IsPlaying property, so we need to
                                                    // track it ourselves.  What a hassle!

        public VideoPlayer()
        {
            InitializeComponent();

            // Hook up the slider update timer
            sliderUpdateTimer = new DispatcherTimer();
            sliderUpdateTimer.Interval = TimeSpan.FromMilliseconds(30);
            sliderUpdateTimer.Tick += SliderUpdateTimer_Tick;
            sliderUpdateTimer.Start();
        }


        // Misc methods

        public void ShowFilePreview(FileInfo selectedFile)
        {
            // Disable the navigation controls
            videoControls.IsEnabled = false;

            // Put it in the media element and start playing.
            // We're going to pause it immediately during the MediaOpened event
            videoPlayer.Open(new Uri(selectedFile.FullName));
            PlayOrPause(true);
        }

        private void PlayOrPause(bool play)
        {
            // Play/pause the video
            if (play)
            {
                videoPlayer.Play();
                sliderUpdateTimer.Start();
            }
            else
            {
                videoPlayer.Pause();
                sliderUpdateTimer.Stop();
            }
            // Remember the playing state, because MediaElement is stupid.
            videoPlaying = play;

            // Update the play button's text
            playButton.Content = play ? "Pause" : "Play";
        }

        private double GetVideoDuration()
        {
            // Gets the duration in seconds of the currently open video.
            // If it's not a video or it's not open, returns 0

            // If it's a gif, handle it differently;
            if (videoPlayer.MediaFormat == "gif")
            {
                string filePath = videoPlayer.Source.AbsolutePath;
                int totalDuration = 0;

                double minimumFrameDelay = 16;  // TODO: Calculate from the framerate?

                using (var image = System.Drawing.Image.FromFile(filePath))
                {
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

            // If it's not a video, return 0
            if (!videoPlayer.NaturalDuration.HasTimeSpan)
                return 0;

            // It's a normal video, so we can just use the duration property
            return videoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
        }

        // Event handlers

        private void videoPlayer_MediaOpening(object sender, Unosquare.FFME.Events.MediaOpeningRoutedEventArgs e)
        {
        }

        private void videoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            // It's rude to suddenly start playing a video without asking,
            // so pause it after we've seen the first frame.
            PlayOrPause(false);

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
            PlayOrPause(!videoPlaying);
        }

        private void videoTimeSlider_MouseMoved(object sender, MouseEventArgs e)
        {
            // Don't do anything if the user isn't holding a button
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            // Find the time to skip to
            double percent = videoTimeSlider.Value / videoTimeSlider.Maximum;
            double time = GetVideoDuration() * percent;

            // Jump to the time
            videoPlayer.Position = TimeSpan.FromSeconds(time);

            // Don't let this event fire again until a minimum amount of time has passed
            // This way we don't get a bunch of micro-jumps as the user moves.
            videoTimeSlider.MouseMove -= videoTimeSlider_MouseMoved;
            new System.Threading.Thread(() =>
            {
                // Sleep for a bit
                System.Threading.Thread.Sleep(30);

                // Resubscribe to the event
                videoTimeSlider.MouseMove += videoTimeSlider_MouseMoved;
            }).Start();

        }

        private void videoTimeSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Don't let the slider move on its own while the user is dragging it
            PlayOrPause(false);
        }

        private void SliderUpdateTimer_Tick(object sender, EventArgs e)
        {
            // Don't do anything if the open file has no duration
            double duration = GetVideoDuration();

            if (duration == 0)
                return;

            // Update the slider to match the video time
            double percent = videoPlayer.Position.TotalSeconds / GetVideoDuration();

            videoTimeSlider.Value = percent * videoTimeSlider.Maximum;
        }
    }
}
