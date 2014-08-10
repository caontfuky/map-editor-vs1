using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Mapeditor
{
    public class QuadTree
    {
        public QuadNode _root;

        public QuadTree(Rectangle bound)
        {
            this._root = new QuadNode(1, bound);
        }

        public void buildQuadTree(List<GameObject> listObject)
        {
            this._root.buildQuadNode(listObject);
        }

        public void export(QuadNode node, List<System.Text.StringBuilder> result)
        {
            //List<string> _Result = new List<string>();
            System.Text.StringBuilder item = new System.Text.StringBuilder("");
            item.Append(String.Format("{0}\t{1}\t{2}\t{3}\t{4}",
                                    node._id.ToString().Trim(),
                                    node._bounds.X.ToString().Trim(),
                                    node._bounds.Y.ToString().Trim(),
                                    node._bounds.Width.ToString().Trim(),
                                    node._bounds.Height.ToString().Trim()));
            if (node._nodeBL != null)
            {
                export(node._nodeBL, result);
                export(node._nodeBR, result);
                export(node._nodeTL, result);
                export(node._nodeTR, result);
            }
            else
            {
                for (int i = 0; i < node._listObject.Count;++i )
                {
                    item.Append( "\t" + node._listObject[i]._id.ToString().Trim());
                }
            }
            result.Add(item);
        }
    }

}
