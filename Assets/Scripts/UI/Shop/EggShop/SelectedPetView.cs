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
    
    public Image petIconImage;

    public Button takeButton;
    public GameObject equippedBadge;

    private void Awake()
    {
        takeButton.onClick.AddListener(() => Take?.Invoke());
    }

    public void UpdateView(PetConfig cfg, int ownedCount, bool equipped)
    {
        if (cfg == null)
        {
            petNameText.text = string.Empty;
            rarityText.text = string.Empty;
            bonusText.text = string.Empty;
            ownedCountText.text = string.Empty;
            petIconImage.sprite = null;
            takeButton.interactable = false;
            equippedBadge.SetActive(false);
            return;
        }

        petIconImage.sprite = cfg.icon;
        petNameText.text = cfg.LocalizedName;
        rarityText.text = cfg.rarity.ToString();
        bonusText.text = MakeBonusLine(cfg);
        ownedCountText.text = ownedCount.ToString();

        equippedBadge.SetActive(equipped);

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

