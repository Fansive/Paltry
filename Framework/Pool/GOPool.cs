using Paltry.Extension;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Paltry
{
    /*>>使用前提<<
     *  任意游戏物体挂载Paltry.MonoAgent
     *>>使用方法<<
     *  1.在PaltryConst.cs里有Paltry.PoolType,在该枚举里注册GameObject的名字
     *  2.初始化对象池:GOPool.Instance.Warm(),
     *      其中frameLimit表示初始化操作用几帧完成(模拟异步),后面方法的frameLimit参数同理
     *  3.使用:GOPool.Instance.Get()
     *  4.回收:GOPool.Instance.Recycle()
     *  5.调整大小:GOPool.Instance.Resize()
     *  6.清空:GOPool.Instance.Clear()
     *  */
    /// <summary>
    /// 专门用于GameObject的对象池
    /// </summary>
    public class GOPool:CSharpSingleton<GOPool>
    {
        //后续可以把分帧加载改成异步加载,在MonoAgent里面判断RequiredUpdate是否为空不太方便,而且也不方便直接获取加载的progress
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
            public Transform GroupLabel;//用于将对象分组管理,它本身属于PoolRoot
            public void PreAddObj()
            {
                var obj = GameObject.Instantiate(Prefab, GroupLabel);
                obj.SetActive(false);
                List.Add(obj);
            }
        }
        /// <summary>
        /// 初始化对象池,该方法只应被调用一次,调整对象池大小可通过Resize方法
        /// </summary>
        /// <param name="poolType"></param>
        /// <param name="prefab"></param>
        /// <param name="size"></param>
        /// <param name="frameLimit"></param>
        public void Warm(GOType poolType,GameObject prefab,int size,int frameLimit=1)
        {
            if(PoolDict.TryGetValue(poolType,out PoolContainer pool))
            {
                Debug.LogWarning("不需要重复暖池");
                return;
            }
            //初始化PoolContainer并加入字典
            var groupLabel = new GameObject(poolType.ToString()).transform;
            groupLabel.SetParent(PoolRoot);
            pool = new PoolContainer()
            {
                List = new List<GameObject>(size),
                Prefab = prefab,
                GroupLabel = groupLabel
            };
            PoolDict[poolType] = pool;
            //调用Resize来设置对象池初始大小
            Resize(poolType, size, frameLimit);
        }
        public GameObject Get(GOType poolType)
        {
            if (!PoolDict.TryGetValue(poolType, out var pool))
                throw new KeyNotFoundException($"{poolType}对象池不存在,请先暖池");

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
                throw new KeyNotFoundException($"{poolType}对象池不存在,请先暖池");
            obj.SetActive(false);
            pool.List.Add(obj);
        }
        /// <summary>
        /// 调整对象池为指定大小,请确保对象池中没有物体被拿出时调用该方法,否则调整后的实际大小可能比预期要大
        /// </summary>
        /// <param name="poolType"></param>
        /// <param name="newSize"></param>
        /// <param name="frameLimit"></param>
        public void Resize(GOType poolType,int newSize,int frameLimit=1)
        {
            if (!PoolDict.TryGetValue(poolType, out var pool))
                throw new KeyNotFoundException($"{poolType}对象池不存在,请先暖池");

            int addCount = newSize - pool.List.Count;//要添加的数量,为正则添加,为负则减少
            int countPerFrame = Mathf.CeilToInt(addCount / frameLimit);
            if (countPerFrame == 0)
                throw new ArgumentException($"frameLimit(={frameLimit})太大,请考虑减小该值");

                switch (addCount,frameLimit)//frameLimit为1,1帧内加载完,不为1则分帧加载
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
        /// 清空对象池中的物体,后续使用时调用Resize即可,不需要再调用Warm
        /// </summary>
        /// <param name="poolType"></param>
        /// <param name="frameLimit"></param>
        public void Clear(GOType poolType,int frameLimit=1)
        {
            Resize(poolType,0, frameLimit);
        }

    }

}
