using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Paltry
{
    /// <summary>
    /// 服务定位器,仅用于框架内部,减少模块间耦合,实际项目里可直接使用具体模块而非这里提供的抽象模块
    /// </summary>
    public static class ServiceLocator 
    {
        public static IAudioMgr IAudioMgr => AudioMgr.Instance;
    }

    public interface IAudioMgr
    {
        public void PlayBGM(string bgmName);
        public void PauseBGM();
        public void StopBGM();
        public void PlaySound(string soundName);
    }
}
