using System;
using System.Collections.Generic;
namespace Paltry
{
    /*>>ʹ��ǰ��<<
     *  ������Ϸ�������Paltry.MonoAgent
     *>>ʹ�÷���<<
     *  1.��ʼ�������,Pool<T>.Instance.Warm();T��Ҫ�ػ�������
     *      ��������:���������ǰ��������ʱ,�ȵ���Factroy�����¶���,�ٵ���OnRecycle��������ս�����
     *      ���ռ�����ӵĹ������ڲ����.���õ�����ʱ�����OnEnable
     *      ����:Factroy=>��������ķ���,OnEnable=>�������ķ���,OnRecycle=>ʧ�����ķ���
     *  2.ʹ��:Pool<T>.Instance.Get(),���ɵõ��������
     *  3.����:Pool<T>.Instance.Recycle(),�Ѵ����ն��󴫽�ȥ
     *  4.(��ѡ)��������ش�С:Pool<T>.Instance.Resize(),��Ѷ���ص���Ϊָ����С
     *      �ڶ���ر�Сʱ�����OnDestroy
     *  5.(��ѡ)��ն����:Pool<T>.Instance.Clear(),������ж������OnDestroy     
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
        /// �ڴ˴���ʼ�������,������,�Ժ�Ҳ����ֱ���޸������ί������������
        /// </summary>
        /// <param name="size">��ʼ������С</param>
        /// <param name="factory">��������ķ���</param>
        /// <param name="onEnable">�������Ļص�,��Getʱ����</param>
        /// <param name="onRecycle">���ն���Ļص�,��Recycleʱ�����ӳ�ʼ����Resizeʱ����</param>
        /// <param name="onDestroy">���ٶ���ķ���,ͨ��Resize���ٶ���������Clear�����ʱ����</param>
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
