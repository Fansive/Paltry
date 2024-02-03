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
        [HideInInspector] public CanvasGroup canvasGroup;//��UIPanelʵ������ʱ��,���CanvasGroup

        private Action fadeOutCallback;//����ȷ��������������ջ,��ӦUIMgr.PopPanel.PopCore
        #region Configuration Data
        //����alpha�Ĺ��ɲ���
        private const float TransitDelta = 0.02f;
        private const float FadeInStart = 0.4f;
        private const float FadeInEnd = 1;
        private const float FadeOutStart = 1;
        private const float FadeOutEnd = 0;
        #endregion
        /// <summary>
        /// ���ڻ�ȡPanel�µ��ӿؼ�,��ȥ��ק����
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
        /// ���뵭��:
        /// �������ʱ,����嵭��,ԭ���ֱ�ӱ䰵��ʧ��
        /// �ر����ʱ,��ǰ��嵭��,�����(Ҳ������һ�����)ֱ��������͸����
        /// </summary>
        public virtual void FadeIn()
        {
            canvasGroup.alpha = FadeInStart;

            //��ֹͣ��ǰ�Ĺ���(�����)
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
            canvasGroup.blocksRaycasts = false;//�����ڼ���ý���

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
