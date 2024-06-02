using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Paltry
{
    public class Grid<T>
    {
        private T[,] cells;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public float CellSize { get; private set; }
        public Vector2 OriginPos { get;private set; }
        public Grid(int width, int height, float cellSize, Vector2 originPos)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            OriginPos = originPos;
            cells = new T[width, height];
        }

        /// <summary>
        /// XY坐标(网格系统坐标)对应的网格单元
        /// </summary>
        public T this[int x,int y]
        {
            get { return cells[x, y];}
            set { cells[x, y] = value; }
        }
        /// <summary>
        /// 世界坐标对应的网格单元
        /// </summary>
        public T this[Vector2 worldPos]
        {
            get { var XY = GetXY(worldPos); return cells[XY.x,XY.y];}
            set { var XY = GetXY(worldPos); cells[XY.x, XY.y] = value; }
        }
        public Vector2 GetWorldPos(int x, int y)
        {
            return OriginPos + new Vector2(x,y) * CellSize;
        }
        public (int x,int y) GetXY(Vector2 worldPos)//若以XY坐标为物体中心,通过WorldPos获取时可能需引入偏移量
        {
            var XY = (worldPos - OriginPos) / CellSize;
            return ((int)XY.x, (int)XY.y);
        }
        public bool IsValid(int x,int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        } 
        public bool IsValid(Vector2 worldPos)
        {
            (int x,int y) = GetXY(worldPos);
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

    }

}
