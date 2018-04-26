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
    /// Interaction logic for FindReplaceTagsWindow.xaml
    /// </summary>
    public partial class FindReplaceTagsWindow : Window
    {
        private string directory;

        public FindReplaceTagsWindow(string directory, IEnumerable<string> autoCompleteTags)
        {
            InitializeComponent();

            this.directory = directory;

            // Set the autocomplete sources
            findTextbox.autoCompletionSource = autoCompleteTags;
            replaceTextbox.autoCompletionSource = autoCompleteTags;
        }

        private void ReplaceTags(string findTag, string replaceTag)
        {
            // Loop over all files in the directory
            DirectoryInfo dir = new DirectoryInfo(directory);
            var files = dir.EnumerateFiles();

            foreach (FileInfo f in files)
            {
                // Get the tags
                HashSet<string> tags = new HashSet<string>(Utils.GetFileTags(f.Name));

                // Skip this file if it doesn't have the find tag
                if (!tags.Contains(findTag))
                    continue;

                // Replace the tag
                tags.Remove(findTag);
                tags.Add(replaceTag);

                // Save the changes to the file system.
                string newName = Utils.ChangeFileTags(f.Name, tags.ToArray());
                string newPath = System.IO.Path.Combine(f.DirectoryName, newName);

                try
                {
                    f.MoveTo(newPath);
                }
                catch (IOException e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        private async void replaceButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Error checking on the input
            string findTag = findTextbox.Text;
            string replaceTag = replaceTextbox.Text;

            // Disable the controls while we wait
            IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;

            // Replace the tags asynchronously
            await Task.Run(() =>
            {
                ReplaceTags(findTag, replaceTag);
            });

            // Close this window
            Close();
        }
    }
}
