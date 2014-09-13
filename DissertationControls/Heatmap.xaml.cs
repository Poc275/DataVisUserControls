using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace DissertationControls
{
    public sealed partial class Heatmap : UserControl
    {
        const double FIRST_ROW_WIDTH = 200;
        const int DEFAULT_NUM_DATA_CLASSES = 9;
        
        StorageFolder _folder;
        string _fileName;
        ColourScheme _colourScheme;
        DatasetModel _datasetModel;

        string _id;
        int _idIndex;

        string[] _columns;
        int[] _columnIndexes;

        double _screenWidth;
        double _cellWidth;

        ObservableCollection<ListViewItem> _columnHeadingsListViewItems;
        string _reorderedItemContent;

        ObservableCollection<ListViewItem> _dataGrid;
        int _reorderedRowIndex;

        List<ListViewItem> _filteredItems;

        public Heatmap()
        {
            this.InitializeComponent();
            _datasetModel = new DatasetModel();
            _columnHeadingsListViewItems = new ObservableCollection<ListViewItem>();
            _dataGrid = new ObservableCollection<ListViewItem>();
            _filteredItems = new List<ListViewItem>();
            _colourScheme = ColourScheme.Instance;
        }


        // PROPERTIES
        public StorageFolder Folder
        {
            get { return _folder; }
            set { _folder = value; }
        }

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string[] Columns
        {
            get { return _columns; }
            set
            {
                _columns = value;
                _columnIndexes = new int[_columns.Length];
            }
        }


        private async void Heatmap_Loaded(object sender, RoutedEventArgs e)
        {
            if (await CheckPropertiesAreSet())
            {
                _screenWidth = dataListView.ActualWidth - FIRST_ROW_WIDTH;

                // setup colour scheme combo box with default values
                colourSchemeComboBox.DataContext = _colourScheme.GetColourSchemes(DEFAULT_NUM_DATA_CLASSES, "Sequential single hue");
                colourSchemeComboBox.SelectedValue = "Blue";

                // Assign event handlers
                numDataClassesComboBox.SelectionChanged += numDataClassesComboBox_SelectionChanged;
                colourSchemeTypeComboBox.SelectionChanged += colourSchemeTypeComboBox_SelectionChanged;
                colourSchemeComboBox.SelectionChanged += colourSchemeComboBox_SelectionChanged;

                Initialise();
            }
        }


        private async void Initialise()
        {
            MessageDialog errorDialog = null;
            
            try
            {
                await _datasetModel.ReadCsvFile(this.Folder, this.FileName);
            }
            catch (FileNotFoundException ex)
            {
                errorDialog = new MessageDialog(ex.Message);
            }
            catch (Exception ex)
            {
                errorDialog = new MessageDialog(ex.Message);
            }

            if (errorDialog != null)
            {
                await errorDialog.ShowAsync();
            }
            else
            {
                _idIndex = Array.IndexOf(_datasetModel.ColumnHeadings, this.Id);
                for (int i = 0; i < this.Columns.Length; i++)
                {
                    _columnIndexes[i] = Array.IndexOf(_datasetModel.ColumnHeadings, this.Columns[i]);
                }

                if (await _datasetModel.GetAxesMaxMin(this.Columns, _columnIndexes))
                {
                    SetupColumnHeadings();
                }
            }
        }


        private async Task<bool> CheckPropertiesAreSet()
        {
            bool allPropertiesSet = true;
            MessageDialog errorDialog = null;
            
            if (String.IsNullOrEmpty(this.Id))
            {
                allPropertiesSet = false;
                errorDialog = new MessageDialog("Id property is not set", "Property not set");
            }
            else if (this.Columns == null)
            {
                allPropertiesSet = false;
                errorDialog = new MessageDialog("Columns property is not set", "Property not set");
            }

            if (errorDialog != null)
            {
                await errorDialog.ShowAsync();
            }

            return allPropertiesSet;
        }

        private void SetupColumnHeadings()
        {
            _cellWidth = _screenWidth / this.Columns.Length;
            
            for (int i = 0; i < this.Columns.Length; i++)
            {
                ListViewItem item = new ListViewItem();
                item.Margin = new Thickness(0);
                item.Padding = new Thickness(0);
                item.Width = _cellWidth;
                item.HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                item.Content = _datasetModel.ColumnHeadings[_columnIndexes[i]];
                item.PointerPressed += columnHeading_PointerPressed;

                ToolTip tt = new ToolTip();
                tt.Content = _datasetModel.ColumnHeadings[_columnIndexes[i]];
                tt.Placement = Windows.UI.Xaml.Controls.Primitives.PlacementMode.Mouse;
                ToolTipService.SetToolTip(item, tt);

                _columnHeadingsListViewItems.Add(item);
            }

            // offset the position of the headings ListView to position over the cells correctly
            columnHeadingsListView.Margin = new Thickness(FIRST_ROW_WIDTH, 0, 0, 0);

            columnHeadingsListView.ItemsSource = _columnHeadingsListViewItems;
            _columnHeadingsListViewItems.CollectionChanged += columnHeadingsListViewItems_CollectionChanged;
            Draw();
        }


        private void Draw()
        {
            _dataGrid.CollectionChanged -= dataGrid_CollectionChanged;
            _dataGrid.Clear();

            // get colour scheme values from combobox selections
            ComboBoxItem nDataClassesSelection = numDataClassesComboBox.SelectedItem as ComboBoxItem;
            int nDataClasses = int.Parse(nDataClassesSelection.Content.ToString());
            byte[][] defaultColourScheme = _colourScheme.GetRGBColours(colourSchemeComboBox.SelectedItem.ToString(), nDataClasses);

            for (int i = 0; i < _datasetModel.Dataset.Count; i++)
            {
                string[] row = _datasetModel.Dataset[i];
                ListViewItem item = BuildRowStructure(_cellWidth, row[_idIndex]);
                Grid rowGrid = item.Content as Grid;

                for (int j = 0; j < this.Columns.Length; j++)
                {
                    int currentColumn = _columnIndexes[j];
                    string columnTitle = _datasetModel.ColumnHeadings[currentColumn];

                    // get max and min for the column
                    double[] minMax = _datasetModel.GetAxisMinMaxValues(columnTitle);
                    double min = minMax[0];
                    double max = minMax[1];

                    double cellValue = double.Parse(row[currentColumn]);
                    double linearTransform = (cellValue - min) / (max - min) * (nDataClasses - 1);
                    int colourIndex = (int)Math.Truncate(linearTransform);
                    byte[] colourRGB = defaultColourScheme[colourIndex];

                    Rectangle cell = new Rectangle();
                    cell.Stroke = new SolidColorBrush(Colors.LightGray);
                    cell.StrokeThickness = 1;
                    cell.Fill = new SolidColorBrush(Color.FromArgb(255, colourRGB[0], colourRGB[1], colourRGB[2]));

                    ToolTip tt = new ToolTip();
                    tt.Content = row[currentColumn];
                    tt.Placement = Windows.UI.Xaml.Controls.Primitives.PlacementMode.Mouse;
                    ToolTipService.SetToolTip(cell, tt);

                    rowGrid.Children.Add(cell);
                    // add to column + 1 to account for the row title
                    Grid.SetColumn(cell, j + 1);
                }
                _dataGrid.Add(item);
            }

            _dataGrid.CollectionChanged += dataGrid_CollectionChanged;
            dataListView.ItemsSource = _dataGrid;

            // check for filtered items
            if (_filteredItems.Count > 0)
            {
                Filter();
            }
        }


        private ListViewItem BuildRowStructure(double cellWidth, string id)
        {
            ListViewItem item = new ListViewItem();
            item.Margin = new Thickness(0);
            item.Padding = new Thickness(0);
            Grid itemGrid = new Grid();

            // the 1st column is the row Id and needs to be larger than
            // the other cells so it can hold the text
            ColumnDefinition idCol = new ColumnDefinition();
            idCol.Width = new GridLength(FIRST_ROW_WIDTH);
            itemGrid.ColumnDefinitions.Add(idCol);

            // add a text block to the first column and set it to the row id
            TextBlock titleTextBlock = new TextBlock();
            titleTextBlock.Text = id;
            titleTextBlock.TextTrimming = TextTrimming.WordEllipsis;
            ToolTip tt = new ToolTip();
            tt.Content = id;
            tt.Placement = Windows.UI.Xaml.Controls.Primitives.PlacementMode.Mouse;
            ToolTipService.SetToolTip(titleTextBlock, tt);
            itemGrid.Children.Add(titleTextBlock);
            Grid.SetColumn(titleTextBlock, 0);

            // the rest of the columns are the data cells themselves
            for (int i = 0; i < this.Columns.Length; i++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(cellWidth);
                itemGrid.ColumnDefinitions.Add(col);
            }

            item.Content = itemGrid;
            return item;
        }


        private void Filter()
        {
            foreach (ListViewItem filteredItem in _filteredItems)
            {
                // get name of filtered item
                Grid filteredRowGrid = filteredItem.Content as Grid;
                string filteredTitle = (filteredRowGrid.Children.ElementAt(0) as TextBlock).Text;
                
                // find it in the collection and filter it
                foreach (ListViewItem item in _dataGrid)
                {
                    Grid rowGrid = item.Content as Grid;
                    string title = (rowGrid.Children.ElementAt(0) as TextBlock).Text;

                    if (String.Equals(filteredTitle, title))
                    {
                        item.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                }
            }
        }


        private void columnHeadingsListViewItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    _columnHeadingsListViewItems[e.NewStartingIndex].Content = _reorderedItemContent;

                    // rearrange column indexes
                    for (int i = 0; i < this.Columns.Length; i++)
                    {
                        string columnTitle = _columnHeadingsListViewItems[i].Content.ToString();
                        _columnIndexes[i] = Array.IndexOf(_datasetModel.ColumnHeadings, columnTitle);
                    }

                    // Redraw the graph based on the rearranged order
                    Draw();
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    _reorderedItemContent = (e.OldItems[0] as ListViewItem).Content.ToString();
                    break;

                default:
                    break;
            }
        }


        private void dataGrid_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    _datasetModel.SwapDataEntries(_reorderedRowIndex, e.NewStartingIndex);
                    Draw();
                    break;
                
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    _reorderedRowIndex = e.OldStartingIndex;
                    break;

                default:
                    break;
            }
        }


        private void columnHeading_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // on pointer pressed, sort the data in descending order for that column
            ListViewItem columnHeading = sender as ListViewItem;
            string columnTitle = columnHeading.Content.ToString();

            // get index
            int columnIndex = Array.IndexOf(_datasetModel.ColumnHeadings, columnTitle);

            // sort the data by that index and redraw
            _datasetModel.ReorderDatasetByDescending(columnIndex);
            Draw();
        }


        // colour scheme combo boxes event handlers
        private void numDataClassesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem colourSchemeType = colourSchemeTypeComboBox.SelectedItem as ComboBoxItem;
            
            if (String.Equals(colourSchemeType.Content.ToString(), "Diverging"))
            {
                // the possible diverging colour schemes change depending on the number of classes
                // get the number of classes and update the combo box options
                ComboBox cb = sender as ComboBox;
                ComboBoxItem cbi = (ComboBoxItem)cb.SelectedValue;
                int nClasses = int.Parse(cbi.Content.ToString());

                List<string> colourSchemes = _colourScheme.GetColourSchemes(nClasses, "Diverging");
                colourSchemeComboBox.DataContext = colourSchemes;
                colourSchemeComboBox.SelectedValue = colourSchemes[0];
            }

            Draw();
        }

        private void colourSchemeTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ComboBoxItem cbi = cb.SelectedValue as ComboBoxItem;
            string type = cbi.Content.ToString();
            ComboBoxItem numClassesSelection = (ComboBoxItem)numDataClassesComboBox.SelectedValue;
            int nClasses = int.Parse(numClassesSelection.Content.ToString());

            // the diverging colour scheme has more data class options, enable them here
            if (String.Equals(type, "Diverging"))
            {
                tenDataClassOption.IsEnabled = true;
                elevenDataClassOption.IsEnabled = true;
            }
            else
            {
                tenDataClassOption.IsEnabled = false;
                elevenDataClassOption.IsEnabled = false;

                // the sequential colour schemes are limited to 9 classes, if changing back from diverging
                // the selection could potentially be > 9, which will result in an empty colour scheme
                // the event handler is removed and reattached to prevent unnecessary colour scheme updates
                // whilst the selection is being updated
                if (nClasses > 9)
                {
                    numDataClassesComboBox.SelectionChanged -= numDataClassesComboBox_SelectionChanged;
                    nClasses = 9;
                    numDataClassesComboBox.SelectedIndex = 6;
                    numDataClassesComboBox.SelectionChanged += numDataClassesComboBox_SelectionChanged;
                }
            }

            List<string> colourSchemes = _colourScheme.GetColourSchemes(nClasses, type);
            colourSchemeComboBox.DataContext = colourSchemes;

            // Select a colour scheme now that the selection is null
            // (this fires a colourSchemeComboBox_SelectionChanged event)
            colourSchemeComboBox.SelectedValue = colourSchemes[0];
        }

        private void colourSchemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if ((sender as ComboBox).SelectedValue != null)
            {
                Draw();
            }
        }


        private void dataListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filterButton.IsEnabled = dataListView.SelectedItems.Count > 0;
        }


        private void filterButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (object selection in dataListView.SelectedItems)
            {
                ListViewItem selectedItem = selection as ListViewItem;
                selectedItem.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                _filteredItems.Add(selectedItem);
            }

            dataListView.SelectedItems.Clear();
            filterButton.IsEnabled = false;
            resetFilterButton.IsEnabled = true;
        }


        private void resetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListViewItem item in _filteredItems)
            {
                item.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            _filteredItems.Clear();
            resetFilterButton.IsEnabled = false;
        }


        private void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            foreach (ListViewItem item in _dataGrid)
            {
                Grid rowGrid = item.Content as Grid;
                TextBlock titleTextBlock = rowGrid.Children.ElementAt(0) as TextBlock;

                // an empty search box acts as a 'reset' button, clear all selections
                if (string.IsNullOrWhiteSpace(args.QueryText))
                {
                    item.IsSelected = false;
                }
                else
                {
                    item.IsSelected = titleTextBlock.Text.IndexOf(args.QueryText, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
        }

    }
}