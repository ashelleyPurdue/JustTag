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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JustTag
{
    /// <summary>
    /// Interaction logic for VideoPlayer.xaml
    /// </summary>
    public partial class VideoPlayer : UserControl
    {
        public VideoPlayer()
        {
            InitializeComponent();
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
            double time = videoPlayer.NaturalDuration.TimeSpan.TotalSeconds * percent;

            // Jump to the time
            videoPlayer.Position = TimeSpan.FromSeconds(time);

            // Show one frame
            videoPlayer.Play();
            System.Threading.Thread.Sleep(2);
            videoPlayer.Pause();

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
            // Don't do anything if the open file is not a video
            if (!videoPlayer.NaturalDuration.HasTimeSpan)
                return;

            // Update the slider to match the video time
            double percent = videoPlayer.Position.TotalSeconds / videoPlayer.NaturalDuration.TimeSpan.TotalSeconds;

            videoTimeSlider.Value = percent * videoTimeSlider.Maximum;
        }
    }
}
