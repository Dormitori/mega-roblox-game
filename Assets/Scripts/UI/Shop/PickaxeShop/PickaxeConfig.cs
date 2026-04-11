using System.Collections.Generic;
using System.Linq;
using Core.Localization;
using UnityEngine;


[CreateAssetMenu(menuName = "Config/Gameplay/Pickaxe Config")]
public class PickaxeConfig : ScriptableObject
{
    public PickaxeType type;
    public Sprite pickaxeIcon;
    public GameObject pickaxePrefab;
    public int baseDamage;
    public float baseSpeedMultiplier;
    public int price;
    public List<Requirements> requirements;

    public string LocalizedName => PickaxeTypeLocalization.GetLocalizedName(type);

    public bool SatisfyRequirements(Inventory inventory)
    {
        if (inventory.GetCurrencyCount(CurrencyType.Coins) < price)
            return false;

        return requirements.All(requirement => inventory.GetBlockCount(requirement.blockType) >= requirement.count);
    }

    public bool TryBuy(Inventory inventory)
    {
        if (!SatisfyRequirements(inventory))
            return false;

        inventory.TryRemoveCurrency(CurrencyType.Coins, price);
        foreach (var requirement in requirements)
            inventory.TryRemoveBlock(requirement.blockType, requirement.count);

        return true;
    }

    public IEnumerable<(BlockConfig, int)> RequirementsBlocks(List<BlockConfig> blockConfigs)
    {
        foreach (var blockConfig in blockConfigs)
        foreach (var requirement in requirements)
            if (requirement.blockType == blockConfig.type)
                yield return (blockConfig, requirement.count);
    }
}


[System.Serializable]
public class Requirements
{
    public BlockType blockType;
    public int count;
}