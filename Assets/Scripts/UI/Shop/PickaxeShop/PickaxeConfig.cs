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
}