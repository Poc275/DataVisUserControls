using Bing.Maps;
using System;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DissertationControls
{
    public sealed partial class BubblePushpin : UserControl
    {
        double _bubbleSize;
        double _originalBubbleSize;
        string _details;
        byte[] _colour;
        Location _latlon;
        string _colourValue;
        bool _searched;
        bool _selected;
        static double scaleFactor;


        public BubblePushpin()
        {
            this.InitializeComponent();
            _searched = false;
            _selected = false;
        }


        public double BubbleSize
        {
            get { return _bubbleSize; }
            set
            {
                _bubbleSize = value;
                // size ellipse correctly, according to area
                // area = pi * radius squared, so: radius = sqrt(area / pi)
                // but we can omit pi as it is a constant
                double radius = Math.Sqrt(this.BubbleSize);

                // multiply by the scale factor to satisfy the largest bubble size property
                if (BubblePushpin.ScaleFactor != 0)
                {
                    radius *= BubblePushpin.ScaleFactor;
                }

                // height and width are measured across the ellipse, so set them to twice the radius
                // setting both width and height to the same value ensures a circle is drawn
                bubbleEllipse.Height = radius * 2;
                bubbleEllipse.Width = radius * 2;
            }
        }

        public double OriginalBubbleSize
        {
            // Property that stores the original size of the bubble
            // so the display can be reset to default
            get { return _originalBubbleSize; }
            set { _originalBubbleSize = value; }
        }

        public string Details
        {
            get { return _details; }
            set { _details = value; }
        }

        public byte[] Colour
        {
            get { return _colour; }
            set
            {
                _colour = value;
                bubbleEllipse.Fill = new SolidColorBrush(Color.FromArgb(255, _colour[0], _colour[1], _colour[2]));
            }
        }

        public Location LatLon
        {
            get { return _latlon; }
            set { _latlon = value; }
        }

        public string ColourValue
        {
            get { return _colourValue; }
            set { _colourValue = value; }
        }

        public static double ScaleFactor
        {
            get { return scaleFactor; }
            set { scaleFactor = value; }
        }

        public bool Searched
        {
            get { return _searched; }
            set
            {
                _searched = value;
                if (_searched)
                {
                    bubbleEllipse.Stroke = new SolidColorBrush(Colors.Yellow);
                    bubbleEllipse.StrokeThickness = 3;
                }
                else
                {
                    bubbleEllipse.Stroke = new SolidColorBrush(Colors.Black);
                    bubbleEllipse.StrokeThickness = 1;
                }
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
                    bubbleEllipse.Stroke = new SolidColorBrush(Colors.Red);
                    bubbleEllipse.StrokeThickness = 3;
                }
                else
                {
                    bubbleEllipse.Stroke = new SolidColorBrush(Colors.Black);
                    bubbleEllipse.StrokeThickness = 1;
                }
            }
        }


        // Event handlers
        protected override void OnPointerEntered(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.Content = this.Details;
            toolTip.Placement = Windows.UI.Xaml.Controls.Primitives.PlacementMode.Mouse;
            ToolTipService.SetToolTip(this, toolTip);
            toolTip.IsOpen = true;

            if (!this.Searched && !this.Selected)
            {
                bubbleEllipse.Stroke = new SolidColorBrush(Colors.Yellow);
            }
        }

        protected override void OnPointerExited(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ToolTip toolTip = (ToolTip)ToolTipService.GetToolTip(this);
            toolTip.IsOpen = false;

            if (!this.Searched && !this.Selected)
            {
                bubbleEllipse.Stroke = new SolidColorBrush(Colors.Black);
            }
        }

        protected override void OnTapped(Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.Selected = !this.Selected;
        }
    }
}