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
            if(!File.Exists(path))//���Ĭ�������ļ�����û��,�ӳ־û��������ȡ
            {
                path = Application.persistentDataPath+"/"+fileName+".json";
                if(!File.Exists(path))//�־û�������Ҳû��,�׳��쳣
                    throw new FileNotFoundException("��streamingAssetsPath��persistentDataPath�о�δ�ҵ��ļ�", fileName);
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

