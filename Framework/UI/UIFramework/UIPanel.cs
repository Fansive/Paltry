using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Paltry.Extension;
using System.Collections.Generic;

namespace Paltry
{
    /// <summary>
    /// UI���ĳ������,���UIMgrʹ��
    /// <para>ƴ��UIԤ�����ֱ�Ӽ̳и��༴��,������UI����ȼ�,UI����,���뵭��Ч��</para>
    /// <para>��������:Child��Children��ֱ�Ӹ������ֻ�ȡԤ�����µĿؼ�,������ק</para>
    /// <para>AutoListen���Զ�����UI���µ��¼�,��OnDestroyʱ�Զ��Ƴ�</para>
    /// <para>����Ҫ��дAwake��OnDestroy,��ص���base</para>
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        public abstract UICacheLevel CacheLevel { get; }
        public abstract UIWindowType WindowType { get; }
        //alpha�Ĺ��ɲ���
        protected virtual float TransitDelta => 0.05f;
        protected virtual float FadeInStart => 0.4f;
        protected virtual float FadeInEnd => 1;
        protected virtual float FadeOutStart => 1;
        protected virtual float FadeOutEnd => 0;
        protected virtual bool useAlphaTransit => true;//�Ƿ���UI���뵭��Ч��
        //ģ̬�������ֱ���ͼ��alpha
        protected virtual float ModelWindowAlpha => 0.7f;


        [HideInInspector] public CanvasGroup canvasGroup;//��UIPanelʵ������ʱ��,���CanvasGroup
        private Action fadeOutCallback;//����ȷ��������������ջ,��ӦUIMgr.PopPanel.PopCore
        private List<(string eventName,Action listener)> removeListenerList;

        /// <summary>
        /// ͨ��·����ȡPanel�µ��ӿؼ�,ע��ؼ������ܰ���'/'
        /// </summary>
        protected T Child<T>(string controlPath) where T : Component 
        {
            return transform.FindByPath(controlPath).GetComponent<T>();
        }

        /// <summary>
        /// ��ȡPanel������ָ�����͵��ӿؼ�,�����ҵ�һ���Ӷ���(û���ҵ�����null)
        /// </summary>
        protected T[] Children<T>() where T : Component
        {
            return (transform as RectTransform).Children<T>();
        }
        /// <summary>
        /// ͨ��<see cref="EventCenter"/>����¼�����,����OnDestroyʱ�Զ��Ƴ��¼�����,
        /// �ʺ���Ӹ���UI���¼�����
        /// </summary>
        protected void AutoListen(string eventName,Action listener)
        {
            EventCenter.AddListener(eventName, listener);
            removeListenerList.Add((eventName, listener));
        }
        /// <summary>
        /// �����дAwake,��ص���base.Awake()
        /// </summary>
        protected virtual void Awake()
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            removeListenerList = new List<(string eventName, Action listener)>();
            if (WindowType == UIWindowType.ModelWindow)//ģ̬������ӱ�������
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
        /// ���뵭��:
        /// �������ʱ,����嵭��,ԭ���ֱ�ӱ䰵��ʧ��
        /// �ر����ʱ,��ǰ��嵭��,�����(Ҳ������һ�����)ֱ��������͸����
        /// </summary>
        public virtual void FadeIn()
        {
            canvasGroup.alpha = FadeInStart;
            if (!useAlphaTransit)//����������,��ֱ�Ӱ�alpha���õ�����״̬
                canvasGroup.alpha = FadeInEnd;

            //��ֹͣ��ǰ�Ĺ���(�����)
            MonoAgent.Instance.RemoveFixedUpdate(FadeInCore);
            MonoAgent.Instance.RemoveFixedUpdate(FadeOutCore);

            if(fadeOutCallback != null)//����"�����������ٴδ�"��bug
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
            canvasGroup.blocksRaycasts = false;//�����ڼ���ý���
            if (!useAlphaTransit)
                canvasGroup.alpha = FadeOutEnd;

            //��ֹͣ��ǰ�Ĺ���(�����)
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
        None,//������,�ر�ʱֱ��Destroy
        Open,//��������,�ر�ʱֻ��ʧ��,ֻ������ֻ����һ�������(��ʵ�󲿷ֶ���)
    }
    public enum UIWindowType
    {
        FullScreen,//ȫ������,��ʱʧ����һ�����
        NormalWindow,//��ͨ����,��ʱ��ס��һ�����,������ʧ����
        ModelWindow,//ģ̬����,��ʱ��������һ�����
    }
}
