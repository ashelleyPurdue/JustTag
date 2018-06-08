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
using System.Windows.Shapes;
using System.IO;

namespace JustTag
{
    /// <summary>
    /// Interaction logic for DeleteTagWindow.xaml
    /// </summary>
    public partial class DeleteTagWindow : Window
    {
        // TODO: Refactor this copy/pasted code
        private string directory;

        public DeleteTagWindow(string directory, IEnumerable<string> autoCompleteTags)
        {
            InitializeComponent();
            this.directory = directory;
        }

        // Misc methods
        private void DeleteTag(string tag)
        {
            // Loop over all files in the directory
            DirectoryInfo dir = new DirectoryInfo(directory);
            var files = dir.EnumerateFileSystemInfos();

            foreach (FileSystemInfo f in files)
            {
                // Get the tags
                TaggedFileName fname = new TaggedFileName(f.Name);

                // Skip this file if it doesn't have the find tag
                if (!fname.tags.Contains(tag))
                    continue;

                // Remove the tag
                fname.tags.Remove(tag);

                // Save the changes to the file system.
                try
                {
                    Utils.ChangeFileTags(f, fname);
                }
                catch (IOException e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        // Events
        private async void goButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable the controls while we wait
            IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;

            // Replace the tags asynchronously
            await Task.Run(() =>
            {
                DeleteTag(deleteTextbox.Text);
            });

            // Close this window
            Close();
        }

        private void deleteTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Refactor this copy/pasted code
            bool valid = Utils.IsTagValid(deleteTextbox.Text);

            // Turn this box red if it's invalid
            deleteTextbox.Background = valid ? Brushes.White : Brushes.Red;

            // Disable the button if it's invalid
            if (!valid)
                goButton.IsEnabled = false;
        }
    }
}
