using UnityEngine;
using VContainer;

public class EggPedestalManager : MonoBehaviour
{
    [SerializeField] private Transform[] pedestals = new Transform[EggHatchingService.SlotCount];
    [SerializeField] private EggTimerLabel[] timerLabels = new EggTimerLabel[EggHatchingService.SlotCount];

    private EggHatchingService _hatchingService;
    private readonly GameObject[] _spawnedEggs = new GameObject[EggHatchingService.SlotCount];
    private readonly EggPedestalAnimator[] _animators = new EggPedestalAnimator[EggHatchingService.SlotCount];

    [Inject]
    private void Initialize(EggHatchingService hatchingService)
    {
        _hatchingService = hatchingService;
        _hatchingService.SlotsChanged += RefreshAll;

        for (int i = 0; i < EggHatchingService.SlotCount; i++)
            timerLabels[i]?.Setup(hatchingService, i);

        RefreshAll();
    }

    private void OnDestroy()
    {
        if (_hatchingService != null)
            _hatchingService.SlotsChanged -= RefreshAll;
    }

    private void RefreshAll()
    {
        for (int i = 0; i < EggHatchingService.SlotCount; i++)
            RefreshSlot(i);
    }

    private void RefreshSlot(int i)
    {
        if (_hatchingService.IsSlotEmpty(i))
        {
            ClearSlot(i);
            return;
        }

        if (_spawnedEggs[i] == null)
        {
            var prefab = _hatchingService.GetSlotConfig(i)?.eggWorldPrefab;
            if (prefab == null || i >= pedestals.Length || pedestals[i] == null) return;

            _spawnedEggs[i] = Instantiate(prefab, pedestals[i].position, pedestals[i].rotation, pedestals[i]);
            _animators[i] = _spawnedEggs[i].AddComponent<EggPedestalAnimator>();
        }

        _animators[i]?.SetHatching(_hatchingService.IsHatched(i));
    }

    private void ClearSlot(int i)
    {
        if (_spawnedEggs[i] == null) return;
        Destroy(_spawnedEggs[i]);
        _spawnedEggs[i] = null;
        _animators[i] = null;
    }
}
