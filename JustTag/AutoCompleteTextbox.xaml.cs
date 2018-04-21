﻿using System;
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

        private void MoveDropdownToCursor()
        {
            autoCompleteDropdown.Margin = GetCursorPos();
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


        // Event Handlers

        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Set the current word to the last word in the textbox
            // If there are no words, it's the empty string
            // TODO: use the cursor position to find the current word
            string[] words = textbox.Text.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            currentWord = "";
            if (words.Length != 0)
                currentWord = words[words.Length - 1];

            // Fill the list box with the words that match it.
            Regex wordRegex = new Regex("^" + currentWord + ".*");

            var matchingWords = from word in autoCompletionSource
                                where wordRegex.IsMatch(word)
                                select word;

            autoCompleteDropdown.ItemsSource = matchingWords;
            MoveDropdownToCursor();

            // Resize the dropdown list so it matches the width of the suggestions
            string longestStr = GetLongestString(matchingWords);
            Size textSize = Utils.MeasureTextSize
            (
                longestStr,
                autoCompleteDropdown.FontFamily,
                autoCompleteDropdown.FontStyle,
                autoCompleteDropdown.FontWeight,
                autoCompleteDropdown.FontStretch,
                autoCompleteDropdown.FontSize
            );

            autoCompleteDropdown.Width = textSize.Width + 15;
        }
    }
}
