using System;
using System.Collections.Generic;
using Paltry.Extension;
namespace Paltry
{
    /*>>ʹ��ǰ��<<
     *  ������Ϸ�������Paltry.MonoAgent,�������е�����Timer.Init()��Timer.Update()
     *>>ʹ�÷���<<
     *  1.��ʱ��:�÷���System.Diagnostics.Stopwatch����һ��,
     *      ��ͨ��IsFinished���ж��Ƿ��˹涨ʱ��.�������Ҫ��,��Discard���ټ�ʱ��
     *      �ɴ���ѭ����ʱ��,���ڼ�ʱ���ʱ�Զ����¿�ʼ��ʱ,���OnFinishedʹ��(����IsFinished����Ϊtrue)
     *  */
    public class Timer
    {
        /// <summary>��ǰ���ۼƵ�ʱ��</summary>
        public float Time { get; private set; }
        /// <summary>�Ƿ����ڼ�ʱ</summary>
        public bool IsRunning { get; private set; }
        /// <summary>�Ƿ�����ɼ�ʱ,��ѭ����ʱ����Ч(��ʱ��ɺ��Զ����¿�ʼ)</summary>
        public bool IsFinished { get; private set; }
        /// <summary>�Ƿ�Ϊѭ����ʱ��</summary>
        public bool IsLooped { get; private set; }
        /// <summary>��ʱ����ʱ������</summary>
        public float MaxTime { get; private set; }
        /// <summary>��ǰʱ��ռ���ʱ��ı���</summary>
        public float TimeRatio => Time / MaxTime;
        /// <summary>��ʱ��ɵ��¼�,����������</summary>
        public Action OnFinished;

        private bool isUnscaled;
        private static List<Timer> timerList;

        /// <summary>
        /// ������ʱ��,�������̿�ʼ��ʱ,�����Timer.StartNew()
        /// </summary>
        /// <param name="maxTime">��ʱ�����ʱ��</param>
        /// <param name="isUnscaled">�Ƿ�ʹ����ʵʱ���ʱ</param>
        public Timer(float maxTime,bool isLooped =false,Action onFinished =null,bool isUnscaled=false)
        {
            this.MaxTime = maxTime;
            this.Time = 0;
            this.IsRunning = false;
            this.IsFinished = false;
            this.IsLooped = isLooped;
            this.OnFinished = onFinished;
            this.isUnscaled = isUnscaled;
            timerList.Add(this);
        }
        /// <summary>
        /// ������ʱ�������̿�ʼ��ʱ
        /// </summary>
        /// <param name="maxTime">��ʱ�����ʱ��</param>
        /// <param name="isUnscaled">�Ƿ�ʹ����ʵʱ���ʱ</param>
        public static Timer StartNew(float maxTime, bool isLooped = false, Action onFinished = null, bool isUnscaled = false)
        {
            return new Timer(maxTime,isLooped,onFinished, isUnscaled) { IsRunning = true };
        }

        /// <summary>��ʼ��ʱ</summary>
        public void Start()
        {
            IsRunning = true;
        }
        /// <summary>��ͣ��ʱ</summary>
        public void Stop()
        {
            IsRunning = false;
        }
        /// <summary>���ò���ͣ��ʱ��</summary>
        public void Reset()
        {
            Time = 0;
            IsRunning = false;
            IsFinished = false;
        }
        /// <summary>���¿�ʼ��ʱ</summary>
        public void Restart()
        {
            Time = 0;
            IsRunning = true;
            IsFinished = false;
        }
        /// <summary>���ټ�ʱ��</summary>
        public void Discard()
        {
            timerList.UnorderRemove(this);
        }

        /// <summary>�޸�Timer�Ĳ���</summary>
        public void Modify(float maxTime,bool isLooped)
        {
            this.MaxTime = maxTime;
            this.IsLooped = isLooped;
        }
        public static void Init()
        {
            timerList = new List<Timer>();
        }
        public static void Update()
        {
            float deltaTime = UnityEngine.Time.deltaTime;
            float unscaledDeltaTime = UnityEngine.Time.unscaledDeltaTime;

            for (int i = 0; i < timerList.Count; i++)
            {
                Timer timer = timerList[i];
                if (!timer.IsRunning)
                    continue;

                timer.Time += timer.isUnscaled ? unscaledDeltaTime : deltaTime;
                if (timer.Time >= timer.MaxTime)
                {
                    timer.OnFinished?.Invoke();
                    timer.IsRunning = false;
                    timer.IsFinished = true;  
                    if(timer.IsLooped)
                        timer.Restart();
                }  
            }
        }

    }
}

