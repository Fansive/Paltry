using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Paltry
{
    /// <summary>
    /// 自定义的Preset,可自行添加/删除Preset
    /// 添加方法:在<see cref="Custom_Presets.GetTriggerPreset(TriggerPresets)"/>
    /// 或<see cref="Custom_Presets.GetSequencePreset(SequencePresets)"/>
    /// 中的switch表达式内添加所需Preset,并在该类里实现对应函数,添加对应枚举
    /// </summary>
    public static class Custom_Presets 
    {
        /// <summary>
        /// 在此处注册可以获取到的TriggerPreset
        /// </summary>
        public static Action<SelectableTrigger> GetTriggerPreset(TriggerPresets preset)
        {
            return preset switch
            {
                TriggerPresets.Example => Example,

                _ => throw new KeyNotFoundException("不存在该Preset:"+preset),
            };
        }
        /// <summary>
        /// 在此处注册可以获取到的TriggerPreset
        /// </summary>
        public static Action<Sequence> GetSequencePreset(SequencePresets preset)
        {
            return preset switch
            {
                SequencePresets.Example => Example,

                _ => throw new KeyNotFoundException("不存在该Preset:" + preset),
            };
        }

        private static void Example(SelectableTrigger trigger)
        {
            trigger.AddEnableTween_Move(new Vector2(0.5f, 0), 1f, Ease.EaseInExpo)
                .AddEnterTween_Scale(1.3f, .5f, Ease.EaseInCirc)
                .SetStayTween_ImageBlink(1f, Ease.EaseOutQuad, .3f);
        }
        private static void Example(Sequence sequence)
        {
            sequence.Sequence_Move(new Vector2(.5f, 0), 1f, Ease.EaseInExpo)
                .Sequence_Scale(1.3f, .5f, Ease.EaseInExpo)
                .Sequence_ImageFadeIn(1f, Ease.EaseInCubic, 1f);
        }
    }

    
    public enum TriggerPresets
    {
        Example
    }
    public enum SequencePresets
    {
        Example
    }
}

