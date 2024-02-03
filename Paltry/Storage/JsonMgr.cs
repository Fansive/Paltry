using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
namespace Paltry
{
    public enum JsonType
    {
        JsonUtility,
        LitJson
    }
    public class JsonMgr
    {
        public static void Save(object data, string fileName, JsonType type=JsonType.LitJson)
        {
            string jsonString = type switch
            {
                JsonType.JsonUtility => JsonUtility.ToJson(data),
                JsonType.LitJson => JsonMapper.ToJson(data),
            };
            File.WriteAllText(Application.persistentDataPath+"/"+fileName+".json", jsonString);
        }
        public static T Load<T>(string fileName,JsonType type=JsonType.LitJson)
        {
            string path = Application.streamingAssetsPath+"/"+fileName+".json";
            if(!File.Exists(path))//如果默认数据文件夹里没有,从持久化数据里读取
            {
                path = Application.persistentDataPath+"/"+fileName+".json";
                if(!File.Exists(path))//持久化数据里也没有,抛出异常
                    throw new FileNotFoundException("在streamingAssetsPath和persistentDataPath中均未找到文件", fileName);
            }
            string jsonString = File.ReadAllText(path);
            return type switch
            {
                JsonType.JsonUtility => JsonUtility.FromJson<T>(jsonString),
                JsonType.LitJson => JsonMapper.ToObject<T>(jsonString),
            };
        }
    }
}

