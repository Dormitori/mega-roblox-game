using UnityEngine;

[CreateAssetMenu(menuName = "Config/Gameplay/BlockConfig")]
public class BlockConfig : ScriptableObject
{
    public string resourceName;
    public float health;
}