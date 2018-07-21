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

namespace JustTag.Controls.PreviewerControls
{
    /// <summary>
    /// Interaction logic for FilePreviewer.xaml
    /// </summary>
    public partial class FilePreviewer : UserControl
    {
        public bool IsOpening { get; private set; }

        private IPreviewerControl[] previewControls;
        private IPreviewerControl activePreviewControl = null;    // The control being used to preview the current file

        public FilePreviewer()
        {
            InitializeComponent();

            // Populate the list of controls
            previewControls = new IPreviewerControl[]
            {
                videoPlayer,
                folderPreviewer
            };
        }

        /// <summary>
        /// Shows a preview for the given file
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <returns></returns>
        public async Task OpenPreview(FileSystemInfo selectedItem)
        {
            IsOpening = true;

            // Close the previously open file
            await ClosePreview();

            // Pick the first control that's capable of opening this file
            activePreviewControl = previewControls.First(c => c.CanOpen(selectedItem));

            // Show the file
            activePreviewControl.Visibility = Visibility.Visible;
            await activePreviewControl.OpenPreview(selectedItem);

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

            // Close the old control
            activePreviewControl.Visibility = Visibility.Collapsed;
            await activePreviewControl.ClosePreview();
            activePreviewControl = null;
        }
    }
}
