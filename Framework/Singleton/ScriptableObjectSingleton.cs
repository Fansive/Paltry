using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Paltry
{
    /// <summary>
    /// ��SO������
    /// �����ַ��ʷ�ʽ:�ڱ༭���к�������ʱ
    /// ��ѡ�����ڱ༭����,����override <see cref="ScriptableObjectSingleton{T}.LoadPath"/>
    /// ����Ҫ�ֶ�����SO,���û��,���к���Զ�����
    /// ��ѡ����������ʱ,��Ҫ��SO�ֶ������Addressable��
    /// Ҳ�������ַ��ʷ�ʽ��ѡ��
    /// ʹ������:
    /// 1.�½��ű�,�̳��Ը���,д����������Բ��ṩLoadPath��LoadWay
    /// 2.��������
    /// 3.���к�SO�ʹ�������,������������������,�����Ժ����ٹ�
    /// PS:��SO�����,����´���SO,ԭ���Ŀ��ֶ�ɾ��
    /// </summary>
    public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObjectSingleton<T>
    {
        /// <summary>
        /// �����ڱ༭������ص���Դ,ָ������·��Ŀ¼(ֱ�Ӹ���Unity���·������)
        /// </summary>
        protected virtual string LoadPath { get; }
        protected abstract SoLoadWay LoadWay { get; }

        private string soName =>'/' + typeof(T).Name + ".asset";

        private static T so;
        public static T SO
        {
            get
            {
                if (so == null)
                { 
                    so = ScriptableObject.CreateInstance<T>();
                    if((so.LoadWay & SoLoadWay.Editor) != 0)
                    {
#if UNITY_EDITOR
                        var asset = AssetDatabase.LoadAssetAtPath<T>(so.LoadPath+so.soName);
                        if (asset == null)
                        {
                            AssetDatabase.CreateAsset(so,so.LoadPath+so.soName);
                            AssetDatabase.SaveAssets();
                        }
                        else so = asset;
#endif
                    }
                    else if((so.LoadWay & SoLoadWay.Runtime) != 0)
                    {
                        so = AALoader.Instance.LoadAsset<T>(typeof(T).Name);
                    }
                }
                
                return so;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// ����Ϸ��ʼ��ʱ,�ҵ����м̳���<see cref="ScriptableObjectSingleton{T}"/>������
        /// ���������ǵ�get������ˢ��SO����
        /// �÷������ڱ༭���»�������ʱ�Զ�����
        /// </summary>
        public static void RefreshSO()
        {
            foreach (var so in from t in ReflectionUtil.CurTypes
                               where t.BaseType?.IsGenericType == true
                               where t.BaseType.GetGenericTypeDefinition() ==
                                   typeof(ScriptableObjectSingleton<>)
                               select t)
            {
                _ = so.BaseType.GetProperty("SO").GetValue(null);
            }
        }
    }
#endif
    [Flags]
    public enum SoLoadWay
    {
        Editor = 1<<0,
        Runtime = 1<<1,
        EditorAndRuntime = Editor | Runtime,
    }
}

