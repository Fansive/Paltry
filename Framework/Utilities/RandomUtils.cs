using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class RandomUtils 
{
    /// <summary>
    /// ϴ���㷨,��[start,end]�ڲ���һϵ�д��ҵ�����,endӦ����start
    /// </summary>
    /// <param name="start">��һ����</param>
    /// <param name="end">���һ����</param>
    /// <returns></returns>
    public static int[] GetShuffleNumbers(int start,int end)
    {
        int length = end - start + 1;
        int[] result = new int[length];
        for (int i = 0; i < length; i++)
            result[i] = i + start;
        for (int i = 0; i < length; i++)
        {
            int rand = Random.Range(i,length);
            (result[i], result[rand]) = (result[rand], result[i]);
        }
        return result;
    }
    /// <summary>
    /// ϴ���㷨,��������������,�����ⴴ������
    /// </summary>
    public static void ShuffleNumbers(int[] nums)
    {
        for (int i = 0; i < nums.Length; i++)
        {
            int rand = Random.Range(i, nums.Length);
            (nums[i], nums[rand]) = (nums[rand], nums[i]);
        }
    }
}
