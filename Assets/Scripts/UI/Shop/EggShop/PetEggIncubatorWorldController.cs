using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

/// <summary>
/// 3D-ячейки инкубатора у домика пет-магазина: установка лучшего яйца из инвентаря, таймер, ускорения, готовность, открытие UI вылупления.
/// Ссылки на промпты/кнопки/TMP/партиклы задаются в инспекторе на каждый слот.
/// </summary>
public class PetEggIncubatorWorldController : MonoBehaviour
{
    [Serializable]
    public class NestSlot
    {
        [Range(0, 2)] public int slotIndex;
        public Transform eggAnchor;
        [Tooltip("Расстояние до игрока, на котором показываются подсказки и кнопки")]
        public float interactRadius = 2.8f;

        [Header("Prompts (можно world-space Canvas)")]
        public GameObject placePromptRoot;
        public GameObject incubatingRoot;
        public GameObject readyRoot;

        public TextMeshProUGUI timerText;
        public TextMeshProUGUI readyText;

        [Header("Actions")]
        public Button placeEggButton;
        public Button adSpeedButton;
        public Button crystalSpeedButton;
        public Button openHatchUiButton;

        [Header("Ускорения (скрываются в фазе «готово»)")]
        public GameObject speedUpsRoot;

        [Header("Эффект «готово» (опционально)")]
        public ParticleSystem readyLoopParticles;
        public GameObject readyExtraEffect;
    }

    public NestSlot[] nests = Array.Empty<NestSlot>();
    public PetHatchRevealPopup hatchRevealPopup;

    [Tooltip("Если пусто — подставится из DI (CharacterMovement)")]
    public CharacterMovement player;

    private EggIncubatorService _incubator;
    private ConfigManager<EggConfig> _eggConfigs;

    private readonly GameObject[] _spawnedEgg = new GameObject[EggIncubatorService.SlotCount];
    private readonly string[] _lastEggId = new string[EggIncubatorService.SlotCount];
    private readonly bool[] _readyFxPlayed = new bool[EggIncubatorService.SlotCount];

    [Inject]
    public void Construct(
        EggIncubatorService incubator,
        ConfigManager<EggConfig> eggConfigs,
        CharacterMovement characterMovement)
    {
        _incubator = incubator;
        _eggConfigs = eggConfigs;
        if (player == null)
            player = characterMovement;
    }

    private void OnEnable()
    {
        if (_incubator != null)
            _incubator.Changed += OnIncubatorChanged;
    }

    private void OnDisable()
    {
        if (_incubator != null)
            _incubator.Changed -= OnIncubatorChanged;
    }

    private void Start()
    {
        foreach (var nest in nests)
        {
            var n = nest;
            if (n.placeEggButton != null)
                n.placeEggButton.onClick.AddListener(() => _incubator?.TryPlaceBestEggInSlot(n.slotIndex));
            if (n.adSpeedButton != null)
                n.adSpeedButton.onClick.AddListener(() => _incubator?.TrySpeedUpWithAd(n.slotIndex));
            if (n.crystalSpeedButton != null)
                n.crystalSpeedButton.onClick.AddListener(() => _incubator?.TrySpeedUpWithCrystals(n.slotIndex));
            if (n.openHatchUiButton != null)
                n.openHatchUiButton.onClick.AddListener(() => OpenHatchUi(n));
        }

        RefreshAll();
    }

    private void OnIncubatorChanged() => RefreshAll();

    private void Update()
    {
        if (player == null || _incubator == null) return;
        foreach (var nest in nests)
        {
            UpdateNestProximity(nest);
            RefreshTimerUi(nest);
        }
    }

    private void OpenHatchUi(NestSlot nest)
    {
        if (hatchRevealPopup == null || _incubator == null) return;
        if (!_incubator.IsReadyForHatchUi(nest.slotIndex, DateTime.UtcNow)) return;
        hatchRevealPopup.OpenForSlot(nest.slotIndex);
    }

    private void UpdateNestProximity(NestSlot nest)
    {
        var anchor = nest.eggAnchor != null ? nest.eggAnchor : transform;
        var close = Vector3.Distance(player.transform.position, anchor.position) <= nest.interactRadius;

        var showPlace = close && _incubator.IsSlotEmpty(nest.slotIndex) && _incubator.HasAnyEggsInInventory();
        var showIncubate = close && !_incubator.IsSlotEmpty(nest.slotIndex) &&
                           !_incubator.IsReadyForHatchUi(nest.slotIndex, DateTime.UtcNow);
        var showReady = close && _incubator.IsReadyForHatchUi(nest.slotIndex, DateTime.UtcNow);

        SetActiveIfPresent(nest.placePromptRoot, showPlace);
        SetActiveIfPresent(nest.incubatingRoot, showIncubate);
        SetActiveIfPresent(nest.readyRoot, showReady);
    }

    private void RefreshAll()
    {
        if (_incubator == null) return;
        foreach (var nest in nests)
        {
            if (nest.slotIndex < 0 || nest.slotIndex >= EggIncubatorService.SlotCount)
                continue;
            RefreshEggModel(nest);
            RefreshTimerUi(nest);
            RefreshReadyState(nest);
        }
    }

    private void RefreshEggModel(NestSlot nest)
    {
        var idx = nest.slotIndex;
        var slot = _incubator.GetSlot(idx);
        var id = slot?.eggId ?? string.Empty;

        var last = _lastEggId[idx] ?? string.Empty;
        if (last != id)
        {
            if (_spawnedEgg[idx] != null)
            {
                Destroy(_spawnedEgg[idx]);
                _spawnedEgg[idx] = null;
            }

            _lastEggId[idx] = id;
        }

        if (string.IsNullOrEmpty(id) || nest.eggAnchor == null)
            return;

        if (_spawnedEgg[idx] != null)
            return;

        var cfg = _eggConfigs.Configs.FirstOrDefault(e => e != null && e.id == id);
        if (cfg?.eggWorldPrefab == null)
            return;

        _spawnedEgg[idx] = Instantiate(cfg.eggWorldPrefab, nest.eggAnchor);
        _spawnedEgg[idx].transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _spawnedEgg[idx].transform.localScale = Vector3.one;
    }

    private void RefreshTimerUi(NestSlot nest)
    {
        if (nest.timerText == null) return;
        if (_incubator.IsSlotEmpty(nest.slotIndex))
        {
            nest.timerText.text = string.Empty;
            return;
        }

        if (_incubator.IsReadyForHatchUi(nest.slotIndex, DateTime.UtcNow))
        {
            nest.timerText.text = string.Empty;
            return;
        }

        nest.timerText.text = FormatTime(_incubator.GetRemaining(nest.slotIndex, DateTime.UtcNow));
    }

    private void RefreshReadyState(NestSlot nest)
    {
        var idx = nest.slotIndex;
        var ready = _incubator.IsReadyForHatchUi(idx, DateTime.UtcNow);

        if (ready && !_readyFxPlayed[idx])
        {
            _readyFxPlayed[idx] = true;
            if (nest.readyLoopParticles != null) nest.readyLoopParticles.Play();
            if (nest.readyExtraEffect != null) nest.readyExtraEffect.SetActive(true);
            if (nest.readyText != null) nest.readyText.text = "Готово";
        }

        if (!ready)
        {
            _readyFxPlayed[idx] = false;
            if (nest.readyExtraEffect != null) nest.readyExtraEffect.SetActive(false);
            if (nest.readyLoopParticles != null)
                nest.readyLoopParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (nest.speedUpsRoot != null)
        {
            // Показываем ускорения только в фазе инкубации (слот занят, ещё не готово к выдаче)
            var incubating = !_incubator.IsSlotEmpty(idx) && !ready;
            nest.speedUpsRoot.SetActive(incubating);
        }
    }

    private static string FormatTime(TimeSpan t)
    {
        if (t <= TimeSpan.Zero) return "00:00";
        if (t.TotalHours >= 1)
            return $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
        return $"{t.Minutes:00}:{t.Seconds:00}";
    }

    private static void SetActiveIfPresent(GameObject go, bool v)
    {
        if (go != null && go.activeSelf != v) go.SetActive(v);
    }
}
