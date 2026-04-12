/// <summary>
/// Сундуки и коробка: логика лута и инвентаря.
/// </summary>
public static class MineChestRules
{
    public static bool GrantsCrystalsOnly(BlockType type) => type == BlockType.WoodenBox;

    public static bool IgnoresBackpackCapacity(BlockType type) => type == BlockType.WoodenBox;
}
