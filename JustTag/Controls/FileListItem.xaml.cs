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

namespace JustTag.Controls
{
    /// <summary>
    /// Interaction logic for FileListItem.xaml
    /// </summary>
    public partial class FileListItem : UserControl
    {
        public readonly FileSystemInfo file;

        private static Dictionary<string, ImageSource> fileIconCache = new Dictionary<string, ImageSource>();

        public FileListItem(FileSystemInfo file)
        {
            InitializeComponent();
            this.file = file;

            nameLabel.Content = file.Name;

            // Change the icon if it's not a folder
            if (file is FileInfo)
                iconImg.Source = Utils.GetFileIcon(file);
        }

    }
}
