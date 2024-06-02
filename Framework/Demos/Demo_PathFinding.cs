using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Paltry.Demo
{
    public class Demo_PathFinding : MonoBehaviour
    {
        public static Demo_PathFinding Instance;
        public Demo_PathFindingMapSO Map;
        public GameObject Cell;
        private Demo_AstarFinder finder;
        private DemoState state;
        public Grid<SpriteRenderer> grid;
        private IEnumerator process;
        public bool isPaused;
        private Timer moveTimer;
        public GameObject canvas;
        public float cellSize;
        void Start()
        {
            Instance = this;
            state = DemoState.Editing;
            moveTimer = new Timer(1/Map.Speed, isLooped: false);
            Map.Obstacles = new bool[Map.Width, Map.Height];

            var obstacles = Map.Obstacles;
            grid = new Grid<SpriteRenderer>(Map.Width, Map.Height, cellSize, Vector2.one*1f);
            for (int i = 0; i < Map.Width; i++)
            {
                for (int j = 0; j < Map.Height; j++)
                {
                    var cell = Instantiate(Cell, transform);
                    cell.transform.localScale = Vector2.one * cellSize;
                    cell.transform.position = grid.GetWorldPos(i, j);
                    grid[i, j] = cell.GetComponent<SpriteRenderer>();
                    grid[i, j].color = obstacles[i, j]
                        ? Map.ObstacleColor : Map.NormalColor;
                }
            } 
        }

        public void Init() 
        {
            finder = new Demo_AstarFinder(Map.Width, Map.Height, Map.Obstacles, Map.useManhattan);
            process = finder.FindPath(Map.Start, Map.End);
        }
        void Update()
        {
            if(state == DemoState.Editing)
            {
                moveTimer.Stop();
                 var mousePos = Camera.main.ScreenToWorldPoint
                    (new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10));
                mousePos += Vector3.one * grid.CellSize / 2;
                 (int x,int y) = grid.GetXY(mousePos);
                if (!grid.IsValid(mousePos))
                    return;
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    grid[mousePos].GetComponent<SpriteRenderer>().color = Map.ObstacleColor;
                    Map.Obstacles[x, y] = true;
                }
                else if(Input.GetKeyDown(KeyCode.Mouse1))
                {//右键起点
                    Map.Start = new Vector2Int(x, y);
                    grid[mousePos].GetComponent<SpriteRenderer>().color = Map.StartColor;
                }
                else if(Input.GetKeyDown(KeyCode.Mouse2))
                {//中键终点
                    Map.End = new Vector2Int(x, y);
                    grid[mousePos].GetComponent<SpriteRenderer>().color = Map.EndColor;
                }
            }
            
            else if(state == DemoState.Running && !isPaused)
            { 
                if (moveTimer.IsFinished)
                {
                    Move();
                    moveTimer.Restart();
                }
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                canvas.SetActive(!canvas.activeSelf);
            }
        }

        public void ResetProcess()
        {
            Init();
            var obstacles = Map.Obstacles;
            for (int i = 0; i < Map.Width; i++)
                for (int j = 0; j < Map.Height; j++)
                    grid[i, j].color = obstacles[i, j]
                        ? Map.ObstacleColor : Map.NormalColor;
            state = DemoState.Editing;
        }
        public void StartFinding()
        {
            Init();
            moveTimer.Start();
            state = DemoState.Running;
        }
        public void Pause()
        {
            isPaused = !isPaused;
        }
        public void NextStep()
        {
            Move();
        }
        public void SpeedUp()
        {
            float maxTime = moveTimer.MaxTime;
            moveTimer.Modify(maxTime / 2, false);
        }
        public void SpeedSlow()
        {
            float maxTime = moveTimer.MaxTime;
            moveTimer.Modify(maxTime * 2, false);
        }
        public void Max()
        {
            moveTimer.Modify(0, false);
        }
        public void Min()
        {
            moveTimer.Modify(.1f, false);
        }
        private void Move()
        {
            process.MoveNext();
        }
        enum DemoState
        {
            None,
            Editing,
            Running
        }
    }
    public class Demo_AstarFinder
    {
        private Func<Vector2Int, Vector2Int, int> heuristic;
        private AStarNode[,] map;//为null的元素表示为障碍物,地图上点的范围:x在[0,width-1],y在[0,height-1]
        private int width, height;
        private HashSet<AStarNode> openSet;
        private HashSet<AStarNode> closeSet;
        private Vector2Int endPos;
        private Vector2Int startPos;

        const int Factor = 10;
        const int DiagonalFactor = 14;
        int counter;
        public Demo_AstarFinder(int width, int height, bool[,] obstacles, bool useManhattan = false)
        {
            this.width = width;
            this.height = height;
            openSet= new HashSet<AStarNode>();
            closeSet= new HashSet<AStarNode>();
            heuristic = useManhattan ? Heuristic_Manhattan : Heuristic_Diagonal;
            map = new AStarNode[width,height];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    map[i, j] = obstacles[i, j] ? null : new AStarNode(i,j,1);
        }

        private AStarNode Reset(Vector2Int start, Vector2Int end)
        {
            endPos = end;
            startPos = start;
            openSet.Clear();
            closeSet.Clear();
            AStarNode startNode = map[start.x, start.y];
            startNode.Reset();
            closeSet.Add(startNode);
            Draw(start, MapColor.Close);
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
        
        public IEnumerator FindPath(Vector2Int start, Vector2Int end)
        {
            if (!IsValid(start) || !IsValid(end))//开始或终点是障碍或在地图外
                yield break;

            counter = 0;
            AStarNode startNode = Reset(start, end);
            while (startNode.pos != end)
            {
                AddNearNodesToOpenSet(startNode);
                
                if (openSet.Count == 0)//此路不通
                    yield break;

                startNode = openSet.Min<AStarNode>();
                openSet.Remove(startNode);
                closeSet.Add(startNode);
                Draw(startNode.pos, MapColor.Close);
                counter++;
                yield return null;
            }

            List<Vector2Int> path = new List<Vector2Int>();
            while (startNode != null)
            {
                path.Add(startNode.pos);
                Draw(startNode.pos, MapColor.Path);
                startNode = startNode.parent;
                yield return null;
            }
            Debug.Log("Iterations:" + counter);
        }

        /// <summary>
        /// 是否满足:在地图内且不为障碍
        /// </summary>
        private bool IsValid(Vector2Int point)
        {
            return point.x >= 0 && point.x < width && point.y >= 0 && point.y < height
                && map[point.x, point.y] != null;
        }
        private void AddNearNode(int offsetX, int offsetY, AStarNode parentNode)
        {
            Vector2Int nearPos = parentNode.pos + new Vector2Int(offsetX, offsetY);
            if (!IsValid(nearPos) || closeSet.Contains(map[nearPos.x, nearPos.y]))
                return;

            AStarNode nearNode = map[nearPos.x, nearPos.y];
            int factor = offsetX * offsetY == 0 ? Factor : DiagonalFactor;
            int newG = parentNode.g + factor;
            if (openSet.Contains(nearNode))
            {//开启列表已经有了,若新的g更小就修改先前的值
                if (newG < nearNode.g)
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
            Draw(nearPos, MapColor.Open);
        }
        private void Draw(Vector2Int pos, MapColor color)
        {
            if (pos == endPos || pos == startPos)
                return;
            var inst = Demo_PathFinding.Instance;
            inst.grid[pos.x,pos.y].color = color switch
            {
                MapColor.Normal => inst.Map.NormalColor,
                MapColor.Open => inst.Map.OpenListColor,
                MapColor.Close => inst.Map.CloseListColor,
                MapColor.Path => inst.Map.PathColor
            };
        }
        private int Heuristic_Diagonal(Vector2Int pointA, Vector2Int pointB)
        {
            int x = Math.Abs(pointA.x - pointB.x);
            int y = Math.Abs(pointA.y - pointB.y);
            return Factor * Math.Abs(x - y) + DiagonalFactor * Math.Min(x, y);
        }
        private int Heuristic_Manhattan(Vector2Int pointA, Vector2Int pointB)
        {
            return Factor * (Math.Abs(pointA.x - pointB.x) + Math.Abs(pointA.y - pointB.y));
        }
        enum MapColor
        {
            Normal,
            Open,
            Close,
            Path
        }
    }

}

