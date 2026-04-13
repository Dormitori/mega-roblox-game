using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Gameplay/Backpack Config")]
public class BackpackShopConfig : ScriptableObject
{
    public List<BackpackUpgradeStep> upgradeSteps;

    public float tailPriceGrowth = 1.45f;
}

[Serializable]
public class BackpackUpgradeStep
{
    public int price;
    public int capacity;
}