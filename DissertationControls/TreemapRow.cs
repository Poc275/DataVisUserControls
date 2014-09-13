using System.Collections.Generic;

namespace DissertationControls
{
    // this class stores accepted TreemapNodes, which enables
    // the row to be 'locked' when the aspect ratio is no longer improved
    public class TreemapRow
    {
        List<TreemapNode> _nodes;

        public TreemapRow()
        {
            _nodes = new List<TreemapNode>();
        }

        public List<TreemapNode> Nodes
        {
            get { return _nodes; }
        }

        public void AddNodeToRow(TreemapNode node)
        {
            _nodes.Add(node);
        }
    }
}
