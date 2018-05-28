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
            // TODO: Preview different file types using different controls
            IsOpening = true;
            await videoPlayer.Open(selectedFile);
            IsOpening = false;
        }

        /// <summary>
        /// Closes the currently-open file preview
        /// </summary>
        /// <returns></returns>
        public Task ClosePreview()
        {
            // TODO: Different closing behavior for different file types
            return videoPlayer.UnloadVideo();
        }
    }
}
