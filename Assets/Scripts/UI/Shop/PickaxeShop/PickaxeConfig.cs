using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(menuName = "Config/Gameplay/Pickaxe Config")]
public class PickaxeConfig : ScriptableObject
{
    public Items item;
    public string pickaxeName;
    public Sprite pickaxeIcon;
    public GameObject pickaxePrefab;
    public int baseDamage;
    public float baseSpeedMultiplier;
    public int price;
    public List<Requirements> requirements;

    public bool SatisfyRequirements(IInventory inventory)
    {
        if (inventory.ItemsCount[Items.Coins] < price)
            return false;

        return requirements.All(requirement => inventory.ItemsCount[requirement.item] >= requirement.count);
    }

    public bool TryBuy(IInventory inventory)
    {
        if (!SatisfyRequirements(inventory))
            return false;

        inventory.TryRemoveItem(Items.Coins, price);
        foreach (var requirement in requirements)
            inventory.TryRemoveItem(requirement.item, requirement.count);

        return true;
    }

    public IEnumerable<(BlockConfig, int)> RequirementsBlocks(List<BlockConfig> blockConfigs)
    {
        foreach (var blockConfig in blockConfigs)
        foreach (var requirement in requirements)
            if (requirement.item == blockConfig.item)
                yield return (blockConfig, requirement.count);
    }
}


[System.Serializable]
public class Requirements
{
    public Items item;
    public int count;
}