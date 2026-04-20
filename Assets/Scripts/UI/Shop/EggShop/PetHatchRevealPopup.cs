using System.Collections;
using System.Linq;
using Core.Audio;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// UI после готового яйца у 3D-ячейки: рулетка спрайтов, финальный пет, Взять / Ок.
/// </summary>
public class PetHatchRevealPopup : PopUpWindow
{
    [Header("Reveal")]
    public Image cycleImage;
    public Image resultImage;
    public ParticleSystem resultParticle;
    [Tooltip("Доп. объект (свечение и т.п.) — включается на финале")]
    public GameObject optionalEffectRoot;

    [Header("Timing")]
    public float spinDuration = 2.2f;
    public float spinStep = 0.07f;

    public Button takeButton;
    public Button okButton;

    private EggIncubatorService _incubator;
    private ConfigManager<PetConfig> _petConfigs;
    private IAudioService _audio;

    private int _slotIndex = -1;
    private bool _committed;
    private Coroutine _routine;

    [Inject]
    private void Construct(
        EggIncubatorService incubator,
        ConfigManager<PetConfig> petConfigs,
        IAudioService audio)
    {
        _incubator = incubator;
        _petConfigs = petConfigs;
        _audio = audio;
    }

    public override void Awake()
    {
        base.Awake();
        if (takeButton != null) takeButton.onClick.AddListener(OnTake);
        if (okButton != null) okButton.onClick.AddListener(OnOk);
        if (closeButton != null) closeButton.gameObject.SetActive(false);
        SetResultButtonsInteractable(false);
        if (resultImage != null) resultImage.gameObject.SetActive(false);
        if (optionalEffectRoot != null) optionalEffectRoot.SetActive(false);
    }

    public void OpenForSlot(int slotIndex)
    {
        if (_incubator == null) return;
        _slotIndex = slotIndex;
        _committed = false;

        if (!_incubator.IsReadyForHatchUi(slotIndex, System.DateTime.UtcNow))
            return;

        var petId = _incubator.GetRolledPetId(slotIndex);
        if (string.IsNullOrEmpty(petId))
            return;

        SetResultButtonsInteractable(false);
        if (closeButton != null) closeButton.gameObject.SetActive(false);
        if (optionalEffectRoot != null) optionalEffectRoot.SetActive(false);
        if (resultImage != null) resultImage.gameObject.SetActive(false);
        if (cycleImage != null) cycleImage.gameObject.SetActive(true);

        ShowWindowAnimated();
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(RevealRoutine(petId));
    }

    private IEnumerator RevealRoutine(string finalPetId)
    {
        var pool = _petConfigs.Configs.Where(p => p != null).ToList();
        var finalCfg = _petConfigs.Configs.FirstOrDefault(p => p != null && p.id == finalPetId);

        var t = 0f;
        while (t < spinDuration && pool.Count > 0)
        {
            var pick = pool[Random.Range(0, pool.Count)];
            if (cycleImage != null)
            {
                cycleImage.sprite = pick.icon;
                cycleImage.enabled = true;
            }
            t += spinStep;
            yield return new WaitForSeconds(spinStep);
        }

        if (cycleImage != null) cycleImage.gameObject.SetActive(false);

        if (finalCfg != null && resultImage != null)
        {
            resultImage.sprite = finalCfg.icon;
            resultImage.gameObject.SetActive(true);
        }

        if (resultParticle != null) resultParticle.Play();
        if (optionalEffectRoot != null) optionalEffectRoot.SetActive(true);

        _audio?.PlaySfx(SoundId.BuySell);
        SetResultButtonsInteractable(true);
        _routine = null;
    }

    private void SetResultButtonsInteractable(bool v)
    {
        if (takeButton != null) takeButton.interactable = v;
        if (okButton != null) okButton.interactable = v;
    }

    private void OnTake()
    {
        if (_committed || _slotIndex < 0) return;
        _committed = true;
        SetResultButtonsInteractable(false);
        if (_incubator != null && _incubator.TryFinalizeHatch(_slotIndex, true))
            _audio?.PlaySfx(SoundId.BuySell);
        CleanupFx();
        HideWindowAnimated();
    }

    private void OnOk()
    {
        if (_committed || _slotIndex < 0) return;
        _committed = true;
        SetResultButtonsInteractable(false);
        _incubator?.TryFinalizeHatch(_slotIndex, false);
        CleanupFx();
        HideWindowAnimated();
    }

    private void CleanupFx()
    {
        if (optionalEffectRoot != null) optionalEffectRoot.SetActive(false);
    }
}
