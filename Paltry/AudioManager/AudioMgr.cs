using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Paltry.Extension;
namespace Paltry
{
    /*>>使用前提<<
     *  1.任意游戏物体挂载Paltry.MonoAgent
     *  2.导入了ABMgr
     *>>使用方法<<
     *  1.配置音频AB包包名:将音频文件分BGM和音效,打包到两个AB包中,修改类中的BGM_ABName和Sound_ABName,
     *      填入对应的AB包包名即可
     *  2.(可选)播放多个音效时利用对象池缓存AudioSource组件,AudioSourcePool_InitCount是对象池提前缓存数      
     *  3.配置音频设置:AudioMgr.Instance.Setting = new Setting(){...}
     *      这一步可以直接写在,读取游戏配置文件之后,将其设置
     *  4.(可选)提前加载音频文件以避免使用时加载带来的卡顿:AudioMgr.Instance.FindClip()
     *  5.播放:调用Play开头的API即可
     *  6.(可选)音频控制:AudioMgr.Instance.Setting的数据直接控制着BGM和音效的开关即大小
     *      不需要再做处理,可以直接对应上UI面板的数据
     *  */
    /// <summary>
    /// 使用之前需要手动设置一次Setting
    /// 资源从AB包加载,BGM和Sound(音效)分别从两个AB包中加载
    /// </summary>
    public class AudioMgr:CSharpSingleton<AudioMgr>
    {
        #region Configuration Data
        public const string BGM_ABName = "bgm";
        public const string Sound_ABName = "sound";
        public const int AudioSourcePool_InitCount = 2;//可选
        #endregion
        private Dictionary<string, AudioClip> BGMCache;
        private Dictionary<string, AudioClip> SoundCache;
        private List<AudioSource> SoundSources;//用来跟踪所有的sound,以调整音量
        private AudioSource BGM;
        private bool isTransiting;
        private GameObject SoundObj;
        public AudioSetting Setting { get; set; }
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
            BGM.priority = 256;//BGM优先级最低
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

        }
        /// <summary>
        /// 播放指定BGM,如果正在播放,会直接替换成当前BGM并重新播放
        /// </summary>
        /// <param name="bgmName"></param>
        /// <param name="volume"></param>
        public void PlayBGM(string bgmName)
        {
            BGM.clip = FindClip(isBGM:true, bgmName);
            BGM.Play();
        }
        [Obsolete("尚未解决音频淡入淡出时调节音量产生的竞态条件问题,不推荐使用")]
        public void PlayBGMTransit(string bgmName,float transitTime=2f)
        {
            float fadeOutTime = transitTime;
            if (BGM.clip == null)//如果还没有BGM,就可以直接开始播放
            {
                BGM.volume = 0;
                fadeOutTime = 0;
            }

            isTransiting = true;
            Tweening.DoOnce(fadeOutTime, Curve.SinEase, (t) =>
            {
                if(Setting.IsBGMEnabled) 
                    BGM.volume = Mathf.Lerp(Setting.BGMVolume, 0, t);
            }, 
            () =>
            {
                BGM.clip = FindClip(isBGM:true, bgmName);
                BGM.Play();
                Tweening.DoOnce(transitTime, Curve.SinEase, (t) =>
                {
                    if(Setting.IsBGMEnabled) 
                        BGM.volume = Mathf.Lerp(0, Setting.BGMVolume, t);
                },() => isTransiting = false);
            });

        }
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
            source.volume = Setting.IsSoundEnabled ? Setting.SoundVolume :0;
            source.PlayOneShot(clip);

            Timer.DelayInvoke(clip.length, RemoveAudioSource);
            void RemoveAudioSource() => Pool<AudioSource>.Instance.Recycle(source);
        }
        /// <summary>
        /// 可以利用该方法来提前加载音频资源
        /// </summary>
        public AudioClip FindClip(bool isBGM,string clipName)
        {
            if (isBGM)
            {
                if (BGMCache.ContainsKey(clipName))
                    return BGMCache[clipName];
                else
                {
                    var clip = ABMgr.Instance.LoadRes<AudioClip>(BGM_ABName, clipName);
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
                    var clip = ABMgr.Instance.LoadRes<AudioClip>(Sound_ABName, clipName);
                    SoundCache.Add(clipName, clip);
                    return clip;
                }
            }
        }

        /// <summary>
        /// 音频配置,可以和UI面板上的值完全对应上
        /// 读取Setting的值没有任何副作用,但设置的话会自动更新相关数据
        /// </summary>
        public class AudioSetting
        {
            private bool _isBGMEnabled;
            public bool IsBGMEnabled
            {
                get => _isBGMEnabled;
                set
                {
                    _isBGMEnabled = value;
                    if (value && !AudioMgr.Instance.isTransiting)//避免和过渡过程冲突
                        AudioMgr.Instance.BGM.volume = BGMVolume;
                    else if(!value)
                        AudioMgr.Instance.BGM.volume = 0;
                }
            }
            private float _bgmVolume;
            public float BGMVolume
            {
                get => _bgmVolume;
                set
                {
                    _bgmVolume = value;
                    if (IsBGMEnabled)
                    {
                        AudioMgr.Instance.BGM.volume = value;
                    }
                }
            }


            private bool _isSoundEnabled;
            public bool IsSoundEnabled
            {
                get => _isSoundEnabled;
                set
                {
                    _isSoundEnabled = value;
                    if (value)
                        SetAllSound(SoundVolume);
                    else
                        SetAllSound(0);
                }
            }
            private float _soundVolume;
            public float SoundVolume { get => _soundVolume;
                set
                {
                    _soundVolume = value;
                    if(IsSoundEnabled)
                        SetAllSound(SoundVolume);
                }
            }
            //这个方法只能设置对象池里的volume,还有一部分是已经取出来的,这部分设置在PlaySound里
            private void SetAllSound(float volume)
            {
                foreach(AudioSource source in AudioMgr.Instance.SoundSources)
                    source.volume = volume;
            }
        }
    }
    
}
