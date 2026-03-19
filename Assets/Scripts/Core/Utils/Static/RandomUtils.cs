using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RandomUtils
{
    public static T WeightedRandom<T>(List<int> probabilitiesList, List<T> itemsList)
    {
        var whole = probabilitiesList.Sum();
        var lengthList = probabilitiesList.Select(x => (float)x / whole).ToList();
        var randomLessOne = Random.value;
        float curLength = 0;
        for (var i = 0; i < itemsList.Count; i++)
        {
            curLength += lengthList[i];
            if (randomLessOne < curLength)
                return itemsList[i];
        }
        return itemsList[^1];
    }
}