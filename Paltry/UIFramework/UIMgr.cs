using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Paltry
{
    /*>>ʹ��ǰ��<<
     *  1.������Ϸ�������Paltry.MonoAgent
     *  2.������ABMgr
     *  3.��UI��AB����Ҫ����ΪCanvas��EventSystem����Ϸ����
     *      (UIMgr���ṩ�������ǵķ���,Ҫ����Ϸ�������Լ����ú�)
     *>>ʹ�÷���<<
     *  1.���÷�UI��AB������,UI_ABName
     *  2.(��ѡ)��UIPanel����,���ù��ɲ���������UI���뵭��Ч��
     *UI�����������:  
     *  1.��PaltryConst.UIName��������Ҫ�õ���Panel��
     *  2.ƴ���,֮��������Ϲ���xxPanel(���ֺ�1.�������һ��),���̳�UIPanel
     *  3.��д�����Ա,�������Ļ�����Ժʹ�������
     *  4.��д�������ں���,�������߼�,��/�ر������UIMgr��OpenPanel��PopPanel
     *  */ 
    public class UIMgr : CSharpSingleton<UIMgr>
    {
        //����UI��ʵ��˳��,�������ֶ�����UIʱ�ָܻ�UI
        private Stack<string> UINavStack;
        //����������UIʵ��
        private Stack<UIPanel> UIEntityStack;

        //������ع�����Դ,�ڵ�һ�δ�AB��������ɺ��������
        private Dictionary<string, GameObject> PanelResCache;
        //������Ҫ��������,��PanelResCache����س���ʱ��������
        private Dictionary<string, UIPanel> PanelCache;

        private RectTransform uiRoot;
        private EventSystem eventSystem;
        #region Configuration Data
        private const string UI_ABName = "ui";//��UI��AB������
        #endregion
        public UIMgr()
        {
            UINavStack = new Stack<string>();
            UIEntityStack = new Stack<UIPanel>();
            PanelResCache = new Dictionary<string, GameObject>();
            PanelCache = new Dictionary<string, UIPanel>();

            //����Canvas��EventSystem�Լ�UIRoot,UIRoot�������е����(�������������Root)
            var eventSysObj = GameObject.Instantiate(ABMgr.Instance.LoadRes<GameObject>(UI_ABName, "EventSystem"));
            eventSysObj.name = "EventSystem";
            eventSystem = eventSysObj.GetComponent<EventSystem>();

            var canvas = ABMgr.Instance.LoadRes<GameObject>(UI_ABName, "Canvas");
            canvas = GameObject.Instantiate(canvas);
            canvas.name = "Canvas";

            uiRoot = new GameObject("UIRoot").AddComponent<RectTransform>();
            uiRoot.SetParent(canvas.transform);
            SetRectTransform(uiRoot);
        }
        public void OpenPanel(string panelName)
        {
            UIPanel newPanel = null;

            //�ȳ��Դ���建������,�ҵ���ֱ�Ӽ���������
            if(PanelCache.TryGetValue(panelName, out newPanel))
            {
                ActivatePanel(newPanel);
            }
            //����建��û��,����Դ��������,��Ҫ�´���һ��
            else if (PanelResCache.TryGetValue(panelName,out var panelObj))
            {
                newPanel = CreatePanel(panelName,panelObj);
                ActivatePanel(newPanel);
            }
            //��û��,��AB�������
            else
            {
                ABMgr.Instance.LoadResAsync<GameObject>(UI_ABName, panelName, (panelObj) =>
                {
                    newPanel = CreatePanel(panelName, panelObj);
                    ActivatePanel(newPanel);
                    PanelResCache.Add(panelName, panelObj);
                    Callback_SetPanel();
                });
                return;
            }


            Callback_SetPanel();
            void Callback_SetPanel()//�����Ҫ�첽����,��ôҪ�Ѹûص�����ȥ
            {
                //���ջ��������ִ�в���
                if (UIEntityStack.TryPeek(out var topPanel))
                {   
                    if (newPanel.WindowType == UIWindowType.ModelWindow)
                    {//ģ̬����,���ý�������(��������ӱ䰵��)
                        SetModelWindowEffect(topPanel, true);
                    }
                    else if(newPanel.WindowType == UIWindowType.FullScreen)
                    {//ȫ������,��Ҫʧ��
                        topPanel.gameObject.SetActive(false);
                    }
                    else//��ͨ����,��ִ���κβ���
                    {

                    }
                }
                UINavStack.Push(panelName);
                UIEntityStack.Push(newPanel);

                //���,ִ�������ĵ���
                newPanel.FadeIn();
            }

        }
        public void PopPanel()
        {
            UIPanel topPanel = UIEntityStack.Pop();
            string topPanelName = UINavStack.Pop();
            

            //�Ƚ���ʱջ��Panel����
            if(UIEntityStack.TryPeek(out UIPanel curPanel))
            {
                if(topPanel.WindowType == UIWindowType.ModelWindow)
                {//��ǰ��ģ̬����,��ʱ�����
                    SetModelWindowEffect(curPanel, false);
                }
                else if(topPanel.WindowType == UIWindowType.FullScreen)
                {
                    curPanel.gameObject.SetActive(true);
                }
                else//��ͨ����,��ִ���κβ���
                {

                }
            }
            else//UIʵ��ջΪ��,����UINavStack�ָ�UI
            {   //���ڴ�ʱͨ��DestroyPanel,������建����϶�û��
                RestorePanel();
            }


            topPanel.FadeOutAsync(PopCore);
            void PopCore()
            {
                //����ٽ���Pop���ʧ��/����
                if (topPanel.CacheLevel == UICacheLevel.Open)
                {   //Ҫ����Ļ�,����ʧ���
                    PanelCache[topPanelName].gameObject.SetActive(false);
                }
                else
                {
                    GameObject.Destroy(topPanel.gameObject);
                }
                //����������,�ָ�����
                topPanel.canvasGroup.blocksRaycasts = true;
            }
            
        }
        /// <summary>
        /// ����������ǰջ�ϵ�UIPanel��������(�����仺��)
        /// <para>ʹ�ø÷���ʱ,ȷ��Ҫ���ٵ�UIPanelȷʵ����</para>
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="alsoDestroyRes">�Ƿ�ͬʱ��������Դ�ļ�(�´������´�AB�������)</param>
        public void DestroyPanel(bool alsoDestroyRes,params string[] panelNames)
        {
            var queue = new Queue<UIPanel>();
            for (int i = 0,j=0; i < UINavStack.Count && j < panelNames.Length; i++)
            {
                UIPanel panel = UIEntityStack.Pop();
                if (panel.name == panelNames[j])
                {
                    PanelCache.Remove(panel.name);
                    GameObject.Destroy(panel.gameObject);
                    if (alsoDestroyRes)
                        PanelResCache.Remove(panel.name);
                    j++;
                }
                else
                    queue.Enqueue(panel);
            }
            while (queue.Count != 0)
            {
                UIEntityStack.Push(queue.Dequeue());
            }
        }
        /// <summary>
        /// ����EventSystem,����ȫ�ֵ�UI����
        /// </summary>
        public void SetInteraction(bool isActive)
        {
            eventSystem.enabled = isActive;
        }

        #region Private Methods
        //ע��,�ָ�Panel�޷�����ģ̬����(����Ҳ��Ӧ�������ָ�,��Ϊ��û�б�����)
        private void RestorePanel()
        {
            string panelName = UINavStack.Peek();
            UIPanel newPanel;
            if (PanelResCache.TryGetValue(panelName, out var panelObj))
            {
                newPanel = CreatePanel(panelName, panelObj);
                ActivatePanel(newPanel, true);
                UIEntityStack.Push(newPanel);
            }
            else
            {
                ABMgr.Instance.LoadResAsync<GameObject>(UI_ABName, panelName, (panelObj) =>
                {
                    newPanel = CreatePanel(panelName, panelObj);
                    ActivatePanel(newPanel, true);
                    PanelResCache.Add(panelName, panelObj);
                    UIEntityStack.Push(newPanel);
                });
            }
        }
        private void ActivatePanel(UIPanel panel,bool fromRestored=false)
        {
            panel.gameObject.SetActive(true);
            if(fromRestored)
                panel.transform.SetAsFirstSibling();
            else
                panel.transform.SetAsLastSibling();
        }
        private UIPanel CreatePanel(string panelName,GameObject panelObj)
        {
            GameObject newPanelObj = GameObject.Instantiate(panelObj, uiRoot);
            newPanelObj.name = panelName;
            UIPanel newPanel = newPanelObj.GetComponent<UIPanel>();
            SetRectTransform(newPanel.transform as RectTransform);

            if (newPanel.CacheLevel == UICacheLevel.Open)
            {
                PanelCache.TryAdd(panelName, newPanel);
            }
            return newPanel;
        }
        private void SetRectTransform(RectTransform rectTransform)
        {
            rectTransform.localPosition = Vector2.zero;
            rectTransform.localScale = Vector2.one;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero; 
        }
        //ģ̬���ڵ�����Ч��
        private void SetModelWindowEffect(UIPanel mainWindow,bool isOn)
        {
            if (isOn)
            {
                mainWindow.canvasGroup.blocksRaycasts = false;
                mainWindow.canvasGroup.alpha = 0.5f;
            }
            else
            {
                mainWindow.canvasGroup.blocksRaycasts = true;
                mainWindow.canvasGroup.alpha = 1f;
            }
        }
        #endregion
    }
}

