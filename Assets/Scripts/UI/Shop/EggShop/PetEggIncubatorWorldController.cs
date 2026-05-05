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

#if UNITY_EDITOR
    [ContextMenu("Auto setup nests UI (create + wire)")]
    private void Editor_AutoSetupNestsUi()
    {
        const int slotCount = EggIncubatorService.SlotCount;

        UnityEditor.Undo.RecordObject(this, "Auto setup incubator nests UI");

        if (nests == null || nests.Length != slotCount)
        {
            var prev = nests ?? Array.Empty<NestSlot>();
            nests = new NestSlot[slotCount];
            for (var i = 0; i < slotCount; i++)
                nests[i] = i < prev.Length && prev[i] != null ? prev[i] : new NestSlot();
        }

        if (hatchRevealPopup == null)
            hatchRevealPopup = FindFirstObjectByType<PetHatchRevealPopup>();

        // World-space Canvas for prompts/buttons (so UI is actually visible)
        var canvasT = transform.Find("IncubatorWorldCanvas");
        GameObject canvasGo;
        if (canvasT != null)
            canvasGo = canvasT.gameObject;
        else
        {
            canvasGo = new GameObject("IncubatorWorldCanvas", typeof(RectTransform));
            UnityEditor.Undo.RegisterCreatedObjectUndo(canvasGo, "Create incubator world canvas");
            canvasGo.transform.SetParent(transform, false);
        }

        var canvas = canvasGo.GetComponent<Canvas>();
        if (canvas == null) canvas = UnityEditor.Undo.AddComponent<Canvas>(canvasGo);
        canvas.renderMode = RenderMode.WorldSpace;

        var raycaster = canvasGo.GetComponent<GraphicRaycaster>();
        if (raycaster == null) raycaster = UnityEditor.Undo.AddComponent<GraphicRaycaster>(canvasGo);

        var canvasRt = canvasGo.GetComponent<RectTransform>();
        canvasRt.sizeDelta = new Vector2(600f, 600f);
        if (canvasRt.localScale.sqrMagnitude < 1e-6f)
            canvasRt.localScale = Vector3.one * 0.01f;

        for (var i = 0; i < slotCount; i++)
        {
            var n = nests[i] ??= new NestSlot();
            if (n.slotIndex < 0 || n.slotIndex >= slotCount) n.slotIndex = i;
            if (n.interactRadius <= 0.01f) n.interactRadius = 2.8f;

            var root = FindOrCreateChild(canvasGo.transform, $"Nest{i}_UI");
            var rootRt = root.GetComponent<RectTransform>();
            if (rootRt == null) rootRt = UnityEditor.Undo.AddComponent<RectTransform>(root.gameObject);
            rootRt.sizeDelta = new Vector2(240f, 120f);

            // If eggAnchor is set, place UI near it for convenience
            if (root != null && n.eggAnchor != null)
            {
                try
                {
                    root.position = n.eggAnchor.position + Vector3.up * 0.35f;
                    root.rotation = Quaternion.identity;
                }
                catch (MissingReferenceException)
                {
                    // В nests мог остаться "битый" Transform (удалённый объект в сцене).
                    // Не мешаем автосетапу — просто сбрасываем ссылку.
                    n.eggAnchor = null;
                }
            }
            var place = FindOrCreateChild(root, "PlacePromptRoot");
            var incubating = FindOrCreateChild(root, "IncubatingRoot");
            var ready = FindOrCreateChild(root, "ReadyRoot");

            n.placePromptRoot = place.gameObject;
            n.incubatingRoot = incubating.gameObject;
            n.readyRoot = ready.gameObject;

            // Place prompt
            var placeBtn = FindOrCreateUIButton(place, "PlaceEggButton", "PLACE");
            n.placeEggButton = placeBtn;

            // Incubating
            var timer = FindOrCreateTmp(incubating, "TimerText", "00:00");
            n.timerText = timer;

            var speedRoot = FindOrCreateChild(incubating, "SpeedUpsRoot");
            n.speedUpsRoot = speedRoot.gameObject;

            var adBtn = FindOrCreateUIButton(speedRoot, "AdSpeedButton", "AD");
            var crBtn = FindOrCreateUIButton(speedRoot, "CrystalSpeedButton", "CRYSTAL");
            n.adSpeedButton = adBtn;
            n.crystalSpeedButton = crBtn;

            // Ready
            var readyText = FindOrCreateTmp(ready, "ReadyText", "Готово");
            n.readyText = readyText;
            var openBtn = FindOrCreateUIButton(ready, "OpenHatchUiButton", "OPEN");
            n.openHatchUiButton = openBtn;

            // Default visibility (не обязательно, но удобно)
            place.gameObject.SetActive(false);
            incubating.gameObject.SetActive(false);
            ready.gameObject.SetActive(false);
        }

        UnityEditor.EditorUtility.SetDirty(this);
    }

    private static Transform FindOrCreateChild(Transform parent, string name)
    {
        // parent может быть "Missing" (уничтоженный объект в сцене, но ссылка осталась).
        // В этом случае просто создаём объект без parent, чтобы не падать.
        if (parent == null)
            return new GameObject(name).transform;

        var t = parent.Find(name);
        if (t != null) return t;

        var go = new GameObject(name);
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create incubator UI object");
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static Button FindOrCreateUIButton(Transform parent, string name, string label)
    {
        if (parent == null)
        {
            var orphan = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var tmpOrphan = orphan.AddComponent<TextMeshProUGUI>();
            tmpOrphan.text = label;
            tmpOrphan.alignment = TextAlignmentOptions.Center;
            return orphan.GetComponent<Button>();
        }

        var t = parent.Find(name);
        GameObject go;
        if (t != null)
            go = t.gameObject;
        else
        {
            go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create incubator UI button");
            go.transform.SetParent(parent, false);
        }

        var img = go.GetComponent<Image>();
        if (img != null && img.color.a < 0.1f) img.color = new Color(1f, 1f, 1f, 0.15f);

        // Label (TMP)
        var labelT = go.transform.Find("Label");
        if (labelT == null)
        {
            var labelGo = new GameObject("Label", typeof(RectTransform));
            UnityEditor.Undo.RegisterCreatedObjectUndo(labelGo, "Create incubator UI label");
            labelGo.transform.SetParent(go.transform, false);
            labelT = labelGo.transform;
        }
        var tmp = labelT.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = UnityEditor.Undo.AddComponent<TextMeshProUGUI>(labelT.gameObject);
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;

        return go.GetComponent<Button>();
    }

    private static TextMeshProUGUI FindOrCreateTmp(Transform parent, string name, string text)
    {
        if (parent == null)
        {
            var orphan = new GameObject(name, typeof(RectTransform));
            var tmpOrphan = orphan.AddComponent<TextMeshProUGUI>();
            tmpOrphan.text = text;
            tmpOrphan.alignment = TextAlignmentOptions.Center;
            return tmpOrphan;
        }

        var t = parent.Find(name);
        GameObject go;
        if (t != null)
            go = t.gameObject;
        else
        {
            go = new GameObject(name, typeof(RectTransform));
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create incubator TMP text");
            go.transform.SetParent(parent, false);
        }

        var tmp = go.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = UnityEditor.Undo.AddComponent<TextMeshProUGUI>(go);
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }
#endif

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
