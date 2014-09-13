using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace DissertationControls
{
    public sealed partial class ParallelCoordsPolyline : UserControl
    {
        PointCollection _polylinePoints;
        string _details;
        bool _selected;
        bool _filtered;
        bool _searched;
        double[] _dataValues;
        byte[] _lineColour;
        string _colourValue;

        public ParallelCoordsPolyline()
        {
            this.InitializeComponent();
            _polylinePoints = new PointCollection();
            _selected = false;
            _filtered = false;
            _searched = false;
        }

        public PointCollection PolylinePoints
        {
            get { return _polylinePoints; }
            set
            {
                _polylinePoints = value;
                polyline.Points = _polylinePoints;
            }
        }

        public string Details
        {
            get { return _details; }
            set { _details = value; }
        }

        public double[] DataValues
        {
            get { return _dataValues; }
            set { _dataValues = value; }
        }

        public byte[] LineColour
        {
            get { return _lineColour; }
            set
            { 
                _lineColour = value;
                polyline.Stroke = new SolidColorBrush(Color.FromArgb(255, _lineColour[0], _lineColour[1], _lineColour[2]));
            }
        }

        public bool Filtered
        {
            get { return _filtered; }
            set
            {
                _filtered = value;
                this.Opacity = _filtered ? 0.2 : 1.0;
            }
        }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                if (_selected)
                {
                    polyline.StrokeThickness = 4;
                    polyline.Stroke = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    polyline.StrokeThickness = 1;
                    polyline.Stroke = new SolidColorBrush(Color.FromArgb(255, this.LineColour[0], this.LineColour[1], this.LineColour[2]));
                }
            }
        }

        public bool Searched
        {
            get { return _searched; }
            set
            {
                _searched = value ? this.Selected = true : this.Selected = false;
            }
        }

        public string ColourValue
        {
            get { return _colourValue; }
            set { _colourValue = value; }
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            if (!this.Selected)
            {
                polyline.StrokeThickness = 3;
                polyline.Stroke = new SolidColorBrush(Colors.Yellow);
            }

            ToolTip toolTip = new ToolTip();
            toolTip.Content = this.Details;
            toolTip.Placement = Windows.UI.Xaml.Controls.Primitives.PlacementMode.Mouse;
            ToolTipService.SetToolTip(this, toolTip);
            toolTip.IsOpen = true;
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            if (!this.Selected)
            {
                polyline.StrokeThickness = 1;
                polyline.Stroke = new SolidColorBrush(Color.FromArgb(255, this.LineColour[0], this.LineColour[1], this.LineColour[2]));
            }   

            ToolTip toolTip = (ToolTip)ToolTipService.GetToolTip(this);
            toolTip.IsOpen = false;
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            this.Selected = !this.Selected;
        }
    }
}