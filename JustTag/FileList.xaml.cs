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

namespace JustTag
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

        public FileSystemInfo SelectedItem
        {
            set { list.SelectedItem = value; }
            get { return (FileSystemInfo)list.SelectedItem; }
        }

        public IList SelectedItems
        {
            get { return list.SelectedItems; }
        }

        public int SelectedIndex
        {
            set { list.SelectedIndex = value; }
            get { return list.SelectedIndex; }
        }

        #endregion

        public IEnumerable<FileSystemInfo> ItemsSource
        {
            get { return m_itemsSource; }
            set
            {
                m_itemsSource = value;
                UpdateItems();
            }
        }
        private IEnumerable<FileSystemInfo> m_itemsSource;


        public FileList()
        {
            InitializeComponent();
        }


        // Interface

        public void ScrollIntoView(FileSystemInfo item)
        {
            list.ScrollIntoView(item);
        }


        // Misc methods

        private void UpdateItems()
        {
            // Add all the items to the listbox as panels
            List<DockPanel> itemPanels = new List<DockPanel>();

            foreach (FileSystemInfo item in m_itemsSource)
            {
                // Create and configure the panel
                DockPanel itemPanel = new DockPanel();

                itemPanel.LastChildFill = true;
                itemPanels.Add(itemPanel);

                // TODO: Add the icon instead of a button
                Button icon = new Button();
                icon.Content = "";
                icon.Width = 10;
                icon.Height = 10;

                itemPanel.Children.Add(icon);

                // Add the label
                Label itemLabel = new Label();
                itemLabel.Content = item.Name;

                DockPanel.SetDock(itemLabel, Dock.Left);
                itemPanel.Children.Add(itemLabel);
            }

            // Put all the panels into the listbox
            list.ItemsSource = itemPanels;
        }

    }
}
