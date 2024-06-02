///改动处:
/// 1.<see cref="Platinio.TweenEngine.TweenPool.Static_Init"/>
///添加了该方法用于初始化静态变量,该方法应在MonoAgent.Awake中调用
///
/// 2.<see cref="Platinio.TweenEngine.TweenPool.GenerateId"/>
///取消了捕获溢出异常的做法,改为取余
///
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Platinio.TweenEngine
{
    public static class TweenPool
    {
        static List<ValueTween> valueTweens = new List<ValueTween>();
        static List<Vector2Tween> vector2Tweens = new List<Vector2Tween>();
        static List<Vector3Tween> vector3Tweens = new List<Vector3Tween>();
        static List<MoveTween> moveTweens = new List<MoveTween>();
        static List<ColorTween> colorTweens = new List<ColorTween>();
        static List<QuaternionTween> quaternionTweens = new List<QuaternionTween>();

        static int counter = 0;

        static List<BaseTween> _activeTweens = new List<BaseTween>();
        public static List<BaseTween> activeTweens;

        /// <summary>
        /// 避免域重载重置添加的
        /// </summary>
        public static void Static_Init()
        {
            valueTweens = new List<ValueTween>();
            vector2Tweens = new List<Vector2Tween>();
            vector3Tweens = new List<Vector3Tween>();
            moveTweens = new List<MoveTween>();
            colorTweens = new List<ColorTween>();
            quaternionTweens = new List<QuaternionTween>();
            _activeTweens = new List<BaseTween>();
            activeTweens = _activeTweens;
        }

        static TweenPool()
        {
            activeTweens = _activeTweens;
        }

        private static int GenerateId()
        {
            counter = (counter + 1) % int.MaxValue;
            //try
            //{
            //    counter++;
            //}
            //catch (OverflowException)
            //{
            //    counter = 0;
            //}

            return counter;
        }
        public static void FinishTween(BaseTween tween)
        {
            _activeTweens.Remove(tween);
            AddTweenToPool(tween);
        }

        private static void AddTweenToPool(BaseTween tween)
        {
            if (tween is ValueTween)
            {
                valueTweens.Add(tween as ValueTween);
            }
            else if (tween is MoveTween)
            {
                moveTweens.Add(tween as MoveTween);
            }
            else if (tween is Vector2Tween)
            {
                vector2Tweens.Add(tween as Vector2Tween);
            }
            else if (tween is Vector3Tween)
            {
                vector3Tweens.Add(tween as Vector3Tween);
            }
            else if (tween is ColorTween)
            {
                colorTweens.Add(tween as ColorTween);
            }
            else if (tween is QuaternionTween)
            {
                quaternionTweens.Add(tween as QuaternionTween);
            }

        }

        private static bool TryGetTween<T>(List<T> list, out T tween) where T : BaseTween
        {
            if (list.Count > 0)
            {
                int last = list.Count - 1;
                tween = list[last];
                list.RemoveAt(last);
                tween.Reset();
                return true;
            }
            else
            {
                tween = null;
                return false;
            }
        }

        public static ValueTween GetValueTween(float start, float end, float t)
        {
            ValueTween tween;
            if (TryGetTween(valueTweens, out tween))
            {
                tween.Reset();
                tween.Init(start, end, t);
            }
            else
            {
                tween = new ValueTween(start, end, t, GenerateId());
            }

            _activeTweens.Add(tween);
            return tween;
        }

        internal static MoveTween GetMoveTween(Transform obj, Transform to, float t)
        {
            MoveTween tween;
            if (TryGetTween(moveTweens, out tween))
            {
                tween.Init(obj, to, t);
            }
            else
            {
                tween = new MoveTween(obj, to, t, GenerateId());
            }

            _activeTweens.Add(tween);
            return tween;
        }

        internal static Vector3Tween GetVector3Tween(Vector3 from, Vector3 to, float time)
        {
            Vector3Tween tween;
            if (TryGetTween(vector3Tweens, out tween))
            {
                tween.Init(from, to, time);
            }
            else
            {
                tween = new Vector3Tween(from, to, time, GenerateId());
            }
            _activeTweens.Add(tween);
            return tween;
        }

        internal static Vector2Tween GetVector2Tween(Vector2 from, Vector2 to, float t)
        {
            Vector2Tween tween;
            if (TryGetTween(vector2Tweens, out tween))
            {
                tween.Init(from, to, t);
            }
            else
            {
                tween = new Vector2Tween(from, to, t, GenerateId());
            }

            _activeTweens.Add(tween);
            return tween;
        }

        internal static ColorTween GetColorTween(Color from, Color to, float t)
        {
            ColorTween tween;
            if (TryGetTween(colorTweens, out tween))
            {
                tween.Init(from, to, t);
            }
            else
            {
                tween = new ColorTween(from, to, t, GenerateId());
            }
            _activeTweens.Add(tween);
            return tween;
        }

        internal static QuaternionTween GetQuaternionTween(Quaternion from, Quaternion to, float t)
        {
            QuaternionTween tween;
            if (TryGetTween(quaternionTweens , out tween))
            {
                tween.Init(from, to, t);
            }
            else
            {
                tween = new QuaternionTween(from, to, t, GenerateId());
            }
            _activeTweens.Add(tween);
            return tween;
        }

        public static void RemoveTween(BaseTween tween)
        {
            _activeTweens.Remove(tween);
        }
    }
}
