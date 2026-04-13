using UnityEngine;

public interface IPlayerControls
{
    bool JumpedThisFrame();
    bool JumpIsPressed();
    Vector2 GetMoveDirection();
    bool MineIsPressed();
}