using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Mapeditor
{
    public class QuadNode
    {
        public const int MIN_SIZE_OF_NODE = 400;

        public int _id; //Id của node
        public Rectangle _bounds; // vị trí của node
        public List<GameObject> _listObject; // Danh sách các đối tượng trong node
        public QuadNode _nodeTL; // Node con left top
        public QuadNode _nodeTR; // Node con right top
        public QuadNode _nodeBL; // Node con left bottom
        public QuadNode _nodeBR; // Node con right bottom

        public QuadNode(int id, Rectangle bounds)
        {
            this._id = id;
            this._bounds = bounds;
            this._listObject = new List<GameObject>();
        }

        public void buildQuadNode(List<GameObject> listObject)
        {
            //if (Rectangle.Intersect(this.m_Bounds, _obj.m_Bounds).Height == 0) //Kiểm tra bounds của Object và Node có tiếp xúc không
            //    return;
            if (listObject.Count == 0)
                return;
            if (this._bounds.Height > MIN_SIZE_OF_NODE ) //  Kiểm tra kích thước node có vượt giới hạn hay không
            {
                _nodeTL = new QuadNode(this._id * 8 + 1, new Rectangle(this._bounds.X, this._bounds.Y, this._bounds.Width / 2, this._bounds.Height / 2));
                _nodeTR = new QuadNode(this._id * 8 + 2, new Rectangle(this._bounds.X + this._bounds.Width / 2, this._bounds.Y, this._bounds.Width / 2, this._bounds.Height / 2));
                _nodeBL = new QuadNode(this._id * 8 + 3, new Rectangle(this._bounds.X, this._bounds.Y + this._bounds.Height / 2, this._bounds.Width / 2, this._bounds.Height / 2));
                _nodeBR = new QuadNode(this._id * 8 + 4, new Rectangle(this._bounds.X + this._bounds.Width / 2, this._bounds.Y + this._bounds.Height / 2, this._bounds.Width / 2, this._bounds.Height / 2));

                clip(listObject, _nodeTL);
                clip(listObject, _nodeTR);
                clip(listObject, _nodeBL);
                clip(listObject, _nodeBR);

                _nodeTL.buildQuadNode(_nodeTL._listObject);
                _nodeTR.buildQuadNode(_nodeTR._listObject);
                _nodeBL.buildQuadNode(_nodeBL._listObject);
                _nodeBR.buildQuadNode(_nodeBR._listObject);

                this._listObject.Clear();
            }
            else
                return; //Nếu kích thước của node <= MINSIZEOFNODE => đây là node lá
        }
        public void clip(List<GameObject> listObject, QuadNode node)
        {
            foreach (GameObject objectItem in listObject)
            {
                if (Rectangle.Intersect(objectItem._bounds,node._bounds).Height != 0)
                {
                    node._listObject.Add(objectItem);
                }
            }
        }
    }
    public class GameObject
    {
        public int _id;
        public Rectangle _bounds;
        public string _type;

        public GameObject(int id, Rectangle bounds, string type)
        {
            this._id = id;
            this._bounds = bounds;
            this._type = type;
        }
    }
}
