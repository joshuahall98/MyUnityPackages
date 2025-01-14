using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Joshua 2023/12/10

[System.Serializable]
public class ObjectPool<T>
{
    public T obj;
    [Range(0, 100), Tooltip("When calculating the liklihood of an object appearing, it adds up all the weights and converts all amounts into perecentages of the max number")]
    public int weight = 100;
}


public static class RandomUtility
{
    /// <summary>
    /// Accepts a list of objects and returns a random value based on a weighting system.
    /// </summary>
    public static T ObjectPoolCalculator<T>(IEnumerable<ObjectPool<T>> list)
    {
        int combinedWeight = 0;

        foreach (var pool in list)
        {
            combinedWeight += pool.weight;
        }

        var random = Random.Range(0, combinedWeight);

        foreach (var pool in list)
        {
            random -= pool.weight;

            if (random <= 0)
            {
                return pool.obj;
            }
        }
        
        return default;
    }


    /// <summary>
    /// Accepts a percentage from 0-100 and returns true or false.
    /// </summary>
    public static bool RandomPercentage(int percent)
    {
        int random = Random.Range(0, 100);

        if(random < percent)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Accepts a list and returns the same list randomly re-ordered.
    /// </summary>
    public static List<T> RandomListSort<T>(List<T> list)
    {
        //return list.OrderBy(t => Random.value).ToList();

        var count = list.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }

        return list;
    } 
}
