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
    /// �����UI�����,�����̳���<see cref="Selectable"/>������ɼ������¼� 
    /// ����ָ������뿪,����̧��,�ƶ�,ѡ��/ȡ��ѡ�пؼ����¼�
    /// </summary>
    public class SelectableTrigger : UIComponentGetter,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler,
        IPointerUpHandler, ISelectHandler, IDeselectHandler, IMoveHandler
    {
        #region Basic Members Supporting Tweens
        /// <summary>����<see cref="Platinio.TweenEngine.BaseTween.ID"/>��hoverTween��ID
        /// ��ʾ��ǰ���ڽ��е�tween��id�б�
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
        /// ������ʽ:
        /// ��Add��ͷ�Ķ����ɶ�����,��Set��ͷ�Ķ���ֻ������һ��
        /// �»���֮ǰ��Tween��׺��Ϊ��Ӷ���,����Tween��Ϊ����¼�
        /// �»���֮���Ǿ�������

        /// <summary>
        /// ���ý�����뿪�ؼ�����ʱ�Ķ���,���Զ�ʹ�ý�����뿪������ͬʱ����һ��
        /// <para> Funcί��:ǰ���������Ǹÿؼ���������,���ڵ���Tween����
        /// ������ֵ����<see cref="BaseTween"/>��id����</para>
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
        /// ����ָ��ͣ���ڿؼ���ʱ��ѭ������,ע��ö���ֻ������һ��,������ûḲ����ǰЧ��
        /// </summary>
        /// <param name="loopTime">��������</param>
        /// <param name="ease">��������</param>
        /// <param name="tween">��������,����float�Ǵ˿̶��������ϵ�ֵ,��1�仯��0, 1�൱�ڶ�����ʼǰ�ĳ�̬</param>
        public SelectableTrigger SetStayTween(float loopTime,Ease ease,Action<float> tween)
        {
            stayTimer = new Timer(loopTime, isLooped: true);
            staySampler = EasingFunctions.GetEasingFunction(ease);
            onPointerStay = tween;
            return this;
        }
        /// <summary>
        /// Ӧ��Ԥ��,Ԥ����������<see cref="Custom_Presets.GetTriggerPreset(TriggerPresets)"/>�ж���,��Ϊ֮��Ӷ�Ӧö��
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
        /// ������������ڶ�����ʼʱ�ı���,����ԭ��scaleΪ3,������2�Ļ�����scale����6
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
        /// ע��,���¼�����Ӽ���ʱ���Զ�����һ��,��Ϊͨ��AddComponent�õ��Ķ���,����onEnable
        /// <para>�¼���Ӽ���ǰ,�������ں���OnEnable����ִ��,����OnEnableû��ִ��</para>
        /// <para>�����Awake(��Start)�����һ�θ��¼�����,��ֻ���һ��</para>
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
                onPointerStay(1);//����������Ϊ��̬
            }
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) => OnPointerDown?.Invoke(eventData);
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) => OnPointerUp?.Invoke(eventData);
        #endregion
    }
    
}