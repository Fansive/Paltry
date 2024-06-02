using Paltry.Others;
using System;
using System.Collections.Generic;
using UnityEngine;
using Paltry.Extension;

namespace Paltry
{
    /*>>ʹ��ǰ��<<
     *  1.������Ϸ�������Paltry.MonoAgent
     *  2.������ABMgr
     *>>ʹ�÷���<<
     *  1.������ƵAB������:����Ƶ�ļ���BGM����Ч,���������AB����,�޸����е�BGM_ABName��Sound_ABName,
     *      �����Ӧ��AB����������
     *  2.(��ѡ)���Ŷ����Чʱ���ö���ػ���AudioSource���,AudioSourcePool_InitCount�Ƕ������ǰ������      
     *  3.������Ƶ����:AudioMgr.Instance.Setting = new Setting(){...}
     *      ��һ������ֱ��д��,��ȡ��Ϸ�����ļ�֮��,��������
     *  4.(��ѡ)��ǰ������Ƶ�ļ��Ա���ʹ��ʱ���ش����Ŀ���:AudioMgr.Instance.FindClip()
     *  5.����:����Play��ͷ��API����
     *  6.(��ѡ)��Ƶ����:AudioMgr.Instance.Setting������ֱ�ӿ�����BGM����Ч�Ŀ��ؼ���С
     *      ����Ҫ��������,����ֱ�Ӷ�Ӧ��UI��������
     *  */
    public class AudioMgr : CSharpSingleton<AudioMgr>, IAudioMgr
    {
        #region Configuration Data
        //public const string BGM_ABName = "bgm";
        //public const string Sound_ABName = "sound";
        public const int AudioSourcePool_InitCount = 2;//��ѡ
        #endregion
        private Dictionary<string, AudioClip> BGMCache;
        private Dictionary<string, AudioClip> SoundCache;
        private List<AudioSource> SoundSources;//�����������е�sound,�Ե�������
        private AudioSource BGM;
        private bool isTransiting;
        private GameObject SoundObj;

        #region AudioSetting
        private AudioSetting setting = AudioSetting.Data;
        public bool IsBgmEnabled
        {
            set 
            {
                setting.IsBgmEnabled = value;
                setting.UpdateBgm();
            }
            get => setting.IsBgmEnabled;
        }
        public float BgmVolume
        {
            set
            {
                setting.BgmVolume = value;
                setting.UpdateBgm();
            }
            get => setting.BgmVolume;
        }
        public bool IsSoundEnabled
        {
            set
            {
                setting.IsSoundEnabled = value;
                setting.UpdateSound();
            }
            get => setting.IsSoundEnabled;
        }
        public float SoundVolume
        {
            set
            {
                setting.SoundVolume = value;
                setting.UpdateSound();
            }
            get => setting.SoundVolume;
        }
        public void SaveAudioSetting()
        {
            setting.Save();
        }
        #endregion

        public AudioMgr()
        {
            BGMCache = new Dictionary<string, AudioClip>();
            SoundCache = new Dictionary<string, AudioClip>();
            SoundSources = new List<AudioSource>();
            GameObject audioRoot = new GameObject("AudioRoot");

            GameObject bgmObj= new GameObject("BGM");
            bgmObj.transform.SetParent(audioRoot.transform);
            BGM = bgmObj.AddComponent<AudioSource>();
            BGM.playOnAwake = false;
            BGM.priority = 256;//BGM���ȼ����
            BGM.loop = true;
            isTransiting = false; 

            SoundObj = new GameObject("Sound");
            SoundObj.transform.SetParent(audioRoot.transform);
            Pool<AudioSource>.Instance.Warm(AudioSourcePool_InitCount,
                factory: () =>
                {
                    var source = SoundObj.AddComponent<AudioSource>();
                    SoundSources.Add(source);
                    return source;
                },
                onEnable: (source) => source.enabled = true,
                onRecycle: (source) => source.enabled = false,
                onDestroy: (source) =>
                {
                    SoundSources.Remove(source);
                    GameObject.Destroy(source);
                });
            
            //Note:Don't refer Instance itself in its constructor,which might lead to
            //infinite recursion
            InitAudioSetting();
        }

        /// <summary>
        /// ��ȡ���ص���Ч���ò�Ӧ����AudioSource
        /// </summary>
        private void InitAudioSetting()
        {
            BGM.volume = IsBgmEnabled.ToInt() * BgmVolume;
            foreach (var source in SoundSources)
                source.volume = IsSoundEnabled.ToInt() * SoundVolume;
        }

        /// <summary>
        /// ����ָ��BGM,������ڲ���,��ֱ���滻�ɵ�ǰBGM�����²���
        /// </summary>
        /// <param name="bgmName"></param>
        /// <param name="volume"></param>
        public void PlayBGM(string bgmName)
        {
            BGM.clip = FindClip(isBGM:true, bgmName);
            BGM.Play();
        }
        //[Obsolete("��δ�����Ƶ���뵭��ʱ�������������ľ�̬��������,���Ƽ�ʹ��")]
        //public void PlayBGMTransit(string bgmName,float transitTime=2f)
        //{
        //    float fadeOutTime = transitTime;
        //    if (BGM.clip == null)//�����û��BGM,�Ϳ���ֱ�ӿ�ʼ����
        //    {
        //        BGM.volume = 0;
        //        fadeOutTime = 0;
        //    }

        //    isTransiting = true;
        //    Tweening.DoOnce(fadeOutTime, Curve.SinEase, (t) =>
        //    {
        //        if(Setting.IsBGMEnabled) 
        //            BGM.volume = Mathf.Lerp(Setting.BGMVolume, 0, t);
        //    }, 
        //    () =>
        //    {
        //        BGM.clip = FindClip(isBGM:true, bgmName);
        //        BGM.Play();
        //        Tweening.DoOnce(transitTime, Curve.SinEase, (t) =>
        //        {
        //            if(Setting.IsBGMEnabled) 
        //                BGM.volume = Mathf.Lerp(0, Setting.BGMVolume, t);
        //        },() => isTransiting = false);
        //    });

        //}
        public void PauseBGM()
        {
            BGM.Pause();
        }
        public void UnpauseBGM()
        {
            BGM.UnPause();
        }
        public void StopBGM()
        {
            BGM.Stop();
        }
        public void PlaySound(string soundName)
        {
            AudioSource source = Pool<AudioSource>.Instance.Get();
            var clip = FindClip(isBGM: false, soundName);
            source.volume = IsSoundEnabled.ToInt() * SoundVolume;
            source.PlayOneShot(clip);

            DelayCall.Call(clip.length, RemoveAudioSource);
            void RemoveAudioSource() => Pool<AudioSource>.Instance.Recycle(source);
        }
        /// <summary>
        /// �������ø÷�������ǰ������Ƶ��Դ
        /// </summary>
        public AudioClip FindClip(bool isBGM,string clipName)
        {
            if (isBGM)
            {
                if (BGMCache.ContainsKey(clipName))
                    return BGMCache[clipName];
                else
                {
                    //var clip = ABMgr.Instance.LoadRes<AudioClip>(BGM_ABName, clipName);
                    var clip = AALoader.Instance.LoadAsset<AudioClip>(clipName);
                    BGMCache.Add(clipName, clip);
                    return clip;
                }
            }
            else
            {
                if (SoundCache.ContainsKey(clipName))
                    return SoundCache[clipName];
                else
                {
                    //var clip = ABMgr.Instance.LoadRes<AudioClip>(Sound_ABName, clipName);
                    var clip = AALoader.Instance.LoadAsset<AudioClip>(clipName);
                    SoundCache.Add(clipName, clip);
                    return clip;
                }
            }
        }

        public record AudioSetting() : PersistentDataMap<AudioSetting>(PaltryCongfig.SO.AudioSettingFileName)
        {
            public void UpdateBgm()
            {
                AudioMgr.Instance.BGM.volume = IsBgmEnabled.ToInt() * BgmVolume;
            }
            public void UpdateSound()
            {
                foreach (var source in AudioMgr.Instance.SoundSources)
                    source.volume = IsSoundEnabled.ToInt() * SoundVolume;
            }

            public bool IsBgmEnabled;
            public float BgmVolume;
            public bool IsSoundEnabled;
            public float SoundVolume;
            public override void SetDefaultValue()
            {
                var so = PaltryCongfig.SO;
                IsBgmEnabled = so.IsBgmEnabled;
                BgmVolume = so.BgmVolume;
                IsSoundEnabled = so.IsSoundEnabled;
                SoundVolume = so.SoundVolume;
            }
        }

        /// <summary>
        /// ��Ƶ����,���Ժ�UI����ϵ�ֵ��ȫ��Ӧ��
        /// ��ȡSetting��ֵû���κθ�����,�����õĻ����Զ������������
        /// </summary>
        //public class AudioSetting
        //{
        //    private bool _isBGMEnabled;
        //    public bool IsBGMEnabled
        //    {
        //        get => _isBGMEnabled;
        //        set
        //        {
        //            _isBGMEnabled = value;
        //            if (value && !AudioMgr.Instance.isTransiting)//����͹��ɹ��̳�ͻ
        //                AudioMgr.Instance.BGM.volume = BGMVolume;
        //            else if(!value)
        //                AudioMgr.Instance.BGM.volume = 0;
        //        }
        //    }
        //    private float _bgmVolume;
        //    public float BGMVolume
        //    {
        //        get => _bgmVolume;
        //        set
        //        {
        //            _bgmVolume = value;
        //            if (IsBGMEnabled)
        //            {
        //                AudioMgr.Instance.BGM.volume = value;
        //            }
        //        }
        //    }


        //    private bool _isSoundEnabled;
        //    public bool IsSoundEnabled
        //    {
        //        get => _isSoundEnabled;
        //        set
        //        {
        //            _isSoundEnabled = value;
        //            if (value)
        //                SetAllSound(SoundVolume);
        //            else
        //                SetAllSound(0);
        //        }
        //    }
        //    private float _soundVolume;
        //    public float SoundVolume { get => _soundVolume;
        //        set
        //        {
        //            _soundVolume = value;
        //            if(IsSoundEnabled)
        //                SetAllSound(SoundVolume);
        //        }
        //    }
        //    //�������ֻ�����ö�������volume,����һ�������Ѿ�ȡ������,�ⲿ��������PlaySound��
        //    private void SetAllSound(float volume)
        //    {
        //        foreach(AudioSource source in AudioMgr.Instance.SoundSources)
        //            source.volume = volume;
        //    }
        //}
    }
    
}
