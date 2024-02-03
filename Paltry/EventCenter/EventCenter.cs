using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Paltry
{
    /*>>使用前提<<
     *  任意游戏物体挂载Paltry.MonoAgent
     *>>使用方法<<
     *  1.注册事件名:在PaltryConst.cs里有Paltry.EventName,在该结构中声明事件名字符串
     *  2.通过AddListener,RemoveListener和Trigger来监听,移除和触发事件,
     *      它们的泛型重载可传入事件参数,要确保监听和触发时的事件参数类型是一致的
     *      如果需要多个参数,通过值元组传递即可
     *  */
    /// <summary>
    /// 事件传入的字符串通过EventName获取,不要硬编码
    /// </summary>
    public class EventCenter:CSharpSingleton<EventCenter>
    {
        private Dictionary<string,Action> EventDict;
        private Dictionary<string, Delegate> EventWithArgsDict;
        public EventCenter()
        {
            EventDict = new Dictionary<string, Action>();
            EventWithArgsDict = new Dictionary<string, Delegate>();
        }
        /// <summary>
        /// 触发事件,事件名字符串请在EventName里定义
        /// </summary>
        public void Trigger(string name)
        {//如果事件没有找到,说明没有人监听过,就没必要触发
            if(EventDict.TryGetValue(name,out Action eventHandlers))
                eventHandlers?.Invoke();//如果事件为空,说明有人监听过但又移除了,也没必要触发
        }
        /// <summary>
        /// 触发事件,并传递参数,请确保参数类型和订阅时一致,事件名字符串请在EventName里定义
        /// </summary>
        public void Trigger<TEventArg>(string name,TEventArg eventArg)
        {
            if (EventWithArgsDict.TryGetValue(name, out Delegate delegates))
            {
                Action<TEventArg> eventHandlers = (delegates as Action<TEventArg>)
                    ?? throw new InvalidCastException($"无法触发<{name}>事件,触发时事件参数类型:<{eventArg.GetType()}>请确保监听和触发时的事件参数类型一致(或可以转换)");
                eventHandlers?.Invoke(eventArg);
            }
        }
        /// <summary>
        /// 订阅事件,事件名字符串请在EventName里定义
        /// </summary>
        public void AddListener(string name,Action listener)
        {//注意!用TryGetValue得到的返回值是对象的引用,通过引用可以修改对象的成员
         //但无法修改对象本身,委托的+=实际上修改了对象本身,因此此处通过返回值修改是无效的
            if (EventDict.ContainsKey(name))
                //eventHandlers += listener;修改的是返回值,无效操作
                EventDict[name] += listener;
            else
                EventDict[name] = listener;
        }
        /// <summary>
        /// 订阅带参数的事件,事件名字符串请在EventName里定义
        /// </summary>
        /// <typeparam name="TEventArg"></typeparam>
        /// <param name="name"></param>
        /// <param name="listener"></param>
        public void AddListener<TEventArg>(string name,Action<TEventArg> listener)
        {
            if (EventWithArgsDict.TryGetValue(name, out Delegate delegates))
            {
                Action<TEventArg> eventHandlers = delegates as Action<TEventArg>;
                eventHandlers += listener;
                EventWithArgsDict[name] = eventHandlers;
            }
            else
                EventWithArgsDict[name] = listener;
        }
        ///注意,如果添加时是通过匿名函数的形式,那么一定要用委托来保存匿名函数的引用
        ///在对象销毁之类的场合,要记得移除监听,把先前的委托传进去
        ///或者也可以在匿名函数内部,判断一定的条件来移除自己(仍需通过委托
        /// <summary>
        /// 移除事件监听,事件名字符串请在EventName里定义
        /// </summary>
        public void RemoveListener(string name,Action listener)
        {
            if (EventDict.ContainsKey(name))
                EventDict[name] -= listener;
        }
        /// <summary>
        /// 移除带参数事件的监听,事件名字符串请在EventName里定义
        /// </summary>
        public void RemoveListener<TEventArg>(string name,Action<TEventArg> listener)
        {
            if (EventWithArgsDict.TryGetValue(name, out Delegate delegates))
            {
                Action<TEventArg> eventHandlers = delegates as Action<TEventArg>;
                eventHandlers -= listener;
                EventWithArgsDict[name] = eventHandlers;
            }
        }
        /// <summary>
        /// 清空一切事件
        /// </summary>
        public void Clear()
        {
            EventDict.Clear();
            EventWithArgsDict.Clear();
        }

    }

}
