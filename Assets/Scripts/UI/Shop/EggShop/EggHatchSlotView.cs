using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EggHatchSlotView : MonoBehaviour
{
    public Image eggIcon;
    public GameObject hatchingGroup;   // timer + watch-ad + hatch-now buttons
    public TextMeshProUGUI timerText;
    public Button watchAdButton;
    public TextMeshProUGUI watchAdLabel;  // assign the TMP child of watchAdButton in Inspector
    public Button hatchNowButton;
    public TextMeshProUGUI hatchNowLabel; // assign the TMP child of hatchNowButton in Inspector
    public Button openButton;          // shown only when egg is ready to open

    public event Action OnWatchAd;
    public event Action OnHatchNow;
    public event Action OnOpen;

    private void Awake()
    {
        if (watchAdButton != null)
            watchAdButton.onClick.AddListener(() => OnWatchAd?.Invoke());
        if (hatchNowButton != null)
            hatchNowButton.onClick.AddListener(() => OnHatchNow?.Invoke());
        if (openButton != null)
            openButton.onClick.AddListener(() => OnOpen?.Invoke());

        SetEmpty();
    }

    // Replaces every occurrence of "x" (case-insensitive, whole-word) in the button label
    // with the actual number of minutes skipped, e.g. "Skip x min" → "Skip 3 min".
    public void SetWatchAdSkipMinutes(int skipSeconds)
    {
        if (watchAdLabel == null) return;
        int minutes = Mathf.Max(1, Mathf.RoundToInt(skipSeconds / 60f));
        watchAdLabel.text = Regex.Replace(watchAdLabel.text, @"\bx\b", minutes.ToString(),
            RegexOptions.IgnoreCase);
    }

    public void SetHatchNowPrice(int crystals)
    {
        if (hatchNowLabel == null) return;
        hatchNowLabel.text = crystals.ToString();
    }

    public void SetEmpty()
    {
        if (eggIcon != null) { eggIcon.sprite = null; eggIcon.enabled = false; }
        if (hatchingGroup != null) hatchingGroup.SetActive(false);
        if (openButton != null) openButton.gameObject.SetActive(false);
    }

    public void SetHatching(Sprite icon, string timeLabel, bool canSkip = true)
    {
        if (eggIcon != null) { eggIcon.sprite = icon; eggIcon.enabled = true; }
        if (timerText != null) timerText.text = timeLabel;
        if (watchAdButton != null) watchAdButton.interactable = true;
        if (hatchNowButton != null) hatchNowButton.interactable = canSkip;
        if (hatchingGroup != null) hatchingGroup.SetActive(true);
        if (openButton != null) openButton.gameObject.SetActive(false);
    }

    // Egg timer finished — hide the hatching controls, show Open button.
    public void SetHatched(Sprite icon)
    {
        if (eggIcon != null) { eggIcon.sprite = icon; eggIcon.enabled = true; }
        if (hatchingGroup != null) hatchingGroup.SetActive(false);
        if (openButton != null) openButton.gameObject.SetActive(true);
    }

    public void UpdateTimer(string timeLabel)
    {
        if (timerText != null) timerText.text = timeLabel;
    }
}
