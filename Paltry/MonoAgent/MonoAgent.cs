using SKCell;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paltry.Extension;
namespace Paltry
{
    /*>>ʹ��ǰ��<<
     *  ��
     *>>ע������<<
     *  1.MonoAgent����ȷ������ڵ���������ʼ��,ʹ�ÿ��ǰȷ��������MonoAgent
     *  2.���̳�Mono��������ڴ˴�����Update(AddUpdate),������һ��ִ�й̶�������Update
     *  3.���ʵ����ӹ���,����Ҫ�ƻ�ԭ�й���*/
    /// <summary>
    /// ���ڿ�������,�ṩȫ�ֹ��õ�Mono���ʵ�
    /// </summary>
    public class MonoAgent : MonoSingleton<MonoAgent>
    {
#if UNITY_EDITOR
        /// <summary>
        /// Ϊ�˽������������ʱ��̬���������õ�����,��ȷ��������������ʼ��
        /// <para>���е����ڳ�ʼ���󶼼����List,���´�����ʱ���������ÿ�,��ִ�г�ʼ��</para>
        /// </summary>
        public static Action _Instances_To_Reset;
#endif
        public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
        public static readonly WaitForSeconds LoadInfoUpdateInternal = new WaitForSeconds(0.1f);
        private List<(int time, Action action)> ConstTimesUpdate = new List<(int count, Action action)> ();
        private event Action updateList;
        private event Action fixedUpdateList;
        protected override void Awake()
        {
#if UNITY_EDITOR
            _Instances_To_Reset?.Invoke();
            _Instances_To_Reset = null;
#endif
            base.Awake();
            DontDestroyOnLoad(gameObject);
            Timer.Init();
        }
        private void Update()
        {
            Do_ConstTimesUpdate();
            updateList?.Invoke();
            InputMgr.Instance.Update();
            Timer.Update();
        }
        private void FixedUpdate()
        {
            fixedUpdateList?.Invoke();
        }
        public void AddUpdate(Action action)
        {
            updateList += action;
        }
        public void RemoveUpdate(Action action)
        {
            updateList -= action;
        }
        public void AddFixedUpdate(Action action)
        {
            fixedUpdateList += action;
        }
        public void RemoveFixedUpdate(Action action)
        {
            fixedUpdateList -= action;
        }
        /// <summary>
        /// ִ�й̶�������Update
        /// </summary>
        /// <param name="time"></param>
        /// <param name="task"></param>
        public void AddConstTimesUpdate(int time,Action task)
        {
            ConstTimesUpdate.Add((time, task));
        }
        private void Do_ConstTimesUpdate()
        {
            for (int i = 0; i < ConstTimesUpdate.Count; i++)
            {
                var task = ConstTimesUpdate[i];
                if (task.time == 0)
                {
                    ConstTimesUpdate.UnorderRemoveAt(i);
                    continue;
                }
                task.action();
                task.time--;
            }
        }
    }

    
}
