using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Text.RegularExpressions;

namespace JustTag
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NavigationStack<string> pathHistory;
        private HashSet<string> allKnownTags = new HashSet<string>();

        public MainWindow()
        {
            InitializeComponent();

            pathHistory = new NavigationStack<string>(Directory.GetCurrentDirectory());
            UpdateCurrentDirectory();

            // Hook the listbox up to the list of known tags
            allTagsListbox.ItemsSource = allKnownTags;

            // Set the filter textbox's autocomplete source to all the tags
            tagFilterTextbox.autoCompletionSource = allKnownTags;
            tagsBox.autoCompletionSource = allKnownTags;
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

            // Sort the directory entries into folders and files
            // This way we can display folders first so the user
            // Can navigate easier.
            var folders = new List<FileSystemInfo>();
            var files = new List<FileSystemInfo>();

            var entries = currentDir.EnumerateFileSystemInfos();
            foreach (FileSystemInfo e in entries)
            {
                // If it's a shortcut, resolve its target first
                FileSystemInfo entry = e;

                if (entry.Extension.ToLower() == ".lnk")
                    entry = Utils.GetShortcutTarget(entry);

                // Pick the right list to put it in
                List<FileSystemInfo> correctList = files;

                if (entry is DirectoryInfo)
                    correctList = folders;

                // Put it in the list
                correctList.Add(entry);
            }

            // Shuffle the files if "shuffle" is ticked
            if ((bool)shuffleCheckbox.IsChecked)
                files = Utils.ShuffleList(files);


            // Add the folders first
            var fileSource = new List<FileSystemInfo>();
            fileSource.AddRange(folders);

            // Add all the files that match the filter
            // Also record their tags in the "all known tags" list
            foreach (FileSystemInfo file in files)
            {
                // Record all the tags
                string[] tags = Utils.GetFileTags(file.Name);
                foreach (string tag in tags)
                    allKnownTags.Add(tag);

                // Add it if it matches
                if (MatchesTagFilter(file.Name))
                    fileSource.Add(file);
            }

            folderContentsBox.ItemsSource = fileSource;

            // Update the known tags listbox
            // Sort it in alphabetical order first
            List<string> knownTagsList = allKnownTags.ToList();
            knownTagsList.Sort();
            allTagsListbox.ItemsSource = knownTagsList;
        }

        private bool MatchesTagFilter(string fileName)
        {
            // Get all the tags from the filename
            string[] tags = Utils.GetFileTags(fileName);

            // Parse the filter
            // TODO: Move this part somewhere else so we only have to parse the filter once.
            List<string> forbiddenTags = new List<string>();
            List<string> requiredTags = new List<string>();

            string[] filterWords = tagFilterTextbox.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // HACK: If any of the words are ":untagged:", show only files without any tags.
            if (filterWords.Contains(":untagged:"))
                return tags.Length == 0;

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

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            // Apply the filter in the filter textbox
            UpdateCurrentDirectory();
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
            // Don't do anything if the last preview hasn't loaded yet
            if (videoPlayer.videoPlayer.IsOpening)
                return;

            // Don't do anything if selection is null
            if (folderContentsBox.SelectedItem == null)
                return;

            // Don't go on if it's not a file
            FileInfo file = folderContentsBox.SelectedItem as FileInfo;

            if (file == null)
                return;

            // Show the file preview
            videoPlayer.ShowFilePreview(file);

            // Enable the tag box and update it with this file's tags
            string name = ((FileSystemInfo)folderContentsBox.SelectedItem).Name;
            string[] tags = Utils.GetFileTags(name);

            StringBuilder builder = new StringBuilder();
            foreach (string t in tags)
                builder.AppendLine(t);

            tagsBox.IsEnabled = true;
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

        private async void tagSaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Update the selected file's tags
            FileInfo file = folderContentsBox.SelectedItem as FileInfo;

            if (file == null)
                return;

            // Parse the tags into a list
            string[] tags = tagsBox.Text.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Get the new file path
            string newFileName = Utils.ChangeFileTags(file.Name, tags);
            string newFilePath = System.IO.Path.Combine(file.DirectoryName, newFileName);

            // Remember the selected index so we can scroll back to it
            // after saving
            int selectedIndex = folderContentsBox.SelectedIndex;

            // Rename the file
            // We may need to wait for the file to be closed.
            bool success = false;
            for (int tryCount = 0; tryCount < 10; tryCount++)
            {
                try
                {
                    file.MoveTo(newFilePath);
                    success = true;
                    break;
                }
                catch(IOException)
                {
                    // Close the file so we can rename it.
                    videoPlayer.videoPlayer.Stop();
                    videoPlayer.videoPlayer.Source = null;

                    // Wait and try again
                    await System.Threading.Tasks.Task.Delay(100);
                }
            }

            // If we failed, inform the user.
            if (!success)
                MessageBox.Show("ERROR: Could not rename file.  Is it already open?");
            
            UpdateCurrentDirectory();

            // Scroll back to the same selected index
            folderContentsBox.SelectedIndex = selectedIndex;
            folderContentsBox.ScrollIntoView(folderContentsBox.SelectedItem);

            // Hide the save button
            tagSaveButton.Visibility = Visibility.Hidden;
        }

        private void tagsBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // HACK: Don't do anything if the save button is invisible
            if (tagSaveButton.Visibility == Visibility.Hidden)
                return;

            // Press the save button if it's shift+enter
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (e.Key == Key.Enter && shift)
            {
                e.Handled = true;
                tagSaveButton_Click(sender, null);
            }
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the settings window
            SettingsWindow w = new SettingsWindow();
            w.ShowDialog();
        }

        private void findReplaceTagsButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the currently open file in case it needs to be renamed
            videoPlayer.UnloadVideo();

            // Show a window for finding/replacing
            var findReplaceWindow = new FindReplaceTagsWindow(Directory.GetCurrentDirectory(), allKnownTags);
            findReplaceWindow.ShowDialog();

            // Refresh the UI
            UpdateCurrentDirectory();
        }

        private void allTagsListbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Add the selected tag to the filter, if it isn't there already
            string selectedTag = allTagsListbox.SelectedItem as string;
            string[] filterTags = tagFilterTextbox.Text.Split(' ');

            if (!filterTags.Contains(selectedTag))
                tagFilterTextbox.Text += " " + selectedTag;
        }

        /// <summary>
        /// Allows the user to drag a tag and drop it into the tags textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void allTagsListbox_MouseMove(object sender, MouseEventArgs e)
        {
            // Only go on if we're left clicking and the mouse is over a listbox item
            IInputElement item = allTagsListbox.InputHitTest(e.GetPosition(allTagsListbox));
            bool mouseOverItem = item is TextBlock;

            if (e.LeftButton != MouseButtonState.Pressed || !mouseOverItem)
                return;

            // Start dragging and dropping.
            DragDrop.DoDragDrop(allTagsListbox, allTagsListbox.SelectedItem, DragDropEffects.Copy);
        }

        private void tagsBox_PreviewDrop(object sender, DragEventArgs e)
        {
            // Don't let the default drag-and-drop stuff happen
            e.Handled = true;

            // Don't do anything if the dragged data is not a string
            if (!e.Data.GetDataPresent(typeof(string)))
                return;

            // Add the dropped tag to the textbox if it's not there already
            string tag = (string)e.Data.GetData(typeof(string));
            string[] existingTags = tagsBox.Text.Split(' ', '\n', '\r');

            if (existingTags.Contains(tag))
                return;

            // Add a newline if there isn't one already
            if (tagsBox.Text.Length > 0 && !tagsBox.Text.EndsWith("\n"))
                tagsBox.Text += '\n';

            tagsBox.Text += tag;
        }


    }
}
