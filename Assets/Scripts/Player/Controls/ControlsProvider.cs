using System;
using UnityEngine;

public class ControlsProvider : MonoBehaviour
{
    public event Action<bool> AutoMiningToggled;
    public IPlayerControls Controls { get; private set; }
    public bool AutoMiningEnabled { get; private set; }

    public InputControls manualControls;
    public AutoMiningControls autoMiningControls;
    public PlayerBlockInventory blockInventory;

    private void Awake()
    {
        Controls = manualControls;
        blockInventory.HasNoMoreSpace += () => ToggleAutoMining(false);
    }

    private void Update()
    {
        DisableAutoMiningOnManualInput();
    }

    public void ToggleAutoMining(bool enable)
    {
        if (enable)
        {
            Controls = autoMiningControls;
            AutoMiningEnabled = true;
            AutoMiningToggled?.Invoke(true);
        }
        else
        {
            Controls = manualControls;
            AutoMiningEnabled = false;
            AutoMiningToggled?.Invoke(false);
        }
    }

    private void DisableAutoMiningOnManualInput()
    {
        if (!AutoMiningEnabled)
            return;

        if (manualControls.JumpedThisFrame() || manualControls.GetMoveDirection().magnitude > 0.1f || manualControls.MineIsPressed())
            ToggleAutoMining(false);
    }
}
