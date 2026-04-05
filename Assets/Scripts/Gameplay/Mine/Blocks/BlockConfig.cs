using UnityEngine;

[CreateAssetMenu(menuName = "Config/Gameplay/BlockConfig")]
public class BlockConfig : ScriptableObject
{
    public BlockType type;
    public Sprite icon;
    public string name;
    public string rarity;
    public int baseSellPrice;
    public float health;
}