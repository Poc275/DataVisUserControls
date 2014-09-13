using System.Collections.Generic;

namespace DissertationControls
{
    // this class controls the treemap drawing area, which enables the 
    // remaining height/width and latest aspect ratio details to be stored
    public class TreemapDrawingArea
    {
        double _width;
        double _height;
        double _aspectRatio;
        double _rowSize;

        public TreemapDrawingArea()
        {
            _width = 0;
            _height = 0;
            _aspectRatio = 0;
        }

        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public double AspectRatio
        {
            get { return _aspectRatio; }
            set { _aspectRatio = value; }
        }

        // RowSize property stores the last accepted
        // width or height of the current row
        public double RowSize
        {
            get { return _rowSize; }
            set { _rowSize = value; }
        }
    }
}
