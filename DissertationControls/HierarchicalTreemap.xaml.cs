using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace DissertationControls
{
    public sealed partial class HierarchicalTreemap : UserControl
    {
        const int DEFAULT_NUM_DATA_CLASSES = 3;
        
        StorageFolder _folder;
        string _fileName;
        List<TreemapNode> _hierarchicalData;
        DatasetModel _datasetModel;

        string _id;
        int _idIndex;

        string _areas;
        int _areasIndex;

        string _description;
        int _descriptionIndex;

        string _hierarchy;
        int _hierarchyIndex;

        string _colour;
        int _colourIndex;

        ColourScheme _colourScheme;
        List<string> _distinctCategories;

        TreemapDrawingArea _drawingArea;
        List<TreemapRow> _rows;
        List<TreemapRow> _childRows;

        // x and y positions so each row can be positioned globally on screen
        double _offsetX;
        double _offsetY;

        double _offsetChildX;
        double _offsetChildY;

        bool _zoomedIn;
        
        public HierarchicalTreemap()
        {
            this.InitializeComponent();
            _hierarchicalData = new List<TreemapNode>();
            _drawingArea = new TreemapDrawingArea();
            _rows = new List<TreemapRow>();
            _childRows = new List<TreemapRow>();

            _offsetX = 0;
            _offsetY = 0;
            _offsetChildX = 0;
            _offsetChildY = 0;

            _colourScheme = ColourScheme.Instance;
            _datasetModel = new DatasetModel();
            _zoomedIn = false;
        }


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
            set{ _id = value; }
        }

        public string Area
        {
            get { return _areas; }
            set { _areas = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string Hierarchy
        {
            get { return _hierarchy; }
            set { _hierarchy = value; }
        }

        public string Colour
        {
            get { return _colour; }
            set { _colour = value; }
        }


        private async void HierarchicalTreemap_Loaded(object sender, RoutedEventArgs e)
        {
            if (await CheckPropertiesAreSet())
            {
                _drawingArea.Width = canvas.ActualWidth;
                _drawingArea.Height = canvas.ActualHeight;
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
                // create property indexes
                _areasIndex = Array.IndexOf(_datasetModel.ColumnHeadings, _areas);
                _idIndex = Array.IndexOf(_datasetModel.ColumnHeadings, _id);
                _descriptionIndex = Array.IndexOf(_datasetModel.ColumnHeadings, _description);
                _hierarchyIndex = Array.IndexOf(_datasetModel.ColumnHeadings, _hierarchy);
                _colourIndex = Array.IndexOf(_datasetModel.ColumnHeadings, _colour);

                if (await CheckTreemapAreaValues())
                {
                    if (await SetupColourComboBoxes())
                    {
                        if (!_datasetModel.IsColourQualitative)
                        {
                            if (await _datasetModel.GetColourMaxMin(_colourIndex))
                            {
                                NormaliseValues();
                            }
                        }
                        else
                        {
                            NormaliseValues();
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
            else if (String.IsNullOrEmpty(this.Area))
            {
                allPropertiesSet = false;
                errorDialog = new MessageDialog("Area property is not set", "Property not set");
            }
            else if (String.IsNullOrEmpty(this.Colour))
            {
                allPropertiesSet = false;
                errorDialog = new MessageDialog("Colour property is not set", "Property not set");
            }
            else if (String.IsNullOrEmpty(this.Hierarchy))
            {
                allPropertiesSet = false;
                errorDialog = new MessageDialog("Hierarchy property is not set", "Property not set");
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

            if (_datasetModel.ColourValueCheck(_colourIndex))
            {
                int nClassesValue = 0;

                // colour values are qualitative
                // hide number of data classes and colour scheme type combo boxes
                // as they are not required, the values have been automatically derived from the data
                numDataClassesTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                numDataClassesComboBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                colourSchemeTypeTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                colourSchemeTypeComboBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                _distinctCategories = _datasetModel.Dataset.Select(o => o[_colourIndex]).Distinct().ToList();
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

        // method that checks the parsing of the treemap area values
        private async Task<bool> CheckTreemapAreaValues()
        {
            bool success = true;

            for (int i = 0; i < _datasetModel.Dataset.Count; i++)
            {
                double result;

                if (!double.TryParse(_datasetModel.Dataset[i][_areasIndex], out result))
                {
                    success = false;
                    await new MessageDialog("string to double parse error at line " + (i + 2).ToString(),
                        "Error parsing the dataset").ShowAsync();
                }
            }

            return success;
        }

        // Method that normalises the areas to the screen space
        private void NormaliseValues()
        {
            double total = 0;
            double area = 0;
            double screenScaleRatio = 0;

            foreach (string[] element in _datasetModel.Dataset)
            {
                if (!double.TryParse(element[_areasIndex], out area))
                {
                    // Output error
                }
                else
                {
                    total += area;
                }
            }

            // Calculate the screen scale ratio
            screenScaleRatio = _drawingArea.Width * _drawingArea.Height / total;

            foreach (string[] element in _datasetModel.Dataset)
            {
                double initialValue = double.Parse(element[_areasIndex]);
                double scaledValue = initialValue * screenScaleRatio;

                TreemapNode node = new TreemapNode();
                node.NormalisedArea = scaledValue;
                node.Name = element[_idIndex];
                node.Value = element[_areasIndex];
                if (_descriptionIndex >= 0)
                {
                    node.Description = element[_descriptionIndex];
                }
                node.Hierarchy = element[_hierarchyIndex];
                node.ColourValue = element[_colourIndex];

                _hierarchicalData.Add(node);
            }

            CreateHierarchy();
            AggregateHierarchyValues();
            SortValues();
            Squarify(_hierarchicalData, new List<TreemapNode>());

            // squarify children
            foreach (TreemapNode node in _hierarchicalData)
            {
                _offsetChildX = node.XPos;
                _offsetChildY = node.YPos;
                SquarifyChildren(node.Children, new List<TreemapNode>(), node.NodeWidth, node.NodeHeight);
            }

            DrawChildren();
            Draw();
        }


        // method that recalculates the normalisation factor when a hierarchy level is zoomed
        private void ZoomNormalisedValues(List<TreemapNode> nodes)
        {
            double total = 0;
            double area = 0;
            double screenScaleRatio = 0;

            foreach (TreemapNode node in nodes)
            {
                if (!double.TryParse(node.Value, out area))
                {
                    // Output error
                }
                else
                {
                    total += area;
                }
            }

            // re-calculate the screen scale ratio
            screenScaleRatio = _drawingArea.Width * _drawingArea.Height / total;

            foreach (TreemapNode node in nodes)
            {
                double initialValue = double.Parse(node.Value);
                double scaledValue = initialValue * screenScaleRatio;
                node.NormalisedArea = scaledValue;
            }
        }


        // Method that converts the flat dataset into a hierarchy
        private void CreateHierarchy()
        {
            // group by hierarchy property first
            _hierarchicalData = _hierarchicalData.GroupBy(x => x.Hierarchy).Select(
                // create new parent node
                grouping => new TreemapNode
                {
                    NodeName = grouping.Key,
                    // assign the existing children's values as new nodes that
                    // are added to the new parent
                    Children = grouping.Select(leafNode => new TreemapNode
                                {
                                    NodeName = leafNode.Name,
                                    Description = leafNode.Description,
                                    Hierarchy = leafNode.Hierarchy,
                                    NormalisedArea = leafNode.NormalisedArea,
                                    Value = leafNode.Value,
                                    ColourValue = leafNode.ColourValue,
                                }).ToList()
                }).ToList();
        }


        private void AggregateHierarchyValues()
        {
            foreach (TreemapNode node in _hierarchicalData)
            {
                node.AggregateValues();
            }
        }


        private void SortValues()
        {
            // sort in descending order for optimal squarify algorithm results
            _hierarchicalData = _hierarchicalData.OrderByDescending(o => o.NormalisedArea).ToList();

            // sort the children also
            foreach (TreemapNode parent in _hierarchicalData)
            {
                parent.Children = parent.Children.OrderByDescending(o => o.NormalisedArea).ToList();
            }
        }


        private void Squarify(List<TreemapNode> children, List<TreemapNode> row)
        {
            double currentX = 0;
            double currentY = 0;
            double totalArea = 0;
            double width = 0;
            double height = 0;
            double aspectRatio = 0;
            bool drawVertically = DrawVertically(_drawingArea.Width, _drawingArea.Height);

            // add next child to the current row
            row.Add(children.ElementAt(0));

            foreach (TreemapNode element in row)
            {
                totalArea += element.NormalisedArea;
            }

            if (drawVertically)
            {
                width = totalArea / _drawingArea.Height;
                height = row.Last().NormalisedArea / width;
            }
            else
            {
                height = totalArea / _drawingArea.Width;
                width = row.Last().NormalisedArea / height;
            }

            aspectRatio = AspectRatio(height, width);

            // Compare aspect ratios to decide whether to accept or reject the rectangle
            if (children.Count > 1 && _drawingArea.AspectRatio == 0 || aspectRatio < _drawingArea.AspectRatio)
            {
                // we have improved the aspect ratio, so update the latest accepted values and continue
                _drawingArea.AspectRatio = aspectRatio;

                if (drawVertically)
                {
                    _drawingArea.RowSize = width;
                }
                else
                {
                    _drawingArea.RowSize = height;
                }
                Squarify(children.Skip(1).ToList(), row);
            }
            else
            {
                // lockout row - remove last entry as it has been rejected, but
                // only if it is not the last element, which just takes up the remaining space
                if (children.Count > 1)
                {
                    int lastElement = row.Count - 1;
                    row.RemoveAt(lastElement);
                }
                else
                {
                    // This is the last data entry - update row size
                    if (drawVertically)
                    {
                        _drawingArea.RowSize = width;
                    }
                    else
                    {
                        _drawingArea.RowSize = height;
                    }
                }

                // Create a new row to add the nodes into
                TreemapRow lockoutRow = new TreemapRow();

                // calculate the sizes of the nodes in the row
                foreach (TreemapNode element in row)
                {
                    double nodeSize = element.NormalisedArea / _drawingArea.RowSize;

                    if (drawVertically)
                    {
                        element.NodeWidth = _drawingArea.RowSize;
                        element.NodeHeight = nodeSize;
                        element.XPos = 0;
                        element.YPos = 0;
                    }
                    else
                    {
                        element.NodeWidth = nodeSize;
                        element.NodeHeight = _drawingArea.RowSize;
                        element.XPos = 0;
                        element.YPos = 0;
                    }

                    element.XPos = currentX + _offsetX;
                    element.YPos = currentY + _offsetY;
                    lockoutRow.AddNodeToRow(element);

                    if (drawVertically)
                    {
                        currentY += nodeSize;
                    }
                    else
                    {
                        currentX += nodeSize;
                    }
                }

                // Add row to collection
                _rows.Add(lockoutRow);

                // Update remaning screen width and offsets, reset aspect ratio ready for the new row
                if (drawVertically)
                {
                    _drawingArea.Width -= _drawingArea.RowSize;
                    _offsetX += _drawingArea.RowSize;
                }
                else
                {
                    _drawingArea.Height -= _drawingArea.RowSize;
                    _offsetY += _drawingArea.RowSize;
                }

                _drawingArea.AspectRatio = 0;
                row.Clear();

                // Continue if we have more than 1 data entry remaining
                if (children.Count > 1)
                {
                    Squarify(children, row);
                }
            }
        }


        private void SquarifyChildren(List<TreemapNode> children, List<TreemapNode> row, double containerWidth, double containerHeight)
        {
            double currentX = 0;
            double currentY = 0;
            double totalArea = 0;
            double width = 0;
            double height = 0;
            double aspectRatio = 0;
            bool drawVertically = DrawVertically(containerWidth, containerHeight);

            // add next child to the current row
            row.Add(children.ElementAt(0));

            foreach (TreemapNode element in row)
            {
                totalArea += element.NormalisedArea;
            }

            if (drawVertically)
            {
                width = totalArea / containerHeight;
                height = row.Last().NormalisedArea / width;
            }
            else
            {
                height = totalArea / containerWidth;
                width = row.Last().NormalisedArea / height;
            }

            aspectRatio = AspectRatio(height, width);

            // Compare aspect ratios to decide whether to accept or reject the rectangle
            if (children.Count > 1 && _drawingArea.AspectRatio == 0 || aspectRatio < _drawingArea.AspectRatio)
            {
                // we have improved the aspect ratio, so update the latest accepted values and continue
                _drawingArea.AspectRatio = aspectRatio;

                if (drawVertically)
                {
                    _drawingArea.RowSize = width;
                }
                else
                {
                    _drawingArea.RowSize = height;
                }
                SquarifyChildren(children.Skip(1).ToList(), row, containerWidth, containerHeight);
            }
            else
            {
                // lockout row - remove last entry as it has been rejected, but
                // only if it is not the last element, which just takes up the remaining space
                if (children.Count > 1)
                {
                    int lastElement = row.Count - 1;
                    row.RemoveAt(lastElement);
                }
                else
                {
                    // This is the last data entry - update row size
                    if (drawVertically)
                    {
                        _drawingArea.RowSize = width;
                    }
                    else
                    {
                        _drawingArea.RowSize = height;
                    }
                }

                // Create a new row to add the nodes into
                TreemapRow lockoutRow = new TreemapRow();

                // create nodes for each element in the row and calculate the sizes
                foreach (TreemapNode element in row)
                {
                    double nodeSize = element.NormalisedArea / _drawingArea.RowSize;

                    if (drawVertically)
                    {
                        element.NodeWidth = _drawingArea.RowSize;
                        element.NodeHeight = nodeSize;
                        element.XPos = 0;
                        element.YPos = 0;
                    }
                    else
                    {
                        element.NodeWidth = nodeSize;
                        element.NodeHeight = _drawingArea.RowSize;
                        element.XPos = 0;
                        element.YPos = 0;
                    }

                    // assign event handlers
                    element.RightTapped += treemapNode_RightTapped;

                    element.XPos = currentX + _offsetChildX;
                    element.YPos = currentY + _offsetChildY;
                    lockoutRow.AddNodeToRow(element);

                    if (drawVertically)
                    {
                        currentY += nodeSize;
                    }
                    else
                    {
                        currentX += nodeSize;
                    }
                }

                // Add row to collection
                _childRows.Add(lockoutRow);

                // Update remaning screen width and offsets, reset aspect ratio ready for the new row
                if (drawVertically)
                {
                    containerWidth -= _drawingArea.RowSize;
                    _offsetChildX += _drawingArea.RowSize;
                }
                else
                {
                    containerHeight -= _drawingArea.RowSize;
                    _offsetChildY += _drawingArea.RowSize;
                }

                _drawingArea.AspectRatio = 0;
                row.Clear();

                // Continue if we have more than 1 data entry remaining
                if (children.Count > 1)
                {
                    SquarifyChildren(children, row, containerWidth, containerHeight);
                }
            }
        }


        private void Draw()
        {            
            foreach (TreemapRow row in _rows)
            {
                foreach (TreemapNode node in row.Nodes)
                {
                    node.ParentCheck();
                    Canvas.SetLeft(node, node.XPos);
                    Canvas.SetTop(node, node.YPos);
                    canvas.Children.Add(node);
                }
            }

            numDataClassesComboBox.SelectionChanged += numDataClassesComboBox_SelectionChanged;
            colourSchemeTypeComboBox.SelectionChanged += colourSchemeTypeComboBox_SelectionChanged;
            colourSchemeComboBox.SelectionChanged += colourSchemeComboBox_SelectionChanged;
        }


        private void DrawChildren()
        {
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
                nClassesValue = _distinctCategories.Count();
                defaultColourScheme = _colourScheme.GetRGBColours(colourSchemeComboBoxSelection, nClassesValue);
            }
            
            foreach (TreemapRow row in _childRows)
            {
                foreach (TreemapNode node in row.Nodes)
                {
                    node.ParentCheck();
                    Canvas.SetLeft(node, node.XPos);
                    Canvas.SetTop(node, node.YPos);
                    canvas.Children.Add(node);

                    if (_datasetModel.IsColourQualitative)
                    {
                        int index = _distinctCategories.IndexOf(node.ColourValue);
                        node.Colour = defaultColourScheme[index];
                    }
                    else
                    {
                        double colourValue = double.Parse(node.ColourValue);
                        double linearTransform = (colourValue - _datasetModel.ColourMinMax[0]) / 
                            (_datasetModel.ColourMinMax[1] - _datasetModel.ColourMinMax[0]) * (nClassesValue - 1);
                        int colourIndex = (int)Math.Truncate(linearTransform);
                        node.Colour = defaultColourScheme[colourIndex];
                    }
                }
            }
        }


        private void DrawZoomedInView()
        {
            byte[][] defaultColourScheme = new byte[][] { };
            int nClassesValue = 0;

            string colourSchemeComboBoxSelection = colourSchemeComboBox.SelectedValue.ToString();

            if (!_datasetModel.IsColourQualitative)
            {
                ComboBoxItem nClassesComboBoxSelection = (ComboBoxItem)numDataClassesComboBox.SelectedValue;
                nClassesValue = int.Parse(nClassesComboBoxSelection.Content.ToString());
                defaultColourScheme = _colourScheme.GetRGBColours(colourSchemeComboBoxSelection, nClassesValue);
            }
            else
            {
                nClassesValue = _distinctCategories.Count();
                defaultColourScheme = _colourScheme.GetRGBColours(colourSchemeComboBoxSelection, nClassesValue);
            }

            foreach (TreemapRow row in _rows)
            {
                foreach (TreemapNode node in row.Nodes)
                {
                    node.ZoomedInView();
                    Canvas.SetLeft(node, node.XPos);
                    Canvas.SetTop(node, node.YPos);
                    canvas.Children.Add(node);

                    if (_datasetModel.IsColourQualitative)
                    {
                        int index = _distinctCategories.IndexOf(node.ColourValue);
                        node.Colour = defaultColourScheme[index];
                    }
                    else
                    {
                        double colourValue = double.Parse(node.ColourValue);
                        double linearTransform = (colourValue - _datasetModel.ColourMinMax[0]) / 
                            (_datasetModel.ColourMinMax[1] - _datasetModel.ColourMinMax[0]) * (nClassesValue - 1);
                        int colourIndex = (int)Math.Truncate(linearTransform);
                        node.Colour = defaultColourScheme[colourIndex];
                    }
                }
            }
        }


        private bool DrawVertically(double width, double height)
        {
            return width > height;
        }


        private double AspectRatio(double height, double width)
        {
            return Math.Max(height / width, width / height);
        }


        private void UpdateColourScheme(List<TreemapRow> rows, byte[][] colourScheme, int nClasses)
        {
            if (_datasetModel.IsColourQualitative)
            {
                foreach (TreemapRow row in rows)
                {
                    foreach (TreemapNode node in row.Nodes)
                    {
                        int index = _distinctCategories.IndexOf(node.ColourValue);
                        node.Colour = colourScheme[index];
                    }
                }
            }
            else
            {
                foreach (TreemapRow row in rows)
                {
                    foreach (TreemapNode node in row.Nodes)
                    {
                        double colourValue = double.Parse(node.ColourValue);
                        double linearTransform = (colourValue - _datasetModel.ColourMinMax[0]) /
                            (_datasetModel.ColourMinMax[1] - _datasetModel.ColourMinMax[0]) * (nClasses - 1);
                        int index = (int)Math.Truncate(linearTransform);
                        node.Colour = colourScheme[index];
                    }
                }
            }
        }


        // Hierarchy context menu event handlers
        private async void treemapNode_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            string menuText = String.Format("Zoom into {0}", (sender as TreemapNode).Hierarchy);
            
            // show a context menu to enable zoom in/out
            var menu = new PopupMenu();

            if (!_zoomedIn)
            {
                menu.Commands.Add(new UICommand(menuText, command =>
                    {
                        ZoomIn(sender);
                    }));
            }
            else
            {
                menu.Commands.Add(new UICommand("Zoom Out", command =>
                    {
                        ZoomOut();
                    }));
            }

            var chosenCommand = await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)sender));
        }


        // method that returns an area to place the context menu into
        // Source: http://code.msdn.microsoft.com/windowsapps/Context-menu-sample-40840351
        private Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform transform = element.TransformToVisual(null);
            Point point = transform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }


        // zoom in on a parent level
        private void ZoomIn(object node)
        {
            _zoomedIn = true;

            // get parent node
            string parentName = (node as TreemapNode).Hierarchy;
            TreemapNode parentNode = _hierarchicalData.First(s => s.NodeName == parentName);

            hierarchyLevelTextBlock.Text = String.Format("Showing: {0}", parentName);

            // clear canvas and row collections ready for drawing the zoomed in view
            canvas.Children.Clear();
            _rows.Clear();

            // reset the drawing area bounds
            _drawingArea.Height = canvas.ActualHeight;
            _drawingArea.Width = canvas.ActualWidth;

            // reset the offsets as we are starting from x0y0
            _offsetX = 0;
            _offsetY = 0;

            // update the normalised values and re-squarify/redraw
            ZoomNormalisedValues(parentNode.Children);
            Squarify(parentNode.Children, new List<TreemapNode>());
            DrawZoomedInView();
        }


        private void ZoomOut()
        {
            _zoomedIn = false;
            hierarchyLevelTextBlock.Text = String.Empty;

            canvas.Children.Clear();
            _rows.Clear();
            _childRows.Clear();
            _hierarchicalData.Clear();

            // reset the drawing area bounds
            _drawingArea.Height = canvas.ActualHeight;
            _drawingArea.Width = canvas.ActualWidth;

            // reset the offsets as we are starting from x0y0
            _offsetX = 0;
            _offsetY = 0;

            // remove the combobox event handlers as these will be re-added during the redraw
            numDataClassesComboBox.SelectionChanged -= numDataClassesComboBox_SelectionChanged;
            colourSchemeTypeComboBox.SelectionChanged -= colourSchemeTypeComboBox_SelectionChanged;
            colourSchemeComboBox.SelectionChanged -= colourSchemeComboBox_SelectionChanged;

            NormaliseValues();
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

                if (_zoomedIn)
                {
                    UpdateColourScheme(_rows, rgbColourScheme, nClasses);
                }
                else
                {
                    UpdateColourScheme(_childRows, rgbColourScheme, nClasses);
                }
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

                if (_zoomedIn)
                {
                    UpdateColourScheme(_rows, rgbColourScheme, nClasses);
                }
                else
                {
                    UpdateColourScheme(_childRows, rgbColourScheme, nClasses);
                }
            }
        }


        // searchbox query submitted event handler
        private void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            foreach (TreemapNode node in canvas.Children)
            {
                // only search leaf nodes
                if (node.Children.Count == 0)
                {
                    // an empty search box acts as a 'reset' button, clear all selections
                    if (string.IsNullOrWhiteSpace(args.QueryText))
                    {
                        node.Searched = false;
                    }
                    else
                    {
                        node.Searched = node.ToString().IndexOf(args.QueryText, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                }
            }
        }
    }
}