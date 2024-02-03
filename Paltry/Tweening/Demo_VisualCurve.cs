using Paltry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TinaX;
using TMPro;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

namespace Paltry.Demo
{
    /// <summary>
    /// 将该脚本挂载在任意位置在原点的游戏物体上并设置好参数即可观察曲线函数效果
    /// <para></para>
    /// </summary>
    public class Demo_VisualCurve : MonoBehaviour
    {
        public float time;
        public float scale;
        public float Scale
        {
            get
            {
                return scale * 0.17f;//内部调整因子
            }
            set
            {
                scale = value;
            }
        }
        public float curveSpace;//曲线之间的间距
        public Color startColor;
        public Color endColor;
        public Color circleColor;
        public bool hideCircle;

        public Material lineMaterial;
        public Sprite circleSprite;

        private int curveCount;//曲线总数
        private int sideLength;//曲线总框架的边长,即一行有几个
        private List<Curve> curves;
        private List<Guid> curveIds;
        private bool circleIsShown;
        private Canvas canvas;
        [HideInInspector] public static bool ClickToPlay;
        void Start()
        {
            //过滤翻转曲线
            curveIds = new List<Guid>();
            curves = new List<Curve>(((IEnumerable<Curve>)Enum.GetValues(typeof(Curve))).Where(c => (int)c % 2 == 1));
            curveCount = curves.Count;
            sideLength = Mathf.CeilToInt(Mathf.Sqrt(curveCount));
            DrawCurves();
        }

        void Update()
        {
            if (hideCircle && circleIsShown)
            {
                foreach (var item in GetComponentsInChildren<SpriteRenderer>())
                    item.enabled = false;
                circleIsShown = false;
            }
            else if (!hideCircle && !circleIsShown)
            {
                foreach (var item in GetComponentsInChildren<SpriteRenderer>())
                    item.enabled = true;
                circleIsShown = true;
            }

            if (ClickToPlay)
            {
                ClickToPlay = false;
                foreach (var item in curveIds)
                {
                    Tweening.Cancel(item);
                }
                curveIds.Clear();
                for (int i = 0; i < transform.childCount; i++)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
                DrawCurves();
            }
        }
        LineRenderer CreateLineRenderer(Vector3 startPoint, string curveName)
        {
            GameObject go = new GameObject(curveName);
            go.SetParent(this.gameObject);
            go.SetLocalPosition(startPoint);
            LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 0;
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
            lineRenderer.material = lineMaterial;
            lineRenderer.startWidth = Scale * 0.05f;
            lineRenderer.endWidth = Scale * 0.05f;
            return lineRenderer;
        }
        void CreateCanvas()
        {
            GameObject go = new GameObject("Canvas");
            go.SetParent(this.gameObject);
            canvas = go.AddComponent<Canvas>();
            go.AddComponent<CanvasScaler>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.localPosition = new Vector3(0.5f, -0.15f, 0);
        }
        void DrawCurves()
        {
            CreateCanvas();

            circleIsShown = true;
            Vector2 DrawStartPoint = transform.position;
            int countOneLine = 0; //当前行已有的曲线数量,用来指示换行
            int index = 0;
            foreach (Curve curve in curves)
            {
                Vector2 constDrawStartPoint = DrawStartPoint;
                LineRenderer LR = CreateLineRenderer(DrawStartPoint, curve.ToString());
                int posIndex = 0;

                //创建文字
                GameObject textObj = new GameObject();
                textObj.SetParent(canvas.gameObject);
                TMP_Text text = textObj.AddComponent<TextMeshPro>();
                text.text = curveTipDict[curve];
                text.rectTransform.sizeDelta = new Vector2(450, 100);
                text.rectTransform.localPosition = DrawStartPoint - new Vector2(0.5f, 0);
                text.rectTransform.pivot = Vector2.up;
                text.rectTransform.localScale *= 0.02233452f;
                text.fontSize = 32;
                text.alpha = 0;
                //创建小球
                var circle = new GameObject();
                circle.SetParent(LR.gameObject);
                var sr = circle.AddComponent<SpriteRenderer>();
                sr.sprite = circleSprite;
                sr.color = circleColor;
                Vector2 circleStart = DrawStartPoint - new Vector2(0.3f * Scale, 0);
                circle.SetPosition(circleStart);
                circle.SetLocalScale(0.05f * Scale * Vector3.one);

                Guid id = Tweening.DoOnce(time, Curve.Linear, (x) =>
                {
                    Func<float, float> f = Tweening.CurveDict[curve];
                    LR.positionCount++;
                    LR.SetPosition(posIndex, constDrawStartPoint + new Vector2(x, f(x)));
                    circle.transform.position = circleStart + new Vector2(0, f(x));
                    text.alpha = f(x);
                    posIndex++;
                }, () =>
                {
                    if (text.alpha < 0.1f)
                    {
                        text.alpha = 1;
                    }
                });

                curveIds.Add(id);
                DrawStartPoint += new Vector2(curveSpace, 0);
                countOneLine++;
                index++;
                //这里并不需要设置两个变量来表示曲线个数(用于排版),直接利用index%sideLength和index/sideLength就行了
                if (countOneLine == sideLength)
                {
                    countOneLine = 0;
                    DrawStartPoint = new Vector2(transform.position.x, DrawStartPoint.y - curveSpace);
                }
            }

        }

        private Dictionary<Curve, string> curveTipDict = new Dictionary<Curve, string>()
        {
            {Curve.Linear,$"{Curve.Linear}:f(x)=x" },
            {Curve.Sin,$"{Curve.Sin}:f(x)=0.5(1-cos2πx)" },
            {Curve.Expo,$"{Curve.Expo}:f(x)=2^[10(x-1)]" },
            {Curve.BackEase,$"{Curve.BackEase}:f(x)=x^3-0.3x*sinπx" },
            {Curve.SinEase,$"{Curve.SinEase}:f(x)=sin(0.5πx)" },
            {Curve.Elastic,$"{Curve.Elastic}:f(x)=100^x*sin(8.5πx)/100" },
            {Curve.Bounce,$"{Curve.Bounce}" },
        };
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(Demo_VisualCurve))]
    public class Demo_VisualCurve_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("播放"))
            {
                Demo_VisualCurve.ClickToPlay = true;
            }
        }
    }
#endif
}