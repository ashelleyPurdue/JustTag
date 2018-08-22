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

        public bool CanOpen(FileSystemInfo file) => Utils.IsImageFile(file);

        public Task ClosePreview()
        {
            image.Source = null;
            return Task.CompletedTask;
        }

        public Task OpenPreview(FileSystemInfo file)
        {
            // Reset the panning/zooming
            zoomBorder.Reset();

            image.Source = Utils.LoadImage(file.FullName);
            return Task.CompletedTask;
        }
    }
}
