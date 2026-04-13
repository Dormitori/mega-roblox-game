using UnityEngine;
using UnityEngine.UI;

public class AutoMiningButton : MonoBehaviour
{
    public Button button;
    public Image activatedGlowImage;
    public ControlsProvider controlsProvider;
    public Transform playerTransform;
    public Transform mineCenter;
    public int buttonActivationRadiusSqr;
    
    public void Awake()
    {
        SetGlowAlpha(0f);
        button.onClick.AddListener(ToggleAutoMining);
        controlsProvider.AutoMiningToggled += OnAutoMiningToggled;
    }

    private void Update()
    {
        var delta = playerTransform.position - mineCenter.position;
        delta.y = 0f;

        var sqrDistance = delta.sqrMagnitude;
        
        button.interactable = sqrDistance < buttonActivationRadiusSqr;
        if (sqrDistance > buttonActivationRadiusSqr)
            controlsProvider.ToggleAutoMining(false);
    }

    private void ToggleAutoMining()
    {
        if (controlsProvider.AutoMiningEnabled)
            controlsProvider.ToggleAutoMining(false);
        else
            controlsProvider.ToggleAutoMining(true);   
    }

    private void OnAutoMiningToggled(bool enable)
    {
        if (enable)
            SetGlowAlpha(1f);
        else
            SetGlowAlpha(0f);
    }

    private void SetGlowAlpha(float alpha)
    {
        var color = activatedGlowImage.color;
        color.a = alpha;
        activatedGlowImage.color = color;
    }
}
