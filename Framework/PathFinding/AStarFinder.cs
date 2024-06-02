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
    /// 3.Smooth Path(Use Bezier,B ����)
    /// 4.PathFinding requeset ��ִ֡�ж��Ѱ·
    /// 
    ///
    public class AStarFinder  
    {
        private Func<Vector2Int, Vector2Int, int> heuristic;
        private AStarNode[,] map;//Ϊnull��Ԫ�ر�ʾΪ�ϰ���,��ͼ�ϵ�ķ�Χ:x��[0,width-1],y��[0,height-1]
        private int width, height;
        private HashSet<AStarNode> openSet;
        private HashSet<AStarNode> closeSet;
        private Vector2Int endPos;

        const int Factor = 10;
        const int DiagonalFactor = 14;
        /// <summary>
        /// weights:ÿ�����Ȩ��,Ϊint.MaxValue��ʾ�����ϰ���
        /// ����ͬ���Ȩ�ز�ͬ,��������10Ϊ��׼(�����븡�����������������
        /// useManhattan:�Ƿ�ʹ�������پ������h(Ĭ����ֱ�߶Խ���,�������Ĵ�һ��)
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
            //�����������ҵĵ�
            AddNearNode(-1, 0, startNode);
            AddNearNode(0, 1, startNode);
            AddNearNode(1, 0, startNode);
            AddNearNode(0, -1, startNode);
            //����б�ǵĵ�,������ϰ�����Ϊ�޷�����
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
            if(!IsValid(start) || !IsValid(end))//��ʼ���յ����ϰ����ڵ�ͼ��
                return null;

            MathUtil.Swap(ref start,ref end);//���������յ�,Ѱ·��ɺ�Ͳ��÷�ת·��
            AStarNode startNode = Reset(start, end);
            while (startNode.pos != end)
            {
                AddNearNodesToOpenSet(startNode);

                if (openSet.Count == 0)//��·��ͨ
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
        /// �Ƿ�����:�ڵ�ͼ���Ҳ�Ϊ�ϰ�
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
            {//�����б��Ѿ�����,���µ�g��С���޸���ǰ��ֵ
                if(newG < nearNode.g)
                {
                    nearNode.g = newG;
                    nearNode.parent = parentNode;
                }
            }
            else//���뿪���б�
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

