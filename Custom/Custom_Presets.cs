using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Paltry
{
    /// <summary>
    /// �Զ����Preset,���������/ɾ��Preset
    /// ��ӷ���:��<see cref="Custom_Presets.GetTriggerPreset(TriggerPresets)"/>
    /// ��<see cref="Custom_Presets.GetSequencePreset(SequencePresets)"/>
    /// �е�switch���ʽ���������Preset,���ڸ�����ʵ�ֶ�Ӧ����,��Ӷ�Ӧö��
    /// </summary>
    public static class Custom_Presets 
    {
        /// <summary>
        /// �ڴ˴�ע����Ի�ȡ����TriggerPreset
        /// </summary>
        public static Action<SelectableTrigger> GetTriggerPreset(TriggerPresets preset)
        {
            return preset switch
            {
                TriggerPresets.Example => Example,

                _ => throw new KeyNotFoundException("�����ڸ�Preset:"+preset),
            };
        }
        /// <summary>
        /// �ڴ˴�ע����Ի�ȡ����TriggerPreset
        /// </summary>
        public static Action<Sequence> GetSequencePreset(SequencePresets preset)
        {
            return preset switch
            {
                SequencePresets.Example => Example,

                _ => throw new KeyNotFoundException("�����ڸ�Preset:" + preset),
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

