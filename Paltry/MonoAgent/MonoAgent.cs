using SKCell;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paltry.Extension;
namespace Paltry
{
    /*>>使用前提<<
     *  无
     *>>注意事项<<
     *  1.MonoAgent用来确保框架内单例正常初始化,使用框架前确保挂载了MonoAgent
     *  2.不继承Mono的类可以在此处调用Update(AddUpdate),还包括一个执行固定次数的Update
     *  3.可适当增加功能,但不要破坏原有功能*/
    /// <summary>
    /// 挂在空物体上,提供全局公用的Mono访问点
    /// </summary>
    public class MonoAgent : MonoSingleton<MonoAgent>
    {
#if UNITY_EDITOR
        /// <summary>
        /// 为了解决禁用域重载时静态变量不重置的问题,以确保单例能正常初始化
        /// <para>所有单例在初始化后都加入该List,在下次运行时将单例都置空,以执行初始化</para>
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
        /// 执行固定次数的Update
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
