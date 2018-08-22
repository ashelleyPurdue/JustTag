using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using JustTag.Tagging;

namespace JustTag.Controls.PreviewerControls
{
    /// <summary>
    /// Interaction logic for ImagePreviewer.xaml
    /// </summary>
    public partial class ImagePreviewer : UserControl, IPreviewerControl
    {
        public ImagePreviewer()
        {
            InitializeComponent();
        }

        public bool CanOpen(TaggedFilePath file) => Utils.IsImageFile(file.FullPath);

        public Task ClosePreview()
        {
            image.Source = null;
            return Task.CompletedTask;
        }

        public Task OpenPreview(TaggedFilePath file)
        {
            // Reset the panning/zooming
            zoomBorder.Reset();

            image.Source = Utils.LoadImage(file.FullPath);
            return Task.CompletedTask;
        }
    }
}
