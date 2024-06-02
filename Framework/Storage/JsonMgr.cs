using LitJson;
using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using NPOI.SS.Formula.Functions;

namespace Paltry
{
    public enum JsonType
    {
        JsonUtility,
        LitJson
    }
    public static class LitJsonExtension
    {
        public static void WriteProperty(this JsonWriter writer, string propertyName, bool boolean)
        {
            writer.WritePropertyName(propertyName);
            writer.Write(boolean);
        }
        public static void WriteProperty(this JsonWriter writer, string propertyName, decimal number)
        {
            writer.WritePropertyName(propertyName);
            writer.Write(number);
        }
        public static void WriteProperty(this JsonWriter writer, string propertyName, double number)
        {
            writer.WritePropertyName(propertyName);
            writer.Write(number);
        }
        public static void WriteProperty(this JsonWriter writer, string propertyName, float number)
        {
            writer.WritePropertyName(propertyName);
            writer.Write(number);
        }
        public static void WriteProperty(this JsonWriter writer, string propertyName, int number)
        {
            writer.WritePropertyName(propertyName);
            writer.Write(number);
        }
        public static void WriteProperty(this JsonWriter writer, string propertyName, long number)
        {
            writer.WritePropertyName(propertyName);
            writer.Write(number);
        }
        public static void WriteProperty(this JsonWriter writer, string propertyName, string str)
        {
            writer.WritePropertyName(propertyName);
            writer.Write(str);
        }
        public static void WriteProperty(this JsonWriter writer, string propertyName, ulong number)
        {
            writer.WritePropertyName(propertyName);
            writer.Write(number);
        }
    }
    public class JsonMgr
    {
        #region Configuration Data
        /// <summary>
        /// ��json�ļ�д��Ϊ�û��Ѻõ��Ű�,�ó�����ע�뵽LitJson��
        /// </summary>
        public const bool Pretty_Print = true;
        #endregion
        static JsonMgr()
        {
            JsonMapper.RegisterExporter<Vector2>((vector2, writer) =>
            {
                writer.WriteObjectStart();//��ʼд�����
                writer.WriteProperty("x", vector2.x);//д��������
                writer.WriteProperty("y", vector2.y);
                writer.WriteObjectEnd();
            });

            JsonMapper.RegisterExporter<Vector3>((vector3, writer) =>
            {
                writer.WriteObjectStart();
                writer.WriteProperty("x", vector3.x);
                writer.WriteProperty("y", vector3.y);
                writer.WriteProperty("z", vector3.z);
                writer.WriteObjectEnd();
            });

            JsonMapper.RegisterExporter<Quaternion>((quaternion, writer) =>
            {
                writer.WriteObjectStart();
                writer.WriteProperty("x", quaternion.x);
                writer.WriteProperty("y", quaternion.y);
                writer.WriteProperty("z", quaternion.z);
                writer.WriteProperty("w", quaternion.w);
                writer.WriteObjectEnd();
            });

        }
        public static void Save(object data, string fileName, JsonType type=JsonType.LitJson)
        {
            string jsonString = type switch
            {
                JsonType.JsonUtility => JsonUtility.ToJson(data),
                JsonType.LitJson => JsonMapper.ToJson(data),
            };
            File.WriteAllText(Application.persistentDataPath+"/"+fileName+".json", jsonString);
        }
        /// <summary>
        /// ���ļ����浽streamingAssetsPath,�ú������ڱ༭���µ���,���ڱ���Ĭ�ϵ���������
        /// </summary>
        public static void Save2StreamingAssets(object data, string fileName, JsonType type=JsonType.LitJson)
        {
            string jsonString = type switch
            {
                JsonType.JsonUtility => JsonUtility.ToJson(data),
                JsonType.LitJson => JsonMapper.ToJson(data),
            };
            File.WriteAllText(Application.streamingAssetsPath + "/"+fileName+".json", jsonString);
        }
        /// <summary>
        /// ����<see cref="PersistentDataMap{T}"/>,�ӳ־û�Ŀ¼��������,���û��������ʹ��
        /// <see cref="PersistentDataMap{T}.SetDefaultValue"/>��ȡĬ��ֵ������
        /// </summary>
        public static T LoadPersistentData<T>() where T : PersistentDataMap<T>,new()
        {
            T data = new();
            string path = Application.persistentDataPath + "/" + data._fileName + ".json";
            if (File.Exists(path))//�������,ֱ�Ӽ���
                data = GetObjFromJson<T>(File.ReadAllText(path));
            else
            {//������,��ȡĬ��ֵ,������
                data.SetDefaultValue();
                Save(data, data._fileName);
            }
            return data;
        }
        public static T Load<T>(string fileName,JsonType type=JsonType.LitJson)
        {
            string path = Application.persistentDataPath+"/"+fileName+".json";
            if(!File.Exists(path))//����־û������ļ�����û��,����ý���ļ������ȡ(���ڳ��μ���)
            {
                path = Application.streamingAssetsPath+"/"+fileName+".json";
                if(!File.Exists(path))//��ý��������Ҳû��,�׳��쳣
                    throw new FileNotFoundException("��streamingAssetsPath��persistentDataPath�о�δ�ҵ��ļ�", fileName);
            }
            string jsonString = File.ReadAllText(path);
            return GetObjFromJson<T>(jsonString, type);
        }

        public static bool TryLoad<T>(string fileName, out T result,JsonType type = JsonType.LitJson)
        {
            string path = Application.persistentDataPath + "/" + fileName + ".json";
            if (!File.Exists(path))//����־û������ļ�����û��,����ý���ļ������ȡ(���ڳ��μ���)
            {
                path = Application.streamingAssetsPath + "/" + fileName + ".json";
                if (!File.Exists(path))//��ý��������Ҳû��,�׳��쳣
                {
                    result = default(T);
                    return false;
                }
            }
            string jsonString = File.ReadAllText(path);
            result = GetObjFromJson<T>(jsonString, type);
            return true;
        }

        static T GetObjFromJson<T>(string jsonString,JsonType type=JsonType.LitJson)
        {
            return type switch
            {
                JsonType.JsonUtility => JsonUtility.FromJson<T>(jsonString),
                JsonType.LitJson => JsonMapper.ToObject<T>(jsonString),
            };
        }
    }
    /// <summary>
    /// �������л��ı�ǩ(����LitJson)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class JsonIgnore : Attribute
    {

    }
}

