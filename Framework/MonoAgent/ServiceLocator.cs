using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Paltry
{
    /// <summary>
    /// ����λ��,�����ڿ���ڲ�,����ģ������,ʵ����Ŀ���ֱ��ʹ�þ���ģ����������ṩ�ĳ���ģ��
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
