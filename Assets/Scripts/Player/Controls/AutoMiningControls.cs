using System.Collections;
using UnityEngine;

public class AutoMiningControls : MonoBehaviour, IPlayerControls
{
    public MineBlocks playerMining;
    public MineManager mineManager;
    public Transform playerTransform;
    
    public AutoMiningControlsState state;

    private int _currentTargetBlock;
    private bool _mineCurrentFrame;

    private void Awake()
    {
        state = AutoMiningControlsState.SearchNextTarget;
    }

    private void Update()
    {
        if (playerMining.CurrentBlock is not null)
        {
            state = AutoMiningControlsState.MiningBlock;
        }
        else
        {
            state = AutoMiningControlsState.SearchNextTarget;
        }
    }

    public bool JumpedThisFrame()
    {
        return false;
    }

    public bool JumpIsPressed()
    {
        return false;
    }

    public Vector2 GetMoveDirection()
    {
        if (state == AutoMiningControlsState.SearchNextTarget)
        {
            var playerPos = new Vector2(playerTransform.position.x, playerTransform.position.z);
            var targetVec = (mineManager.BlocksGridPositions[_currentTargetBlock] - playerPos);
            if (targetVec.magnitude < 0.1f)
                UpdateNextTargetBlock();
            return targetVec.normalized;
        }
        
        return Vector2.zero;
    }

    public bool MineIsPressed()
    {
        if (state == AutoMiningControlsState.MiningBlock)
        {
            if (!_mineCurrentFrame)
                StartCoroutine(MineWithCoolDown());
            else
            {
                _mineCurrentFrame = false;
                return true;
            }
        }

        return false;
    }

    private IEnumerator MineWithCoolDown()
    {
        yield return new WaitForSeconds(1f);
        _mineCurrentFrame = true;
    }

    private void UpdateNextTargetBlock()
    {
        _currentTargetBlock = (_currentTargetBlock + 1) % mineManager.BlocksGridPositions.Count;
    }
}

public enum AutoMiningControlsState
{
    ReachingInitialPosition,
    MiningBlock,
    SearchNextTarget,
}
