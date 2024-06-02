using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class RandomUtils 
{
    /// <summary>
    /// 洗牌算法,在[start,end]内产生一系列打乱的数字,end应大于start
    /// </summary>
    /// <param name="start">第一个数</param>
    /// <param name="end">最后一个数</param>
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
    /// 洗牌算法,将传入的数组打乱,不额外创建数组
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
