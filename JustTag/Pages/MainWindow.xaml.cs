﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Text.RegularExpressions;

namespace JustTag.Pages
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

            // Populate the sort-by combo box
            sortByBox.ItemsSource = Enum.GetValues(typeof(SortMethod));
            sortByBox.SelectedIndex = 0;
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


            // Parse the tag filter
            TagFilter filter = new TagFilter(tagFilterTextbox.Text);

            // Get all files/folders that match the filter
            var entries = currentDir.EnumerateFileSystemInfos();
            var files = from FileSystemInfo e in entries
                        where filter.Matches(e.Name)
                        orderby (e is DirectoryInfo) descending    // Make folders appear first, for easier navigating
                        select e;

            var fileSource = files.ToList();

            // Shuffle the files if "shuffle" is ticked
            if ((bool)shuffleCheckbox.IsChecked)
                fileSource = Utils.ShuffleList(fileSource);

            folderContentsBox.ItemsSource = fileSource;

            // Put them in the list of all files you can flip through in full-screen mode
            Fullscreen.browsableFiles = (from f in fileSource
                                         where f is FileInfo
                                         select (FileInfo)f).ToArray();

            // Record all encountered tags in the "all known tags" list.
            foreach (FileSystemInfo file in files)
            {
                TaggedFileName fname = new TaggedFileName(file.Name);

                foreach (string tag in fname.tags)
                    allKnownTags.Add(tag);
            }

            // Update the known tags listbox
            // Sort it in alphabetical order first
            List<string> knownTagsList = allKnownTags.ToList();
            knownTagsList.Sort();
            allTagsListbox.ItemsSource = knownTagsList;
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
            FileSystemInfo selectedItem = folderContentsBox.SelectedItem as FileSystemInfo;

            // If it's a shortcut, look up its target
            if (selectedItem.Extension.ToLower() == ".lnk")
                selectedItem = Utils.GetShortcutTarget(selectedItem);

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
            if (filePreviewer.IsOpening)
                return;

            // Don't do anything if selection is null
            if (folderContentsBox.SelectedItem == null)
                return;

            // If the selected item is a shortcut, resolve it.
            FileSystemInfo selectedItem = folderContentsBox.SelectedItem as FileSystemInfo;
            if (selectedItem.Extension.ToLower() == ".lnk")
                selectedItem = Utils.GetShortcutTarget(selectedItem);

            // Show the file preview
            filePreviewer.OpenPreview(selectedItem);

            // Enable the tag box and update it with this file's tags
            // NOTE: This affects the shortcut itself, not its target.  This is intentional.
            TaggedFileName fname = new TaggedFileName(selectedItem.Name);

            StringBuilder builder = new StringBuilder();
            foreach (string t in fname.tags)
                builder.AppendLine(t);

            tagsBox.IsEnabled = true;
            tagsBox.Text = builder.ToString();

            // Hide the save button
            tagSaveButton.Visibility = Visibility.Hidden;
        }

        private void tagsBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Make the background red and disable the save button if any of the tags are invalid
            string[] tags = tagsBox.Text.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string tag in tags)
            {
                if (!Utils.IsTagValid(tag))
                {
                    tagsBox.Background = Brushes.Red;
                    tagSaveButton.Visibility = Visibility.Hidden;

                    return;
                }
            }

            // Show the save button
            tagsBox.Background = Brushes.White;
            tagSaveButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Updates the selected file's tags
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void tagSaveButton_Click(object sender, RoutedEventArgs e)
        {
            FileSystemInfo selectedItem = folderContentsBox.SelectedItem;
            TaggedFileName fname = new TaggedFileName(selectedItem.Name);

            // Parse the tags into a list
            string[] tags = tagsBox.Text.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Change the tags
            fname.tags.Clear();
            fname.tags.AddRange(tags);

            // Remember the selected index so we can scroll back to it
            // after saving
            int selectedIndex = folderContentsBox.SelectedIndex;

            // Rename the file
            try
            {
                await filePreviewer.ClosePreview();
                Utils.ChangeFileTags(selectedItem, fname);
            }
            catch (IOException err)
            {
                MessageBox.Show("ERROR: Could not rename file." + err.Message);
            }
            
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

        private async void findReplaceTagsButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the currently open file in case it needs to be renamed
            await filePreviewer.ClosePreview();

            // Show a window for finding/replacing
            var findReplaceWindow = new FindReplaceTagsWindow(Directory.GetCurrentDirectory(), allKnownTags);
            findReplaceWindow.ShowDialog();

            // Refresh the UI
            UpdateCurrentDirectory();
        }

        private async void deleteTagButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the currently open file in case it needs to be renamed
            await filePreviewer.ClosePreview();

            // Show the window
            var toolWindow = new DeleteTagWindow(Directory.GetCurrentDirectory(), allKnownTags);
            toolWindow.ShowDialog();

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
