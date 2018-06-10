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

            // Create the first icon
            CreateIcon(0);

            // Create the rest of the icons
            for (int i = 1; i < MAX_ICONS; i++)
                CreateIcon(-previewIcons[i - 1].Height * SEPARATION_PERCENTAGE);
        }

        private void CreateIcon(double offset)
        {
            Image image = new Image();
            image.Margin = new Thickness(0, offset, 0, 0);
            image.Height = IMAGE_HEIGHT;

            previewIcons.Add(image);
            stackPanel.Children.Add(image);
        }

        public async Task Open(DirectoryInfo dir)
        {
            // Clear all existing icons
            foreach (Image image in previewIcons)
                image.Source = null;

            // Get the icons of the first few files
            ImageSource[] selectedIcons = null;
            await Task.Run(() =>
            {
                var allIcons = from FileSystemInfo file in dir.EnumerateFileSystemInfos()
                               where file is FileInfo
                               orderby file.Name
                               select Utils.GetFileIcon(file);

                selectedIcons = allIcons.Take(MAX_ICONS).ToArray();
            });

            for (int i = 0; i < selectedIcons.Length; i++)
                previewIcons[i].Source = selectedIcons[i];
        }

        private void stackPanel_LayoutUpdated(object sender, EventArgs e)
        {
            // Update all the icons' heights and positions
            double iconHeight = ActualHeight * 0.5;
            double offset = -iconHeight * SEPARATION_PERCENTAGE;

            previewIcons[0].Height = iconHeight;
            previewIcons[0].Margin = new Thickness(0);

            for (int i = 1; i < MAX_ICONS; i++)
            {
                previewIcons[i].Height = iconHeight;
                previewIcons[i].Margin = new Thickness(0, offset, 0, 0);
            }

        }
    }
}
