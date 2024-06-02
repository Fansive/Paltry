using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Paltry.Extension;
using System.Collections.Generic;

namespace Paltry
{
    /// <summary>
    /// UI面板的抽象基类,配合UIMgr使用
    /// <para>拼好UI预制体后直接继承该类即可,可设置UI缓存等级,UI类型,淡入淡出效果</para>
    /// <para>便利方法:Child和Children可直接根据名字获取预制体下的控件,无需拖拽</para>
    /// <para>AutoListen可自动监听UI更新的事件,在OnDestroy时自动移除</para>
    /// <para>若需要覆写Awake和OnDestroy,务必调用base</para>
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        public abstract UICacheLevel CacheLevel { get; }
        public abstract UIWindowType WindowType { get; }
        //alpha的过渡参数
        protected virtual float TransitDelta => 0.05f;
        protected virtual float FadeInStart => 0.4f;
        protected virtual float FadeInEnd => 1;
        protected virtual float FadeOutStart => 1;
        protected virtual float FadeOutEnd => 0;
        protected virtual bool useAlphaTransit => true;//是否开启UI淡入淡出效果
        //模态窗口遮罩背景图的alpha
        protected virtual float ModelWindowAlpha => 0.7f;


        [HideInInspector] public CanvasGroup canvasGroup;//在UIPanel实例化的时候,添加CanvasGroup
        private Action fadeOutCallback;//用来确保淡出后正常出栈,对应UIMgr.PopPanel.PopCore
        private List<(string eventName,Action listener)> removeListenerList;

        /// <summary>
        /// 通过路径获取Panel下的子控件,注意控件名不能包含'/'
        /// </summary>
        protected T Child<T>(string controlPath) where T : Component 
        {
            return transform.FindByPath(controlPath).GetComponent<T>();
        }

        /// <summary>
        /// 获取Panel下所有指定类型的子控件,仅查找第一层子对象(没有找到返回null)
        /// </summary>
        protected T[] Children<T>() where T : Component
        {
            return (transform as RectTransform).Children<T>();
        }
        /// <summary>
        /// 通过<see cref="EventCenter"/>添加事件监听,并在OnDestroy时自动移除事件监听,
        /// 适合添加更新UI的事件监听
        /// </summary>
        protected void AutoListen(string eventName,Action listener)
        {
            EventCenter.AddListener(eventName, listener);
            removeListenerList.Add((eventName, listener));
        }
        /// <summary>
        /// 如果覆写Awake,务必调用base.Awake()
        /// </summary>
        protected virtual void Awake()
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            removeListenerList = new List<(string eventName, Action listener)>();
            if (WindowType == UIWindowType.ModelWindow)//模态窗口添加背景遮罩
            {
                Image backGroundMask = gameObject.AddComponent<Image>();
                backGroundMask.color = new Color(0, 0, 0, ModelWindowAlpha);
            }
        }
        protected virtual void OnDestroy()
        {
            foreach(var entry in removeListenerList)
                EventCenter.RemoveListener(entry.eventName, entry.listener);
        }


        /// <summary>
        /// 淡入淡出:
        /// 打开新面板时,新面板淡入,原面板直接变暗或失活
        /// 关闭面板时,当前面板淡出,新面板(也就是上一个面板)直接设置满透明度
        /// </summary>
        public virtual void FadeIn()
        {
            canvasGroup.alpha = FadeInStart;
            if (!useAlphaTransit)//不开启过渡,则直接把alpha设置到结束状态
                canvasGroup.alpha = FadeInEnd;

            //先停止先前的过渡(如果有)
            MonoAgent.Instance.RemoveFixedUpdate(FadeInCore);
            MonoAgent.Instance.RemoveFixedUpdate(FadeOutCore);

            if(fadeOutCallback != null)//处理"淡出过程中再次打开"的bug
            {
                fadeOutCallback.Invoke();
                gameObject.SetActive(true);
            }

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
            if (!useAlphaTransit)
                canvasGroup.alpha = FadeOutEnd;

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
