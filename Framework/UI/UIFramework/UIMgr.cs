using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
namespace Paltry
{
    /*>>使用前提<<
     *  1.任意游戏物体挂载Paltry.MonoAgent
     *  2.导入了AALoader
     *  3.Addressable分组里要有名为Canvas和EventSystem的游戏物体
     *      (UIMgr不提供配置它们的方法,要在游戏物体里自己配置好)
     *>>使用方法<<
     *UI面板制作流程:  
     *  1.在PaltryConst.UIName里填入需要用到的Panel名
     *  2.拼面板,之后在面板上挂载xxPanel(名字和1.里输入的一致),并继承UIPanel
     *  3.覆写抽象成员,即该面板的缓存策略和窗口类型
     *  4.(可选)覆写虚属性,调整淡入淡出设置以及模态窗口设置
     *  4.覆写生命周期函数,完成相关逻辑,打开/关闭面板用UIMgr的OpenPanel和PopPanel
     *      打开Prompt时用ShowPrompt,关闭同理
     *UI通信:
     *  1.通过事件中心
     *  2.比较简单的数据传递,可以在UI面板的类里声明公开静态变量让外部修改
     *  3.通过Model将数据共享,而每一个UI面板作为ViewController
     *  */
    public class UIMgr : CSharpSingleton<UIMgr>
    {
        //保存UI的实际顺序,便于在手动销毁UI时能恢复UI
        private Stack<string> UINavStack;
        //保存真正的UI实体
        private Stack<UIPanel> UIEntityStack;

        //缓存加载过的资源,在第一次从AB包加载完成后加入这里
        private Dictionary<string, GameObject> PanelResCache;
        //缓存需要缓存的面板,从PanelResCache里加载出来时加入这里
        private Dictionary<string, UIPanel> PanelCache;
        //记录打开的Prompt,用于销毁(不缓存)
        private Dictionary<string, UIPanel> PromptDict;

        //private RectTransform bgRoot;//背景层
        private RectTransform panelRoot;//普通面板层
        private RectTransform promptRoot;//提示信息层

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
            //创建Canvas和EventSystem以及UIRoot,UIRoot下是所有的面板(后续可添加其他Root)
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
        /// 用于打开Panel,打开Prompt请用ShowPrompt
        /// </summary>
        public void OpenPanel(string panelName,Action callback=null)
        {
            UIPanel newPanel = null;

            //先尝试从面板缓存里找,找到就直接激活拿来用
            if (PanelCache.TryGetValue(panelName, out newPanel))
            {
                ActivatePanel(newPanel);
            }
            //若面板缓存没有,在资源缓存里找,需要新创建一个
            else if (PanelResCache.TryGetValue(panelName, out var panelObj))
            {
                newPanel = CreatePanel(panelName, panelObj, panelRoot);
                ActivatePanel(newPanel);
            }
            //都没有,通过资源管理来加载
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
            void Callback_SetPanel()//如果需要异步加载,那么要把该回调传进去
            {
                //如果栈顶有面板才执行操作
                if (UIEntityStack.TryPeek(out var topPanel))
                {
                    if (newPanel.WindowType == UIWindowType.ModelWindow)
                    {//模态窗口,禁用交互即可(后续可添加变暗等)
                        SetModelWindowEffect(topPanel, true);
                    }
                    else if (newPanel.WindowType == UIWindowType.FullScreen)
                    {//全屏窗口,需要失活
                        topPanel.gameObject.SetActive(false);
                    }
                    else if (newPanel.WindowType == UIWindowType.NormalWindow)
                    {//普通窗口,不执行任何操作

                    }
                }
                UINavStack.Push(panelName);
                UIEntityStack.Push(newPanel);

                //最后,执行新面板的淡入
                newPanel.FadeIn();

                callback?.Invoke();
            }

        }
        public void PopPanel()
        {
            UIPanel poppedPanel = UIEntityStack.Pop();
            string poppedPanelName = UINavStack.Pop();


            //先将此时栈顶Panel激活
            if (UIEntityStack.TryPeek(out UIPanel curPanel))
            {
                if (poppedPanel.WindowType == UIWindowType.ModelWindow)
                {//先前是模态窗口,此时激活交互
                    SetModelWindowEffect(curPanel, false);
                }
                else if (poppedPanel.WindowType == UIWindowType.FullScreen)
                {
                    curPanel.gameObject.SetActive(true);
                }
                else if (poppedPanel.WindowType == UIWindowType.NormalWindow)
                {//普通窗口,不执行任何操作

                }
            }
            else//UI实体栈为空,根据UINavStack恢复UI
            {   //由于此时通过DestroyPanel,所以面板缓存里肯定没有
                RestorePanel();
            }


            poppedPanel.FadeOutAsync(PopCore);
            void PopCore()
            {
                //最后再将被Pop面板失活/销毁
                if (poppedPanel.CacheLevel == UICacheLevel.Open)
                {   //要缓存的话,仅仅失活即可
                    PanelCache[poppedPanelName].gameObject.SetActive(false);
                }
                else
                {
                    GameObject.Destroy(poppedPanel.gameObject);
                }
                //淡出结束后,恢复交互
                poppedPanel.canvasGroup.blocksRaycasts = true;
            }

        }
        /// <summary>
        /// 将留存在先前栈上的UIPanel彻底销毁(包括其缓存)
        /// <para>使用该方法时,确保要销毁的UIPanel确实存在</para>
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="alsoDestroyRes">是否同时销毁其资源文件(下次需重新从资源管理里加载)</param>
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
        /// 显示Prompt,即系统提示之类的东西,位于最上层
        /// </summary>
        /// <param name="promptName"></param>
        public void ShowPrompt(string promptName)
        {//Prompt一般比较少,所以不加入Panel缓存
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
        /// Prompt不采用栈存储,需指定关闭哪一个
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
        /// 设置EventSystem,控制全局的UI交互
        /// </summary>
        public void SetInteraction(bool isActive)
        {
            eventSystem.enabled = isActive;
        }

        #region Private Methods
        //注意,恢复Panel仅用于FullScreen,不用于其他三种(毕竟其他的也不是很大没必要删除)
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
        /// 模态窗口的效果 mainWindow是作为模态窗口"背景"的,栈中上一个Panel
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

