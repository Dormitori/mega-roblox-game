using DG.Tweening;
using UnityEngine;

public class MineLootToastController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform stackRoot;
    [SerializeField] private MineLootToastItemView itemPrefab;

    [Header("Layout")]
    [SerializeField] private int maxVisibleItems = 4;

    [Header("Animation")]
    [SerializeField] private float enterDuration = 0.12f;
    [SerializeField] private float holdDuration = 0.9f;
    [SerializeField] private float exitDuration = 0.18f;
    [SerializeField] private float enterXOffset = 30f;
    [SerializeField] private float exitYOffset = 12f;

    public void ShowBlock(BlockConfig cfg, int amount = 1)
    {
        if (amount <= 0) return;

        var icon = cfg != null ? cfg.icon : null;
        var name = cfg != null ? cfg.LocalizedName : "";
        var text = string.IsNullOrEmpty(name) ? $"+{amount}" : $"+{amount} {name}";
        SpawnItem(text, icon);
    }

    public void ShowText(string text, Sprite icon = null)
    {
        SpawnItem(text, icon);
    }

    private void SpawnItem(string text, Sprite icon)
    {
        if (stackRoot == null || itemPrefab == null)
            return;

        TrimOverflow();

        var item = Instantiate(itemPrefab, stackRoot);
        item.gameObject.SetActive(true);
        item.Set(text, icon);

        var rt = item.rectTransform != null ? item.rectTransform : (RectTransform)item.transform;
        var cg = item.canvasGroup;
        if (cg != null) cg.alpha = 0f;

        rt.anchoredPosition = new Vector2(enterXOffset, rt.anchoredPosition.y);
        var seq = DOTween.Sequence();
        seq.SetUpdate(true);
        seq.Append(rt.DOAnchorPosX(0f, enterDuration).SetEase(Ease.OutCubic));
        if (cg != null) seq.Join(cg.DOFade(1f, enterDuration));
        seq.AppendInterval(holdDuration);
        seq.Append(rt.DOAnchorPosY(rt.anchoredPosition.y + exitYOffset, exitDuration).SetEase(Ease.InCubic));
        if (cg != null) seq.Join(cg.DOFade(0f, exitDuration));
        seq.OnComplete(() => Destroy(item.gameObject));
    }

    private void TrimOverflow()
    {
        if (stackRoot == null) return;
        while (stackRoot.childCount >= maxVisibleItems)
        {
            var oldest = stackRoot.GetChild(0);
            if (oldest != null)
                Destroy(oldest.gameObject);
        }
    }
}

