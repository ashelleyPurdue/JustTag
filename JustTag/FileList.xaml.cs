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

        public IEnumerable<FileSystemInfo> ItemsSource
        {
            get { return list.ItemsSource as IEnumerable<FileSystemInfo>; }
            set { list.ItemsSource = value; }
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

        public FileList()
        {
            InitializeComponent();
        }

        public void ScrollIntoView(FileSystemInfo item)
        {
            list.ScrollIntoView(item);
        }
    }
}
