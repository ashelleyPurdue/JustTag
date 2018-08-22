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
using JustTag.Tagging;

namespace JustTag.Controls.PreviewerControls
{
    /// <summary>
    /// Interaction logic for FolderPreviewer.xaml
    /// </summary>
    public partial class FolderPreviewer : UserControl, IPreviewerControl
    {
        private const int MAX_ICONS = 5;
        private const double IMAGE_HEIGHT = 100;
        private const double SEPARATION_PERCENTAGE = 0.9;

        private List<Image> previewIcons = new List<Image>();

        public FolderPreviewer()
        {
            InitializeComponent();

            // Create the icons
            for (int i = 0; i < MAX_ICONS; i++)
            {
                Image image = new Image();
                image.Stretch = Stretch.Fill;

                previewIcons.Add(image);
                stackPanel.Children.Add(image);
            }
        }

        public bool CanOpen(TaggedFilePath file) => file.IsFolder;

        public async Task OpenPreview(TaggedFilePath folder)
        {
            // Close the previous folder
            await ClosePreview();

            // Get the thumbnails of the first few files
            ImageSource[] selectedIcons = null;

            var allIcons = from TaggedFilePath file in TagUtils.GetMatchingFiles(folder.FullPath, "")
                           where !file.IsFolder
                           select GetThumbnail(file);

            selectedIcons = allIcons.Take(MAX_ICONS).ToArray();

            // Display them stacked on top of each other.
            for (int i = 0; i < selectedIcons.Length; i++)
                previewIcons[i].Source = selectedIcons[i];
        }

        public Task ClosePreview()
        {
            // Close all of the images
            foreach (Image previewIcon in previewIcons)
                previewIcon.Source = null;

            return Task.CompletedTask;
        }

        private ImageSource GetThumbnail(TaggedFilePath file)
        {
            // TODO: If it's a directory, return a picture of a folder
            if (file.IsFolder)
                return null;

            // If the file isn't an image, then just use its icon as the thumbnail
            // TODO: Let the thumnail for videos be the first frame
            if (!Utils.IsImageFile(file.FullPath))
                return Utils.GetFileIcon(new FileInfo(file.FullPath));   // TODO: Make GetFileIcon take a string path instead.

            // The file is an image, so it serves as its own thumbnail
            // Load the image into a bitmap and return it.
            return Utils.LoadImage(file.FullPath);
        }

        private void stackPanel_LayoutUpdated(object sender, EventArgs e)
        {
            // Update all the icons' heights and positions
            double iconHeight = ActualHeight * 0.5;
            double offset = -iconHeight * SEPARATION_PERCENTAGE;

            previewIcons[0].Height = iconHeight;
            previewIcons[0].Width = iconHeight;
            previewIcons[0].Margin = new Thickness(0);

            for (int i = 1; i < MAX_ICONS; i++)
            {
                previewIcons[i].Height = iconHeight;
                previewIcons[i].Width = iconHeight;
                previewIcons[i].Margin = new Thickness(0, offset, 0, 0);
            }

        }
    }
}
