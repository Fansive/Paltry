using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;

namespace Paltry
{
    /// Consider:
    /// 1.(Smooth)Weights(OK)
    /// 2.Path to WayPoint(suitable rotate)
    /// 3.Smooth Path(Use Bezier,B 样条)
    /// 4.PathFinding requeset 分帧执行多个寻路
    /// 
    ///
    public class AStarFinder  
    {
        private Func<Vector2Int, Vector2Int, int> heuristic;
        private AStarNode[,] map;//为null的元素表示为障碍物,地图上点的范围:x在[0,width-1],y在[0,height-1]
        private int width, height;
        private HashSet<AStarNode> openSet;
        private HashSet<AStarNode> closeSet;
        private Vector2Int endPos;

        const int Factor = 10;
        const int DiagonalFactor = 14;
        /// <summary>
        /// weights:每个点的权重,为int.MaxValue表示它是障碍物
        /// 若不同点的权重不同,请至少以10为基准(不引入浮点数以提高运算性能
        /// useManhattan:是否使用曼哈顿距离计算h(默认是直线对角线,性能消耗大一点)
        /// </summary>
        public AStarFinder(int width,int height,int[,]weights,bool useManhattan=false)
        {
            this.width = width;
            this.height = height;
            openSet= new HashSet<AStarNode>();
            closeSet= new HashSet<AStarNode>();
            heuristic = useManhattan ? Heuristic_Manhattan : Heuristic_Diagonal;
            map = new AStarNode[width,height];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    map[i, j] = weights[i, j]==int.MaxValue 
                        ? null : new AStarNode(i, j, weights[i,j]);
        }

        private AStarNode Reset(Vector2Int start,Vector2Int end)
        {
            endPos = end;
            openSet.Clear();
            closeSet.Clear();
            AStarNode startNode = map[start.x, start.y];
            startNode.Reset();
            closeSet.Add(startNode);
            return startNode;
        }
        private void AddNearNodesToOpenSet(AStarNode startNode)
        {
            //加入上下左右的点
            AddNearNode(-1, 0, startNode);
            AddNearNode(0, 1, startNode);
            AddNearNode(1, 0, startNode);
            AddNearNode(0, -1, startNode);
            //加入斜角的点,如果有障碍则认为无法到达
            bool left = IsValid(startNode.pos + Vector2Int.left);
            bool up = IsValid(startNode.pos + Vector2Int.up);
            bool right = IsValid(startNode.pos + Vector2Int.right);
            bool down = IsValid(startNode.pos + Vector2Int.down);
            if (left && up)
                AddNearNode(-1, 1, startNode);
            if (up && right)
                AddNearNode(1, 1, startNode);
            if (right && down)
                AddNearNode(1, -1, startNode);
            if (down && left)
                AddNearNode(-1, -1, startNode);
        }
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            if(!IsValid(start) || !IsValid(end))//开始或终点是障碍或在地图外
                return null;

            MathUtil.Swap(ref start,ref end);//交换起点和终点,寻路完成后就不用翻转路径
            AStarNode startNode = Reset(start, end);
            while (startNode.pos != end)
            {
                AddNearNodesToOpenSet(startNode);

                if (openSet.Count == 0)//此路不通
                    return null;

                startNode = openSet.Min<AStarNode>();
                openSet.Remove(startNode);
                closeSet.Add(startNode);
            }

            List<Vector2Int> path = new List<Vector2Int>();
            while(startNode != null)
            {
                path.Add(startNode.pos);
                startNode = startNode.parent;
            }
            return path;
        }

        /// <summary>
        /// 是否满足:在地图内且不为障碍
        /// </summary>
        private bool IsValid(Vector2Int point)
        {
            return point.x >= 0 && point.x < width && point.y >= 0 && point.y < height
                && map[point.x, point.y] != null;
        }
        private void AddNearNode(int offsetX,int offsetY,AStarNode parentNode)
        {
            Vector2Int nearPos = parentNode.pos + new Vector2Int(offsetX, offsetY);
            if (!IsValid(nearPos) || closeSet.Contains(map[nearPos.x,nearPos.y]))
                return;

            AStarNode nearNode = map[nearPos.x, nearPos.y];
            int factor = offsetX*offsetY == 0 ? Factor : DiagonalFactor;
            int newG = parentNode.g + factor;
            if (openSet.Contains(nearNode))
            {//开启列表已经有了,若新的g更小就修改先前的值
                if(newG < nearNode.g)
                {
                    nearNode.g = newG;
                    nearNode.parent = parentNode;
                }
            }
            else//加入开启列表
            {
                nearNode.g = newG;
                nearNode.h = heuristic(nearPos, endPos);
                nearNode.parent = parentNode;
                openSet.Add(nearNode);
            }
        }
        private int Heuristic_Diagonal(Vector2Int pointA, Vector2Int pointB)
        {
            int x = Math.Abs(pointA.x - pointB.x);
            int y = Math.Abs(pointA.y - pointB.y);
            return Factor*Math.Abs(x-y) + DiagonalFactor*Math.Min(x,y);
        }
        private int Heuristic_Manhattan(Vector2Int pointA,Vector2Int pointB)
        {
            return Factor*(Math.Abs(pointA.x-pointB.x) + Math.Abs(pointA.y-pointB.y));
        }
    }
}

