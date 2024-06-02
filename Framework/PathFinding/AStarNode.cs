using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Paltry
{
    public class AStarNode : IComparable<AStarNode>
    {
        public int  g, h;
        public int f => (g + h) * weight;
        public AStarNode parent;
        public readonly Vector2Int pos;
        private readonly int weight;
        public AStarNode(int x,int y,int weight)
        {
            this.pos = new Vector2Int(x, y);
            this.weight = weight;
        }
        public int CompareTo(AStarNode other)
        {
            if (f > other.f)
                return 1;
            else if (f < other.f)
                return -1;
            else
                return h.CompareTo(other.h); 
        }

        public void Reset()
        {
            g = h = 0;
            parent = null;
        }

    }

}
