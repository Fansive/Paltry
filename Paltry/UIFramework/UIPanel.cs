using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Paltry
{
    public abstract class UIPanel : MonoBehaviour
    {
        public abstract UICacheLevel CacheLevel { get; }
        public abstract UIWindowType WindowType{ get; }
        [HideInInspector] public CanvasGroup canvasGroup;//在UIPanel实例化的时候,添加CanvasGroup

        private Action fadeOutCallback;//用来确保淡出后正常出栈,对应UIMgr.PopPanel.PopCore
        #region Configuration Data
        //配置alpha的过渡参数
        private const float TransitDelta = 0.02f;
        private const float FadeInStart = 0.4f;
        private const float FadeInEnd = 1;
        private const float FadeOutStart = 1;
        private const float FadeOutEnd = 0;
        #endregion
        /// <summary>
        /// 用于获取Panel下的子控件,免去拖拽操作
        /// </summary>
        protected T Child<T>(string controlName) where T : UIBehaviour
            => transform.Find(controlName).GetComponent<T>();

        protected virtual void Awake()
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        protected virtual void OnEnable()
        {

        }
        protected virtual void OnDisable()
        {

        }
        protected virtual void OnDestroy()
        {

        }
        /// <summary>
        /// 淡入淡出:
        /// 打开新面板时,新面板淡入,原面板直接变暗或失活
        /// 关闭面板时,当前面板淡出,新面板(也就是上一个面板)直接设置满透明度
        /// </summary>
        public virtual void FadeIn()
        {
            canvasGroup.alpha = FadeInStart;

            //先停止先前的过渡(如果有)
            MonoAgent.Instance.RemoveFixedUpdate(FadeInCore);
            MonoAgent.Instance.RemoveFixedUpdate(FadeOutCore);
            fadeOutCallback?.Invoke();
            fadeOutCallback = null;

            MonoAgent.Instance.AddFixedUpdate(FadeInCore);
            
        }
        void FadeInCore()
        {
            if (canvasGroup.alpha >= FadeInEnd)
            {
                MonoAgent.Instance.RemoveFixedUpdate(FadeInCore);
            }
            canvasGroup.alpha += TransitDelta;
        }
        public virtual void FadeOutAsync(Action callback)
        {
            canvasGroup.alpha = FadeOutStart;
            canvasGroup.blocksRaycasts = false;//淡出期间禁用交互

            //先停止先前的过渡(如果有)
            MonoAgent.Instance.RemoveFixedUpdate(FadeInCore);
            MonoAgent.Instance.RemoveFixedUpdate(FadeOutCore);
            fadeOutCallback?.Invoke();
            fadeOutCallback = null;

            fadeOutCallback = callback;
            MonoAgent.Instance.AddFixedUpdate(FadeOutCore);
        }
        void FadeOutCore()
        {
            if (canvasGroup.alpha <= FadeOutEnd)
            {
                MonoAgent.Instance.RemoveFixedUpdate(FadeOutCore);
                fadeOutCallback.Invoke();
                fadeOutCallback = null;
            }
            canvasGroup.alpha -= TransitDelta;
        }

    }
    public enum UICacheLevel
    {
        None,//不缓存,关闭时直接Destroy
        Open,//开启缓存,关闭时只是失活,只适用于只存在一个的面板(其实大部分都是)
    }
    public enum UIWindowType
    {
        FullScreen,//全屏窗口,打开时失活上一个面板
        NormalWindow,//普通窗口,打开时挡住上一个面板,但不会失活它
        ModelWindow,//模态窗口,打开时仅禁用上一个面板
    }
}
