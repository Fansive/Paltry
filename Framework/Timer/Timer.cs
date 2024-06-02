using System;
using System.Collections.Generic;
using Paltry.Extension;
namespace Paltry
{
    /*>>使用前提<<
     *  任意游戏物体挂载Paltry.MonoAgent,并在其中调用了Timer.Init()和Timer.Update()
     *>>使用方法<<
     *  1.计时器:用法和System.Diagnostics.Stopwatch基本一致,
     *      可通过IsFinished来判断是否到了规定时间.如果不需要了,用Discard销毁计时器
     *      可创建循环计时器,它在计时完成时自动重新开始计时,需绑定OnFinished使用(它的IsFinished不会为true)
     *  */
    public class Timer
    {
        /// <summary>当前已累计的时间</summary>
        public float Time { get; private set; }
        /// <summary>是否正在计时</summary>
        public bool IsRunning { get; private set; }
        /// <summary>是否已完成计时,对循环计时器无效(计时完成后自动重新开始)</summary>
        public bool IsFinished { get; private set; }
        /// <summary>是否为循环计时器</summary>
        public bool IsLooped { get; private set; }
        /// <summary>计时器的时间上限</summary>
        public float MaxTime { get; private set; }
        /// <summary>当前时间占最大时间的比例</summary>
        public float TimeRatio => Time / MaxTime;
        /// <summary>计时完成的事件,可自行增减</summary>
        public Action OnFinished;

        private bool isUnscaled;
        private static List<Timer> timerList;

        /// <summary>
        /// 创建计时器,若想立刻开始计时,请调用Timer.StartNew()
        /// </summary>
        /// <param name="maxTime">计时器最大时间</param>
        /// <param name="isUnscaled">是否使用真实时间计时</param>
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
        /// 创建计时器并立刻开始计时
        /// </summary>
        /// <param name="maxTime">计时器最大时间</param>
        /// <param name="isUnscaled">是否使用真实时间计时</param>
        public static Timer StartNew(float maxTime, bool isLooped = false, Action onFinished = null, bool isUnscaled = false)
        {
            return new Timer(maxTime,isLooped,onFinished, isUnscaled) { IsRunning = true };
        }

        /// <summary>开始计时</summary>
        public void Start()
        {
            IsRunning = true;
        }
        /// <summary>暂停计时</summary>
        public void Stop()
        {
            IsRunning = false;
        }
        /// <summary>重置并暂停计时器</summary>
        public void Reset()
        {
            Time = 0;
            IsRunning = false;
            IsFinished = false;
        }
        /// <summary>重新开始计时</summary>
        public void Restart()
        {
            Time = 0;
            IsRunning = true;
            IsFinished = false;
        }
        /// <summary>销毁计时器</summary>
        public void Discard()
        {
            timerList.UnorderRemove(this);
        }

        /// <summary>修改Timer的参数</summary>
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

