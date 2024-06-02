using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paltry
{
    /// <summary>
    /// 用于执行Sequence,调用Act方法即可
    /// </summary>
    public class SequenceActor 
    {
        public List<Sequence> Sequences = new List<Sequence>();
        /// <summary>
        /// 执行所有动画序列
        /// </summary>
        /// <param name="delay">延迟执行的时间</param>
        public void Act(float delay = 0)
        {
            if (Sequences[0].IsEasy)//EasySequence,执行顺序按照List的添加顺序
            {
                Sequences[0].StartTime = delay;
                for (int i = 1; i < Sequences.Count; i++)
                {
                    Sequences[i].StartTime = Sequences[i - 1].StartTime
                        + Sequences[i - 1].FinishTime;
                }
            }
            else//普通Sequence
                Sequences.Sort();

            foreach(Sequence sequence in Sequences)
                DelayCall.Call(sequence.StartTime + delay, () => sequence.OnSequence());
        }

    }
}

