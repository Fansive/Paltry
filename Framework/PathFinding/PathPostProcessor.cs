using System;
using System.Collections.Generic;
using UnityEngine;

namespace Paltry
{
    public class PathPostProcessor
    {
        private Vector2 originPos;
        private float cellSize;

        public PathPostProcessor(Vector2 originPos, float cellSize)
        {
            this.originPos = originPos;
            this.cellSize = cellSize;
        }

        public static PathPostProcessor CreateFromGrid<T>(Grid<T> grid)
        {
            return new PathPostProcessor(grid.OriginPos, grid.CellSize);
        }

        private Vector2 ToWorldPoint(Vector2Int point)
        {
            return originPos + (Vector2)point * cellSize;
        }
        public List<Vector2> ToWorldPath(List<Vector2Int> path)
        {
            return path.ConvertAll<Vector2>(ToWorldPoint);
        }

        public List<Vector2> ToWaypoints(List<Vector2Int> path)
        {
            if (path.Count < 3)
                return ToWorldPath(path);

            List<Vector2> waypoints = new List<Vector2>();
            waypoints.Add(ToWorldPoint(path[0]));
            for (int i = 1; i < path.Count-1; i++)
            {
                Vector2Int p1 = path[i-1], p2 = path[i], p3 = path[i+1];
                if (p1 + p3 != 2 * p2)
                    waypoints.Add(ToWorldPoint(p2));
            }
            waypoints.Add(ToWorldPoint(path[^1]));
            return waypoints;
        }

    }
}
