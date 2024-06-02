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
    /// UI动画序列
    /// </summary>
    /// 使用时,需通过<see cref="Paltry.Extension.UIExtension.AddSequence(SelectableTrigger, float, SequenceActor)"/>
    /// 或<see cref="Paltry.Extension.UIExtension.AddEasySequence(SelectableTrigger, SequenceActor)"/>
    /// 扩展方法来添加Sequence,指定是普通还是Easy,并绑定<see cref="SequenceActor"/>
    /// 添加完成后,要调用SequenceActor的Act方法开始播放动画
    /// 普通Sequence:指定开始时间,执行时按照开始时间依次执行,
    ///     可以不管结束时间,并且也可以同时执行多个序列
    /// EasySequence:不指定开始时间,按添加顺序依次执行,无法同时执行多个动画,
    ///     上一个序列执行完成后自动执行下一个(取决于序列里时间最长的动画)
    /// 以上是Sequence之间的顺序,Sequence内部也可指定顺序,默认所有动画同时开始,而通过delay参数可指定开始时间    
    public class Sequence : UIComponentGetter,IComparable<Sequence>
    {
        public Action OnSequence;
        public float StartTime;
        private float _finishTime;
        /// <summary>该Sequence最终完成的时间,它取决于最后一个动画的完成时间</summary>
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
        /// 移动UI,注意此处的移动是通过Pivot,有的时候可能不会达到预期效果
        /// </summary>
        /// <param name="absolutePosition">在canvas的相对坐标,从(0,0)到(1,1),左下角为原点</param>
        public Sequence Sequence_Move(Vector2 absolutePosition, float time, Ease ease,float delay=0)
        {//移动的坐标不对
            OnSequence += () => Rect.MoveUI_Modified(absolutePosition, UIMgr.Instance.Canvas, time)
                .SetEase(ease).SetDelay(delay);

            FinishTime = time + delay;
            return this;
        }
        public Sequence Sequence_MoveInWorld(Vector2 destination, float time, Ease ease, float delay = 0)
        {//移动的坐标不对
            OnSequence += () => Rect.Move(destination, time).SetEase(ease).SetDelay(delay);

            FinishTime = time + delay;
            return this;
        }
        /// <summary>
        /// 该缩放是相对于动画开始时的倍数,例如原先scale为3,这里填2的话最终scale就是6
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
        /// 带有Extern前缀的需要手动传入Text组件
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
  
