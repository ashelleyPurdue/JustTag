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
    /// Interaction logic for FolderPreviewer.xaml
    /// </summary>
    public partial class FolderPreviewer : UserControl
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

        public void Open(DirectoryInfo dir)
        {
            // Clear all existing icons
            foreach (Image image in previewIcons)
                image.Source = null;

            // Get the icons of the first few files
            ImageSource[] selectedIcons = null;

            var allIcons = from FileSystemInfo file in dir.EnumerateFileSystemInfos()
                            where file is FileInfo
                            orderby file.Name
                            select GetThumbnail(file);

            selectedIcons = allIcons.Take(MAX_ICONS).ToArray();

            for (int i = 0; i < selectedIcons.Length; i++)
                previewIcons[i].Source = selectedIcons[i];
        }

        private ImageSource GetThumbnail(FileSystemInfo file)
        {
            // TODO: If it's a directory, return a picture of a folder
            if (file is DirectoryInfo)
                return null;

            // If the file is an image, then it serves as its own thumbnail
            if (Utils.IsImageFile(file))
                return new BitmapImage(new Uri(file.FullName));

            // Fall back to the file's icon
            return Utils.GetFileIcon(file);
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
