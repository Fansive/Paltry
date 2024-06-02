using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paltry.Demo
{
    [CreateAssetMenu(fileName = "Demo_PathFindingMap", menuName ="ScriptObjects/PathFindingMap",order =1)]
    public class Demo_PathFindingMapSO : ScriptableObject
    {
        public Vector2Int Start,End;
        public int Width, Height;
        public bool useManhattan;
        public bool[,] Obstacles;
        public Color NormalColor;
        public Color ObstacleColor;
        public Color OpenListColor;
        public Color CloseListColor;
        public Color PathColor;
        public Color StartColor;
        public Color EndColor;
        public float Speed;
    }
}

