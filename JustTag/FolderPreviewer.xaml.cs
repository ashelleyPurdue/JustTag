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
        private List<Image> previewIcons = new List<Image>();

        public FolderPreviewer()
        {
            InitializeComponent();
            const double IMAGE_HEIGHT = 100;
            const double SEPARATION_PERCENTAGE = 0.9;

            // Create all the icons
            for (int i = 0; i < MAX_ICONS; i++)
            {
                Image image = new Image();

                previewIcons.Add(image);
                image.Margin = new Thickness(0, 0, 0, 0);
                image.Height = IMAGE_HEIGHT;

                stackPanel.Children.Add(image);
            }
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
    }
}
