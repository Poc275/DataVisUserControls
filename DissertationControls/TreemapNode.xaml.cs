using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DissertationControls
{
    public sealed partial class TreemapNode : UserControl
    {
        // constants for the minimum node size
        // to check if the textblock should be hidden
        const int MIN_NODE_WIDTH = 15;
        const int MIN_NODE_HEIGHT = 30;
        const double TEXT_WIDTH_RATIO = 0.8;
        
        double _nodeWidth;
        double _nodeHeight;
        double _xPos;
        double _yPos;
        double _normalisedArea;
        string _value;
        string _name;
        string _description;
        string _colourValue;
        byte[] _colour;
        string _hierarchy;
        bool _searched;
        bool _selected;
        List<TreemapNode> _children;
        
        public TreemapNode()
        {
            this.InitializeComponent();
            _nodeWidth = 0;
            _nodeHeight = 0;
            _xPos = 0;
            _yPos = 0;
            _normalisedArea = 0;
            _value = "";
            _name = "";
            _description = "";
            _colourValue = "";
            _colour = new byte[3];
            _hierarchy = "";
            _searched = false;
            _selected = false;
            _children = new List<TreemapNode>();
        }

        public double NodeWidth
        {
            get { return _nodeWidth; }
            set
            {
                _nodeWidth = value;
                treemapRect.Width = value;

                // if node is not wide enough to contain a textblock, hide it
                if (value < MIN_NODE_WIDTH)
                {
                    treemapTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                else
                {
                    treemapTextBlock.Width = value * TEXT_WIDTH_RATIO;
                }
            }
        }

        public double NodeHeight
        {
            get { return _nodeHeight; }
            set
            {
                _nodeHeight = value;
                treemapRect.Height = value;

                // if node is not tall enough to contain a textblock, hide it
                if (value < MIN_NODE_HEIGHT)
                {
                    treemapTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }

        public double XPos
        {
            get { return _xPos; }
            set { _xPos = value; }
        }

        public double YPos
        {
            get { return _yPos; }
            set { _yPos = value; }
        }

        public double NormalisedArea
        {
            get { return _normalisedArea; }
            set { _normalisedArea = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string NodeName
        {
            get { return _name; }
            set
            {
                _name = value;
                treemapTextBlock.Text = value;
            }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string ColourValue
        {
            get { return _colourValue; }
            set { _colourValue = value; }
        }

        public byte[] Colour
        {
            get { return _colour; }
            set
            {
                _colour = value;
                treemapRect.Fill = new SolidColorBrush(Color.FromArgb(255, value[0], value[1], value[2]));
            }
        }

        public string Hierarchy
        {
            get { return _hierarchy; }
            set { _hierarchy = value; }
        }

        public bool Searched
        {
            get { return _searched; }
            set
            {
                _searched = value;
                if (_searched)
                {
                    treemapRect.Stroke = new SolidColorBrush(Colors.Yellow);
                    treemapRect.StrokeThickness = 3;
                }
                else
                {
                    treemapRect.Stroke = new SolidColorBrush(Colors.Gray);
                    treemapRect.StrokeThickness = 0.5;
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
                    treemapRect.Stroke = new SolidColorBrush(Colors.Red);
                    treemapRect.StrokeThickness = 3;
                }
                else
                {
                    // parent node check to reset appearance correctly
                    if (this.Children.Count <= 0)
                    {
                        treemapRect.Stroke = new SolidColorBrush(Colors.Gray);
                        treemapRect.StrokeThickness = 0.5;
                    }
                    else
                    {
                        treemapRect.Stroke = new SolidColorBrush(Colors.Red);
                        treemapRect.StrokeThickness = 1;
                    }
                }
            }
        }

        public List<TreemapNode> Children
        {
            get { return _children; }
            set { _children = value; }
        }


        public void AggregateValues()
        {
            double total = 0;

            foreach (TreemapNode child in this.Children)
            {
                total += child.NormalisedArea;
            }

            this.NormalisedArea = total;
        }


        // method which provides a different visual appearance to
        // parent 'container' rectangles of the hierarchy
        public void ParentCheck()
        {
            if (this.Children.Count > 0)
            {
                treemapRect.Stroke = new SolidColorBrush(Colors.Red);
                treemapRect.StrokeThickness = 1;
                treemapRect.Fill = new SolidColorBrush();
                // prevent parent's tooltips from obscuring their children
                treemapRect.IsHitTestVisible = false;
            }
            else
            {
                // hide the child textblocks to prevent overlap
                treemapTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }


        // method that sets the appearance of a zoomed in node
        public void ZoomedInView()
        {
            if (this.NodeHeight > MIN_NODE_HEIGHT && this.NodeWidth > MIN_NODE_WIDTH)
            {
                treemapTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }


        protected override void OnPointerEntered(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.Content = this.ToString();
            toolTip.Placement = Windows.UI.Xaml.Controls.Primitives.PlacementMode.Mouse;
            ToolTipService.SetToolTip(this, toolTip);
            toolTip.IsOpen = true;

            if (!this.Searched && !this.Selected)
            {
                treemapRect.Stroke = new SolidColorBrush(Colors.Yellow);
                treemapRect.StrokeThickness = 1;
            }
        }


        protected override void OnPointerExited(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ToolTip toolTip = (ToolTip)ToolTipService.GetToolTip(this);
            toolTip.IsOpen = false;

            // parent/selected/searched check to reset appearance correctly
            if (this.Children.Count <= 0)
            {
                if (!this.Searched && !this.Selected)
                {
                    treemapRect.Stroke = new SolidColorBrush(Colors.Gray);
                    treemapRect.StrokeThickness = 0.5;
                }
            }
            else
            {
                if (!this.Selected)
                {
                    treemapRect.Stroke = new SolidColorBrush(Colors.Red);
                    treemapRect.StrokeThickness = 1;
                }
            }
        }

        protected override void OnTapped(Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // toggle selection
            this.Selected = !this.Selected;
        }


        public override string ToString()
        {
            return String.Format("{0}\r{1}\r{2}\r{3}", this.Value, this.NodeName, this.Description, this.ColourValue);
        }
    }
}