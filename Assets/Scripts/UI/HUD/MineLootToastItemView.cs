using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MineLootToastItemView : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;
    public Image background;
    public Image icon;
    public TextMeshProUGUI label;

    private void Reset()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Set(string text, Sprite sprite)
    {
        if (label != null)
            label.text = text;

        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }
    }
}

