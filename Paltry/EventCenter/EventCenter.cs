using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Paltry
{
    /*>>ʹ��ǰ��<<
     *  ������Ϸ�������Paltry.MonoAgent
     *>>ʹ�÷���<<
     *  1.ע���¼���:��PaltryConst.cs����Paltry.EventName,�ڸýṹ�������¼����ַ���
     *  2.ͨ��AddListener,RemoveListener��Trigger������,�Ƴ��ʹ����¼�,
     *      ���ǵķ������ؿɴ����¼�����,Ҫȷ�������ʹ���ʱ���¼�����������һ�µ�
     *      �����Ҫ�������,ͨ��ֵԪ�鴫�ݼ���
     *  */
    /// <summary>
    /// �¼�������ַ���ͨ��EventName��ȡ,��ҪӲ����
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
        /// �����¼�,�¼����ַ�������EventName�ﶨ��
        /// </summary>
        public void Trigger(string name)
        {//����¼�û���ҵ�,˵��û���˼�����,��û��Ҫ����
            if(EventDict.TryGetValue(name,out Action eventHandlers))
                eventHandlers?.Invoke();//����¼�Ϊ��,˵�����˼����������Ƴ���,Ҳû��Ҫ����
        }
        /// <summary>
        /// �����¼�,�����ݲ���,��ȷ���������ͺͶ���ʱһ��,�¼����ַ�������EventName�ﶨ��
        /// </summary>
        public void Trigger<TEventArg>(string name,TEventArg eventArg)
        {
            if (EventWithArgsDict.TryGetValue(name, out Delegate delegates))
            {
                Action<TEventArg> eventHandlers = (delegates as Action<TEventArg>)
                    ?? throw new InvalidCastException($"�޷�����<{name}>�¼�,����ʱ�¼���������:<{eventArg.GetType()}>��ȷ�������ʹ���ʱ���¼���������һ��(�����ת��)");
                eventHandlers?.Invoke(eventArg);
            }
        }
        /// <summary>
        /// �����¼�,�¼����ַ�������EventName�ﶨ��
        /// </summary>
        public void AddListener(string name,Action listener)
        {//ע��!��TryGetValue�õ��ķ���ֵ�Ƕ��������,ͨ�����ÿ����޸Ķ���ĳ�Ա
         //���޷��޸Ķ�����,ί�е�+=ʵ�����޸��˶�����,��˴˴�ͨ������ֵ�޸�����Ч��
            if (EventDict.ContainsKey(name))
                //eventHandlers += listener;�޸ĵ��Ƿ���ֵ,��Ч����
                EventDict[name] += listener;
            else
                EventDict[name] = listener;
        }
        /// <summary>
        /// ���Ĵ��������¼�,�¼����ַ�������EventName�ﶨ��
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
        ///ע��,������ʱ��ͨ��������������ʽ,��ôһ��Ҫ��ί����������������������
        ///�ڶ�������֮��ĳ���,Ҫ�ǵ��Ƴ�����,����ǰ��ί�д���ȥ
        ///����Ҳ���������������ڲ�,�ж�һ�����������Ƴ��Լ�(����ͨ��ί��
        /// <summary>
        /// �Ƴ��¼�����,�¼����ַ�������EventName�ﶨ��
        /// </summary>
        public void RemoveListener(string name,Action listener)
        {
            if (EventDict.ContainsKey(name))
                EventDict[name] -= listener;
        }
        /// <summary>
        /// �Ƴ��������¼��ļ���,�¼����ַ�������EventName�ﶨ��
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
        /// ���һ���¼�
        /// </summary>
        public void Clear()
        {
            EventDict.Clear();
            EventWithArgsDict.Clear();
        }

    }

}
