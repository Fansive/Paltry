using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace Paltry
{
    /*>>ʹ��ǰ��<<
     *  ������Ϸ�������Paltry.MonoAgent
     *>>ʹ�÷���<<
     *  1.����AB��·��:�޸�ABPathΪ���صĳ�ʼ·��,�޸�MainABNameΪ��ƽ̨����������,
     *      ȷ���˴������������ʹ������������һ��
     *  2.����API������Դ����,LoadABWithProgressAsync���Է��ؼ���AB���Ľ�����Ϣ
     *      �����䷵�ص�ABLoadInfo�����ڼ���������ʾ���ص���ϸ��Ϣ
     *  */
    public class ABMgr:CSharpSingleton<ABMgr>
    {
        private Dictionary<string, AssetBundle> ABDict;
        private AssetBundleManifest MainManifest;
        #region Configuration Data
        //����AB��·��
        private readonly string ABPath = Application.streamingAssetsPath + "/";
        private string MainABName
        {
            get
            {//���ݲ�ͬƽ̨��ȷ��������
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
            //�����������������Ϣ��
            var mainAB = AssetBundle.LoadFromFile(ABPath + MainABName);
            MainManifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        public void LoadAB(string abName)
        {
            if (!ABDict.ContainsKey(abName))//û�м��ع�AB���Ż����
            {
                //���ذ���������Ϣ
                string[] dependencies = MainManifest.GetAllDependencies(abName);
                foreach (string depend in dependencies)
                {
                    if (!ABDict.ContainsKey(depend))
                    {
                        AssetBundle abDepend = AssetBundle.LoadFromFile(ABPath + depend);
                        ABDict.Add(depend, abDepend);
                    }
                }
                //���������İ�
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
        /// �첽����AB��,�����ؼ��ؽ��������Ϣ
        /// <para>�����Ҫ��ʾ���ؽ���,��ôҪ���жϷ��ص�ABLoadInfo�Ƿ�isDone,�����,�Ͳ�Ӧ��ʾ</para>
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="callback"></param>
        /// <returns>AB�����ؽ��ȵ���Ϣ</returns>
        /// TODO:���첽������ԴҲ�����ʾ������Ϣ�Ĺ���
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
                //��ǰ���˵�����Ҫ���ص�����
                string[] dependencies = MainManifest.GetAllDependencies(abName)
                    .Where(depend => !ABDict.ContainsKey(depend)).ToArray();
                info.ABNameList = dependencies.ToList();
                info.ABNameList.Add(abName);

                foreach (string ab in info.ABNameList)
                {
                    //��ʼ����
                    var abCreateRequest = AssetBundle.LoadFromFileAsync(ABPath + ab);
                    info.curABName = ab;

                    //������
                    while (!abCreateRequest.isDone)
                    {
                        info.curProgress = abCreateRequest.progress;
                        yield return PaltryConst.LoadInfoUpdateInternal;
                    }

                    //�������
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
            return ABDict[abName].LoadAsset(resName) ?? throw new System.ArgumentException($"�޷��ҵ���Դ<{resName}>,������Դ��");
        }
        public T LoadRes<T>(string abName, string resName) where T : Object
        {
            LoadAB(abName);
            return ABDict[abName].LoadAsset<T>(resName) ?? throw new System.ArgumentException($"�޷��ҵ���Դ<{resName}>,������Դ��");
        }
        public Object LoadRes(string abName, string resName,System.Type type)
        {
            LoadAB(abName);
            return ABDict[abName].LoadAsset(resName,type) ?? throw new System.ArgumentException($"�޷��ҵ���Դ<{resName}>,������Դ��");
        }
        /// <summary>
        /// �첽������Դ
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="resName"></param>
        /// <param name="isABAsync">�����Դ�����AB����δ����,��ô��ʱ����AB���ķ�ʽ�Ƿ�Ϊ�첽?</param>
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
        public void ClearAB()//�������AB��,��MainManifest�����
        {
            AssetBundle.UnloadAllAssetBundles(false);
            ABDict.Clear();
        }
        private void CheckABResult(AssetBundle ab,string abName)
        {
            if(ab == null)
            {
                throw new System.ArgumentException($"AB��<{abName}>����ʧ��,�������");
            }
        }
    }
    /// <summary>
    /// AB���ļ�����Ϣ,�ɻ�ȡ���ڼ��صİ���,��ǰ�����ؽ���,�������б�(�����б�+�Լ�)
    /// <para>�ڼ��ؽ�������ʾ����Ҫ���صİ�����:�����м�����Ϣ�ļ������б��󲢼�</para>
    /// </summary>
    public class ABLoadInfo
    {
        public string curABName;//��ǰ���ڼ��ص�AB������
        public float curProgress;//��ǰ���ڼ��ص�AB���ļ��ؽ���
        public float loadedCount;//�Ѽ�����ɵİ�����
        public List<string> ABNameList = new List<string>();
        public bool isDone => loadedCount == ABNameList.Count;
    }
}

