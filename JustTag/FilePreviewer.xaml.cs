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
using System.IO;

namespace JustTag
{
    /// <summary>
    /// Interaction logic for FilePreviewer.xaml
    /// </summary>
    public partial class FilePreviewer : UserControl
    {
        public bool IsOpening { get; private set; }

        private Control activePreviewControl = null;    // The control being used to preview the current file

        public FilePreviewer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows a preview for the given file
        /// </summary>
        /// <param name="selectedFile"></param>
        /// <returns></returns>
        public async Task OpenPreview(FileInfo selectedFile)
        {
            IsOpening = true;

            // Close the previously open file
            await ClosePreview();

            // TODO: Choose a different control based on the file type
            activePreviewControl = videoPlayer;
            activePreviewControl.Visibility = Visibility.Visible;
            await videoPlayer.Open(selectedFile);

            IsOpening = false;
        }

        /// <summary>
        /// Closes the currently-open file preview
        /// </summary>
        /// <returns></returns>
        public async Task ClosePreview()
        {
            // Don't do anything if already closed
            if (activePreviewControl == null)
                return;

            // Hide the old control
            activePreviewControl.Visibility = Visibility.Collapsed;
            activePreviewControl = null;

            // TODO: Different closing behavior for different file types
            await videoPlayer.UnloadVideo();
        }
    }
}
