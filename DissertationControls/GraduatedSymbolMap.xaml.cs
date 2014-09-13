using Bing.Maps;
using DissertationControls.GeocodeService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DissertationControls
{
    public sealed partial class GraduatedSymbolMap : UserControl
    {
        const string BING_MAPS_KEY = "Amtb1dMrbwAnN2nax0fK5uGlcTfAJHf_K7B3hT1yrBvsHu3OEti_uH9P1XkBIVt-";
        const int DEFAULT_NUM_DATA_CLASSES = 3;
        
        StorageFolder _folder;
        string _fileName;

        DatasetModel _datasetModel;
        ColourScheme _colourScheme;
        List<string> _distinctCategories;
        List<BubblePushpin> _bubblePushpins;

        string _latitude;
        int _latitideIndex;

        string _longitude;
        int _longitudeIndex;

        string _placeName;
        int _placeNameIndex;

        string _description;
        int _descriptionIndex;

        string _symbolSize;
        int _symbolSizeIndex;

        string _symbolColour;
        int _symbolColourIndex;

        double _largestBubbleSize;

        public GraduatedSymbolMap()
        {
            this.InitializeComponent();
            _datasetModel = new DatasetModel();
            _bubblePushpins = new List<BubblePushpin>();
            _colourScheme = ColourScheme.Instance;

            // Initialise map properties
            map.LogoPosition = MapForegroundPosition.BottomLeft;
            map.MapType = MapType.Road;
            map.CopyrightPosition = MapForegroundPosition.BottomLeft;
            map.ShowBuildings = false;
            map.ShowBreadcrumb = true;
            map.ShowNavigationBar = true;
            map.ShowTraffic = false;
            map.ShowScaleBar = false;
            map.ViewRestriction = MapViewRestriction.ZoomOutToWholeWorld;
            map.SetView(new Bing.Maps.Location(0.0, 0.0), 2);
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

        public string Latitude
        {
            get { return _latitude; }
            set { _latitude = value; }
        }

        public string Longitude
        {
            get { return _longitude; }
            set { _longitude = value; }
        }

        public string PlaceName
        {
            get { return _placeName; }
            set { _placeName = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string SymbolSize
        {
            get { return _symbolSize; }
            set { _symbolSize = value; }
        }

        public string SymbolColour
        {
            get { return _symbolColour; }
            set { _symbolColour = value; }
        }

        public double LargestBubbleSize
        {
            get { return _largestBubbleSize; }
            set { _largestBubbleSize = value; }
        }


        private async void GraduatedSymbolMap_Loaded(object sender, RoutedEventArgs e)
        {
            if (await CheckPropertiesAreSet())
            {
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
                _latitideIndex = Array.IndexOf(_datasetModel.ColumnHeadings, this.Latitude);
                _longitudeIndex = Array.IndexOf(_datasetModel.ColumnHeadings, this.Longitude);
                _placeNameIndex = Array.IndexOf(_datasetModel.ColumnHeadings, this.PlaceName);
                _symbolSizeIndex = Array.IndexOf(_datasetModel.ColumnHeadings, this.SymbolSize);
                _symbolColourIndex = Array.IndexOf(_datasetModel.ColumnHeadings, this.SymbolColour);
                _descriptionIndex = Array.IndexOf(_datasetModel.ColumnHeadings, this.Description);

                if (await SetupColourComboBoxes())
                {
                    if (await CheckSymbolSizeValues())
                    {
                        if (this.LargestBubbleSize != 0)
                        {
                            CalculateScaleFactor();
                        }

                        if (!_datasetModel.IsColourQualitative)
                        {
                            if (await _datasetModel.GetColourMaxMin(_symbolColourIndex))
                            {
                                CreateBubbles();
                            }
                        }
                        else
                        {
                            CreateBubbles();
                        }
                    }
                }
            }
        }


        private async Task<bool> CheckPropertiesAreSet()
        {
            bool allPropertiesSet = true;
            MessageDialog errorDialog = null;

            if (String.IsNullOrEmpty(this.PlaceName))
            {
                allPropertiesSet = false;
                errorDialog = new MessageDialog("Place name property is not set", "Property not set");
            }
            else if (String.IsNullOrEmpty(this.SymbolSize))
            {
                allPropertiesSet = false;
                errorDialog = new MessageDialog("Symbol size property is not set", "Property not set");
            }
            else if (String.IsNullOrEmpty(this.SymbolColour))
            {
                allPropertiesSet = false;
                errorDialog = new MessageDialog("Symbol colour property is not set", "Property not set");
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

            if (_datasetModel.ColourValueCheck(_symbolColourIndex))
            {
                int nClassesValue = 0;

                // colour values are qualitative
                // hide number of data classes and colour scheme type combo boxes
                // as they are not required, the values have been automatically derived from the data
                numDataClassesTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                numDataClassesComboBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                colourSchemeTypeTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                colourSchemeTypeComboBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                _distinctCategories = _datasetModel.Dataset.Select(o => o[_symbolColourIndex]).Distinct().ToList();
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


        // method that checks the parsing of the symbol size values
        private async Task<bool> CheckSymbolSizeValues()
        {
            bool success = true;
            
            for (int i = 0; i < _datasetModel.Dataset.Count; i++)
            {
                double result;

                if (!double.TryParse(_datasetModel.Dataset[i][_symbolSizeIndex], out result))
                {
                    success = false;
                    await new MessageDialog("string to double parse error at line " + (i + 2).ToString(), 
                        "Error parsing the dataset").ShowAsync();
                }
            }

            return success;
        }


        // method that calculates a scale factor based on the largest bubble size property
        private void CalculateScaleFactor()
        {
            double largestBubbleValue = _datasetModel.Dataset.Max(o => double.Parse(o[_symbolSizeIndex]));
            double radius = Math.Sqrt(largestBubbleValue);
            BubblePushpin.ScaleFactor = this.LargestBubbleSize / radius;
        }


        private async void CreateBubbles()
        {
            StringBuilder sb = new StringBuilder();
            byte[][] colourSchemeArray;
            int nClassesValue = 3;
            string colourSchemeComboBoxSelection = colourSchemeComboBox.SelectedValue.ToString();

            if (!_datasetModel.IsColourQualitative)
            {
                ComboBoxItem nClassesComboBoxSelection = (ComboBoxItem)numDataClassesComboBox.SelectedValue;
                nClassesValue = int.Parse(nClassesComboBoxSelection.Content.ToString());
                colourSchemeArray = _colourScheme.GetRGBColours(colourSchemeComboBoxSelection, nClassesValue);
            }
            else
            {
                nClassesValue = _distinctCategories.Count();
                colourSchemeArray = _colourScheme.GetRGBColours(colourSchemeComboBoxSelection, nClassesValue);
            }

            for (int i = 0; i < _datasetModel.Dataset.Count; i++)
            {
                string[] row = _datasetModel.Dataset[i];
                
                BubblePushpin bubble = new BubblePushpin();
                bubble.OriginalBubbleSize = double.Parse(row[_symbolSizeIndex]);
                bubble.BubbleSize = bubble.OriginalBubbleSize;
                bubble.ColourValue = row[_symbolColourIndex];

                // use StringBuilder to create the tooltip details
                sb.AppendLine(row[_placeNameIndex]);
                sb.Append(_datasetModel.ColumnHeadings[_symbolSizeIndex]);
                sb.Append(": ");
                sb.AppendLine(row[_symbolSizeIndex]);
                sb.Append(_datasetModel.ColumnHeadings[_symbolColourIndex]);
                sb.Append(": ");
                sb.Append(row[_symbolColourIndex]);
                if (!String.IsNullOrEmpty(this.Description))
                {
                    sb.Append(": ");
                    sb.Append(row[_descriptionIndex]);
                }
                bubble.Details = sb.ToString();

                // plot symbols on map - if lat/lon not set then geocode the place name
                if (String.IsNullOrEmpty(this.Latitude) || String.IsNullOrEmpty(this.Longitude) ||
                        String.IsNullOrEmpty(row[_latitideIndex]) || String.IsNullOrEmpty(row[_longitudeIndex]))
                {
                    Bing.Maps.Location geocodedLocation = await GeocodePlaceName(row[_placeNameIndex]);
                    bubble.LatLon = geocodedLocation;
                }
                else
                {
                    double latitude = 0.0;
                    double longitude = 0.0;
                    
                    if (!double.TryParse(row[_latitideIndex], out latitude) || 
                         !double.TryParse(row[_longitudeIndex], out longitude))
                    {
                        await new MessageDialog("Cannot parse latitude/longitude for " + row[_placeNameIndex], 
                            "Error parsing dataset").ShowAsync();
                    }
                    
                    bubble.LatLon = new Bing.Maps.Location(latitude, longitude);
                }

                // Apply colour
                if (_datasetModel.IsColourQualitative)
                {
                    int index = _distinctCategories.IndexOf(row[_symbolColourIndex]);
                    bubble.Colour = colourSchemeArray[index];
                }
                else
                {
                    double colourValue = double.Parse(row[_symbolColourIndex]);
                    double linearTransform = (colourValue - _datasetModel.ColourMinMax[0]) /
                        (_datasetModel.ColourMinMax[1] - _datasetModel.ColourMinMax[0]) * (nClassesValue - 1);
                    int colourIndex = (int)Math.Truncate(linearTransform);
                    bubble.Colour = colourSchemeArray[colourIndex];
                }
                
                _bubblePushpins.Add(bubble);
                sb.Clear();
            }

            Draw();
        }


        private void Draw()
        {
            // Draw in order from largest to smallest to prevent occlusion
            _bubblePushpins = _bubblePushpins.OrderByDescending(o => o.OriginalBubbleSize).ToList();

            foreach (BubblePushpin bubblePushpin in _bubblePushpins)
            {
                MapLayer.SetPosition(bubblePushpin, bubblePushpin.LatLon);
                map.Children.Add(bubblePushpin);
            }

            // Assign event handlers at the end so events are not fired during initialisation
            numDataClassesComboBox.SelectionChanged += numDataClassesComboBox_SelectionChanged;
            colourSchemeTypeComboBox.SelectionChanged += colourSchemeTypeComboBox_SelectionChanged;
            colourSchemeComboBox.SelectionChanged += colourSchemeComboBox_SelectionChanged;
        }


        private void UpdateColourScheme(byte[][] colourScheme, int nClasses)
        {
            if (_datasetModel.IsColourQualitative)
            {
                for (int i = 0; i < _bubblePushpins.Count; i++)
                {
                    int index = _distinctCategories.IndexOf(_bubblePushpins[i].ColourValue);
                    _bubblePushpins[i].Colour = colourScheme[index];
                }
            }
            else
            {
                for (int i = 0; i < _bubblePushpins.Count; i++)
                {
                    double colourValue = double.Parse(_bubblePushpins[i].ColourValue);
                    double linearTransform = (colourValue - _datasetModel.ColourMinMax[0]) /
                        (_datasetModel.ColourMinMax[1] - _datasetModel.ColourMinMax[0]) * (nClasses - 1);
                    int colourIndex = (int)Math.Truncate(linearTransform);
                    _bubblePushpins[i].Colour = colourScheme[colourIndex];
                }
            }
        }


        private async Task<Bing.Maps.Location> GeocodePlaceName(string place)
        {
            Bing.Maps.Location geocodedPlace = new Bing.Maps.Location();
            
            GeocodeRequest geocodeRequest = new GeocodeRequest();
            geocodeRequest.Credentials = new Credentials();
            geocodeRequest.Credentials.ApplicationId = BING_MAPS_KEY;

            geocodeRequest.Query = place;
            GeocodeServiceClient geocodeService = new GeocodeServiceClient(
                GeocodeServiceClient.EndpointConfiguration.BasicHttpBinding_IGeocodeService);
            GeocodeResponse geocodeResponse = await geocodeService.GeocodeAsync(geocodeRequest);

            if (geocodeResponse.Results.Count > 0)
            {
                geocodedPlace.Latitude = geocodeResponse.Results[0].Locations[0].Latitude;
                geocodedPlace.Longitude = geocodeResponse.Results[0].Locations[0].Longitude;
            }

            return geocodedPlace;
        }


        // Resize symbols slider value changed event handler
        private void symbolSizeSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            foreach (UIElement element in map.Children)
            {
                if (element is BubblePushpin)
                {
                    BubblePushpin bp = element as BubblePushpin;

                    if (e.NewValue == 0.0)
                    {
                        // revert back to original sizes
                        bp.BubbleSize = bp.OriginalBubbleSize;
                    }
                    else
                    {
                        if (e.NewValue < e.OldValue)
                        {
                            // shrinking bubbles so we divide to get the new size
                            // convert negative values to positive values to prevent
                            // the subsequent area calculations from breaking
                            if (e.NewValue < 0.0)
                            {
                                bp.BubbleSize = bp.BubbleSize / (e.NewValue * -1.0);
                            }
                            else
                            {
                                bp.BubbleSize = bp.BubbleSize / e.NewValue;
                            }
                        }
                        else
                        {
                            // growing bubbles - multiply
                            bp.BubbleSize = bp.BubbleSize * e.NewValue;
                        }
                    }
                }
            }
        }

        // colour scheme event handlers
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
                string colourScheme = colourSchemeComboBox.SelectedValue.ToString();
                byte[][] rgbColourScheme = _colourScheme.GetRGBColours(colourScheme, nClasses);
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
                string colourScheme = cb.SelectedValue.ToString();
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

                byte[][] rgbColourScheme = _colourScheme.GetRGBColours(colourScheme, nClasses);
                UpdateColourScheme(rgbColourScheme, nClasses);
            }
        }


        private void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            foreach (BubblePushpin bubblePushpin in _bubblePushpins)
            {
                // an empty search box acts as a 'reset' button, clear all selections
                if (string.IsNullOrWhiteSpace(args.QueryText))
                {
                    bubblePushpin.Searched = false;
                }
                else
                {
                    bubblePushpin.Searched = bubblePushpin.Details.IndexOf(args.QueryText, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
        }
    }
}