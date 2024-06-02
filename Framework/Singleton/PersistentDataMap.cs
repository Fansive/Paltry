using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paltry
{
    /// <summary>
    /// 持久化数据的内存映射(单例),与磁盘数据对应,首次获取Data时将从磁盘
    /// 加载,若没有则使用默认值
    /// 使用Save方法可将内存数据保存到磁盘
    /// 使用方法:
    /// 1.继承该类(record),提供无参构造函数作为参数列表,为父类填入文件名
    /// 2.声明数据字段,并重写为它们赋默认值的SetDefaultValue方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_fileName"></param>
    public abstract record PersistentDataMap<T>(string _fileName) where T : PersistentDataMap<T>,new()
    {
        private static T data;
        public static T Data
        {
            protected set => data = value;
            get => data ??= JsonMgr.LoadPersistentData<T>();
        }
        /// <summary>
        /// 设为默认数据,当持久化数据还未创建时自动调用该方法获取默认值
        /// 子类必须在该方法中为所有字段设置默认值
        /// 该方法也可用于将数据重置
        /// </summary>
        public abstract void SetDefaultValue();
        public void Save() => JsonMgr.Save(data,_fileName);
    }

}
