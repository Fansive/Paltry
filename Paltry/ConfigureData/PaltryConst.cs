using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Paltry
{
    /*在此处进行Paltry的一些常数配置*/

    /// <summary>
    /// 在该枚举下加入需要作为对象池物体的名字
    /// 仅限GameObject,其他类型的直接用Pool<T>即可
    /// </summary>
    public enum GOType
    {
        bullet
    }
    public struct UIName
    {
        public const string LoginPanel = nameof(LoginPanel);
        public const string SelectPanel = nameof(SelectPanel);
    }
    public partial struct EventName
    {
        //事件名字符串可以这样定义(这个字符串仅作演示,可以删掉)
        public const string test = nameof(test);

    }
    public class PaltryConst
    {
        /// <summary>
        /// AB包/AB包资源加载时,进度更新间隔
        /// </summary>
        public static YieldInstruction LoadInfoUpdateInternal = null;


    }

}
