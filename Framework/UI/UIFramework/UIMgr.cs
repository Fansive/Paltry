using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
namespace Paltry
{
    /*>>ʹ��ǰ��<<
     *  1.������Ϸ�������Paltry.MonoAgent
     *  2.������AALoader
     *  3.Addressable������Ҫ����ΪCanvas��EventSystem����Ϸ����
     *      (UIMgr���ṩ�������ǵķ���,Ҫ����Ϸ�������Լ����ú�)
     *>>ʹ�÷���<<
     *UI�����������:  
     *  1.��PaltryConst.UIName��������Ҫ�õ���Panel��
     *  2.ƴ���,֮��������Ϲ���xxPanel(���ֺ�1.�������һ��),���̳�UIPanel
     *  3.��д�����Ա,�������Ļ�����Ժʹ�������
     *  4.(��ѡ)��д������,�������뵭�������Լ�ģ̬��������
     *  4.��д�������ں���,�������߼�,��/�ر������UIMgr��OpenPanel��PopPanel
     *      ��Promptʱ��ShowPrompt,�ر�ͬ��
     *UIͨ��:
     *  1.ͨ���¼�����
     *  2.�Ƚϼ򵥵����ݴ���,������UI������������������̬�������ⲿ�޸�
     *  3.ͨ��Model�����ݹ���,��ÿһ��UI�����ΪViewController
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
        //��¼�򿪵�Prompt,��������(������)
        private Dictionary<string, UIPanel> PromptDict;

        //private RectTransform bgRoot;//������
        private RectTransform panelRoot;//��ͨ����
        private RectTransform promptRoot;//��ʾ��Ϣ��

        public RectTransform Canvas { get; private set; }
        public RectTransform WorldCanvas { get; private set; }
        private EventSystem eventSystem;
        #region Configuration Data
        private const bool UseWorldCanvas = false;
        #endregion
        public UIMgr()
        {
            UINavStack = new Stack<string>();
            UIEntityStack = new Stack<UIPanel>();
            PanelResCache = new Dictionary<string, GameObject>();
            PanelCache = new Dictionary<string, UIPanel>();
            PromptDict = new Dictionary<string, UIPanel>();

            BuildUILayerRoot();
        }
        private void BuildUILayerRoot()
        {
            //����Canvas��EventSystem�Լ�UIRoot,UIRoot�������е����(�������������Root)
            var eventSysObj = GameObject.Instantiate(AALoader.Instance.LoadAsset<GameObject>("EventSystem"));
            eventSysObj.name = "EventSystem";
            eventSystem = eventSysObj.GetComponent<EventSystem>();

            var canvasObj = GameObject.Instantiate(AALoader.Instance.LoadAsset<GameObject>("Canvas"));
            canvasObj.name = "Canvas";
            Canvas = canvasObj.GetComponent<RectTransform>();

            if (UseWorldCanvas)
            {
                var worldCanvasObj = GameObject.Instantiate(AALoader.Instance.LoadAsset<GameObject>("WorldCanvas"));
                worldCanvasObj.name = "WorldCanvas";
                WorldCanvas = worldCanvasObj.GetComponent<RectTransform>();
            }

            panelRoot = new GameObject("PanelRoot").AddComponent<RectTransform>();
            panelRoot.SetParent(canvasObj.transform);
            SetRectTransform(panelRoot);

            promptRoot = new GameObject("PromptRoot").AddComponent<RectTransform>();
            promptRoot.SetParent(canvasObj.transform);
            SetRectTransform(promptRoot);
        }
        /// <summary>
        /// ���ڴ�Panel,��Prompt����ShowPrompt
        /// </summary>
        public void OpenPanel(string panelName,Action callback=null)
        {
            UIPanel newPanel = null;

            //�ȳ��Դ���建������,�ҵ���ֱ�Ӽ���������
            if (PanelCache.TryGetValue(panelName, out newPanel))
            {
                ActivatePanel(newPanel);
            }
            //����建��û��,����Դ��������,��Ҫ�´���һ��
            else if (PanelResCache.TryGetValue(panelName, out var panelObj))
            {
                newPanel = CreatePanel(panelName, panelObj, panelRoot);
                ActivatePanel(newPanel);
            }
            //��û��,ͨ����Դ����������
            else
            {
                AALoader.Instance.LoadAssetAsync<GameObject>(panelName, (panelObj) =>
                {
                    newPanel = CreatePanel(panelName, panelObj, panelRoot);
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
                    else if (newPanel.WindowType == UIWindowType.FullScreen)
                    {//ȫ������,��Ҫʧ��
                        topPanel.gameObject.SetActive(false);
                    }
                    else if (newPanel.WindowType == UIWindowType.NormalWindow)
                    {//��ͨ����,��ִ���κβ���

                    }
                }
                UINavStack.Push(panelName);
                UIEntityStack.Push(newPanel);

                //���,ִ�������ĵ���
                newPanel.FadeIn();

                callback?.Invoke();
            }

        }
        public void PopPanel()
        {
            UIPanel poppedPanel = UIEntityStack.Pop();
            string poppedPanelName = UINavStack.Pop();


            //�Ƚ���ʱջ��Panel����
            if (UIEntityStack.TryPeek(out UIPanel curPanel))
            {
                if (poppedPanel.WindowType == UIWindowType.ModelWindow)
                {//��ǰ��ģ̬����,��ʱ�����
                    SetModelWindowEffect(curPanel, false);
                }
                else if (poppedPanel.WindowType == UIWindowType.FullScreen)
                {
                    curPanel.gameObject.SetActive(true);
                }
                else if (poppedPanel.WindowType == UIWindowType.NormalWindow)
                {//��ͨ����,��ִ���κβ���

                }
            }
            else//UIʵ��ջΪ��,����UINavStack�ָ�UI
            {   //���ڴ�ʱͨ��DestroyPanel,������建����϶�û��
                RestorePanel();
            }


            poppedPanel.FadeOutAsync(PopCore);
            void PopCore()
            {
                //����ٽ���Pop���ʧ��/����
                if (poppedPanel.CacheLevel == UICacheLevel.Open)
                {   //Ҫ����Ļ�,����ʧ���
                    PanelCache[poppedPanelName].gameObject.SetActive(false);
                }
                else
                {
                    GameObject.Destroy(poppedPanel.gameObject);
                }
                //����������,�ָ�����
                poppedPanel.canvasGroup.blocksRaycasts = true;
            }

        }
        /// <summary>
        /// ����������ǰջ�ϵ�UIPanel��������(�����仺��)
        /// <para>ʹ�ø÷���ʱ,ȷ��Ҫ���ٵ�UIPanelȷʵ����</para>
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="alsoDestroyRes">�Ƿ�ͬʱ��������Դ�ļ�(�´������´���Դ���������)</param>
        public void DestroyPanel(bool alsoDestroyRes, params string[] panelNames)
        {
            var queue = new Queue<UIPanel>();
            for (int i = 0, j = 0; i < UINavStack.Count && j < panelNames.Length; i++)
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
        /// ��ʾPrompt,��ϵͳ��ʾ֮��Ķ���,λ�����ϲ�
        /// </summary>
        /// <param name="promptName"></param>
        public void ShowPrompt(string promptName)
        {//Promptһ��Ƚ���,���Բ�����Panel����
            UIPanel newPrompt;
            if (PanelResCache.TryGetValue(promptName, out var panelObj))
            {
                newPrompt = CreatePanel(promptName, panelObj, promptRoot);
                ActivatePanel(newPrompt);
                PromptDict.Add(promptName, newPrompt);
                newPrompt.FadeIn();
            }
            else
            {
                AALoader.Instance.LoadAssetAsync<GameObject>(promptName, (panelObj) =>
                {
                    newPrompt = CreatePanel(promptName, panelObj, promptRoot);
                    ActivatePanel(newPrompt);
                    PromptDict.Add(promptName, newPrompt);
                    PanelResCache.Add(promptName, panelObj);
                    newPrompt.FadeIn();
                });
            }
        }
        /// <summary>
        /// Prompt������ջ�洢,��ָ���ر���һ��
        /// </summary>
        public void ClosePrompt(string promptName)
        {
            if (PromptDict.TryGetValue(promptName, out var prompt))
            {
                prompt.FadeOutAsync(ClosePromptCore);
            }
            void ClosePromptCore()
            {
                PromptDict.Remove(promptName);
                GameObject.Destroy(prompt.gameObject);
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
        //ע��,�ָ�Panel������FullScreen,��������������(�Ͼ�������Ҳ���Ǻܴ�û��Ҫɾ��)
        private void RestorePanel()
        {
            string panelName = UINavStack.Peek();
            UIPanel newPanel;
            if (PanelResCache.TryGetValue(panelName, out var panelObj))
            {
                newPanel = CreatePanel(panelName, panelObj, panelRoot);
                ActivatePanel(newPanel, true);
                UIEntityStack.Push(newPanel);
            }
            else
            {
                AALoader.Instance.LoadAssetAsync<GameObject>(panelName, (panelObj) =>
                {
                    newPanel = CreatePanel(panelName, panelObj, panelRoot);
                    ActivatePanel(newPanel, true);
                    PanelResCache.Add(panelName, panelObj);
                    UIEntityStack.Push(newPanel);
                });
            }
        }
        private void ActivatePanel(UIPanel panel, bool fromRestored = false)
        {
            panel.gameObject.SetActive(true);
            if (fromRestored)
                panel.transform.SetAsFirstSibling();
            else
                panel.transform.SetAsLastSibling();
        }
        private UIPanel CreatePanel(string panelName, GameObject panelObj, RectTransform uiRoot)
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
            rectTransform.localPosition = new Vector2(-10, 0);
            rectTransform.localScale = Vector2.one;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
        }
        /// <summary>
        /// ģ̬���ڵ�Ч�� mainWindow����Ϊģ̬����"����"��,ջ����һ��Panel
        /// </summary>
        private void SetModelWindowEffect(UIPanel mainWindow, bool isOn)
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

