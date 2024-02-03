using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Paltry
{
    /*>>使用前提<<
     *  任意游戏物体挂载Paltry.MonoAgent,并在其中调用了Timer.Init()和Timer.Update()
     *>>使用方法<<
     *  1.计时器:用法和System.Diagnostics.Stopwatch基本一致,
     *      可通过IsFinished来判断是否到了规定时间.如果不需要了,用Discard销毁计时器
     *  2.延时调用:Timer.DelayInvoke()可以延迟一段时间再执行一个函数
     *  计时器和延时调用都支持Unscaled版本(按真实时间)
     *  */
    public class Timer
    {
        /// <summary>当前已累计的时间</summary>
        public float Time { get; private set; }
        /// <summary>是否正在计时</summary>
        public bool IsRunning { get; private set; }
        /// <summary>是否已完成计时</summary>
        public bool IsFinished { get; private set; }

        private bool isUnscaled;
        private float maxTime;

        private static List<Timer> timerList;
        private static List<Timer> timerListUnscaled;
        private static List<DelayInvokeTimer> delayInvokeList;
        private static List<DelayInvokeTimer> delayInvokeListUnscaled;
        struct DelayInvokeTimer
        {
            public float time;
            public Action action;
            public string id;
        }
        /// <summary>
        /// 创建计时器,若想立刻开始计时,请调用Timer.StartNew()
        /// </summary>
        /// <param name="maxTime">计时器最大时间</param>
        /// <param name="isUnscaled">是否使用真实时间计时</param>
        public Timer(float maxTime,bool isUnscaled=false)
        {
            this.maxTime = maxTime;
            this.isUnscaled = isUnscaled;
            this.Time = 0;
            this.IsRunning = false;
            this.IsFinished = false;
            if (isUnscaled)
                timerListUnscaled?.Add(this);
            else
                timerList?.Add(this);
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
            if (isUnscaled)
                timerListUnscaled.Remove(this);
            else
                timerList.Remove(this);
        }
        /// <summary>
        /// 创建计时器并立刻开始计时
        /// </summary>
        /// <param name="maxTime">计时器最大时间</param>
        /// <param name="isUnscaled">是否使用真实时间计时</param>
        /// <returns></returns>
        public static Timer StartNew(float maxTime,bool isUnscaled=false)
        {
            return new Timer(maxTime, isUnscaled) { IsRunning = true };
        }
        public static void DelayInvoke(float delayTime, Action action)
        {
            DelayInvoke(delayTime, action, null, false);
        }
        /// <summary>
        /// 延迟执行一个函数
        /// </summary>
        /// <param name="delayTime">延迟时间,应为正数</param>
        /// <param name="action">要执行的函数</param>
        /// <param name="id">函数id,用于取消执行</param>
        public static void DelayInvoke(float delayTime,Action action,string id=null)
        {
            DelayInvoke(delayTime, action, id, false);
        }
        /// <summary>
        /// 延迟执行一个函数
        /// </summary>
        /// <param name="delayTime">延迟时间,应为正数</param>
        /// <param name="action">要执行的函数</param>
        /// <param name="isUnscaled">是否使用真实时间延迟执行</param>
        public static void DelayInvoke(float delayTime,Action action,bool isUnscaled=false)
        {
            DelayInvoke(delayTime, action, null, isUnscaled);
        }
        /// <summary>
        /// 延迟执行一个函数
        /// </summary>
        /// <param name="delayTime">延迟时间</param>
        /// <param name="action">要执行的函数</param>
        /// <param name="id">函数id,用于取消执行</param>
        /// <param name="isUnscaled">是否使用真实时间延迟执行</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void DelayInvoke(float delayTime,Action action,string id,bool isUnscaled)
        {
            if (delayTime <= 0)
            {
                throw new ArgumentOutOfRangeException("delayTime", delayTime, "延迟调用的时间必须为正值");
            }
            if (isUnscaled)
                delayInvokeListUnscaled.Add(new DelayInvokeTimer() { time=delayTime,action=action,id= id } );
            else
                delayInvokeList.Add(new DelayInvokeTimer() { time=delayTime,action=action,id= id} );
        }
        /// <summary>
        /// 取消延迟执行
        /// </summary>
        /// <param name="id">待取消函数的id</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void CancelDelayInvoke(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id", "id不能为空");
            }

            for (int i = 0; i < delayInvokeList.Count; i++)
            {
                if (delayInvokeList[i].id == id)
                {
                    delayInvokeList.RemoveAt(i);
                    return;
                }
            }
            for (int i = 0; i < delayInvokeListUnscaled.Count; i++)
            {
                if (delayInvokeListUnscaled[i].id == id)
                {
                    delayInvokeListUnscaled.RemoveAt(i);
                    return;
                }
            }
        }
        public static void Init()
        {
            timerList = new List<Timer>();
            timerListUnscaled = new List<Timer>();
            delayInvokeList = new List<DelayInvokeTimer>();
            delayInvokeListUnscaled = new List<DelayInvokeTimer>();
        }
        public static void Update()
        {
            float deltaTime = UnityEngine.Time.deltaTime;
            float unscaledDeltaTime = UnityEngine.Time.unscaledDeltaTime;
            UpdateTimerList(timerList, deltaTime);
            UpdateTimerList(timerListUnscaled, unscaledDeltaTime);
            UpdateDelayInvokeList(delayInvokeList, deltaTime);
            UpdateDelayInvokeList(delayInvokeListUnscaled, unscaledDeltaTime);
            void UpdateTimerList(List<Timer> list,float delta)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Timer timer = list[i];
                    if (!timer.IsRunning)
                        continue;
                    timer.Time += delta;
                    if (timer.Time >= timer.maxTime)
                    {
                        timer.IsRunning = false;
                        timer.IsFinished = true;
                    }
                }
            }
            void UpdateDelayInvokeList(List<DelayInvokeTimer> list, float delta)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    DelayInvokeTimer timer = list[i];
                    timer.time -= delta;
                    if (timer.time <= 0)
                    {
                        timer.action();
                        list.RemoveAt(i);
                        continue;
                    }
                    list[i] = timer;//值类型从List索引器获取到的是copy,需要写回
                }
            }
        }

    }

}

