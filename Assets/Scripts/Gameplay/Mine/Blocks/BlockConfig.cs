using UnityEngine;

[CreateAssetMenu(menuName = "Config/Gameplay/BlockConfig")]
public class BlockConfig : ScriptableObject
{
    public Items item;
    public Sprite icon;
    public string name;
    public string rarity;
    public int baseSellPrice;
    public float health;
}