using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;


namespace DissertationControls
{
    public sealed partial class RangeSlider : UserControl
    {
        public event EventHandler<ValueChangedEventArgs> UpperValueChangedEvent;
        public event EventHandler<ValueChangedEventArgs> LowerValueChangedEvent;
        ValueChangedEventArgs args;

        double _max;
        double _min;
        double _normalisationFactor;

        // Dependency properties
        public static DependencyProperty UpperValueProperty { private set; get; }
        public static DependencyProperty LowerValueProperty { private set; get; }

        public RangeSlider()
        {
            this.InitializeComponent();
            _max = 1.0;
            _min = 0.0;
            _normalisationFactor = 0.0;
            args = new ValueChangedEventArgs();
        }

        static RangeSlider()
        {
            UpperValueProperty = DependencyProperty.Register("UpperValue",
                                        typeof(double),
                                        typeof(RangeSlider),
                                        new PropertyMetadata(1.0));

            LowerValueProperty = DependencyProperty.Register("LowerValue",
                                        typeof(double),
                                        typeof(RangeSlider),
                                        new PropertyMetadata(0.0));
        }


        // Properties
        public double UpperValue
        {
            get { return (double)GetValue(UpperValueProperty); }
            set { SetValue(UpperValueProperty, value); }
        }

        public double LowerValue
        {
            get { return (double)GetValue(LowerValueProperty); }
            set { SetValue(LowerValueProperty, value); }
        }

        public double Max
        {
            get { return _max; }
            set
            {
                if (value > this.Min)
                {
                    _max = value;
                    this.UpperValue = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Max Property", "The maximum value must be greater than the minimum");
                }
            }
        }

        public double Min
        {
            get { return _min; }
            set
            {
                if (value < this.Max)
                {
                    _min = value;
                    this.LowerValue = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Min Property", "The minimum value must be less than the maximum");
                }
            }
        }


        // Events
        private void RangeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            // Normalise range to slider height
            double maxMinDiff = this.Max - this.Min;
            _normalisationFactor = maxMinDiff / this.ActualHeight;
        }

        private void UpperThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            RowDefinition upperRect = root.RowDefinitions.ElementAt(0);
            RowDefinition lowerRect = root.RowDefinitions.ElementAt(4);
            double lowerRectHeight = lowerRect.Height.Value;
            double remainingTrackSpace = this.ActualHeight - lowerRectHeight - UpperThumb.ActualHeight;

            double upperRectHeight = upperRect.Height.Value;
            double updatedUpperRectHeight = upperRectHeight += e.VerticalChange;

            // if the new height is larger than the available space, cancel the drag
            // otherwise the upper and lower values will overlap
            if (updatedUpperRectHeight > remainingTrackSpace)
            {
                UpperThumb.CancelDrag();
            }
            else
            {
                if (updatedUpperRectHeight > 0)
                {
                    upperRect.Height = new GridLength(updatedUpperRectHeight);
                }
            }
        }

        private void UpperThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            double upperRectHeight = root.RowDefinitions.ElementAt(0).Height.Value;
            double deltaChange = upperRectHeight * _normalisationFactor;
            this.UpperValue = this.Max - deltaChange;

            // update event args with new value and fire the event
            args.NewValue = this.UpperValue;
            OnUpperValueChanged(args);
        }

        private void LowerThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            RowDefinition lowerRect = root.RowDefinitions.ElementAt(4);
            RowDefinition upperRect = root.RowDefinitions.ElementAt(0);
            double upperRectHeight = upperRect.Height.Value;
            double remainingTrackSpace = this.ActualHeight - upperRectHeight - LowerThumb.ActualHeight;

            double lowerRectHeight = lowerRect.Height.Value;
            double inverseVertChange = e.VerticalChange * -1.0;
            double updatedLowerRectHeight = lowerRectHeight += inverseVertChange;

            if (updatedLowerRectHeight > remainingTrackSpace)
            {
                LowerThumb.CancelDrag();
            }
            else
            {
                if (updatedLowerRectHeight > 0)
                {
                    lowerRect.Height = new GridLength(updatedLowerRectHeight);
                }
            }
        }

        private void LowerThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            double lowerRectHeight = root.RowDefinitions.ElementAt(4).Height.Value;
            double deltaChange = lowerRectHeight * _normalisationFactor;
            this.LowerValue = this.Min + deltaChange;

            // update event args with new value and fire the event
            args.NewValue = this.LowerValue;
            OnLowerValueChanged(args);
        }


        // Custom events for when the upper/lower values are changed
        // Check if the events are subscribed to before firing
        public void OnUpperValueChanged(ValueChangedEventArgs args)
        {
            if (UpperValueChangedEvent != null)
            {
                UpperValueChangedEvent(this, args);
            }
        }

        public void OnLowerValueChanged(ValueChangedEventArgs args)
        {
            if (LowerValueChangedEvent != null)
            {
                LowerValueChangedEvent(this, args);
            }
        }

        // method that resets the thumbs back to max/min
        public void ResetRangeSlider()
        {
            RowDefinition lowerRect = root.RowDefinitions.ElementAt(4);
            RowDefinition upperRect = root.RowDefinitions.ElementAt(0);

            lowerRect.Height = new GridLength(0);
            upperRect.Height = new GridLength(0);

            this.UpperValue = this.Max;
            this.LowerValue = this.Min;

            // fire lower and upper value changed events
            args.NewValue = this.LowerValue;
            OnLowerValueChanged(args);
            args.NewValue = this.UpperValue;
            OnUpperValueChanged(args);
        }
    }


    // Custom EventArgs class which enables the new value to be retrieved
    public class ValueChangedEventArgs : EventArgs
    {
        public ValueChangedEventArgs() { }

        public double NewValue { get; set; }
    }
}
