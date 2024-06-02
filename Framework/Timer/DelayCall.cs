using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paltry.Collections;
using System;

namespace Paltry
{
    public class DelayCall 
    {
        struct Delay : IComparable<Delay>
        {
            public float moment;
            public Action call;
            public string id;
            public Delay(float moment, Action call, string id)
            {
                this.moment = moment;
                this.call = call;
                this.id = id;
            }
            public int CompareTo(Delay other)
            {
                if (moment < other.moment)
                    return 1;
                else if (moment > other.moment) 
                    return -1;
                else return 0;
            }
        }
        private static PriorityQueue<Delay> queue, realtimeQueue;
        private static float timeline, realtimeline,lastRealtime;
        private static bool isPaused,isRealtimePaused;
        public static void Init()
        {
            queue = new PriorityQueue<Delay>(8);
            realtimeQueue = new PriorityQueue<Delay>();
            MonoAgent.Instance.AddUpdate(Update);
        }
        public static void Call(float delayTime,Action call,string id=null,bool useRealTime=false)
        {
            if (useRealTime)
                realtimeQueue.Enqueue(new Delay(realtimeline+delayTime,call,id));
            else
                queue.Enqueue(new Delay(timeline+delayTime,call,id));
        }
        /// <summary>
        /// ��ͣ/����������ʱ������Ӱ�����ʱ����
        /// </summary>
        public static void Pause(bool isPause)
        {
            isPaused = isPause;
        }
        /// <summary>
        /// ��ͣ/�������а���ʵʱ�����ʱ����
        /// </summary>
        public static void PauseRealtime(bool isPause)
        {
            isRealtimePaused = isPause;
        }
        /// <summary>
        /// ȡ����Ӧid����ʱ����,�����������ͬid,����Ӧȷ��idΨһ
        /// </summary>
        public static void Cancel(string id)
        {
            if (id == null)
                return;
            Predicate<Delay> predicate = x =>{
                if (x.id == id)
                    return true;
                return false;
            };

            if (queue.Count > 0)
            {
                ref Delay delay = ref queue.FindRef(predicate);
                    if(delay.id == id)
                        delay.call = null;
            }
            if(realtimeQueue.Count > 0)
            {
                ref Delay delay = ref realtimeQueue.FindRef(predicate);
                    if(delay.id == id)
                        delay.call = null;
            }
            
        }

        private static void Update()
        {
            timeline += isPaused ? 0 : Time.deltaTime;
            realtimeline += isRealtimePaused? 0: Time.realtimeSinceStartup - lastRealtime;
            lastRealtime = Time.realtimeSinceStartup;

            while(queue.Count >0 && queue.Peek().moment <= timeline)
                queue.Dequeue().call?.Invoke();
            while(realtimeQueue.Count >0 && realtimeQueue.Peek().moment <= realtimeline)
                realtimeQueue.Dequeue().call?.Invoke();
        }

        public static void Clear()
        {
            queue.Clear();
            realtimeQueue.Clear();
        }
 
    }
}

