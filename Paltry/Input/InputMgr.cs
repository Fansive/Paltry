using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Paltry
{
    /*>>使用前提<<
     *  1.任意游戏物体挂载Paltry.MonoAgent,
     *      且确保在MonoAgent的Update(或其他Update)里调用了InputMgr.Instance.Update();
     *  2.导入了EventCenter      
     *>>使用方法<<
     *  1.配置感兴趣的按键:在构造函数里,KeyStates的初始化中填入自己感兴趣的按键
     *  2.监听按键事件:EventCenter.Instance.AddListener(EventName.Input...)
     *      输入事件名在该脚本最底端已提前定义好,请不要硬编码字符串作为参数
     *      可以监听按键的按下,正在按,抬起,双击,长按事件,双击和长按的阈值可以在类里面配置
     *  3.按键检测开关:InputMgr.Instance.SetKeyCheck()      
     *  */
    /// <summary>
    /// 使用方法:在Init方法里初始化KeyStates,填入项目中感兴趣的键,此处触发按键检测事件
    /// 不建议在测试功能时需要检测按键的地方用这个,还是用Input.GetKeyDown,因为这里的按键需要提前注册
    ///<para>需要用到的地方监听这些按键就行(通过EventName.Input获取到事件字符串)</para> 
    /// </summary>
    public class InputMgr:CSharpSingleton<InputMgr>
    {
        private KeyState[] KeyStates;
        private float RealTimeNow;
        #region Configuration Data
        struct InputThreshold//输入检测的阈值
        {
            public const float DoubleClick = 0.4f;
            public const float LongPress = 0.4f;
        }
        public InputMgr()
        {
            KeyStates = new KeyState[]//在此处填入需要检测的键,如此处是WSAD
            {
                new KeyState(KeyCode.W),
                new KeyState(KeyCode.S),
                new KeyState(KeyCode.A),
                new KeyState(KeyCode.D),
            };
            Array.Sort(KeyStates);//便于SetKeyCheck中查找
        }
        #endregion
        struct KeyState : IComparable<KeyState>
        {
            public readonly KeyCode keyCode;
            public bool isDisabled;
            public bool isSecondClick;
            public float lastDownTime;//上次按下(KeyDown)的时间
            public float lastUpTime;//上次抬起(KeyUp)的时间
            public KeyState(KeyCode keyCode):this()
            {
                this.keyCode = keyCode;
            }
            public int CompareTo(KeyState other)
            {
                if (this.keyCode > other.keyCode)
                    return 1;
                else if (this.keyCode < other.keyCode)
                    return -1;
                else
                    return 0;
            }
        }
        private void UpdateKeyState(ref KeyState key)
        {
            if(key.isDisabled)
                return;

            if (Input.GetKeyDown(key.keyCode))
            {
                EventCenter.Instance.Trigger("Input.KeyDown." + key.keyCode.ToString());
                float delta = RealTimeNow - key.lastUpTime;
                if (key.isSecondClick && delta < InputThreshold.DoubleClick)
                {
                    EventCenter.Instance.Trigger("Input.KeyDoubleClick." + key.keyCode.ToString());
                    key.isSecondClick = false;
                }
                else
                {
                    key.isSecondClick = true;
                }
                key.lastDownTime = RealTimeNow;
            }
            if (Input.GetKey(key.keyCode))
            {
                EventCenter.Instance.Trigger("Input.KeyOn." + key.keyCode.ToString());
                float delta = RealTimeNow - key.lastDownTime;
                if (delta > InputThreshold.LongPress)
                    EventCenter.Instance.Trigger<float>("Input.KeyLongPress." + key.keyCode.ToString()
                        , delta);
            }
            if (Input.GetKeyUp(key.keyCode))
            {
                EventCenter.Instance.Trigger<float>("Input.KeyUp." + key.keyCode.ToString()
                    , RealTimeNow - key.lastDownTime);
                key.lastUpTime = RealTimeNow;
            }
        }
        public void Update()
        {
            RealTimeNow = Time.realtimeSinceStartup;
            for (int i = 0; i < KeyStates.Length; i++)
            {
                UpdateKeyState(ref KeyStates[i]);
            }
        }
        /// <summary>
        /// 启用/禁用按键检测状态
        /// </summary>
        /// <param name="keysToSet"></param>
        /// <param name="isEnabled"></param>
        public void SetKeyCheck(bool isEnabled,params KeyCode[] keysToSet)
        {//确保两个序列都是有序的,然后顺序查找需要设置状态的键
            Array.Sort(keysToSet);
            for (int i = 0,j=0; j < keysToSet.Length; i++)
            {
                if (KeyStates[i].keyCode == keysToSet[j])
                {
                    KeyStates[i].isDisabled = !isEnabled;
                    j++;
                }
            }
        }
    }
    public partial struct EventName
    {
        public struct Input
        {
            public struct Down
            {
                public const string A = "Input.KeyDown.A";
                public const string B = "Input.KeyDown.B";
                public const string C = "Input.KeyDown.C";
                public const string D = "Input.KeyDown.D";
                public const string E = "Input.KeyDown.E";
                public const string F = "Input.KeyDown.F";
                public const string G = "Input.KeyDown.G";
                public const string H = "Input.KeyDown.H";
                public const string I = "Input.KeyDown.I";
                public const string J = "Input.KeyDown.J";
                public const string K = "Input.KeyDown.K";
                public const string L = "Input.KeyDown.L";
                public const string M = "Input.KeyDown.M";
                public const string N = "Input.KeyDown.N";
                public const string O = "Input.KeyDown.O";
                public const string P = "Input.KeyDown.P";
                public const string Q = "Input.KeyDown.Q";
                public const string R = "Input.KeyDown.R";
                public const string S = "Input.KeyDown.S";
                public const string T = "Input.KeyDown.T";
                public const string U = "Input.KeyDown.U";
                public const string V = "Input.KeyDown.V";
                public const string W = "Input.KeyDown.W";
                public const string X = "Input.KeyDown.X";
                public const string Y = "Input.KeyDown.Y";
                public const string Z = "Input.KeyDown.Z";
            }
            public struct On
            {
                public const string A = "Input.KeyOn.A";
                public const string B = "Input.KeyOn.B";
                public const string C = "Input.KeyOn.C";
                public const string D = "Input.KeyOn.D";
                public const string E = "Input.KeyOn.E";
                public const string F = "Input.KeyOn.F";
                public const string G = "Input.KeyOn.G";
                public const string H = "Input.KeyOn.H";
                public const string I = "Input.KeyOn.I";
                public const string J = "Input.KeyOn.J";
                public const string K = "Input.KeyOn.K";
                public const string L = "Input.KeyOn.L";
                public const string M = "Input.KeyOn.M";
                public const string N = "Input.KeyOn.N";
                public const string O = "Input.KeyOn.O";
                public const string P = "Input.KeyOn.P";
                public const string Q = "Input.KeyOn.Q";
                public const string R = "Input.KeyOn.R";
                public const string S = "Input.KeyOn.S";
                public const string T = "Input.KeyOn.T";
                public const string U = "Input.KeyOn.U";
                public const string V = "Input.KeyOn.V";
                public const string W = "Input.KeyOn.W";
                public const string X = "Input.KeyOn.X";
                public const string Y = "Input.KeyOn.Y";
                public const string Z = "Input.KeyOn.Z";
            }
            public struct Up
            {
                public const string A = "Input.KeyUp.A";
                public const string B = "Input.KeyUp.B";
                public const string C = "Input.KeyUp.C";
                public const string D = "Input.KeyUp.D";
                public const string E = "Input.KeyUp.E";
                public const string F = "Input.KeyUp.F";
                public const string G = "Input.KeyUp.G";
                public const string H = "Input.KeyUp.H";
                public const string I = "Input.KeyUp.I";
                public const string J = "Input.KeyUp.J";
                public const string K = "Input.KeyUp.K";
                public const string L = "Input.KeyUp.L";
                public const string M = "Input.KeyUp.M";
                public const string N = "Input.KeyUp.N";
                public const string O = "Input.KeyUp.O";
                public const string P = "Input.KeyUp.P";
                public const string Q = "Input.KeyUp.Q";
                public const string R = "Input.KeyUp.R";
                public const string S = "Input.KeyUp.S";
                public const string T = "Input.KeyUp.T";
                public const string U = "Input.KeyUp.U";
                public const string V = "Input.KeyUp.V";
                public const string W = "Input.KeyUp.W";
                public const string X = "Input.KeyUp.X";
                public const string Y = "Input.KeyUp.Y";
                public const string Z = "Input.KeyUp.Z";
            }
            public struct DoubleClick
            {
                public const string A = "Input.KeyDoubleClick.A";
                public const string B = "Input.KeyDoubleClick.B";
                public const string C = "Input.KeyDoubleClick.C";
                public const string D = "Input.KeyDoubleClick.D";
                public const string E = "Input.KeyDoubleClick.E";
                public const string F = "Input.KeyDoubleClick.F";
                public const string G = "Input.KeyDoubleClick.G";
                public const string H = "Input.KeyDoubleClick.H";
                public const string I = "Input.KeyDoubleClick.I";
                public const string J = "Input.KeyDoubleClick.J";
                public const string K = "Input.KeyDoubleClick.K";
                public const string L = "Input.KeyDoubleClick.L";
                public const string M = "Input.KeyDoubleClick.M";
                public const string N = "Input.KeyDoubleClick.N";
                public const string O = "Input.KeyDoubleClick.O";
                public const string P = "Input.KeyDoubleClick.P";
                public const string Q = "Input.KeyDoubleClick.Q";
                public const string R = "Input.KeyDoubleClick.R";
                public const string S = "Input.KeyDoubleClick.S";
                public const string T = "Input.KeyDoubleClick.T";
                public const string U = "Input.KeyDoubleClick.U";
                public const string V = "Input.KeyDoubleClick.V";
                public const string W = "Input.KeyDoubleClick.W";
                public const string X = "Input.KeyDoubleClick.X";
                public const string Y = "Input.KeyDoubleClick.Y";
                public const string Z = "Input.KeyDoubleClick.Z";

            }
            public struct LongPress
            {
                public const string A = "Input.KeyLongPress.A";
                public const string B = "Input.KeyLongPress.B";
                public const string C = "Input.KeyLongPress.C";
                public const string D = "Input.KeyLongPress.D";
                public const string E = "Input.KeyLongPress.E";
                public const string F = "Input.KeyLongPress.F";
                public const string G = "Input.KeyLongPress.G";
                public const string H = "Input.KeyLongPress.H";
                public const string I = "Input.KeyLongPress.I";
                public const string J = "Input.KeyLongPress.J";
                public const string K = "Input.KeyLongPress.K";
                public const string L = "Input.KeyLongPress.L";
                public const string M = "Input.KeyLongPress.M";
                public const string N = "Input.KeyLongPress.N";
                public const string O = "Input.KeyLongPress.O";
                public const string P = "Input.KeyLongPress.P";
                public const string Q = "Input.KeyLongPress.Q";
                public const string R = "Input.KeyLongPress.R";
                public const string S = "Input.KeyLongPress.S";
                public const string T = "Input.KeyLongPress.T";
                public const string U = "Input.KeyLongPress.U";
                public const string V = "Input.KeyLongPress.V";
                public const string W = "Input.KeyLongPress.W";
                public const string X = "Input.KeyLongPress.X";
                public const string Y = "Input.KeyLongPress.Y";
                public const string Z = "Input.KeyLongPress.Z";

            }
        }
    }
    
}
