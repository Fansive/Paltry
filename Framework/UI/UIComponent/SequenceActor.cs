using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paltry
{
    /// <summary>
    /// ����ִ��Sequence,����Act��������
    /// </summary>
    public class SequenceActor 
    {
        public List<Sequence> Sequences = new List<Sequence>();
        /// <summary>
        /// ִ�����ж�������
        /// </summary>
        /// <param name="delay">�ӳ�ִ�е�ʱ��</param>
        public void Act(float delay = 0)
        {
            if (Sequences[0].IsEasy)//EasySequence,ִ��˳����List�����˳��
            {
                Sequences[0].StartTime = delay;
                for (int i = 1; i < Sequences.Count; i++)
                {
                    Sequences[i].StartTime = Sequences[i - 1].StartTime
                        + Sequences[i - 1].FinishTime;
                }
            }
            else//��ͨSequence
                Sequences.Sort();

            foreach(Sequence sequence in Sequences)
                DelayCall.Call(sequence.StartTime + delay, () => sequence.OnSequence());
        }

    }
}

