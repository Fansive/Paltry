using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;

namespace Paltry
{
    /// <summary>
    /// UI��������
    /// </summary>
    /// ʹ��ʱ,��ͨ��<see cref="Paltry.Extension.UIExtension.AddSequence(SelectableTrigger, float, SequenceActor)"/>
    /// ��<see cref="Paltry.Extension.UIExtension.AddEasySequence(SelectableTrigger, SequenceActor)"/>
    /// ��չ���������Sequence,ָ������ͨ����Easy,����<see cref="SequenceActor"/>
    /// �����ɺ�,Ҫ����SequenceActor��Act������ʼ���Ŷ���
    /// ��ͨSequence:ָ����ʼʱ��,ִ��ʱ���տ�ʼʱ������ִ��,
    ///     ���Բ��ܽ���ʱ��,����Ҳ����ͬʱִ�ж������
    /// EasySequence:��ָ����ʼʱ��,�����˳������ִ��,�޷�ͬʱִ�ж������,
    ///     ��һ������ִ����ɺ��Զ�ִ����һ��(ȡ����������ʱ����Ķ���)
    /// ������Sequence֮���˳��,Sequence�ڲ�Ҳ��ָ��˳��,Ĭ�����ж���ͬʱ��ʼ,��ͨ��delay������ָ����ʼʱ��    
    public class Sequence : UIComponentGetter,IComparable<Sequence>
    {
        public Action OnSequence;
        public float StartTime;
        private float _finishTime;
        /// <summary>��Sequence������ɵ�ʱ��,��ȡ�������һ�����������ʱ��</summary>
        public float FinishTime
        {
            get => _finishTime;
            set
            {
                _finishTime = Mathf.Max(_finishTime, value);
            }
        }
        public bool IsEasy;
        public void InitComponent(float startTime)
        {
            StartTime = startTime;
        }
        public void InitEasyComponent()
        {
            IsEasy = true;
        }
        public int CompareTo(Sequence other)
        {
            return this.StartTime.CompareTo(other.StartTime);
        }


        public Sequence ApplyPreset(SequencePresets preset)
        {
            Custom_Presets.GetSequencePreset(preset).Invoke(this);
            return this;
        }
        /// <summary>
        /// �ƶ�UI,ע��˴����ƶ���ͨ��Pivot,�е�ʱ����ܲ���ﵽԤ��Ч��
        /// </summary>
        /// <param name="absolutePosition">��canvas���������,��(0,0)��(1,1),���½�Ϊԭ��</param>
        public Sequence Sequence_Move(Vector2 absolutePosition, float time, Ease ease,float delay=0)
        {//�ƶ������겻��
            OnSequence += () => Rect.MoveUI_Modified(absolutePosition, UIMgr.Instance.Canvas, time)
                .SetEase(ease).SetDelay(delay);

            FinishTime = time + delay;
            return this;
        }
        public Sequence Sequence_MoveInWorld(Vector2 destination, float time, Ease ease, float delay = 0)
        {//�ƶ������겻��
            OnSequence += () => Rect.Move(destination, time).SetEase(ease).SetDelay(delay);

            FinishTime = time + delay;
            return this;
        }
        /// <summary>
        /// ������������ڶ�����ʼʱ�ı���,����ԭ��scaleΪ3,������2�Ļ�����scale����6
        /// </summary>
        public Sequence Sequence_Scale(float scale, float time, Ease ease,float delay = 0)
        {
            OnSequence += () => Rect.ScaleTween(Rect.localScale * scale, time)
                .SetEase(ease).SetDelay(delay);

            FinishTime = time + delay;
            return this;
        }
        public Sequence Sequence_Sound(string UISoundName,float delay = 0)
        {
            OnSequence += () =>
                DelayCall.Call(delay, () => ServiceLocator.IAudioMgr.PlaySound(UISoundName));

            FinishTime = delay;
            return this;
        }

        public Sequence Sequence_ImageFadeIn(float time,Ease ease,float delay=0)
        {
            OnSequence += () => Image.FadeIn(time).SetEase(ease).SetDelay(delay);

            FinishTime = time + delay;
            return this;
        }
        public Sequence Sequence_ImageFadeOut(float time,Ease ease,float delay=0)
        {
            OnSequence += () => Image.FadeOut(time).SetEase(ease).SetDelay(delay);

            FinishTime = time + delay;
            return this;
        }
        
        public Sequence Sequence_TextFadeIn(float time,Ease ease,float delay = 0)
        {
            OnSequence += () => TMPText.FadeIn(time).SetEase(ease).SetDelay(delay);

            FinishTime = time + delay;
            return this;
        }
        public Sequence Sequence_TextFadeOut(float time, Ease ease, float delay = 0)
        {
            OnSequence += () => TMPText.FadeOut(time).SetEase(ease).SetDelay(delay);

            FinishTime = time + delay;
            return this;
        } 
        /// <summary>
        /// ����Externǰ׺����Ҫ�ֶ�����Text���
        /// </summary>
        public Sequence Sequence_ExternTextFadeIn(TMP_Text text,float time,Ease ease,float delay = 0)
        {
            OnSequence += () => text.FadeIn(time).SetEase(ease).SetDelay(delay);

            FinishTime = time + delay;
            return this;
        }
        public Sequence Sequence_ExternTextFadeOut(TMP_Text text,float time, Ease ease, float delay = 0)
        {
            OnSequence += () => text.FadeOut(time).SetEase(ease).SetDelay(delay);

            FinishTime = time + delay;
            return this;
        }
    }
}
  
