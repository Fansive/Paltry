using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paltry
{
    /// <summary>
    /// �־û����ݵ��ڴ�ӳ��(����),��������ݶ�Ӧ,�״λ�ȡDataʱ���Ӵ���
    /// ����,��û����ʹ��Ĭ��ֵ
    /// ʹ��Save�����ɽ��ڴ����ݱ��浽����
    /// ʹ�÷���:
    /// 1.�̳и���(record),�ṩ�޲ι��캯����Ϊ�����б�,Ϊ���������ļ���
    /// 2.���������ֶ�,����дΪ���Ǹ�Ĭ��ֵ��SetDefaultValue����
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
        /// ��ΪĬ������,���־û����ݻ�δ����ʱ�Զ����ø÷�����ȡĬ��ֵ
        /// ��������ڸ÷�����Ϊ�����ֶ�����Ĭ��ֵ
        /// �÷���Ҳ�����ڽ���������
        /// </summary>
        public abstract void SetDefaultValue();
        public void Save() => JsonMgr.Save(data,_fileName);
    }

}
