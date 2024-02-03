using SKCell;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Mathf;
namespace Paltry
{
    /*>>使用前提<<
     *  无
     *>>使用方法<<
     *  1.通过DoOnce执行一次动画过程,通过DoLoop执行多次动画过程
     *  2.带Unscaled的方法执行时不受时间缩放影响(按真实时间),执行间隔在类中可以配置
     *  3.每次执行都有一个Guid返回值,可通过该值来取消对应动画过程
     *  4.关于动画曲线(Curve)的具体效果,可前往Paltry.Demo.Demo_VisualCurve.cs进行可视化
     *  5.手动添加曲线:先在Curve枚举里添加曲线名(包括Reverse版本),再在CurveDict里添加曲线函数
     *  */
    /// <summary>提供对动画曲线采样以实现过渡过程的方法
    /// <para>了解预设动画曲线表现效果=>Paltry.Demo.Demo_VisualCurve</para> 
    /// </summary>
    public class Tweening
    {
        public const float RealTimeInterval = 0.02f;
        private static readonly Dictionary<Guid, Coroutine> CoroutineDict = new Dictionary<Guid, Coroutine>();
        /// <summary> 动画曲线函数的字典,也可用于获取一些常用函数 </summary>
        public static readonly Dictionary<Curve, Func<float, float>> CurveDict = new Dictionary<Curve, Func<float, float>>()
        {//TODO 后续提供外部添加自定义曲线函数的方法
            {Curve.Linear, x => x },
            {Curve.Sin, x => 0.5f * (1.0f - Cos(x * PI * 2)) },
            {Curve.Expo,x => Pow(2.0f, 10.0f * (x - 1.0f)) },
            {Curve.BackEase,x => Pow(x,3) - 0.3f * x * Sin(x*PI) },
            {Curve.SinEase,x => Sin(0.5f * PI * x) },
            {Curve.Elastic,x => Pow(100,x) * Sin(8.5f * PI * x) / 100f },
            {Curve.Bounce,x =>{
            if (x < 0.363636f)
            {
                return  7.5625f * x * x;
            }
            else if (x < 0.72727f)
            {
                x -= 0.545454f;
                return (7.5625f * x * x + 0.75f);
            }
            else if (x < 0.909091f)
            {
                x -= 0.818182f;
                return (7.5625f * x * x + 0.9375f);
            }
            else
            {
                x -= 0.954545f;
                return (7.5625f * x * x + 0.984375f);
            }
                }
            },

        };

        /// <summary>
        /// 开始一次过渡过程,执行间隔和FixedUpdate相同,即假设物理更新频率为50,那么action每0.02s执行一次
        /// <para>该过程受时间缩放影响</para>
        /// </summary>
        /// <param name="time">持续时间</param>
        /// <param name="curve">曲线类型,带Reverse的表示x从1->0采样</param>
        /// <param name="action">每一帧执行的操作,类型参数float是采样值(曲线上f(x)的值)</param>
        /// <param name="onFinish">过程结束后的回调函数</param>
        /// <returns>该过程的Guid,用于停止过程</returns>
        public static Guid DoOnce(float time,Curve curve,Action<float> action,Action onFinish=null)
        {
            return DoCore(time, 1, -1, true, curve, action, onFinish);
        }
        /// <summary>
        /// 开始一次过渡过程,不受时间缩放影响,执行间隔是Tweening.RealTimeInterval,默认0.02f
        /// </summary>
        /// <param name="time">持续时间</param>
        /// <param name="curve">曲线类型,带Reverse的表示x从1->0采样</param>
        /// <param name="action">每一帧执行的操作,类型参数float是采样值(曲线上f(x)的值)</param>
        /// <param name="onFinish">过程结束后的回调函数</param>
        /// <returns>该过程的Guid,用于停止过程</returns>
        public static Guid DoOnceUnscaled(float time,Curve curve,Action<float> action,Action onFinish=null)
        {
            return DoCore(time, 1, -1, false, curve, action, onFinish);
        }
        /// <summary>
        /// 循环执行多次过渡过程,执行间隔和FixedUpdate相同,即假设物理更新频率为50,那么action每0.02s执行一次
        /// <para>该过程受时间缩放影响</para>
        /// </summary>
        /// <param name="time">单次循环的持续时间</param>
        /// <param name="loopCount">循环次数,填int.MaxValue表示无限循环</param>
        /// <param name="loopInterval">两次循环间的间隔时间</param>
        /// <param name="curve">曲线类型,带Reverse的表示x从1->0取样</param>
        /// <param name="action">每一帧执行的操作,类型参数float是采样值(曲线上f(x)的值)</param>
        /// <param name="onFinish">过程结束后的回调函数</param>
        /// <returns>该过程的Guid,用于停止过程</returns>
        public static Guid DoLoop(float time,int loopCount,float loopInterval, Curve curve,Action<float> action,Action onFinish=null)
        {
            return DoCore(time, loopCount, loopInterval, true, curve, action, onFinish);
        }
        /// <summary>
        /// 循环执行多次过渡过程,不受时间缩放影响,执行间隔是Tweening.RealTimeInterval,默认0.02f
        /// </summary>
        /// <param name="time">单次循环的持续时间</param>
        /// <param name="loopCount">循环次数,填int.MaxValue表示无限循环</param>
        /// <param name="loopInterval">两次循环间的间隔时间</param>
        /// <param name="curve">曲线类型,带Reverse的表示x从1->0取样</param>
        /// <param name="action">每一帧执行的操作,类型参数float是采样值(曲线上f(x)的值)</param>
        /// <param name="onFinish">过程结束后的回调函数</param>
        /// <returns>该过程的Guid,用于停止过程</returns>
        public static Guid DoLoopUnscaled(float time,int loopCount,float loopInterval, Curve curve,Action<float> action,Action onFinish=null)
        {
            return DoCore(time, loopCount, loopInterval, false, curve, action, onFinish);
        }
        private static Guid DoCore(float time,int loopCount,float loopInterval,bool isScaled,Curve curve,Action<float> action,Action onFinish)
        {
            Guid guid = Guid.NewGuid();
            Coroutine cr = MonoAgent.Instance.StartCoroutine(DoCRCore(time,loopCount,loopInterval,isScaled, curve, action, onFinish,guid));
            CoroutineDict[guid] = cr;
            return guid;
        }
        protected static IEnumerator DoCRCore(float time,int loopCount,float loopInterval,bool isScaled, Curve curve,Action<float> action,Action onFinish,Guid guid)
        {
            float sampleCount = time / (isScaled ? Time.fixedDeltaTime : RealTimeInterval);
            float step = 1 / sampleCount;//若按真实世界来,预设为50帧(0.02f)
            bool isReversed = false;//先假定是未翻转情况
            CurveDict.TryGetValue(curve, out Func<float,float> func);

            if ((int)curve % 2 == 0)//是偶数枚举,翻转执行
            {
                isReversed = true;
                func = CurveDict[curve-1];
            }

            for (int i = 0; i < loopCount; i++)
            {
                float x = 0;
                for (int j = 0; j < sampleCount; j++)
                {
                    if (isReversed)
                        action(1 - func(1 - x));
                    else
                        action(func(x));
                    x += step;
                    if (isScaled)
                        yield return MonoAgent.WaitForFixedUpdate;
                    else//TODO 之后将RealTimeInterval这样的常数集中到一个类里供外部修改
                        yield return new WaitForSecondsRealtime(RealTimeInterval);
                }

                if (loopCount == int.MaxValue)//无限循环
                    i = 0;
                if (loopCount != 1)//TODO 这里会频繁地new对象,之后写好对象池后优化一下
                {    
                    if(isScaled)
                        yield return new WaitForSeconds(loopInterval);
                    else
                        yield return new WaitForSecondsRealtime(loopInterval);
                }

            }

            CoroutineDict.Remove(guid);//结束后从字典里清除guid
            onFinish?.Invoke();
        }
        /// <summary>
        /// 停止对应id的过程
        /// </summary>
        /// <param name="guid">过程Guid,通过Do系列函数获取</param>
        public static void Cancel(Guid guid)
        {
            if (CoroutineDict.TryGetValue(guid,out Coroutine cr))
            {
                MonoAgent.Instance.StopCoroutine(cr);
                CoroutineDict.Remove(guid);
            }
        }

    }
    /// <summary>
    /// 动画曲线类型,后缀为Reverse的表示翻转的曲线
    /// </summary>
    public enum Curve
    {
        Linear = 1,
        LinearReverse = 2,//偶数枚举为翻转曲线
        Sin,
        SinReverse,
        Expo,
        ExpoReverse,
        BackEase,
        BackEaseReverse,
        SinEase,
        SinEaseReverse,
        Elastic,
        ElasticReverse,
        Bounce,
        BounceReverse,
    }
}