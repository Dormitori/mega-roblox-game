using System.Collections.Generic;
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

    [Header("Batching")]
    [SerializeField] private float batchFlushInterval = 1f;

    private ObjectPool<MineLootToastItemView> _pool;

    private readonly List<MineLootToastItemView> _activeItems = new();
    private readonly Dictionary<MineLootToastItemView, Sequence> _sequences = new();

    private readonly Dictionary<BlockConfig, int> _pendingBlocks = new();
    private int _pendingCrystals;
    private float _flushTimer;

    private string _crystalsLabel;
    private string CrystalsLabel => _crystalsLabel ??= I2.Loc.LocalizationManager.GetTranslation("Crystals");

    private void Awake()
    {
        if (itemPrefab != null && stackRoot != null)
            _pool = new ObjectPool<MineLootToastItemView>(itemPrefab, stackRoot, prewarm: maxVisibleItems + 2);
    }

    private void Update()
    {
        _flushTimer += Time.unscaledDeltaTime;
        if (_flushTimer >= batchFlushInterval)
        {
            _flushTimer = 0f;
            FlushPending();
        }
    }

    public void ShowBlock(BlockConfig cfg, int amount = 1)
    {
        if (amount <= 0 || cfg == null) return;
        if (_pendingBlocks.ContainsKey(cfg))
            _pendingBlocks[cfg] += amount;
        else
            _pendingBlocks[cfg] = amount;
    }

    public void AddCrystals(int amount)
    {
        if (amount > 0) _pendingCrystals += amount;
    }

    public void ShowText(string text, Sprite icon = null)
    {
        SpawnItem(text, icon);
    }

    private void FlushPending()
    {
        foreach (var kvp in _pendingBlocks)
        {
            var cfg = kvp.Key;
            var amount = kvp.Value;
            var label = cfg.LocalizedName;
            SpawnItem(string.IsNullOrEmpty(label) ? $"+{amount}" : $"+{amount} {label}", cfg.icon);
        }
        _pendingBlocks.Clear();

        if (_pendingCrystals > 0)
        {
            SpawnItem($"+{_pendingCrystals} {CrystalsLabel}", null);
            _pendingCrystals = 0;
        }
    }

    private void SpawnItem(string text, Sprite icon)
    {
        if (_pool == null) return;

        TrimOverflow();

        var item = _pool.Rent();
        if (item == null) return;

        item.transform.SetAsLastSibling();
        item.Set(text, icon);

        var rt = item.rectTransform != null ? item.rectTransform : (RectTransform)item.transform;
        var cg = item.canvasGroup;
        if (cg != null) cg.alpha = 0f;
        rt.anchoredPosition = new Vector2(enterXOffset, rt.anchoredPosition.y);

        _activeItems.Add(item);

        var seq = DOTween.Sequence();
        seq.SetUpdate(true);
        seq.Append(rt.DOAnchorPosX(0f, enterDuration).SetEase(Ease.OutCubic));
        if (cg != null) seq.Join(cg.DOFade(1f, enterDuration));
        seq.AppendInterval(holdDuration);
        seq.Append(rt.DOAnchorPosY(rt.anchoredPosition.y + exitYOffset, exitDuration).SetEase(Ease.InCubic));
        if (cg != null) seq.Join(cg.DOFade(0f, exitDuration));
        seq.OnComplete(() => ReturnItem(item));

        _sequences[item] = seq;
    }

    private void ReturnItem(MineLootToastItemView item)
    {
        if (item == null) return;
        if (_sequences.TryGetValue(item, out var seq))
        {
            seq.Kill();
            _sequences.Remove(item);
        }
        _activeItems.Remove(item);
        _pool.Return(item);
    }

    private void TrimOverflow()
    {
        while (_activeItems.Count >= maxVisibleItems)
            ReturnItem(_activeItems[0]);
    }
}

