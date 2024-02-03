using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Paltry
{
    /*>>ʹ��ǰ��<<
     *  ������Ϸ�������Paltry.MonoAgent,�������е�����Timer.Init()��Timer.Update()
     *>>ʹ�÷���<<
     *  1.��ʱ��:�÷���System.Diagnostics.Stopwatch����һ��,
     *      ��ͨ��IsFinished���ж��Ƿ��˹涨ʱ��.�������Ҫ��,��Discard���ټ�ʱ��
     *  2.��ʱ����:Timer.DelayInvoke()�����ӳ�һ��ʱ����ִ��һ������
     *  ��ʱ������ʱ���ö�֧��Unscaled�汾(����ʵʱ��)
     *  */
    public class Timer
    {
        /// <summary>��ǰ���ۼƵ�ʱ��</summary>
        public float Time { get; private set; }
        /// <summary>�Ƿ����ڼ�ʱ</summary>
        public bool IsRunning { get; private set; }
        /// <summary>�Ƿ�����ɼ�ʱ</summary>
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
        /// ������ʱ��,�������̿�ʼ��ʱ,�����Timer.StartNew()
        /// </summary>
        /// <param name="maxTime">��ʱ�����ʱ��</param>
        /// <param name="isUnscaled">�Ƿ�ʹ����ʵʱ���ʱ</param>
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
            if (isUnscaled)
                timerListUnscaled.Remove(this);
            else
                timerList.Remove(this);
        }
        /// <summary>
        /// ������ʱ�������̿�ʼ��ʱ
        /// </summary>
        /// <param name="maxTime">��ʱ�����ʱ��</param>
        /// <param name="isUnscaled">�Ƿ�ʹ����ʵʱ���ʱ</param>
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
        /// �ӳ�ִ��һ������
        /// </summary>
        /// <param name="delayTime">�ӳ�ʱ��,ӦΪ����</param>
        /// <param name="action">Ҫִ�еĺ���</param>
        /// <param name="id">����id,����ȡ��ִ��</param>
        public static void DelayInvoke(float delayTime,Action action,string id=null)
        {
            DelayInvoke(delayTime, action, id, false);
        }
        /// <summary>
        /// �ӳ�ִ��һ������
        /// </summary>
        /// <param name="delayTime">�ӳ�ʱ��,ӦΪ����</param>
        /// <param name="action">Ҫִ�еĺ���</param>
        /// <param name="isUnscaled">�Ƿ�ʹ����ʵʱ���ӳ�ִ��</param>
        public static void DelayInvoke(float delayTime,Action action,bool isUnscaled=false)
        {
            DelayInvoke(delayTime, action, null, isUnscaled);
        }
        /// <summary>
        /// �ӳ�ִ��һ������
        /// </summary>
        /// <param name="delayTime">�ӳ�ʱ��</param>
        /// <param name="action">Ҫִ�еĺ���</param>
        /// <param name="id">����id,����ȡ��ִ��</param>
        /// <param name="isUnscaled">�Ƿ�ʹ����ʵʱ���ӳ�ִ��</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void DelayInvoke(float delayTime,Action action,string id,bool isUnscaled)
        {
            if (delayTime <= 0)
            {
                throw new ArgumentOutOfRangeException("delayTime", delayTime, "�ӳٵ��õ�ʱ�����Ϊ��ֵ");
            }
            if (isUnscaled)
                delayInvokeListUnscaled.Add(new DelayInvokeTimer() { time=delayTime,action=action,id= id } );
            else
                delayInvokeList.Add(new DelayInvokeTimer() { time=delayTime,action=action,id= id} );
        }
        /// <summary>
        /// ȡ���ӳ�ִ��
        /// </summary>
        /// <param name="id">��ȡ��������id</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void CancelDelayInvoke(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id", "id����Ϊ��");
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
                    list[i] = timer;//ֵ���ʹ�List��������ȡ������copy,��Ҫд��
                }
            }
        }

    }

}

