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
using System.Collections;
using System.IO;
using JustTag.Tagging;

namespace JustTag.Controls.FileBrowser
{
    /// <summary>
    /// Interaction logic for FileList.xaml
    /// </summary>
    public partial class FileList : UserControl
    {
        #region Things shared with ListBox
        public event SelectionChangedEventHandler SelectionChanged
        {
            add { list.SelectionChanged += value; }
            remove { list.SelectionChanged -= value; }
        }

        public TaggedFilePath SelectedItem
        {
            set => list.SelectedItem = value;
            get => ((FileListItem)list.SelectedItem)?.file;
        }

        public IList SelectedItems => list.SelectedItems;

        public int SelectedIndex
        {
            set => list.SelectedIndex = value;
            get => list.SelectedIndex;
        }

        #endregion

        public IEnumerable<TaggedFilePath> ItemsSource
        {
            get => m_itemsSource;
            set
            {
                m_itemsSource = value;
                UpdateItems();
            }
        }
        private IEnumerable<TaggedFilePath> m_itemsSource;


        public FileList()
        {
            InitializeComponent();
        }


        // Interface

        public void ScrollIntoView(TaggedFilePath item)
        {
            list.ScrollIntoView(item);
        }


        // Misc methods

        private void UpdateItems()
        {
            // Add all the items to the listbox as panels
            List<FileListItem> itemControls = new List<FileListItem>();

            foreach (TaggedFilePath item in m_itemsSource)
                itemControls.Add(new FileListItem(item));

            // Put all the panels into the listbox
            list.ItemsSource = itemControls;
        }

    }
}
