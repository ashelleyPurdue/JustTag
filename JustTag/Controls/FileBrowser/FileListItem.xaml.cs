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

namespace JustTag.Controls.FileBrowser
{
    /// <summary>
    /// Interaction logic for FileListItem.xaml
    /// </summary>
    public partial class FileListItem : UserControl
    {
        public readonly TaggedFilePath file;

        private static Dictionary<string, ImageSource> fileIconCache = new Dictionary<string, ImageSource>();

        public FileListItem(TaggedFilePath file)
        {
            InitializeComponent();
            this.file = file;

            nameLabel.Content = file.Name;

            // Change the icon if it's not a folder
            if (!file.IsFolder)
                iconImg.Source = Utils.GetFileIcon(new FileInfo(file.FullPath));    // TODO: Chnage Utils.GetFileIcon to use a TaggedFilePath
        }

    }
}
