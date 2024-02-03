using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Paltry
{
    /*>>使用前提<<
     *  1.任意游戏物体挂载Paltry.MonoAgent
     *  2.导入了ABMgr
     *  3.放UI的AB包里要有名为Canvas和EventSystem的游戏物体
     *      (UIMgr不提供配置它们的方法,要在游戏物体里自己配置好)
     *>>使用方法<<
     *  1.配置放UI的AB包包名,UI_ABName
     *  2.(可选)在UIPanel类里,配置过渡参数来调整UI淡入淡出效果
     *UI面板制作流程:  
     *  1.在PaltryConst.UIName里填入需要用到的Panel名
     *  2.拼面板,之后在面板上挂载xxPanel(名字和1.里输入的一致),并继承UIPanel
     *  3.覆写抽象成员,即该面板的缓存策略和窗口类型
     *  4.覆写生命周期函数,完成相关逻辑,打开/关闭面板用UIMgr的OpenPanel和PopPanel
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

        private RectTransform uiRoot;
        private EventSystem eventSystem;
        #region Configuration Data
        private const string UI_ABName = "ui";//放UI的AB包包名
        #endregion
        public UIMgr()
        {
            UINavStack = new Stack<string>();
            UIEntityStack = new Stack<UIPanel>();
            PanelResCache = new Dictionary<string, GameObject>();
            PanelCache = new Dictionary<string, UIPanel>();

            //创建Canvas和EventSystem以及UIRoot,UIRoot下是所有的面板(后续可添加其他Root)
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

            //先尝试从面板缓存里找,找到就直接激活拿来用
            if(PanelCache.TryGetValue(panelName, out newPanel))
            {
                ActivatePanel(newPanel);
            }
            //若面板缓存没有,在资源缓存里找,需要新创建一个
            else if (PanelResCache.TryGetValue(panelName,out var panelObj))
            {
                newPanel = CreatePanel(panelName,panelObj);
                ActivatePanel(newPanel);
            }
            //都没有,从AB包里加载
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
            void Callback_SetPanel()//如果需要异步加载,那么要把该回调传进去
            {
                //如果栈顶有面板才执行操作
                if (UIEntityStack.TryPeek(out var topPanel))
                {   
                    if (newPanel.WindowType == UIWindowType.ModelWindow)
                    {//模态窗口,禁用交互即可(后续可添加变暗等)
                        SetModelWindowEffect(topPanel, true);
                    }
                    else if(newPanel.WindowType == UIWindowType.FullScreen)
                    {//全屏窗口,需要失活
                        topPanel.gameObject.SetActive(false);
                    }
                    else//普通窗口,不执行任何操作
                    {

                    }
                }
                UINavStack.Push(panelName);
                UIEntityStack.Push(newPanel);

                //最后,执行新面板的淡入
                newPanel.FadeIn();
            }

        }
        public void PopPanel()
        {
            UIPanel topPanel = UIEntityStack.Pop();
            string topPanelName = UINavStack.Pop();
            

            //先将此时栈顶Panel激活
            if(UIEntityStack.TryPeek(out UIPanel curPanel))
            {
                if(topPanel.WindowType == UIWindowType.ModelWindow)
                {//先前是模态窗口,此时激活交互
                    SetModelWindowEffect(curPanel, false);
                }
                else if(topPanel.WindowType == UIWindowType.FullScreen)
                {
                    curPanel.gameObject.SetActive(true);
                }
                else//普通窗口,不执行任何操作
                {

                }
            }
            else//UI实体栈为空,根据UINavStack恢复UI
            {   //由于此时通过DestroyPanel,所以面板缓存里肯定没有
                RestorePanel();
            }


            topPanel.FadeOutAsync(PopCore);
            void PopCore()
            {
                //最后再将被Pop面板失活/销毁
                if (topPanel.CacheLevel == UICacheLevel.Open)
                {   //要缓存的话,仅仅失活即可
                    PanelCache[topPanelName].gameObject.SetActive(false);
                }
                else
                {
                    GameObject.Destroy(topPanel.gameObject);
                }
                //淡出结束后,恢复交互
                topPanel.canvasGroup.blocksRaycasts = true;
            }
            
        }
        /// <summary>
        /// 将留存在先前栈上的UIPanel彻底销毁(包括其缓存)
        /// <para>使用该方法时,确保要销毁的UIPanel确实存在</para>
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="alsoDestroyRes">是否同时销毁其资源文件(下次需重新从AB包里加载)</param>
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
        /// 设置EventSystem,控制全局的UI交互
        /// </summary>
        public void SetInteraction(bool isActive)
        {
            eventSystem.enabled = isActive;
        }

        #region Private Methods
        //注意,恢复Panel无法用于模态窗口(这种也不应该用来恢复,因为都没有背景板)
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
        //模态窗口的设置效果
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

