using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class PetRevealPopup : PopUpWindow
{
    [Header("Scroll")]
    public ScrollRect scrollRect;
    public Transform petItemsRoot;
    public PetRevealItemView petItemPrefab;

    [Header("Buttons")]
    public Button rollButton;
    public Button equipButton;
    public Button closeResultButton;

    [Header("Animation")]
    [SerializeField] private float scrollDuration = 3f;
    [SerializeField] private int fillerCount = 10;
    [SerializeField] private int trailingFillerCount = 6;
    [SerializeField] private float bounceOvershoot = 2f;

    private PetEquipService _petEquipService;
    private EggHatchingService _hatchingService;
    private List<PetConfig> _petConfigs;

    private int _slotIndex;
    private string _rolledPetId;
    private bool _isRolling;
    private Tweener _scrollTween;
    private PetRevealItemView _winnerView;
    private float _winnerNormalizedPos;

    private readonly List<PetRevealItemView> _petViews = new();

    [Inject]
    private void Initialize(
        PetEquipService petEquipService,
        EggHatchingService hatchingService,
        ConfigManager<PetConfig> petConfigs)
    {
        _petEquipService = petEquipService;
        _hatchingService = hatchingService;
        _petConfigs = petConfigs.Configs;
    }

    public override void Awake()
    {
        base.Awake();
        rollButton.onClick.AddListener(OnRollClicked);
        equipButton.onClick.AddListener(OnEquipClicked);
        closeResultButton.onClick.AddListener(() => HideWindowAnimated());
    }

    public void Setup(EggConfig eggConfig, int slotIndex)
    {
        _slotIndex = slotIndex;
        _rolledPetId = null;
        _isRolling = false;

        BuildPreview();
        ResetUI();
    }

    private void BuildPreview()
    {
        ClearPetViews();
        for (int i = 0; i <= fillerCount + trailingFillerCount; i++)
            SpawnView(RandomPetConfig());
    }

    // Leading fillers → winner → trailing fillers so scroll looks endless in both directions.
    private void BuildRevealList(string winnerId)
    {
        ClearPetViews();

        for (int i = 0; i < fillerCount; i++)
            SpawnView(RandomPetConfig());

        var winnerCfg = _petConfigs.FirstOrDefault(p => p.id == winnerId) ?? _petConfigs[0];
        _winnerView = SpawnView(winnerCfg);

        for (int i = 0; i < trailingFillerCount; i++)
            SpawnView(RandomPetConfig());

        // Must be at position 0 before measuring so world positions are stable.
        scrollRect.horizontalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
        _winnerNormalizedPos = ComputeWinnerNormalizedPos();
    }

    // Returns the normalizedPosition that centers the winner in the viewport.
    // Measured in world space so canvas scale / pivot setup doesn't matter.
    // All items share the same prefab width, so winner's center from content left is
    // purely a function of index + layout settings — no world-space measurement needed.
    private float ComputeWinnerNormalizedPos()
    {
        if (_winnerView == null) return 0.5f;

        Canvas.ForceUpdateCanvases();

        var layout    = petItemsRoot.GetComponent<HorizontalLayoutGroup>();
        var contentRT = (RectTransform)petItemsRoot;
        var winnerRT  = (RectTransform)_winnerView.transform;

        float itemWidth    = winnerRT.rect.width;
        float spacing      = layout != null ? layout.spacing : 0f;
        float paddingLeft  = layout != null ? layout.padding.left : 0f;
        float viewportWidth = scrollRect.viewport.rect.width;
        float contentWidth  = contentRT.rect.width;
        float scrollable    = contentWidth - viewportWidth;
        if (scrollable <= 0f) return 0.5f;

        float winnerCenter = paddingLeft + fillerCount * (itemWidth + spacing) + itemWidth * 0.5f;
        return Mathf.Clamp01((winnerCenter - viewportWidth * 0.5f) / scrollable);
    }

    private PetRevealItemView SpawnView(PetConfig cfg)
    {
        var view = Instantiate(petItemPrefab, petItemsRoot);
        view.Bind(cfg);
        _petViews.Add(view);
        return view;
    }

    private PetConfig RandomPetConfig() => _petConfigs[Random.Range(0, _petConfigs.Count)];

    private void ClearPetViews()
    {
        // SetParent(null) detaches from the layout group immediately (Destroy is deferred
        // to end-of-frame, so without this the layout would still include old items when
        // we measure right after spawning new ones).
        for (int i = petItemsRoot.childCount - 1; i >= 0; i--)
        {
            var child = petItemsRoot.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
        _petViews.Clear();
        _winnerView = null;
    }

    private void ResetUI()
    {
        rollButton.gameObject.SetActive(true);
        rollButton.interactable = true;
        equipButton.gameObject.SetActive(false);
        closeResultButton.gameObject.SetActive(false);

        foreach (var v in _petViews)
            v.SetHighlight(false);

        scrollRect.horizontalNormalizedPosition = 0f;
    }

    private void OnRollClicked()
    {
        if (_isRolling) return;

        _hatchingService.PetHatched += CaptureRoll;
        _hatchingService.TryCollect(_slotIndex);
        _hatchingService.PetHatched -= CaptureRoll;

        if (_rolledPetId == null) return;

        _isRolling = true;
        rollButton.interactable = false;

        BuildRevealList(_rolledPetId);
        // normalizedPos already at 0 from BuildRevealList; tween to winner center.
        _scrollTween?.Kill();
        _scrollTween = DOVirtual.Float(0f, _winnerNormalizedPos, scrollDuration,
                v => scrollRect.horizontalNormalizedPosition = v)
            .SetEase(Ease.OutBack, bounceOvershoot)
            .OnComplete(OnRevealComplete);
    }

    private void CaptureRoll(string petId) => _rolledPetId = petId;

    private void OnRevealComplete()
    {
        _winnerView?.SetHighlight(true);
        rollButton.gameObject.SetActive(false);
        equipButton.gameObject.SetActive(true);
        closeResultButton.gameObject.SetActive(true);
        _isRolling = false;
    }

    private void OnEquipClicked()
    {
        if (!string.IsNullOrEmpty(_rolledPetId))
            _petEquipService?.TryEquip(_rolledPetId);
        HideWindowAnimated();
    }

    private void OnDestroy()
    {
        _scrollTween?.Kill();
    }
}
