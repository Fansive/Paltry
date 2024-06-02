using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Paltry.Others
{
    public class PaltryCongfig : ScriptableObjectSingleton<PaltryCongfig>
    {
        protected override string LoadPath => "Assets/Paltry/Cofig";
        protected override SoLoadWay LoadWay => SoLoadWay.EditorAndRuntime;
        [Header("DataTable")]
        public bool DT_Enable;
        public string DT_ExcelRootDirectory;
        public string DT_CodeDirectory;
        public string DT_ClasssName;
        public string DT_Namespace;
        public string[] DT_Usings;
        public string DT_Usings_Str
        {
            get
            {
                var usings = DT_Usings.Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select(u => $"using {u};\n");
                if (usings != null)
                    return string.Join("", usings);
                else return null;
            }
        }
            
        [Header("ControlUtil")]
        [Tooltip("改键绑定设置配置文件的文件名,它将保存在持久化目录中,在加载改键设置时从此处加载")]
        public string ControlBindingSaveName;

        [Header("AudioSetting")]
        public string AudioSettingFileName;
        public bool IsBgmEnabled;
        [Range(0,100)]
        public float BgmVolume;
        public bool IsSoundEnabled;
        [Range(0,100)]
        public float SoundVolume;


    }

}
