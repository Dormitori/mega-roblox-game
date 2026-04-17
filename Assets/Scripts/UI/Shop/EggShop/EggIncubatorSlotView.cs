using System;
using System.Collections.Generic;
using System.Linq;
using Core.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EggIncubatorSlotView : MonoBehaviour
{
    public Image eggIcon;
    public TextMeshProUGUI timerText;
    public Button claimButton;
    public Button speedUpCrystalsButton;
    public Button speedUpAdButton;

    [Header("Optional 3D preview")]
    public Transform previewRoot;

    private EggIncubatorService _service;
    private Inventory _inventory;
    private List<EggConfig> _eggConfigs;
    private IAudioService _audio;
    private int _slotIndex;
    private GameObject _previewInstance;

    public void Initialize(
        EggIncubatorService service,
        int slotIndex,
        Inventory inventory,
        List<EggConfig> eggConfigs,
        IAudioService audio)
    {
        _service = service;
        _slotIndex = slotIndex;
        _inventory = inventory;
        _eggConfigs = eggConfigs;
        _audio = audio;

        if (claimButton != null) claimButton.onClick.AddListener(OnClaim);
        if (speedUpCrystalsButton != null) speedUpCrystalsButton.onClick.AddListener(OnSpeedUpCrystals);
        if (speedUpAdButton != null) speedUpAdButton.onClick.AddListener(OnSpeedUpAd);
    }

    private void Update()
    {
        // лёгкое обновление таймера в реальном времени, чтобы не ждать событий
        if (_service == null) return;
        var s = _service.GetSlot(_slotIndex);
        if (s == null || string.IsNullOrEmpty(s.eggId)) return;
        UpdateTimerOnly();
    }

    public void Refresh()
    {
        if (_service == null) return;

        var slot = _service.GetSlot(_slotIndex);
        if (slot == null || string.IsNullOrEmpty(slot.eggId))
        {
            SetEmpty();
            return;
        }

        var egg = ResolveEgg(slot.eggId);
        if (eggIcon != null) eggIcon.sprite = egg != null ? egg.icon : null;

        UpdateTimerOnly();

        var canClaim = _service.CanClaim(_slotIndex, DateTime.UtcNow);
        if (claimButton != null) claimButton.interactable = canClaim;

        if (speedUpCrystalsButton != null)
            speedUpCrystalsButton.interactable = _inventory != null &&
                                                _inventory.GetCurrencyCount(CurrencyType.Crystals) > 0;

        if (speedUpAdButton != null)
            speedUpAdButton.interactable = true;

        SetupPreview(egg);
    }

    private void UpdateTimerOnly()
    {
        if (timerText == null || _service == null) return;
        var remain = _service.GetRemaining(_slotIndex, DateTime.UtcNow);
        timerText.text = FormatTime(remain);
    }

    private void OnClaim()
    {
        if (_service == null) return;
        if (_service.TryClaim(_slotIndex))
        {
            _audio?.PlaySfx(SoundId.BuySell);
            Refresh();
        }
    }

    private void OnSpeedUpCrystals()
    {
        if (_service == null) return;
        if (_service.TrySpeedUpWithCrystals(_slotIndex))
        {
            _audio?.PlaySfx(SoundId.BuySell);
            Refresh();
        }
    }

    private void OnSpeedUpAd()
    {
        _service?.TrySpeedUpWithAd(_slotIndex);
    }

    private void SetEmpty()
    {
        if (eggIcon != null) eggIcon.sprite = null;
        if (timerText != null) timerText.text = string.Empty;
        if (claimButton != null) claimButton.interactable = false;
        if (speedUpCrystalsButton != null) speedUpCrystalsButton.interactable = false;
        if (speedUpAdButton != null) speedUpAdButton.interactable = false;
        ClearPreview();
    }

    private EggConfig ResolveEgg(string eggId)
    {
        if (_eggConfigs == null || string.IsNullOrEmpty(eggId)) return null;
        return _eggConfigs.FirstOrDefault(e => e != null && e.id == eggId);
    }

    private static string FormatTime(TimeSpan t)
    {
        if (t <= TimeSpan.Zero) return "00:00";
        if (t.TotalHours >= 1)
            return $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
        return $"{t.Minutes:00}:{t.Seconds:00}";
    }

    private void SetupPreview(EggConfig egg)
    {
        if (previewRoot == null)
            return;

        var prefab = egg != null ? egg.eggWorldPrefab : null;
        if (prefab == null)
        {
            ClearPreview();
            return;
        }

        if (_previewInstance != null && _previewInstance.name.StartsWith(prefab.name))
            return;

        ClearPreview();
        _previewInstance = Instantiate(prefab, previewRoot);
        _previewInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _previewInstance.transform.localScale = Vector3.one;
    }

    private void ClearPreview()
    {
        if (_previewInstance != null)
            Destroy(_previewInstance);
        _previewInstance = null;
    }
}

