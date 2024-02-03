using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace Paltry
{
    /*>>使用前提<<
     *  任意游戏物体挂载Paltry.MonoAgent
     *>>使用方法<<
     *  1.配置AB包路径:修改ABPath为加载的初始路径,修改MainABName为各平台的主包包名,
     *      确保此处的主包包名和打包的主包包名一致
     *  2.调用API加载资源即可,LoadABWithProgressAsync可以返回加载AB包的进度信息
     *      利用其返回的ABLoadInfo可以在加载条上显示加载的详细信息
     *  */
    public class ABMgr:CSharpSingleton<ABMgr>
    {
        private Dictionary<string, AssetBundle> ABDict;
        private AssetBundleManifest MainManifest;
        #region Configuration Data
        //配置AB包路径
        private readonly string ABPath = Application.streamingAssetsPath + "/";
        private string MainABName
        {
            get
            {//根据不同平台来确定主包名
#if UNITY_Android
                return "Android";
#elif UNITY_IOS
                return "IOS";
#else
                return "PC";
#endif
            }
        }
        #endregion
        public ABMgr()
        {
            ABDict = new Dictionary<string, AssetBundle>();
            //加载主包里的依赖信息表
            var mainAB = AssetBundle.LoadFromFile(ABPath + MainABName);
            MainManifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        public void LoadAB(string abName)
        {
            if (!ABDict.ContainsKey(abName))//没有加载过AB包才会加载
            {
                //加载包的依赖信息
                string[] dependencies = MainManifest.GetAllDependencies(abName);
                foreach (string depend in dependencies)
                {
                    if (!ABDict.ContainsKey(depend))
                    {
                        AssetBundle abDepend = AssetBundle.LoadFromFile(ABPath + depend);
                        ABDict.Add(depend, abDepend);
                    }
                }
                //加载真正的包
                AssetBundle ab = AssetBundle.LoadFromFile(ABPath + abName);
                CheckABResult(ab,abName);
                ABDict.Add(abName, ab);
            }
        }
        public void LoadABAsync(string abName,System.Action callback=null)
        {
            if (!ABDict.ContainsKey(abName))
            {
                MonoAgent.Instance.StartCoroutine(LoadABAsyncCore());
            }
            else
                callback?.Invoke();

            IEnumerator LoadABAsyncCore()
            {
                AssetBundleCreateRequest abCreateRequest;
                string[] dependencies = MainManifest.GetAllDependencies(abName);

                foreach (string depend in dependencies)
                {
                    if (!ABDict.ContainsKey(depend))
                    {
                        abCreateRequest = AssetBundle.LoadFromFileAsync(ABPath + depend);
                        yield return abCreateRequest;
                        ABDict.Add(depend, abCreateRequest.assetBundle);
                    }
                }

                abCreateRequest = AssetBundle.LoadFromFileAsync(ABPath+abName);
                yield return abCreateRequest;
                ABDict.Add(abName,abCreateRequest.assetBundle);

                callback?.Invoke();
            }
        }
        /// <summary>
        /// 异步加载AB包,并返回加载进度相关信息
        /// <para>如果需要显示加载进度,那么要先判断返回的ABLoadInfo是否isDone,如果是,就不应显示</para>
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="callback"></param>
        /// <returns>AB包加载进度的信息</returns>
        /// TODO:给异步加载资源也添加显示进度信息的功能
        public ABLoadInfo LoadABWithProgressAsync(string abName,System.Action callback = null)
        {
            ABLoadInfo info = new ABLoadInfo();
            if (!ABDict.ContainsKey(abName))
            {
                MonoAgent.Instance.StartCoroutine(LoadABAsyncCore());
            }
            return info;


            IEnumerator LoadABAsyncCore()
            {
                //提前过滤掉不需要加载的依赖
                string[] dependencies = MainManifest.GetAllDependencies(abName)
                    .Where(depend => !ABDict.ContainsKey(depend)).ToArray();
                info.ABNameList = dependencies.ToList();
                info.ABNameList.Add(abName);

                foreach (string ab in info.ABNameList)
                {
                    //开始加载
                    var abCreateRequest = AssetBundle.LoadFromFileAsync(ABPath + ab);
                    info.curABName = ab;

                    //加载中
                    while (!abCreateRequest.isDone)
                    {
                        info.curProgress = abCreateRequest.progress;
                        yield return PaltryConst.LoadInfoUpdateInternal;
                    }

                    //加载完成
                    info.curProgress = abCreateRequest.progress;
                    info.loadedCount++;
                    ABDict.Add(ab, abCreateRequest.assetBundle);
                }

                callback?.Invoke();
            }

        }
        public Object LoadRes(string abName,string resName)
        {
            LoadAB(abName);
            return ABDict[abName].LoadAsset(resName) ?? throw new System.ArgumentException($"无法找到资源<{resName}>,请检查资源名");
        }
        public T LoadRes<T>(string abName, string resName) where T : Object
        {
            LoadAB(abName);
            return ABDict[abName].LoadAsset<T>(resName) ?? throw new System.ArgumentException($"无法找到资源<{resName}>,请检查资源名");
        }
        public Object LoadRes(string abName, string resName,System.Type type)
        {
            LoadAB(abName);
            return ABDict[abName].LoadAsset(resName,type) ?? throw new System.ArgumentException($"无法找到资源<{resName}>,请检查资源名");
        }
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="resName"></param>
        /// <param name="isABAsync">如果资源所需的AB包还未加载,那么此时加载AB包的方式是否为异步?</param>
        /// <returns></returns>
        public void LoadResAsync(string abName, string resName,System.Action<Object> callback, bool isABAsync = true)
        {
            if (isABAsync)
                LoadABAsync(abName,()=>MonoAgent.Instance.StartCoroutine(LoadResAsyncCore()));
            else
            {
                LoadAB(abName);
                MonoAgent.Instance.StartCoroutine(LoadResAsyncCore());
            }

            IEnumerator LoadResAsyncCore()
            {
                AssetBundleRequest abRequest = ABDict[abName].LoadAssetAsync(resName);
                yield return abRequest;
                callback(abRequest.asset);
            }
        }
        public void LoadResAsync<T>(string abName, string resName, System.Action<T> callback, bool isABAsync = true) where T:Object
        {
            if (isABAsync)
                LoadABAsync(abName, () => MonoAgent.Instance.StartCoroutine(LoadResAsyncCore()));
            else
            {
                LoadAB(abName);
                MonoAgent.Instance.StartCoroutine(LoadResAsyncCore());
            }

            IEnumerator LoadResAsyncCore()
            {
                AssetBundleRequest abRequest = ABDict[abName].LoadAssetAsync<T>(resName);
                yield return abRequest;
                callback((T)abRequest.asset);
            }
        }
        public void LoadResAsync(string abName, string resName, System.Action<Object> callback,System.Type type, bool isABAsync = true)
        {
            if (isABAsync)
                LoadABAsync(abName, () => MonoAgent.Instance.StartCoroutine(LoadResAsyncCore()));
            else
            {
                LoadAB(abName);
                MonoAgent.Instance.StartCoroutine(LoadResAsyncCore());
            }

            IEnumerator LoadResAsyncCore()
            {
                AssetBundleRequest abRequest = ABDict[abName].LoadAssetAsync(resName,type);
                yield return abRequest;
                callback(abRequest.asset);
            }
        }

        public void UnLoadAB(string abName)
        {
            if(ABDict.TryGetValue(abName,out AssetBundle ab))
            {
                ab.Unload(false);
                ABDict.Remove(abName);
            }
        }
        public void UnLoadABAsync(string abName,System.Action callback=null)
        {
            if (ABDict.TryGetValue(abName, out AssetBundle ab))
            {
                MonoAgent.Instance.StartCoroutine(UnLoadABAsyncCore());
                IEnumerator UnLoadABAsyncCore()
                {
                    var asyncOp = ab.UnloadAsync(false);
                    yield return asyncOp;
                    ABDict.Remove(abName);
                    callback?.Invoke();
                }
            }
            else
                callback?.Invoke();
        }
        public void ClearAB()//清空所有AB包,但MainManifest不清空
        {
            AssetBundle.UnloadAllAssetBundles(false);
            ABDict.Clear();
        }
        private void CheckABResult(AssetBundle ab,string abName)
        {
            if(ab == null)
            {
                throw new System.ArgumentException($"AB包<{abName}>加载失败,请检查包名");
            }
        }
    }
    /// <summary>
    /// AB包的加载信息,可获取正在加载的包名,当前包加载进度,加载总列表(依赖列表+自己)
    /// <para>在加载进度条显示所有要加载的包个数:把所有加载信息的加载总列表求并集</para>
    /// </summary>
    public class ABLoadInfo
    {
        public string curABName;//当前正在加载的AB包包名
        public float curProgress;//当前正在加载的AB包的加载进度
        public float loadedCount;//已加载完成的包个数
        public List<string> ABNameList = new List<string>();
        public bool isDone => loadedCount == ABNameList.Count;
    }
}

