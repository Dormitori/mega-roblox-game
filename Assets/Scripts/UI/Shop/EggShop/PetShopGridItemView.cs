using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PetShopGridItemView : MonoBehaviour, IPointerClickHandler
{
    public event Action Clicked;

    public Image iconImage;
    public Image darkImage;
    public Image equippedIcon;
    public Image selectedOutline;
    public TextMeshProUGUI countText;

    [SerializeField] private float notOwnedAlpha = 0.6f;

    public void SetIcon(Sprite icon)
    {
        if (iconImage != null)
            iconImage.sprite = icon;
    }

    public void UpdateState(int ownedCount, bool equipped, bool selected)
    {
        if (darkImage != null)
        {
            var c = darkImage.color;
            c.a = ownedCount > 0 ? 0f : notOwnedAlpha;
            darkImage.color = c;
        }

        if (equippedIcon != null)
        {
            var c = equippedIcon.color;
            c.a = equipped ? 1f : 0f;
            equippedIcon.color = c;
        }

        if (selectedOutline != null)
        {
            var c = selectedOutline.color;
            c.a = selected ? 1f : 0f;
            selectedOutline.color = c;
        }

        if (countText != null)
        {
            countText.gameObject.SetActive(ownedCount > 0);
            countText.text = ownedCount.ToString();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke();
    }
}

