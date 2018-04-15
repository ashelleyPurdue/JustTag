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
using System.Text.RegularExpressions;

namespace FileBrowserWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NavigationStack<string> pathHistory;

        public MainWindow()
        {
            InitializeComponent();

            pathHistory = new NavigationStack<string>(Directory.GetCurrentDirectory());
            UpdateCurrentDirectory();
        }
        

        // Misc methods

        private void UpdateCurrentDirectory()
        {
            // Move to the directory
            Directory.SetCurrentDirectory(pathHistory.Current);
            DirectoryInfo currentDir = new DirectoryInfo(pathHistory.Current);

            // Set the textbox to the working directory
            currentPathBox.Text = pathHistory.Current;

            // Update the forward/back/up buttons
            forwardButton.IsEnabled = pathHistory.HasNext;
            backButton.IsEnabled = pathHistory.HasPrev;

            upButton.IsEnabled = Directory.GetParent(pathHistory.Current) != null;

            // Add all subdirectories to the listbox.
            // We want the subdirectories listed first so
            // the user can navigate easier
            folderContentsBox.Items.Clear();
            
            var subdirs = currentDir.EnumerateDirectories();
            foreach (DirectoryInfo dir in subdirs)
                folderContentsBox.Items.Add(dir);

            // Add all files to the listbox that match the filter
            var files = currentDir.EnumerateFiles();
            foreach (FileInfo file in files)
            {
                if (!MatchesTagFilter(file.Name))
                    continue;

                folderContentsBox.Items.Add(file);
            }
        }

        private void ShowFilePreview()
        {
            // If it's an image file, show it in the zoombox
            FileInfo selectedFile = folderContentsBox.SelectedItem as FileInfo;

            if (selectedFile == null)
                return;

            try
            {
                imagePreviewer.Source = new BitmapImage(new Uri(selectedFile.FullName));
            }
            catch (NotSupportedException)
            {
                // Nothing to see here.
            }
        }

        private bool MatchesTagFilter(string fileName)
        {
            // Get all the tags from the filename
            string[] tags = GetFileTags(fileName);

            // Parse the filter
            // TODO: Move this part somewhere else so we only have to parse the filter once.
            List<string> forbiddenTags = new List<string>();
            List<string> requiredTags = new List<string>();

            string[] filterWords = tagFilterTextbox.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in filterWords)
            {
                // Sort each word into either forbidden or required tags
                // Anything with a '-' at the start means it's a forbidden tag.
                if (word[0] == '-')
                {
                    forbiddenTags.Add(word.Substring(1));
                    continue;
                }

                requiredTags.Add(word);
            }

            // Return false if any of the required tags are missing
            foreach (string t in requiredTags)
                if (!tags.Contains(t))
                    return false;

            // Return false if any of the forbidden tags are present
            foreach (string t in forbiddenTags)
                if (tags.Contains(t))
                    return false;

            // It passed the filter
            return true;
        }

        private string[] GetFileTags(string fileName)
        {
            string betweenBrackets = Regex.Match(fileName, @"\[([^)]*)\]").Groups[1].Value;
            return betweenBrackets.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Renames the given file so it has the given tags
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="newTags"></param>
        private string ChangeFileTags(string fileName, string[] newTags)
        {
            // Get the stuff before and after the tags
            string beforeTags = fileName.Split('[')[0];
            string afterTags = fileName.Split(']')[1];

            // Make a new tag string from the array
            StringBuilder builder = new StringBuilder();

            builder.Append("[");
            for (int i = 0; i < newTags.Length; i++)
            {
                // Add a space if this isn't the first
                if (i != 0)
                    builder.Append(" ");

                // Add the tag
                builder.Append(newTags[i]);
            }
            builder.Append("]");

            // Jam them together to make the new filename
            return beforeTags + builder.ToString() + afterTags;
        }


        // Event handlers

        private void textbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;     // Don't go on if it's not the enter key

            // Navigate to the place.
            if (Directory.Exists(currentPathBox.Text))
            {
                pathHistory.Push(currentPathBox.Text);
                UpdateCurrentDirectory();
            }
        }

        private void currentPathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Change to a red background if the text is not a valid path

            bool valid = Directory.Exists(currentPathBox.Text);
            currentPathBox.Background = valid ? Brushes.White : Brushes.Red;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            pathHistory.MoveBack();
            UpdateCurrentDirectory();
        }

        private void upButton_Click(object sender, RoutedEventArgs e)
        {
            // Move to the parent directory
            string parent = Directory.GetParent(pathHistory.Current).FullName;
            pathHistory.Push(parent);
            UpdateCurrentDirectory();
        }

        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            pathHistory.MoveForward();
            UpdateCurrentDirectory();
        }

        private void folderContentsBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Object selectedItem = folderContentsBox.SelectedItem;

            // If the selected item is a folder, move to that folder.
            DirectoryInfo dir = selectedItem as DirectoryInfo;
            if (dir != null)
            {
                pathHistory.Push(dir.FullName);
                UpdateCurrentDirectory();

                return;
            }

            // If the selected item is a file, open that file.
            FileInfo file = selectedItem as FileInfo;
            if (file != null)
            {
                System.Diagnostics.Process.Start(file.FullName);
            }
        }

        private void folderContentsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Don't do anything if selection is null
            if (folderContentsBox.SelectedItem == null)
                return;

            ShowFilePreview();

            // Update the tag box with this file's tags
            string name = ((FileSystemInfo)folderContentsBox.SelectedItem).Name;
            string[] tags = GetFileTags(name);

            StringBuilder builder = new StringBuilder();
            foreach (string t in tags)
                builder.AppendLine(t);

            tagsBox.Text = builder.ToString();

            // Hide the save button
            tagSaveButton.Visibility = Visibility.Hidden;
        }

        private void tagsBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Make the background red if any of the tags are invalid

            // Show the save button
            tagSaveButton.Visibility = Visibility.Visible;
        }

        private void tagSaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Update the selected file's tags
            FileInfo file = folderContentsBox.SelectedItem as FileInfo;

            if (file == null)
                return;

            // Parse the tags into a list
            string[] tags = tagsBox.Text.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Get the new file path
            string newFileName = ChangeFileTags(file.Name, tags);
            string newFilePath = System.IO.Path.Combine(file.DirectoryName, newFileName);

            // DEBUG: Print the file name instead of renaming
            MessageBox.Show(newFilePath);

            // Hide the save button
            tagSaveButton.Visibility = Visibility.Hidden;
        }
    }
}
