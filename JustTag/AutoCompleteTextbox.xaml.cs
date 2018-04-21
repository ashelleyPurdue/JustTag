using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace JustTag
{
    /// <summary>
    /// Interaction logic for AutoCompleteTextbox.xaml
    /// </summary>
    public partial class AutoCompleteTextbox : UserControl
    {
        public event TextChangedEventHandler TextChanged
        {
            add { textbox.TextChanged += value; }
            remove { textbox.TextChanged -= value; }
        }

        public bool AcceptsReturn
        {
            get { return textbox.AcceptsReturn; }
            set { textbox.AcceptsReturn = value; }
        }

        public string Text
        {
            get { return textbox.Text; }
            set { textbox.Text = value; }
        }

        public IEnumerable<string> autoCompletionSource;
        private string currentWord;


        public AutoCompleteTextbox()
        {
            InitializeComponent();
        }


        // Misc methods

        private Thickness GetCursorPos()
        {
            // Returns the relative position of the typing cursor

            // Get all the text before the cursor so we can measure it.
            string earlierText = textbox.Text.Substring(0, textbox.CaretIndex);

            // Measure it
            FormattedText formattedText = Utils.GetFormattedText(earlierText, textbox);
            formattedText.MaxTextWidth = textbox.ActualWidth;

            double width = formattedText.Width;
            double height = formattedText.Height;

            return new Thickness(formattedText.Width, formattedText.Height, 0, 0);
        }

        private string GetLongestString(IEnumerable<string> strings)
        {
            // Returns the longest string in the collection
            string longest = "";

            foreach (string s in strings)
            {
                if (s.Length > longest.Length)
                    longest = s;
            }

            return longest;
        }

        private string GetWordAt(string str, int pos)
        {
            // Returns the word at the given cursor position.
            // Returns the empty string if str is empty

            if (str == "")
                return "";

            int currPos = pos;

            // HACK: if the pos is beyond the string, move it to
            // the last character.  This is to accomodate situations
            // where the typing cursor is at the very end of the string.
            if (currPos >= str.Length)
                currPos = str.Length - 1;

            // Rewind until we reach whitespace
            while (true)
            {
                if (currPos == 0)
                    break;

                if (Char.IsWhiteSpace(str[currPos - 1]))
                    break;

                currPos--;
            }


            // Step forward until we reach the end of the word
            StringBuilder builder = new StringBuilder();
            while (currPos < str.Length && !Char.IsWhiteSpace(str[currPos]))
            {
                builder.Append(str[currPos]);
                currPos++;
            }

            return builder.ToString();
        }


        // Event Handlers

        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Update the current word
            currentWord = GetWordAt(textbox.Text, textbox.CaretIndex);

            // Show the suggestion box
            suggestionBox.Visibility = Visibility.Visible;

            // Fill the suggestion box with the words that match it.
            Regex wordRegex = new Regex("^" + currentWord + ".*");

            var matchingWords = from word in autoCompletionSource
                                where wordRegex.IsMatch(word)
                                select word;

            suggestionBox.ItemsSource = matchingWords;
            suggestionBox.Margin = GetCursorPos();       // Move the menu to the cursor pos

            // Resize the dropdown list so it matches the width of the suggestions
            string longestStr = GetLongestString(matchingWords);

            FormattedText textSize = Utils.GetFormattedText(longestStr, suggestionBox);
            suggestionBox.Width = textSize.Width + 15;
        }

        private void textbox_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            // Hide the suggestion box when losing keyboard focus
            suggestionBox.Visibility = Visibility.Collapsed;
        }
    }
}
