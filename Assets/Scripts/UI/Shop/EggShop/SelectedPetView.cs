using System;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedPetView : MonoBehaviour
{
    public event Action Take;

    public TextMeshProUGUI petNameText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI bonusText;
    public TextMeshProUGUI ownedCountText;

    [Tooltip("Крупная иконка пета (опционально)")]
    public Image petIconImage;

    public Button takeButton;
    public GameObject equippedBadge;

    private void Awake()
    {
        if (takeButton != null)
            takeButton.onClick.AddListener(() => Take?.Invoke());
    }

    public void UpdateView(PetConfig cfg, int ownedCount, bool equipped)
    {
        if (cfg == null)
        {
            if (petNameText != null) petNameText.text = string.Empty;
            if (rarityText != null) rarityText.text = string.Empty;
            if (bonusText != null) bonusText.text = string.Empty;
            if (ownedCountText != null) ownedCountText.text = string.Empty;
            if (petIconImage != null)
            {
                petIconImage.sprite = null;
                petIconImage.enabled = false;
            }
            if (takeButton != null) takeButton.interactable = false;
            if (equippedBadge != null) equippedBadge.SetActive(false);
            return;
        }

        if (petIconImage != null) petIconImage.sprite = cfg.icon;
        if (petNameText != null) petNameText.text = cfg.LocalizedName;
        if (rarityText != null) rarityText.text = cfg.rarity.ToString();
        if (bonusText != null) bonusText.text = MakeBonusLine(cfg);
        if (ownedCountText != null) ownedCountText.text = ownedCount.ToString();

        if (equippedBadge != null) equippedBadge.SetActive(equipped);

        if (takeButton != null)
            takeButton.interactable = ownedCount > 0 && !equipped;
    }

    private static string MakeBonusLine(PetConfig cfg)
    {
        var statTerm = cfg.statType switch
        {
            PetStatType.GoldPricePercent => "PetStat/Gold",
            PetStatType.BlockDamagePercent => "PetStat/Damage",
            PetStatType.AttackSpeedPercent => "PetStat/AttackSpeed",
            PetStatType.RareOreChancePercent => "PetStat/RareOreChance",
            _ => "PetStat/Gold"
        };

        var statLabel = LocalizationManager.GetTranslation(statTerm);
        return $"+{cfg.bonusPercent:0.#}% {statLabel}";
    }
}

