using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using Platinio.TweenEngine;
using TMPro;

namespace Paltry
{
    /// <summary>
    /// 添加在UI组件上,监听继承自<see cref="Selectable"/>的组件可监听的事件 
    /// 包括指针进入离开,按下抬起,移动,选中/取消选中控件的事件
    /// </summary>
    public class SelectableTrigger : UIComponentGetter,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler, ISelectHandler, IDeselectHandler, IMoveHandler
    {
        #region Basic Members Supporting Tweens
        /// <summary>用于<see cref="Platinio.TweenEngine.BaseTween.ID"/>的hoverTween的ID
        /// 表示当前正在进行的tween的id列表
        /// </summary>
        public List<int> enterTweenIDs = new List<int>(0);
        
        private Timer stayTimer;
        private EasingFunctions.Function staySampler;
        private Action<float> onPointerStay;
        void Update()
        { 
            if (stayTimer?.IsRunning ?? false)
                    onPointerStay(staySampler(1,0,MathUtil.Map01_010(stayTimer.TimeRatio)));
        }
        #endregion

        #region Interface for Tween Making
        /// 命名格式:
        /// 以Add开头的动画可多次添加,以Set开头的动画只能设置一次
        /// 下划线之前带Tween后缀的为添加动画,不带Tween的为添加事件
        /// 下划线之后是具体内容

        /// <summary>
        /// 设置进入和离开控件区域时的动画,会自动使得进入和离开动画仅同时播放一个
        /// <para> Func委托:前两个参数是该控件自身的组件,用于调用Tween动画
        /// 而返回值返是<see cref="BaseTween"/>的id数组</para>
        /// </summary>
        public SelectableTrigger AddEnterTween(Func<RectTransform, Image, int[]> enterTween,Func<RectTransform, Image, int[]> exitTween)
        {
            OnPointerEnter += e =>
            {
                foreach (int id in enterTweenIDs)
                    PlatinioTween.instance.CancelTween(id);
                enterTweenIDs.AddRange(enterTween(Rect, Image));
            };
            OnPointerExit += e =>
            {
                foreach (int id in enterTweenIDs)
                    PlatinioTween.instance.CancelTween(id);
                enterTweenIDs.AddRange(exitTween(Rect, Image));
            };
            return this;
        }
        /// <summary>
        /// 设置指针停留在控件内时的循环动画,注意该动画只能设置一次,多次设置会覆盖先前效果
        /// </summary>
        /// <param name="loopTime">动画周期</param>
        /// <param name="ease">动画曲线</param>
        /// <param name="tween">动画内容,参数float是此刻动画曲线上的值,从1变化到0, 1相当于动画开始前的初态</param>
        public SelectableTrigger SetStayTween(float loopTime,Ease ease,Action<float> tween)
        {
            stayTimer = new Timer(loopTime, isLooped: true);
            staySampler = EasingFunctions.GetEasingFunction(ease);
            onPointerStay = tween;
            return this;
        }
        /// <summary>
        /// 应用预设,预设需自行在<see cref="Custom_Presets.GetTriggerPreset(TriggerPresets)"/>中定义,并为之添加对应枚举
        /// </summary>
        /// <param name="preset"></param>
        public SelectableTrigger ApplyPreset(TriggerPresets preset)
        {
            Custom_Presets.GetTriggerPreset(preset).Invoke(this);
            return this;
        }

        public SelectableTrigger AddEnableTween_Move(Vector2 absolutePosition,float time,Ease ease)
        {
            onEnable += () => Rect.MoveUI_Modified(absolutePosition, UIMgr.Instance.Canvas, time).SetEase(ease);
            return this;
        }
        public SelectableTrigger AddEnableTween_MoveInWorld(Vector2 destination, float time, Ease ease)
        {
            onEnable += () => Rect.Move(destination, time).SetEase(ease);
            return this;
        }
        public SelectableTrigger AddEnter_Sound(string UISoundName)
        {
            OnPointerEnter += e => ServiceLocator.IAudioMgr.PlaySound(UISoundName);
            return this;
        }
        public SelectableTrigger AddDown_Sound(string UISoundName)
        {
            OnPointerDown += e => ServiceLocator.IAudioMgr.PlaySound(UISoundName);
            return this;
        }
        public SelectableTrigger AddUp_Sound(string UISoundName)
        {
            OnPointerUp += e => ServiceLocator.IAudioMgr.PlaySound(UISoundName);
            return this;
        }
        /// <summary>
        /// 该缩放是相对于动画开始时的倍数,例如原先scale为3,这里填2的话最终scale就是6
        /// </summary>
        public SelectableTrigger AddEnterTween_Scale(float scale,float time,Ease ease)
        {
            Vector3 originScale = Rect.localScale;
            AddEnterTween(
                (rect, image) => new int[] { rect.ScaleTween(scale * originScale, time).SetEase(ease).ID },
                (rect, image) => new int[] { rect.ScaleTween(originScale, time).SetEase(ease).ID });
            return this;
        }
        public SelectableTrigger AddEnterTween_TextColor(Color color,float time,Ease ease)
        {
            Color originColor = TMPText.color;
            AddEnterTween(
                (rect, image) => new int[] { TMPText.ColorTween(color, time).SetEase(ease).ID },
                (rect, image) => new int[] { TMPText.ColorTween(originColor, time).SetEase(ease).ID });
            return this;
        }
        public SelectableTrigger AddEnterTween_ExternTextColor(TMP_Text text,Color color,float time,Ease ease)
        {
            Color originColor = text.color;
            AddEnterTween(
                (rect, image) => new int[] { text.ColorTween(color, time).SetEase(ease).ID },
                (rect, image) => new int[] { text.ColorTween(originColor, time).SetEase(ease).ID });
            return this;
        }

        public SelectableTrigger SetStayTween_ImageBlink(float loopTime,Ease ease,float minAlpha=0,float maxAlpha = 1)
        {
            SetStayTween(loopTime, ease, value =>
            {
                value = Mathf.Lerp(minAlpha, maxAlpha, value);
                Image.color += new Color(0,0,0, value-Image.color.a);
                //Image.color = Image.color with { a = value}; I Really need this for C#10.0
            });
            return this;
        } 
        public SelectableTrigger SetStayTween_TextBlink(float loopTime,Ease ease,float minAlpha=0,float maxAlpha = 1)
        {
            SetStayTween(loopTime, ease, value =>
            {
                value = Mathf.Lerp(minAlpha, maxAlpha, value);
                TMPText.color += new Color(0,0,0, value- TMPText.color.a);
            });
            return this;
        }
        public SelectableTrigger SetStayTween_ExternTextBlink(TMP_Text text,float loopTime,Ease ease,float minAlpha=0,float maxAlpha = 1)
        {
            SetStayTween(loopTime, ease, value =>
            {
                value = Mathf.Lerp(minAlpha, maxAlpha, value);
                text.color += new Color(0,0,0, value- text.color.a);
            });
            return this;
        }
        #endregion

        #region Events
        public event Action<PointerEventData> OnPointerEnter;
        public event Action<PointerEventData> OnPointerExit;
        public event Action<PointerEventData> OnPointerDown;
        public event Action<PointerEventData> OnPointerUp;
        public event Action<AxisEventData> OnMove;
        public event Action<BaseEventData> OnSelect;
        public event Action<BaseEventData> OnDeSelect;
        private Action _onEnable;
        /// <summary>
        /// 注意,该事件在添加监听时会自动调用一次,因为通过AddComponent得到的对象,在其onEnable
        /// <para>事件添加监听前,生命周期函数OnEnable就已执行,导致OnEnable没有执行</para>
        /// <para>请仅在Awake(或Start)中添加一次该事件监听,且只添加一次</para>
        /// </summary>
        public event Action onEnable
        {
            add
            {
                _onEnable += value;
                if (isActiveAndEnabled)
                    _onEnable?.Invoke();
            }
            remove => _onEnable -= value;
        }
        public event Action onDisable;
        void OnEnable() => _onEnable?.Invoke();
        void OnDisable() => onDisable?.Invoke();
        void ISelectHandler.OnSelect(BaseEventData eventData) => OnSelect?.Invoke(eventData);
        void IDeselectHandler.OnDeselect(BaseEventData eventData) => OnDeSelect?.Invoke(eventData);
        void IMoveHandler.OnMove(AxisEventData eventData) => OnMove?.Invoke(eventData);
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnter?.Invoke(eventData);
            stayTimer?.Restart();

        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            OnPointerExit?.Invoke(eventData);
            if (stayTimer != null)
            {
                stayTimer.Stop();
                onPointerStay(1);//将动画重置为初态
            }
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) => OnPointerDown?.Invoke(eventData);
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) => OnPointerUp?.Invoke(eventData);
        #endregion
    }
    
}