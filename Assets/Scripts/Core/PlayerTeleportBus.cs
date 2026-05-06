using System;
using UnityEngine;

/// <summary>
/// Единая точка оповещения о телепорте игрока (respawn/return/home и т.п.).
/// </summary>
public static class PlayerTeleportBus
{
    public static event Action<Vector3, Quaternion> Teleported;

    public static void Raise(Vector3 position, Quaternion rotation)
    {
        Teleported?.Invoke(position, rotation);
    }
}

