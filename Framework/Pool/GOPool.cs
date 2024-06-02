using Paltry.Extension;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Paltry
{
    /*>>ʹ��ǰ��<<
     *  ������Ϸ�������Paltry.MonoAgent
     *>>ʹ�÷���<<
     *  1.��PaltryConst.cs����Paltry.PoolType,�ڸ�ö����ע��GameObject������
     *  2.��ʼ�������:GOPool.Instance.Warm(),
     *      ����frameLimit��ʾ��ʼ�������ü�֡���(ģ���첽),���淽����frameLimit����ͬ��
     *  3.ʹ��:GOPool.Instance.Get()
     *  4.����:GOPool.Instance.Recycle()
     *  5.������С:GOPool.Instance.Resize()
     *  6.���:GOPool.Instance.Clear()
     *  */
    /// <summary>
    /// ר������GameObject�Ķ����
    /// </summary>
    public class GOPool:CSharpSingleton<GOPool>
    {
        //�������԰ѷ�֡���ظĳ��첽����,��MonoAgent�����ж�RequiredUpdate�Ƿ�Ϊ�ղ�̫����,����Ҳ������ֱ�ӻ�ȡ���ص�progress
        private Transform PoolRoot;
        private Dictionary<GOType, PoolContainer> PoolDict;
        public GOPool()
        {
            PoolRoot = new GameObject("PoolRoot").transform;
            PoolDict = new Dictionary<GOType, PoolContainer>();
        }
        private class PoolContainer
        {
            public List<GameObject> List;
            public GameObject Prefab;
            public Transform GroupLabel;//���ڽ�����������,����������PoolRoot
            public void PreAddObj()
            {
                var obj = GameObject.Instantiate(Prefab, GroupLabel);
                obj.SetActive(false);
                List.Add(obj);
            }
        }
        /// <summary>
        /// ��ʼ�������,�÷���ֻӦ������һ��,��������ش�С��ͨ��Resize����
        /// </summary>
        /// <param name="poolType"></param>
        /// <param name="prefab"></param>
        /// <param name="size"></param>
        /// <param name="frameLimit"></param>
        public void Warm(GOType poolType,GameObject prefab,int size,int frameLimit=1)
        {
            if(PoolDict.TryGetValue(poolType,out PoolContainer pool))
            {
                Debug.LogWarning("����Ҫ�ظ�ů��");
                return;
            }
            //��ʼ��PoolContainer�������ֵ�
            var groupLabel = new GameObject(poolType.ToString()).transform;
            groupLabel.SetParent(PoolRoot);
            pool = new PoolContainer()
            {
                List = new List<GameObject>(size),
                Prefab = prefab,
                GroupLabel = groupLabel
            };
            PoolDict[poolType] = pool;
            //����Resize�����ö���س�ʼ��С
            Resize(poolType, size, frameLimit);
        }
        public GameObject Get(GOType poolType)
        {
            if (!PoolDict.TryGetValue(poolType, out var pool))
                throw new KeyNotFoundException($"{poolType}����ز�����,����ů��");

            GameObject obj;
            if (pool.List.Count == 0)
                obj = GameObject.Instantiate(pool.Prefab, pool.GroupLabel);
            else
                obj = pool.List.PopLast();
            obj.SetActive(true);
            return obj;
        }
        public void Recycle(GOType poolType,GameObject obj)
        {
            if (!PoolDict.TryGetValue(poolType, out var pool))
                throw new KeyNotFoundException($"{poolType}����ز�����,����ů��");
            obj.SetActive(false);
            pool.List.Add(obj);
        }
        /// <summary>
        /// ���������Ϊָ����С,��ȷ���������û�����屻�ó�ʱ���ø÷���,����������ʵ�ʴ�С���ܱ�Ԥ��Ҫ��
        /// </summary>
        /// <param name="poolType"></param>
        /// <param name="newSize"></param>
        /// <param name="frameLimit"></param>
        public void Resize(GOType poolType,int newSize,int frameLimit=1)
        {
            if (!PoolDict.TryGetValue(poolType, out var pool))
                throw new KeyNotFoundException($"{poolType}����ز�����,����ů��");

            int addCount = newSize - pool.List.Count;//Ҫ��ӵ�����,Ϊ�������,Ϊ�������
            int countPerFrame = Mathf.CeilToInt(addCount / frameLimit);
            if (countPerFrame == 0)
                throw new ArgumentException($"frameLimit(={frameLimit})̫��,�뿼�Ǽ�С��ֵ");

                switch (addCount,frameLimit)//frameLimitΪ1,1֡�ڼ�����,��Ϊ1���֡����
            {
                case (0,_):return;
                case ( > 0, 1):
                    for (int i = 0; i < addCount; i++)
                        pool.PreAddObj();break;
                case ( < 0, 1):
                    for (int i = 0; i < -addCount; i++)
                        GameObject.Destroy(pool.List.PopLast());break;
                case ( > 0, _):
                    MonoAgent.Instance.AddConstTimesUpdate(addCount, () =>
                    {
                        for (int i = 0; i < countPerFrame && pool.List.Count != newSize; i++)
                            pool.PreAddObj();
                    });break;
                case ( < 0, _):
                    MonoAgent.Instance.AddConstTimesUpdate(-addCount, () =>
                    {
                        for (int i = 0; i < -countPerFrame && pool.List.Count != newSize; i++)
                            GameObject.Destroy(pool.List.PopLast());
                    }); break;
            }

        }
        /// <summary>
        /// ��ն�����е�����,����ʹ��ʱ����Resize����,����Ҫ�ٵ���Warm
        /// </summary>
        /// <param name="poolType"></param>
        /// <param name="frameLimit"></param>
        public void Clear(GOType poolType,int frameLimit=1)
        {
            Resize(poolType,0, frameLimit);
        }

    }

}
