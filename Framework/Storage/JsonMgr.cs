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
        /// 将json文件写入为用户友好的排版,该常量将注入到LitJson中
        /// </summary>
        public const bool Pretty_Print = true;
        #endregion
        static JsonMgr()
        {
            JsonMapper.RegisterExporter<Vector2>((vector2, writer) =>
            {
                writer.WriteObjectStart();//开始写入对象
                writer.WriteProperty("x", vector2.x);//写入属性名
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
        /// 将文件保存到streamingAssetsPath,该函数仅在编辑器下调用,用于保存默认的配置数据
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
        /// 用于<see cref="PersistentDataMap{T}"/>,从持久化目录加载数据,如果没有数据则使用
        /// <see cref="PersistentDataMap{T}.SetDefaultValue"/>获取默认值并保存
        /// </summary>
        public static T LoadPersistentData<T>() where T : PersistentDataMap<T>,new()
        {
            T data = new();
            string path = Application.persistentDataPath + "/" + data._fileName + ".json";
            if (File.Exists(path))//如果存在,直接加载
                data = GetObjFromJson<T>(File.ReadAllText(path));
            else
            {//不存在,获取默认值,并保存
                data.SetDefaultValue();
                Save(data, data._fileName);
            }
            return data;
        }
        public static T Load<T>(string fileName,JsonType type=JsonType.LitJson)
        {
            string path = Application.persistentDataPath+"/"+fileName+".json";
            if(!File.Exists(path))//如果持久化数据文件夹里没有,从流媒体文件夹里读取(用于初次加载)
            {
                path = Application.streamingAssetsPath+"/"+fileName+".json";
                if(!File.Exists(path))//流媒体数据里也没有,抛出异常
                    throw new FileNotFoundException("在streamingAssetsPath和persistentDataPath中均未找到文件", fileName);
            }
            string jsonString = File.ReadAllText(path);
            return GetObjFromJson<T>(jsonString, type);
        }

        public static bool TryLoad<T>(string fileName, out T result,JsonType type = JsonType.LitJson)
        {
            string path = Application.persistentDataPath + "/" + fileName + ".json";
            if (!File.Exists(path))//如果持久化数据文件夹里没有,从流媒体文件夹里读取(用于初次加载)
            {
                path = Application.streamingAssetsPath + "/" + fileName + ".json";
                if (!File.Exists(path))//流媒体数据里也没有,抛出异常
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
    /// 跳过序列化的标签(仅限LitJson)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class JsonIgnore : Attribute
    {

    }
}

