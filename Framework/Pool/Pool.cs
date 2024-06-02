using System;
using System.Collections.Generic;
namespace Paltry
{
    /*>>使用前提<<
     *  任意游戏物体挂载Paltry.MonoAgent
     *>>使用方法<<
     *  1.初始化对象池,Pool<T>.Instance.Warm();T是要池化的类型
     *      创建过程:对象池中提前缓存物体时,先调用Factroy创建新对象,再调用OnRecycle并将其回收进池子
     *      回收加入池子的过程在内部完成.而得到对象时则调用OnEnable
     *      所以:Factroy=>创建对象的方法,OnEnable=>激活对象的方法,OnRecycle=>失活对象的方法
     *  2.使用:Pool<T>.Instance.Get(),即可得到所需对象
     *  3.回收:Pool<T>.Instance.Recycle(),把待回收对象传进去
     *  4.(可选)调整对象池大小:Pool<T>.Instance.Resize(),会把对象池调整为指定大小
     *      在对象池变小时会调用OnDestroy
     *  5.(可选)清空对象池:Pool<T>.Instance.Clear(),会对所有对象调用OnDestroy     
     * 
     *  */
    public class Pool<T> : CSharpSingleton<Pool<T>>
    {
        private Stack<T> CacheStack;
        public Func<T> Factory { get; set; }
        public Action<T> OnEnable { get; set; }
        public Action<T> OnRecycle { get; set; }
        public Action<T> OnDestroy{ get; set; }
        /// <summary>
        /// 在此处初始化对象池,并配置,稍后也可以直接修改上面的委托属性来配置
        /// </summary>
        /// <param name="size">初始容量大小</param>
        /// <param name="factory">创建对象的方法</param>
        /// <param name="onEnable">激活对象的回调,在Get时调用</param>
        /// <param name="onRecycle">回收对象的回调,在Recycle时、池子初始化、Resize时调用</param>
        /// <param name="onDestroy">销毁对象的方法,通过Resize减少对象数量和Clear对象池时调用</param>
        public void Warm(int size,Func<T> factory,Action<T>onEnable=null,Action<T> onRecycle = null,Action<T> onDestroy=null)
        {
            Factory = factory;
            OnEnable = onEnable;
            OnRecycle = onRecycle;
            OnDestroy = onDestroy;
            CacheStack = new Stack<T>(size);
            for (int i = 0; i < size; i++)
                Recycle(Factory());
        }
        public T Get()
        {
            T obj;
            if(CacheStack.Count > 0)
                obj = CacheStack.Pop();
            else
                obj = Factory();
            OnEnable?.Invoke(obj);
            return obj;
        }
        public void Recycle(T obj)
        {
            CacheStack.Push(obj);
            OnRecycle?.Invoke(obj);
        }
        public void Resize(int newSize)
        {
            int addCount = newSize - CacheStack.Count;
            if (addCount > 0)
            {
                for (int i = 0; i < addCount; i++)
                    Recycle(Factory());
            }
            else
            {
                for (int i = 0; i < -addCount; i++)
                {
                    T obj = CacheStack.Pop();
                    OnDestroy?.Invoke(obj);
                }
            }
        }
        public void Clear()
        {
            Resize(0);
        }

    }

}
