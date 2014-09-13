using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace DissertationControls
{
    public sealed partial class ParallelCoords : UserControl
    {
        const double RANGE_SLIDER_WIDTH = 12.0;
        const int DEFAULT_NUM_DATA_CLASSES = 3;
        
        StorageFolder _folder;
        string _fileName;
        DatasetModel _datasetModel;

        double _canvasWidth;
        double _canvasHeight;
        ColourScheme _colourScheme;
        List<string> _distinctCategories;

        string _id;
        int _idIndex;

        string[] _axes;
        int[] _axesIndexes;

        string _colourValue;
        int _colourValueIndex;

        ObservableCollection<ListViewItem> _axesCollection;
        string _reorderedItemContent;
        double _xSeparation;

        List<RangeSlider> _rangeSliders;
        List<ParallelCoordsPolyline> _polylines;

        public ParallelCoords()
        {
            this.InitializeComponent();
            _datasetModel = new DatasetModel();
            _axesCollection = new ObservableCollection<ListViewItem>();
            _rangeSliders = new List<RangeSlider>();
            _polylines = new List<ParallelCoordsPolyline>();
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

        public string[] Axes
        {
            get { return _axes; }
            set
            {
                _axes = value;
                // Initialise the indexes array and min/max arrays now we know how many axes
                _axesIndexes = new int[_axes.Length];
            }
        }

        public string ColourValue
        {
            get { return _colourValue; }
            set { _colourValue = value; }
        }


        private async void ParallelCoords_Loaded(object sender, RoutedEventArgs e)
        {
            // check all mandatory properties are set
            if (await CheckPropertiesAreSet())
            {
                _canvasWidth = canvas.ActualWidth;
                _canvasHeight = canvas.ActualHeight;
                // Calculate the x distance between each axis (equi-spaced,
                // starting at 0 which is why we reduce the number of axes by 1)
                _xSeparation = _canvasWidth / (_axes.Length - 1);
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
                // generate property indexes
                _idIndex = Array.IndexOf(_datasetModel.ColumnHeadings, this.Id);
                _colourValueIndex = Array.IndexOf(_datasetModel.ColumnHeadings, this.ColourValue);

                for (int i = 0; i < this.Axes.Length; i++)
                {
                    _axesIndexes[i] = Array.IndexOf(_datasetModel.ColumnHeadings, this.Axes[i]);
                }

                if (await _datasetModel.GetAxesMaxMin(this.Axes, _axesIndexes))
                {
                    if (await SetupColourComboBoxes())
                    {
                        if (!_datasetModel.IsColourQualitative)
                        {
                            // get colour min max values if sequential
                            if (await _datasetModel.GetColourMaxMin(_colourValueIndex))
                            {
                                SetupAxes();
                            }
                        }
                        else
                        {
                            SetupAxes();
                        }
                    }
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
            else if (String.IsNullOrEmpty(this.ColourValue))
            {
                allPropertiesSet = false;
                errorDialog = new MessageDialog("Colour value property is not set", "Property not set");
            }
            else if (this.Axes == null)
            {
                allPropertiesSet = false;
                errorDialog = new MessageDialog("Axes property is not set", "Property not set");
            }

            if (errorDialog != null)
            {
                await errorDialog.ShowAsync();
            }

            return allPropertiesSet;
        }


        private async Task<bool> SetupColourComboBoxes()
        {
            bool success = true;
            
            if (_datasetModel.ColourValueCheck(_colourValueIndex))
            {
                int nClassesValue = 0;

                // colour values are qualitative
                // hide number of data classes and colour scheme type combo boxes
                // as they are not required, the values have been automatically derived from the data
                numDataClassesTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                numDataClassesComboBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                colourSchemeTypeTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                colourSchemeTypeComboBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                _distinctCategories = _datasetModel.Dataset.Select(o => o[_colourValueIndex]).Distinct().ToList();
                nClassesValue = _distinctCategories.Count();
                if (nClassesValue > 12)
                {
                    // too many categories for a colorbrewer scheme
                    success = false;
                    await new MessageDialog("The colour scheme supports a maximum of 12 categories for optimum visualisation refinement",
                        "More than 12 distinct categories").ShowAsync();
                }

                // set default colour scheme selections
                colourSchemeTypeComboBox.SelectedItem = "Qualitative";
                colourSchemeComboBox.DataContext = _colourScheme.GetColourSchemes(nClassesValue, "Qualitative");
                List<string> items = (List<string>)colourSchemeComboBox.ItemsSource;
                colourSchemeComboBox.SelectedItem = items[0];
            }
            else
            {
                // colour values are not qualitative
                List<string> colourSchemeTypes = new List<string>();
                colourSchemeTypes.Add("Sequential single hue");
                colourSchemeTypes.Add("Sequential multi hue");
                colourSchemeTypes.Add("Diverging");
                colourSchemeTypeComboBox.DataContext = colourSchemeTypes;

                // set default colour scheme selections
                colourSchemeTypeComboBox.SelectedValue = "Sequential single hue";
                colourSchemeComboBox.DataContext = _colourScheme.GetColourSchemes(DEFAULT_NUM_DATA_CLASSES, "Sequential single hue");
                colourSchemeComboBox.SelectedValue = "Grey";
            }

            return success;
        }


        // Method that sets up the axes and draws them
        private void SetupAxes()
        {
            for (int i = 0; i < this.Axes.Length; i++)
            {
                ListViewItem item = new ListViewItem();
                item.Content = _datasetModel.ColumnHeadings[_axesIndexes[i]];
                item.Margin = new Thickness(0);
                item.Padding = new Thickness(0);

                // position axis labels correctly
                if (i == 0)
                {
                    // for the 1st heading, we want to align to the left
                    item.Width = _xSeparation / 2;
                    item.HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
                }
                else if (i == this.Axes.Length - 1)
                {
                    // for the last heading, we align to the right
                    item.Width = _xSeparation / 2;
                    item.HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Right;
                }
                else
                {
                    item.Width = _xSeparation;
                    item.HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                }

                _axesCollection.Add(item);
            }

            // bind axes to list view for display
            axesRow.ItemsSource = _axesCollection;

            // Now the axes collection has been created, subscribe to the collection changed event so
            // we are notified if the collection changes in the future due to a drag and drop operation
            _axesCollection.CollectionChanged += _axesCollection_CollectionChanged;

            AddRangeSliders();
            Draw();
        }


        private void AddRangeSliders()
        {
            _rangeSliders.Clear();

            for (int i = 0; i < this.Axes.Length; i++)
            {
                RangeSlider rangeSlider = new RangeSlider();
                rangeSlider.Height = _canvasHeight;
                rangeSlider.Width = RANGE_SLIDER_WIDTH;
                Canvas.SetTop(rangeSlider, 0.0);

                // Centralise positions
                if (i == 0)
                {
                    Canvas.SetLeft(rangeSlider, 0.0);
                }
                else if (i == this.Axes.Length - 1)
                {
                    Canvas.SetLeft(rangeSlider, (i * _xSeparation) - rangeSlider.Width);
                }
                else
                {
                    Canvas.SetLeft(rangeSlider, (i * _xSeparation) - (rangeSlider.Width / 2));
                }

                int index = _axesIndexes[i];
                string axisName = _datasetModel.ColumnHeadings[index];
                double[] minMax = _datasetModel.GetAxisMinMaxValues(axisName);

                rangeSlider.Max = minMax[1];
                rangeSlider.Min = minMax[0];

                rangeSlider.UpperValueChangedEvent += rangeSlider_UpperValueChangedEvent;
                rangeSlider.LowerValueChangedEvent += rangeSlider_LowerValueChangedEvent;

                _rangeSliders.Add(rangeSlider);
                canvas.Children.Add(rangeSlider);
            }
        }


        private void Draw()
        {
            StringBuilder sb = new StringBuilder();
            byte[][] defaultColourScheme;
            int nClassesValue = 3;
            string colourSchemeComboBoxSelection = colourSchemeComboBox.SelectedValue.ToString();

            if (!_datasetModel.IsColourQualitative)
            {
                ComboBoxItem nClassesComboBoxSelection = (ComboBoxItem)numDataClassesComboBox.SelectedValue;
                nClassesValue = int.Parse(nClassesComboBoxSelection.Content.ToString());
                defaultColourScheme = _colourScheme.GetRGBColours(colourSchemeComboBoxSelection, nClassesValue);
            }
            else
            {
                _distinctCategories = _datasetModel.Dataset.Select(o => o[_colourValueIndex]).Distinct().ToList();
                nClassesValue = _distinctCategories.Count();
                defaultColourScheme = _colourScheme.GetRGBColours(colourSchemeComboBoxSelection, nClassesValue);
            }

            foreach (string[] row in _datasetModel.Dataset)
            {
                ParallelCoordsPolyline line = new ParallelCoordsPolyline();
                line.ColourValue = row[_colourValueIndex];
                sb.AppendLine(row[_idIndex]);

                // Apply line colour
                if (_datasetModel.IsColourQualitative)
                {
                    int index = _distinctCategories.IndexOf(row[_colourValueIndex]);
                    line.LineColour = defaultColourScheme[index];
                }
                else
                {
                    double colourValue = double.Parse(row[_colourValueIndex]);
                    double linearTransform = (colourValue - _datasetModel.ColourMinMax[0]) /
                        (_datasetModel.ColourMinMax[1] - _datasetModel.ColourMinMax[0]) * (nClassesValue - 1);
                    int colourIndex = (int)Math.Truncate(linearTransform);
                    line.LineColour = defaultColourScheme[colourIndex];
                }

                PointCollection points = new PointCollection();
                double[] dataValues = new double[_axesIndexes.Length];
                
                for (int i = 0; i < _axesIndexes.Length; i++)
                {
                    // get value for the axis
                    double val = double.Parse(row[_axesIndexes[i]]);
                    dataValues[i] = val;

                    // get max/min values for the axis
                    string axisTitle = _datasetModel.ColumnHeadings[_axesIndexes[i]];
                    double[] minMax = _datasetModel.GetAxisMinMaxValues(axisTitle);

                    // get normalised value (between 0 and 1) and multiply
                    // by screen height to convert it into a screen coordinate
                    double yCoord = NormaliseValue(val, minMax[1], minMax[0]) * _canvasHeight;
                    double xCoord = _xSeparation * i;

                    points.Add(new Point(xCoord, yCoord));

                    sb.Append(_datasetModel.ColumnHeadings[_axesIndexes[i]]);
                    sb.Append(": ");
                    sb.AppendLine(val.ToString());
                }

                // colour value tooltip text
                // check to see if the colour value is already an axis
                if (Array.IndexOf(this.Axes, this.ColourValue) < 0)
                {
                    // colour value is not an axis, add to the tooltip text
                    sb.Append(_datasetModel.ColumnHeadings[_colourValueIndex]);
                    sb.Append(": ");
                    sb.Append(row[_colourValueIndex]);
                }

                line.Details = sb.ToString();
                sb.Clear();

                line.PolylinePoints = points;
                line.DataValues = dataValues;
                _polylines.Add(line);
                canvas.Children.Add(line);
            }

            // Assign event handlers at the end so events are not fired during initialisation
            numDataClassesComboBox.SelectionChanged += numDataClassesComboBox_SelectionChanged;
            colourSchemeTypeComboBox.SelectionChanged += colourSchemeTypeComboBox_SelectionChanged;
            colourSchemeComboBox.SelectionChanged += colourSchemeComboBox_SelectionChanged;
        }


        // Method that normalises the data values
        private double NormaliseValue(double val, double max, double min)
        {
            // Min and max are reversed in this equation because x0y0
            // is in the top right corner
            return (val - max) / (min - max);
        }


        private void UpdateColourScheme(byte[][] colourScheme, int nClasses)
        {
            if (_datasetModel.IsColourQualitative)
            {
                for (int i = 0; i < _polylines.Count; i++)
                {
                    int index = _distinctCategories.IndexOf(_polylines[i].ColourValue);
                    _polylines[i].LineColour = colourScheme[index];
                }
            }
            else
            {
                for (int i = 0; i < _polylines.Count; i++)
                {
                    double colourValue = double.Parse(_polylines[i].ColourValue);
                    double linearTransform = (colourValue - _datasetModel.ColourMinMax[0]) /
                        (_datasetModel.ColourMinMax[1] - _datasetModel.ColourMinMax[0]) * (nClasses - 1);
                    int colourIndex = (int)Math.Truncate(linearTransform);
                    _polylines[i].LineColour = colourScheme[colourIndex];
                }
            }
        }


        // Event that is fired when the observable collection of axes headings is reordered through drag and drop
        private void _axesCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // When the order of the collection is modified, this event is called twice, once as a remove action
            // which contains the old item and index, and again as an add action, which contains the new index
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    // copy the content over to the new location
                    _axesCollection[e.NewStartingIndex].Content = _reorderedItemContent;

                    // rearrange axes indexes correctly so we can redraw and reposition headings
                    for (int i = 0; i < this.Axes.Length; i++)
                    {
                        string axisTitle = _axesCollection[i].Content.ToString();
                        _axesIndexes[i] = Array.IndexOf(_datasetModel.ColumnHeadings, axisTitle);

                        if (i == 0)
                        {
                            _axesCollection[i].Width = _xSeparation / 2;
                            _axesCollection[i].HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
                        }
                        else if (i == this.Axes.Length - 1)
                        {
                            _axesCollection[i].Width = _xSeparation / 2;
                            _axesCollection[i].HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Right;
                        }
                        else
                        {
                            _axesCollection[i].Width = _xSeparation;
                            _axesCollection[i].HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                        }
                    }

                    // Clear canvas and polylines collection
                    canvas.Children.Clear();
                    _polylines.Clear();

                    // Remove event handlers as these will be readded in the Draw() method
                    numDataClassesComboBox.SelectionChanged -= numDataClassesComboBox_SelectionChanged;
                    colourSchemeTypeComboBox.SelectionChanged -= colourSchemeTypeComboBox_SelectionChanged;
                    colourSchemeComboBox.SelectionChanged -= colourSchemeComboBox_SelectionChanged;

                    // Redraw
                    AddRangeSliders();
                    Draw();
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    // Retrieve the old item's content and index
                    _reorderedItemContent = (e.OldItems[0] as ListViewItem).Content.ToString();
                    break;

                default:
                    break;
            }
        }


        private void rangeSlider_LowerValueChangedEvent(object sender, ValueChangedEventArgs e)
        {
            bool filter = false;
            resetRangeSlidersButton.IsEnabled = true;

            for (int i = 0; i < _polylines.Count; i++)
            {
                double[] rowValues = _polylines[i].DataValues;
                
                for (int j = 0; j < rowValues.Length; j++)
                {
                    if (rowValues[j] < _rangeSliders[j].LowerValue ||
                            rowValues[j] > _rangeSliders[j].UpperValue)
                    {
                        // value is outside range so filter
                        filter = true;
                    }
                }

                _polylines[i].Filtered = filter;

                // reset filter for the next row
                filter = false;
            }
        }

        private void rangeSlider_UpperValueChangedEvent(object sender, ValueChangedEventArgs e)
        {
            bool filter = false;
            resetRangeSlidersButton.IsEnabled = true;

            for (int i = 0; i < _polylines.Count; i++)
            {
                double[] rowValues = _polylines[i].DataValues;

                for (int j = 0; j < rowValues.Length; j++)
                {
                    if (rowValues[j] > _rangeSliders[j].UpperValue ||
                            rowValues[j] < _rangeSliders[j].LowerValue)
                    {
                        // value is outside range so filter
                        filter = true;
                    }
                }

                _polylines[i].Filtered = filter;

                // reset filter for the next row
                filter = false;
            }
        }


        // colour scheme combo box event handlers
        private void numDataClassesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ComboBoxItem cbi = (ComboBoxItem)cb.SelectedValue;
            int nClasses = int.Parse(cbi.Content.ToString());

            if (String.Equals(colourSchemeTypeComboBox.SelectedValue.ToString(), "Diverging"))
            {
                // the possible diverging colour schemes change depending on the number of classes
                List<string> colourSchemes = _colourScheme.GetColourSchemes(nClasses, "Diverging");
                colourSchemeComboBox.DataContext = colourSchemes;
                colourSchemeComboBox.SelectedValue = colourSchemes[0];
            }
            else if (colourSchemeTypeComboBox.SelectedValue != null &&
                        colourSchemeComboBox.SelectedValue != null)
            {
                string colourSchemeSelection = colourSchemeComboBox.SelectedValue.ToString();
                byte[][] rgbColourScheme = _colourScheme.GetRGBColours(colourSchemeSelection, nClasses);
                UpdateColourScheme(rgbColourScheme, nClasses);
            }
        }

        private void colourSchemeTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            string type = cb.SelectedValue.ToString();
            ComboBoxItem cbi = (ComboBoxItem)numDataClassesComboBox.SelectedValue;
            int nClasses = int.Parse(cbi.Content.ToString());

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
            // (this fires a colourSchemeComboBox selection changed event)
            colourSchemeComboBox.SelectedValue = colourSchemes[0];
        }

        private void colourSchemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedValue != null)
            {
                ComboBox cb = sender as ComboBox;
                string colourSchemeSelection = cb.SelectedValue.ToString();
                int nClasses = 0;

                // if colour scheme type is qualitative, we ignore the user setting for number of data classes
                // and use the number of distinct categories instead
                if (_datasetModel.IsColourQualitative)
                {
                    nClasses = _distinctCategories.Count();
                }
                else
                {
                    ComboBoxItem cbi = (ComboBoxItem)numDataClassesComboBox.SelectedValue;
                    nClasses = int.Parse(cbi.Content.ToString());
                }

                byte[][] rgbColourScheme = _colourScheme.GetRGBColours(colourSchemeSelection, nClasses);
                UpdateColourScheme(rgbColourScheme, nClasses);
            }
        }


        private void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            for (int i = 0; i < _polylines.Count; i++)
            {
                // an empty search box acts as a 'reset' button, clear all selections
                if (string.IsNullOrWhiteSpace(args.QueryText))
                {
                    _polylines[i].Searched = false;
                }
                else
                {
                    _polylines[i].Searched = _polylines[i].Details.IndexOf(args.QueryText, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
        }


        private void resetRangeSlidersButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (RangeSlider rangeSlider in _rangeSliders)
            {
                rangeSlider.ResetRangeSlider();
            }
            
            resetRangeSlidersButton.IsEnabled = false;
        }
    }
}