using SKCell;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Mathf;
namespace Paltry
{
    /*>>ʹ��ǰ��<<
     *  ��
     *>>ʹ�÷���<<
     *  1.ͨ��DoOnceִ��һ�ζ�������,ͨ��DoLoopִ�ж�ζ�������
     *  2.��Unscaled�ķ���ִ��ʱ����ʱ������Ӱ��(����ʵʱ��),ִ�м�������п�������
     *  3.ÿ��ִ�ж���һ��Guid����ֵ,��ͨ����ֵ��ȡ����Ӧ��������
     *  4.���ڶ�������(Curve)�ľ���Ч��,��ǰ��Paltry.Demo.Demo_VisualCurve.cs���п��ӻ�
     *  5.�ֶ��������:����Curveö�������������(����Reverse�汾),����CurveDict��������ߺ���
     *  */
    /// <summary>�ṩ�Զ������߲�����ʵ�ֹ��ɹ��̵ķ���
    /// <para>�˽�Ԥ�趯�����߱���Ч��=>Paltry.Demo.Demo_VisualCurve</para> 
    /// </summary>
    public class Tweening
    {
        public const float RealTimeInterval = 0.02f;
        private static readonly Dictionary<Guid, Coroutine> CoroutineDict = new Dictionary<Guid, Coroutine>();
        /// <summary> �������ߺ������ֵ�,Ҳ�����ڻ�ȡһЩ���ú��� </summary>
        public static readonly Dictionary<Curve, Func<float, float>> CurveDict = new Dictionary<Curve, Func<float, float>>()
        {//TODO �����ṩ�ⲿ����Զ������ߺ����ķ���
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
        /// ��ʼһ�ι��ɹ���,ִ�м����FixedUpdate��ͬ,�������������Ƶ��Ϊ50,��ôactionÿ0.02sִ��һ��
        /// <para>�ù�����ʱ������Ӱ��</para>
        /// </summary>
        /// <param name="time">����ʱ��</param>
        /// <param name="curve">��������,��Reverse�ı�ʾx��1->0����</param>
        /// <param name="action">ÿһִ֡�еĲ���,���Ͳ���float�ǲ���ֵ(������f(x)��ֵ)</param>
        /// <param name="onFinish">���̽�����Ļص�����</param>
        /// <returns>�ù��̵�Guid,����ֹͣ����</returns>
        public static Guid DoOnce(float time,Curve curve,Action<float> action,Action onFinish=null)
        {
            return DoCore(time, 1, -1, true, curve, action, onFinish);
        }
        /// <summary>
        /// ��ʼһ�ι��ɹ���,����ʱ������Ӱ��,ִ�м����Tweening.RealTimeInterval,Ĭ��0.02f
        /// </summary>
        /// <param name="time">����ʱ��</param>
        /// <param name="curve">��������,��Reverse�ı�ʾx��1->0����</param>
        /// <param name="action">ÿһִ֡�еĲ���,���Ͳ���float�ǲ���ֵ(������f(x)��ֵ)</param>
        /// <param name="onFinish">���̽�����Ļص�����</param>
        /// <returns>�ù��̵�Guid,����ֹͣ����</returns>
        public static Guid DoOnceUnscaled(float time,Curve curve,Action<float> action,Action onFinish=null)
        {
            return DoCore(time, 1, -1, false, curve, action, onFinish);
        }
        /// <summary>
        /// ѭ��ִ�ж�ι��ɹ���,ִ�м����FixedUpdate��ͬ,�������������Ƶ��Ϊ50,��ôactionÿ0.02sִ��һ��
        /// <para>�ù�����ʱ������Ӱ��</para>
        /// </summary>
        /// <param name="time">����ѭ���ĳ���ʱ��</param>
        /// <param name="loopCount">ѭ������,��int.MaxValue��ʾ����ѭ��</param>
        /// <param name="loopInterval">����ѭ����ļ��ʱ��</param>
        /// <param name="curve">��������,��Reverse�ı�ʾx��1->0ȡ��</param>
        /// <param name="action">ÿһִ֡�еĲ���,���Ͳ���float�ǲ���ֵ(������f(x)��ֵ)</param>
        /// <param name="onFinish">���̽�����Ļص�����</param>
        /// <returns>�ù��̵�Guid,����ֹͣ����</returns>
        public static Guid DoLoop(float time,int loopCount,float loopInterval, Curve curve,Action<float> action,Action onFinish=null)
        {
            return DoCore(time, loopCount, loopInterval, true, curve, action, onFinish);
        }
        /// <summary>
        /// ѭ��ִ�ж�ι��ɹ���,����ʱ������Ӱ��,ִ�м����Tweening.RealTimeInterval,Ĭ��0.02f
        /// </summary>
        /// <param name="time">����ѭ���ĳ���ʱ��</param>
        /// <param name="loopCount">ѭ������,��int.MaxValue��ʾ����ѭ��</param>
        /// <param name="loopInterval">����ѭ����ļ��ʱ��</param>
        /// <param name="curve">��������,��Reverse�ı�ʾx��1->0ȡ��</param>
        /// <param name="action">ÿһִ֡�еĲ���,���Ͳ���float�ǲ���ֵ(������f(x)��ֵ)</param>
        /// <param name="onFinish">���̽�����Ļص�����</param>
        /// <returns>�ù��̵�Guid,����ֹͣ����</returns>
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
            float step = 1 / sampleCount;//������ʵ������,Ԥ��Ϊ50֡(0.02f)
            bool isReversed = false;//�ȼٶ���δ��ת���
            CurveDict.TryGetValue(curve, out Func<float,float> func);

            if ((int)curve % 2 == 0)//��ż��ö��,��תִ��
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
                    else//TODO ֮��RealTimeInterval�����ĳ������е�һ�����﹩�ⲿ�޸�
                        yield return new WaitForSecondsRealtime(RealTimeInterval);
                }

                if (loopCount == int.MaxValue)//����ѭ��
                    i = 0;
                if (loopCount != 1)//TODO �����Ƶ����new����,֮��д�ö���غ��Ż�һ��
                {    
                    if(isScaled)
                        yield return new WaitForSeconds(loopInterval);
                    else
                        yield return new WaitForSecondsRealtime(loopInterval);
                }

            }

            CoroutineDict.Remove(guid);//��������ֵ������guid
            onFinish?.Invoke();
        }
        /// <summary>
        /// ֹͣ��Ӧid�Ĺ���
        /// </summary>
        /// <param name="guid">����Guid,ͨ��Doϵ�к�����ȡ</param>
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
    /// ������������,��׺ΪReverse�ı�ʾ��ת������
    /// </summary>
    public enum Curve
    {
        Linear = 1,
        LinearReverse = 2,//ż��ö��Ϊ��ת����
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