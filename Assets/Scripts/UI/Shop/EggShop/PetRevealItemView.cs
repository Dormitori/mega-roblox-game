using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetRevealItemView : MonoBehaviour
{
    public Image icon;
    public GameObject highlightOverlay;

    public string PetId { get; private set; }

    public void Bind(PetConfig cfg)
    {
        PetId = cfg.id;
        if (icon != null)
        {
            icon.sprite = cfg.icon;
            icon.enabled = cfg.icon != null;
        }
        if (highlightOverlay != null) highlightOverlay.SetActive(false);
    }

    public void SetHighlight(bool on)
    {
        if (highlightOverlay != null) highlightOverlay.SetActive(on);
    }
}
