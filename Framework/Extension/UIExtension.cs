using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Platinio.TweenEngine;
namespace Paltry.Extension
{
    public static class UIExtension  
    {
        public static Sequence AddEasySequence(this SelectableTrigger trigger, SequenceActor actor)
        {
            Sequence seq = trigger.gameObject.AddComponent<Sequence>();
            seq.InitEasyComponent();
            actor.Sequences.Add(seq);
            return seq;
        }
        public static Sequence AddEasySequence(this UIBehaviour behaviour, SequenceActor actor)
        {
            Sequence seq = behaviour.gameObject.AddComponent<Sequence>();
            seq.InitEasyComponent();
            actor.Sequences.Add(seq);
            return seq;
        }
        public static Sequence AddSequence(this SelectableTrigger trigger,float startTime, SequenceActor actor)
        {
            Sequence seq = trigger.gameObject.AddComponent<Sequence>();
            seq.InitComponent(startTime);
            actor.Sequences.Add(seq);
            return seq;
        }
        public static Sequence AddSequence(this UIBehaviour behaviour,float startTime,SequenceActor actor)
        {
            Sequence seq = behaviour.gameObject.AddComponent<Sequence>();
            seq.InitComponent(startTime);
            actor.Sequences.Add(seq);
            return seq;
        }
        public static void ApplyTriggerPreset(this UIBehaviour behaviour,TriggerPresets preset)
        {
            behaviour.gameObject.AddComponent<SelectableTrigger>().ApplyPreset(preset);
        }
        public static void AddEvents(this UIBehaviour behaviour,Action onEnable=null,Action onDisable=null,
            Action<PointerEventData> onPointerEnter=null,Action<PointerEventData> onPointerExit=null,
            Action<PointerEventData>onPointerDown=null,Action<PointerEventData>onPointerUp=null,
            Action<AxisEventData> onMove = null, Action<BaseEventData> onSelect = null, Action<BaseEventData> onDeselect = null)
        {
            var trigger = behaviour.gameObject.GetOrAddComponent<SelectableTrigger>();
            trigger.onEnable += onEnable;
            trigger.onDisable += onDisable;
            trigger.OnPointerEnter += onPointerEnter;
            trigger.OnPointerExit += onPointerExit;
            trigger.OnPointerDown += onPointerDown;
            trigger.OnPointerUp += onPointerUp;
            trigger.OnMove += onMove;
            trigger.OnSelect += onSelect;
            trigger.OnDeSelect += onDeselect;
        }
        public static SelectableTrigger AddTrigger(this UIBehaviour behaviour,Action<SelectableTrigger> triggerBuilder=null)
        {
            var trigger = behaviour.gameObject.AddComponent<SelectableTrigger>();
            triggerBuilder?.Invoke(trigger);
            return trigger;
        } 

        /// <summary>
        /// 获取Button下组物体的TMP_Text
        /// </summary>
        public static TMP_Text ChildText(this Button button)
        {
            return button.transform.GetChild(0).GetComponent<TMP_Text>();
        }

        /// <summary>
        /// Identical to GetComponent&lt;RectTransform&gt;(); Don't abuse it.
        /// </summary>
        public static RectTransform RectTransform(this UIBehaviour behaviour)
        {
            return behaviour.GetComponent<RectTransform>();
        }

        /// <summary>
        /// 获取UI父对象下(第一层级)的所有特定类型组件,(没有找到返回null)
        /// </summary>
        public static T[] Children<T>(this RectTransform rect) where T : Component
        {
            T[] buffer = new T[rect.childCount];
            for (int i = 0; i < rect.childCount; i++)
            {
                buffer[i] = rect.GetChild(i).GetComponent<T>();
                if (buffer[i] == null)
                {
                    T[] children = new T[i];
                    Array.Copy(buffer, children, children.Length);
                    return children;
                }
            }
            return null;
        }
        public static T Child<T>(this UIBehaviour behaviour, string controlName) where T : Component
        {
            return behaviour.GetComponent<RectTransform>().FindByPath(controlName).GetComponent<T>();
        }
        public static T Child<T>(this RectTransform rect, string controlPath) where T : Component
        {
            return rect.FindByPath(controlPath).GetComponent<T>();
        }
    }
}

