using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Paltry;
using System;
using System.Text;

namespace Paltry
{
    public class AALoader : CSharpSingleton<AALoader>
    {
        private Dictionary<string, IEnumerator> handles;

        public AALoader()
        {
            Addressables.InitializeAsync();
            handles = new Dictionary<string, IEnumerator>();
        }
        public T LoadAsset<T>(string resName)
        {
            string handleKey = resName + "_" + typeof(T).Name;

            if (handles.ContainsKey(handleKey))//���ǵ�һ�μ���
                return ((AsyncOperationHandle<T>)handles[handleKey]).Result;
            else
            {
                var handle = Addressables.LoadAssetAsync<T>(resName);
                handles.Add(handleKey, handle);
                return handle.WaitForCompletion();
            }
        }

        public IList<T> LoadAssets<T>(string key)
        {
            string handleKey = key + "_" + typeof(T).Name;

            if (handles.ContainsKey(handleKey))//���ǵ�һ�μ���
                return ((AsyncOperationHandle<IList<T>>)handles[handleKey]).Result;
            else
            {
                var handle = Addressables.LoadAssetsAsync<T>(key,null);
                handles.Add(handleKey, handle);
                return handle.WaitForCompletion();
            }
        }

        public IList<T> LoadAssets<T>(string[] keys, Addressables.MergeMode mode = Addressables.MergeMode.Intersection)
        {
            string handleKey = GetHandleKey(keys);

            if (handles.ContainsKey(handleKey))//���ǵ�һ�μ���
                return ((AsyncOperationHandle<IList<T>>)handles[handleKey]).Result;
            else
            {
                var handle = Addressables.LoadAssetsAsync<T>(new List<string>(keys),null,mode);
                handles.Add(handleKey, handle);
                return handle.WaitForCompletion();
            }
        }
        /// <summary>
        /// �첽����ָ����Դ
        /// </summary>
        public void LoadAssetAsync<T>(string resName, Action<T> callback)
        {
            string handleKey = resName + "_" + typeof(T).Name;//������Դͬ��,��ͨ����������

            if(handles.ContainsKey(handleKey))//���ǵ�һ�μ���
            {
                var handle = (AsyncOperationHandle<T>)handles[handleKey];
                if (handle.IsDone)
                    CheckedCallback(handle);
                else
                    handle.Completed += CheckedCallback;
            }

            else//��һ�μ���
            {
                var handle = Addressables.LoadAssetAsync<T>(resName);
                handle.Completed += CheckedCallback;
                handles.Add(handleKey, handle);
            }

            void CheckedCallback(AsyncOperationHandle<T> handle)
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    callback(handle.Result);
                else
                {
                    Debug.LogWarning("LoadAsset Failed");
                    handles.Remove(handleKey);
                }
            }
        }
        /// <summary>
        /// ͨ����Դ�����ǩ���첽������Դ
        /// </summary>
        /// <param name="key">��Դ�����ǩ��</param>
        public void LoadAssetsAsync<T>(string key, Action<T> callback)
        {
            string handleKey = key + "_" + typeof(T).Name;//������Դͬ��,��ͨ����������

            if (handles.ContainsKey(handleKey))//���ǵ�һ�μ���
            {
                var handle = (AsyncOperationHandle<IList<T>>)handles[handleKey];
                if (handle.IsDone)
                    CheckedCallback(handle);
                else
                    handle.Completed += CheckedCallback;
            }

            else//��һ�μ���
            {
                var handle = Addressables.LoadAssetsAsync<T>(key, null);
                handle.Completed += CheckedCallback;
                handles.Add(handleKey, handle);
            }

            void CheckedCallback(AsyncOperationHandle<IList<T>> handle)
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    foreach (var i in handle.Result)
                        callback(i);
                else
                {
                    Debug.LogWarning("LoadAsset Failed");
                    handles.Remove(handleKey);
                }
            }
        }
        /// <summary>
        /// ͨ����Դ��+��ǩ���첽������Դ
        /// </summary>
        /// <param name="keys">��ʽ:��Դ��,��ǩ1,��ǩ2...</param>
        /// <param name="mode">Ĭ��Ϊ����</param>
        public void LoadAssetsAsync<T>(string[] keys, Action<T> callback, Addressables.MergeMode mode = Addressables.MergeMode.Intersection)
        {
            string handleKey = GetHandleKey(keys);

            if (handles.ContainsKey(handleKey))//���ǵ�һ�μ���
            {
                var handle = (AsyncOperationHandle<IList<T>>)handles[handleKey];
                if (handle.IsDone)
                    CheckedCallback(handle);
                else
                    handle.Completed += CheckedCallback;
            }

            else//��һ�μ���
            {
                var handle = Addressables.LoadAssetsAsync<T>(new List<string>(keys), null,mode);
                handle.Completed += CheckedCallback;
                handles.Add(handleKey, handle);
            }

            void CheckedCallback(AsyncOperationHandle<IList<T>> handle)
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    foreach (var i in handle.Result)
                        callback(i);
                else
                {
                    Debug.LogWarning("LoadAsset Failed");
                    handles.Remove(handleKey);
                }
            }
        }

        /// <summary>
        /// ��Ҫ���ڻ�ȡ���ذٷֱ�
        /// </summary>
        public AsyncOperationHandle<T> GetHandle<T>(string key)
        {
            return (AsyncOperationHandle<T>)handles[key + "_" + typeof(T).Name];
        }
        /// <summary>
        /// ��Ҫ���ڻ�ȡ���ذٷֱ�
        /// </summary>
        public AsyncOperationHandle<IList<T>> GetHandle<T>(string[] keys)
        {
            return (AsyncOperationHandle<IList<T>>)handles[GetHandleKey(keys)];
        }
        private string GetHandleKey(string[] keys)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var key in keys)
                sb.Append(key);
            return sb.ToString();
        }
        /// <summary>
        /// ͨ����Դ�����ǩ���ͷż��ص���Դ
        /// </summary>
        /// <param name="key">��Դ�����ǩ��</param>
        public void Release<T>(string key)
        {
            key += "_" + typeof(T);
            if (handles.ContainsKey(key))
            {
                Addressables.Release<T>((AsyncOperationHandle<T>)handles[key]);
                handles.Remove(key);
            }
        }
        /// <summary>
        /// �ͷ���Դ,����Դ��+��ǩ�����صķ�ʽ���Ӧ
        /// </summary>
        /// <param name="keys">��ʽ:��Դ��,��ǩ1,��ǩ2...</param>
        public void Release<T>(string[] keys)
        {
            string handleKey = GetHandleKey(keys);

            if (handles.ContainsKey(handleKey))
            {
                Addressables.Release<T>((AsyncOperationHandle<T>)handles[handleKey]);
                handles.Remove(handleKey);
            }
        }
    }
}

